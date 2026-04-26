using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DAL;
using DTO;

namespace BUS
{
    public class TeamBUS
    {
        private readonly TeamDAL _teamDal = new TeamDAL();
        private readonly RecruitmentDAL _recruitDal = new RecruitmentDAL();

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

        public ServiceResultDTO LayDoiCuaToi(int maNguoiDung)
        {
            if (maNguoiDung <= 0) return ServiceResultDTO.Fail("Người dùng không hợp lệ.");

            DataTable dt = _teamDal.LayDoiCuaToi(maNguoiDung);
            if (dt.Rows.Count == 0)
                return ServiceResultDTO.Ok("Không thuộc đội nào.", null);

            var row = dt.Rows[0];
            var data = new
            {
                ma_doi       = Convert.ToInt32(row["ma_doi"]),
                ten_doi      = row["ten_doi"].ToString(),
                logo_url     = row["logo_url"] == DBNull.Value ? null : row["logo_url"].ToString(),
                slogan       = row["slogan"] == DBNull.Value ? null : row["slogan"].ToString(),
                trang_thai   = row["trang_thai"].ToString(),
                vai_tro      = row["vai_tro_noi_bo"].ToString(),
                phan_he      = row["phan_he"] == DBNull.Value ? null : row["phan_he"].ToString(),
                ten_game     = row["ten_game"].ToString(),
                ten_nhom     = row["ten_nhom"].ToString()
            };
            return ServiceResultDTO.Ok("Lấy thông tin đội thành công.", data);
        }

        // Tất cả đội+nhóm mà user tham gia
        public ServiceResultDTO LayTatCaDoiCuaToi(int maNguoiDung)
        {
            if (maNguoiDung <= 0) return ServiceResultDTO.Fail("Người dùng không hợp lệ.");
            DataTable dt = _teamDal.LayTatCaDoiCuaToi(maNguoiDung);
            return ServiceResultDTO.Ok("OK", DataTableToList(dt));
        }

        // Team Explorer
        public ServiceResultDTO LayDanhSachDoiCongKhai(int? maTroChoi, bool? dangTuyen, string tuKhoa)
        {
            DataTable dt = _teamDal.LayDanhSachDoiCongKhai(maTroChoi, dangTuyen, tuKhoa);
            return ServiceResultDTO.Ok("OK", DataTableToList(dt));
        }

        // Chi tiết đội
        public ServiceResultDTO LayChiTietDoi(int maDoi)
        {
            DataTable dt = _teamDal.LayChiTietDoi(maDoi);
            if (dt.Rows.Count == 0) return ServiceResultDTO.Fail("Đội không tồn tại.");
            var row = dt.Rows[0];
            var squads = DataTableToList(_teamDal.LayDanhSachNhom(maDoi));
            return ServiceResultDTO.Ok("OK", new
            {
                ma_doi = Convert.ToInt32(row["ma_doi"]),
                ten_doi = row["ten_doi"].ToString(),
                logo_url = row["logo_url"] == DBNull.Value ? null : row["logo_url"].ToString(),
                slogan = row["slogan"] == DBNull.Value ? null : row["slogan"].ToString(),
                trang_thai = row["trang_thai"].ToString(),
                ma_doi_truong = Convert.ToInt32(row["ma_doi_truong"]),
                ma_manager = row["ma_manager"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ma_manager"]),
                ten_manager = row["ten_manager"].ToString(),
                dang_tuyen = Convert.ToBoolean(row["dang_tuyen"]),
                mo_ta = row["mo_ta"].ToString(),
                nhom_doi = squads
            });
        }

        // Thành viên nhóm
        public ServiceResultDTO LayThanhVienNhom(int maNhom)
        {
            DataTable dt = _teamDal.LayThanhVienNhom(maNhom);
            return ServiceResultDTO.Ok("OK", DataTableToList(dt));
        }

        // Xin gia nhập
        public ServiceResultDTO XinGiaNhap(int maNguoiDung, int maNhom, int? maHoSo)
        {
            if (maNguoiDung <= 0 || maNhom <= 0)
                return ServiceResultDTO.Fail("Dữ liệu không hợp lệ.");
            if (_teamDal.NguoiDungDangThuocDoiKhac(maNguoiDung))
                return ServiceResultDTO.Fail("Bạn đang thuộc đội khác, hãy rời đội trước khi gửi đơn.");
            try
            {
                int maDon = _teamDal.TaoXinGiaNhap(maNguoiDung, maNhom, maHoSo);
                return ServiceResultDTO.Ok("Đã gửi đơn xin gia nhập.", new { maDon });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("UQ_XINGIANHAP"))
                    return ServiceResultDTO.Fail("Bạn đã gửi đơn xin gia nhập nhóm này rồi.");
                return ServiceResultDTO.Fail("Không thể gửi đơn: " + ex.Message);
            }
        }

