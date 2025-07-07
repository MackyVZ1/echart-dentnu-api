namespace echart_dentnu_api.Services
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(echart_dentnu_api.Models.tbdentalrecorduserModel user);
        // ถ้าต้องการส่งข้อมูล RoleName กลับด้วย อาจเปลี่ยนเป็น
        // (string Token, string RoleName) GenerateToken(backend_net6.Models.tbdentalrecorduserModel user);
        // หรือให้ AutController สร้าง RoleName เอง
    }
}