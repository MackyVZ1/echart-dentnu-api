using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration; // จำเป็นต้องมีเพื่อเข้าถึง IConfiguration
using Microsoft.Extensions.Logging; // สำหรับ Logging ภายใน Service
using echart_dentnu_api.Models; // เพื่อเข้าถึง tbdentalrecorduserModel

namespace echart_dentnu_api.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _config;
        private readonly ILogger<JwtTokenGenerator> _logger;

        public JwtTokenGenerator(IConfiguration config, ILogger<JwtTokenGenerator> logger)
        {
            _config = config;
            _logger = logger;
        }

        public string GenerateToken(tbdentalrecorduserModel user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Users),
                new Claim("RoleID", user.RoleID.ToString()),
                new Claim("ClinicId", user.Clinicid?.ToString() ?? "0") // ต้องแน่ใจว่า Clinicid เป็น int?
            };

            // ส่วนของการกำหนด Role Name ตาม RoleID ยังคงเหมือนเดิม
            string roleClaim = user.RoleID switch
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
            claims.Add(new Claim(ClaimTypes.Role, roleClaim));

            var jwtSecret = _config["JWT_SECRET"];
            var jwtIssuer = _config["JWT_ISSUER"];
            var jwtAudience = _config["JWT_AUDIENCE"];
            var jwtExpireMinutes = int.Parse(_config["JWT_TOKEN_EXPIRE_MINUTES"] ?? "60");

            if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                _logger.LogError("JWT configuration is missing in JwtTokenGenerator. Check .env file.");
                throw new InvalidOperationException("JWT_SECRET, JWT_ISSUER, and JWT_AUDIENCE must be set in .env file.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtExpireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}