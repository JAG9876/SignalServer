using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SignalApi
{
    public class TokenManager
    {
        private readonly string _secretKey;
        public ITokenRepository tokenRepository { get; set; }

        public TokenManager(string secretKey)
        {
            _secretKey = secretKey;
        }

        public string GenerateAccessToken(string issuer, string audience, string deviceId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, "user_id_123"),
                new Claim("role", "admin"),
                new Claim("deviceId", deviceId)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken(string userId, string deviceId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[] {
                new Claim("userId", userId),
                new Claim("deviceId", deviceId)
            };
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        internal string ExtractDeviceIdFromToken(string bearer)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(bearer);
            var deviceIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "deviceId");
            return deviceIdClaim?.Value ?? string.Empty;
        }

        internal bool ValidateBearerToken(string bearer)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);
            try
            {
                tokenHandler.ValidateToken(bearer, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out SecurityToken validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal bool ValidateRefreshToken(string refreshToken)
        {
            return tokenRepository.HasRefreshToken(refreshToken);
        }
    }
}
