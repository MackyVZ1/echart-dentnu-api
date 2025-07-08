using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using echart_dentnu_api.Services;
using System.Security.Claims;
using echart_dentnu_api.Database;

namespace echart_dentnu_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class authController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<authController> _logger;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public authController(AppDbContext db, ILogger<authController> logger, IJwtTokenGenerator jwtTokenGenerator)
        {
            _db = db;
            _logger = logger;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (string.IsNullOrEmpty(loginRequest.Users) || string.IsNullOrEmpty(loginRequest.Passw))
            {
                return BadRequest(new { Message = "Username and password are required" });
            }

            try
            {
                var user = await _db.Tbdentalrecordusers
                                    .FirstOrDefaultAsync(u => u.Users == loginRequest.Users);

                if (user == null || user.Passw != Helper.PasswordHasher.HashMd5(loginRequest.Passw))
                {
                    return Unauthorized(new { Message = "Invalid credentials" });
                }

                user.Status = 1;
                _db.Tbdentalrecordusers.Update(user);
                await _db.SaveChangesAsync();

                var tokenResponse = _jwtTokenGenerator.GenerateTokenResponse(user);

                // Debug: ตรวจสอบ token ที่สร้างขึ้น
                _logger.LogInformation($"Generated token for user {user.Users}:");
                _logger.LogInformation($"Token: {tokenResponse.AccessToken}");
                _logger.LogInformation($"Token Length: {tokenResponse.AccessToken.Length}");
                _logger.LogInformation($"Token Dots Count: {tokenResponse.AccessToken.Count(c => c == '.')}");

                // ตรวจสอบว่า token มี 3 ส่วนหรือไม่
                var tokenParts = tokenResponse.AccessToken.Split('.');
                if (tokenParts.Length != 3)
                {
                    _logger.LogError($"Invalid token format! Expected 3 parts, got {tokenParts.Length}");
                    return StatusCode(500, new { Message = "Token generation failed" });
                }

                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login for user: {loginRequest.Users}");
                return StatusCode(500, new { Message = "Internal Server Error" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { Message = "Invalid user identifier in token" });
            }

            try
            {
                var user = await _db.Tbdentalrecordusers.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                user.Status = 0;
                _db.Tbdentalrecordusers.Update(user);
                await _db.SaveChangesAsync();

                return Ok(new { Message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during logout for user ID: {userId}");
                return StatusCode(500, new { Message = "Internal Server Error" });
            }
        }
    }

    public class LoginRequest
    {
        public string Users { get; set; } = string.Empty;
        public string Passw { get; set; } = string.Empty;
    }
}
