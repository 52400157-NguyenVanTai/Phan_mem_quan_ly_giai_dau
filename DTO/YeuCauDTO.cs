using System;
using System.Collections.Generic;

namespace DTO
{
    // DTO thống nhất cho thẻ Yêu Cầu trên trang /YeuCau
    public class YeuCauTongHopDTO
    {
        public int ma_yeu_cau { get; set; }
        
        // Loại yêu cầu: 
        // "yeu_cau_tao_giai_dau" (Admin duyệt)
        // "dang_ky_tham_gia_giai_dau" (BTC duyệt đội)
        // "loi_moi_tham_gia_giai_dau" (Chủ tịch nhận)
        // "loi_moi_trong_tai", "loi_moi_btc" (User nhận)
        // "loi_moi", "xin_gia_nhap" (từ app-doi cũ)
        public string loai_yeu_cau { get; set; }
        
        public string tieu_de { get; set; }
        public string noi_dung { get; set; }
        public DateTime ngay_tao { get; set; }
        public string trang_thai { get; set; }

        // Thông tin liên quan
        public int? ma_nguoi_gui { get; set; }
        public string ten_nguoi_gui { get; set; }
        public int? ma_nguoi_nhan { get; set; }
        public string ten_nguoi_nhan { get; set; }
        
        // Tùy theo loại yêu cầu, ma_entity sẽ trỏ tới mã giải đấu, mã đội, ...
        public int? ma_giai_dau { get; set; }
        public string ten_giai_dau { get; set; }
        public int? ma_doi { get; set; }
        public string ten_doi { get; set; }
        
        public string ten_game { get; set; }

        // Dữ liệu mở rộng (JSON hoặc object) để chứa full form giải đấu / full profile đội
        public object thong_tin_chi_tiet { get; set; }
    }

    public class XuLyYeuCauRequestDTO
    {
        public string loai_yeu_cau { get; set; }
        public int ma_yeu_cau { get; set; }
        public bool chap_nhan { get; set; }
        public string ly_do { get; set; } // Dùng cho Admin từ chối giải đấu
    }

    public class DangKyGiaiDauRequestDTO
    {
        public int ma_giai_dau { get; set; }
        public int ma_doi { get; set; }
    }

    public class MoiThamGiaGiaiRequestDTO
    {
        public int ma_giai_dau { get; set; }
        public int ma_doi { get; set; }
        public string loi_nhan { get; set; }
    }

    public class MoiNhanSuGiaiDauRequestDTO
    {
        public int ma_giai_dau { get; set; }
        public string username_or_email { get; set; }
        public string vai_tro { get; set; } // "trong_tai" hoặc "btc"
        public string loi_nhan { get; set; }
    }
}
