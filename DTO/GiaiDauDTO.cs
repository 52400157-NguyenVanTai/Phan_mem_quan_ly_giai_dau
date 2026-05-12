using System;
using System.Collections.Generic;

namespace DTO
{
    // DTO hien thi giai dau trong danh sach
    public class GiaiDauDTO
    {
        public int ma_giai_dau { get; set; }
        public string ten_giai_dau { get; set; }
        public int? ma_tro_choi { get; set; }
        public string ten_game { get; set; }
        public string the_thuc { get; set; }
        public string banner_url { get; set; }
        public string mo_ta { get; set; }
        public int? so_nguoi_moi_doi { get; set; }
        public int so_doi_toi_thieu { get; set; }
        public int? so_doi_toi_da { get; set; }
        public int min_members_per_team { get; set; }
        public string trang_thai { get; set; }
        public int? ma_nguoi_tao { get; set; }
        public string ten_nguoi_tao { get; set; }
        public string ly_do_tu_choi { get; set; }
        public bool is_registration_locked { get; set; }
        public bool dang_mo_dang_ky { get; set; }
        public decimal tong_giai_thuong { get; set; }
        public DateTime ngay_tao { get; set; }   // thời điểm tạo bản ghi
        public int so_doi_dang_ky { get; set; }
        public int so_doi_da_duyet { get; set; }
    }

    // Chi tiet giai dau bao gom giai doan
    public class GiaiDauChiTietDTO
    {
        public GiaiDauDTO giai_dau { get; set; }
        public List<GiaiDoanDTO> giai_doan { get; set; }
        public List<DoiThamGiaDTO> doi_tham_gia { get; set; }
        public List<GiaiThuongDTO> danh_sach_giai_thuong { get; set; }
    }

    // DTO cho giai doan thi dau
    public class GiaiDoanDTO
    {
        public int ma_giai_doan { get; set; }
        public int ma_giai_dau { get; set; }
        public int so_thu_tu { get; set; }
        public string ten_giai_doan { get; set; }
        public string the_thuc { get; set; }
        public int so_doi { get; set; }
        public int? so_doi_di_tiep { get; set; }
        public int? nguong_match_point { get; set; }
        public string bang_diem_json { get; set; }
        public string trang_thai { get; set; }
    }

    // Doi tham gia giai
    public class DoiThamGiaDTO
    {
        public int ma_tham_gia { get; set; }
        public int ma_nhom { get; set; }
        public string ten_doi { get; set; }
        public string logo_url { get; set; }
        public string ten_game { get; set; }
        public string trang_thai_duyet { get; set; }
        public string trang_thai_tham_gia { get; set; }
    }

    // Giai thuong
    public class GiaiThuongDTO
    {
        public int ma_giai_thuong { get; set; }
        public string ten_giai { get; set; }
        public decimal gia_tri { get; set; }
    }

    public class GiaiThuongRequestDTO
    {
        public string ten_giai { get; set; }
        public decimal gia_tri { get; set; }
    }

    // Request DTO: Tao giai dau (Buoc 1 + 2 + 3)
    public class TaoGiaiDauRequestDTO
    {
        // Buoc 1: Thong tin co ban
        public string ten_giai_dau { get; set; }
        public string banner_url { get; set; }
        public string mo_ta { get; set; }
        public decimal tong_giai_thuong { get; set; }
        public List<GiaiThuongRequestDTO> danh_sach_giai_thuong { get; set; }

        // Buoc 2: Tua game & rang buoc doi
        public int? ma_tro_choi { get; set; }
        public int so_doi_toi_thieu { get; set; }
        public int? so_doi_toi_da { get; set; }
        public int min_members_per_team { get; set; }

        // Buoc 3: Multi-Stage
        public List<GiaiDoanRequestDTO> giai_doan { get; set; }
    }

    // Request DTO: Moi giai doan
    public class GiaiDoanRequestDTO
    {
        public int so_thu_tu { get; set; }
        public string ten_giai_doan { get; set; }
        public string the_thuc { get; set; }
        public int so_doi { get; set; }
        public int? so_doi_di_tiep { get; set; }
        public int? nguong_match_point { get; set; }
        public string bang_diem_json { get; set; }
    }

