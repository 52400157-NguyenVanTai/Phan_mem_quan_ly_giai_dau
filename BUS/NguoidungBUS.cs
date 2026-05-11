using DAL;
using DTO;
using System;
using System.Text.RegularExpressions;

namespace BUS
{
    public class NguoidungBUS
    {
        private readonly NguoidungDAL nguoidungDAL = new NguoidungDAL();

        public ApiResponseDTO DangNhap(LoginRequestDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ten_dang_nhap_hoac_email) || string.IsNullOrWhiteSpace(request.mat_khau))
            {
                return Loi("Vui lòng nhập tài khoản và mật khẩu.");
            }

            NguoidungDTO user = nguoidungDAL.GetByUsernameOrEmail(request.ten_dang_nhap_hoac_email.Trim());

            if (user == null || !PasswordHasher.VerifyPassword(request.mat_khau, user.mat_khau_ma_hoa))
            {
                return Loi("Tài khoản hoặc mật khẩu không đúng.");
            }

            if (user.is_banned)
            {
                return Loi("Tài khoản của bạn đang bị khóa.");
            }

            user.mat_khau_ma_hoa = null;
            return ThanhCong("Đăng nhập thành công.", user);
        }

        public ApiResponseDTO DangKy(RegisterRequestDTO request)
        {
            ApiResponseDTO validation = ValidateRegister(request);
            if (!validation.success)
            {
                return validation;
            }

            if (nguoidungDAL.ExistsUsername(request.ten_dang_nhap.Trim()))
            {
                return Loi("Tên đăng nhập đã tồn tại.");
            }

            if (nguoidungDAL.ExistsEmail(request.email.Trim()))
            {
                return Loi("Email đã tồn tại.");
            }

            NguoidungDTO user = new NguoidungDTO
            {
                ten_dang_nhap = request.ten_dang_nhap.Trim(),
                email = request.email.Trim(),
                mat_khau_ma_hoa = PasswordHasher.HashPassword(request.mat_khau),
                vai_tro_he_thong = "user"
            };

            int id = nguoidungDAL.Insert(user);
            user.ma_nguoi_dung = id;
            user.mat_khau_ma_hoa = null;

            return ThanhCong("Đăng ký thành công.", user);
        }

        public ApiResponseDTO LayHoSo(int maNguoiDung)
        {
            NguoidungDTO user = nguoidungDAL.GetById(maNguoiDung);
            if (user == null)
            {
                return Loi("Không tìm thấy người dùng.");
            }

            user.mat_khau_ma_hoa = null;
            return ThanhCong("Lấy hồ sơ thành công.", user);
        }

        public ApiResponseDTO CapNhatHoSo(int maNguoiDung, UpdateProfileRequestDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ten_dang_nhap) || string.IsNullOrWhiteSpace(request.email))
            {
                return Loi("Vui lòng nhập tên đăng nhập và email.");
            }

            if (!IsValidEmail(request.email.Trim()))
            {
                return Loi("Email không hợp lệ.");
            }

            NguoidungDTO current = nguoidungDAL.GetById(maNguoiDung);
            if (current == null)
            {
                return Loi("Không tìm thấy người dùng.");
            }

            if (nguoidungDAL.ExistsUsername(request.ten_dang_nhap.Trim(), maNguoiDung))
            {
                return Loi("Tên đăng nhập đã được sử dụng.");
            }

            if (nguoidungDAL.ExistsEmail(request.email.Trim(), maNguoiDung))
            {
                return Loi("Email đã được sử dụng.");
            }

            current.ten_dang_nhap = request.ten_dang_nhap.Trim();
            current.email = request.email.Trim();
            current.avatar_url = string.IsNullOrWhiteSpace(request.avatar_url) ? null : request.avatar_url.Trim();
            current.bio = string.IsNullOrWhiteSpace(request.bio) ? null : request.bio.Trim();

            nguoidungDAL.UpdateProfile(current);
            current.mat_khau_ma_hoa = null;

            return ThanhCong("Cập nhật hồ sơ thành công.", current);
        }

        public ApiResponseDTO DoiMatKhau(int maNguoiDung, ChangePasswordRequestDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.mat_khau_cu) || string.IsNullOrWhiteSpace(request.mat_khau_moi))
            {
                return Loi("Vui lòng nhập đầy đủ thông tin mật khẩu.");
            }

            if (request.mat_khau_moi != request.xac_nhan_mat_khau_moi)
            {
                return Loi("Xác nhận mật khẩu mới không khớp.");
            }

            if (request.mat_khau_moi.Length < 6)
            {
                return Loi("Mật khẩu mới phải có ít nhất 6 ký tự.");
            }

            NguoidungDTO user = nguoidungDAL.GetById(maNguoiDung);
            if (user == null || !PasswordHasher.VerifyPassword(request.mat_khau_cu, user.mat_khau_ma_hoa))
            {
                return Loi("Mật khẩu cũ không đúng.");
            }

            nguoidungDAL.UpdatePassword(maNguoiDung, PasswordHasher.HashPassword(request.mat_khau_moi));
            return ThanhCong("Đổi mật khẩu thành công.", null);
        }

        public ApiResponseDTO QuenMatKhau(ForgotPasswordRequestDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.email) || string.IsNullOrWhiteSpace(request.mat_khau_moi))
            {
                return Loi("Vui lòng nhập email và mật khẩu mới.");
            }

            if (request.mat_khau_moi != request.xac_nhan_mat_khau_moi)
            {
                return Loi("Xác nhận mật khẩu mới không khớp.");
            }

            if (request.mat_khau_moi.Length < 6)
            {
                return Loi("Mật khẩu mới phải có ít nhất 6 ký tự.");
            }

            NguoidungDTO user = nguoidungDAL.GetByEmail(request.email.Trim());
            if (user == null)
            {
                return Loi("Email không tồn tại trong hệ thống.");
            }

            nguoidungDAL.UpdatePassword(user.ma_nguoi_dung, PasswordHasher.HashPassword(request.mat_khau_moi));
            return ThanhCong("Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại.", null);
        }

        private ApiResponseDTO ValidateRegister(RegisterRequestDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ten_dang_nhap) || string.IsNullOrWhiteSpace(request.email) || string.IsNullOrWhiteSpace(request.mat_khau))
            {
                return Loi("Vui lòng nhập đầy đủ thông tin đăng ký.");
            }

            if (request.ten_dang_nhap.Trim().Length < 3)
            {
                return Loi("Tên đăng nhập phải có ít nhất 3 ký tự.");
            }

            if (!IsValidEmail(request.email.Trim()))
            {
                return Loi("Email không hợp lệ.");
            }

            if (request.mat_khau.Length < 6)
            {
                return Loi("Mật khẩu phải có ít nhất 6 ký tự.");
            }

            if (request.mat_khau != request.xac_nhan_mat_khau)
            {
                return Loi("Xác nhận mật khẩu không khớp.");
            }

            return ThanhCong("Hợp lệ.", null);
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private ApiResponseDTO ThanhCong(string message, object data)
        {
            return new ApiResponseDTO { success = true, message = message, data = data };
        }

        private ApiResponseDTO Loi(string message)
        {
            return new ApiResponseDTO { success = false, message = message, data = null };
        }
    }
}
