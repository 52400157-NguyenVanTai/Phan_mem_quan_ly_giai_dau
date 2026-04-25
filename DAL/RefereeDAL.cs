using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using DTO;

namespace DAL
{
    public class RefereeDAL
    {
        public bool GanTrongTai(int maTran, int maTrongTai)
        {
            const string query = @"
UPDATE TRAN_DAU
SET ma_trong_tai = @MaTrongTai
WHERE ma_tran = @MaTran;";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran },
                new SqlParameter("@MaTrongTai", SqlDbType.Int){ Value = maTrongTai }
            });

            return affected > 0;
        }

        public List<Dictionary<string, object>> LayLichSuSuaKetQua(int? maTran)
        {
            const string query = @"
SELECT ls.ma_log,
       ls.ma_tran,
       ls.ma_trong_tai_sua,
       ls.nguoi_sua,
       nd.ten_dang_nhap AS ten_nguoi_sua,
       ls.thoi_gian_sua,
       ls.ly_do_sua,
       ls.du_lieu_cu,
       ls.du_lieu_moi
FROM LICH_SU_SUA_KET_QUA ls
LEFT JOIN NGUOI_DUNG nd ON nd.ma_nguoi_dung = ls.nguoi_sua
WHERE (@MaTran IS NULL OR ls.ma_tran = @MaTran)
ORDER BY ls.thoi_gian_sua DESC, ls.ma_log DESC;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = (object)maTran ?? DBNull.Value }
            });

            return ToList(dt);
        }

        private void DongBoChiTietKetQuaDoiTheoTran(SqlConnection conn, SqlTransaction tran, int maTran)
        {
            DataProvider.ExecuteNonQuery(@"
;WITH TeamAgg AS
(
    SELECT ct.ma_nhom,
           SUM(ISNULL(ctnct.so_kill, 0)) AS tong_kill,
           SUM(ISNULL(ctnct.so_assist, 0)) AS tong_assist,
           SUM(ISNULL(ctnct.so_death, 0)) AS tong_death,
           SUM(ISNULL(ctnct.diem_sinh_ton, 0)) AS tong_sinh_ton
    FROM CHI_TIET_TRAN_DAU ct
    LEFT JOIN TRAN_DAU td ON td.ma_tran = ct.ma_tran
    LEFT JOIN THAM_GIA_GIAI tgg ON tgg.ma_giai_dau = td.ma_giai_dau AND tgg.ma_nhom = ct.ma_nhom
    LEFT JOIN DOI_HINH_THI_DAU dh ON dh.ma_tham_gia = tgg.ma_tham_gia AND ISNULL(dh.is_du_bi, 0) = 0
    LEFT JOIN CHI_TIET_NGUOI_CHOI_TRAN ctnct ON ctnct.ma_tran = td.ma_tran AND ctnct.ma_nguoi_dung = dh.ma_nguoi_dung
    WHERE ct.ma_tran = @MaTran
    GROUP BY ct.ma_nhom
),
Ranked AS
(
    SELECT ma_nhom,
           CAST((ISNULL(tong_kill, 0) + (ISNULL(tong_assist, 0) * 0.1) + ISNULL(tong_sinh_ton, 0)) AS FLOAT) AS diem_tong,
           ROW_NUMBER() OVER (ORDER BY (ISNULL(tong_kill, 0) + (ISNULL(tong_assist, 0) * 0.1) + ISNULL(tong_sinh_ton, 0)) DESC, ma_nhom ASC) AS xep_hang
    FROM TeamAgg
)
UPDATE ct
SET ct.diem_so = r.diem_tong,
    ct.thu_hang = r.xep_hang,
    ct.ket_qua = CASE
        WHEN (SELECT COUNT(1) FROM CHI_TIET_TRAN_DAU WHERE ma_tran = @MaTran) = 2
             THEN CASE WHEN r.xep_hang = 1 THEN 'thang' ELSE 'thua' END
        ELSE CASE WHEN r.xep_hang = 1 THEN 'thang' ELSE NULL END
    END
FROM CHI_TIET_TRAN_DAU ct
JOIN Ranked r ON r.ma_nhom = ct.ma_nhom
WHERE ct.ma_tran = @MaTran;", new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran }
            }, conn, tran);
        }

        private void DongBoBangXepHangLive(SqlConnection conn, SqlTransaction tran, int maTran)
        {
            DataTable info = DataProvider.ExecuteQuery(@"
SELECT td.ma_giai_dau, td.ma_giai_doan, gd.the_thuc, gd.diem_nguong_match_point
FROM TRAN_DAU td
JOIN GIAI_DOAN gd ON gd.ma_giai_doan = td.ma_giai_doan
WHERE td.ma_tran = @MaTran;", new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran }
            }, conn, tran);

            if (info.Rows.Count == 0)
            {
                return;
            }

            DataRow row = info.Rows[0];
            int maGiaiDau = Convert.ToInt32(row["ma_giai_dau"]);
            int maGiaiDoan = Convert.ToInt32(row["ma_giai_doan"]);
            string theThuc = row["the_thuc"].ToString();
            int diemNguong = row["diem_nguong_match_point"] == DBNull.Value ? 0 : Convert.ToInt32(row["diem_nguong_match_point"]);

            DataProvider.ExecuteNonQuery(@"
UPDATE BANG_XEP_HANG
SET so_tran_da_dau = 0,
    so_tran_thang = 0,
    so_tran_thua = 0,
    hieu_so_phu = 0,
    tong_diem_hang = 0,
    tong_diem_kill = 0,
    so_lan_top_1 = 0,
    diem_tong_ket = 0,
    thu_hang_hien_tai = 0,
    is_match_point = 0
WHERE ma_giai_doan = @MaGiaiDoan;", new[]
            {
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan }
            }, conn, tran);

            DataProvider.ExecuteNonQuery(@"
;WITH TeamMatch AS
(
    SELECT td.ma_giai_doan,
           ct.ma_nhom,
           td.ma_tran,
           ISNULL(ct.diem_so, 0) AS diem_so,
           ISNULL(ct.thu_hang, 9999) AS thu_hang,
           ISNULL(ct.ket_qua, '') AS ket_qua,
           COUNT(1) OVER (PARTITION BY td.ma_tran) AS so_doi_trong_tran,
           MAX(ISNULL(ct.diem_so, 0)) OVER (PARTITION BY td.ma_tran) AS diem_max,
           MIN(ISNULL(ct.diem_so, 0)) OVER (PARTITION BY td.ma_tran) AS diem_min
    FROM TRAN_DAU td
    JOIN CHI_TIET_TRAN_DAU ct ON ct.ma_tran = td.ma_tran
    WHERE td.ma_giai_doan = @MaGiaiDoan
      AND td.trang_thai = 'da_hoan_thanh'
),
Agg AS
(
    SELECT ma_nhom,
           COUNT(DISTINCT ma_tran) AS so_tran_da_dau,
           SUM(CASE WHEN ket_qua = 'thang' THEN 1 ELSE 0 END) AS so_tran_thang,
           SUM(CASE WHEN ket_qua = 'thua' THEN 1 ELSE 0 END) AS so_tran_thua,
           SUM(CASE WHEN so_doi_trong_tran = 2 THEN (diem_so - CASE WHEN ket_qua = 'thang' THEN diem_min ELSE diem_max END) ELSE 0 END) AS hieu_so_phu,
           SUM(CASE WHEN thu_hang <= 16 THEN (17 - thu_hang) ELSE 0 END) AS tong_diem_hang,
           SUM(diem_so) AS tong_diem_kill,
           SUM(CASE WHEN thu_hang = 1 THEN 1 ELSE 0 END) AS so_lan_top_1
    FROM TeamMatch
    GROUP BY ma_nhom
)
UPDATE bxh
SET bxh.so_tran_da_dau = ISNULL(a.so_tran_da_dau, 0),
    bxh.so_tran_thang = ISNULL(a.so_tran_thang, 0),
    bxh.so_tran_thua = ISNULL(a.so_tran_thua, 0),
    bxh.hieu_so_phu = ISNULL(a.hieu_so_phu, 0),
    bxh.tong_diem_hang = ISNULL(a.tong_diem_hang, 0),
    bxh.tong_diem_kill = ISNULL(a.tong_diem_kill, 0),
    bxh.so_lan_top_1 = ISNULL(a.so_lan_top_1, 0),
    bxh.diem_tong_ket = CASE
        WHEN @TheThuc = 'thuy_si' THEN (ISNULL(a.so_tran_thang, 0) * 1000) + (ISNULL(a.hieu_so_phu, 0) * 10) + ISNULL(a.tong_diem_kill, 0)
        WHEN @TheThuc = 'champion_rush' THEN ISNULL(a.tong_diem_hang, 0) + ISNULL(a.tong_diem_kill, 0)
        WHEN @TheThuc = 'vong_tron' THEN (ISNULL(a.so_tran_thang, 0) * 3) + ISNULL(a.hieu_so_phu, 0)
        ELSE (ISNULL(a.so_tran_thang, 0) * 3) + ISNULL(a.tong_diem_hang, 0) + ISNULL(a.tong_diem_kill, 0)
    END
FROM BANG_XEP_HANG bxh
LEFT JOIN Agg a ON a.ma_nhom = bxh.ma_nhom
WHERE bxh.ma_giai_doan = @MaGiaiDoan;", new[]
            {
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan },
                new SqlParameter("@TheThuc", SqlDbType.NVarChar){ Value = theThuc }
            }, conn, tran);

            DataProvider.ExecuteNonQuery(@"
;WITH Ranked AS
(
    SELECT ma_bxh,
           ROW_NUMBER() OVER (
               ORDER BY diem_tong_ket DESC, so_tran_thang DESC, hieu_so_phu DESC, so_lan_top_1 DESC, ma_nhom ASC
           ) AS stt
    FROM BANG_XEP_HANG
    WHERE ma_giai_doan = @MaGiaiDoan
)
UPDATE bxh
SET bxh.thu_hang_hien_tai = r.stt
FROM BANG_XEP_HANG bxh
JOIN Ranked r ON r.ma_bxh = bxh.ma_bxh;", new[]
            {
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan }
            }, conn, tran);

            if (string.Equals(theThuc, "champion_rush", StringComparison.OrdinalIgnoreCase) && diemNguong > 0)
            {
                DataProvider.ExecuteNonQuery(@"
UPDATE BANG_XEP_HANG
SET is_match_point = CASE WHEN diem_tong_ket >= @DiemNguong THEN 1 ELSE 0 END
WHERE ma_giai_doan = @MaGiaiDoan;", new[]
                {
                    new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan },
                    new SqlParameter("@DiemNguong", SqlDbType.Int){ Value = diemNguong }
                }, conn, tran);
            }
            else
            {
                DataProvider.ExecuteNonQuery("UPDATE BANG_XEP_HANG SET is_match_point = 0 WHERE ma_giai_doan = @MaGiaiDoan", new[]
                {
                    new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan }
                }, conn, tran);
            }

            DataProvider.ExecuteNonQuery(@"
UPDATE THAM_GIA_GIAI
SET trang_thai_tham_gia = CASE
    WHEN ma_nhom IN (SELECT ma_nhom FROM BANG_XEP_HANG WHERE ma_giai_doan = @MaGiaiDoan)
         THEN 'dang_thi_dau'
    ELSE trang_thai_tham_gia
END
WHERE ma_giai_dau = @MaGiaiDau
  AND trang_thai_duyet = 'da_duyet';", new[]
            {
                new SqlParameter("@MaGiaiDoan", SqlDbType.Int){ Value = maGiaiDoan },
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            }, conn, tran);
        }

        private void TuDongDayNhanhDau(SqlConnection conn, SqlTransaction tran, int maTran)
        {
            DataTable info = DataProvider.ExecuteQuery(@"
SELECT td.ma_tran_tiep_theo_thang,
       td.ma_tran_tiep_theo_thua,
       gd.the_thuc
FROM TRAN_DAU td
LEFT JOIN GIAI_DOAN gd ON gd.ma_giai_doan = td.ma_giai_doan
WHERE td.ma_tran = @MaTran;", new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran }
            }, conn, tran);

            if (info.Rows.Count == 0)
            {
                return;
            }

            DataRow row = info.Rows[0];
            string theThuc = row["the_thuc"] == DBNull.Value ? string.Empty : row["the_thuc"].ToString();
            if (!string.Equals(theThuc, "loai_truc_tiep", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(theThuc, "nhanh_thang_nhanh_thua", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            int? maTranThang = row["ma_tran_tiep_theo_thang"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ma_tran_tiep_theo_thang"]);
            int? maTranThua = row["ma_tran_tiep_theo_thua"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ma_tran_tiep_theo_thua"]);

            DataTable rank = DataProvider.ExecuteQuery(@"
SELECT ma_nhom, thu_hang, ket_qua
FROM CHI_TIET_TRAN_DAU
WHERE ma_tran = @MaTran
ORDER BY ISNULL(thu_hang, 9999), ma_nhom;", new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran }
            }, conn, tran);

            if (rank.Rows.Count < 2)
            {
                return;
            }

            int maNhomThang = 0;
            int maNhomThua = 0;
            foreach (DataRow r in rank.Rows)
            {
                string ketQua = r["ket_qua"] == DBNull.Value ? string.Empty : r["ket_qua"].ToString();
                if (maNhomThang <= 0 && string.Equals(ketQua, "thang", StringComparison.OrdinalIgnoreCase))
                {
                    maNhomThang = Convert.ToInt32(r["ma_nhom"]);
                }
                if (maNhomThua <= 0 && string.Equals(ketQua, "thua", StringComparison.OrdinalIgnoreCase))
                {
                    maNhomThua = Convert.ToInt32(r["ma_nhom"]);
                }
            }

            if (maNhomThang <= 0)
            {
                maNhomThang = Convert.ToInt32(rank.Rows[0]["ma_nhom"]);
            }
            if (maNhomThua <= 0 && rank.Rows.Count > 1)
            {
                maNhomThua = Convert.ToInt32(rank.Rows[1]["ma_nhom"]);
            }

            if (maTranThang.HasValue && maNhomThang > 0)
            {
                GanNhomVaoTranDich(conn, tran, maTranThang.Value, maNhomThang);
            }
            if (maTranThua.HasValue && maNhomThua > 0)
            {
                GanNhomVaoTranDich(conn, tran, maTranThua.Value, maNhomThua);
            }
        }

        private static void GanNhomVaoTranDich(SqlConnection conn, SqlTransaction tran, int maTranDich, int maNhom)
        {
            DataProvider.ExecuteNonQuery(@"
IF NOT EXISTS (
    SELECT 1 FROM CHI_TIET_TRAN_DAU WHERE ma_tran = @MaTranDich AND ma_nhom = @MaNhom
)
AND (SELECT COUNT(1) FROM CHI_TIET_TRAN_DAU WHERE ma_tran = @MaTranDich) < 2
BEGIN
    INSERT INTO CHI_TIET_TRAN_DAU(ma_tran, ma_nhom, diem_so, thu_hang, ket_qua)
    VALUES(@MaTranDich, @MaNhom, 0, NULL, NULL);
END;", new[]
            {
                new SqlParameter("@MaTranDich", SqlDbType.Int){ Value = maTranDich },
                new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom }
            }, conn, tran);
        }

        private void DongBoBangXepHangCaNhan(SqlConnection conn, SqlTransaction tran, int maTran)
        {
            DataTable dt = DataProvider.ExecuteQuery("SELECT ma_giai_dau FROM TRAN_DAU WHERE ma_tran = @MaTran", new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran }
            }, conn, tran);

            if (dt.Rows.Count == 0)
            {
                return;
            }

            int maGiaiDau = Convert.ToInt32(dt.Rows[0]["ma_giai_dau"]);

            DataProvider.ExecuteNonQuery("DELETE FROM BANG_XEP_HANG_CA_NHAN WHERE ma_giai_dau = @MaGiaiDau", new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            }, conn, tran);

            DataProvider.ExecuteNonQuery(@"
INSERT INTO BANG_XEP_HANG_CA_NHAN(ma_giai_dau, ma_nguoi_dung, tong_kill, tong_death, tong_assist, diem_kda_trung_binh, so_lan_dat_mvp_tran)
SELECT td.ma_giai_dau,
       ctnct.ma_nguoi_dung,
       SUM(ISNULL(ctnct.so_kill, 0)) AS tong_kill,
       SUM(ISNULL(ctnct.so_death, 0)) AS tong_death,
       SUM(ISNULL(ctnct.so_assist, 0)) AS tong_assist,
       AVG(ISNULL(ctnct.diem_kda_tran, 0)) AS diem_kda_trung_binh,
       SUM(CASE WHEN ISNULL(ctnct.is_mvp_tran, 0) = 1 THEN 1 ELSE 0 END) AS so_lan_dat_mvp_tran
FROM CHI_TIET_NGUOI_CHOI_TRAN ctnct
JOIN TRAN_DAU td ON td.ma_tran = ctnct.ma_tran
WHERE td.ma_giai_dau = @MaGiaiDau
GROUP BY td.ma_giai_dau, ctnct.ma_nguoi_dung;", new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            }, conn, tran);
        }

        public bool TranTonTai(int maTran)
        {
            const string query = "SELECT COUNT(1) FROM TRAN_DAU WHERE ma_tran = @MaTran";
            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran }
            });
            return Convert.ToInt32(result) > 0;
        }

        public bool NhomThuocTran(int maTran, int maNhom)
        {
            const string query = @"
SELECT COUNT(1)
FROM CHI_TIET_TRAN_DAU
WHERE ma_tran = @MaTran
  AND ma_nhom = @MaNhom;";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran },
                new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom }
            });

            return Convert.ToInt32(result) > 0;
        }

        public bool NguoiDungThuocNhomTrongTran(int maTran, int maNhom, int maNguoiDung)
        {
            const string query = @"
SELECT COUNT(1)
FROM TRAN_DAU td
JOIN THAM_GIA_GIAI tgg ON tgg.ma_giai_dau = td.ma_giai_dau
JOIN DOI_HINH_THI_DAU dh ON dh.ma_tham_gia = tgg.ma_tham_gia
WHERE td.ma_tran = @MaTran
  AND tgg.ma_nhom = @MaNhom
  AND dh.ma_nguoi_dung = @MaNguoiDung;";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran },
                new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom },
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung }
            });

            return Convert.ToInt32(result) > 0;
        }

        public bool DaCoKhieuNaiChoXuLy(int maTran, int maNhom)
        {
            const string query = @"
SELECT COUNT(1)
FROM KHIEU_NAI_KET_QUA
WHERE ma_tran = @MaTran
  AND ma_nhom = @MaNhom
  AND trang_thai = 'cho_xu_ly';";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran },
                new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom }
            });

            return Convert.ToInt32(result) > 0;
        }

        public bool NguoiPhanCongLaBanToChuc(int maTran, int maNguoiDung)
        {
            const string query = @"
SELECT COUNT(1)
FROM TRAN_DAU td
JOIN QUAN_TRI_GIAI_DAU qt ON qt.ma_giai_dau = td.ma_giai_dau
WHERE td.ma_tran = @MaTran
  AND qt.ma_nguoi_dung = @MaNguoiDung
  AND qt.vai_tro_giai = 'ban_to_chuc';";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran },
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung }
            });

            return Convert.ToInt32(result) > 0;
        }

        public bool NguoiDuocGanLaTrongTaiCuaGiai(int maTran, int maTrongTai)
        {
            const string query = @"
SELECT COUNT(1)
FROM TRAN_DAU td
JOIN QUAN_TRI_GIAI_DAU qt ON qt.ma_giai_dau = td.ma_giai_dau
WHERE td.ma_tran = @MaTran
  AND qt.ma_nguoi_dung = @MaTrongTai
  AND qt.vai_tro_giai = 'trong_tai';";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran },
                new SqlParameter("@MaTrongTai", SqlDbType.Int){ Value = maTrongTai }
            });

            return Convert.ToInt32(result) > 0;
        }

        public bool KiemTraTrongTaiPhuTrach(int maTran, int maTrongTai)
        {
            const string query = @"
SELECT COUNT(1)
FROM TRAN_DAU
WHERE ma_tran = @MaTran
  AND ma_trong_tai = @MaTrongTai;";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran },
                new SqlParameter("@MaTrongTai", SqlDbType.Int){ Value = maTrongTai }
            });

            return Convert.ToInt32(result) > 0;
        }

        public List<Dictionary<string, object>> LayTranCuaToi(int maTrongTai, string tab)
        {
            string condition;
            switch ((tab ?? string.Empty).ToLowerInvariant())
            {
                case "sap_dien_ra":
                    condition = "td.trang_thai IN ('chua_dau', 'dang_dau')";
                    break;
                case "da_hoan_thanh":
                    condition = "td.thoi_gian_nhap_diem IS NOT NULL";
                    break;
                default:
                    condition = "td.trang_thai = 'da_hoan_thanh' AND td.thoi_gian_nhap_diem IS NULL";
                    break;
            }

            string query = @"
SELECT td.ma_tran,
       td.ma_giai_dau,
       td.ma_giai_doan,
       td.vong_dau,
       td.the_thuc_tran,
       td.thoi_gian_bat_dau,
       td.thoi_gian_ket_thuc,
       td.trang_thai,
       td.thoi_gian_nhap_diem,
       td.so_lan_sua
FROM TRAN_DAU td
WHERE td.ma_trong_tai = @MaTrongTai
  AND " + condition + @"
ORDER BY ISNULL(td.thoi_gian_bat_dau, td.thoi_gian_ket_thuc) DESC, td.ma_tran DESC;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaTrongTai", SqlDbType.Int){ Value = maTrongTai }
            });

            return ToList(dt);
        }

        public DataRow LayThongTinTran(int maTran)
        {
            const string query = @"
SELECT ma_tran, ma_giai_dau, ma_giai_doan, ma_trong_tai, trang_thai, thoi_gian_ket_thuc, thoi_gian_nhap_diem, so_lan_sua
FROM TRAN_DAU
WHERE ma_tran = @MaTran;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran }
            });

            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        public List<Dictionary<string, object>> LayRosterNhapLieuTheoTran(int maTran)
        {
            const string query = @"
SELECT ct.ma_nhom,
       n.ten_nhom,
       dh.ma_nguoi_dung,
       nd.ten_dang_nhap,
       ISNULL(hsg.in_game_name, nd.ten_dang_nhap) AS ten_hien_thi,
       dh.ma_vi_tri,
       vt.ten_vi_tri,
       ISNULL(ctnct.so_kill, 0) AS so_kill,
       ISNULL(ctnct.so_death, 0) AS so_death,
       ISNULL(ctnct.so_assist, 0) AS so_assist,
       ISNULL(ctnct.diem_sinh_ton, 0) AS diem_sinh_ton,
       ISNULL(ctnct.diem_kda_tran, 0) AS diem_kda_tran,
       ISNULL(ctnct.is_mvp_tran, 0) AS is_mvp_tran
FROM TRAN_DAU td
JOIN CHI_TIET_TRAN_DAU ct ON td.ma_tran = ct.ma_tran
JOIN NHOM_DOI n ON n.ma_nhom = ct.ma_nhom
JOIN THAM_GIA_GIAI tgg ON tgg.ma_giai_dau = td.ma_giai_dau AND tgg.ma_nhom = ct.ma_nhom
JOIN DOI_HINH_THI_DAU dh ON dh.ma_tham_gia = tgg.ma_tham_gia AND ISNULL(dh.is_du_bi, 0) = 0
JOIN NGUOI_DUNG nd ON nd.ma_nguoi_dung = dh.ma_nguoi_dung
LEFT JOIN HO_SO_IN_GAME hsg ON hsg.ma_nguoi_dung = dh.ma_nguoi_dung
LEFT JOIN DANH_MUC_VI_TRI vt ON vt.ma_vi_tri = dh.ma_vi_tri
LEFT JOIN CHI_TIET_NGUOI_CHOI_TRAN ctnct ON ctnct.ma_tran = td.ma_tran AND ctnct.ma_nguoi_dung = dh.ma_nguoi_dung
WHERE td.ma_tran = @MaTran
ORDER BY ct.ma_nhom, dh.is_du_bi, nd.ten_dang_nhap;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran }
            });

            return ToList(dt);
        }

        public bool NguoiDungThuocTran(int maTran, int maNguoiDung)
        {
            const string query = @"
SELECT COUNT(1)
FROM TRAN_DAU td
JOIN CHI_TIET_TRAN_DAU ct ON td.ma_tran = ct.ma_tran
JOIN THAM_GIA_GIAI tgg ON tgg.ma_giai_dau = td.ma_giai_dau AND tgg.ma_nhom = ct.ma_nhom
JOIN DOI_HINH_THI_DAU dh ON dh.ma_tham_gia = tgg.ma_tham_gia
WHERE td.ma_tran = @MaTran
  AND dh.ma_nguoi_dung = @MaNguoiDung;";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran },
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung }
            });

            return Convert.ToInt32(result) > 0;
        }

        public bool CoTheSuaTrong12h(int maTran)
        {
            const string query = @"
SELECT COUNT(1)
FROM TRAN_DAU
WHERE ma_tran = @MaTran
  AND thoi_gian_nhap_diem IS NOT NULL
  AND ISNULL(so_lan_sua, 0) = 0
  AND DATEDIFF(HOUR, thoi_gian_nhap_diem, GETDATE()) <= 12;";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran }
            });

            return Convert.ToInt32(result) > 0;
        }

        public bool DaNhapDiemLanDau(int maTran)
        {
            const string query = @"
SELECT COUNT(1)
FROM TRAN_DAU
WHERE ma_tran = @MaTran
  AND thoi_gian_nhap_diem IS NOT NULL;";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran }
            });

            return Convert.ToInt32(result) > 0;
        }

        public List<Dictionary<string, object>> LayChiSoTheoTran(int maTran)
        {
            const string query = @"
SELECT ma_nguoi_dung, ma_vi_tri, so_kill, so_death, so_assist, diem_sinh_ton, diem_kda_tran, is_mvp_tran
FROM CHI_TIET_NGUOI_CHOI_TRAN
WHERE ma_tran = @MaTran
ORDER BY ma_nguoi_dung;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran }
            });

            return ToList(dt);
        }

        public void LuuKetQuaTran(int maTran, List<RefereePlayerStatInputDTO> chiSo, int maNguoiSua, string lyDo, bool laSua, bool boQuaKhoa12h)
        {
            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        string duLieuCu = SerializeRows(LayChiSoTheoTranTrongTran(conn, tran, maTran));

                        DataProvider.ExecuteNonQuery("DELETE FROM CHI_TIET_NGUOI_CHOI_TRAN WHERE ma_tran = @MaTran", new[]
                        {
                            new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran }
                        }, conn, tran);

                        double kdaMax = double.MinValue;
                        HashSet<int> topPlayers = new HashSet<int>();

                        foreach (RefereePlayerStatInputDTO item in chiSo)
                        {
                            double kda = (item.SoKill + item.SoAssist) / (double)Math.Max(1, item.SoDeath);
                            if (kda > kdaMax)
                            {
                                kdaMax = kda;
                                topPlayers.Clear();
                                topPlayers.Add(item.MaNguoiDung);
                            }
                            else if (Math.Abs(kda - kdaMax) < 0.0001)
                            {
                                topPlayers.Add(item.MaNguoiDung);
                            }

                            DataProvider.ExecuteNonQuery(@"
INSERT INTO CHI_TIET_NGUOI_CHOI_TRAN(ma_tran, ma_nguoi_dung, ma_vi_tri, so_kill, so_death, so_assist, diem_sinh_ton, diem_kda_tran, is_mvp_tran)
VALUES(@MaTran, @MaNguoiDung, @MaViTri, @SoKill, @SoDeath, @SoAssist, @DiemSinhTon, @DiemKda, 0);", new[]
                            {
                                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran },
                                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = item.MaNguoiDung },
                                new SqlParameter("@MaViTri", SqlDbType.Int){ Value = (object)item.MaViTri ?? DBNull.Value },
                                new SqlParameter("@SoKill", SqlDbType.Int){ Value = item.SoKill },
                                new SqlParameter("@SoDeath", SqlDbType.Int){ Value = item.SoDeath },
                                new SqlParameter("@SoAssist", SqlDbType.Int){ Value = item.SoAssist },
                                new SqlParameter("@DiemSinhTon", SqlDbType.Float){ Value = (object)item.DiemSinhTon ?? DBNull.Value },
                                new SqlParameter("@DiemKda", SqlDbType.Float){ Value = kda }
                            }, conn, tran);
                        }

                        if (topPlayers.Count > 0)
                        {
                            foreach (int maNguoiDung in topPlayers)
                            {
                                DataProvider.ExecuteNonQuery(@"
UPDATE CHI_TIET_NGUOI_CHOI_TRAN
SET is_mvp_tran = 1
WHERE ma_tran = @MaTran
  AND ma_nguoi_dung = @MaNguoiDung;", new[]
                                {
                                    new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran },
                                    new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung }
                                }, conn, tran);
                            }
                        }

                        string updateTran = laSua
                            ? @"UPDATE TRAN_DAU
