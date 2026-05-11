using DAL;
using DTO;
using System.Collections.Generic;

namespace BUS
{
    public class GiaiDauBUS
    {
        private readonly GiaiDauDAL dal = new GiaiDauDAL();

        public ApiResponseDTO TaoGiaiDau(int maNguoiDung, TaoGiaiDauRequestDTO req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.ten_giai_dau))
                return Loi("Vui lòng nhập tên giải đấu.");
            if (req.so_doi_toi_thieu < 2) req.so_doi_toi_thieu = 2;
            if (req.giai_doan == null || req.giai_doan.Count == 0)
                return Loi("Vui lòng thêm ít nhất 1 giai đoạn thi đấu.");
            foreach (var gd in req.giai_doan)
            {
                if (string.IsNullOrWhiteSpace(gd.ten_giai_doan))
                    return Loi("Vui lòng nhập tên cho mỗi giai đoạn.");
                if (string.IsNullOrWhiteSpace(gd.the_thuc))
                    return Loi("Vui lòng chọn thể thức cho giai đoạn: " + gd.ten_giai_doan);
            }
            int id = dal.TaoGiaiDau(maNguoiDung, req);
            return Ok("Tạo giải đấu thành công.", dal.LayGiaiDau(id));
        }

        public ApiResponseDTO CapNhatGiaiDau(int maNguoiDung, CapNhatGiaiDauRequestDTO req)
        {
            if (req == null || req.ma_giai_dau <= 0) return Loi("Dữ liệu không hợp lệ.");
            if (string.IsNullOrWhiteSpace(req.ten_giai_dau)) return Loi("Vui lòng nhập tên giải đấu.");
            if (!dal.LaBTC(req.ma_giai_dau, maNguoiDung)) return Loi("Bạn không có quyền chỉnh sửa giải đấu này.");
            string tt = dal.LayTrangThai(req.ma_giai_dau);
            if (tt != "nhap" && tt != "bi_tu_choi") return Loi("Chỉ được chỉnh sửa khi giải ở trạng thái Bản nháp hoặc Bị từ chối.");
            if (req.giai_doan == null || req.giai_doan.Count == 0) return Loi("Vui lòng thêm ít nhất 1 giai đoạn.");
            dal.CapNhatGiaiDau(req);
            return Ok("Cập nhật giải đấu thành công.", dal.LayGiaiDau(req.ma_giai_dau));
        }

        public ApiResponseDTO GuiPheDuyet(int maNguoiDung, int maGiaiDau)
        {
            if (!dal.LaBTC(maGiaiDau, maNguoiDung)) return Loi("Bạn không có quyền.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "nhap" && tt != "bi_tu_choi") return Loi("Chỉ gửi phê duyệt từ trạng thái Bản nháp hoặc Bị từ chối.");
            dal.CapNhatTrangThai(maGiaiDau, "cho_xet_duyet");
            return Ok("Đã gửi yêu cầu phê duyệt.", null);
        }

        public ApiResponseDTO PheDuyet(int maNguoiDung, int maGiaiDau)
        {
            if (!dal.LaAdmin(maNguoiDung)) return Loi("Chỉ Admin mới được phê duyệt.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "cho_xet_duyet") return Loi("Giải đấu không ở trạng thái chờ phê duyệt.");
            dal.CapNhatTrangThai(maGiaiDau, "sap_dien_ra");
            return Ok("Đã phê duyệt giải đấu.", null);
        }

        public ApiResponseDTO TuChoi(int maNguoiDung, TuChoiGiaiDauRequestDTO req)
        {
            if (req == null || req.ma_giai_dau <= 0) return Loi("Dữ liệu không hợp lệ.");
            if (string.IsNullOrWhiteSpace(req.ly_do)) return Loi("Vui lòng nhập lý do từ chối.");
            if (!dal.LaAdmin(maNguoiDung)) return Loi("Chỉ Admin mới được từ chối.");
            string tt = dal.LayTrangThai(req.ma_giai_dau);
            if (tt != "cho_xet_duyet") return Loi("Giải đấu không ở trạng thái chờ phê duyệt.");
            dal.TuChoi(req.ma_giai_dau, req.ly_do);
            return Ok("Đã từ chối giải đấu.", null);
        }

        public ApiResponseDTO MoDangKy(int maNguoiDung, int maGiaiDau)
        {
            if (!dal.LaBTC(maGiaiDau, maNguoiDung)) return Loi("Bạn không có quyền.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "sap_dien_ra") return Loi("Chỉ mở đăng ký từ trạng thái Sắp diễn ra.");
            dal.CapNhatTrangThai(maGiaiDau, "mo_dang_ky");
            return Ok("Đã mở đăng ký.", null);
        }

        public ApiResponseDTO DongDangKy(int maNguoiDung, int maGiaiDau)
        {
            if (!dal.LaBTC(maGiaiDau, maNguoiDung)) return Loi("Bạn không có quyền.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "mo_dang_ky") return Loi("Chỉ đóng đăng ký từ trạng thái Mở đăng ký.");
            dal.CapNhatTrangThai(maGiaiDau, "khoa_dang_ky");
            return Ok("Đã chốt sổ đăng ký.", null);
        }

        public ApiResponseDTO MoLaiDangKy(int maNguoiDung, int maGiaiDau)
        {
            if (!dal.LaBTC(maGiaiDau, maNguoiDung)) return Loi("Bạn không có quyền.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "khoa_dang_ky") return Loi("Chỉ mở lại từ trạng thái Khóa đăng ký.");
            dal.CapNhatTrangThai(maGiaiDau, "mo_dang_ky");
            return Ok("Đã mở lại đăng ký.", null);
        }

        public ApiResponseDTO KhoiTranh(int maNguoiDung, int maGiaiDau)
        {
            if (!dal.LaBTC(maGiaiDau, maNguoiDung)) return Loi("Bạn không có quyền.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "khoa_dang_ky") return Loi("Chỉ khởi tranh từ trạng thái Khóa đăng ký.");
            GiaiDauDTO gd = dal.LayGiaiDau(maGiaiDau);
            int soDoi = dal.DemDoiDaDuyet(maGiaiDau);
            if (soDoi < gd.so_doi_toi_thieu)
                return Loi("Chưa đủ số đội tối thiểu (" + gd.so_doi_toi_thieu + " đội). Hiện có " + soDoi + " đội.");
            dal.CapNhatTrangThai(maGiaiDau, "dang_dien_ra");
            return Ok("Đã khởi tranh giải đấu!", null);
        }

        public ApiResponseDTO BeMac(int maNguoiDung, int maGiaiDau)
        {
            if (!dal.LaBTC(maGiaiDau, maNguoiDung)) return Loi("Bạn không có quyền.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "dang_dien_ra") return Loi("Chỉ bế mạc từ trạng thái Đang diễn ra.");
            dal.CapNhatTrangThai(maGiaiDau, "ket_thuc");
            return Ok("Đã bế mạc giải đấu.", null);
        }

        public ApiResponseDTO HuyGiaiDau(int maNguoiDung, int maGiaiDau)
        {
            bool isBTC = dal.LaBTC(maGiaiDau, maNguoiDung);
            bool isAdmin = dal.LaAdmin(maNguoiDung);
            if (!isBTC && !isAdmin) return Loi("Bạn không có quyền hủy giải đấu.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt == "ket_thuc" || tt == "da_huy") return Loi("Không thể hủy giải đấu ở trạng thái này.");
            dal.CapNhatTrangThai(maGiaiDau, "da_huy");
            return Ok("Đã hủy giải đấu.", null);
        }

        public ApiResponseDTO ToggleKhoaDangKy(int maNguoiDung, int maGiaiDau, bool locked)
        {
            if (!dal.LaBTC(maGiaiDau, maNguoiDung)) return Loi("Bạn không có quyền.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "mo_dang_ky") return Loi("Chỉ toggle khóa khi đang Mở đăng ký.");
            dal.ToggleRegistrationLock(maGiaiDau, locked);
            return Ok(locked ? "Đã khóa đăng ký tạm thời." : "Đã mở khóa đăng ký.", null);
        }

        public ApiResponseDTO LayGiaiDauCuaToi(int maNguoiDung)
        {
            return Ok("Lấy danh sách giải đấu thành công.", dal.LayGiaiDauCuaToi(maNguoiDung));
        }

        public ApiResponseDTO LayChiTiet(int maGiaiDau)
        {
            GiaiDauDTO gd = dal.LayGiaiDau(maGiaiDau);
            if (gd == null) return Loi("Không tìm thấy giải đấu.");
            return Ok("Lấy chi tiết thành công.", new GiaiDauChiTietDTO
            {
                giai_dau = gd,
                giai_doan = dal.LayGiaiDoan(maGiaiDau),
                doi_tham_gia = dal.LayDoiThamGia(maGiaiDau)
            });
        }

        public ApiResponseDTO LayDanhSachChoPheDuyet(int maNguoiDung)
        {
            if (!dal.LaAdmin(maNguoiDung)) return Loi("Chỉ Admin mới xem được danh sách này.");
            return Ok("Lấy danh sách thành công.", dal.LayDanhSachChoPheDuyet());
        }

        public ApiResponseDTO LayDanhSachPublic()
        {
            return Ok("Lấy danh sách thành công.", dal.LayDanhSachPublic());
        }

        private ApiResponseDTO Ok(string msg, object data)
        {
            return new ApiResponseDTO { success = true, message = msg, data = data };
        }

        private ApiResponseDTO Loi(string msg)
        {
            return new ApiResponseDTO { success = false, message = msg, data = null };
        }

        public ApiResponseDTO DangKyThamGia(int maNguoiDung, DangKyGiaiDauRequestDTO req)
        {
            if (req == null || req.ma_giai_dau <= 0 || req.ma_doi <= 0) return Loi("Dữ liệu không hợp lệ.");
            
            // Validate user is team president
            bool isPresident = false;
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new System.Data.SqlClient.SqlCommand("SELECT COUNT(1) FROM DOI WHERE ma_doi = @d AND ma_chu_tich = @u", conn))
            {
                cmd.Parameters.AddWithValue("@d", req.ma_doi);
                cmd.Parameters.AddWithValue("@u", maNguoiDung);
                conn.Open();
                isPresident = System.Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
            if (!isPresident) return Loi("Chỉ Chủ tịch đội mới được đăng ký tham gia giải.");

            string tt = dal.LayTrangThai(req.ma_giai_dau);
            if (tt != "mo_dang_ky") return Loi("Giải đấu hiện không mở đăng ký.");

            dal.DangKyThamGiaGiai(req.ma_giai_dau, req.ma_doi);
            return Ok("Đã nộp đơn đăng ký thành công. Vui lòng chờ BTC xét duyệt.", null);
        }

        public ApiResponseDTO MoiDoiThamGia(int maNguoiDung, MoiThamGiaGiaiRequestDTO req)
        {
            if (req == null || req.ma_giai_dau <= 0 || req.ma_doi <= 0) return Loi("Dữ liệu không hợp lệ.");
            if (!dal.LaBTC(req.ma_giai_dau, maNguoiDung)) return Loi("Chỉ BTC mới được mời đội.");
            
            dal.MoiDoiThamGia(req.ma_giai_dau, req.ma_doi, req.loi_nhan);
            return Ok("Đã gửi lời mời đến đội.", null);
        }

        public ApiResponseDTO MoiNhanSu(int maNguoiDung, MoiNhanSuGiaiDauRequestDTO req)
        {
            if (req == null || req.ma_giai_dau <= 0 || string.IsNullOrWhiteSpace(req.username_or_email)) return Loi("Dữ liệu không hợp lệ.");
            if (!dal.LaBTC(req.ma_giai_dau, maNguoiDung)) return Loi("Chỉ BTC mới được mời nhân sự.");
            if (req.vai_tro != "btc" && req.vai_tro != "trong_tai") return Loi("Vai trò không hợp lệ.");

            dal.MoiNhanSu(req.ma_giai_dau, req.username_or_email, req.vai_tro, req.loi_nhan);
            return Ok("Đã gửi lời mời.", null);
        }
    }
}