        // Duyệt/từ chối đơn xin gia nhập
        public ServiceResultDTO DuyetXinGiaNhap(int maNguoiDuyet, int maDonXin, bool chapNhan)
        {
            var don = _teamDal.LayXinGiaNhap(maDonXin);
            if (don == null) return ServiceResultDTO.Fail("Không tìm thấy đơn xin.");
            if (don["trang_thai"].ToString() != "cho_duyet")
                return ServiceResultDTO.Fail("Đơn này đã được xử lý.");

            int maNhom = Convert.ToInt32(don["ma_nhom"]);
            int maNguoiXin = Convert.ToInt32(don["ma_nguoi_dung"]);

            if (!_teamDal.CoQuyenQuanLyNhom(maNguoiDuyet, maNhom))
                return ServiceResultDTO.Fail("Bạn không có quyền duyệt đơn cho nhóm này.");

            if (!chapNhan)
            {
                _teamDal.CapNhatXinGiaNhap(maDonXin, "tu_choi");
                _teamDal.TaoThongBao(maNguoiXin, "Đơn xin gia nhập bị từ chối",
                    "Đơn xin gia nhập nhóm của bạn đã bị từ chối.", "doi", "xin_gia_nhap", maDonXin);
                return ServiceResultDTO.Ok("Đã từ chối đơn.");
            }

            if (_teamDal.NguoiDungDangThuocDoiKhac(maNguoiXin))
                return ServiceResultDTO.Fail("Người này đã thuộc đội khác.");

            _teamDal.CapNhatXinGiaNhap(maDonXin, "chap_nhan");
            _recruitDal.TiepNhanUngVienVaoNhom(maNhom, maNguoiXin);
            _teamDal.TaoThongBao(maNguoiXin, "Chúc mừng!",
                "Đơn xin gia nhập nhóm của bạn đã được duyệt.", "doi", "xin_gia_nhap", maDonXin);
            return ServiceResultDTO.Ok("Đã duyệt và thêm thành viên.");
        }

        // Danh sách đơn xin
        public ServiceResultDTO LayDanhSachXinGiaNhap(int maNguoiDung, int maNhom)
        {
            if (!_teamDal.CoQuyenQuanLyNhom(maNguoiDung, maNhom))
                return ServiceResultDTO.Fail("Bạn không có quyền xem đơn xin.");
            DataTable dt = _teamDal.LayDanhSachXinGiaNhap(maNhom);
            return ServiceResultDTO.Ok("OK", DataTableToList(dt));
        }

        // Gửi lời mời gia nhập
        public ServiceResultDTO GuiLoiMoi(int maNguoiGui, int maDoi, int maNhom, string tenNguoiNhan)
        {
            if (!_teamDal.CoQuyenQuanLyNhom(maNguoiGui, maNhom))
                return ServiceResultDTO.Fail("Bạn không có quyền gửi lời mời.");
            var user = _teamDal.TimNguoiDung(tenNguoiNhan);
            if (user == null) return ServiceResultDTO.Fail("Không tìm thấy người dùng.");
            int maNguoiNhan = Convert.ToInt32(user["ma_nguoi_dung"]);
            if (maNguoiNhan == maNguoiGui) return ServiceResultDTO.Fail("Không thể mời chính mình.");
            if (_teamDal.NguoiDungDangThuocDoiKhac(maNguoiNhan))
                return ServiceResultDTO.Fail("Người này đã thuộc đội khác.");

            try
            {
                int maLoiMoi = _recruitDal.TaoLoiMoi(maDoi, maNhom, maNguoiNhan);
                _teamDal.TaoThongBao(maNguoiNhan, "Bạn nhận được lời mời gia nhập đội",
                    "Bạn được mời gia nhập nhóm thi đấu. Kiểm tra mục Thông báo để phản hồi.", "doi", "loi_moi", maLoiMoi);
                return ServiceResultDTO.Ok("Đã gửi lời mời.", new { maLoiMoi });
            }
            catch (Exception ex)
            {
                return ServiceResultDTO.Fail("Không thể gửi lời mời: " + ex.Message);
            }
        }

