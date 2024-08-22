using AutoMapper;
using WebAPI.Aspects;
using WebAPI.Auth.abstracts;
using WebAPI.Core.Exceptions;
using WebAPI.Data.abstracts;
using WebAPI.Data.Abstracts;
using WebAPI.DTO;
using WebAPI.Services.abstracts;

namespace WebAPI.Services.concretes;

public class UserQueryService : IUserQueryService
{
    private readonly IUserQueryRepository _userQueryRepository;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;

    public UserQueryService(IUserQueryRepository userQueryRepository, IJwtService jwtService, IMapper mapper)
    {
        _userQueryRepository = userQueryRepository ?? throw new ArgumentNullException(nameof(userQueryRepository));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [LoggingAspect]
    [ExceptionAspect]
    [PerformanceAspect]
    [AuthorizationAspect("Admin")]
    public async Task<UserResponseDto> GetUserByIdAsync(int id)
    {
        var user = await _userQueryRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException($"User with id {id} not found.");
        return _mapper.Map<UserResponseDto>(user);
    }

    [LoggingAspect]
    [ExceptionAspect]
    [PerformanceAspect]
    [AuthorizationAspect("Admin")]
    public async Task<UserResponseDto> GetUserByEmailAsync(string email)
    {
        var user = await _userQueryRepository.GetByEmailAsync(email);
        if (user == null)
            throw new NotFoundException($"User with email {email} not found.");
        return _mapper.Map<UserResponseDto>(user);
    }

    [LoggingAspect]
    [ExceptionAspect]
    [PerformanceAspect]
    [AuthorizationAspect("Admin")]
    [CachingAspect(300, "users:all")]
    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _userQueryRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserResponseDto>>(users);
    }

    [LoggingAspect]
    [ExceptionAspect]
    [PerformanceAspect]
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _userQueryRepository.ExistsByEmailAsync(email);
    }
}