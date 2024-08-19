using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebAPI.Data.abstracts;
using WebAPI.DTO;
using WebAPI.Models;
using WebAPI.Aspects;
using WebAPI.Packages.Redis.abstracts;

namespace WebAPI.Data.concretes
{
    public class UserQueryRepository : IUserQueryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IRedisService _redisService;
        private readonly ILogger<UserQueryRepository> _logger;

        public UserQueryRepository(
            ApplicationDbContext context, 
            IMapper mapper, 
            IRedisService redisService,
            ILogger<UserQueryRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [CachingAspect(300, "user:")]
        public async Task<UserResponseDto> GetByIdAsync(int id)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

                return user != null ? _mapper.Map<UserResponseDto>(user) : null;
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
        public async Task<UserResponseDto> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));

            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

                return user != null ? _mapper.Map<UserResponseDto>(user) : null;
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
        public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
        {
            try
            {
                var users = await _context.Users
                    .AsNoTracking()
                    .Where(u => !u.IsDeleted)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<UserResponseDto>>(users);
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
