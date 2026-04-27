using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class AdminDAL
    {
        // ================================================================
        // MODULE 1: GLOBAL DASHBOARD
        // ================================================================

        public Dictionary<string, object> LayThongKeDashboard()
        {
            const string query = "SELECT * FROM VW_DASHBOARD_STATS;";
            DataTable dt = DataProvider.ExecuteQuery(query);

            if (dt.Rows.Count == 0)
            {
                return new Dictionary<string, object>();
            }

            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (DataColumn col in dt.Columns)
            {
                result[col.ColumnName] = dt.Rows[0][col] == DBNull.Value ? 0 : dt.Rows[0][col];
            }
            return result;
        }

        public List<Dictionary<string, object>> LayGiaiChoXetDuyet()
        {
            const string query = @"
SELECT TOP 20
    g.ma_giai_dau,
    g.ten_giai_dau,
    g.trang_thai,
    g.ngay_bat_dau,
    g.ngay_ket_thuc,
    g.tong_giai_thuong,
    nd.ten_dang_nhap AS ten_nguoi_tao,
    tc.ten_game
FROM GIAI_DAU g
LEFT JOIN NGUOI_DUNG nd ON nd.ma_nguoi_dung = g.ma_nguoi_tao
LEFT JOIN TRO_CHOI tc ON tc.ma_tro_choi = g.ma_tro_choi
WHERE g.trang_thai = 'cho_xet_duyet'
  AND ISNULL(g.is_deleted, 0) = 0
ORDER BY g.ma_giai_dau DESC;";
            return ToList(DataProvider.ExecuteQuery(query));
        }

        public List<Dictionary<string, object>> LayKhieuNaiChoXuLy()
        {
            const string query = @"
SELECT TOP 20
    kn.ma_khieu_nai,
    kn.ma_tran,
    kn.ma_nhom,
    kn.noi_dung,
    kn.trang_thai,
    kn.thoi_gian_tao,
    nd.ten_dang_nhap AS ten_nguoi_gui,
    n.ten_nhom
FROM KHIEU_NAI_KET_QUA kn
LEFT JOIN NGUOI_DUNG nd ON nd.ma_nguoi_dung = kn.ma_nguoi_gui
LEFT JOIN NHOM_DOI n ON n.ma_nhom = kn.ma_nhom
WHERE kn.trang_thai = 'cho_xu_ly'
ORDER BY kn.thoi_gian_tao DESC;";
            return ToList(DataProvider.ExecuteQuery(query));
        }

        // ================================================================
        // MODULE 3: QUẢN LÝ NGƯỜI DÙNG & BAN/UNBAN
        // ================================================================

        public List<Dictionary<string, object>> TimKiemNguoiDung(string tuKhoa, bool? isBanned, int top = 50)
        {
            string filter = "";
            List<SqlParameter> ps = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                filter += " AND (nd.ten_dang_nhap LIKE @TuKhoa OR nd.email LIKE @TuKhoa)";
                ps.Add(new SqlParameter("@TuKhoa", SqlDbType.NVarChar) { Value = "%" + tuKhoa.Trim() + "%" });
            }
            if (isBanned.HasValue)
            {
                filter += " AND ISNULL(nd.is_banned, 0) = @IsBanned";
                ps.Add(new SqlParameter("@IsBanned", SqlDbType.Bit) { Value = isBanned.Value ? 1 : 0 });
            }

            string query = @"
SELECT TOP " + Math.Max(1, Math.Min(top, 200)) + @"
    nd.ma_nguoi_dung,
    nd.ten_dang_nhap,
    nd.email,
    nd.vai_tro_he_thong,
    nd.ngay_tao,
    ISNULL(nd.is_banned, 0) AS is_banned,
    nd.ly_do_ban,
    nd.thoi_gian_ban,
    ban_admin.ten_dang_nhap AS ten_admin_ban
FROM NGUOI_DUNG nd
LEFT JOIN NGUOI_DUNG ban_admin ON ban_admin.ma_nguoi_dung = nd.ma_admin_ban
WHERE 1 = 1" + filter + @"
ORDER BY nd.ma_nguoi_dung DESC;";

            return ToList(DataProvider.ExecuteQuery(query, ps.Count > 0 ? ps.ToArray() : null));
        }

        public bool NguoiDungTonTai(int maNguoiDung)
        {
            object r = DataProvider.ExecuteScalar(
                "SELECT COUNT(1) FROM NGUOI_DUNG WHERE ma_nguoi_dung = @Id",
                new[] { new SqlParameter("@Id", SqlDbType.Int) { Value = maNguoiDung } });
            return Convert.ToInt32(r) > 0;
        }

        /// <summary>
        /// Lấy danh sách các giải đang chạy mà user đang tham gia (trong DOI_HINH_THI_DAU).
        /// Dùng để xác định giải nào cần thông báo cho BTC khi ban user.
        /// </summary>
        public List<Dictionary<string, object>> LayGiaiDangThamGia(int maNguoiDung)
        {
            const string query = @"
SELECT DISTINCT
    g.ma_giai_dau,
    g.ten_giai_dau,
    n.ten_nhom,
    qt.ma_nguoi_dung AS ma_btc
FROM DOI_HINH_THI_DAU dh
JOIN THAM_GIA_GIAI tgg ON tgg.ma_tham_gia = dh.ma_tham_gia
JOIN GIAI_DAU g ON g.ma_giai_dau = tgg.ma_giai_dau
JOIN NHOM_DOI n ON n.ma_nhom = tgg.ma_nhom
JOIN QUAN_TRI_GIAI_DAU qt ON qt.ma_giai_dau = g.ma_giai_dau AND qt.vai_tro_giai = 'ban_to_chuc'
WHERE dh.ma_nguoi_dung = @MaNguoiDung
  AND g.trang_thai IN ('chuan_bi_dien_ra', 'dang_dien_ra')
  AND ISNULL(g.is_deleted, 0) = 0;";
            return ToList(DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung }
            }));
        }

        /// <summary>
        /// Lấy danh sách thành viên đang thi đấu của một đội (nhóm) trong tất cả giải đang chạy.
        /// Dùng khi ban cả đội.
        /// </summary>
        public List<int> LayThanhVienDangThiDauCuaDoi(int maDoi)
        {
            const string query = @"
SELECT DISTINCT dh.ma_nguoi_dung
FROM DOI_HINH_THI_DAU dh
JOIN THAM_GIA_GIAI tgg ON tgg.ma_tham_gia = dh.ma_tham_gia
JOIN GIAI_DAU g ON g.ma_giai_dau = tgg.ma_giai_dau
JOIN NHOM_DOI nd ON nd.ma_nhom = tgg.ma_nhom
WHERE nd.ma_doi = @MaDoi
  AND g.trang_thai IN ('chuan_bi_dien_ra', 'dang_dien_ra')
  AND ISNULL(g.is_deleted, 0) = 0;";
            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaDoi", SqlDbType.Int) { Value = maDoi }
            });

            List<int> list = new List<int>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(Convert.ToInt32(row["ma_nguoi_dung"]));
            }
            return list;
        }

        public void BanNguoiDungTrongTransaction(SqlConnection conn, SqlTransaction tran,
            int maNguoiDung, int maAdmin, string lyDo)
        {
            DataProvider.ExecuteNonQuery(@"
UPDATE NGUOI_DUNG
SET is_banned = 1,
    ly_do_ban = @LyDo,
    thoi_gian_ban = GETDATE(),
    ma_admin_ban = @MaAdmin
WHERE ma_nguoi_dung = @MaNguoiDung;",
                new[]
                {
                    new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung },
                    new SqlParameter("@MaAdmin", SqlDbType.Int) { Value = maAdmin },
                    new SqlParameter("@LyDo", SqlDbType.NVarChar) { Value = (object)lyDo ?? DBNull.Value }
                }, conn, tran);
        }

        public void GachKhoiDoiHinhTrongTransaction(SqlConnection conn, SqlTransaction tran, int maNguoiDung)
        {
            // Xóa khỏi DOI_HINH_THI_DAU của các giải đang chạy
            DataProvider.ExecuteNonQuery(@"
DELETE dh
FROM DOI_HINH_THI_DAU dh
JOIN THAM_GIA_GIAI tgg ON tgg.ma_tham_gia = dh.ma_tham_gia
JOIN GIAI_DAU g ON g.ma_giai_dau = tgg.ma_giai_dau
WHERE dh.ma_nguoi_dung = @MaNguoiDung
  AND g.trang_thai IN ('chuan_bi_dien_ra', 'dang_dien_ra')
  AND ISNULL(g.is_deleted, 0) = 0;",
                new[]
                {
                    new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung }
                }, conn, tran);
        }

        public void GuiThongBaoTrongTransaction(SqlConnection conn, SqlTransaction tran,
            int maNguoiNhan, string tieuDe, string noiDung)
        {
            DataProvider.ExecuteNonQuery(@"
INSERT INTO THONG_BAO(ma_nguoi_nhan, tieu_de, noi_dung, loai_thong_bao, da_doc, ngay_tao)
VALUES(@MaNguoiNhan, @TieuDe, @NoiDung, 'ban_user', 0, GETDATE());",
                new[]
                {
                    new SqlParameter("@MaNguoiNhan", SqlDbType.Int) { Value = maNguoiNhan },
                    new SqlParameter("@TieuDe", SqlDbType.NVarChar) { Value = tieuDe },
                    new SqlParameter("@NoiDung", SqlDbType.NVarChar) { Value = noiDung }
                }, conn, tran);
        }

        public bool UnbanNguoiDung(int maNguoiDung)
        {
            int affected = DataProvider.ExecuteNonQuery(@"
UPDATE NGUOI_DUNG
SET is_banned = 0,
    ly_do_ban = NULL,
    thoi_gian_ban = NULL,
    ma_admin_ban = NULL
WHERE ma_nguoi_dung = @MaNguoiDung;",
                new[]
                {
                    new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung }
                });
            return affected > 0;
        }

        // ================================================================
        // MODULE 5: HARD WIPE
        // ================================================================

        public void XoaCungGiaiDau(int maGiaiDau)
        {
            // Gọi Stored Procedure đã có sẵn trong database
            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SP_XoaXachGiaiDau", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.Add(new SqlParameter("@MaGiaiDau", SqlDbType.Int) { Value = maGiaiDau });
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool GiaiTonTai(int maGiaiDau)
        {
            object r = DataProvider.ExecuteScalar(
                "SELECT COUNT(1) FROM GIAI_DAU WHERE ma_giai_dau = @Id AND ISNULL(is_deleted, 0) = 0",
                new[] { new SqlParameter("@Id", SqlDbType.Int) { Value = maGiaiDau } });
            return Convert.ToInt32(r) > 0;
        }

        public string LayTrangThaiGiai(int maGiaiDau)
        {
            object r = DataProvider.ExecuteScalar(
                "SELECT trang_thai FROM GIAI_DAU WHERE ma_giai_dau = @Id",
                new[] { new SqlParameter("@Id", SqlDbType.Int) { Value = maGiaiDau } });
            return r == null || r == DBNull.Value ? null : r.ToString();
        }

        // ================================================================
        // HELPER
        // ================================================================

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
