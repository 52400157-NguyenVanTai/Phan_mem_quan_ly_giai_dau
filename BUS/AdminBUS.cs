using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DAL;
using DTO;

namespace BUS
{
    /// <summary>
    /// Business Logic cho toàn bộ Module Admin hệ thống (Trụ cột 5).
    /// Gồm: Dashboard, Ban/Unban User & Đội, CRUD Game, Hard Wipe giải đấu.
    /// </summary>
    public class AdminBUS
    {
        private readonly AdminDAL _dal = new AdminDAL();
        private readonly IdentityDAL _identityDal = new IdentityDAL();

        // ================================================================
        // MODULE 1: GLOBAL DASHBOARD
        // ================================================================

        public ServiceResultDTO LayThongKeDashboard(int maAdmin)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới được xem dashboard.");
            }

            var thongKe = _dal.LayThongKeDashboard();
            var giaiChoXetDuyet = _dal.LayGiaiChoXetDuyet();
            var khieuNaiChoXuLy = _dal.LayKhieuNaiChoXuLy();

            return ServiceResultDTO.Ok("Lấy thống kê dashboard thành công.", new
            {
                ThongKe = thongKe,
                ActionRequired = new
                {
                    GiaiChoXetDuyet = giaiChoXetDuyet,
                    KhieuNaiChoXuLy = khieuNaiChoXuLy
                }
            });
        }

        // ================================================================
        // MODULE 3: QUẢN LÝ NGƯỜI DÙNG (BAN / UNBAN)
        // ================================================================

        public ServiceResultDTO TimKiemNguoiDung(int maAdmin, string tuKhoa, bool? isBanned)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới được tìm kiếm người dùng.");
            }

            var list = _dal.TimKiemNguoiDung(tuKhoa, isBanned);
            return ServiceResultDTO.Ok("Tìm kiếm người dùng thành công.", list);
        }

        /// <summary>
        /// Ban 1 user. Side effects: gạch tên khỏi đội hình giải đang chạy, gửi thông báo cho BTC.
        /// </summary>
        public ServiceResultDTO BanNguoiDung(int maAdmin, int maNguoiBan, string lyDo)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới được ban người dùng.");
            }

            if (maAdmin == maNguoiBan)
            {
                return ServiceResultDTO.Fail("Admin không thể tự ban chính mình.");
            }

            if (string.IsNullOrWhiteSpace(lyDo))
            {
                return ServiceResultDTO.Fail("Phải nhập lý do ban.");
            }

            if (!_dal.NguoiDungTonTai(maNguoiBan))
            {
                return ServiceResultDTO.Fail("Không tìm thấy người dùng cần ban.");
            }

            NguoiDungDTO nguoiBanInfo = _identityDal.LayTheoId(maNguoiBan);
            if (nguoiBanInfo != null && string.Equals(nguoiBanInfo.VaiTroHeThong, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Không thể ban tài khoản admin hệ thống khác.");
            }

            // Lấy danh sách giải đang tham gia TRƯỚC khi ban (để thông báo BTC)
            var giaiDangThamGia = _dal.LayGiaiDangThamGia(maNguoiBan);

            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Set is_banned = 1
                        _dal.BanNguoiDungTrongTransaction(conn, tran, maNguoiBan, maAdmin, lyDo.Trim());

                        // 2. Gạch tên khỏi đội hình giải đang chạy
                        _dal.GachKhoiDoiHinhTrongTransaction(conn, tran, maNguoiBan);

                        // 3. Gửi thông báo cho BTC của từng giải bị ảnh hưởng
                        HashSet<int> btcDaGui = new HashSet<int>();
                        foreach (Dictionary<string, object> giaiInfo in giaiDangThamGia)
                        {
                            int maBtc = 0;
                            if (giaiInfo.ContainsKey("ma_btc") && giaiInfo["ma_btc"] != null)
                            {
                                maBtc = Convert.ToInt32(giaiInfo["ma_btc"]);
                            }

                            if (maBtc > 0 && !btcDaGui.Contains(maBtc))
                            {
                                string tenGiai = giaiInfo.ContainsKey("ten_giai_dau") ? giaiInfo["ten_giai_dau"]?.ToString() : "Không rõ";
                                string tenNhom = giaiInfo.ContainsKey("ten_nhom") ? giaiInfo["ten_nhom"]?.ToString() : "Không rõ";
                                string tieuDe = "[CẢNH BÁO] Tuyển thủ bị ban khỏi hệ thống";
                                string noiDung = string.Format(
                                    "Admin đã ban tài khoản mã #{0} (nhóm: {1}) khỏi hệ thống. Tuyển thủ này đã bị gạch tên khỏi đội hình giải \"{2}\". Lý do: {3}",
                                    maNguoiBan, tenNhom, tenGiai, lyDo.Trim());
                                _dal.GuiThongBaoTrongTransaction(conn, tran, maBtc, tieuDe, noiDung);
                                btcDaGui.Add(maBtc);
                            }
                        }

                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        return ServiceResultDTO.Fail("Ban người dùng thất bại, đã rollback: " + ex.Message);
                    }
                }
            }

            return ServiceResultDTO.Ok("Đã ban người dùng thành công và gạch tên khỏi các giải đang chạy.",
                new { maNguoiBan, soGiaiAnhHuong = giaiDangThamGia.Count });
        }

        /// <summary>
        /// Ban toàn bộ đội tuyển: ban từng thành viên đang thi đấu của đội.
        /// </summary>
        public ServiceResultDTO BanDoi(int maAdmin, int maDoi, string lyDo)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới được ban đội tuyển.");
            }

            if (string.IsNullOrWhiteSpace(lyDo))
            {
                return ServiceResultDTO.Fail("Phải nhập lý do ban.");
            }

            List<int> dsMaThanhVien = _dal.LayThanhVienDangThiDauCuaDoi(maDoi);
            if (dsMaThanhVien.Count == 0)
            {
                return ServiceResultDTO.Fail("Đội này hiện không có thành viên đang tham gia giải đấu nào.");
            }

            int soBanThanhCong = 0;
            List<string> errors = new List<string>();

            foreach (int maTv in dsMaThanhVien)
            {
                // Bỏ qua admin
                NguoiDungDTO tvInfo = _identityDal.LayTheoId(maTv);
                if (tvInfo != null && string.Equals(tvInfo.VaiTroHeThong, "admin", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ServiceResultDTO ketQua = BanNguoiDung(maAdmin, maTv, lyDo.Trim() + " [Ban đội mã #" + maDoi + "]");
                if (ketQua.Success)
                {
                    soBanThanhCong++;
                }
                else
                {
                    errors.Add("User #" + maTv + ": " + ketQua.Message);
                }
            }

            return ServiceResultDTO.Ok(
                string.Format("Đã ban {0}/{1} thành viên đang thi đấu của đội.", soBanThanhCong, dsMaThanhVien.Count),
                new { soBanThanhCong, tongThanhVien = dsMaThanhVien.Count, errors });
        }

        public ServiceResultDTO UnbanNguoiDung(int maAdmin, int maNguoiDung)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới được unban người dùng.");
            }

            if (!_dal.NguoiDungTonTai(maNguoiDung))
            {
                return ServiceResultDTO.Fail("Không tìm thấy người dùng.");
            }

            bool ok = _dal.UnbanNguoiDung(maNguoiDung);
            return ok
                ? ServiceResultDTO.Ok("Đã gỡ ban người dùng thành công.", new { maNguoiDung })
                : ServiceResultDTO.Fail("Không thể gỡ ban (người dùng có thể chưa bị ban).");
        }

        // ================================================================
        // MODULE 5: HARD WIPE GIẢI ĐẤU
        // ================================================================

        /// <summary>
        /// Xóa cứng (Hard Wipe) toàn bộ dữ liệu của một giải đấu qua SP_XoaXachGiaiDau.
        /// Chỉ cho phép xóa giải khi không còn ở trạng thái hoạt động.
        /// </summary>
        public ServiceResultDTO XoaCungGiaiDau(int maAdmin, int maGiaiDau)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới được xóa cứng giải đấu.");
            }

            if (!_dal.GiaiTonTai(maGiaiDau))
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu (hoặc giải đã bị xóa trước đó).");
            }

            string trangThai = _dal.LayTrangThaiGiai(maGiaiDau);
            if (trangThai == "dang_dien_ra" || trangThai == "chuan_bi_dien_ra")
            {
                return ServiceResultDTO.Fail(
                    "Không thể xóa cứng giải đang hoạt động (trạng thái: " + trangThai + "). " +
                    "Hãy khóa giải trước, sau đó mới xóa cứng.");
            }

            try
            {
                _dal.XoaCungGiaiDau(maGiaiDau);
                return ServiceResultDTO.Ok("Đã xóa cứng toàn bộ dữ liệu giải đấu #" + maGiaiDau + " thành công.", new { maGiaiDau });
            }
            catch (Exception ex)
            {
                return ServiceResultDTO.Fail("Xóa cứng thất bại: " + ex.Message);
            }
        }

        // ================================================================
        // PRIVATE HELPERS
        // ================================================================

        private bool LaAdmin(int maNguoiDung)
        {
            NguoiDungDTO user = _identityDal.LayTheoId(maNguoiDung);
            return user != null && string.Equals(user.VaiTroHeThong, "admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
