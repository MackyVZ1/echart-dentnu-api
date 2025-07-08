
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
        public string Role { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public int RoleID { get; set; }
        public string Users { get; set; } = string.Empty;
    }
}
