using System.Security.Authentication;
using AOP.Aspects;
using AutoMapper;
using Core.Auth.abstracts;
using Core.Exceptions;
using Core.RabbitMQ.abstracts;
using Microsoft.AspNetCore.Identity;
using WebAPI.Data.abstracts;
using WebAPI.DTO;
using WebAPI.Models;
using WebAPI.Services.abstracts;

namespace WebAPI.Services.concretes
{
    public class UserCommandService : IUserCommandService
    {
        private readonly IUserCommandRepository _userCommandRepository;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IMapper _mapper;
        private readonly IRabbitMQService _rabbitMQService;

        public UserCommandService(
            IUserCommandRepository userCommandRepository,
            IUserQueryRepository userQueryRepository,
            IJwtService jwtService,
            IPasswordHasher<User> passwordHasher,
            IMapper mapper,
            IRabbitMQService rabbitMQService)
        {
            _userCommandRepository = userCommandRepository ?? throw new ArgumentNullException(nameof(userCommandRepository));
            _userQueryRepository = userQueryRepository ?? throw new ArgumentNullException(nameof(userQueryRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _rabbitMQService = rabbitMQService ?? throw new ArgumentNullException(nameof(rabbitMQService));
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<UserResponseDto> RegisterUserAsync(UserCreateDto userDto)
        {
            if (userDto == null)
                throw new BadRequestException("User data is required.");

            if (await _userQueryRepository.ExistsByEmailAsync(userDto.Email))
                throw new ConflictException("A user with this email already exists.");

            var user = _mapper.Map<User>(userDto);
            user.Role = "Customer"; // Default role
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.PasswordHash = _passwordHasher.HashPassword(user, userDto.Password);

            var createdUser = await _userCommandRepository.CreateAsync(user);
            var userResponseDto = _mapper.Map<UserResponseDto>(createdUser);

            await _rabbitMQService.PublishMessage("user_events", "user_created", userResponseDto);

            return userResponseDto;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect("Admin")]
        public async Task<UserResponseDto> UpdateUserAsync(int id, UserUpdateDto userDto)
        {
            if (userDto == null)
                throw new BadRequestException("User update data is required.");

            var existingUser = await _userQueryRepository.GetByIdAsync(id);
            if (existingUser == null)
                throw new NotFoundException($"User with id {id} not found.");

            _mapper.Map(userDto, existingUser);
            existingUser.UpdatedAt = DateTime.UtcNow;

            var updatedUser = await _userCommandRepository.UpdateAsync(id, existingUser);
            var userResponseDto = _mapper.Map<UserResponseDto>(updatedUser);

            await _rabbitMQService.PublishMessage("user_events", "user_updated", userResponseDto);

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
                await _rabbitMQService.PublishMessage("user_events", "user_deleted", new { UserId = id });
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
                throw new AuthenticationException("Invalid email or password.");

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
                throw new AuthenticationException("Invalid email or password.");

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
                throw new NotFoundException($"User with id {userId} not found.");

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
                throw new AuthenticationException("Current password is incorrect.");

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            var updatedUser = await _userCommandRepository.UpdateAsync(userId, user);
            if (updatedUser != null)
            {
                await _rabbitMQService.PublishMessage("user_events", "password_changed", new { UserId = userId });
                return true;
            }
            return false;
        }
    }
}