using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DAL;
using DTO;

namespace BUS
{
    public class TournamentBuilderBUS
    {
        private readonly TournamentBuilderDAL _dal = new TournamentBuilderDAL();
        private readonly IdentityDAL _identityDal = new IdentityDAL();

        private const string TrangThaiNhap = "nhap";
        private const string TrangThaiChoXetDuyet = "cho_xet_duyet";
        private const string TrangThaiChuanBiDienRa = "chuan_bi_dien_ra";
        private const string TrangThaiDangDienRa = "dang_dien_ra";
        private const string TrangThaiTongKet = "tong_ket";
        private const string TrangThaiKetThuc = "ket_thuc";
        private const string TrangThaiTamHoan = "tam_hoan";

        public ServiceResultDTO TaoBanNhap(TaoGiaiDauDTO dto)
        {
            if (dto == null || dto.MaNguoiTao <= 0 || string.IsNullOrWhiteSpace(dto.TenGiaiDau))
            {
                return ServiceResultDTO.Fail("Dữ liệu khởi tạo giải đấu không hợp lệ.");
            }

            dto.TenGiaiDau = dto.TenGiaiDau.Trim();
            if (System.Text.RegularExpressions.Regex.IsMatch(dto.TenGiaiDau, @"^[\d\W_]"))
            {
                return ServiceResultDTO.Fail("Tên giải đấu không được bắt đầu bằng số hoặc ký tự đặc biệt.");
            }

            if (!dto.NgayBatDau.HasValue || dto.NgayBatDau.Value <= DateTime.Now.AddMinutes(5))
            {
                return ServiceResultDTO.Fail("Ngày bắt đầu phải ở tương lai ít nhất 5 phút.");
            }

            if (!dto.NgayKetThuc.HasValue || dto.NgayKetThuc.Value <= dto.NgayBatDau.Value.AddMinutes(10))
            {
                return ServiceResultDTO.Fail("Ngày kết thúc phải sau ngày bắt đầu ít nhất 10 phút.");
            }

            if (dto.SoDoiToiThieu < 2)
            {
                return ServiceResultDTO.Fail("Số đội tối thiểu phải tối thiểu 2.");
            }

            if (dto.SoDoiToiDa.HasValue && dto.SoDoiToiDa.Value < dto.SoDoiToiThieu)
            {
                return ServiceResultDTO.Fail("Số đội tối đa phải lớn hơn hoặc bằng số đội tối thiểu.");
            }

            if (dto.GiaiDoan == null || dto.GiaiDoan.Count == 0)
            {
                return ServiceResultDTO.Fail("Phải có ít nhất 1 giai đoạn.");
            }

            dto.TheThuc = ChuanHoaTheThucGiaiDau(dto);
            if (string.IsNullOrWhiteSpace(dto.TheThuc))
            {
                return ServiceResultDTO.Fail("Không xác định được thể thức giải đấu.");
            }

            // Validate stage dates
            for (int i = 0; i < dto.GiaiDoan.Count; i++)
            {
                var giaiDoan = dto.GiaiDoan[i];
                if (!giaiDoan.NgayBatDau.HasValue || !giaiDoan.NgayKetThuc.HasValue)
                {
                    return ServiceResultDTO.Fail($"Giai đoạn {i + 1}: Thiếu ngày bắt đầu hoặc ngày kết thúc.");
                }
                if (giaiDoan.NgayBatDau >= giaiDoan.NgayKetThuc)
                {
                    return ServiceResultDTO.Fail($"Giai đoạn {i + 1}: Ngày kết thúc phải sau ngày bắt đầu.");
                }
                if (i > 0 && dto.GiaiDoan[i - 1].NgayKetThuc >= giaiDoan.NgayBatDau)
                {
                    return ServiceResultDTO.Fail($"Giai đoạn {i}: Ngày bắt đầu phải sau ngày kết thúc của giai đoạn {i}.");
                }
            }

            // Stage 1 start must equal tournament start
            if (dto.GiaiDoan[0].NgayBatDau != dto.NgayBatDau)
            {
                return ServiceResultDTO.Fail("Ngày bắt đầu giai đoạn 1 phải bằng ngày bắt đầu giải.");
            }

            // Last stage end must equal tournament end
            if (dto.GiaiDoan[dto.GiaiDoan.Count - 1].NgayKetThuc != dto.NgayKetThuc)
            {
                return ServiceResultDTO.Fail("Ngày kết thúc giai đoạn cuối phải bằng ngày kết thúc giải.");
            }

            // Always save as draft (nhap)
            string trangThai = TrangThaiNhap;

            int maGiaiDau = _dal.TaoGiaiDau(dto, trangThai);

            if (dto.GiaiThuongs != null && dto.GiaiThuongs.Count > 0)
            {
                _dal.ThemGiaiThuong(maGiaiDau, dto.GiaiThuongs);
            }

            if (dto.GiaiDoan != null && dto.GiaiDoan.Count > 0)
            {
                _dal.ThemGiaiDoan(maGiaiDau, dto.GiaiDoan);
            }

            string msg = "Đã lưu giải đấu dưới dạng nháp. Hãy gửi xét duyệt để admin phê duyệt.";
            return ServiceResultDTO.Ok(msg, new { maGiaiDau, trangThai });
        }

