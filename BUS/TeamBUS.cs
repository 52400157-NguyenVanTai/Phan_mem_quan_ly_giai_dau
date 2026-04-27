using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Script.Serialization;
using DAL;
using DTO;

namespace BUS
{
    public class TeamBUS
    {
        private readonly TeamDAL _teamDal = new TeamDAL();
        private readonly RecruitmentDAL _recruitDal = new RecruitmentDAL();

        public ServiceResultDTO TaoDoiVaNhieuNhom(TaoDoiDTO doi, string squadsJson)
        {
            if (doi == null || doi.MaNguoiDungTao <= 0 || string.IsNullOrWhiteSpace(doi.TenDoi))
            {
                return ServiceResultDTO.Fail("Dữ liệu đội không hợp lệ.");
            }

            if (_teamDal.TenDoiTonTai(doi.TenDoi))
            {
                return ServiceResultDTO.Fail("Tên đội đã tồn tại trên hệ thống.");
            }

            if (_teamDal.NguoiDungDangThuocDoiKhac(doi.MaNguoiDungTao))
            {
                return ServiceResultDTO.Fail("Bạn đang thuộc đội khác, không thể tạo đội mới.");
            }

            List<dynamic> squads;
            try
            {
                var serializer = new JavaScriptSerializer();
                squads = serializer.Deserialize<List<dynamic>>(squadsJson);
            }
            catch
            {
                return ServiceResultDTO.Fail("Dữ liệu nhóm không hợp lệ.");
            }

            if (squads == null || squads.Count == 0)
            {
                return ServiceResultDTO.Fail("Khi tạo đội bắt buộc phải có ít nhất 1 nhóm hợp lệ.");
            }

            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    int maDoi = _teamDal.TaoDoi(doi.MaNguoiDungTao, doi.TenDoi, doi.TenVietTat, doi.LogoUrl, doi.Slogan, conn, tran);

                    // Create management group for team and get its ma_nhom
                    int maNhomQuanLy = _teamDal.TaoNhomQuanLy(maDoi, conn, tran);

                    // Add creator to management group as Chủ tịch (nhóm quản lý không có đội trưởng)
                    _teamDal.ThemThanhVienVaoNhomQuanLy(doi.MaNguoiDungTao, maNhomQuanLy, "chu_tich", conn, tran);

                    var squadIds = new List<object>();
                    var gameCountMap = new Dictionary<int, int>();
                    foreach (var sq in squads)
                    {
                        int maTroChoi = Convert.ToInt32(sq["maTroChoi"]);
                        string tenNhom = sq["tenNhom"].ToString();
                        if (maTroChoi <= 0 || string.IsNullOrWhiteSpace(tenNhom))
                        {
                            tran.Rollback();
                            return ServiceResultDTO.Fail("Dữ liệu nhóm không hợp lệ.");
                        }

                        // Kiểm tra giới hạn: mỗi game tối đa 2 nhóm
                        if (!gameCountMap.ContainsKey(maTroChoi))
                            gameCountMap[maTroChoi] = 0;
                        gameCountMap[maTroChoi]++;
                        if (gameCountMap[maTroChoi] > 2)
                        {
                            tran.Rollback();
                            return ServiceResultDTO.Fail($"Mỗi game chỉ có tối đa 2 nhóm per đội. Game {maTroChoi} đã vượt quá giới hạn.");
                        }

                        int maNhom = _teamDal.TaoNhom(maDoi, maTroChoi, tenNhom, doi.MaNguoiDungTao, conn, tran);
                        squadIds.Add(new { maNhom });
                    }

                    // Kiểm tra: đội phải có ít nhất 1 nhóm game
                    if (gameCountMap.Count == 0)
                    {
                        tran.Rollback();
                        return ServiceResultDTO.Fail("Đội phải có ít nhất 1 nhóm game.");
                    }

                    tran.Commit();
                    return ServiceResultDTO.Ok("Tạo đội và các nhóm thành công.", new { maDoi, squads = squadIds });
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
            if (maCaptain <= 0) maCaptain = maNguoiTao;

            if (maNguoiTao <= 0 || maDoi <= 0 || string.IsNullOrWhiteSpace(tenNhom))
            {
                return ServiceResultDTO.Fail("Dữ liệu tạo nhóm không hợp lệ.");
            }

            // Kiểm tra điều kiện nhóm Ban điều hành: mỗi đội chỉ có đúng 1 nhóm Ban điều hành
            if (maTroChoi == 0)
            {
                if (_teamDal.CoNhomBanDieuHanh(maDoi))
                    return ServiceResultDTO.Fail("Đội đã có nhóm Ban điều hành. Mỗi đội chỉ có 1 nhóm Ban điều hành.");
            }
            // Kiểm tra điều kiện nhóm game: tối đa 2 nhóm per game per đội
            else
            {
                if (_teamDal.DemSoNhomTheoGame(maDoi, maTroChoi) >= 2)
                    return ServiceResultDTO.Fail("Mỗi game chỉ có tối đa 2 nhóm per đội. Đã đạt giới hạn.");
            }

            if (_teamDal.DemSoNhomCuaDoi(maDoi) >= 12)
            {
                return ServiceResultDTO.Fail("Đội đã đạt tối đa 12 nhóm thi đấu.");
            }

            // Only block if captain belongs to a DIFFERENT team
            if (_teamDal.NguoiDungDangThuocDoiKhac(maCaptain) && !_teamDal.LaChuTich(maCaptain, maDoi))
            {
                // Check if captain is already a member of THIS team
                DataTable dtCap = _teamDal.LayTatCaDoiCuaToi(maCaptain);
                bool thuocDoiNay = false;
                foreach (DataRow r in dtCap.Rows)
                    if (Convert.ToInt32(r["ma_doi"]) == maDoi) { thuocDoiNay = true; break; }
                if (!thuocDoiNay)
                    return ServiceResultDTO.Fail("Đội trưởng đang thuộc đội khác, không thể chỉ định.");
            }

            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    int maNhom = _teamDal.TaoNhom(maDoi, maTroChoi, tenNhom, maCaptain, conn, tran);
                    // Nhóm được tạo rỗng, không tự động thêm người tạo vào nhóm
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
            if (maNguoiDung <= 0 || maNhom <= 0)
                return ServiceResultDTO.Fail("Dữ liệu không hợp lệ.");

            // Kiểm tra quyền: chỉ Ban điều hành/Đội trưởng mới được thêm thành viên
            bool duocPhep = _teamDal.KiemTraRoleNoiBoTrongNhom(maNguoiThucHien, maNhom, "ban_dieu_hanh")
                || _teamDal.KiemTraRoleNoiBoTrongNhom(maNguoiThucHien, maNhom, "doi_truong");
            if (!duocPhep)
                return ServiceResultDTO.Fail("Chỉ Ban điều hành hoặc Đội trưởng mới có quyền thêm thành viên vào nhóm.");

            // Lấy thông tin nhóm
            var nhomInfo = _teamDal.LayThongTinNhom(maNhom);
            if (nhomInfo == null)
                return ServiceResultDTO.Fail("Nhóm không tồn tại.");

            int maDoi = Convert.ToInt32(nhomInfo["ma_doi"]);
            int maTroChoi = nhomInfo["ma_tro_choi"] != DBNull.Value ? Convert.ToInt32(nhomInfo["ma_tro_choi"]) : 0;
            bool isNhomGame = maTroChoi > 0;
            bool isNhomBanDieuHanh = !isNhomGame;

            // Kiểm tra người dùng có trong đội chưa
            if (_teamDal.NguoiDungDangThuocDoiKhac(maNguoiDung))
                return ServiceResultDTO.Fail("Người dùng đang thuộc đội khác.");

            // Lấy vai trò của người dùng trong đội
            var vaiTro = _teamDal.LayVaiTroNguoiDungTrongDoi(maNguoiDung, maDoi);

            // Nếu chưa trong đội, thêm vào đội trước
            if (vaiTro == null || vaiTro == "")
            {
                // Lấy nhóm quản lý của đội
                int maNhomQuanLy = _teamDal.LayNhomQuanLy(maDoi);
                if (maNhomQuanLy <= 0)
                    return ServiceResultDTO.Fail("Không tìm thấy nhóm quản lý của đội.");

                using (SqlConnection conn = DataProvider.CreateConnection())
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            _teamDal.ThemThanhVien(maNguoiDung, maNhomQuanLy, "thanh_vien", "ban_huan_luyen", null, conn, tran);
                            tran.Commit();
                        }
                        catch
                        {
                            tran.Rollback();
                            return ServiceResultDTO.Fail("Không thể thêm người dùng vào đội.");
                        }
                    }
                }
                vaiTro = "thanh_vien";
            }

            // Kiểm tra hạn chế tham gia nhóm game
            if (isNhomGame)
            {
                // Chủ tịch và Ban điều hành có thể tham gia 2 nhóm (ban điều hành + 1 nhóm game)
                // Thành viên chỉ được tham gia 1 nhóm game
                if (vaiTro != "chu_tich" && vaiTro != "ban_dieu_hanh")
                {
                    // Kiểm tra thành viên đã tham gia nhóm game nào chưa
                    if (_teamDal.DemSoNhomGameCuaNguoiDung(maNguoiDung, maDoi) >= 1)
                        return ServiceResultDTO.Fail("Thành viên chỉ được tham gia 1 nhóm game. Vui lòng rời nhóm hiện tại trước.");
                }
                else
                {
                    // Chủ tịch/Ban điều hành: kiểm tra đã tham gia 1 nhóm game chưa
                    if (_teamDal.DemSoNhomGameCuaNguoiDung(maNguoiDung, maDoi) >= 1)
                        return ServiceResultDTO.Fail("Chủ tịch/Ban điều hành chỉ được tham gia 1 nhóm game bên cạnh nhóm Ban điều hành.");
                }
            }

            // Thêm vào nhóm cụ thể
            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        _teamDal.ThemThanhVien(maNguoiDung, maNhom, "thanh_vien", phanHe == "ban_huan_luyen" ? "ban_huan_luyen" : "thi_dau", maViTri > 0 ? (int?)maViTri : null, conn, tran);
                        tran.Commit();
                        return ServiceResultDTO.Ok("Đã thêm thành viên vào nhóm.");
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        return ServiceResultDTO.Fail("Không thể thêm thành viên vào nhóm: " + ex.Message);
                    }
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

        public ServiceResultDTO XoaNhom(int maNguoiThucHien, int maDoi, int maNhom)
        {
            if (maNguoiThucHien <= 0 || maDoi <= 0 || maNhom <= 0)
                return ServiceResultDTO.Fail("Dữ liệu xóa nhóm không hợp lệ.");

            if (!_teamDal.LaChuTich(maNguoiThucHien, maDoi))
                return ServiceResultDTO.Fail("Chỉ Chủ tịch mới có quyền xóa nhóm.");

            if (!_teamDal.NhomThuocDoi(maNhom, maDoi))
                return ServiceResultDTO.Fail("Nhóm không thuộc đội này.");

            if (_teamDal.DemSoNhomCuaDoi(maDoi) <= 1)
                return ServiceResultDTO.Fail("Đội phải còn ít nhất 1 nhóm. Nếu muốn xóa toàn bộ, hãy dùng xóa đội.");

            if (_teamDal.NhomChuaChuTich(maNhom))
                return ServiceResultDTO.Fail("Không thể xóa nhóm đang chứa Chủ tịch. Hãy chuyển Chủ tịch sang nhóm khác trước.");

            if (_teamDal.NhomCoDuLieuThamGiaGiai(maNhom))
                return ServiceResultDTO.Fail("Nhóm đã phát sinh dữ liệu giải đấu/trận đấu, không thể xóa.");

            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    _teamDal.XoaNhom(maNhom, conn, tran);
                    tran.Commit();
                    return ServiceResultDTO.Ok("Xóa nhóm thành công.");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return ServiceResultDTO.Fail("Không thể xóa nhóm: " + ex.Message);
                }
            }
        }

        public ServiceResultDTO XoaDoi(int maNguoiThucHien, int maDoi)
        {
            if (maNguoiThucHien <= 0 || maDoi <= 0)
                return ServiceResultDTO.Fail("Dữ liệu xóa đội không hợp lệ.");

            if (!_teamDal.LaChuTich(maNguoiThucHien, maDoi))
                return ServiceResultDTO.Fail("Chỉ Chủ tịch mới có quyền xóa đội.");

            DataTable dtNhom = _teamDal.LayDanhSachNhom(maDoi);
            foreach (DataRow row in dtNhom.Rows)
            {
                int maNhom = Convert.ToInt32(row["ma_nhom"]);
                if (_teamDal.NhomCoDuLieuThamGiaGiai(maNhom))
                    return ServiceResultDTO.Fail("Đội có nhóm đã phát sinh dữ liệu giải đấu/trận đấu, không thể xóa.");
            }

            try
            {
                bool ok = _teamDal.XoaDoiVinhVien(maDoi, maNguoiThucHien);
                return ok
                    ? ServiceResultDTO.Ok("Xóa đội thành công.")
                    : ServiceResultDTO.Fail("Không thể xóa đội.");
            }
            catch (Exception ex)
            {
                return ServiceResultDTO.Fail("Không thể xóa đội: " + ex.Message);
            }
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
        public ServiceResultDTO LayChiTietDoi(int maDoi, int maNguoiDung = 0)
        {
            DataTable dt = _teamDal.LayChiTietDoi(maDoi);
            if (dt.Rows.Count == 0) return ServiceResultDTO.Fail("Đội không tồn tại.");
            var row = dt.Rows[0];
            var squads = DataTableToList(_teamDal.LayDanhSachNhom(maDoi));

            // Determine current user role
            string vaiTroHienTai = null;
            if (maNguoiDung > 0)
            {
                int maDoiTruong = Convert.ToInt32(row["ma_doi_truong"]);
                if (maDoiTruong == maNguoiDung)
                {
                    vaiTroHienTai = "chu_tich";  // User is the team owner/chủ tịch
                }
                else
                {
                    // Check if user is member of any squad in this team
                    DataTable dtAll = _teamDal.LayTatCaDoiCuaToi(maNguoiDung);
                    foreach (DataRow r in dtAll.Rows)
                    {
                        if (Convert.ToInt32(r["ma_doi"]) == maDoi)
                        {
                            vaiTroHienTai = r["vai_tro_noi_bo"].ToString();
                            break;
                        }
                    }
                }
            }

            // Count total members directly from THANH_VIEN_DOI to avoid duplicates
            DataTable dtMembers = _teamDal.LaySoThanhVienDoi(maDoi);
            int soThanhVien = dtMembers.Rows.Count > 0 ? Convert.ToInt32(dtMembers.Rows[0]["so_thanh_vien"]) : 0;

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
                ngay_tao = row["ngay_tao"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ngay_tao"]),
                so_thanh_vien = soThanhVien,
                vai_tro_hien_tai = vaiTroHienTai,
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
        public ServiceResultDTO LayThanhVienNhomQuanLy(int maNguoiDung, int maDoi)
        {
            if (!_teamDal.CoQuyenQuanLyDoi(maNguoiDung, maDoi))
                return ServiceResultDTO.Fail("Bạn không có quyền xem danh sách thành viên.");
            DataTable dt = _teamDal.LayThanhVienNhomQuanLy(maDoi);
            return ServiceResultDTO.Ok("OK", DataTableToList(dt));
        }

        public ServiceResultDTO GuiYeuCauThamGiaNhom(int maNguoiDung, int maNhom, int maHoSo)
        {
            // Check if user has game profile for the squad's game
            var squadInfo = _teamDal.LayThongTinNhom(maNhom);
            if (squadInfo == null)
                return ServiceResultDTO.Fail("Nhóm không tồn tại.");
            
            int maTroChoi = Convert.ToInt32(squadInfo["ma_tro_choi"]);
            if (!_teamDal.NguoiChoiCoHoSoDungGame(maNguoiDung, maTroChoi))
                return ServiceResultDTO.Fail("Bạn chưa có hồ sơ thi đấu cho game này. Vui lòng tạo hồ sơ trước.");

            // Check if user is already in the team
            if (_teamDal.NguoiDungDangThuocDoiKhac(maNguoiDung))
                return ServiceResultDTO.Fail("Bạn đang thuộc đội khác.");

            try
            {
                int maYeuCau = _teamDal.TaoYeuCauThamGiaNhom(maNguoiDung, maNhom, maHoSo);
                // Notify squad Đội trưởng
                var captain = _teamDal.LayDoiTruongNhom(maNhom);
                if (captain != null)
                {
                    int maCaptain = Convert.ToInt32(captain["ma_nguoi_dung"]);
                    _teamDal.TaoThongBao(maCaptain, "Yêu cầu tham gia nhóm",
                        "Có thành viên muốn tham gia nhóm của bạn. Kiểm tra để duyệt.", "doi", "yeu_cau_tham_gia", maYeuCau);
                }
                return ServiceResultDTO.Ok("Đã gửi yêu cầu tham gia nhóm.", new { maYeuCau });
            }
            catch (Exception ex)
            {
                return ServiceResultDTO.Fail("Không thể gửi yêu cầu: " + ex.Message);
            }
        }

        public ServiceResultDTO LayYeuCauThamGiaNhom(int maNguoiDung, int maNhom)
        {
            if (!_teamDal.CoQuyenQuanLyNhom(maNguoiDung, maNhom))
                return ServiceResultDTO.Fail("Bạn không có quyền xem yêu cầu tham gia.");
            DataTable dt = _teamDal.LayYeuCauThamGiaNhom(maNhom);
            return ServiceResultDTO.Ok("OK", DataTableToList(dt));
        }

        public ServiceResultDTO CapNhatDoiTruongNhom(int maNguoiDung, int maNhom, int maDoiTruongMoi)
        {
            // Check if user is Chủ tịch of the team by checking their role in the squad
            var squadMembers = _teamDal.LayThanhVienNhom(maNhom);
            var chuTichMember = squadMembers.AsEnumerable()
                .FirstOrDefault(row => row["ma_nguoi_dung"].ToString() == maNguoiDung.ToString());

            if (chuTichMember == null || chuTichMember["vai_tro_noi_bo"].ToString() != "chu_tich")
                return ServiceResultDTO.Fail("Chỉ Chủ tịch mới có thể thay đổi Đội trưởng.");

            // Check if new captain is a member of the squad
            var isMember = squadMembers.AsEnumerable()
                .Any(row => row["ma_nguoi_dung"].ToString() == maDoiTruongMoi.ToString());

            if (!isMember)
                return ServiceResultDTO.Fail("Người được chọn không phải thành viên của nhóm.");

            bool success = _teamDal.CapNhatDoiTruongNhom(maNhom, maDoiTruongMoi);
            if (success)
                return ServiceResultDTO.Ok("Đã cập nhật Đội trưởng thành công.");
            else
                return ServiceResultDTO.Fail("Cập nhật Đội trưởng thất bại.");
        }

        public ServiceResultDTO CapNhatThongTinDoi(int maNguoiDung, int maDoi, string tenDoi, string tenVietTat, string logoUrl, string slogan)
        {
            // Check if user is Chủ tịch of the team
            DataTable team = _teamDal.LayChiTietDoi(maDoi);
            if (team == null || team.Rows.Count == 0)
                return ServiceResultDTO.Fail("Đội không tồn tại.");

            var vaiTro = team.Rows[0]["vai_tro_hien_tai"]?.ToString();
            if (vaiTro != "chu_tich")
                return ServiceResultDTO.Fail("Chỉ Chủ tịch mới có thể thay đổi thông tin đội.");

            bool success = _teamDal.CapNhatThongTinDoi(maDoi, tenDoi, tenVietTat, logoUrl, slogan);
            if (success)
                return ServiceResultDTO.Ok("Đã cập nhật thông tin đội thành công.");
            else
                return ServiceResultDTO.Fail("Cập nhật thông tin đội thất bại.");
        }

        public ServiceResultDTO CapNhatDangTuyen(int maNguoiDung, int maDoi, bool dangTuyen)
        {
            // Check if user is Chủ tịch or Ban điều hành of the team
            DataTable team = _teamDal.LayChiTietDoi(maDoi);
            if (team == null || team.Rows.Count == 0)
                return ServiceResultDTO.Fail("Đội không tồn tại.");

            // Get user's team info to check role
            var userTeam = LayDoiCuaToi(maNguoiDung);
            if (!userTeam.Success || userTeam.Data == null)
                return ServiceResultDTO.Fail("Bạn không thuộc đội này.");

            var userTeamData = (dynamic)userTeam.Data;
            if (userTeamData.ma_doi != maDoi)
                return ServiceResultDTO.Fail("Bạn không thuộc đội này.");

            var vaiTro = userTeamData.vai_tro?.ToString();
            if (vaiTro != "chu_tich" && vaiTro != "ban_dieu_hanh")
                return ServiceResultDTO.Fail("Chỉ Chủ tịch hoặc Ban điều hành mới có thể thay đổi trạng thái tuyển dụng.");

            bool success = _teamDal.CapNhatDangTuyen(maDoi, dangTuyen);
            if (success)
                return ServiceResultDTO.Ok(dangTuyen ? "Đã bật tuyển dụng." : "Đã tắt tuyển dụng.");
            else
                return ServiceResultDTO.Fail("Cập nhật trạng thái tuyển dụng thất bại.");
        }

        public ServiceResultDTO CapNhatLogo(int maNguoiDung, int maDoi, string logoUrl)
        {
            // Check if user is Chủ tịch of the team
            var userTeam = LayDoiCuaToi(maNguoiDung);
            if (!userTeam.Success || userTeam.Data == null)
                return ServiceResultDTO.Fail("Bạn không thuộc đội này.");

            var userTeamData = (dynamic)userTeam.Data;
            if (userTeamData.ma_doi != maDoi)
                return ServiceResultDTO.Fail("Bạn không thuộc đội này.");

            var vaiTro = userTeamData.vai_tro?.ToString();
            if (vaiTro != "chu_tich")
                return ServiceResultDTO.Fail("Chỉ Chủ tịch mới có thể thay đổi logo đội.");

            bool success = _teamDal.CapNhatLogo(maDoi, logoUrl);
            if (success)
                return ServiceResultDTO.Ok("Đã cập nhật logo đội thành công.");
            else
                return ServiceResultDTO.Fail("Cập nhật logo đội thất bại.");
        }

        public ServiceResultDTO DuyetYeuCauThamGiaNhom(int maNguoiDung, int maYeuCau, bool chapNhan)
        {
            var yeuCau = _teamDal.LayYeuCauThamGiaNhom(maYeuCau);
            if (yeuCau == null || yeuCau.Rows.Count == 0)
                return ServiceResultDTO.Fail("Không tìm thấy yêu cầu.");

            int maNhom = Convert.ToInt32(yeuCau.Rows[0]["ma_nhom"]);
            if (!_teamDal.CoQuyenQuanLyNhom(maNguoiDung, maNhom))
                return ServiceResultDTO.Fail("Bạn không có quyền duyệt yêu cầu này.");

            string trangThai = chapNhan ? "chap_nhan" : "tu_choi";
            _teamDal.DuyetYeuCauThamGiaNhom(maYeuCau, trangThai, maNguoiDung);

            if (chapNhan)
            {
                int maNguoiXin = Convert.ToInt32(yeuCau.Rows[0]["ma_nguoi_dung"]);
                int maHoSo = yeuCau.Rows[0]["ma_ho_so"] != DBNull.Value ? Convert.ToInt32(yeuCau.Rows[0]["ma_ho_so"]) : 0;
                if (maHoSo > 0)
                    _recruitDal.TiepNhanUngVienVaoNhom(maNhom, maNguoiXin);
                _teamDal.TaoThongBao(maNguoiXin, "Chúc mừng!", "Yêu cầu tham gia nhóm của bạn đã được duyệt.", "doi", "yeu_cau_tham_gia", maYeuCau);
            }

            return ServiceResultDTO.Ok("Đã xử lý yêu cầu thành công.");
        }

        public ServiceResultDTO LayDanhSachXinGiaNhap(int maNguoiDung, int maNhom)
        {
            if (!_teamDal.CoQuyenQuanLyNhom(maNguoiDung, maNhom))
                return ServiceResultDTO.Fail("Bạn không có quyền xem đơn xin.");
            DataTable dt = _teamDal.LayDanhSachXinGiaNhap(maNhom);
            return ServiceResultDTO.Ok("OK", DataTableToList(dt));
        }

        // Kích thành viên khỏi nhóm
        public ServiceResultDTO KichThanhVienKhoiNhom(int maNguoiThucHien, int maNguoiBiKich, int maNhom)
        {
            // Lấy thông tin nhóm
            var nhomInfo = _teamDal.LayThongTinNhom(maNhom);
            if (nhomInfo == null)
                return ServiceResultDTO.Fail("Nhóm không tồn tại.");
            
            int maDoi = Convert.ToInt32(nhomInfo["ma_doi"]);
            
            // Kiểm tra quyền của người thực hiện
            var vaiTroThucHien = _teamDal.LayVaiTroNguoiDungTrongDoi(maNguoiThucHien, maDoi);
            if (vaiTroThucHien != "chu_tich" && vaiTroThucHien != "ban_dieu_hanh")
                return ServiceResultDTO.Fail("Chỉ Chủ tịch hoặc Ban điều hành mới có quyền kích thành viên.");
            
            // Lấy vai trò của người bị kích
            var vaiTroBiKich = _teamDal.LayVaiTroNguoiDungTrongDoi(maNguoiBiKich, maDoi);
            
            // Chủ tịch không thể bị kích
            if (vaiTroBiKich == "chu_tich")
                return ServiceResultDTO.Fail("Không thể kích Chủ tịch.");
            
            // Ban điều hành không thể kích lẫn nhau, chỉ có chủ tịch mới được kích ban điều hành
            if (vaiTroBiKich == "ban_dieu_hanh" && vaiTroThucHien != "chu_tich")
                return ServiceResultDTO.Fail("Chỉ Chủ tịch mới có quyền kích Ban điều hành.");
            
            // Kiểm tra người bị kích có trong nhóm không
            var thanhVienNhom = _teamDal.LayThanhVienNhom(maNhom);
            var thanhVien = thanhVienNhom.AsEnumerable()
                .FirstOrDefault(row => Convert.ToInt32(row["ma_nguoi_dung"]) == maNguoiBiKich);
            
            if (thanhVien == null)
                return ServiceResultDTO.Fail("Thành viên không thuộc nhóm này.");
            
            // Thực hiện kích
            bool success = _teamDal.KichThanhVienKhoiNhom(maNguoiBiKich, maNhom, maNguoiThucHien);
            if (!success)
                return ServiceResultDTO.Fail("Kích thành viên thất bại.");
            
            // Thông báo cho người bị kích
            _teamDal.TaoThongBao(maNguoiBiKich, "Bạn đã bị kích khỏi nhóm",
                "Bạn đã bị kích khỏi nhóm bởi quản lý đội.", "doi", "kich", maNhom);
            
            return ServiceResultDTO.Ok("Đã kích thành viên khỏi nhóm.");
        }

        // Rời nhóm (tự nguyện)
        public ServiceResultDTO RoiNhom(int maNguoiDung, int maNhom)
        {
            // Kiểm tra người dùng có trong nhóm không
            var thanhVienNhom = _teamDal.LayThanhVienNhom(maNhom);
            var thanhVien = thanhVienNhom.AsEnumerable()
                .FirstOrDefault(row => Convert.ToInt32(row["ma_nguoi_dung"]) == maNguoiDung);
            
            if (thanhVien == null)
                return ServiceResultDTO.Fail("Bạn không thuộc nhóm này.");
            
            // Chủ tịch không thể rời nhóm ban điều hành
            var vaiTro = thanhVien["vai_tro_noi_bo"].ToString();
            var nhomInfo = _teamDal.LayThongTinNhom(maNhom);
            bool isNhomBanDieuHanh = nhomInfo["ma_tro_choi"] == DBNull.Value;
            
            if (vaiTro == "chu_tich" && isNhomBanDieuHanh)
                return ServiceResultDTO.Fail("Chủ tịch không thể rời nhóm Ban điều hành.");
            
            // Thực hiện rời nhóm
            bool success = _teamDal.RoiNhom(maNguoiDung, maNhom);
            if (!success)
                return ServiceResultDTO.Fail("Rời nhóm thất bại.");
            
            return ServiceResultDTO.Ok("Đã rời nhóm thành công.");
        }

        // Rời đội (tự nguyện)
        public ServiceResultDTO RoiDoi(int maNguoiDung, int maDoi)
        {
            // Kiểm tra người dùng có trong đội không
            var vaiTro = _teamDal.LayVaiTroNguoiDungTrongDoi(maNguoiDung, maDoi);
            if (vaiTro == null || vaiTro == "")
                return ServiceResultDTO.Fail("Bạn không thuộc đội này.");
            
            // Chủ tịch không thể rời đội
            if (vaiTro == "chu_tich")
                return ServiceResultDTO.Fail("Chủ tịch không thể rời đội. Vui lòng giải tán đội hoặc chuyển quyền Chủ tịch.");
            
            // Thực hiện rời đội
            bool success = _teamDal.RoiDoi(maNguoiDung, maDoi);
            if (!success)
                return ServiceResultDTO.Fail("Rời đội thất bại.");
            
            return ServiceResultDTO.Ok("Đã rời đội thành công.");
        }

        // Xác nhận/Từ chối lời mời từ Đội trưởng (cho Chủ tịch/Ban điều hành)
        public ServiceResultDTO XacNhanLoiMoi(int maNguoiXacNhan, int maYeuCau, bool chapNhan)
        {
            var yeuCau = _teamDal.LayYeuCauXacNhanLoiMoi(maYeuCau);
            if (yeuCau == null)
                return ServiceResultDTO.Fail("Không tìm thấy yêu cầu.");

            int maDoi = Convert.ToInt32(yeuCau["ma_doi"]);
            
            // Kiểm tra quyền: chỉ Chủ tịch hoặc Ban điều hành mới được xác nhận
            var vaiTro = _teamDal.LayVaiTroNguoiDungTrongDoi(maNguoiXacNhan, maDoi);
            if (vaiTro != "chu_tich" && vaiTro != "ban_dieu_hanh")
                return ServiceResultDTO.Fail("Chỉ Chủ tịch hoặc Ban điều hành mới có quyền xác nhận.");

            int maNguoiNhan = Convert.ToInt32(yeuCau["ma_nguoi_nhan"]);
            int? maNhom = yeuCau["ma_nhom"] != DBNull.Value ? (int?)Convert.ToInt32(yeuCau["ma_nhom"]) : null;

            if (!chapNhan)
            {
                // Từ chối yêu cầu
                _teamDal.CapNhatTrangThaiYeuCauXacNhanLoiMoi(maYeuCau, "tu_choi", maNguoiXacNhan);
                
                // Thông báo cho Đội trưởng
                int maNguoiGui = Convert.ToInt32(yeuCau["ma_nguoi_gui"]);
                _teamDal.TaoThongBao(maNguoiGui, "Lời mời bị từ chối",
                    "Yêu cầu mời thành viên của bạn đã bị từ chối.", "doi", "yeu_cau_loi_moi", maYeuCau);
                
                return ServiceResultDTO.Ok("Đã từ chối yêu cầu.");
            }

            // Chấp nhận - gửi lời mời thực sự
            try
            {
                int maNguoiGui = Convert.ToInt32(yeuCau["ma_nguoi_gui"]);
                int maLoiMoi = _recruitDal.TaoLoiMoi(maDoi, maNhom, maNguoiNhan, maNguoiGui);
                
                // Cập nhật trạng thái yêu cầu
                _teamDal.CapNhatTrangThaiYeuCauXacNhanLoiMoi(maYeuCau, "da_xac_nhan", maNguoiXacNhan);
                
                string message = maNhom.HasValue && maNhom.Value > 0
                    ? "Bạn được mời gia nhập một nhóm thi đấu."
                    : "Bạn được mời gia nhập đội (chưa phân vào nhóm).";
                _teamDal.TaoThongBao(maNguoiNhan, "Bạn nhận được lời mời gia nhập đội",
                    message + " Kiểm tra mục Thông báo để phản hồi.", "doi", "loi_moi", maLoiMoi);
                
                return ServiceResultDTO.Ok("Đã xác nhận và gửi lời mời.", new { maLoiMoi });
            }
            catch (Exception ex)
            {
                return ServiceResultDTO.Fail("Không thể gửi lời mời: " + ex.Message);
            }
        }

        // Gửi lời mời gia nhập
        public ServiceResultDTO GuiLoiMoi(int maNguoiGui, int maDoi, int? maNhom, string tenNguoiNhan)
        {
            // Kiểm tra quyền theo role mới
            // Chủ tịch/Ban điều hành: mời trực tiếp, phân nhóm luôn
            // Đội trưởng: gửi yêu cầu lên Chủ tịch/Ban điều hành để xác nhận trước
            var vaiTro = _teamDal.LayVaiTroNguoiDungTrongDoi(maNguoiGui, maDoi);
            if (vaiTro == null || vaiTro == "")
                return ServiceResultDTO.Fail("Bạn không thuộc đội này.");

            bool isDoiTruong = vaiTro == "doi_truong";
            bool isChuTichOrBanDieuHanh = vaiTro == "chu_tich" || vaiTro == "ban_dieu_hanh";

            if (!isChuTichOrBanDieuHanh && !isDoiTruong)
                return ServiceResultDTO.Fail("Bạn không có quyền gửi lời mời.");

            // Kiểm tra quyền quản lý nhóm (nếu mời vào nhóm cụ thể)
            if (maNhom.HasValue && maNhom.Value > 0)
            {
                // Ban điều hành không thể mời vào nhóm ban điều hành
                var nhomInfo = _teamDal.LayThongTinNhom(maNhom.Value);
                if (nhomInfo == null)
                    return ServiceResultDTO.Fail("Nhóm không tồn tại.");
                
                bool isNhomBanDieuHanh = nhomInfo["ma_tro_choi"] == DBNull.Value;
                if (vaiTro == "ban_dieu_hanh" && isNhomBanDieuHanh)
                    return ServiceResultDTO.Fail("Ban điều hành không thể mời vào nhóm Ban điều hành.");

                if (!_teamDal.CoQuyenQuanLyNhom(maNguoiGui, maNhom.Value))
                    return ServiceResultDTO.Fail("Bạn không có quyền gửi lời mời vào nhóm này.");
            }

            var user = _teamDal.TimNguoiDung(tenNguoiNhan);
            if (user == null) return ServiceResultDTO.Fail("Không tìm thấy người dùng.");
            int maNguoiNhan = Convert.ToInt32(user["ma_nguoi_dung"]);
            if (maNguoiNhan == maNguoiGui) return ServiceResultDTO.Fail("Không thể mời chính mình.");
            if (_teamDal.NguoiDungDangThuocDoiKhac(maNguoiNhan))
                return ServiceResultDTO.Fail("Người này đã thuộc đội khác.");

            // Nếu là Đội trưởng, cần gửi yêu cầu lên Chủ tịch/Ban điều hành để xác nhận
            if (isDoiTruong)
            {
                // Tạo yêu cầu xác nhận lời mời
                int maYeuCau = _teamDal.TaoYeuCauXacNhanLoiMoi(maNguoiGui, maDoi, maNhom, maNguoiNhan);
                
                // Thông báo cho Chủ tịch/Ban điều hành
                DataTable quanLyNhom = _teamDal.LayQuanLyDoi(maDoi);
                foreach (DataRow row in quanLyNhom.Rows)
                {
                    int maQuanLy = Convert.ToInt32(row["ma_nguoi_dung"]);
                    _teamDal.TaoThongBao(maQuanLy, "Yêu cầu xác nhận lời mời",
                        $"Đội trưởng muốn mời {tenNguoiNhan} vào đội. Vui lòng xác nhận.", "doi", "yeu_cau_loi_moi", maYeuCau);
                }
                
                return ServiceResultDTO.Ok("Đã gửi yêu cầu lên Chủ tịch/Ban điều hành để xác nhận.");
            }

            try
            {
                int maLoiMoi = _recruitDal.TaoLoiMoi(maDoi, maNhom.HasValue ? maNhom.Value : (int?)null, maNguoiNhan, maNguoiGui);
                string message = maNhom.HasValue && maNhom.Value > 0
                    ? "Bạn được mời gia nhập một nhóm thi đấu."
                    : "Bạn được mời gia nhập đội (chưa phân vào nhóm).";
                _teamDal.TaoThongBao(maNguoiNhan, "Bạn nhận được lời mời gia nhập đội",
                    message + " Kiểm tra mục Thông báo để phản hồi.", "doi", "loi_moi", maLoiMoi);
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
            string[] vaiTroHopLe = { "ban_dieu_hanh", "doi_truong", "thanh_vien" };
            if (Array.IndexOf(vaiTroHopLe, vaiTroMoi) < 0)
                return ServiceResultDTO.Fail("Vai trò không hợp lệ. Chỉ cho phép: Ban điều hành, Đội trưởng, Thành viên.");

            var tv = _teamDal.LayThanhVien(maThanhVien);
            if (tv == null) return ServiceResultDTO.Fail("Thành viên không tồn tại.");
            int maDoi = Convert.ToInt32(tv["ma_doi"]);
            int maNguoiDuocDoi = Convert.ToInt32(tv["ma_nguoi_dung"]);
            string vaiTroCu = tv["vai_tro_noi_bo"].ToString();

            if (vaiTroCu == "chu_tich")
                return ServiceResultDTO.Fail("Không thể thay đổi vai trò của Chủ tịch.");
            if (maNguoiThucHien == maNguoiDuocDoi)
                return ServiceResultDTO.Fail("Không thể tự thay đổi vai trò của mình.");
            if (!_teamDal.LaChuTich(maNguoiThucHien, maDoi))
                return ServiceResultDTO.Fail("Chỉ Chủ tịch mới có quyền phân vai trò.");

            _teamDal.CapNhatVaiTro(maThanhVien, vaiTroMoi);
            _teamDal.TaoThongBao(maNguoiDuocDoi, "Vai trò thay đổi",
                "Vai trò của bạn trong đội đã được cập nhật thành: " + vaiTroMoi, "doi", "doi", maDoi);
            return ServiceResultDTO.Ok("Cập nhật vai trò thành công.");
        }

        // Toggle tuyển dụng
        public ServiceResultDTO ToggleDangTuyen(int maNguoiDung, int maDoi, bool dangTuyen)
        {
            if (!_teamDal.LaChuTich(maNguoiDung, maDoi))
                return ServiceResultDTO.Fail("Chỉ Chủ tịch mới có quyền.");
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
