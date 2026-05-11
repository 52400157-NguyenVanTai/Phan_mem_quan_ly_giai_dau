using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class NguoidungDTO
    {
        public int ma_nguoi_dung { get; set; }
        public string ten_dang_nhap { get; set; }
        public string email { get; set; }
        public string mat_khau_ma_hoa { get; set; }
        public string vai_tro_he_thong { get; set; }
        public string avatar_url { get; set; }
        public string bio { get; set; }
        public bool is_banned { get; set; } = false;
        public string ly_do_ban { get; set; }
        public DateTime? thoi_gian_ban { get; set; }
        public int? ma_admin_ban { get; set; }
        public DateTime ngay_tao { get; set; }
    }
}
