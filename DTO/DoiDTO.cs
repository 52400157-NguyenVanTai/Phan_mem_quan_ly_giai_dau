using System;
using System.Collections.Generic;

namespace DTO
{
    public class DoiDTO
    {
        public int ma_doi { get; set; }
        public int ma_nhom { get; set; }
        public int ma_tro_choi { get; set; }
        public string ten_game { get; set; }
        public string ten_doi { get; set; }
        public string ten_viet_tat { get; set; }
        public int ma_chu_tich { get; set; }
        public string ten_chu_tich { get; set; }
        public string logo_url { get; set; }
        public string slogan { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
        public bool dang_tuyen { get; set; }
        public DateTime ngay_tao { get; set; }
        public int so_thanh_vien { get; set; }
        public string vai_tro_cua_toi { get; set; }
    }

    public class TaoDoiRequestDTO
    {
        public string ten_doi { get; set; }
        public string ten_viet_tat { get; set; }
        public int ma_tro_choi { get; set; }
        public string logo_url { get; set; }
        public string slogan { get; set; }
        public string mo_ta { get; set; }
    }

    public class CapNhatDoiRequestDTO
    {
        public int ma_doi { get; set; }
        public string ten_doi { get; set; }
        public string ten_viet_tat { get; set; }
        public string logo_url { get; set; }
        public string slogan { get; set; }
        public string mo_ta { get; set; }
        public bool dang_tuyen { get; set; }
    }

    public class ThanhVienDoiDTO
    {
        public int ma_thanh_vien { get; set; }
        public int ma_nguoi_dung { get; set; }
        public string username { get; set; }
        public string ho_ten { get; set; }
        public string avatar_url { get; set; }
        public string ten_vi_tri { get; set; }
        public string vai_tro_noi_bo { get; set; }
        public string phan_he { get; set; }
        public DateTime ngay_tham_gia { get; set; }
    }

    public class MoiThanhVienRequestDTO
    {
        public int ma_doi { get; set; }
        public string username_or_email { get; set; }
        public int? ma_vi_tri { get; set; }
        public string mo_ta { get; set; }
    }

    public class XinGiaNhapDoiRequestDTO
    {
        public int ma_doi { get; set; }
        public string mo_ta { get; set; }
    }

    public class CapNhatVaiTroThanhVienRequestDTO
    {
        public int ma_doi { get; set; }
        public int ma_nguoi_dung { get; set; }
        public string vai_tro_noi_bo { get; set; }
    }

    public class LoaiThanhVienRequestDTO
    {
        public int ma_doi { get; set; }
        public int ma_nguoi_dung { get; set; }
    }

    public class RoiDoiRequestDTO
    {
        public int ma_doi { get; set; }
    }

    public class BatTuyenDungRequestDTO
    {
        public int ma_doi { get; set; }
        public bool dang_tuyen { get; set; }
    }

    public class DoiChiTietDTO
    {
        public DoiDTO doi { get; set; }
        public List<ThanhVienDoiDTO> thanh_vien { get; set; }
        public List<DoiTranDauDTO> lich_su_thi_dau { get; set; }
        public List<DoiGiaiDauDTO> giai_dau { get; set; }
        public List<DoiTranDauDTO> tran_dau_tiep_theo { get; set; }
        public DoiThongKeDTO thong_ke { get; set; }
    }

    public class DoiTranDauDTO
    {
        public int ma_tran { get; set; }
        public string ten_giai_dau { get; set; }
        public string vong_dau { get; set; }
        public DateTime? thoi_gian_bat_dau { get; set; }
        public string trang_thai { get; set; }
        public string ket_qua { get; set; }
        public double diem_so { get; set; }
    }

    public class DoiGiaiDauDTO
    {
        public int ma_giai_dau { get; set; }
        public string ten_giai_dau { get; set; }
        public string trang_thai { get; set; }
        public string trang_thai_tham_gia { get; set; }
        public DateTime? ngay_bat_dau { get; set; }
        public DateTime? ngay_ket_thuc { get; set; }
    }

    public class DoiThongKeDTO
    {
        public int tong_tran { get; set; }
        public int so_tran_thang { get; set; }
        public int so_tran_thua { get; set; }
        public int so_giai_tham_gia { get; set; }
        public List<string> giai_thuong { get; set; }
    }

    public class YeuCauDoiDTO
    {
        public int ma_yeu_cau { get; set; }
        public string loai_yeu_cau { get; set; }
        public int ma_doi { get; set; }
        public string ten_doi { get; set; }
        public string ten_game { get; set; }
        public int? ma_nguoi_gui { get; set; }
        public string ten_nguoi_gui { get; set; }
        public int? ma_nguoi_nhan { get; set; }
        public string ten_nguoi_nhan { get; set; }
        public string vi_tri { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
        public DateTime ngay_tao { get; set; }
        public string ho_so_in_game_id { get; set; }
        public string ho_so_in_game_name { get; set; }
        public string ho_so_vi_tri { get; set; }
        public string ho_so_thanh_tich { get; set; }
    }

    public class XuLyYeuCauDoiRequestDTO
    {
        public int ma_yeu_cau { get; set; }
        public string loai_yeu_cau { get; set; }
        public bool chap_nhan { get; set; }
    }
}
