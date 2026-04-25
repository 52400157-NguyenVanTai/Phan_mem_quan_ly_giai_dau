using System;
using System.Data.SqlClient;
using DAL;
using DTO;

namespace BUS
{
    public class TeamBUS
    {
        private readonly TeamDAL _teamDal = new TeamDAL();

        public ServiceResultDTO TaoDoiVaNhomMacDinh(TaoDoiDTO doi, int maTroChoiMacDinh, string tenNhomMacDinh)
        {
            if (doi == null || doi.MaNguoiDungTao <= 0 || string.IsNullOrWhiteSpace(doi.TenDoi))
            {
                return ServiceResultDTO.Fail("Dữ liệu đội không hợp lệ.");
            }

            if (maTroChoiMacDinh <= 0 || string.IsNullOrWhiteSpace(tenNhomMacDinh))
            {
                return ServiceResultDTO.Fail("Thông tin nhóm mặc định không hợp lệ.");
            }

            if (_teamDal.TenDoiTonTai(doi.TenDoi))
            {
                return ServiceResultDTO.Fail("Tên đội đã tồn tại trên hệ thống.");
            }

            if (_teamDal.NguoiDungDangThuocDoiKhac(doi.MaNguoiDungTao))
            {
                return ServiceResultDTO.Fail("Bạn đang thuộc đội khác, không thể tạo đội mới.");
            }

            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    int maDoi = _teamDal.TaoDoi(doi.MaNguoiDungTao, doi.TenDoi, doi.LogoUrl, doi.Slogan, conn, tran);
                    int maNhom = _teamDal.TaoNhom(maDoi, maTroChoiMacDinh, tenNhomMacDinh, doi.MaNguoiDungTao, conn, tran);
                    _teamDal.ThemThanhVien(doi.MaNguoiDungTao, maNhom, "leader", "thi_dau", null, conn, tran);

                    tran.Commit();
                    return ServiceResultDTO.Ok("Tạo đội và nhóm mặc định thành công.", new { maDoi, maNhom });
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return ServiceResultDTO.Fail("Không thể tạo đội: " + ex.Message);
                }
            }
        }

        public ServiceResultDTO TaoNhom(int maNguoiTao, int maDoi, int maTroChoi, string tenNhom, int maCaptain)
        {
            if (maNguoiTao <= 0 || maDoi <= 0 || maTroChoi <= 0 || maCaptain <= 0 || string.IsNullOrWhiteSpace(tenNhom))
            {
                return ServiceResultDTO.Fail("Dữ liệu tạo nhóm không hợp lệ.");
            }

            if (_teamDal.NguoiDungDangThuocDoiKhac(maCaptain))
            {
                return ServiceResultDTO.Fail("Captain đang thuộc đội khác, không thể chỉ định.");
            }

            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    int maNhom = _teamDal.TaoNhom(maDoi, maTroChoi, tenNhom, maCaptain, conn, tran);
                    _teamDal.ThemThanhVien(maCaptain, maNhom, "captain", "thi_dau", null, conn, tran);
                    tran.Commit();
                    return ServiceResultDTO.Ok("Tạo nhóm thành công.", new { maNhom });
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return ServiceResultDTO.Fail("Không thể tạo nhóm: " + ex.Message);
                }
            }
        }

        public ServiceResultDTO ThemThanhVienVaoNhom(int maNguoiThucHien, int maNguoiDung, int maNhom, int maTroChoiNhom, int maViTri, string phanHe)
        {
            if (maNguoiDung <= 0 || maNhom <= 0 || maTroChoiNhom <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu thêm thành viên không hợp lệ.");
            }

            bool duocPhep = _teamDal.KiemTraRoleNoiBoTrongNhom(maNguoiThucHien, maNhom, "leader")
                || _teamDal.KiemTraRoleNoiBoTrongNhom(maNguoiThucHien, maNhom, "captain");
            if (!duocPhep)
            {
                return ServiceResultDTO.Fail("Bạn không có quyền thêm thành viên trong nhóm này.");
            }

            if (!_teamDal.NguoiChoiCoHoSoDungGame(maNguoiDung, maTroChoiNhom))
            {
                return ServiceResultDTO.Fail("Tuyển thủ không có hồ sơ in-game tương ứng với game của nhóm.");
            }

            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    _teamDal.ThemThanhVien(maNguoiDung, maNhom, "member", phanHe == "ban_huan_luyen" ? "ban_huan_luyen" : "thi_dau", maViTri > 0 ? (int?)maViTri : null, conn, tran);
                    tran.Commit();
                    return ServiceResultDTO.Ok("Thêm thành viên vào nhóm thành công.");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return ServiceResultDTO.Fail("Không thể thêm thành viên: " + ex.Message);
                }
            }
        }

        public ServiceResultDTO GiaiTanDoi(int maNguoiThucHien, int maDoi)
        {
            if (maNguoiThucHien <= 0 || maDoi <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu giải tán đội không hợp lệ.");
            }

            bool ok = _teamDal.GiaiTanDoi(maDoi, maNguoiThucHien);
            return ok
                ? ServiceResultDTO.Ok("Giải tán đội thành công. Thành viên đã chuyển về trạng thái Free Agent.")
                : ServiceResultDTO.Fail("Bạn không có quyền giải tán đội hoặc đội không tồn tại.");
        }
    }
}
