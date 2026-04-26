using System;
using System.Data;
using System.Data.SqlClient;
using DTO;

namespace DAL
{
    /// <summary>
    /// DAL cho luồng đăng nhập cũ (TaiKhoanController).
    /// Chỉ lấy dữ liệu user từ DB — KHÔNG tự verify mật khẩu.
    /// Việc verify BCrypt được thực hiện tại tầng BUS.
    /// </summary>
    public class NguoiDungDAL
    {
        /// <summary>
        /// Tìm user theo username HOẶC email. Trả null nếu không tìm thấy.
        /// KHÔNG so sánh mật khẩu tại đây — để BUS dùng BCrypt.Verify.
        /// </summary>
        public NguoiDungDTO LayTheoDinhDanh(string dinhDanh)
        {
            const string query = @"
SELECT TOP 1
    ma_nguoi_dung, ten_dang_nhap, email,
    mat_khau_ma_hoa, vai_tro_he_thong,
    avatar_url, bio, ngay_tao,
    ISNULL(is_banned, 0) AS is_banned,
    ly_do_ban, thoi_gian_ban
FROM NGUOI_DUNG
WHERE ten_dang_nhap = @DinhDanh OR email = @DinhDanh;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@DinhDanh", SqlDbType.NVarChar) { Value = dinhDanh.Trim() }
            });

            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            return new NguoiDungDTO
            {
                MaNguoiDung   = Convert.ToInt32(row["ma_nguoi_dung"]),
                TenDangNhap   = row["ten_dang_nhap"].ToString(),
                Email         = row["email"] != DBNull.Value ? row["email"].ToString() : "",
                MatKhauMaHoa  = row["mat_khau_ma_hoa"].ToString(),
                VaiTroHeThong = row["vai_tro_he_thong"].ToString(),
                AvatarUrl     = row["avatar_url"]   == DBNull.Value ? null : row["avatar_url"].ToString(),
                Bio           = row["bio"]          == DBNull.Value ? null : row["bio"].ToString(),
                NgayTao       = Convert.ToDateTime(row["ngay_tao"]),
                IsBanned      = Convert.ToBoolean(row["is_banned"]),
                LyDoBan       = row["ly_do_ban"]     == DBNull.Value ? null : row["ly_do_ban"].ToString(),
                ThoiGianBan   = row["thoi_gian_ban"] == DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(row["thoi_gian_ban"])
            };
        }
    }
}
