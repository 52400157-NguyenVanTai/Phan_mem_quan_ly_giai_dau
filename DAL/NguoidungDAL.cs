using DTO;
using System;
using System.Data.SqlClient;

namespace DAL
{
    public class NguoidungDAL
    {
        public NguoidungDTO GetByUsernameOrEmail(string usernameOrEmail)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT ma_nguoi_dung, ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong, avatar_url, bio, is_banned, ly_do_ban, thoi_gian_ban, ma_admin_ban, ngay_tao
                                                        FROM NGUOI_DUNG
                                                        WHERE ten_dang_nhap = @value OR email = @value", connection))
            {
                command.CommandTimeout = 120;
                command.Parameters.AddWithValue("@value", usernameOrEmail);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapUser(reader) : null;
                }
            }
        }

        public NguoidungDTO GetByEmail(string email)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT ma_nguoi_dung, ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong, avatar_url, bio, is_banned, ly_do_ban, thoi_gian_ban, ma_admin_ban, ngay_tao
                                                        FROM NGUOI_DUNG
                                                        WHERE email = @email", connection))
            {
                command.CommandTimeout = 120;
                command.Parameters.AddWithValue("@email", email);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapUser(reader) : null;
                }
            }
        }

        public NguoidungDTO GetById(int maNguoiDung)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT ma_nguoi_dung, ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong, avatar_url, bio, is_banned, ly_do_ban, thoi_gian_ban, ma_admin_ban, ngay_tao
                                                        FROM NGUOI_DUNG
                                                        WHERE ma_nguoi_dung = @id", connection))
            {
                command.CommandTimeout = 120;
                command.Parameters.AddWithValue("@id", maNguoiDung);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapUser(reader) : null;
                }
            }
        }

        public bool ExistsUsername(string username, int? exceptUserId = null)
        {
            return Exists("ten_dang_nhap", username, exceptUserId);
        }

        public bool ExistsEmail(string email, int? exceptUserId = null)
        {
            return Exists("email", email, exceptUserId);
        }

        public int Insert(NguoidungDTO user)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"INSERT INTO NGUOI_DUNG (ten_dang_nhap, email, mat_khau_ma_hoa, vai_tro_he_thong)
                                                        OUTPUT INSERTED.ma_nguoi_dung
                                                        VALUES (@username, @email, @passwordHash, @role)", connection))
            {
                command.CommandTimeout = 120;
                command.Parameters.AddWithValue("@username", user.ten_dang_nhap);
                command.Parameters.AddWithValue("@email", user.email);
                command.Parameters.AddWithValue("@passwordHash", user.mat_khau_ma_hoa);
                command.Parameters.AddWithValue("@role", user.vai_tro_he_thong);
                connection.Open();

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void UpdateProfile(NguoidungDTO user)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"UPDATE NGUOI_DUNG
                                                        SET ten_dang_nhap = @username,
                                                            email = @email,
                                                            avatar_url = @avatarUrl,
                                                            bio = @bio
                                                        WHERE ma_nguoi_dung = @id", connection))
            {
                command.CommandTimeout = 120;
                command.Parameters.AddWithValue("@username", user.ten_dang_nhap);
                command.Parameters.AddWithValue("@email", user.email);
                command.Parameters.AddWithValue("@avatarUrl", (object)user.avatar_url ?? DBNull.Value);
                command.Parameters.AddWithValue("@bio", (object)user.bio ?? DBNull.Value);
                command.Parameters.AddWithValue("@id", user.ma_nguoi_dung);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdatePassword(int maNguoiDung, string passwordHash)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand("UPDATE NGUOI_DUNG SET mat_khau_ma_hoa = @passwordHash WHERE ma_nguoi_dung = @id", connection))
            {
                command.CommandTimeout = 120;
                command.Parameters.AddWithValue("@passwordHash", passwordHash);
                command.Parameters.AddWithValue("@id", maNguoiDung);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private bool Exists(string columnName, string value, int? exceptUserId)
        {
            string query = "SELECT COUNT(1) FROM NGUOI_DUNG WHERE " + columnName + " = @value";
            if (exceptUserId.HasValue)
            {
                query += " AND ma_nguoi_dung <> @exceptUserId";
            }

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.CommandTimeout = 120;
                command.Parameters.AddWithValue("@value", value);
                if (exceptUserId.HasValue)
                {
                    command.Parameters.AddWithValue("@exceptUserId", exceptUserId.Value);
                }

                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private NguoidungDTO MapUser(SqlDataReader reader)
        {
            return new NguoidungDTO
            {
                ma_nguoi_dung = Convert.ToInt32(reader["ma_nguoi_dung"]),
                ten_dang_nhap = reader["ten_dang_nhap"].ToString(),
                email = reader["email"].ToString(),
                mat_khau_ma_hoa = reader["mat_khau_ma_hoa"].ToString(),
                vai_tro_he_thong = reader["vai_tro_he_thong"].ToString(),
                avatar_url = reader["avatar_url"] == DBNull.Value ? null : reader["avatar_url"].ToString(),
                bio = reader["bio"] == DBNull.Value ? null : reader["bio"].ToString(),
                is_banned = Convert.ToBoolean(reader["is_banned"]),
                ly_do_ban = reader["ly_do_ban"] == DBNull.Value ? null : reader["ly_do_ban"].ToString(),
                thoi_gian_ban = reader["thoi_gian_ban"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["thoi_gian_ban"]),
                ma_admin_ban = reader["ma_admin_ban"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ma_admin_ban"]),
                ngay_tao = Convert.ToDateTime(reader["ngay_tao"])
            };
        }
    }
}
