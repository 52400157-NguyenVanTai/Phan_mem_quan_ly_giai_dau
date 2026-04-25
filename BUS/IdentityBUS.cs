using System;
using DAL;
using DTO;
using BCryptNet = BCrypt.Net.BCrypt;

namespace BUS
{
    public class IdentityBUS
    {
        private readonly IdentityDAL _identityDal = new IdentityDAL();

        public ServiceResultDTO DangKy(DangKyNguoiDungDTO dto)
        {
            if (dto == null)
            {
                return ServiceResultDTO.Fail("Dữ liệu đăng ký không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(dto.TenDangNhap) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.MatKhau))
            {
                return ServiceResultDTO.Fail("Tên đăng nhập, email và mật khẩu là bắt buộc.");
            }

            if (dto.MatKhau.Trim().Length < 8)
            {
                return ServiceResultDTO.Fail("Mật khẩu tối thiểu 8 ký tự.");
            }

            if (_identityDal.TenDangNhapTonTai(dto.TenDangNhap))
            {
                return ServiceResultDTO.Fail("Tên đăng nhập đã tồn tại.");
            }

            if (_identityDal.EmailTonTai(dto.Email))
            {
                return ServiceResultDTO.Fail("Email đã tồn tại.");
            }

            var user = new NguoiDungDTO
            {
                TenDangNhap = dto.TenDangNhap.Trim(),
                Email = dto.Email.Trim(),
                MatKhauMaHoa = BCryptNet.HashPassword(dto.MatKhau.Trim()),
                VaiTroHeThong = "user",
                AvatarUrl = string.IsNullOrWhiteSpace(dto.AvatarUrl) ? null : dto.AvatarUrl.Trim(),
                Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio.Trim()
            };

            int maNguoiDung = _identityDal.TaoNguoiDung(user);
            return ServiceResultDTO.Ok("Đăng ký thành công.", new { maNguoiDung });
        }

        public ServiceResultDTO DangNhap(DangNhapDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.DinhDanh) || string.IsNullOrWhiteSpace(dto.MatKhau))
            {
                return ServiceResultDTO.Fail("Vui lòng nhập đầy đủ thông tin đăng nhập.");
            }

            NguoiDungDTO user = _identityDal.LayTheoDinhDanh(dto.DinhDanh.Trim());
            if (user == null)
            {
                return ServiceResultDTO.Fail("Tài khoản không tồn tại.");
            }

            if (!BCryptNet.Verify(dto.MatKhau.Trim(), user.MatKhauMaHoa))
            {
                return ServiceResultDTO.Fail("Mật khẩu không chính xác.");
            }

            return ServiceResultDTO.Ok("Đăng nhập thành công.", new
            {
                user.MaNguoiDung,
                user.TenDangNhap,
                user.Email,
                user.VaiTroHeThong,
                user.AvatarUrl,
                user.Bio
            });
        }

        public ServiceResultDTO DoiMatKhau(CapNhatMatKhauDTO dto)
        {
            if (dto == null || dto.MaNguoiDung <= 0 || string.IsNullOrWhiteSpace(dto.MatKhauCu) || string.IsNullOrWhiteSpace(dto.MatKhauMoi))
            {
                return ServiceResultDTO.Fail("Dữ liệu đổi mật khẩu không hợp lệ.");
            }

            if (dto.MatKhauMoi.Trim().Length < 8)
            {
                return ServiceResultDTO.Fail("Mật khẩu mới tối thiểu 8 ký tự.");
            }

            NguoiDungDTO user = _identityDal.LayTheoId(dto.MaNguoiDung);
            if (user == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy người dùng.");
            }

            if (!BCryptNet.Verify(dto.MatKhauCu.Trim(), user.MatKhauMaHoa))
            {
                return ServiceResultDTO.Fail("Mật khẩu cũ không đúng.");
            }

            bool ok = _identityDal.CapNhatMatKhau(dto.MaNguoiDung, BCryptNet.HashPassword(dto.MatKhauMoi.Trim()));
            return ok ? ServiceResultDTO.Ok("Đổi mật khẩu thành công.") : ServiceResultDTO.Fail("Không thể đổi mật khẩu.");
        }

        public ServiceResultDTO CapNhatThongTin(CapNhatThongTinCoBanDTO dto)
        {
            if (dto == null || dto.MaNguoiDung <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu cập nhật không hợp lệ.");
            }

            bool ok = _identityDal.CapNhatThongTinCoBan(dto);
            return ok ? ServiceResultDTO.Ok("Cập nhật thông tin thành công.") : ServiceResultDTO.Fail("Không thể cập nhật thông tin.");
        }
    }
}
