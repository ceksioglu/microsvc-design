using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data.abstracts;
using WebAPI.DTO;
using WebAPI.Models;
using WebAPI.Aspects;
using WebAPI.Packages.RabbitMQ.abstracts;
using Microsoft.AspNetCore.Identity;

namespace WebAPI.Data.concretes
{
    public class UserCommandRepository : IUserCommandRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IRabbitMQService _rabbitMQService;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserCommandRepository(
            ApplicationDbContext context, 
            IMapper mapper, 
            IRabbitMQService rabbitMQService,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _rabbitMQService = rabbitMQService ?? throw new ArgumentNullException(nameof(rabbitMQService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<UserResponseDto> CreateAsync(UserCreateDto userDto)
        {
            if (userDto == null)
                throw new ArgumentNullException(nameof(userDto));

            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = _passwordHasher.HashPassword(user, userDto.Password);
            user.Role = "Customer";
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await PublishUserEvent("user_created", user);

            return _mapper.Map<UserResponseDto>(user);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<UserResponseDto> UpdateAsync(int id, UserUpdateDto userDto)
        {
            if (userDto == null)
                throw new ArgumentNullException(nameof(userDto));

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with id {id} not found.");

            _mapper.Map(userDto, user);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await PublishUserEvent("user_updated", user);

            return _mapper.Map<UserResponseDto>(user);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await PublishUserEvent("user_deleted", id);

            return true;
        }

        [LoggingAspect]
        [ExceptionAspect]
        private async Task PublishUserEvent(string eventType, object payload)
        {
            await _rabbitMQService.PublishMessage("user_events", eventType, payload);
        }
    }
}