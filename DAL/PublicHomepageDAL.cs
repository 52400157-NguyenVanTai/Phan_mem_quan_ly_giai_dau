using System.Data;

namespace DAL
{
    public class PublicHomepageDAL
    {
        public DataTable LayHeroStats()
        {
            const string query = @"
SELECT
    TongGiaiDangHoatDong = (
        SELECT COUNT(1)
        FROM GIAI_DAU gd
        WHERE ISNULL(gd.is_deleted, 0) = 0
          AND ISNULL(gd.hien_thi_public, 0) = 1
          AND gd.trang_thai IN ('chuan_bi_dien_ra', 'dang_dien_ra')
    ),
    TongDoiTuyenThamGia = (
        SELECT COUNT(DISTINCT nd.ma_doi)
        FROM THAM_GIA_GIAI tgg
        INNER JOIN GIAI_DAU gd ON gd.ma_giai_dau = tgg.ma_giai_dau
        INNER JOIN NHOM_DOI nd ON nd.ma_nhom = tgg.ma_nhom
        WHERE tgg.trang_thai_duyet = 'da_duyet'
          AND ISNULL(gd.is_deleted, 0) = 0
          AND ISNULL(gd.hien_thi_public, 0) = 1
    ),
    TongGameHoTro = (
        SELECT COUNT(1)
        FROM TRO_CHOI tc
        WHERE ISNULL(tc.is_active, 1) = 1
    ),
    TongLuotTheoDoi = (
        SELECT ISNULL(SUM(CAST(tt.dang_theo_doi AS INT)), 0)
        FROM TUONG_TAC_GIAI_DAU tt
        INNER JOIN GIAI_DAU gd ON gd.ma_giai_dau = tt.ma_giai_dau
        WHERE ISNULL(gd.is_deleted, 0) = 0
          AND ISNULL(gd.hien_thi_public, 0) = 1
    ),
    TongGiaiMoDangKy = (
        SELECT COUNT(1)
        FROM GIAI_DAU gd
        WHERE ISNULL(gd.is_deleted, 0) = 0
          AND ISNULL(gd.hien_thi_public, 0) = 1
          AND gd.trang_thai = 'chuan_bi_dien_ra'
          AND ISNULL(gd.dang_mo_dang_ky, 0) = 1
    ),
    TongGiaiSapDienRa = (
        SELECT COUNT(1)
        FROM GIAI_DAU gd
        WHERE ISNULL(gd.is_deleted, 0) = 0
          AND ISNULL(gd.hien_thi_public, 0) = 1
          AND gd.trang_thai = 'chuan_bi_dien_ra'
          AND ISNULL(gd.dang_mo_dang_ky, 0) = 0
    );";

            return DataProvider.ExecuteQuery(query);
        }

        public DataTable LayGiaiNoiBat(int top)
        {
            string query = @"
SELECT TOP (@Top)
    gd.ma_giai_dau,
    gd.ten_giai_dau,
    gd.ma_tro_choi,
    tc.ten_game,
    gd.trang_thai,
    gd.ngay_bat_dau,
    gd.thoi_gian_dong_dang_ky,
    gd.tong_giai_thuong,
    gd.so_luong_doi_toi_da,
    SoDoiDaDangKy = (
        SELECT COUNT(1)
        FROM THAM_GIA_GIAI tgg
        WHERE tgg.ma_giai_dau = gd.ma_giai_dau
          AND tgg.trang_thai_duyet = 'da_duyet'
    )
FROM GIAI_DAU gd
LEFT JOIN TRO_CHOI tc ON tc.ma_tro_choi = gd.ma_tro_choi
WHERE ISNULL(gd.is_deleted, 0) = 0
  AND ISNULL(gd.hien_thi_public, 0) = 1
  AND gd.trang_thai IN ('chuan_bi_dien_ra', 'dang_dien_ra')
ORDER BY
    CASE
        WHEN gd.trang_thai = 'chuan_bi_dien_ra' AND ISNULL(gd.dang_mo_dang_ky, 0) = 1 THEN 0
        WHEN gd.trang_thai = 'chuan_bi_dien_ra' AND ISNULL(gd.dang_mo_dang_ky, 0) = 0 THEN 1
        WHEN gd.trang_thai = 'dang_dien_ra' THEN 2
        ELSE 3
    END,
    CASE WHEN gd.ngay_bat_dau IS NULL THEN 1 ELSE 0 END,
    gd.ngay_bat_dau ASC,
    gd.tong_giai_thuong DESC,
    gd.ma_giai_dau DESC;";

            return DataProvider.ExecuteQuery(query, new[]
            {
                new System.Data.SqlClient.SqlParameter("@Top", System.Data.SqlDbType.Int) { Value = top }
            });
        }

