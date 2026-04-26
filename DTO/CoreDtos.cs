using System;
using System.Collections.Generic;

namespace DTO
{
    public class ServiceResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public static ServiceResultDTO Ok(string message, object data = null)
        {
            return new ServiceResultDTO { Success = true, Message = message, Data = data };
        }

        public static ServiceResultDTO Fail(string message)
        {
            return new ServiceResultDTO { Success = false, Message = message };
        }
    }

    public class DangKyNguoiDungDTO
    {
        public string TenDangNhap { get; set; }
        public string Email { get; set; }
        public string MatKhau { get; set; }
        public string AvatarUrl { get; set; }
        public string Bio { get; set; }
    }

    public class DangNhapDTO
    {
        public string DinhDanh { get; set; }
        public string MatKhau { get; set; }
    }

    public class CapNhatMatKhauDTO
    {
        public int MaNguoiDung { get; set; }
        public string MatKhauCu { get; set; }
        public string MatKhauMoi { get; set; }
    }

    public class CapNhatThongTinCoBanDTO
    {
        public int MaNguoiDung { get; set; }
        public string AvatarUrl { get; set; }
        public string Bio { get; set; }
    }

    public class HoSoInGameDTO
    {
        public int MaHoSo { get; set; }
        public int MaNguoiDung { get; set; }
        public int MaTroChoi { get; set; }
        public int MaViTriSoTruong { get; set; }
        public string InGameId { get; set; }
        public string InGameName { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class TroChoiDTO
    {
        public int MaTroChoi { get; set; }
        public string TenGame { get; set; }
        public string TheLoai { get; set; }
    }

    public class ViTriDTO
    {
        public int MaViTri { get; set; }
        public int MaTroChoi { get; set; }
        public string TenViTri { get; set; }
        public string KyHieu { get; set; }
        public string LoaiViTri { get; set; }
    }

    public class TaoDoiDTO
    {
        public int MaNguoiDungTao { get; set; }
        public string TenDoi { get; set; }
        public string LogoUrl { get; set; }
        public string Slogan { get; set; }
    }

    public class DoiDTO
    {
        public int MaDoi { get; set; }
        public string TenDoi { get; set; }
        public int MaManager { get; set; }
        public string LogoUrl { get; set; }
        public string Slogan { get; set; }
        public string TrangThai { get; set; }
    }

    public class NhomDoiDTO
    {
        public int MaNhom { get; set; }
        public int MaDoi { get; set; }
        public int MaTroChoi { get; set; }
        public string TenNhom { get; set; }
        public int? MaDoiTruongNhom { get; set; }
    }

    public class ThanhVienDoiDTO
    {
        public int MaThanhVien { get; set; }
        public int MaNguoiDung { get; set; }
        public int MaNhom { get; set; }
        public int? MaViTri { get; set; }
        public string VaiTroNoiBo { get; set; }
        public string PhanHe { get; set; }
        public string TrangThaiDuyet { get; set; }
        public string TrangThaiHopDong { get; set; }
    }

    public class BaiDangTuyenDungDTO
    {
        public int MaBaiDang { get; set; }
        public int MaDoi { get; set; }
        public int MaNhom { get; set; }
        public int MaViTri { get; set; }
        public string NoiDung { get; set; }
        public string TrangThai { get; set; }
    }

    public class DonUngTuyenDTO
    {
        public int MaDon { get; set; }
        public int MaBaiDang { get; set; }
        public int MaUngVien { get; set; }
        public string TrangThai { get; set; }
    }

    public class LoiMoiDTO
    {
        public int MaLoiMoi { get; set; }
        public int MaDoi { get; set; }
        public int MaNhom { get; set; }
        public int MaNguoiDuocMoi { get; set; }
        public string TrangThai { get; set; }
    }

    public class YeuCauTaoGiaiDTO
    {
        public int MaYeuCau { get; set; }
        public int MaNguoiGui { get; set; }
        public string TenGiaiDau { get; set; }
        public int? MaTroChoi { get; set; }
        public string TheThuc { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public decimal TongGiaiThuong { get; set; }
        public string TrangThai { get; set; }
        public int? MaAdminDuyet { get; set; }
        public string LyDoHuy { get; set; }
    }

    public class TaoGiaiDauDTO
    {
        public int MaNguoiTao { get; set; }
        public string TenGiaiDau { get; set; }
        public int? MaTroChoi { get; set; }
        public string TheThuc { get; set; }
        public string BannerUrl { get; set; }
        public decimal TongGiaiThuong { get; set; }
        public string MoTa { get; set; }
        public int? SoNguoiMoiDoi { get; set; }
        public DateTime? ThoiGianMoDangKy { get; set; }
        public DateTime? ThoiGianDongDangKy { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string TrangThaiKhoiTao { get; set; }
        public List<TaoGiaiThuongDTO> GiaiThuongs { get; set; }
    }

    public class TaoGiaiThuongDTO
    {
        public string TenGiai { get; set; }
        public string PhanThuong { get; set; }
        public string MoTa { get; set; }
    }

    public class CapNhatTrangThaiGiaiDTO
    {
        public int MaGiaiDau { get; set; }
        public int MaNguoiThucHien { get; set; }
        public string TrangThaiMoi { get; set; }
        public string LyDo { get; set; }
    }

    public class GiaiDauDTO
    {
        public int MaGiaiDau { get; set; }
        public int? MaNguoiTao { get; set; }
        public string TenGiaiDau { get; set; }
        public int? MaTroChoi { get; set; }
        public string BannerUrl { get; set; }
        public decimal TongGiaiThuong { get; set; }
        public DateTime? ThoiGianMoDangKy { get; set; }
        public DateTime? ThoiGianDongDangKy { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string TrangThai { get; set; }
        public bool HienThiPublic { get; set; }
        public DateTime? ThoiGianKhoa { get; set; }
        public int? MaNguoiKhoa { get; set; }
        public string LyDoKhoa { get; set; }
    }

    public class TaoGiaiDoanDTO
    {
        public int MaGiaiDau { get; set; }
        public string TenGiaiDoan { get; set; }
        public string TheThuc { get; set; }
        public int SoDoiDiTiep { get; set; }
        public int? DiemNguongMatchPoint { get; set; }
    }

    public class GiaiDoanDTO
    {
        public int MaGiaiDoan { get; set; }
        public int MaGiaiDau { get; set; }
        public string TenGiaiDoan { get; set; }
        public string TheThuc { get; set; }
        public int ThuTu { get; set; }
        public int SoDoiDiTiep { get; set; }
        public int? DiemNguongMatchPoint { get; set; }
    }

    public class DuyetThamGiaGiaiDTO
    {
        public int MaGiaiDau { get; set; }
        public int MaNhom { get; set; }
        public bool ChapNhan { get; set; }
    }

    public class CapNhatHatGiongDTO
    {
        public int MaGiaiDau { get; set; }
        public int MaNhom { get; set; }
        public int HatGiong { get; set; }
    }

    public class CapNhatDoiHinhGiaiDTO
    {
        public int MaGiaiDau { get; set; }
        public int MaNhom { get; set; }
    }

    public class TaoLichGiaiDoanDTO
    {
        public int MaGiaiDau { get; set; }
        public int MaGiaiDoan { get; set; }
        public bool DungHatGiong { get; set; }
    }

    public class MatchNodeDTO
    {
        public int MaTran { get; set; }
        public int MaGiaiDau { get; set; }
        public int MaGiaiDoan { get; set; }
        public int SoVong { get; set; }
        public string NhanhDau { get; set; }
        public string TheThucTran { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public string TrangThai { get; set; }
        public int? MaTranTiepTheoThang { get; set; }
        public int? MaTranTiepTheoThua { get; set; }
        public string TenNhomA { get; set; }
        public string TenNhomB { get; set; }
    }

    public class RefereePlayerStatInputDTO
    {
        public int MaNguoiDung { get; set; }
        public int? MaViTri { get; set; }
        public int SoKill { get; set; }
        public int SoDeath { get; set; }
        public int SoAssist { get; set; }
        public double? DiemSinhTon { get; set; }
    }

    public class RefereeSubmitResultDTO
    {
        public int MaTran { get; set; }
        public string LyDo { get; set; }
        public List<RefereePlayerStatInputDTO> ChiSoNguoiChoi { get; set; }
    }

    public class TaoKhieuNaiKetQuaDTO
    {
        public int MaTran { get; set; }
        public int MaNhom { get; set; }
        public string NoiDung { get; set; }
    }

    public class XuLyKhieuNaiDTO
    {
        public int MaKhieuNai { get; set; }
        public bool ChapNhan { get; set; }
        public string PhanHoiAdmin { get; set; }
    }

    public class PublicTournamentOverviewDTO
    {
        public int MaGiaiDau { get; set; }
        public string TenGiaiDau { get; set; }
        public string BannerUrl { get; set; }
        public string TrangThai { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public List<GiaiDoanDTO> GiaiDoan { get; set; }
    }
}
