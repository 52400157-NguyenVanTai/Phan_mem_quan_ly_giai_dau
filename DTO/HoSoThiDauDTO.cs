using System;

namespace DTO
{
    public class HoSoThiDauDTO
    {
        public int ma_ho_so { get; set; }
        public int ma_nguoi_dung { get; set; }
        public int ma_tro_choi { get; set; }
        public string ten_game { get; set; }
        public string in_game_id { get; set; }
        public string in_game_name { get; set; }
        public string loai_vi_tri { get; set; }
        public int? ma_vi_tri_so_truong { get; set; }
        public string ten_vi_tri { get; set; }
        public string thanh_tich { get; set; }
        public DateTime ngay_cap_nhat { get; set; }
    }

    public class HoSoThiDauRequestDTO
    {
        public int ma_tro_choi { get; set; }
        public string in_game_id { get; set; }
        public string in_game_name { get; set; }
        public string loai_vi_tri { get; set; }
        public int? ma_vi_tri_so_truong { get; set; }
        public string thanh_tich { get; set; }
    }

    public class HoSoThiDauDeleteRequestDTO
    {
        public int ma_tro_choi { get; set; }
    }

    public class TroChoiDTO
    {
        public int ma_tro_choi { get; set; }
        public string ten_game { get; set; }
        public string the_loai { get; set; }
    }

    public class ViTriDTO
    {
        public int ma_vi_tri { get; set; }
        public int? ma_tro_choi { get; set; }
        public string ten_vi_tri { get; set; }
        public string ky_hieu { get; set; }
        public string loai_vi_tri { get; set; }
    }
}