        public DataTable LayGameHoTro(int top)
        {
            string query = @"
SELECT TOP (@Top)
    tc.ma_tro_choi,
    tc.ten_game,
    tc.the_loai,
    SoGiaiDangVanHanh = (
        SELECT COUNT(1)
        FROM GIAI_DAU gd
        WHERE gd.ma_tro_choi = tc.ma_tro_choi
          AND ISNULL(gd.is_deleted, 0) = 0
          AND ISNULL(gd.hien_thi_public, 0) = 1
          AND gd.trang_thai IN ('chuan_bi_dien_ra', 'dang_dien_ra')
    )
FROM TRO_CHOI tc
WHERE ISNULL(tc.is_active, 1) = 1
ORDER BY SoGiaiDangVanHanh DESC, tc.ten_game ASC;";

            return DataProvider.ExecuteQuery(query, new[]
            {
                new System.Data.SqlClient.SqlParameter("@Top", System.Data.SqlDbType.Int) { Value = top }
            });
        }

        public DataTable LayDoiNoiBat(int top)
        {
            string query = @"
SELECT TOP (@Top)
    d.ma_doi,
    d.ten_doi,
    d.logo_url,
    d.slogan,
    ISNULL(d.dang_tuyen, 0) AS dang_tuyen,
    SoThanhVienActive = (
        SELECT COUNT(DISTINCT tv.ma_nguoi_dung)
        FROM NHOM_DOI nd
        INNER JOIN THANH_VIEN_DOI tv ON tv.ma_nhom = nd.ma_nhom
        WHERE nd.ma_doi = d.ma_doi
          AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
          AND tv.trang_thai_duyet = 'da_duyet'
    ),
    SoGiaiDaThamGia = (
        SELECT COUNT(DISTINCT tgg.ma_giai_dau)
        FROM NHOM_DOI nd
        INNER JOIN THAM_GIA_GIAI tgg ON tgg.ma_nhom = nd.ma_nhom
        INNER JOIN GIAI_DAU gd ON gd.ma_giai_dau = tgg.ma_giai_dau
        WHERE nd.ma_doi = d.ma_doi
          AND tgg.trang_thai_duyet = 'da_duyet'
          AND ISNULL(gd.is_deleted, 0) = 0
          AND ISNULL(gd.hien_thi_public, 0) = 1
    )
FROM DOI d
WHERE d.trang_thai = 'dang_hoat_dong'
ORDER BY
    SoGiaiDaThamGia DESC,
    SoThanhVienActive DESC,
    ISNULL(d.dang_tuyen, 0) DESC,
    d.ten_doi ASC;";

            return DataProvider.ExecuteQuery(query, new[]
            {
                new System.Data.SqlClient.SqlParameter("@Top", System.Data.SqlDbType.Int) { Value = top }
            });
        }

        public DataTable LayGiaiMoDangKy(int top)
        {
            string query = @"
SELECT TOP (@Top)
    gd.ma_giai_dau,
    gd.ten_giai_dau,
    gd.ma_tro_choi,
    tc.ten_game,
    gd.trang_thai,
    gd.ngay_bat_dau,
    gd.thoi_gian_dong_dang_ky,
    gd.tong_giai_thuong,
    gd.so_luong_doi_toi_da,
    SoDoiDaDangKy = (
        SELECT COUNT(1)
        FROM THAM_GIA_GIAI tgg
        WHERE tgg.ma_giai_dau = gd.ma_giai_dau
          AND tgg.trang_thai_duyet = 'da_duyet'
    )
FROM GIAI_DAU gd
LEFT JOIN TRO_CHOI tc ON tc.ma_tro_choi = gd.ma_tro_choi
WHERE ISNULL(gd.is_deleted, 0) = 0
  AND ISNULL(gd.hien_thi_public, 0) = 1
  AND gd.trang_thai = 'chuan_bi_dien_ra'
  AND ISNULL(gd.dang_mo_dang_ky, 0) = 1
ORDER BY gd.thoi_gian_dong_dang_ky ASC, gd.ma_giai_dau DESC;";

            return DataProvider.ExecuteQuery(query, new[]
            {
                new System.Data.SqlClient.SqlParameter("@Top", System.Data.SqlDbType.Int) { Value = top }
            });
        }

