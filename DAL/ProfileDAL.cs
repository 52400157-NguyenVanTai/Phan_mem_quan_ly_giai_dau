using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DTO;

namespace DAL
{
    public class ProfileDAL
    {
        public List<TroChoiDTO> LayDanhSachTroChoi()
        {
            const string query = "SELECT ma_tro_choi, ten_game, the_loai FROM TRO_CHOI ORDER BY ten_game";
            DataTable dt = DataProvider.ExecuteQuery(query);
            var list = new List<TroChoiDTO>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new TroChoiDTO
                {
                    MaTroChoi = Convert.ToInt32(row["ma_tro_choi"]),
                    TenGame = row["ten_game"].ToString(),
                    TheLoai = row["the_loai"].ToString()
                });
            }
            return list;
        }

        public List<ViTriDTO> LayViTriTheoGame(int maTroChoi)
        {
            const string query = @"SELECT ma_vi_tri, ma_tro_choi, ten_vi_tri, ky_hieu, loai_vi_tri
FROM DANH_MUC_VI_TRI
WHERE ma_tro_choi = @MaTroChoi
ORDER BY loai_vi_tri, ten_vi_tri";
            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaTroChoi", SqlDbType.Int){ Value = maTroChoi }
            });

            var list = new List<ViTriDTO>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ViTriDTO
                {
                    MaViTri = Convert.ToInt32(row["ma_vi_tri"]),
                    MaTroChoi = Convert.ToInt32(row["ma_tro_choi"]),
                    TenViTri = row["ten_vi_tri"].ToString(),
                    KyHieu = row["ky_hieu"] == DBNull.Value ? null : row["ky_hieu"].ToString(),
                    LoaiViTri = row["loai_vi_tri"].ToString()
                });
            }
            return list;
        }

        public HoSoInGameDTO LayHoSo(int maNguoiDung, int maTroChoi)
        {
            const string query = @"
SELECT TOP 1 ma_ho_so, ma_nguoi_dung, ma_tro_choi, in_game_id, in_game_name, ma_vi_tri_so_truong, ngay_cap_nhat
FROM HO_SO_IN_GAME
WHERE ma_nguoi_dung = @MaNguoiDung AND ma_tro_choi = @MaTroChoi;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung },
                new SqlParameter("@MaTroChoi", SqlDbType.Int){ Value = maTroChoi }
            });

            if (dt.Rows.Count == 0)
            {
                return null;
            }

            DataRow row = dt.Rows[0];
            return new HoSoInGameDTO
            {
                MaHoSo = Convert.ToInt32(row["ma_ho_so"]),
                MaNguoiDung = Convert.ToInt32(row["ma_nguoi_dung"]),
                MaTroChoi = Convert.ToInt32(row["ma_tro_choi"]),
                InGameId = row["in_game_id"].ToString(),
                InGameName = row["in_game_name"].ToString(),
                MaViTriSoTruong = Convert.ToInt32(row["ma_vi_tri_so_truong"]),
                NgayCapNhat = row["ngay_cap_nhat"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["ngay_cap_nhat"])
            };
        }

        public bool DaTonTaiHoSo(int maNguoiDung, int maTroChoi)
        {
            const string query = "SELECT COUNT(1) FROM HO_SO_IN_GAME WHERE ma_nguoi_dung = @MaNguoiDung AND ma_tro_choi = @MaTroChoi";
            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung },
                new SqlParameter("@MaTroChoi", SqlDbType.Int){ Value = maTroChoi }
            });
            return Convert.ToInt32(result) > 0;
        }

        public int TaoHoSo(HoSoInGameDTO dto)
        {
            const string query = @"
INSERT INTO HO_SO_IN_GAME(ma_nguoi_dung, ma_tro_choi, in_game_id, in_game_name, ma_vi_tri_so_truong)
OUTPUT INSERTED.ma_ho_so
VALUES(@MaNguoiDung, @MaTroChoi, @InGameId, @InGameName, @MaViTriSoTruong);";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = dto.MaNguoiDung },
                new SqlParameter("@MaTroChoi", SqlDbType.Int){ Value = dto.MaTroChoi },
                new SqlParameter("@InGameId", SqlDbType.NVarChar){ Value = dto.InGameId.Trim() },
                new SqlParameter("@InGameName", SqlDbType.NVarChar){ Value = dto.InGameName.Trim() },
                new SqlParameter("@MaViTriSoTruong", SqlDbType.Int){ Value = dto.MaViTriSoTruong }
            });

            return Convert.ToInt32(result);
        }

        public bool CapNhatHoSo(HoSoInGameDTO dto)
        {
            const string query = @"
UPDATE HO_SO_IN_GAME
SET in_game_id = @InGameId,
    in_game_name = @InGameName,
    ma_vi_tri_so_truong = @MaViTriSoTruong,
    ngay_cap_nhat = GETDATE()
WHERE ma_nguoi_dung = @MaNguoiDung
  AND ma_tro_choi = @MaTroChoi";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = dto.MaNguoiDung },
                new SqlParameter("@MaTroChoi", SqlDbType.Int){ Value = dto.MaTroChoi },
                new SqlParameter("@InGameId", SqlDbType.NVarChar){ Value = dto.InGameId.Trim() },
                new SqlParameter("@InGameName", SqlDbType.NVarChar){ Value = dto.InGameName.Trim() },
                new SqlParameter("@MaViTriSoTruong", SqlDbType.Int){ Value = dto.MaViTriSoTruong }
            });

            return affected > 0;
        }
    }
}
