using DAL;
using DTO;
using BCryptNet = BCrypt.Net.BCrypt;

namespace BUS
{
    /// <summary>
    /// BUS cho luồng đăng nhập cũ (TaiKhoanController).
    /// Verify BCrypt tại đây vì BUS project có package BCrypt.Net-Next.
    /// DAL chỉ lấy dữ liệu, không verify.
    /// </summary>
    public class NguoiDungBUS
    {
        private readonly NguoiDungDAL _dal = new NguoiDungDAL();

        /// <summary>
        /// Kiểm tra đăng nhập: tìm theo username hoặc email, verify BCrypt.
        /// Trả null nếu không tìm thấy hoặc sai mật khẩu.
        /// </summary>
        public NguoiDungDTO KiemTraDangNhap(string dinhDanh, string matKhau)
        {
            if (string.IsNullOrWhiteSpace(dinhDanh) || string.IsNullOrWhiteSpace(matKhau))
                return null;

            // DAL chỉ lấy user từ DB theo username/email
            NguoiDungDTO user = _dal.LayTheoDinhDanh(dinhDanh.Trim());
            if (user == null) return null;

            // Verify BCrypt tại tầng BUS (BCrypt.Net-Next chỉ có trong BUS project)
            if (!BCryptNet.Verify(matKhau.Trim(), user.MatKhauMaHoa))
                return null;

            return user;
        }
    }
}
