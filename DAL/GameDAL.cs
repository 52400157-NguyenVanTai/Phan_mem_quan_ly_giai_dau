using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// CRUD quản lý danh mục game (TRO_CHOI) — Module 2 Trụ cột 5.
    /// Luật: Không được XÓA game đang có giải đấu. Chỉ được ẨN (is_active = 0).
    /// </summary>
    public class GameDAL
    {
        public List<Dictionary<string, object>> LayTatCaGame(bool baoCaInactive = false)
        {
            string query = @"
SELECT
    tc.ma_tro_choi,
    tc.ten_game,
    tc.the_loai,
    ISNULL(tc.is_active, 1) AS is_active,
    (SELECT COUNT(1) FROM GIAI_DAU g WHERE g.ma_tro_choi = tc.ma_tro_choi AND ISNULL(g.is_deleted, 0) = 0) AS so_giai_dau,
    (SELECT COUNT(1) FROM DANH_MUC_VI_TRI vt WHERE vt.ma_tro_choi = tc.ma_tro_choi) AS so_vi_tri
FROM TRO_CHOI tc";

            if (!baoCaInactive)
            {
                query += " WHERE ISNULL(tc.is_active, 1) = 1";
            }

            query += " ORDER BY tc.ma_tro_choi DESC;";

            return ToList(DataProvider.ExecuteQuery(query));
        }

        public bool GameTonTai(int maGame)
        {
            object r = DataProvider.ExecuteScalar(
                "SELECT COUNT(1) FROM TRO_CHOI WHERE ma_tro_choi = @Id",
                new[] { new SqlParameter("@Id", SqlDbType.Int) { Value = maGame } });
            return Convert.ToInt32(r) > 0;
        }

        public bool GameDangCoGiaiDau(int maGame)
        {
            const string query = @"
SELECT COUNT(1)
FROM GIAI_DAU
WHERE ma_tro_choi = @MaGame
  AND ISNULL(is_deleted, 0) = 0
  AND trang_thai NOT IN ('khoa', 'ket_thuc');";
            object r = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaGame", SqlDbType.Int) { Value = maGame }
            });
            return Convert.ToInt32(r) > 0;
        }

        public bool TenGameDaTonTai(string tenGame, int? boQuaMaGame = null)
        {
            string query = "SELECT COUNT(1) FROM TRO_CHOI WHERE ten_game = @TenGame";
            List<SqlParameter> ps = new List<SqlParameter>
            {
                new SqlParameter("@TenGame", SqlDbType.NVarChar) { Value = tenGame.Trim() }
            };
            if (boQuaMaGame.HasValue)
            {
                query += " AND ma_tro_choi <> @BoQua";
                ps.Add(new SqlParameter("@BoQua", SqlDbType.Int) { Value = boQuaMaGame.Value });
            }
            object r = DataProvider.ExecuteScalar(query, ps.ToArray());
            return Convert.ToInt32(r) > 0;
        }

        public int ThemGame(string tenGame, string theLoai)
        {
            const string query = @"
INSERT INTO TRO_CHOI(ten_game, the_loai, is_active)
OUTPUT INSERTED.ma_tro_choi
VALUES(@TenGame, @TheLoai, 1);";
            object r = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@TenGame", SqlDbType.NVarChar) { Value = tenGame.Trim() },
                new SqlParameter("@TheLoai", SqlDbType.NVarChar) { Value = theLoai.Trim() }
            });
            return Convert.ToInt32(r);
        }

        public bool SuaGame(int maGame, string tenGame, string theLoai)
        {
            int affected = DataProvider.ExecuteNonQuery(@"
UPDATE TRO_CHOI
SET ten_game = @TenGame, the_loai = @TheLoai
WHERE ma_tro_choi = @MaGame;",
                new[]
                {
                    new SqlParameter("@MaGame", SqlDbType.Int) { Value = maGame },
                    new SqlParameter("@TenGame", SqlDbType.NVarChar) { Value = tenGame.Trim() },
                    new SqlParameter("@TheLoai", SqlDbType.NVarChar) { Value = theLoai.Trim() }
                });
            return affected > 0;
        }

        /// <summary>
        /// Ẩn game (is_active = 0) — dùng thay vì xóa để bảo toàn lịch sử.
        /// </summary>
        public bool AnGame(int maGame)
        {
            int affected = DataProvider.ExecuteNonQuery(@"
UPDATE TRO_CHOI SET is_active = 0 WHERE ma_tro_choi = @MaGame;",
                new[]
                {
                    new SqlParameter("@MaGame", SqlDbType.Int) { Value = maGame }
                });
            return affected > 0;
        }

        /// <summary>
        /// Kích hoạt lại game đã ẩn.
        /// </summary>
        public bool KichHoatGame(int maGame)
        {
            int affected = DataProvider.ExecuteNonQuery(@"
UPDATE TRO_CHOI SET is_active = 1 WHERE ma_tro_choi = @MaGame;",
                new[]
                {
                    new SqlParameter("@MaGame", SqlDbType.Int) { Value = maGame }
                });
            return affected > 0;
        }

        private static List<Dictionary<string, object>> ToList(DataTable dt)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                Dictionary<string, object> item = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    item[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                }
                list.Add(item);
            }
            return list;
        }
    }
}
