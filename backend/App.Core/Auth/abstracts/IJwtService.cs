using System.Security.Claims;

namespace Core.Auth.abstracts
{
    public interface IJwtService
    {
        string GenerateToken(string userId, string email, string role);
        string GenerateToken(IEnumerable<Claim> claims);
        (bool isValid, ClaimsPrincipal claimsPrincipal) ValidateToken(string token);
        IDictionary<string, string> DecodeToken(string token);
        bool IsTokenExpired(string token);
        string RefreshToken(string token);
    }
}