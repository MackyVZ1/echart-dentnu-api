using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using echart_dentnu_api.Models;
using Microsoft.AspNetCore.Authorization;
using echart_dentnu_api.Services;
using Microsoft.AspNetCore.Server.IIS.Core;

namespace backend_net6.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class authController : ControllerBase
    {
        private readonly Database _db;
        private readonly IConfiguration _config;
        private readonly ILogger<authController> _logger;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public authController(Database db, IConfiguration config, ILogger<authController> logger, IJwtTokenGenerator jwtTokenGenerator) // <-- เพิ่ม IJwtTokenGenerator
        {
            _db = db;
            _config = config;
            _logger = logger;
            _jwtTokenGenerator = jwtTokenGenerator; // <-- กำหนดค่า
        }

        /// <returns>Login</returns>
        /// <response code="200">Login Successfully</response>
        /// <response code="400">Username and password are required, Wrong password</response>
        /// <response code="404">Tbdentalrecorduser not found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Users) || string.IsNullOrEmpty(loginRequest.Passw))
            {
                return BadRequest("Username and password are required");
            }

            _logger.LogDebug($"Attempting login for user: {loginRequest.Users}");

            try
            {
                var user = await _db.Tbdentalrecordusers
                                    // .Include(u => u.Role) // ไม่จำเป็นต้อง Include Role ถ้า logic Role Name อยู่ใน switch case
                                    .FirstOrDefaultAsync(u => u.Users == loginRequest.Users);

                if (user == null)
                {
                    _logger.LogWarning($"User {loginRequest.Users} not found.");
                    return NotFound("Tbdentalrecorduser not found");
                }

                if (user.Passw != PasswordHasher.HashMd5(loginRequest.Passw))
                {
                    _logger.LogWarning($"Invalid password for user: {loginRequest.Users}");
                    return BadRequest("Wrong password");
                }

                // *** อัปเดต Status เป็น 1 เมื่อ Login สำเร็จ ***
                if (user.Status != 1)
                {
                    user.Status = 1;
                    _db.Tbdentalrecordusers.Update(user);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation($"User {loginRequest.Users} status updated to 1 (logged in).");
                }

                // *** ใช้ JwtTokenGenerator ในการสร้าง Token ***
                var jwtToken = _jwtTokenGenerator.GenerateToken(user);

                // ส่วนของการกำหนด Role Name สำหรับ Response (ใช้ Logic เดิมหรือปรับให้รับจาก TokenGenerator)
                string roleNameForResponse = user.RoleID switch
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


                _logger.LogInformation($"User {loginRequest.Users} logged in successfully with RoleID: {user.RoleID}");

                return Ok(new
                {
                    Token = jwtToken,
                    UserInfo = new
                    {
                        UserId = user.UserId,
                        Username = user.Users,
                        RoleID = user.RoleID,
                        RoleName = roleNameForResponse, // ใช้ Role Name ที่สร้างขึ้นมาสำหรับ Response
                    }

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login for user: {loginRequest.Users}");
                // หากเกิด InvalidOperationException จาก JwtTokenGenerator (เช่น JWT config หาย)
                if (ex is InvalidOperationException)
                {
                    return StatusCode(500, ex.Message); // ส่งข้อความ error ที่ชัดเจนกลับไป
                }
                return StatusCode(500, "Internal Server Error");
            }
        }
        /// <returns>Logout</returns>
        /// <response code="200">Logged out successfully.</response>
        /// <response code="400">Invalid user ID format in token, User was already logged out</response>
        /// <response code="401">Invalid token: User ID not found</response>
        /// <response code="404">Tbdentalrecorduser not found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("Logout attempt failed: UserId claim not found in token");
                return Unauthorized("Invalid token: User ID not found");
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning($"Logout attempt failed: Invalid UserId format in token: {userIdClaim.Value}");
                return BadRequest("Invalid user ID format in token");
            }

            _logger.LogDebug($"Attempting logout for user ID: {userId}");

            try
            {
                var user = await _db.Tbdentalrecordusers.FindAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning($"Logout attempt failed: User with ID {userId} not found");
                    return NotFound("Tbdentalrecorduser not found");
                }

                if (user.Status != 0)
                {
                    user.Status = 0;
                    _db.Tbdentalrecordusers.Update(user);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation($"User {user.Users} (ID: {userId}) status updated to 0 (logged out).");
                }
                else
                {
                    _logger.LogInformation($"User {user.Users} (ID: {userId}) was already logged out (status 0).");
                    return BadRequest($"User {user.Users} (ID: {userId}) was already logged out");
                }

                return Ok("Logged out successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during logout for user ID: {userId}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }

    public class LoginRequest
    {
        public string Users { get; set; } = string.Empty;
        public string Passw { get; set; } = string.Empty;
    }
}