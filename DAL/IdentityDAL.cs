using System;
using System.Data;
using System.Data.SqlClient;
using DTO;

namespace DAL
{
    public class IdentityDAL
    {
        public bool TenDangNhapTonTai(string tenDangNhap)
        {
            const string query = "SELECT COUNT(1) FROM NGUOI_DUNG WHERE ten_dang_nhap = @TenDangNhap";
            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@TenDangNhap", SqlDbType.NVarChar) { Value = tenDangNhap.Trim() }
            });
            return Convert.ToInt32(result) > 0;
        }

        public bool EmailTonTai(string email)
        {
            const string query = "SELECT COUNT(1) FROM NGUOI_DUNG WHERE email = @Email";
            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@Email", SqlDbType.NVarChar) { Value = email.Trim() }
            });
            return Convert.ToInt32(result) > 0;
        }

        public int TaoNguoiDung(NguoiDungDTO dto)
        {
            const string query = @"
INSERT INTO NGUOI_DUNG(ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong, avatar_url, bio)
OUTPUT INSERTED.ma_nguoi_dung
VALUES(@TenDangNhap, @Email, @MatKhauMaHoa, @VaiTroHeThong, @AvatarUrl, @Bio);";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@TenDangNhap", SqlDbType.NVarChar){ Value = dto.TenDangNhap.Trim() },
                new SqlParameter("@Email", SqlDbType.NVarChar){ Value = dto.Email.Trim() },
                new SqlParameter("@MatKhauMaHoa", SqlDbType.NVarChar){ Value = dto.MatKhauMaHoa },
                new SqlParameter("@VaiTroHeThong", SqlDbType.NVarChar){ Value = dto.VaiTroHeThong },
                new SqlParameter("@AvatarUrl", SqlDbType.NVarChar){ Value = (object)dto.AvatarUrl ?? DBNull.Value },
                new SqlParameter("@Bio", SqlDbType.NVarChar){ Value = (object)dto.Bio ?? DBNull.Value }
            });

            return Convert.ToInt32(result);
        }

        public NguoiDungDTO LayTheoDinhDanh(string dinhDanh)
        {
            const string query = @"
SELECT TOP 1 ma_nguoi_dung, ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong, avatar_url, bio, ngay_tao,
    ISNULL(is_banned, 0) AS is_banned, ly_do_ban, thoi_gian_ban
FROM NGUOI_DUNG
WHERE ten_dang_nhap = @DinhDanh OR email = @DinhDanh;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@DinhDanh", SqlDbType.NVarChar){ Value = dinhDanh.Trim() }
            });

            return dt.Rows.Count == 0 ? null : MapNguoiDung(dt.Rows[0]);
        }

        public NguoiDungDTO LayTheoId(int maNguoiDung)
        {
            const string query = @"
SELECT TOP 1 ma_nguoi_dung, ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong, avatar_url, bio, ngay_tao,
    ISNULL(is_banned, 0) AS is_banned, ly_do_ban, thoi_gian_ban
FROM NGUOI_DUNG
WHERE ma_nguoi_dung = @MaNguoiDung;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung }
            });

            return dt.Rows.Count == 0 ? null : MapNguoiDung(dt.Rows[0]);
        }

        public bool CapNhatMatKhau(int maNguoiDung, string matKhauHash)
        {
            const string query = "UPDATE NGUOI_DUNG SET mat_khau_ma_hoa = @MatKhauMaHoa WHERE ma_nguoi_dung = @MaNguoiDung";
            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MatKhauMaHoa", SqlDbType.NVarChar){ Value = matKhauHash },
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung }
            });
            return affected > 0;
        }

        public bool CapNhatThongTinCoBan(CapNhatThongTinCoBanDTO dto)
        {
            const string query = @"
UPDATE NGUOI_DUNG
SET avatar_url = @AvatarUrl,
    bio = @Bio,
    email = @Email
WHERE ma_nguoi_dung = @MaNguoiDung";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@AvatarUrl", SqlDbType.NVarChar){ Value = (object)dto.AvatarUrl ?? DBNull.Value },
                new SqlParameter("@Bio", SqlDbType.NVarChar){ Value = (object)dto.Bio ?? DBNull.Value },
                new SqlParameter("@Email", SqlDbType.NVarChar){ Value = (object)dto.Email ?? DBNull.Value },
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = dto.MaNguoiDung }
            });

            return affected > 0;
        }

        public bool CapNhatAvatarUrl(int maNguoiDung, string avatarUrl)
        {
            const string query = "UPDATE NGUOI_DUNG SET avatar_url = @AvatarUrl WHERE ma_nguoi_dung = @MaNguoiDung";
            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@AvatarUrl", SqlDbType.NVarChar){ Value = (object)avatarUrl ?? DBNull.Value },
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung }
            });
            return affected > 0;
        }

        private static NguoiDungDTO MapNguoiDung(DataRow row)
        {
            return new NguoiDungDTO
            {
                MaNguoiDung   = Convert.ToInt32(row["ma_nguoi_dung"]),
                TenDangNhap   = row["ten_dang_nhap"].ToString(),
                Email         = row["email"].ToString(),
                MatKhauMaHoa  = row["mat_khau_ma_hoa"].ToString(),
                VaiTroHeThong = row["vai_tro_he_thong"].ToString(),
                AvatarUrl     = row["avatar_url"]   == DBNull.Value ? null : row["avatar_url"].ToString(),
                Bio           = row["bio"]          == DBNull.Value ? null : row["bio"].ToString(),
                NgayTao       = Convert.ToDateTime(row["ngay_tao"]),
                IsBanned      = row.Table.Columns.Contains("is_banned") && Convert.ToBoolean(row["is_banned"]),
                LyDoBan       = row.Table.Columns.Contains("ly_do_ban")     && row["ly_do_ban"]     != DBNull.Value ? row["ly_do_ban"].ToString()     : null,
                ThoiGianBan   = row.Table.Columns.Contains("thoi_gian_ban") && row["thoi_gian_ban"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["thoi_gian_ban"]) : null
            };
        }
    }
}
