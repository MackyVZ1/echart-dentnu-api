
using echart_dentnu_api.Models;

namespace echart_dentnu_api.Services
{
    public interface IJwtTokenGenerator
    {
        TokenResponse GenerateTokenResponse(tbdentalrecorduserModel user);
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Role { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;
    }
}