        private static string ChuanHoaTheThucGiaiDau(TaoGiaiDauDTO dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.TheThuc))
            {
                return dto.TheThuc.Trim().ToLowerInvariant();
            }

            if (dto.GiaiDoan == null || dto.GiaiDoan.Count == 0)
            {
                return null;
            }

            string[] stageFormats = dto.GiaiDoan
                .Select(g => (g?.TheThuc ?? string.Empty).Trim().ToLowerInvariant())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToArray();

            if (stageFormats.Length == 0)
            {
                return null;
            }

            if (stageFormats.Length > 1)
            {
                return "hon_hop";
            }

            string stageFormat = stageFormats[0];
            switch (stageFormat)
            {
                case "loai_truc_tiep":
                case "nhanh_thang_nhanh_thua":
                    return stageFormat;
                case "vong_tron":
                case "league_bang_cheo":
                case "thuy_si":
                case "champion_rush":
                    return "vong_tron_tinh_diem";
                default:
                    return "hon_hop";
            }
        }

        public ServiceResultDTO GuiXetDuyet(int maNguoiGui, int maGiaiDau)
        {
            DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiGui, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền gửi xét duyệt giải đấu này.");
            }

            string trangThaiHienTai = giaiDau["trang_thai"].ToString();
            if (!string.Equals(trangThaiHienTai, TrangThaiNhap, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Chỉ có thể gửi xét duyệt khi giải ở trạng thái nháp.");
            }

            bool ok = _dal.CapNhatTrangThaiGiaiTheoMa(maGiaiDau, TrangThaiChoXetDuyet);
            if (!ok)
            {
                return ServiceResultDTO.Fail("Không thể gửi xét duyệt.");
            }

            NguoiDungDTO nguoiGui = _identityDal.LayTheoId(maNguoiGui);
            string tenNguoiGui = nguoiGui != null ? nguoiGui.TenDangNhap : ("UID #" + maNguoiGui);
            int? maTroChoi = giaiDau["ma_tro_choi"] == DBNull.Value ? (int?)null : Convert.ToInt32(giaiDau["ma_tro_choi"]);
            string tenGame = _dal.LayTenGameTheoMa(maTroChoi) ?? "Chưa chọn game";
            string tenGiai = giaiDau["ten_giai_dau"].ToString();
            string thoiGianDuKien = giaiDau["ngay_bat_dau"] == DBNull.Value
                ? "Chưa có"
                : Convert.ToDateTime(giaiDau["ngay_bat_dau"]).ToString("dd/MM/yyyy HH:mm");

            foreach (int maAdmin in _dal.LayDanhSachAdminHeThong())
            {
                _dal.TaoThongBao(
                    maAdmin,
                    "Yêu cầu tạo giải mới cần duyệt",
                    string.Format("{0} vừa gửi yêu cầu tạo giải \"{1}\" ({2}) - dự kiến: {3}. Trạng thái: chờ xét duyệt. Vào tab Yêu cầu Admin để xử lý.", tenNguoiGui, tenGiai, tenGame, thoiGianDuKien),
                    "giai_dau",
                    "giai_dau",
                    maGiaiDau);
            }

            return ServiceResultDTO.Ok("Đã gửi xét duyệt thành công.", new { maGiaiDau, trangThai = "chờ xét duyệt" });
        }

        public ServiceResultDTO PheDuyet(int maAdmin, int maGiaiDau)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới có quyền phê duyệt.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            string trangThaiHienTai = giaiDau["trang_thai"].ToString();
            if (!string.Equals(trangThaiHienTai, TrangThaiChoXetDuyet, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Chỉ có thể phê duyệt khi giải ở trạng thái chờ xét duyệt.");
            }

            bool ok = _dal.CapNhatTrangThaiGiaiTheoMa(maGiaiDau, TrangThaiChuanBiDienRa);
            if (!ok)
            {
                return ServiceResultDTO.Fail("Không thể phê duyệt.");
            }

            if (giaiDau["ma_nguoi_tao"] != DBNull.Value)
            {
                int maNguoiTao = Convert.ToInt32(giaiDau["ma_nguoi_tao"]);
                _dal.TaoThongBao(
                    maNguoiTao,
                    "Yêu cầu tạo giải đã được duyệt",
                    string.Format("Giải đấu \"{0}\" đã được admin duyệt. Bạn có thể mở đăng ký và tiếp tục thiết lập giải.", giaiDau["ten_giai_dau"]),
                    "giai_dau",
                    "giai_dau",
                    maGiaiDau);
            }

            return ServiceResultDTO.Ok("Phê duyệt giải đấu thành công. Giải đã chuyển sang trạng thái chuẩn bị diễn ra.", new { maGiaiDau, trangThai = "chuẩn bị diễn ra" });
        }

        public ServiceResultDTO BulkPheDuyet(int maAdmin)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới có quyền phê duyệt hàng loạt.");
            }

            // Get all tournaments in "cho_xet_duyet" status
            DataTable choXetDuyetList = _dal.LayDanhSachGiaiTheoTrangThai(TrangThaiChoXetDuyet);
            if (choXetDuyetList == null || choXetDuyetList.Rows.Count == 0)
            {
                return ServiceResultDTO.Ok("Không có giải đấu nào chờ xét duyệt.", new { approvedCount = 0 });
            }

            int approvedCount = 0;
            int failedCount = 0;
            List<string> failedIds = new List<string>();

            foreach (DataRow row in choXetDuyetList.Rows)
            {
                int maGiaiDau = Convert.ToInt32(row["ma_giai_dau"]);
                bool ok = _dal.CapNhatTrangThaiGiaiTheoMa(maGiaiDau, TrangThaiChuanBiDienRa);
                if (ok)
                {
                    approvedCount++;

                    DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
                    if (giaiDau != null && giaiDau["ma_nguoi_tao"] != DBNull.Value)
                    {
                        int maNguoiTao = Convert.ToInt32(giaiDau["ma_nguoi_tao"]);
                        _dal.TaoThongBao(
                            maNguoiTao,
                            "Yêu cầu tạo giải đã được duyệt",
                            string.Format("Giải đấu \"{0}\" đã được admin duyệt.", giaiDau["ten_giai_dau"]),
                            "giai_dau",
                            "giai_dau",
                            maGiaiDau);
                    }
                }
                else
                {
                    failedCount++;
                    failedIds.Add(maGiaiDau.ToString());
                }
            }

            string message = $"Đã phê duyệt {approvedCount} giải đấu thành công.";
            if (failedCount > 0)
            {
                message += $" Không thể duyệt {failedCount} giải đấu (ID: {string.Join(", ", failedIds)}).";
            }

            return ServiceResultDTO.Ok(message, new { approvedCount, failedCount });
        }

        public ServiceResultDTO TuChoiYeuCau(int maAdmin, int maGiaiDau, string lyDo)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới có quyền từ chối.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            string trangThai = giaiDau["trang_thai"].ToString();
            if (!string.Equals(trangThai, TrangThaiChoXetDuyet, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Chỉ được từ chối khi giải ở trạng thái chờ xét duyệt.");
            }

            bool ok = _dal.CapNhatTrangThaiGiaiTheoMa(maGiaiDau, TrangThaiNhap);
            if (!ok)
            {
                return ServiceResultDTO.Fail("Không thể từ chối yêu cầu.");
            }

            string lyDoTuChoi = string.IsNullOrWhiteSpace(lyDo) ? "Không đáp ứng tiêu chí xét duyệt." : lyDo.Trim();
            if (giaiDau["ma_nguoi_tao"] != DBNull.Value)
            {
                int maNguoiTao = Convert.ToInt32(giaiDau["ma_nguoi_tao"]);
                _dal.TaoThongBao(
                    maNguoiTao,
                    "Yêu cầu tạo giải bị từ chối",
                    string.Format("Giải đấu \"{0}\" chưa được duyệt. Lý do: {1}", giaiDau["ten_giai_dau"], lyDoTuChoi),
                    "giai_dau",
                    "giai_dau",
                    maGiaiDau);
            }

            return ServiceResultDTO.Ok("Đã từ chối yêu cầu tạo giải.");
        }

        public ServiceResultDTO LayDanhSachChoXetDuyet(int maAdmin)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới có quyền xem danh sách yêu cầu.");
            }

            DataTable dt = _dal.LayDanhSachChoXetDuyet();
            List<object> list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    ma_giai_dau = Convert.ToInt32(row["ma_giai_dau"]),
                    ten_giai_dau = row["ten_giai_dau"].ToString(),
                    ma_nguoi_tao = row["ma_nguoi_tao"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ma_nguoi_tao"]),
                    ten_nguoi_tao = row["ten_nguoi_tao"] == DBNull.Value ? null : row["ten_nguoi_tao"].ToString(),
                    ma_tro_choi = row["ma_tro_choi"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ma_tro_choi"]),
                    ten_game = row["ten_game"] == DBNull.Value ? null : row["ten_game"].ToString(),
                    ngay_bat_dau = row["ngay_bat_dau"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ngay_bat_dau"]),
                    ngay_ket_thuc = row["ngay_ket_thuc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ngay_ket_thuc"]),
                    trang_thai = row["trang_thai"].ToString()
                });
            }

            return ServiceResultDTO.Ok("Lấy danh sách yêu cầu chờ xét duyệt thành công.", list);
        }

        public ServiceResultDTO KhoaGiai(CapNhatTrangThaiGiaiDTO dto)
        {
            return ServiceResultDTO.Fail("Chức năng khóa giải đã được thay thế bằng hệ thống trạng thái mới.");
        }

        public ServiceResultDTO MoKhoaGiai(CapNhatTrangThaiGiaiDTO dto)
        {
            return ServiceResultDTO.Fail("Chức năng mở khóa giải đã được thay thế bằng hệ thống trạng thái mới.");
        }

        public ServiceResultDTO CapNhatGiaiDau(int maNguoiThucHien, TaoGiaiDauDTO dto, int maGiaiDau)
        {
            if (dto == null || maGiaiDau <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu cập nhật giải đấu không hợp lệ.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền cập nhật giải đấu này.");
            }

            string trangThai = giaiDau["trang_thai"].ToString();
            if (string.Equals(trangThai, TrangThaiDangDienRa, StringComparison.OrdinalIgnoreCase)
                || string.Equals(trangThai, TrangThaiTongKet, StringComparison.OrdinalIgnoreCase)
                || string.Equals(trangThai, TrangThaiKetThuc, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Không thể thay đổi thông tin khi giải đang diễn ra, tổng kết hoặc đã kết thúc.");
            }

            // Check if important fields changed
            bool importantFieldsChanged = false;
            if (!string.Equals(dto.TenGiaiDau, giaiDau["ten_giai_dau"].ToString(), StringComparison.OrdinalIgnoreCase))
                importantFieldsChanged = true;
            if (dto.MaTroChoi != Convert.ToInt32(giaiDau["ma_tro_choi"]))
                importantFieldsChanged = true;
            if (!string.Equals(dto.BannerUrl ?? "", (giaiDau["banner_url"] == DBNull.Value ? "" : giaiDau["banner_url"].ToString()), StringComparison.OrdinalIgnoreCase))
                importantFieldsChanged = true;
            if (dto.TongGiaiThuong != Convert.ToDecimal(giaiDau["tong_giai_thuong"]))
                importantFieldsChanged = true;
            if (dto.NgayBatDau.HasValue && giaiDau["ngay_bat_dau"] != DBNull.Value && dto.NgayBatDau.Value != Convert.ToDateTime(giaiDau["ngay_bat_dau"]))
                importantFieldsChanged = true;
            if (dto.NgayKetThuc.HasValue && giaiDau["ngay_ket_thuc"] != DBNull.Value && dto.NgayKetThuc.Value != Convert.ToDateTime(giaiDau["ngay_ket_thuc"]))
                importantFieldsChanged = true;

            bool ok = _dal.CapNhatGiaiDau(maGiaiDau, dto);
            if (!ok)
            {
                return ServiceResultDTO.Fail("Không thể cập nhật giải đấu.");
            }

            if (importantFieldsChanged && string.Equals(trangThai, TrangThaiChuanBiDienRa, StringComparison.OrdinalIgnoreCase))
            {
                _dal.CapNhatTrangThaiGiaiTheoMa(maGiaiDau, TrangThaiNhap);
                return ServiceResultDTO.Ok("Đã cập nhật giải đấu. Giải đã chuyển về trạng thái nháp, vui lòng gửi lại để xét duyệt.");
            }

            return ServiceResultDTO.Ok("Đã cập nhật giải đấu thành công.");
        }

        public ServiceResultDTO TamHoanGiaiDau(TamHoanGiaiDauDTO dto)
        {
            if (dto == null || dto.MaGiaiDau <= 0 || dto.MaAdmin <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu tạm hoãn giải đấu không hợp lệ.");
            }

            if (!LaAdmin(dto.MaAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin mới có quyền tạm hoãn giải đấu.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(dto.MaGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            string trangThai = giaiDau["trang_thai"].ToString();

            // Can only suspend from "chuan_bi_dien_ra" to "tong_ket"
            if (!string.Equals(trangThai, TrangThaiChuanBiDienRa, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(trangThai, TrangThaiDangDienRa, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(trangThai, TrangThaiTongKet, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Chỉ có thể tạm hoãn khi giải ở trạng thái chuẩn bị diễn ra, đang diễn ra hoặc tổng kết.");
            }

            DateTime ngayBatDauTamHoan = DateTime.Now;
            DateTime ngayKetThuc = giaiDau["ngay_ket_thuc"] != DBNull.Value ? Convert.ToDateTime(giaiDau["ngay_ket_thuc"]) : DateTime.MaxValue;

            bool ok = _dal.TamHoanGiaiDau(dto.MaGiaiDau, ngayBatDauTamHoan, dto.MaAdmin, dto.LyDo);
            if (!ok)
            {
                return ServiceResultDTO.Fail("Không thể tạm hoãn giải đấu.");
            }

            return ServiceResultDTO.Ok("Đã tạm hoãn giải đấu thành công.");
        }

        public ServiceResultDTO KhoiPhucTuTamHoan(int maAdmin, int maGiaiDau)
        {
            if (maGiaiDau <= 0 || maAdmin <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu khôi phục không hợp lệ.");
            }

            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin mới có quyền khôi phục giải đấu.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            string trangThai = giaiDau["trang_thai"].ToString();
            if (!string.Equals(trangThai, TrangThaiTamHoan, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Giải đấu không ở trạng thái tạm hoãn.");
            }

            // Calculate new end date: old end date + suspension time + 5 hours
            DateTime? ngayBatDauTamHoan = giaiDau["thoi_gian_bat_dau_tam_hoan"] != DBNull.Value 
                ? (DateTime?)Convert.ToDateTime(giaiDau["thoi_gian_bat_dau_tam_hoan"]) 
                : null;
            
            if (!ngayBatDauTamHoan.HasValue)
            {
                return ServiceResultDTO.Fail("Không tìm thấy thời gian bắt đầu tạm hoãn.");
            }

            DateTime ngayKetThucCu = giaiDau["ngay_ket_thuc"] != DBNull.Value 
                ? Convert.ToDateTime(giaiDau["ngay_ket_thuc"]) 
                : DateTime.MaxValue;

            TimeSpan thoiGianTamHoan = DateTime.Now - ngayBatDauTamHoan.Value;
            DateTime ngayKetThucMoi = ngayKetThucCu.Add(thoiGianTamHoan).AddHours(5);

            bool ok = _dal.KhoiPhucTuTamHoan(maGiaiDau, ngayKetThucMoi);
            return ok
                ? ServiceResultDTO.Ok($"Đã khôi phục giải đấu. Ngày kết thúc mới: {ngayKetThucMoi:dd/MM/yyyy HH:mm}")
                : ServiceResultDTO.Fail("Không thể khôi phục giải đấu.");
        }

        public ServiceResultDTO ThemGiaiDoan(int maNguoiThucHien, TaoGiaiDoanDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.TheThuc))
            {
                return ServiceResultDTO.Fail("Dữ liệu giai đoạn không hợp lệ.");
            }

            // Note: ThemGiaiDoan is now called during tournament creation with a list of stages
            // This method is kept for backward compatibility but may not be used in the new flow
            return ServiceResultDTO.Fail("Phương thức ThemGiaiDoan đã thay đổi. Vui lòng sử dụng TaoBanNhap để tạo giai đoạn.");
        }

        public ServiceResultDTO LenThuTuGiaiDoan(int maNguoiThucHien, int maGiaiDau, int maGiaiDoan)
        {
            return DoiThuTuGiaiDoan(maNguoiThucHien, maGiaiDau, maGiaiDoan, true);
        }

        public ServiceResultDTO XuongThuTuGiaiDoan(int maNguoiThucHien, int maGiaiDau, int maGiaiDoan)
        {
            return DoiThuTuGiaiDoan(maNguoiThucHien, maGiaiDau, maGiaiDoan, false);
        }

        public ServiceResultDTO XoaGiaiDoan(int maNguoiThucHien, int maGiaiDau, int maGiaiDoan)
        {
            DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền xóa giai đoạn.");
            }

            bool ok = _dal.XoaGiaiDoan(maGiaiDoan);
            return ok ? ServiceResultDTO.Ok("Xóa giai đoạn thành công.") : ServiceResultDTO.Fail("Không thể xóa giai đoạn.");
        }

        public ServiceResultDTO LayDanhSachGiaiCuaToi(int maNguoiTao)
        {
            DataTable dt = _dal.LayDanhSachGiaiCuaToi(maNguoiTao);
            List<object> list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    ma_giai_dau = Convert.ToInt32(row["ma_giai_dau"]),
                    ten_giai_dau = row["ten_giai_dau"].ToString(),
                    ma_tro_choi = row["ma_tro_choi"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ma_tro_choi"]),
                    ten_game = row["ten_game"].ToString(),
                    tong_giai_thuong = Convert.ToDecimal(row["tong_giai_thuong"]),
                    trang_thai = row["trang_thai"].ToString(),
                    ngay_bat_dau = row["ngay_bat_dau"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ngay_bat_dau"])
                });
            }
            return ServiceResultDTO.Ok("Thành công", list);
        }

        public ServiceResultDTO LayDanhSachGiaiDoan(int maGiaiDau)
        {
            List<GiaiDoanDTO> data = _dal.LayDanhSachGiaiDoan(maGiaiDau);
            return ServiceResultDTO.Ok("Lấy danh sách giai đoạn thành công.", data);
        }

        public ServiceResultDTO DuyetDangKyDoi(int maNguoiThucHien, DuyetThamGiaGiaiDTO dto)
        {
            if (dto == null || dto.MaGiaiDau <= 0 || dto.MaNhom <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu duyệt đăng ký không hợp lệ.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(dto.MaGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền duyệt đăng ký đội.");
            }

            if (dto.ChapNhan && _dal.NhomKhacGameGiai(dto.MaGiaiDau, dto.MaNhom))
            {
                return ServiceResultDTO.Fail("Không thể duyệt: Tựa game của đội không khớp với tựa game của giải đấu.");
            }

            bool ok = _dal.CapNhatDuyetThamGia(dto.MaGiaiDau, dto.MaNhom, dto.ChapNhan);
            return ok
                ? ServiceResultDTO.Ok(dto.ChapNhan ? "Đã duyệt đội tham gia." : "Đã từ chối đội tham gia.")
                : ServiceResultDTO.Fail("Không thể cập nhật trạng thái đăng ký đội.");
        }

        public ServiceResultDTO CapNhatHatGiong(int maNguoiThucHien, CapNhatHatGiongDTO dto)
        {
            if (dto == null || dto.MaGiaiDau <= 0 || dto.MaNhom <= 0 || dto.HatGiong <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu hạt giống không hợp lệ.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(dto.MaGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền cập nhật hạt giống.");
            }

            bool ok = _dal.CapNhatHatGiong(dto.MaGiaiDau, dto.MaNhom, dto.HatGiong);
            return ok
                ? ServiceResultDTO.Ok("Cập nhật hạt giống thành công.")
                : ServiceResultDTO.Fail("Không thể cập nhật hạt giống.");
        }

        public ServiceResultDTO CapNhatDoiHinhThiDau(int maNguoiThucHien, CapNhatDoiHinhGiaiDTO dto)
        {
            if (dto == null || dto.MaGiaiDau <= 0 || dto.MaNhom <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu đồng bộ đội hình không hợp lệ.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(dto.MaGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền cập nhật đội hình thi đấu.");
            }

            string trangThai = giaiDau["trang_thai"].ToString();
            if (!string.Equals(trangThai, TrangThaiChoXetDuyet, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(trangThai, TrangThaiChuanBiDienRa, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Danh sách thi đấu đã bị khóa. Bạn không thể thay đổi nhân sự lúc này!");
            }

            int soNguoi = _dal.DongBoDoiHinhThiDau(dto.MaGiaiDau, dto.MaNhom);
            if (soNguoi <= 0)
            {
                return ServiceResultDTO.Fail("Không có thành viên hợp lệ để tạo danh sách thi đấu.");
            }

            return ServiceResultDTO.Ok("Đồng bộ roster thành công.", new { soNguoi });
        }

        public ServiceResultDTO ChuyenSangDangDienRa(int maNguoiThucHien, int maGiaiDau)
        {
            if (maNguoiThucHien <= 0 || maGiaiDau <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu chuyển trạng thái giải không hợp lệ.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền thao tác giải đấu này.");
            }

            string trangThai = giaiDau["trang_thai"].ToString();
            if (string.Equals(trangThai, TrangThaiTamHoan, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Giải đấu đang bị tạm hoãn, không thể chuyển sang đang diễn ra.");
            }

            List<int> dsNhom = _dal.LayDanhSachNhomDaDuyet(maGiaiDau);
            if (dsNhom.Count < 2)
            {
                return ServiceResultDTO.Fail("Cần ít nhất 2 đội đã duyệt để bắt đầu giải.");
            }

            int tongNguoi = 0;
            foreach (int maNhom in dsNhom)
            {
                int soNguoi = _dal.DongBoDoiHinhThiDau(maGiaiDau, maNhom);
                if (soNguoi <= 0)
                {
                    return ServiceResultDTO.Fail("Không thể khóa roster cho nhóm " + maNhom + ". Hãy kiểm tra đội hình hợp lệ trước khi bắt đầu giải.");
                }
                tongNguoi += soNguoi;
            }

            bool ok = _dal.CapNhatTrangThaiGiaiTheoMa(maGiaiDau, TrangThaiDangDienRa);
            return ok
                ? ServiceResultDTO.Ok("Giải đã chuyển sang đang diễn ra và roster toàn bộ đội đã được khóa.", new { maGiaiDau, tongNhom = dsNhom.Count, tongNguoi })
                : ServiceResultDTO.Fail("Không thể cập nhật trạng thái giải sang đang diễn ra.");
        }

        private ServiceResultDTO DoiThuTuGiaiDoan(int maNguoiThucHien, int maGiaiDau, int maGiaiDoan, bool len)
        {
            DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền sắp xếp giai đoạn.");
            }

            List<GiaiDoanDTO> list = _dal.LayDanhSachGiaiDoan(maGiaiDau);
            GiaiDoanDTO current = list.Find(x => x.MaGiaiDoan == maGiaiDoan);
            if (current == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giai đoạn cần sắp xếp.");
            }

            int thuTuDich = len ? current.ThuTu - 1 : current.ThuTu + 1;
            GiaiDoanDTO target = list.Find(x => x.ThuTu == thuTuDich);
            if (target == null)
            {
                return ServiceResultDTO.Fail(len ? "Giai đoạn này đã ở vị trí đầu." : "Giai đoạn này đã ở vị trí cuối.");
            }

            bool ok = _dal.HoanDoiThuTuGiaiDoan(maGiaiDau, current.ThuTu, target.ThuTu);
            return ok
                ? ServiceResultDTO.Ok("Sắp xếp giai đoạn thành công.")
                : ServiceResultDTO.Fail("Không thể đổi thứ tự giai đoạn.");
        }

        private bool LaAdmin(int maNguoiDung)
        {
            NguoiDungDTO user = _identityDal.LayTheoId(maNguoiDung);
            return user != null && string.Equals(user.VaiTroHeThong, "admin", StringComparison.OrdinalIgnoreCase);
        }

        private bool CoQuyenQuanLyGiai(int maNguoiDung, DataRow giaiDau)
        {
            if (LaAdmin(maNguoiDung))
            {
                return true;
            }

            if (giaiDau["ma_nguoi_tao"] == DBNull.Value)
            {
                return false;
            }

            return Convert.ToInt32(giaiDau["ma_nguoi_tao"]) == maNguoiDung;
        }

        public ServiceResultDTO LayGiaiCuaToi(int maNguoiDung)
        {
            if (maNguoiDung <= 0) return ServiceResultDTO.Fail("Người dùng không hợp lệ.");

            System.Data.DataTable dt = _dal.LayGiaiCuaToi(maNguoiDung);
            var list = new System.Collections.Generic.List<object>();
            foreach (System.Data.DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    ma_giai_dau      = Convert.ToInt32(row["ma_giai_dau"]),
                    ten_giai_dau     = row["ten_giai_dau"].ToString(),
                    trang_thai       = row["trang_thai"].ToString(),
                    tong_giai_thuong = row["tong_giai_thuong"] == DBNull.Value ? 0 : Convert.ToDouble(row["tong_giai_thuong"]),
                    ngay_bat_dau     = row["ngay_bat_dau"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ngay_bat_dau"]),
                    ngay_ket_thuc    = row["ngay_ket_thuc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ngay_ket_thuc"]),
                    ten_game         = row["ten_game"] == DBNull.Value ? null : row["ten_game"].ToString(),
                    ma_tro_choi      = row["ma_tro_choi"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ma_tro_choi"])
                });
            }
            return ServiceResultDTO.Ok("Lấy danh sách giải thành công.", list);
        }
        public ServiceResultDTO LayDanhSachDangKyDoi(int maNguoiThucHien, int maGiaiDau)
        {
            if (maNguoiThucHien <= 0 || maGiaiDau <= 0)
            {
                return ServiceResultDTO.Fail("Du lieu dang ky doi khong hop le.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Khong tim thay giai dau.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Ban khong co quyen xem danh sach dang ky doi.");
            }

            System.Data.DataTable dt = _dal.LayDanhSachDangKyDoi(maGiaiDau);
            var list = new System.Collections.Generic.List<object>();
            foreach (System.Data.DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    ma_nhom = Convert.ToInt32(row["ma_nhom"]),
                    ten_nhom = row["ten_nhom"].ToString(),
                    ma_doi = Convert.ToInt32(row["ma_doi"]),
                    ten_doi = row["ten_doi"].ToString(),
                    slogan = row["slogan"] == DBNull.Value ? null : row["slogan"].ToString(),
                    ten_game = row["ten_game"] == DBNull.Value ? null : row["ten_game"].ToString(),
                    trang_thai_duyet = row["trang_thai_duyet"].ToString(),
                    hat_giong = row["hat_giong"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["hat_giong"])
                });
            }

            return ServiceResultDTO.Ok("Lay danh sach dang ky doi thanh cong.", list);
        }

        // Automatic status transition methods (to be called by scheduled job)
        public ServiceResultDTO KiemTraVaChuyenTrangThaiChuanBiDienRa()
        {
            // Transition from "chuan_bi_dien_ra" to "dang_dien_ra" when start date is reached
            const string query = @"
UPDATE GIAI_DAU
SET trang_thai = 'dang_dien_ra'
WHERE trang_thai = 'chuan_bi_dien_ra'
  AND ngay_bat_dau <= GETDATE()
  AND is_deleted = 0";

            int affected = _dal.ExecuteRawNonQuery(query);
            return ServiceResultDTO.Ok($"Đã chuyển {affected} giải đấu sang trạng thái đang diễn ra.");
        }

        public ServiceResultDTO KiemTraVaChuyenTrangThaiTongKet()
        {
            // Transition from "dang_dien_ra" to "tong_ket" 6 hours after last match entry
            // This requires tracking last match entry time - to be implemented with match data
            return ServiceResultDTO.Ok("Chức năng chuyển sang tổng kết sẽ được thực hiện sau khi có dữ liệu trận đấu.");
        }

        public ServiceResultDTO KiemTraVaChuyenTrangThaiKetThuc()
        {
            // Transition from "dang_dien_ra" or "tong_ket" to "ket_thuc" at 23:59 on end date
            const string query = @"
UPDATE GIAI_DAU
SET trang_thai = 'ket_thuc'
WHERE trang_thai IN ('dang_dien_ra', 'tong_ket')
  AND ngay_ket_thuc < DATEADD(DAY, 1, CAST(CAST(ngay_ket_thuc AS DATE) AS DATETIME))
  AND is_deleted = 0";

            int affected = _dal.ExecuteRawNonQuery(query);
            return ServiceResultDTO.Ok($"Đã chuyển {affected} giải đấu sang trạng thái kết thúc.");
        }

        public ServiceResultDTO TuDongHuyYeuCauQuaHan()
        {
            // Auto-cancel pending requests when start date is reached
            const string query = @"
UPDATE GIAI_DAU
SET trang_thai = 'nhap'
WHERE trang_thai = 'cho_xet_duyet'
  AND ngay_bat_dau <= GETDATE()
  AND is_deleted = 0";

            int affected = _dal.ExecuteRawNonQuery(query);
            return ServiceResultDTO.Ok($"Đã tự động hủy {affected} yêu cầu quá hạn.");
        }

        public ServiceResultDTO TuDongKhoiPhucKhiKhongDuDoi()
        {
            // Auto-revert to draft if not enough teams at start date
            const string query = @"
UPDATE GIAI_DAU g
SET trang_thai = 'nhap'
WHERE g.trang_thai = 'chuan_bi_dien_ra'
  AND g.ngay_bat_dau <= GETDATE()
  AND g.so_doi_toi_thieu > (SELECT COUNT(1) FROM THAM_GIA_GIAI tg WHERE tg.ma_giai_dau = g.ma_giai_dau AND tg.trang_thai_duyet = 'da_duyet')
  AND g.is_deleted = 0";

            int affected = _dal.ExecuteRawNonQuery(query);
            return ServiceResultDTO.Ok($"Đã tự động chuyển {affected} giải đấu về nháp do không đủ đội.");
        }

        public ServiceResultDTO TuDongDongDangKyKhiDuDoi()
        {
            // Auto-close registration when max teams reached
            const string query = @"
UPDATE GIAI_DAU g
SET dang_mo_dang_ky = 0
WHERE g.dang_mo_dang_ky = 1
  AND g.so_doi_toi_da IS NOT NULL
  AND g.so_doi_toi_da <= (SELECT COUNT(1) FROM THAM_GIA_GIAI tg WHERE tg.ma_giai_dau = g.ma_giai_dau AND tg.trang_thai_duyet = 'da_duyet')
  AND g.is_deleted = 0";

            int affected = _dal.ExecuteRawNonQuery(query);
            return ServiceResultDTO.Ok($"Đã tự động đóng đăng ký {affected} giải đấu do đủ số đội.");
        }

        public ServiceResultDTO MoiTrongTai(int maNguoiThucHien, MoiTrongTaiDTO dto)
        {
            DataRow giaiDau = _dal.LayGiaiTheoId(dto.MaGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền mời trọng tài cho giải đấu này.");
            }

            string trangThai = giaiDau["trang_thai"].ToString();
            if (!string.Equals(trangThai, TrangThaiChuanBiDienRa, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Chỉ có thể mời trọng tài khi giải ở trạng thái chuẩn bị diễn ra.");
            }

            dto.MaNguoiMoi = maNguoiThucHien;
            bool ok = _dal.MoiTrongTai(dto);
            return ok ? ServiceResultDTO.Ok("Đã mời trọng tài thành công.") : ServiceResultDTO.Fail("Không thể mời trọng tài.");
        }

        public ServiceResultDTO MoiBanToChuc(int maNguoiThucHien, MoiBanToChucDTO dto)
        {
            DataRow giaiDau = _dal.LayGiaiTheoId(dto.MaGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền mời ban tổ chức cho giải đấu này.");
            }

            string trangThai = giaiDau["trang_thai"].ToString();
            if (!string.Equals(trangThai, TrangThaiChuanBiDienRa, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Chỉ có thể mời ban tổ chức khi giải ở trạng thái chuẩn bị diễn ra.");
            }

            dto.MaNguoiMoi = maNguoiThucHien;
            bool ok = _dal.MoiBanToChuc(dto);
            return ok ? ServiceResultDTO.Ok("Đã mời ban tổ chức thành công.") : ServiceResultDTO.Fail("Không thể mời ban tổ chức.");
        }

        public ServiceResultDTO CapNhatDangMoDangKy(int maNguoiThucHien, int maGiaiDau, bool dangMo)
        {
            DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền thay đổi trạng thái đăng ký.");
            }

            string trangThai = giaiDau["trang_thai"].ToString();
            if (!string.Equals(trangThai, TrangThaiChuanBiDienRa, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Chỉ có thể thay đổi trạng thái đăng ký khi giải ở trạng thái chuẩn bị diễn ra.");
            }

            bool ok = _dal.CapNhatDangMoDangKy(maGiaiDau, dangMo);
            return ok ? ServiceResultDTO.Ok(dangMo ? "Đã mở đăng ký." : "Đã đóng đăng ký.") : ServiceResultDTO.Fail("Không thể thay đổi trạng thái đăng ký.");
        }
    }
}
