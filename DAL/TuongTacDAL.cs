using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// DAL cho tương tác người dùng với giải đấu: Like và Follow.
    /// Sử dụng UPSERT (MERGE) để đảm bảo mỗi cặp user-giai chỉ có 1 dòng.
    /// </summary>
    public class TuongTacDAL
    {
        // ---- Lấy trạng thái của 1 user với 1 giải ----
        public Dictionary<string, object> LayTrangThai(int maNguoiDung, int maGiaiDau)
        {
            const string query = @"
SELECT da_like, dang_theo_doi
FROM TUONG_TAC_GIAI_DAU
WHERE ma_nguoi_dung = @MaNguoiDung AND ma_giai_dau = @MaGiaiDau;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung },
                new SqlParameter("@MaGiaiDau",   SqlDbType.Int) { Value = maGiaiDau }
            });

            if (dt.Rows.Count == 0)
                return new Dictionary<string, object> { ["da_like"] = false, ["dang_theo_doi"] = false };

            return new Dictionary<string, object>
            {
                ["da_like"]       = Convert.ToBoolean(dt.Rows[0]["da_like"]),
                ["dang_theo_doi"] = Convert.ToBoolean(dt.Rows[0]["dang_theo_doi"])
            };
        }

        // ---- Lấy số like + follow tổng hợp của 1 giải ----
        public Dictionary<string, object> LayTongHop(int maGiaiDau)
        {
            const string query = @"
SELECT ISNULL(tong_like, 0) AS tong_like, ISNULL(tong_theo_doi, 0) AS tong_theo_doi
FROM VW_TUONG_TAC_TONG_HOP
WHERE ma_giai_dau = @MaGiaiDau;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int) { Value = maGiaiDau }
            });

            if (dt.Rows.Count == 0)
                return new Dictionary<string, object> { ["tong_like"] = 0, ["tong_theo_doi"] = 0 };

            return new Dictionary<string, object>
            {
                ["tong_like"]      = Convert.ToInt32(dt.Rows[0]["tong_like"]),
                ["tong_theo_doi"]  = Convert.ToInt32(dt.Rows[0]["tong_theo_doi"])
            };
        }

        // ---- Toggle Like (UPSERT) ----
        /// <summary>Bật/tắt like. Trả về trạng thái mới (true = đã like).</summary>
        public bool ToggleLike(int maNguoiDung, int maGiaiDau)
        {
            // Lấy trạng thái hiện tại
            bool currentLike = false;
            var current = LayTrangThai(maNguoiDung, maGiaiDau);
            if (current.ContainsKey("da_like")) currentLike = Convert.ToBoolean(current["da_like"]);
            bool newLike = !currentLike;

            const string upsert = @"
MERGE TUONG_TAC_GIAI_DAU AS target
USING (SELECT @MaNguoiDung AS ma_nguoi_dung, @MaGiaiDau AS ma_giai_dau) AS src
ON target.ma_nguoi_dung = src.ma_nguoi_dung AND target.ma_giai_dau = src.ma_giai_dau
WHEN MATCHED THEN
    UPDATE SET da_like = @DaLike
WHEN NOT MATCHED THEN
    INSERT (ma_nguoi_dung, ma_giai_dau, da_like, dang_theo_doi)
    VALUES (@MaNguoiDung, @MaGiaiDau, @DaLike, 0);";

            DataProvider.ExecuteNonQuery(upsert, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung },
                new SqlParameter("@MaGiaiDau",   SqlDbType.Int) { Value = maGiaiDau },
                new SqlParameter("@DaLike",       SqlDbType.Bit) { Value = newLike }
            });

            return newLike;
        }

        // ---- Toggle Follow (UPSERT) ----
        /// <summary>Bật/tắt theo dõi. Trả về trạng thái mới (true = đang theo dõi).</summary>
        public bool ToggleFollow(int maNguoiDung, int maGiaiDau)
        {
            bool currentFollow = false;
            var current = LayTrangThai(maNguoiDung, maGiaiDau);
            if (current.ContainsKey("dang_theo_doi")) currentFollow = Convert.ToBoolean(current["dang_theo_doi"]);
            bool newFollow = !currentFollow;

            const string upsert = @"
MERGE TUONG_TAC_GIAI_DAU AS target
USING (SELECT @MaNguoiDung AS ma_nguoi_dung, @MaGiaiDau AS ma_giai_dau) AS src
ON target.ma_nguoi_dung = src.ma_nguoi_dung AND target.ma_giai_dau = src.ma_giai_dau
WHEN MATCHED THEN
    UPDATE SET dang_theo_doi = @DangTheoDoi
WHEN NOT MATCHED THEN
    INSERT (ma_nguoi_dung, ma_giai_dau, da_like, dang_theo_doi)
    VALUES (@MaNguoiDung, @MaGiaiDau, 0, @DangTheoDoi);";

            DataProvider.ExecuteNonQuery(upsert, new[]
            {
                new SqlParameter("@MaNguoiDung",  SqlDbType.Int) { Value = maNguoiDung },
                new SqlParameter("@MaGiaiDau",    SqlDbType.Int) { Value = maGiaiDau },
                new SqlParameter("@DangTheoDoi",  SqlDbType.Bit) { Value = newFollow }
            });

            return newFollow;
        }

        // ---- Lấy danh sách giải đang theo dõi của một user ----
        public List<Dictionary<string, object>> LayGiaiDangTheoDoi(int maNguoiDung)
        {
            const string query = @"
SELECT
    g.ma_giai_dau, g.ten_giai_dau, g.trang_thai, g.tong_giai_thuong,
    tc.ma_tro_choi, tc.ten_game
FROM TUONG_TAC_GIAI_DAU tt
JOIN GIAI_DAU g ON g.ma_giai_dau = tt.ma_giai_dau
LEFT JOIN TRO_CHOI tc ON tc.ma_tro_choi = g.ma_tro_choi
WHERE tt.ma_nguoi_dung = @MaNguoiDung
  AND tt.dang_theo_doi = 1
  AND ISNULL(g.is_deleted, 0) = 0
ORDER BY tt.thoi_gian_tao DESC;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung }
            });

            var list = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var d = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                    d[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                list.Add(d);
            }
            return list;
        }
    }
}
