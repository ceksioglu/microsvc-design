using AOP.Aspects;
using Core.Redis.abstracts;
using DataAccess.Models;
using DataAccess.Repositories.abstracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories.concretes
{
    public class UserQueryRepository : IUserQueryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IRedisService _redisService;
        private readonly ILogger<UserQueryRepository> _logger;

        public UserQueryRepository(
            ApplicationDbContext context,
            IRedisService redisService,
            ILogger<UserQueryRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "user:")]
        public async Task<User> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving user with id {id}.");
                throw new ApplicationException($"An error occurred while retrieving the user with id {id}.", ex);
            }
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "user:email:")]
        public async Task<User> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));

            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving user with email {email}.");
                throw new ApplicationException($"An error occurred while retrieving the user with email {email}.", ex);
            }
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "users:all")]
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .Where(u => !u.IsDeleted)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all users.");
                throw new ApplicationException("An error occurred while retrieving all users.", ex);
            }
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));

            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.Email == email && !u.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while checking existence of user with email {email}.");
                throw new ApplicationException($"An error occurred while checking the existence of user with email {email}.", ex);
            }
        }
    }
}