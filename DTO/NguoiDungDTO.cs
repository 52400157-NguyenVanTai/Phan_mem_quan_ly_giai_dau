using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class NguoiDungDTO
    {
        public int MaNguoiDung { get; set; }
        public string TenDangNhap { get; set; }
        public string Email { get; set; }
        public string MatKhauMaHoa { get; set; }
        public string VaiTroHeThong { get; set; }
        public string AvatarUrl { get; set; }
        public string Bio { get; set; }
        public DateTime NgayTao { get; set; }

        // --- Trụ cột 5: Ban/Unban ---
        public bool IsBanned { get; set; }
        public string LyDoBan { get; set; }
        public DateTime? ThoiGianBan { get; set; }
    }
}