        public DataTable LayGiaiSapDienRa(int top)
        {
            string query = @"
SELECT TOP (@Top)
    gd.ma_giai_dau,
    gd.ten_giai_dau,
    gd.ma_tro_choi,
    tc.ten_game,
    gd.trang_thai,
    gd.ngay_bat_dau,
    gd.thoi_gian_dong_dang_ky,
    gd.tong_giai_thuong,
    gd.so_luong_doi_toi_da,
    SoDoiDaDangKy = (
        SELECT COUNT(1)
        FROM THAM_GIA_GIAI tgg
        WHERE tgg.ma_giai_dau = gd.ma_giai_dau
          AND tgg.trang_thai_duyet = 'da_duyet'
    )
FROM GIAI_DAU gd
LEFT JOIN TRO_CHOI tc ON tc.ma_tro_choi = gd.ma_tro_choi
WHERE ISNULL(gd.is_deleted, 0) = 0
  AND ISNULL(gd.hien_thi_public, 0) = 1
  AND gd.trang_thai = 'chuan_bi_dien_ra'
  AND ISNULL(gd.dang_mo_dang_ky, 0) = 0
ORDER BY gd.ngay_bat_dau ASC, gd.ma_giai_dau DESC;";

            return DataProvider.ExecuteQuery(query, new[]
            {
                new System.Data.SqlClient.SqlParameter("@Top", System.Data.SqlDbType.Int) { Value = top }
            });
        }

        public DataTable LayTranGanDayHoacDangDau(int top)
        {
            string query = @"
WITH MatchTeams AS (
    SELECT
        td.ma_tran,
        d.ten_doi,
        ROW_NUMBER() OVER (PARTITION BY td.ma_tran ORDER BY ctd.ma_nhom) AS rn
    FROM TRAN_DAU td
    INNER JOIN CHI_TIET_TRAN_DAU ctd ON ctd.ma_tran = td.ma_tran
    INNER JOIN NHOM_DOI nd ON nd.ma_nhom = ctd.ma_nhom
    INNER JOIN DOI d ON d.ma_doi = nd.ma_doi
)
SELECT TOP (@Top)
    td.ma_tran,
    td.ma_giai_dau,
    gd.ten_giai_dau,
    gdo.ten_giai_doan,
    td.trang_thai,
    TeamA.ten_doi AS ten_doi_a,
    TeamB.ten_doi AS ten_doi_b,
    td.thoi_gian_bat_dau,
    td.thoi_gian_ket_thuc
FROM TRAN_DAU td
INNER JOIN GIAI_DAU gd ON gd.ma_giai_dau = td.ma_giai_dau
LEFT JOIN GIAI_DOAN gdo ON gdo.ma_giai_doan = td.ma_giai_doan
LEFT JOIN MatchTeams TeamA ON TeamA.ma_tran = td.ma_tran AND TeamA.rn = 1
LEFT JOIN MatchTeams TeamB ON TeamB.ma_tran = td.ma_tran AND TeamB.rn = 2
WHERE ISNULL(gd.is_deleted, 0) = 0
  AND ISNULL(gd.hien_thi_public, 0) = 1
  AND td.trang_thai IN ('dang_dau', 'da_hoan_thanh', 'chua_dau')
ORDER BY
    CASE
        WHEN td.trang_thai = 'dang_dau' THEN 0
        WHEN td.trang_thai = 'chua_dau' THEN 1
        ELSE 2
    END,
    ISNULL(td.thoi_gian_bat_dau, td.thoi_gian_ket_thuc) DESC,
    td.ma_tran DESC;";

            return DataProvider.ExecuteQuery(query, new[]
            {
                new System.Data.SqlClient.SqlParameter("@Top", System.Data.SqlDbType.Int) { Value = top }
            });
        }
    }
}
