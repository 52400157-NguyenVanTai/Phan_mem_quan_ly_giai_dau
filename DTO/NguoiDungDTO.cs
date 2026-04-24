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
        public string VaiTro { get; set; }
        public DateTime NgayTao { get; set; }
    }
}