    // Request DTO: Cap nhat giai dau
    public class CapNhatGiaiDauRequestDTO
    {
        public int ma_giai_dau { get; set; }
        public string ten_giai_dau { get; set; }
        public string banner_url { get; set; }
        public string mo_ta { get; set; }
        public decimal tong_giai_thuong { get; set; }
        public List<GiaiThuongRequestDTO> danh_sach_giai_thuong { get; set; }
        public int? ma_tro_choi { get; set; }
        public int so_doi_toi_thieu { get; set; }
        public int? so_doi_toi_da { get; set; }
        public int min_members_per_team { get; set; }
        public List<GiaiDoanRequestDTO> giai_doan { get; set; }
    }

    // Request DTO: Admin tu choi giai dau
    public class TuChoiGiaiDauRequestDTO
    {
        public int ma_giai_dau { get; set; }
        public string ly_do { get; set; }
    }

    // Request DTO: Hanh dong trang thai don gian (chi can ma_giai_dau)
    public class GiaiDauActionRequestDTO
    {
        public int ma_giai_dau { get; set; }
    }

    // ===== PHASE 2: MATCH & REFEREE DTOs =====

    public class TranDauDTO
    {
        public int ma_tran { get; set; }
        public int ma_giai_dau { get; set; }
        public int? ma_giai_doan { get; set; }
        public string ten_giai_doan { get; set; }
        public int? ma_trong_tai { get; set; }
        public string ten_trong_tai { get; set; }
        public string vong_dau { get; set; }
        public int? so_vong { get; set; }
        public string nhanh_dau { get; set; }
        public string the_thuc_tran { get; set; }
        public string trang_thai { get; set; }
        public string id_phong_game { get; set; }
        public string mat_khau_phong { get; set; }
        // Không lưu thời gian — BTC bấm nút để bắt đầu/kết thúc trận (Manual-Trigger)
        public List<ChiTietTranDauDTO> chi_tiet { get; set; }
    }

    public class ChiTietTranDauDTO
    {
        public int ma_tran { get; set; }
        public int ma_nhom { get; set; }
        public string ten_doi { get; set; }
        public string logo_url { get; set; }
        public double diem_so { get; set; }
        public int? thu_hang { get; set; }
        public string ket_qua { get; set; }
        public int so_kill { get; set; }
        public bool is_check_in { get; set; }
    }

    public class BangXepHangDTO
    {
        public int ma_nhom { get; set; }
        public string ten_doi { get; set; }
        public string logo_url { get; set; }
        public int so_tran_da_dau { get; set; }
        public int so_tran_thang { get; set; }
        public int so_tran_thua { get; set; }
        public int hieu_so_phu { get; set; }
        public double tong_diem_hang { get; set; }
        public double tong_diem_kill { get; set; }
        public double diem_tong_ket { get; set; }
        public int thu_hang_hien_tai { get; set; }
        public bool is_match_point { get; set; }
    }

    // Request: Ket qua tran 1v1 (MOBA/FPS)
    public class KetQuaTranRequestDTO
    {
        public int ma_tran { get; set; }
        public int ma_nhom_thang { get; set; }
        public int? ty_so_doi_1 { get; set; }
        public int? ty_so_doi_2 { get; set; }
    }

    // Request: Ket qua Battle Royale / Champion Rush
    public class KetQuaBattleRoyaleRequestDTO
    {
        public int ma_tran { get; set; }
        public List<KetQuaDoiBRDTO> ket_qua { get; set; }
    }

    public class KetQuaDoiBRDTO
    {
        public int ma_nhom { get; set; }
        public int thu_hang { get; set; }
        public int so_kill { get; set; }
    }

    // Request: Set Room/Password
    public class SetRoomRequestDTO
    {
        public int ma_tran { get; set; }
        public string id_phong_game { get; set; }
        public string mat_khau_phong { get; set; }
    }

    // Request: Check-in
    public class CheckInRequestDTO
    {
        public int ma_tran { get; set; }
        public int ma_nhom { get; set; }
    }

    // Request: Invite referee
    public class MoiTrongTaiRequestDTO
    {
        public int ma_giai_dau { get; set; }
        public string ten_dang_nhap { get; set; }
    }

    public class TrongTaiDTO
    {
        public int ma_giai_dau { get; set; }
        public int ma_nguoi_dung { get; set; }
        public string ten_dang_nhap { get; set; }
        public string trang_thai { get; set; }
        public DateTime ngay_cap_quyen { get; set; }
    }

    // DTO cho bracket view
    public class BracketDTO
    {
        public GiaiDoanDTO giai_doan { get; set; }
        public List<TranDauDTO> tran_dau { get; set; }
        public List<BangXepHangDTO> bang_xep_hang { get; set; }
    }
}