SET so_lan_sua = CASE WHEN @BoQuaKhoa12h = 1 THEN so_lan_sua ELSE so_lan_sua + 1 END
WHERE ma_tran = @MaTran;"
                            : @"UPDATE TRAN_DAU
SET thoi_gian_nhap_diem = ISNULL(thoi_gian_nhap_diem, GETDATE()),
    so_lan_sua = ISNULL(so_lan_sua, 0)
WHERE ma_tran = @MaTran;";

                        DataProvider.ExecuteNonQuery(updateTran, new[]
                        {
                            new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran },
                            new SqlParameter("@BoQuaKhoa12h", SqlDbType.Bit){ Value = boQuaKhoa12h }
                        }, conn, tran);

                        if (laSua)
                        {
                            string duLieuMoi = SerializeRows(LayChiSoTheoTranTrongTran(conn, tran, maTran));
                            DataProvider.ExecuteNonQuery(@"
INSERT INTO LICH_SU_SUA_KET_QUA(ma_tran, ma_trong_tai_sua, nguoi_sua, ly_do_sua, du_lieu_cu, du_lieu_moi)
VALUES(@MaTran, @MaNguoiSua, @MaNguoiSua, @LyDo, @DuLieuCu, @DuLieuMoi);", new[]
                            {
                                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran },
                                new SqlParameter("@MaNguoiSua", SqlDbType.Int){ Value = maNguoiSua },
                                new SqlParameter("@LyDo", SqlDbType.NVarChar){ Value = (object)lyDo ?? DBNull.Value },
                                new SqlParameter("@DuLieuCu", SqlDbType.NVarChar){ Value = (object)duLieuCu ?? DBNull.Value },
                                new SqlParameter("@DuLieuMoi", SqlDbType.NVarChar){ Value = (object)duLieuMoi ?? DBNull.Value }
                            }, conn, tran);
                        }

                        DongBoChiTietKetQuaDoiTheoTran(conn, tran, maTran);
                        DongBoBangXepHangLive(conn, tran, maTran);
                        TuDongDayNhanhDau(conn, tran, maTran);
                        DongBoBangXepHangCaNhan(conn, tran, maTran);

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public int TaoKhieuNaiKetQua(TaoKhieuNaiKetQuaDTO dto, int maNguoiGui)
        {
            const string query = @"
INSERT INTO KHIEU_NAI_KET_QUA(ma_tran, ma_nhom, ma_nguoi_gui, noi_dung)
OUTPUT INSERTED.ma_khieu_nai
VALUES(@MaTran, @MaNhom, @MaNguoiGui, @NoiDung);";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = dto.MaTran },
                new SqlParameter("@MaNhom", SqlDbType.Int){ Value = dto.MaNhom },
                new SqlParameter("@MaNguoiGui", SqlDbType.Int){ Value = maNguoiGui },
                new SqlParameter("@NoiDung", SqlDbType.NVarChar){ Value = dto.NoiDung.Trim() }
            });

            return Convert.ToInt32(result);
        }

        public DataRow LayThongTinDongBoTheoTran(int maTran)
        {
            const string query = @"
SELECT td.ma_tran,
       td.ma_giai_dau,
       td.ma_giai_doan,
       gd.the_thuc,
       gd.diem_nguong_match_point
FROM TRAN_DAU td
LEFT JOIN GIAI_DOAN gd ON gd.ma_giai_doan = td.ma_giai_doan
WHERE td.ma_tran = @MaTran;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran }
            });

            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        public List<Dictionary<string, object>> LayDanhSachKhieuNai(string trangThai)
        {
            string query = @"
SELECT kn.ma_khieu_nai,
       kn.ma_tran,
       kn.ma_nhom,
       kn.ma_nguoi_gui,
       nd.ten_dang_nhap AS ten_nguoi_gui,
       kn.noi_dung,
       kn.trang_thai,
       kn.ma_admin_xu_ly,
       kn.phan_hoi_admin,
       kn.thoi_gian_tao,
       kn.thoi_gian_xu_ly
FROM KHIEU_NAI_KET_QUA kn
JOIN NGUOI_DUNG nd ON nd.ma_nguoi_dung = kn.ma_nguoi_gui
WHERE (@TrangThai IS NULL OR kn.trang_thai = @TrangThai)
ORDER BY kn.thoi_gian_tao DESC, kn.ma_khieu_nai DESC;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@TrangThai", SqlDbType.NVarChar){ Value = string.IsNullOrWhiteSpace(trangThai) ? (object)DBNull.Value : trangThai.Trim() }
            });

            return ToList(dt);
        }

        public bool XuLyKhieuNai(int maKhieuNai, int maAdmin, bool chapNhan, string phanHoi)
        {
            const string query = @"
UPDATE KHIEU_NAI_KET_QUA
SET trang_thai = @TrangThai,
    ma_admin_xu_ly = @MaAdmin,
    phan_hoi_admin = @PhanHoi,
    thoi_gian_xu_ly = GETDATE()
WHERE ma_khieu_nai = @MaKhieuNai
  AND trang_thai = 'cho_xu_ly';";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@TrangThai", SqlDbType.NVarChar){ Value = chapNhan ? "da_xu_ly" : "tu_choi" },
                new SqlParameter("@MaAdmin", SqlDbType.Int){ Value = maAdmin },
                new SqlParameter("@PhanHoi", SqlDbType.NVarChar){ Value = (object)phanHoi ?? DBNull.Value },
                new SqlParameter("@MaKhieuNai", SqlDbType.Int){ Value = maKhieuNai }
            });

            return affected > 0;
        }

        private List<Dictionary<string, object>> LayChiSoTheoTranTrongTran(SqlConnection conn, SqlTransaction tran, int maTran)
        {
            DataTable dt = DataProvider.ExecuteQuery(@"
SELECT ma_nguoi_dung, ma_vi_tri, so_kill, so_death, so_assist, diem_sinh_ton, diem_kda_tran, is_mvp_tran
FROM CHI_TIET_NGUOI_CHOI_TRAN
WHERE ma_tran = @MaTran
ORDER BY ma_nguoi_dung;", new[]
            {
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran }
            }, conn, tran);

            return ToList(dt);
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

        private static string SerializeRows(List<Dictionary<string, object>> rows)
        {
            if (rows == null || rows.Count == 0)
            {
                return "[]";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < rows.Count; i++)
            {
                if (i > 0) sb.Append("|");
                Dictionary<string, object> row = rows[i];
                bool first = true;
                foreach (KeyValuePair<string, object> kv in row)
                {
                    if (!first) sb.Append(";");
                    first = false;
                    string value = kv.Value == null ? "null" : kv.Value.ToString().Replace(";", ",").Replace("|", ",");
                    sb.Append(kv.Key).Append("=").Append(value);
                }
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
