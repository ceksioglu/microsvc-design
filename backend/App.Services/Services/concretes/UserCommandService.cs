using AOP.Aspects;
using AutoMapper;
using Core.Auth.abstracts;
using DataAccess.DTO;
using DataAccess.Models;
using DataAccess.Repositories.abstracts;
using EventHandler.Events.UserEvents;
using EventHandler.Handlers.abstracts;
using Microsoft.AspNetCore.Identity;
using Services.Services.abstracts;

namespace Services.Services.concretes
{
    public class UserCommandService : IUserCommandService
    {
        private readonly IUserCommandRepository _userCommandRepository;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IMapper _mapper;
        private readonly IEventPublisher _eventPublisher;

        public UserCommandService(
            IUserCommandRepository userCommandRepository,
            IUserQueryRepository userQueryRepository,
            IJwtService jwtService,
            IPasswordHasher<User> passwordHasher,
            IMapper mapper,
            IEventPublisher eventPublisher)
        {
            _userCommandRepository = userCommandRepository;
            _userQueryRepository = userQueryRepository;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _eventPublisher = eventPublisher;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<UserResponseDto> RegisterUserAsync(UserCreateDto userDto)
        {
            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = _passwordHasher.HashPassword(user, userDto.Password);
            var createdUser = await _userCommandRepository.CreateAsync(user);
            var userResponseDto = _mapper.Map<UserResponseDto>(createdUser);

            await _eventPublisher.PublishAsync(new UserCreatedEvent
            {
                UserId = userResponseDto.Id,
                Email = userResponseDto.Email
            }, "user_events", "user_created");

            return userResponseDto;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect("Admin")]
        public async Task<UserResponseDto> UpdateUserAsync(int id, UserUpdateDto userDto)
        {
            var existingUser = await _userQueryRepository.GetByIdAsync(id);
            _mapper.Map(userDto, existingUser);
            var updatedUser = await _userCommandRepository.UpdateAsync(id, existingUser);
            var userResponseDto = _mapper.Map<UserResponseDto>(updatedUser);

            await _eventPublisher.PublishAsync(new UserUpdatedEvent
            {
                UserId = userResponseDto.Id,
                Email = userResponseDto.Email
            }, "user_events", "user_updated");

            return userResponseDto;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect("Admin")]
        public async Task<bool> DeleteUserAsync(int id)
        {
            var result = await _userCommandRepository.DeleteAsync(id);
            if (result)
            {
                await _eventPublisher.PublishAsync(new UserDeletedEvent
                {
                    UserId = id
                }, "user_events", "user_deleted");
            }
            return result;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<string> AuthenticateUserAsync(string email, string password)
        {
            var user = await _userQueryRepository.GetByEmailAsync(email);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password.");

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Invalid email or password.");

            return _jwtService.GenerateToken(user.Id.ToString(), user.Email, user.Role);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userQueryRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with id {userId} not found.");

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Current password is incorrect.");

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            var updatedUser = await _userCommandRepository.UpdateAsync(userId, user);
            
            if (updatedUser != null)
            {
                await _eventPublisher.PublishAsync(new UserUpdatedEvent
                {
                    UserId = userId,
                    Email = updatedUser.Email
                }, "user_events", "password_changed");
                return true;
            }
            return false;
        }
    }
}