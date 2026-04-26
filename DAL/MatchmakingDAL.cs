using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DTO;

namespace DAL
{
    public class MatchmakingDAL
    {
        public DataRow LayGiaiDoanTheoId(int maGiaiDoan)
        {
            const string query = "SELECT * FROM GIAI_DOAN WHERE ma_giai_doan = @MaGiaiDoan";
            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan }
            });
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        public bool GiaiDoanTruocDaKetThuc(int maGiaiDau, int thuTu)
        {
            if (thuTu <= 1) return true;

            const string query = @"
SELECT COUNT(1)
FROM GIAI_DOAN
WHERE ma_giai_dau = @MaGiaiDau
  AND thu_tu = @ThuTuTruoc
  AND trang_thai = 'ket_thuc';";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                new SqlParameter("@ThuTuTruoc", SqlDbType.Int){ Value = thuTu - 1 }
            });
            return Convert.ToInt32(result) > 0;
        }

        public List<int> LayDanhSachNhomChoGiaiDoan(int maGiaiDau, int thuTuGiaiDoan)
        {
            if (thuTuGiaiDoan <= 1)
            {
                return LayDanhSachNhomDaDuyet(maGiaiDau);
            }

            const string query = @"
SELECT TOP 2147483647 bxh.ma_nhom
FROM GIAI_DOAN gd
JOIN BANG_XEP_HANG bxh ON bxh.ma_giai_doan = gd.ma_giai_doan
WHERE gd.ma_giai_dau = @MaGiaiDau
  AND gd.thu_tu = @ThuTuTruoc
ORDER BY bxh.thu_hang_hien_tai ASC, bxh.diem_tong_ket DESC;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                new SqlParameter("@ThuTuTruoc", SqlDbType.Int){ Value = thuTuGiaiDoan - 1 }
            });

            List<int> list = new List<int>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(Convert.ToInt32(row["ma_nhom"]));
            }
            return list;
        }

        public List<Dictionary<string, object>> LayMvpTheoTranGanNhat(int maGiaiDau, int top)
        {
            const string query = @"
SELECT TOP (@Top)
       td.ma_tran,
       td.ma_giai_doan,
       ctnct.ma_nguoi_dung,
       nd.ten_dang_nhap,
       ISNULL(hsg.in_game_name, nd.ten_dang_nhap) AS ten_hien_thi,
       ctnct.so_kill,
       ctnct.so_death,
       ctnct.so_assist,
       ctnct.diem_kda_tran,
       ctnct.diem_sinh_ton
FROM CHI_TIET_NGUOI_CHOI_TRAN ctnct
JOIN TRAN_DAU td ON td.ma_tran = ctnct.ma_tran
JOIN NGUOI_DUNG nd ON nd.ma_nguoi_dung = ctnct.ma_nguoi_dung
LEFT JOIN GIAI_DAU g ON g.ma_giai_dau = td.ma_giai_dau
LEFT JOIN HO_SO_IN_GAME hsg ON hsg.ma_nguoi_dung = ctnct.ma_nguoi_dung AND hsg.ma_tro_choi = g.ma_tro_choi
WHERE td.ma_giai_dau = @MaGiaiDau
  AND ISNULL(ctnct.is_mvp_tran, 0) = 1
ORDER BY td.ma_tran DESC, ctnct.diem_kda_tran DESC;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                new SqlParameter("@Top", SqlDbType.Int){ Value = Math.Max(1, top) }
            });

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

        public Dictionary<string, object> LayMvpGiai(int maGiaiDau)
        {
            const string query = @"
SELECT TOP 1
       bxh.ma_nguoi_dung,
       nd.ten_dang_nhap,
       ISNULL(hsg.in_game_name, nd.ten_dang_nhap) AS ten_hien_thi,
       bxh.tong_kill,
       bxh.tong_death,
       bxh.tong_assist,
       bxh.diem_kda_trung_binh,
       bxh.so_lan_dat_mvp_tran
FROM BANG_XEP_HANG_CA_NHAN bxh
JOIN NGUOI_DUNG nd ON nd.ma_nguoi_dung = bxh.ma_nguoi_dung
LEFT JOIN GIAI_DAU g ON g.ma_giai_dau = bxh.ma_giai_dau
LEFT JOIN HO_SO_IN_GAME hsg ON hsg.ma_nguoi_dung = bxh.ma_nguoi_dung AND hsg.ma_tro_choi = g.ma_tro_choi
WHERE bxh.ma_giai_dau = @MaGiaiDau
ORDER BY bxh.diem_kda_trung_binh DESC, bxh.so_lan_dat_mvp_tran DESC, bxh.tong_kill DESC, bxh.ma_nguoi_dung ASC;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });

            if (dt.Rows.Count == 0)
            {
                return null;
            }

            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (DataColumn col in dt.Columns)
            {
                result[col.ColumnName] = dt.Rows[0][col] == DBNull.Value ? null : dt.Rows[0][col];
            }
            return result;
        }

        public List<Dictionary<string, object>> LayDoiHinhTieuBieu(int maGiaiDau)
        {
            const string query = @"
;WITH PlayerAgg AS
(
    SELECT ctnct.ma_nguoi_dung,
           SUM(ISNULL(ctnct.so_kill, 0)) AS tong_kill,
           SUM(ISNULL(ctnct.so_death, 0)) AS tong_death,
           SUM(ISNULL(ctnct.so_assist, 0)) AS tong_assist,
           CAST((SUM(ISNULL(ctnct.so_kill, 0)) + SUM(ISNULL(ctnct.so_assist, 0))) * 1.0 / NULLIF(CASE WHEN SUM(ISNULL(ctnct.so_death, 0)) <= 0 THEN 1 ELSE SUM(ISNULL(ctnct.so_death, 0)) END, 0) AS FLOAT) AS kda_tong
    FROM CHI_TIET_NGUOI_CHOI_TRAN ctnct
    JOIN TRAN_DAU td ON td.ma_tran = ctnct.ma_tran
    WHERE td.ma_giai_dau = @MaGiaiDau
    GROUP BY ctnct.ma_nguoi_dung
),
Ranked AS
(
    SELECT p.ma_nguoi_dung,
           nd.ten_dang_nhap,
           ISNULL(hsg.in_game_name, nd.ten_dang_nhap) AS ten_hien_thi,
           hsg.ma_vi_tri_so_truong,
           vt.ten_vi_tri,
           p.tong_kill,
           p.tong_death,
           p.tong_assist,
           p.kda_tong,
           ROW_NUMBER() OVER (
               PARTITION BY hsg.ma_vi_tri_so_truong
               ORDER BY p.kda_tong DESC, p.tong_kill DESC, p.ma_nguoi_dung ASC
           ) AS rn
    FROM PlayerAgg p
    JOIN NGUOI_DUNG nd ON nd.ma_nguoi_dung = p.ma_nguoi_dung
    JOIN GIAI_DAU g ON g.ma_giai_dau = @MaGiaiDau
    LEFT JOIN HO_SO_IN_GAME hsg ON hsg.ma_nguoi_dung = p.ma_nguoi_dung AND hsg.ma_tro_choi = g.ma_tro_choi
    LEFT JOIN DANH_MUC_VI_TRI vt ON vt.ma_vi_tri = hsg.ma_vi_tri_so_truong
    WHERE hsg.ma_vi_tri_so_truong IS NOT NULL
)
SELECT ma_nguoi_dung,
       ten_dang_nhap,
       ten_hien_thi,
       ma_vi_tri_so_truong,
       ten_vi_tri,
       tong_kill,
       tong_death,
       tong_assist,
       kda_tong
FROM Ranked
WHERE rn = 1
ORDER BY ma_vi_tri_so_truong;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });

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

        public List<Dictionary<string, object>> LayBanHuanLuyenVoDich(int maGiaiDau)
        {
            const string query = @"
;WITH Champion AS
(
    SELECT TOP 1 bxh.ma_nhom
    FROM BANG_XEP_HANG bxh
    JOIN GIAI_DOAN gd ON gd.ma_giai_doan = bxh.ma_giai_doan
    WHERE gd.ma_giai_dau = @MaGiaiDau
    ORDER BY gd.thu_tu DESC, bxh.thu_hang_hien_tai ASC, bxh.diem_tong_ket DESC
)
SELECT nd.ma_nguoi_dung,
       nd.ten_dang_nhap,
       ISNULL(hsg.in_game_name, nd.ten_dang_nhap) AS ten_hien_thi,
       tv.vai_tro_noi_bo,
       tv.phan_he,
       d.ten_doi,
       n.ten_nhom
FROM Champion c
JOIN NHOM_DOI n ON n.ma_nhom = c.ma_nhom
JOIN DOI d ON d.ma_doi = n.ma_doi
LEFT JOIN THANH_VIEN_DOI tv ON tv.ma_nhom = n.ma_nhom AND tv.trang_thai_duyet = 'da_duyet'
LEFT JOIN NGUOI_DUNG nd ON nd.ma_nguoi_dung = tv.ma_nguoi_dung
LEFT JOIN GIAI_DAU g ON g.ma_giai_dau = @MaGiaiDau
LEFT JOIN HO_SO_IN_GAME hsg ON hsg.ma_nguoi_dung = nd.ma_nguoi_dung AND hsg.ma_tro_choi = g.ma_tro_choi
WHERE (tv.phan_he = 'ban_huan_luyen' OR tv.vai_tro_noi_bo IN ('leader', 'captain'))
UNION ALL
SELECT nd.ma_nguoi_dung,
       nd.ten_dang_nhap,
       ISNULL(hsg.in_game_name, nd.ten_dang_nhap) AS ten_hien_thi,
       'manager' AS vai_tro_noi_bo,
       'ban_huan_luyen' AS phan_he,
       d.ten_doi,
       n.ten_nhom
FROM Champion c
JOIN NHOM_DOI n ON n.ma_nhom = c.ma_nhom
JOIN DOI d ON d.ma_doi = n.ma_doi
JOIN NGUOI_DUNG nd ON nd.ma_nguoi_dung = d.ma_doi_truong
LEFT JOIN GIAI_DAU g ON g.ma_giai_dau = @MaGiaiDau
LEFT JOIN HO_SO_IN_GAME hsg ON hsg.ma_nguoi_dung = nd.ma_nguoi_dung AND hsg.ma_tro_choi = g.ma_tro_choi
WHERE d.ma_doi_truong IS NOT NULL;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });

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

        public List<int> LayDanhSachNhomDaDuyet(int maGiaiDau)
        {
            const string query = @"
SELECT ma_nhom
FROM THAM_GIA_GIAI
WHERE ma_giai_dau = @MaGiaiDau
  AND trang_thai_duyet = 'da_duyet'
ORDER BY ISNULL(hat_giong, 999999), ma_nhom;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });

            List<int> list = new List<int>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(Convert.ToInt32(row["ma_nhom"]));
            }
            return list;
        }

        public int TaoTran(SqlConnection conn, SqlTransaction tran, MatchNodeDTO node)
        {
            const string query = @"
INSERT INTO TRAN_DAU
(
    ma_giai_dau, ma_giai_doan, so_vong, nhanh_dau, vong_dau, the_thuc_tran,
    thoi_gian_bat_dau, thoi_gian_ket_thuc, trang_thai,
    ma_tran_tiep_theo_thang, ma_tran_tiep_theo_thua
)
OUTPUT INSERTED.ma_tran
VALUES
(
    @MaGiaiDau, @MaGiaiDoan, @SoVong, @NhanhDau, @VongDau, @TheThucTran,
    @ThoiGianBatDau, @ThoiGianKetThuc, @TrangThai,
    @MaTranTiepTheoThang, @MaTranTiepTheoThua
);";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = node.MaGiaiDau },
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = node.MaGiaiDoan },
                new SqlParameter("@SoVong", SqlDbType.Int){ Value = node.SoVong },
                new SqlParameter("@NhanhDau", SqlDbType.NVarChar){ Value = (object)node.NhanhDau ?? DBNull.Value },
                new SqlParameter("@VongDau", SqlDbType.NVarChar){ Value = "Vòng " + node.SoVong },
                new SqlParameter("@TheThucTran", SqlDbType.NVarChar){ Value = node.TheThucTran ?? "BO1" },
                new SqlParameter("@ThoiGianBatDau", SqlDbType.DateTime){ Value = (object)node.ThoiGianBatDau ?? DBNull.Value },
                new SqlParameter("@ThoiGianKetThuc", SqlDbType.DateTime){ Value = (object)node.ThoiGianKetThuc ?? DBNull.Value },
                new SqlParameter("@TrangThai", SqlDbType.NVarChar){ Value = node.TrangThai ?? "chua_dau" },
                new SqlParameter("@MaTranTiepTheoThang", SqlDbType.Int){ Value = (object)node.MaTranTiepTheoThang ?? DBNull.Value },
                new SqlParameter("@MaTranTiepTheoThua", SqlDbType.Int){ Value = (object)node.MaTranTiepTheoThua ?? DBNull.Value }
            }, conn, tran);

            return Convert.ToInt32(result);
        }

        public void GanNhomVaoTran(SqlConnection conn, SqlTransaction tran, int maTran, int maNhom)
        {
            const string query = @"
IF NOT EXISTS (SELECT 1 FROM CHI_TIET_TRAN_DAU WHERE ma_tran = @MaTran AND ma_nhom = @MaNhom)
INSERT INTO CHI_TIET_TRAN_DAU(ma_tran, ma_nhom, diem_so, thu_hang, ket_qua)
VALUES(@MaTran, @MaNhom, 0, NULL, NULL);";

            DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran },
                new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom }
            }, conn, tran);
        }

        public void TaoBangXepHangGiaiDoanNeuChuaCo(SqlConnection conn, SqlTransaction tran, int maGiaiDoan, int maGiaiDau, List<int> dsNhom)
        {
            const string query = @"
IF NOT EXISTS (SELECT 1 FROM BANG_XEP_HANG WHERE ma_giai_doan = @MaGiaiDoan AND ma_nhom = @MaNhom)
INSERT INTO BANG_XEP_HANG(ma_giai_dau, ma_giai_doan, ma_nhom)
VALUES(@MaGiaiDau, @MaGiaiDoan, @MaNhom);";

            foreach (int maNhom in dsNhom)
            {
                DataProvider.ExecuteNonQuery(query, new[]
                {
                    new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                    new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan },
                    new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom }
                }, conn, tran);
            }
        }

        public bool DaCoTranTrongGiaiDoan(int maGiaiDoan)
        {
            const string query = "SELECT COUNT(1) FROM TRAN_DAU WHERE ma_giai_doan = @MaGiaiDoan";
            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan }
            });
            return Convert.ToInt32(result) > 0;
        }

        public void CapNhatTrangThaiGiaiDoan(SqlConnection conn, SqlTransaction tran, int maGiaiDoan, string trangThai)
        {
            DataProvider.ExecuteNonQuery("UPDATE GIAI_DOAN SET trang_thai = @TrangThai WHERE ma_giai_doan = @MaGiaiDoan", new[]
            {
                new SqlParameter("@TrangThai", SqlDbType.NVarChar){ Value = trangThai },
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan }
            }, conn, tran);
        }

        public List<MatchNodeDTO> LayTranTheoGiaiDoan(int maGiaiDoan)
        {
            const string query = @"
SELECT td.*, t1.ten_nhom AS ten_nhom_a, t2.ten_nhom AS ten_nhom_b
FROM TRAN_DAU td
OUTER APPLY (
    SELECT TOP 1 n.ten_nhom
    FROM CHI_TIET_TRAN_DAU ct
    JOIN NHOM_DOI n ON n.ma_nhom = ct.ma_nhom
    WHERE ct.ma_tran = td.ma_tran
    ORDER BY ct.ma_nhom ASC
) t1
OUTER APPLY (
    SELECT TOP 1 n.ten_nhom
    FROM CHI_TIET_TRAN_DAU ct
    JOIN NHOM_DOI n ON n.ma_nhom = ct.ma_nhom
    WHERE ct.ma_tran = td.ma_tran
      AND ct.ma_nhom NOT IN (
          SELECT TOP 1 ct2.ma_nhom
          FROM CHI_TIET_TRAN_DAU ct2
          WHERE ct2.ma_tran = td.ma_tran
          ORDER BY ct2.ma_nhom ASC
      )
    ORDER BY ct.ma_nhom ASC
) t2
WHERE td.ma_giai_doan = @MaGiaiDoan
ORDER BY td.so_vong, td.ma_tran;";
            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan }
            });

            List<MatchNodeDTO> list = new List<MatchNodeDTO>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new MatchNodeDTO
                {
                    MaTran = Convert.ToInt32(row["ma_tran"]),
                    MaGiaiDau = Convert.ToInt32(row["ma_giai_dau"]),
                    MaGiaiDoan = row["ma_giai_doan"] == DBNull.Value ? 0 : Convert.ToInt32(row["ma_giai_doan"]),
                    SoVong = row["so_vong"] == DBNull.Value ? 0 : Convert.ToInt32(row["so_vong"]),
                    NhanhDau = row["nhanh_dau"] == DBNull.Value ? null : row["nhanh_dau"].ToString(),
                    TheThucTran = row["the_thuc_tran"] == DBNull.Value ? null : row["the_thuc_tran"].ToString(),
                    ThoiGianBatDau = row["thoi_gian_bat_dau"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["thoi_gian_bat_dau"]),
                    ThoiGianKetThuc = row["thoi_gian_ket_thuc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["thoi_gian_ket_thuc"]),
                    TrangThai = row["trang_thai"].ToString(),
                    MaTranTiepTheoThang = row["ma_tran_tiep_theo_thang"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ma_tran_tiep_theo_thang"]),
                    MaTranTiepTheoThua = row["ma_tran_tiep_theo_thua"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ma_tran_tiep_theo_thua"]),
                    TenNhomA = row["ten_nhom_a"] == DBNull.Value ? null : row["ten_nhom_a"].ToString(),
                    TenNhomB = row["ten_nhom_b"] == DBNull.Value ? null : row["ten_nhom_b"].ToString()
                });
            }

            return list;
        }

        public List<Dictionary<string, object>> LayBangXepHangTheoGiaiDoan(int maGiaiDoan)
        {
            const string query = @"
SELECT bxh.*, n.ten_nhom, d.ten_doi
FROM BANG_XEP_HANG bxh
JOIN NHOM_DOI n ON bxh.ma_nhom = n.ma_nhom
JOIN DOI d ON n.ma_doi = d.ma_doi
WHERE bxh.ma_giai_doan = @MaGiaiDoan
ORDER BY bxh.thu_hang_hien_tai ASC, bxh.diem_tong_ket DESC, bxh.hieu_so_phu DESC;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan }
            });

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

        public DataRow LayTongQuanPublicGiai(int maGiaiDau)
        {
            const string query = @"
SELECT ma_giai_dau, ten_giai_dau, banner_url, trang_thai, ngay_bat_dau, ngay_ket_thuc
FROM GIAI_DAU
WHERE ma_giai_dau = @MaGiaiDau
  AND is_deleted = 0
  AND trang_thai IN ('mo_dang_ky', 'sap_dien_ra', 'dang_dien_ra', 'ket_thuc');";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        public int LaySoVongLonNhat(int maGiaiDoan)
        {
            const string query = "SELECT ISNULL(MAX(so_vong), 0) FROM TRAN_DAU WHERE ma_giai_doan = @MaGiaiDoan";
            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan }
            });
            return Convert.ToInt32(result);
        }

        public bool TatCaTranTrongVongDaKetThuc(int maGiaiDoan, int soVong)
        {
            const string query = @"
SELECT COUNT(1)
FROM TRAN_DAU
WHERE ma_giai_doan = @MaGiaiDoan
  AND so_vong = @SoVong
  AND trang_thai <> 'da_hoan_thanh';";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan },
                new SqlParameter("@SoVong", SqlDbType.Int){ Value = soVong }
            });

            return Convert.ToInt32(result) == 0;
        }

        public List<Dictionary<string, object>> LayBangXepHangRaw(int maGiaiDoan)
        {
            const string query = @"
SELECT *
FROM BANG_XEP_HANG
WHERE ma_giai_doan = @MaGiaiDoan
ORDER BY so_tran_thang DESC, so_tran_thua ASC, diem_tong_ket DESC;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan }
            });

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

        public bool DaTungGapNhauTrongGiaiDoan(int maGiaiDoan, int maNhomA, int maNhomB)
        {
            const string query = @"
SELECT COUNT(1)
FROM TRAN_DAU t
JOIN CHI_TIET_TRAN_DAU a ON t.ma_tran = a.ma_tran
JOIN CHI_TIET_TRAN_DAU b ON t.ma_tran = b.ma_tran
WHERE t.ma_giai_doan = @MaGiaiDoan
  AND ((a.ma_nhom = @A AND b.ma_nhom = @B) OR (a.ma_nhom = @B AND b.ma_nhom = @A));";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan },
                new SqlParameter("@A", SqlDbType.Int){ Value = maNhomA },
                new SqlParameter("@B", SqlDbType.Int){ Value = maNhomB }
            });

            return Convert.ToInt32(result) > 0;
        }

        public void CapNhatTrangThaiThamGiaSwiss(int maGiaiDau, int maGiaiDoan)
        {
            const string queryDiTiep = @"
UPDATE t
SET t.trang_thai_tham_gia = 'di_tiep'
FROM THAM_GIA_GIAI t
JOIN BANG_XEP_HANG bxh ON bxh.ma_nhom = t.ma_nhom AND bxh.ma_giai_dau = t.ma_giai_dau
WHERE t.ma_giai_dau = @MaGiaiDau
  AND bxh.ma_giai_doan = @MaGiaiDoan
  AND bxh.so_tran_thang >= 3
  AND t.trang_thai_duyet = 'da_duyet';";

            const string queryBiLoai = @"
UPDATE t
SET t.trang_thai_tham_gia = 'bi_loai'
FROM THAM_GIA_GIAI t
JOIN BANG_XEP_HANG bxh ON bxh.ma_nhom = t.ma_nhom AND bxh.ma_giai_dau = t.ma_giai_dau
WHERE t.ma_giai_dau = @MaGiaiDau
  AND bxh.ma_giai_doan = @MaGiaiDoan
  AND bxh.so_tran_thua >= 3
  AND t.trang_thai_duyet = 'da_duyet';";

            SqlParameter[] p =
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan }
            };

            DataProvider.ExecuteNonQuery(queryDiTiep, p);
            DataProvider.ExecuteNonQuery(queryBiLoai, p);
        }

        public void CapNhatCoMatchPoint(int maGiaiDoan, int diemNguong)
        {
            const string query = @"
UPDATE BANG_XEP_HANG
SET is_match_point = CASE WHEN diem_tong_ket >= @DiemNguong THEN 1 ELSE is_match_point END
WHERE ma_giai_doan = @MaGiaiDoan;";

            DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan },
                new SqlParameter("@DiemNguong", SqlDbType.Int){ Value = diemNguong }
            });
        }

        public int? LayNhomTop1Tran(int maTran)
        {
            const string query = @"
SELECT TOP 1 ma_nhom
FROM CHI_TIET_TRAN_DAU
WHERE ma_tran = @MaTran
ORDER BY ISNULL(thu_hang, 9999), diem_so DESC;";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran }
            });

            if (result == null || result == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt32(result);
        }

        public bool NhomDangMatchPoint(int maGiaiDoan, int maNhom)
        {
            const string query = @"
SELECT COUNT(1)
FROM BANG_XEP_HANG
WHERE ma_giai_doan = @MaGiaiDoan
  AND ma_nhom = @MaNhom
  AND is_match_point = 1;";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan },
                new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom }
            });

            return Convert.ToInt32(result) > 0;
        }

        public List<Dictionary<string, object>> LayDanhSachGiaiCongKhai(int? maTroChoi)
        {
            string query = @"
SELECT 
    g.ma_giai_dau, 
    g.ten_giai_dau, 
    g.trang_thai, 
    g.tong_giai_thuong, 
    tc.ma_tro_choi,
    tc.ten_game
FROM GIAI_DAU g
LEFT JOIN TRO_CHOI tc ON g.ma_tro_choi = tc.ma_tro_choi
WHERE ISNULL(g.is_deleted, 0) = 0
  AND g.trang_thai IN ('mo_dang_ky', 'sap_dien_ra', 'dang_dien_ra', 'ket_thuc') ";

            List<SqlParameter> p = new List<SqlParameter>();
            if (maTroChoi.HasValue && maTroChoi.Value > 0)
            {
                query += " AND g.ma_tro_choi = @MaGame ";
                p.Add(new SqlParameter("@MaGame", SqlDbType.Int) { Value = maTroChoi.Value });
            }

            query += " ORDER BY g.ma_giai_dau DESC;";

            DataTable dt = DataProvider.ExecuteQuery(query, p.Count > 0 ? p.ToArray() : null);
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                }
                result.Add(dict);
            }
            return result;
        }

        public void KetThucGiaiDoan(int maGiaiDoan)
        {
            const string query = "UPDATE GIAI_DOAN SET trang_thai = 'ket_thuc' WHERE ma_giai_doan = @MaGiaiDoan";
            DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan }
            });
        }
    }
}
