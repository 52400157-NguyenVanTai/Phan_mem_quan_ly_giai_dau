using DTO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DAL
{
    public class HoSoThiDauDAL
    {
        public List<TroChoiDTO> GetTroChoi()
        {
            List<TroChoiDTO> items = new List<TroChoiDTO>();
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand("SELECT ma_tro_choi, ten_game, the_loai FROM DANH_MUC_TRO_CHOI WHERE is_active = 1 ORDER BY ten_game", connection))
            {
                command.CommandTimeout = 120;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new TroChoiDTO
                        {
                            ma_tro_choi = Convert.ToInt32(reader["ma_tro_choi"]),
                            ten_game = reader["ten_game"].ToString(),
                            the_loai = reader["the_loai"].ToString()
                        });
                    }
                }
            }
            return items;
        }

        public List<ViTriDTO> GetViTri(int? maTroChoi, string loaiViTri)
        {
            List<ViTriDTO> items = new List<ViTriDTO>();
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT ma_vi_tri, ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri
                                                        FROM DANH_MUC_VI_TRI
                                                        WHERE (@loaiViTri IS NULL OR loai_vi_tri = @loaiViTri)
                                                          AND (ma_tro_choi IS NULL OR @maTroChoi IS NULL OR ma_tro_choi = @maTroChoi)
                                                        ORDER BY loai_vi_tri, ten_vi_tri", connection))
            {
                command.CommandTimeout = 120;
                command.Parameters.AddWithValue("@maTroChoi", (object)maTroChoi ?? DBNull.Value);
                command.Parameters.AddWithValue("@loaiViTri", string.IsNullOrWhiteSpace(loaiViTri) ? (object)DBNull.Value : loaiViTri);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new ViTriDTO
                        {
                            ma_vi_tri = Convert.ToInt32(reader["ma_vi_tri"]),
                            ma_tro_choi = reader["ma_tro_choi"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ma_tro_choi"]),
                            ten_vi_tri = reader["ten_vi_tri"].ToString(),
                            ky_hieu = reader["ky_hieu"].ToString(),
                            loai_vi_tri = reader["loai_vi_tri"].ToString()
                        });
                    }
                }
            }
            return items;
        }

        public HoSoThiDauDTO GetByUserId(int maNguoiDung)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT TOP 1 h.ma_ho_so, h.ma_nguoi_dung, h.ma_tro_choi, t.ten_game,
                                                               h.in_game_id, h.in_game_name, v.loai_vi_tri,
                                                               h.ma_vi_tri_so_truong, v.ten_vi_tri, h.thanh_tich, h.ngay_cap_nhat
                                                        FROM HO_SO_IN_GAME h
                                                        INNER JOIN DANH_MUC_TRO_CHOI t ON h.ma_tro_choi = t.ma_tro_choi
                                                        LEFT JOIN DANH_MUC_VI_TRI v ON h.ma_vi_tri_so_truong = v.ma_vi_tri
                                                        WHERE h.ma_nguoi_dung = @maNguoiDung
                                                        ORDER BY h.ngay_cap_nhat DESC", connection))
            {
                command.CommandTimeout = 120;
                command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapHoSo(reader) : null;
                }
            }
        }

        public List<HoSoThiDauDTO> GetAllByUserId(int maNguoiDung)
        {
            List<HoSoThiDauDTO> items = new List<HoSoThiDauDTO>();
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT h.ma_ho_so, h.ma_nguoi_dung, h.ma_tro_choi, t.ten_game,
                                                               h.in_game_id, h.in_game_name, v.loai_vi_tri,
                                                               h.ma_vi_tri_so_truong, v.ten_vi_tri, h.thanh_tich, h.ngay_cap_nhat
                                                        FROM HO_SO_IN_GAME h
                                                        INNER JOIN DANH_MUC_TRO_CHOI t ON h.ma_tro_choi = t.ma_tro_choi
                                                        LEFT JOIN DANH_MUC_VI_TRI v ON h.ma_vi_tri_so_truong = v.ma_vi_tri
                                                        WHERE h.ma_nguoi_dung = @maNguoiDung
                                                        ORDER BY h.ngay_cap_nhat DESC, t.ten_game", connection))
            {
                command.CommandTimeout = 120;
                command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(MapHoSo(reader));
                    }
                }
            }
            return items;
        }

        public HoSoThiDauDTO GetByUserIdAndGame(int maNguoiDung, int maTroChoi)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT TOP 1 h.ma_ho_so, h.ma_nguoi_dung, h.ma_tro_choi, t.ten_game,
                                                               h.in_game_id, h.in_game_name, v.loai_vi_tri,
                                                               h.ma_vi_tri_so_truong, v.ten_vi_tri, h.thanh_tich, h.ngay_cap_nhat
                                                        FROM HO_SO_IN_GAME h
                                                        INNER JOIN DANH_MUC_TRO_CHOI t ON h.ma_tro_choi = t.ma_tro_choi
                                                        LEFT JOIN DANH_MUC_VI_TRI v ON h.ma_vi_tri_so_truong = v.ma_vi_tri
                                                        WHERE h.ma_nguoi_dung = @maNguoiDung AND h.ma_tro_choi = @maTroChoi
                                                        ORDER BY h.ngay_cap_nhat DESC", connection))
            {
                command.CommandTimeout = 120;
                command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                command.Parameters.AddWithValue("@maTroChoi", maTroChoi);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapHoSo(reader) : null;
                }
            }
        }

        public int Save(int maNguoiDung, HoSoThiDauRequestDTO request)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"IF EXISTS (SELECT 1 FROM HO_SO_IN_GAME WITH (UPDLOCK, HOLDLOCK) WHERE ma_nguoi_dung = @maNguoiDung AND ma_tro_choi = @maTroChoi)
                                                        BEGIN
                                                            UPDATE HO_SO_IN_GAME
                                                            SET in_game_id = @inGameId,
                                                                in_game_name = @inGameName,
                                                                ma_vi_tri_so_truong = @maViTri,
                                                                thanh_tich = @thanhTich,
                                                                ngay_cap_nhat = GETDATE()
                                                            WHERE ma_nguoi_dung = @maNguoiDung AND ma_tro_choi = @maTroChoi;
                                                            SELECT ma_ho_so FROM HO_SO_IN_GAME WHERE ma_nguoi_dung = @maNguoiDung AND ma_tro_choi = @maTroChoi;
                                                        END
                                                        ELSE
                                                        BEGIN
                                                            INSERT INTO HO_SO_IN_GAME (ma_nguoi_dung, ma_tro_choi, in_game_id, in_game_name, ma_vi_tri_so_truong, thanh_tich)
                                                            OUTPUT INSERTED.ma_ho_so
                                                            VALUES (@maNguoiDung, @maTroChoi, @inGameId, @inGameName, @maViTri, @thanhTich);
                                                        END", connection))
            {
                command.CommandTimeout = 120;
                command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                command.Parameters.AddWithValue("@maTroChoi", request.ma_tro_choi);
                command.Parameters.AddWithValue("@inGameId", string.IsNullOrWhiteSpace(request.in_game_id) ? (object)DBNull.Value : request.in_game_id.Trim());
                command.Parameters.AddWithValue("@inGameName", string.IsNullOrWhiteSpace(request.in_game_name) ? (object)DBNull.Value : request.in_game_name.Trim());
                command.Parameters.AddWithValue("@maViTri", (object)request.ma_vi_tri_so_truong ?? DBNull.Value);
                command.Parameters.AddWithValue("@thanhTich", string.IsNullOrWhiteSpace(request.thanh_tich) ? (object)DBNull.Value : request.thanh_tich.Trim());
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void Delete(int maNguoiDung, int maTroChoi)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int? maHoSo = null;
                        using (SqlCommand command = new SqlCommand(@"SELECT ma_ho_so
                                                                    FROM HO_SO_IN_GAME
                                                                    WHERE ma_nguoi_dung = @maNguoiDung AND ma_tro_choi = @maTroChoi", connection, transaction))
                        {
                            command.CommandTimeout = 120;
                            command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                            command.Parameters.AddWithValue("@maTroChoi", maTroChoi);

                            object value = command.ExecuteScalar();
                            if (value != null && value != DBNull.Value)
                            {
                                maHoSo = Convert.ToInt32(value);
                            }
                        }

                        if (!maHoSo.HasValue)
                        {
                            transaction.Commit();
                            return;
                        }

                        using (SqlCommand command = new SqlCommand(@"UPDATE XIN_GIA_NHAP
                                                                    SET ma_ho_so = NULL
                                                                    WHERE ma_ho_so = @maHoSo", connection, transaction))
                        {
                            command.CommandTimeout = 120;
                            command.Parameters.AddWithValue("@maHoSo", maHoSo.Value);
                            command.ExecuteNonQuery();
                        }

                        using (SqlCommand command = new SqlCommand(@"DELETE FROM HO_SO_IN_GAME
                                                                    WHERE ma_ho_so = @maHoSo
                                                                      AND ma_nguoi_dung = @maNguoiDung
                                                                      AND ma_tro_choi = @maTroChoi", connection, transaction))
                        {
                            command.CommandTimeout = 120;
                            command.Parameters.AddWithValue("@maHoSo", maHoSo.Value);
                            command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                            command.Parameters.AddWithValue("@maTroChoi", maTroChoi);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private HoSoThiDauDTO MapHoSo(SqlDataReader reader)
        {
            return new HoSoThiDauDTO
            {
                ma_ho_so = Convert.ToInt32(reader["ma_ho_so"]),
                ma_nguoi_dung = Convert.ToInt32(reader["ma_nguoi_dung"]),
                ma_tro_choi = Convert.ToInt32(reader["ma_tro_choi"]),
                ten_game = reader["ten_game"].ToString(),
                in_game_id = reader["in_game_id"] == DBNull.Value ? null : reader["in_game_id"].ToString(),
                in_game_name = reader["in_game_name"] == DBNull.Value ? null : reader["in_game_name"].ToString(),
                loai_vi_tri = reader["loai_vi_tri"] == DBNull.Value ? null : reader["loai_vi_tri"].ToString(),
                ma_vi_tri_so_truong = reader["ma_vi_tri_so_truong"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ma_vi_tri_so_truong"]),
                ten_vi_tri = reader["ten_vi_tri"] == DBNull.Value ? null : reader["ten_vi_tri"].ToString(),
                thanh_tich = reader["thanh_tich"] == DBNull.Value ? null : reader["thanh_tich"].ToString(),
                ngay_cap_nhat = Convert.ToDateTime(reader["ngay_cap_nhat"])
            };
        }
    }
}
