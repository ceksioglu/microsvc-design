using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Auth.abstracts;
using Microsoft.IdentityModel.Tokens;

namespace Core.Auth.concretes
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }

        public string GenerateToken(string userId, string email, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            return GenerateToken(claims);
        }

        public string GenerateToken(IEnumerable<Claim> claims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (bool isValid, ClaimsPrincipal claimsPrincipal) ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return (true, principal);
            }
            catch
            {
                return (false, null);
            }
        }

        public IDictionary<string, string> DecodeToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            var claims = jsonToken?.Claims.ToDictionary(c => c.Type, c => c.Value) 
                         ?? new Dictionary<string, string>();

            return claims;
        }

        public bool IsTokenExpired(string token)
        {
            var (isValid, principal) = ValidateToken(token);
            if (!isValid) return true;

            var expiryClaim = principal.FindFirst(JwtRegisteredClaimNames.Exp);
            if (expiryClaim == null) return true;

            var expiryDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiryClaim.Value));
            return expiryDate < DateTimeOffset.UtcNow;
        }

        public string RefreshToken(string token)
        {
            var (isValid, principal) = ValidateToken(token);
            if (!isValid) throw new SecurityTokenException("Invalid token");

            var claims = principal.Claims.ToList();
            
            // Remove the old expiration claim
            claims.RemoveAll(c => c.Type == JwtRegisteredClaimNames.Exp);

            // Add a new JWT ID claim
            claims.RemoveAll(c => c.Type == JwtRegisteredClaimNames.Jti);
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            return GenerateToken(claims);
        }

        private TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}