        // RBAC: Cập nhật vai trò thành viên
        public ServiceResultDTO CapNhatVaiTro(int maNguoiThucHien, int maThanhVien, string vaiTroMoi)
        {
            string[] vaiTroHopLe = { "coach", "captain", "member" };
            if (Array.IndexOf(vaiTroHopLe, vaiTroMoi) < 0)
                return ServiceResultDTO.Fail("Vai trò không hợp lệ. Chỉ cho phép: coach, captain, member.");

            var tv = _teamDal.LayThanhVien(maThanhVien);
            if (tv == null) return ServiceResultDTO.Fail("Thành viên không tồn tại.");
            int maDoi = Convert.ToInt32(tv["ma_doi"]);
            int maNguoiDuocDoi = Convert.ToInt32(tv["ma_nguoi_dung"]);
            string vaiTroCu = tv["vai_tro_noi_bo"].ToString();

            if (vaiTroCu == "leader")
                return ServiceResultDTO.Fail("Không thể thay đổi vai trò của Chairman.");
            if (maNguoiThucHien == maNguoiDuocDoi)
                return ServiceResultDTO.Fail("Không thể tự thay đổi vai trò của mình.");
            if (!_teamDal.LaChairman(maNguoiThucHien, maDoi))
                return ServiceResultDTO.Fail("Chỉ Chairman mới có quyền phân vai trò.");

            _teamDal.CapNhatVaiTro(maThanhVien, vaiTroMoi);
            _teamDal.TaoThongBao(maNguoiDuocDoi, "Vai trò thay đổi",
                "Vai trò của bạn trong đội đã được cập nhật thành: " + vaiTroMoi, "doi", "doi", maDoi);
            return ServiceResultDTO.Ok("Cập nhật vai trò thành công.");
        }

        // Toggle tuyển dụng
        public ServiceResultDTO ToggleDangTuyen(int maNguoiDung, int maDoi, bool dangTuyen)
        {
            if (!_teamDal.LaChairman(maNguoiDung, maDoi))
                return ServiceResultDTO.Fail("Chỉ Chairman mới có quyền.");
            _teamDal.CapNhatDangTuyen(maDoi, dangTuyen);
            return ServiceResultDTO.Ok(dangTuyen ? "Đã bật tuyển dụng." : "Đã tắt tuyển dụng.");
        }

        // Thông báo
        public ServiceResultDTO LayThongBao(int maNguoiDung)
        {
            DataTable dt = _teamDal.LayThongBao(maNguoiDung);
            int chuaDoc = _teamDal.DemThongBaoChuaDoc(maNguoiDung);
            return ServiceResultDTO.Ok("OK", new { chua_doc = chuaDoc, danh_sach = DataTableToList(dt) });
        }

        public ServiceResultDTO DanhDauDaDoc(int maNguoiDung, int maThongBao)
        {
            _teamDal.DanhDauDaDoc(maThongBao, maNguoiDung);
            return ServiceResultDTO.Ok("OK");
        }

        public ServiceResultDTO DanhDauTatCaDaDoc(int maNguoiDung)
        {
            _teamDal.DanhDauTatCaDaDoc(maNguoiDung);
            return ServiceResultDTO.Ok("OK");
        }

        // Tìm kiếm
        public ServiceResultDTO TimKiem(string tuKhoa)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa)) return ServiceResultDTO.Ok("OK", new List<object>());
            DataTable dt = _teamDal.TimKiemToanCuc(tuKhoa);
            return ServiceResultDTO.Ok("OK", DataTableToList(dt));
        }

        // Dashboard sections
        public ServiceResultDTO LayGiaiNoiBat() => ServiceResultDTO.Ok("OK", DataTableToList(_teamDal.LayGiaiNoiBat(12)));
        public ServiceResultDTO LayGiaiSapBatDau() => ServiceResultDTO.Ok("OK", DataTableToList(_teamDal.LayGiaiSapBatDau(12)));
        public ServiceResultDTO LayGiaiDangMoDangKy() => ServiceResultDTO.Ok("OK", DataTableToList(_teamDal.LayGiaiDangMoDangKy(12)));
        public ServiceResultDTO LayGiaiTheoGame(int maTroChoi, string trangThai) => ServiceResultDTO.Ok("OK", DataTableToList(_teamDal.LayGiaiTheoGame(maTroChoi, trangThai)));
        public ServiceResultDTO LayGiaiDaThamGia(int maNguoiDung) => ServiceResultDTO.Ok("OK", DataTableToList(_teamDal.LayGiaiDaThamGia(maNguoiDung)));

        // Giải đang theo dõi
        public ServiceResultDTO LayGiaiDangTheoDoi(int maNguoiDung)
        {
            var dal = new TuongTacDAL();
            return ServiceResultDTO.Ok("OK", dal.LayGiaiDangTheoDoi(maNguoiDung));
        }

        // Helper
        private static List<Dictionary<string, object>> DataTableToList(DataTable dt)
        {
            var list = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var d = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                    d[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                list.Add(d);
            }
            return list;
        }
    }
}
