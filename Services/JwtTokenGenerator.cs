
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using echart_dentnu_api.Models;
using Microsoft.IdentityModel.Tokens;

namespace echart_dentnu_api.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtTokenGenerator> _logger;

        public JwtTokenGenerator(IConfiguration configuration, ILogger<JwtTokenGenerator> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public TokenResponse GenerateTokenResponse(tbdentalrecorduserModel user)
        {
            try
            {
                var expirationMinutes = int.TryParse(_configuration["JWT_TOKEN_EXPIRE_MINUTES"], out int minutes) ? minutes : 180;
                var roleName = GetRoleName(user.RoleID);
                var accessToken = GenerateAccessToken(user, expirationMinutes, roleName);

                return new TokenResponse
                {
                    AccessToken = accessToken,
                    Role = roleName,
                    UserId = user.UserId.ToString(),
                    Users = user.Users,
                    RoleID = user.RoleID,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating token response for user {user.Users}");
                throw;
            }
        }

        private string GenerateAccessToken(tbdentalrecorduserModel user, int expirationMinutes, string roleName)
        {
            var jwtSecret = _configuration["JWT_SECRET"];
            var jwtIssuer = _configuration["JWT_ISSUER"];
            var jwtAudience = _configuration["JWT_AUDIENCE"];

            if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                _logger.LogError("JWT configuration is missing. JWT_SECRET, JWT_ISSUER, and JWT_AUDIENCE must be set.");
                throw new InvalidOperationException("JWT configuration is missing");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Users),
                new Claim(ClaimTypes.Role, roleName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        private string GetRoleName(int roleId)
        {
            return roleId switch
            {
                1 => "Administrator",
                2 => "ระบบนัดหมาย",
                3 => "การเงิน",
                4 => "เวชระเบียน",
                5 => "อาจารย์",
                6 => "ปริญญาตรี",
                7 => "ระบบยา",
                8 => "ผู้ใช้งานทั่วไป",
                9 => "ปริญญาโท",
                10 => "RequirementDiag",
                11 => "หัวหน้าผู้ช่วยทันตแพทย์",
                12 => "ผู้ช่วยทันตแพทย์",
                _ => "ผู้ใช้งานทั่วไป"
            };
        }
    }
}
