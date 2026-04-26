using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class TeamDAL
    {
        public bool TenDoiTonTai(string tenDoi)
        {
            const string query = "SELECT COUNT(1) FROM DOI WHERE ten_doi = @TenDoi AND trang_thai <> 'da_giai_the'";
            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@TenDoi", SqlDbType.NVarChar){ Value = tenDoi.Trim() }
            });
            return Convert.ToInt32(result) > 0;
        }

        public bool NhomThuocDoi(int maNhom, int maDoi)
        {
            const string query = "SELECT COUNT(1) FROM NHOM_DOI WHERE ma_nhom = @MaNhom AND ma_doi = @MaDoi";
            return Convert.ToInt32(DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaNhom", SqlDbType.Int) { Value = maNhom },
                new SqlParameter("@MaDoi", SqlDbType.Int) { Value = maDoi }
            })) > 0;
        }

        public bool NhomChuaChairman(int maNhom)
        {
            const string query = @"
SELECT COUNT(1)
FROM THANH_VIEN_DOI
WHERE ma_nhom = @MaNhom
  AND vai_tro_noi_bo = 'leader'
  AND trang_thai_hop_dong = 'dang_hieu_luc';";
            return Convert.ToInt32(DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaNhom", SqlDbType.Int) { Value = maNhom }
            })) > 0;
        }

        public bool NhomCoDuLieuThamGiaGiai(int maNhom)
        {
            const string query = @"
SELECT CASE WHEN EXISTS (SELECT 1 FROM THAM_GIA_GIAI WHERE ma_nhom = @MaNhom)
   OR EXISTS (SELECT 1 FROM CHI_TIET_TRAN_DAU WHERE ma_nhom = @MaNhom)
   OR EXISTS (SELECT 1 FROM KHIEU_NAI_KET_QUA WHERE ma_nhom = @MaNhom)
   OR EXISTS (SELECT 1 FROM BANG_XEP_HANG WHERE ma_nhom = @MaNhom)
THEN 1 ELSE 0 END;";
            return Convert.ToInt32(DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaNhom", SqlDbType.Int) { Value = maNhom }
            })) == 1;
        }

        public void XoaNhom(int maNhom, SqlConnection conn, SqlTransaction tran)
        {
            const string queryXoaLoiMoi = "DELETE FROM LOI_MOI_GIA_NHAP WHERE ma_nhom = @MaNhom";
            const string queryXoaDonXin = "DELETE FROM XIN_GIA_NHAP WHERE ma_nhom = @MaNhom";
            const string queryXoaDonUngTuyen = @"DELETE d
FROM DON_UNG_TUYEN d
JOIN BAI_DANG_TUYEN_DUNG b ON b.ma_bai_dang = d.ma_bai_dang
WHERE b.ma_nhom = @MaNhom";
            const string queryXoaBaiDang = "DELETE FROM BAI_DANG_TUYEN_DUNG WHERE ma_nhom = @MaNhom";
            const string queryXoaThanhVien = "DELETE FROM THANH_VIEN_DOI WHERE ma_nhom = @MaNhom";
            const string queryXoaNhom = "DELETE FROM NHOM_DOI WHERE ma_nhom = @MaNhom";

            DataProvider.ExecuteNonQuery(queryXoaLoiMoi, new[] { new SqlParameter("@MaNhom", SqlDbType.Int) { Value = maNhom } }, conn, tran);
            DataProvider.ExecuteNonQuery(queryXoaDonXin, new[] { new SqlParameter("@MaNhom", SqlDbType.Int) { Value = maNhom } }, conn, tran);
            DataProvider.ExecuteNonQuery(queryXoaDonUngTuyen, new[] { new SqlParameter("@MaNhom", SqlDbType.Int) { Value = maNhom } }, conn, tran);
            DataProvider.ExecuteNonQuery(queryXoaBaiDang, new[] { new SqlParameter("@MaNhom", SqlDbType.Int) { Value = maNhom } }, conn, tran);
            DataProvider.ExecuteNonQuery(queryXoaThanhVien, new[] { new SqlParameter("@MaNhom", SqlDbType.Int) { Value = maNhom } }, conn, tran);
            DataProvider.ExecuteNonQuery(queryXoaNhom, new[] { new SqlParameter("@MaNhom", SqlDbType.Int) { Value = maNhom } }, conn, tran);
        }

        public int TaoDoi(int maManager, string tenDoi, string logoUrl, string slogan, SqlConnection conn, SqlTransaction tran)
        {
            const string query = @"
INSERT INTO DOI(ten_doi, ma_doi_truong, ma_manager, logo_url, slogan, trang_thai)
OUTPUT INSERTED.ma_doi
VALUES(@TenDoi, @MaManager, @MaManager, @LogoUrl, @Slogan, 'dang_hoat_dong');";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@TenDoi", SqlDbType.NVarChar){ Value = tenDoi.Trim() },
                new SqlParameter("@MaManager", SqlDbType.Int){ Value = maManager },
                new SqlParameter("@LogoUrl", SqlDbType.NVarChar){ Value = (object)logoUrl ?? DBNull.Value },
                new SqlParameter("@Slogan", SqlDbType.NVarChar){ Value = (object)slogan ?? DBNull.Value }
            }, conn, tran);

            return Convert.ToInt32(result);
        }

        public int TaoNhom(int maDoi, int maTroChoi, string tenNhom, int maCaptain, SqlConnection conn, SqlTransaction tran)
        {
            const string query = @"
INSERT INTO NHOM_DOI(ma_doi, ma_tro_choi, ten_nhom, ma_doi_truong_nhom)
OUTPUT INSERTED.ma_nhom
VALUES(@MaDoi, @MaTroChoi, @TenNhom, @MaDoiTruongNhom);";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaDoi", SqlDbType.Int){ Value = maDoi },
                new SqlParameter("@MaTroChoi", SqlDbType.Int){ Value = maTroChoi },
                new SqlParameter("@TenNhom", SqlDbType.NVarChar){ Value = tenNhom.Trim() },
                new SqlParameter("@MaDoiTruongNhom", SqlDbType.Int){ Value = maCaptain }
            }, conn, tran);

            return Convert.ToInt32(result);
        }

        public void ThemThanhVien(int maNguoiDung, int maNhom, string vaiTroNoiBo, string phanHe, int? maViTri, SqlConnection conn, SqlTransaction tran)
        {
            const string query = @"
INSERT INTO THANH_VIEN_DOI(ma_nguoi_dung, ma_nhom, vai_tro_noi_bo, phan_he, ma_vi_tri, trang_thai_duyet, trang_thai_hop_dong)
VALUES(@MaNguoiDung, @MaNhom, @VaiTroNoiBo, @PhanHe, @MaViTri, 'da_duyet', 'dang_hieu_luc');";

            DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung },
                new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom },
                new SqlParameter("@VaiTroNoiBo", SqlDbType.NVarChar){ Value = vaiTroNoiBo },
                new SqlParameter("@PhanHe", SqlDbType.NVarChar){ Value = phanHe },
                new SqlParameter("@MaViTri", SqlDbType.Int){ Value = (object)maViTri ?? DBNull.Value }
            }, conn, tran);
        }

        public int DemSoNhomCuaDoi(int maDoi)
        {
            const string query = "SELECT COUNT(1) FROM NHOM_DOI WHERE ma_doi = @MaDoi";
            return Convert.ToInt32(DataProvider.ExecuteScalar(query, new[] { new SqlParameter("@MaDoi", SqlDbType.Int) { Value = maDoi } }));
        }

        public int DemSoNhomTheoGame(int maDoi, int maTroChoi)
        {
            const string query = "SELECT COUNT(1) FROM NHOM_DOI WHERE ma_doi = @MaDoi AND ma_tro_choi = @MaTroChoi";
            return Convert.ToInt32(DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaDoi", SqlDbType.Int) { Value = maDoi },
                new SqlParameter("@MaTroChoi", SqlDbType.Int) { Value = maTroChoi }
            }));
        }

        public bool NguoiDungDangThuocDoiKhac(int maNguoiDung)
        {
            const string query = @"
SELECT COUNT(1)
FROM THANH_VIEN_DOI tv
JOIN NHOM_DOI n ON tv.ma_nhom = n.ma_nhom
JOIN DOI d ON d.ma_doi = n.ma_doi
WHERE tv.ma_nguoi_dung = @MaNguoiDung
  AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
  AND d.trang_thai <> 'da_giai_the';";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung }
            });

            return Convert.ToInt32(result) > 0;
        }

        public bool GiaiTanDoi(int maDoi, int maManager)
        {
            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    const string checkManagerQuery = "SELECT COUNT(1) FROM DOI WHERE ma_doi = @MaDoi AND ma_doi_truong = @MaManager";
                    object check = DataProvider.ExecuteScalar(checkManagerQuery, new[]
                    {
                        new SqlParameter("@MaDoi", SqlDbType.Int){ Value = maDoi },
                        new SqlParameter("@MaManager", SqlDbType.Int){ Value = maManager }
                    }, conn, tran);

                    if (Convert.ToInt32(check) == 0)
                    {
                        tran.Rollback();
                        return false;
                    }

                    const string updateMemberContract = @"
UPDATE tv
SET tv.trang_thai_hop_dong = 'tu_do'
FROM THANH_VIEN_DOI tv
JOIN NHOM_DOI n ON n.ma_nhom = tv.ma_nhom
WHERE n.ma_doi = @MaDoi;";

                    DataProvider.ExecuteNonQuery(updateMemberContract, new[]
                    {
                        new SqlParameter("@MaDoi", SqlDbType.Int){ Value = maDoi }
                    }, conn, tran);

                    const string dissolveTeamQuery = "UPDATE DOI SET trang_thai = 'da_giai_the' WHERE ma_doi = @MaDoi";
                    DataProvider.ExecuteNonQuery(dissolveTeamQuery, new[]
                    {
                        new SqlParameter("@MaDoi", SqlDbType.Int){ Value = maDoi }
                    }, conn, tran);

                    tran.Commit();
                    return true;
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        public bool XoaDoiVinhVien(int maDoi, int maManager)
        {
            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    const string checkManagerQuery = "SELECT COUNT(1) FROM DOI WHERE ma_doi = @MaDoi AND ma_doi_truong = @MaManager";
                    object check = DataProvider.ExecuteScalar(checkManagerQuery, new[]
                    {
                        new SqlParameter("@MaDoi", SqlDbType.Int){ Value = maDoi },
                        new SqlParameter("@MaManager", SqlDbType.Int){ Value = maManager }
                    }, conn, tran);

                    if (Convert.ToInt32(check) == 0)
                    {
                        tran.Rollback();
                        return false;
                    }

                    DataTable dtNhom = LayDanhSachNhom(maDoi);
                    foreach (DataRow row in dtNhom.Rows)
                    {
                        int maNhom = Convert.ToInt32(row["ma_nhom"]);
                        if (NhomCoDuLieuThamGiaGiai(maNhom))
                        {
                            tran.Rollback();
                            return false;
                        }
                    }

                    foreach (DataRow row in dtNhom.Rows)
                    {
                        int maNhom = Convert.ToInt32(row["ma_nhom"]);
                        XoaNhom(maNhom, conn, tran);
                    }

                    const string deleteTeamQuery = "DELETE FROM DOI WHERE ma_doi = @MaDoi";
                    DataProvider.ExecuteNonQuery(deleteTeamQuery, new[]
                    {
                        new SqlParameter("@MaDoi", SqlDbType.Int){ Value = maDoi }
                    }, conn, tran);

                    tran.Commit();
                    return true;
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        public bool NguoiChoiCoHoSoDungGame(int maNguoiDung, int maTroChoi)
        {
            const string query = "SELECT COUNT(1) FROM HO_SO_IN_GAME WHERE ma_nguoi_dung = @MaNguoiDung AND ma_tro_choi = @MaTroChoi";
            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung },
                new SqlParameter("@MaTroChoi", SqlDbType.Int){ Value = maTroChoi }
            });
            return Convert.ToInt32(result) > 0;
        }

        public bool KiemTraRoleNoiBoTrongNhom(int maNguoiDung, int maNhom, string roleYeuCau)
        {
            const string query = @"
SELECT COUNT(1)
FROM THANH_VIEN_DOI
WHERE ma_nguoi_dung = @MaNguoiDung
  AND ma_nhom = @MaNhom
  AND vai_tro_noi_bo = @VaiTroNoiBo
  AND trang_thai_hop_dong = 'dang_hieu_luc';";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung },
                new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom },
                new SqlParameter("@VaiTroNoiBo", SqlDbType.NVarChar){ Value = roleYeuCau }
            });

            return Convert.ToInt32(result) > 0;
        }

        public DataTable LayDoiCuaToi(int maNguoiDung)
        {
            const string query = @"
SELECT TOP 1
    d.ma_doi, d.ten_doi, d.logo_url, d.slogan, d.trang_thai,
    tv.vai_tro_noi_bo, tv.phan_he,
    tc.ten_game,
    nd.ten_nhom
FROM THANH_VIEN_DOI tv
JOIN NHOM_DOI nd ON tv.ma_nhom = nd.ma_nhom
JOIN DOI d ON nd.ma_doi = d.ma_doi
JOIN TRO_CHOI tc ON nd.ma_tro_choi = tc.ma_tro_choi
WHERE tv.ma_nguoi_dung = @MaNguoiDung
  AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
  AND d.trang_thai <> 'da_giai_the'
ORDER BY d.ma_doi;";

            return DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung }
            });
        }

        // Lấy tất cả đội mà user đang tham gia (nhiều nhóm, nhiều đội)
        public DataTable LayTatCaDoiCuaToi(int maNguoiDung)
        {
            const string query = @"
SELECT
    d.ma_doi, d.ten_doi, d.logo_url, d.slogan, d.trang_thai AS trang_thai_doi,
    nd.ma_nhom, nd.ten_nhom, tc.ten_game, tc.ma_tro_choi,
    tv.vai_tro_noi_bo, tv.phan_he, tv.ma_thanh_vien,
    (SELECT COUNT(1) FROM THANH_VIEN_DOI t2 WHERE t2.ma_nhom = nd.ma_nhom AND t2.trang_thai_hop_dong = 'dang_hieu_luc') AS so_thanh_vien
FROM THANH_VIEN_DOI tv
JOIN NHOM_DOI nd ON tv.ma_nhom = nd.ma_nhom
JOIN DOI d ON nd.ma_doi = d.ma_doi
JOIN TRO_CHOI tc ON nd.ma_tro_choi = tc.ma_tro_choi
WHERE tv.ma_nguoi_dung = @MaNguoiDung
  AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
  AND d.trang_thai <> 'da_giai_the'
ORDER BY d.ten_doi, nd.ten_nhom;";
            return DataProvider.ExecuteQuery(query, new[] { new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung } });
        }

        // Danh sách đội công khai (Team Explorer)
        public DataTable LayDanhSachDoiCongKhai(int? maTroChoi, bool? dangTuyen, string tuKhoa)
        {
            string query = @"
SELECT DISTINCT
    d.ma_doi, d.ten_doi, d.logo_url, d.slogan, d.trang_thai,
    ISNULL(d.dang_tuyen, 0) AS dang_tuyen,
    d.ngay_tao,
    (SELECT COUNT(DISTINCT tv.ma_nguoi_dung) FROM THANH_VIEN_DOI tv
     JOIN NHOM_DOI n2 ON n2.ma_nhom = tv.ma_nhom
     WHERE n2.ma_doi = d.ma_doi AND tv.trang_thai_hop_dong = 'dang_hieu_luc') AS tong_thanh_vien,
    (SELECT STRING_AGG(tc.ten_game, ', ') FROM NHOM_DOI nd2
     JOIN TRO_CHOI tc ON tc.ma_tro_choi = nd2.ma_tro_choi
     WHERE nd2.ma_doi = d.ma_doi) AS danh_sach_game
FROM DOI d
WHERE d.trang_thai = 'dang_hoat_dong'";

            var parms = new List<SqlParameter>();

            if (maTroChoi.HasValue && maTroChoi.Value > 0)
            {
                query += @" AND EXISTS (SELECT 1 FROM NHOM_DOI nd3 WHERE nd3.ma_doi = d.ma_doi AND nd3.ma_tro_choi = @MaTroChoi)";
                parms.Add(new SqlParameter("@MaTroChoi", SqlDbType.Int) { Value = maTroChoi.Value });
            }
            if (dangTuyen.HasValue && dangTuyen.Value)
            {
                query += @" AND ISNULL(d.dang_tuyen, 0) = 1";
            }
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query += @" AND d.ten_doi LIKE @TuKhoa";
                parms.Add(new SqlParameter("@TuKhoa", SqlDbType.NVarChar) { Value = "%" + tuKhoa.Trim() + "%" });
            }
            query += " ORDER BY d.ngay_tao DESC;";
            return DataProvider.ExecuteQuery(query, parms.ToArray());
        }

        // Chi tiết đội
        public DataTable LayChiTietDoi(int maDoi)
        {
            const string query = @"
SELECT d.ma_doi, d.ten_doi, d.logo_url, d.slogan, d.trang_thai,
       d.ma_doi_truong, d.ma_manager, d.ngay_tao, ISNULL(d.dang_tuyen, 0) AS dang_tuyen,
       ISNULL(d.mo_ta, '') AS mo_ta,
       u.ten_dang_nhap AS ten_manager
FROM DOI d
LEFT JOIN NGUOI_DUNG u ON u.ma_nguoi_dung = d.ma_doi_truong
WHERE d.ma_doi = @MaDoi AND d.trang_thai <> 'da_giai_the';";
            return DataProvider.ExecuteQuery(query, new[] { new SqlParameter("@MaDoi", SqlDbType.Int) { Value = maDoi } });
        }

        // Danh sách nhóm (squads) của một đội
        public DataTable LayDanhSachNhom(int maDoi)
        {
            const string query = @"
SELECT nd.ma_nhom, nd.ten_nhom, nd.ma_tro_choi, tc.ten_game,
       (SELECT COUNT(1) FROM THANH_VIEN_DOI tv WHERE tv.ma_nhom = nd.ma_nhom AND tv.trang_thai_hop_dong = 'dang_hieu_luc') AS so_thanh_vien
FROM NHOM_DOI nd
JOIN TRO_CHOI tc ON tc.ma_tro_choi = nd.ma_tro_choi
WHERE nd.ma_doi = @MaDoi
ORDER BY nd.ma_nhom;";
            return DataProvider.ExecuteQuery(query, new[] { new SqlParameter("@MaDoi", SqlDbType.Int) { Value = maDoi } });
        }

        // Thành viên của một nhóm
        public DataTable LayThanhVienNhom(int maNhom)
        {
            const string query = @"
SELECT tv.ma_thanh_vien, tv.ma_nguoi_dung, tv.vai_tro_noi_bo, tv.phan_he, tv.ngay_tham_gia,
       u.ten_dang_nhap, u.avatar_url,
       vt.ten_vi_tri, vt.ky_hieu AS ky_hieu_vi_tri,
       h.in_game_name, h.in_game_id
FROM THANH_VIEN_DOI tv
JOIN NGUOI_DUNG u ON u.ma_nguoi_dung = tv.ma_nguoi_dung
LEFT JOIN DANH_MUC_VI_TRI vt ON vt.ma_vi_tri = tv.ma_vi_tri
LEFT JOIN NHOM_DOI nd ON nd.ma_nhom = tv.ma_nhom
LEFT JOIN HO_SO_IN_GAME h ON h.ma_nguoi_dung = tv.ma_nguoi_dung AND h.ma_tro_choi = nd.ma_tro_choi
WHERE tv.ma_nhom = @MaNhom
  AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
ORDER BY
  CASE tv.vai_tro_noi_bo WHEN 'leader' THEN 1 WHEN 'coach' THEN 2 WHEN 'captain' THEN 3 ELSE 4 END,
  tv.ngay_tham_gia;";
            return DataProvider.ExecuteQuery(query, new[] { new SqlParameter("@MaNhom", SqlDbType.Int) { Value = maNhom } });
        }

        // Xin gia nhập nhóm
        public int TaoXinGiaNhap(int maNguoiDung, int maNhom, int? maHoSo)
        {
            const string query = @"
INSERT INTO XIN_GIA_NHAP(ma_nguoi_dung, ma_nhom, ma_ho_so, trang_thai)
OUTPUT INSERTED.ma_don_xin
VALUES(@MaNguoiDung, @MaNhom, @MaHoSo, 'cho_duyet');";
            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung },
                new SqlParameter("@MaNhom", SqlDbType.Int) { Value = maNhom },
                new SqlParameter("@MaHoSo", SqlDbType.Int) { Value = (object)maHoSo ?? DBNull.Value }
            });
            return Convert.ToInt32(result);
        }

        // Danh sách đơn xin gia nhập cho một nhóm
        public DataTable LayDanhSachXinGiaNhap(int maNhom)
        {
            const string query = @"
SELECT x.ma_don_xin, x.ma_nguoi_dung, x.ma_nhom, x.ma_ho_so, x.trang_thai, x.ngay_tao,
       u.ten_dang_nhap, u.avatar_url,
       h.in_game_name, h.in_game_id,
       vt.ten_vi_tri
FROM XIN_GIA_NHAP x
JOIN NGUOI_DUNG u ON u.ma_nguoi_dung = x.ma_nguoi_dung
LEFT JOIN HO_SO_IN_GAME h ON h.ma_ho_so = x.ma_ho_so
LEFT JOIN DANH_MUC_VI_TRI vt ON vt.ma_vi_tri = h.ma_vi_tri_so_truong
WHERE x.ma_nhom = @MaNhom AND x.trang_thai = 'cho_duyet'
ORDER BY x.ngay_tao;";
            return DataProvider.ExecuteQuery(query, new[] { new SqlParameter("@MaNhom", SqlDbType.Int) { Value = maNhom } });
        }

        // Duyệt/Từ chối đơn xin gia nhập
        public bool CapNhatXinGiaNhap(int maDonXin, string trangThai)
        {
            const string query = "UPDATE XIN_GIA_NHAP SET trang_thai = @TrangThai WHERE ma_don_xin = @MaDonXin";
            return DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@TrangThai", SqlDbType.NVarChar) { Value = trangThai },
                new SqlParameter("@MaDonXin", SqlDbType.Int) { Value = maDonXin }
            }) > 0;
        }

        // Lấy thông tin đơn xin
        public DataRow LayXinGiaNhap(int maDonXin)
        {
            const string query = @"
SELECT x.ma_don_xin, x.ma_nguoi_dung, x.ma_nhom, x.trang_thai,
       nd.ma_doi, nd.ma_tro_choi
FROM XIN_GIA_NHAP x
JOIN NHOM_DOI nd ON nd.ma_nhom = x.ma_nhom
WHERE x.ma_don_xin = @MaDonXin;";
            DataTable dt = DataProvider.ExecuteQuery(query, new[] { new SqlParameter("@MaDonXin", SqlDbType.Int) { Value = maDonXin } });
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        // Kiểm tra quyền quản lý nhóm (bao gồm leader/coach/captain + Chairman check)
        public bool CoQuyenQuanLyNhom(int maNguoiDung, int maNhom)
        {
            const string query = @"
SELECT CASE
  WHEN EXISTS (
    SELECT 1 FROM THANH_VIEN_DOI WHERE ma_nguoi_dung = @MaNguoiDung AND ma_nhom = @MaNhom
      AND vai_tro_noi_bo IN ('leader','coach','captain') AND trang_thai_hop_dong = 'dang_hieu_luc'
  ) THEN 1
  WHEN EXISTS (
    SELECT 1 FROM THANH_VIEN_DOI tv
    JOIN NHOM_DOI n1 ON n1.ma_nhom = tv.ma_nhom
    WHERE tv.ma_nguoi_dung = @MaNguoiDung AND tv.vai_tro_noi_bo = 'leader'
      AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
      AND n1.ma_doi = (SELECT ma_doi FROM NHOM_DOI WHERE ma_nhom = @MaNhom)
  ) THEN 1
  ELSE 0
END;";
            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung },
                new SqlParameter("@MaNhom", SqlDbType.Int) { Value = maNhom }
            });
            return Convert.ToInt32(result) == 1;
        }

        // Kiểm tra user có phải Chairman (leader) của đội
        public bool LaChairman(int maNguoiDung, int maDoi)
        {
            const string query = @"
SELECT COUNT(1) FROM DOI WHERE ma_doi = @MaDoi AND ma_doi_truong = @MaNguoiDung AND trang_thai <> 'da_giai_the';";
            return Convert.ToInt32(DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaDoi", SqlDbType.Int) { Value = maDoi },
                new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung }
            })) > 0;
        }

        // Cập nhật vai trò nội bộ
        public bool CapNhatVaiTro(int maThanhVien, string vaiTroMoi)
        {
            const string query = "UPDATE THANH_VIEN_DOI SET vai_tro_noi_bo = @VaiTro WHERE ma_thanh_vien = @MaThanhVien";
            return DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@VaiTro", SqlDbType.NVarChar) { Value = vaiTroMoi },
                new SqlParameter("@MaThanhVien", SqlDbType.Int) { Value = maThanhVien }
            }) > 0;
        }

        // Lấy thông tin thành viên theo ID
        public DataRow LayThanhVien(int maThanhVien)
        {
            const string query = @"
SELECT tv.*, nd.ma_doi FROM THANH_VIEN_DOI tv
JOIN NHOM_DOI nd ON nd.ma_nhom = tv.ma_nhom
WHERE tv.ma_thanh_vien = @MaThanhVien;";
            DataTable dt = DataProvider.ExecuteQuery(query, new[] { new SqlParameter("@MaThanhVien", SqlDbType.Int) { Value = maThanhVien } });
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        // Tìm user theo tên đăng nhập hoặc email
        public DataRow TimNguoiDung(string tuKhoa)
        {
            const string query = @"
SELECT ma_nguoi_dung, ten_dang_nhap, email, avatar_url
FROM NGUOI_DUNG
WHERE ten_dang_nhap = @TuKhoa OR email = @TuKhoa;";
            DataTable dt = DataProvider.ExecuteQuery(query, new[] { new SqlParameter("@TuKhoa", SqlDbType.NVarChar) { Value = tuKhoa.Trim() } });
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        // Cập nhật trạng thái tuyển dụng
        public bool CapNhatDangTuyen(int maDoi, bool dangTuyen)
        {
            const string query = "UPDATE DOI SET dang_tuyen = @DangTuyen WHERE ma_doi = @MaDoi";
            return DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@DangTuyen", SqlDbType.Bit) { Value = dangTuyen },
                new SqlParameter("@MaDoi", SqlDbType.Int) { Value = maDoi }
            }) > 0;
        }

        // Lấy mã nhóm từ thành viên
        public int LayMaNhomTuThanhVien(int maThanhVien)
        {
            const string query = "SELECT ma_nhom FROM THANH_VIEN_DOI WHERE ma_thanh_vien = @MaThanhVien";
            object result = DataProvider.ExecuteScalar(query, new[] { new SqlParameter("@MaThanhVien", SqlDbType.Int) { Value = maThanhVien } });
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        // Tạo thông báo
        public void TaoThongBao(int maNguoiNhan, string tieuDe, string noiDung, string loaiThongBao, string loaiEntity = null, int? maEntity = null)
        {
            const string query = @"
INSERT INTO THONG_BAO(ma_nguoi_nhan, tieu_de, noi_dung, loai_thong_bao, loai_entity, ma_entity)
VALUES(@MaNguoiNhan, @TieuDe, @NoiDung, @LoaiThongBao, @LoaiEntity, @MaEntity);";
            DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaNguoiNhan", SqlDbType.Int) { Value = maNguoiNhan },
                new SqlParameter("@TieuDe", SqlDbType.NVarChar) { Value = tieuDe },
                new SqlParameter("@NoiDung", SqlDbType.NVarChar) { Value = noiDung },
                new SqlParameter("@LoaiThongBao", SqlDbType.NVarChar) { Value = loaiThongBao },
                new SqlParameter("@LoaiEntity", SqlDbType.NVarChar) { Value = (object)loaiEntity ?? DBNull.Value },
                new SqlParameter("@MaEntity", SqlDbType.Int) { Value = (object)maEntity ?? DBNull.Value }
            });
        }

        // Lấy danh sách thông báo
        public DataTable LayThongBao(int maNguoiDung)
        {
            const string query = @"
SELECT TOP 50 ma_thong_bao, tieu_de, noi_dung, loai_thong_bao, da_doc,
       ngay_tao, loai_entity, ma_entity, hanh_dong
FROM THONG_BAO
WHERE ma_nguoi_nhan = @MaNguoiDung
ORDER BY ngay_tao DESC;";
            return DataProvider.ExecuteQuery(query, new[] { new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung } });
        }

        // Đếm thông báo chưa đọc
        public int DemThongBaoChuaDoc(int maNguoiDung)
        {
            const string query = "SELECT COUNT(1) FROM THONG_BAO WHERE ma_nguoi_nhan = @MaNguoiDung AND da_doc = 0";
            return Convert.ToInt32(DataProvider.ExecuteScalar(query, new[] { new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung } }));
        }

        // Đánh dấu đã đọc
        public void DanhDauDaDoc(int maThongBao, int maNguoiDung)
        {
            const string query = "UPDATE THONG_BAO SET da_doc = 1 WHERE ma_thong_bao = @MaThongBao AND ma_nguoi_nhan = @MaNguoiDung";
            DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaThongBao", SqlDbType.Int) { Value = maThongBao },
                new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung }
            });
        }

        // Đánh dấu tất cả đã đọc
        public void DanhDauTatCaDaDoc(int maNguoiDung)
        {
            const string query = "UPDATE THONG_BAO SET da_doc = 1 WHERE ma_nguoi_nhan = @MaNguoiDung AND da_doc = 0";
            DataProvider.ExecuteNonQuery(query, new[] { new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung } });
        }

        // Tìm kiếm toàn cục (giải + đội)
        public DataTable TimKiemToanCuc(string tuKhoa)
        {
            const string query = @"
SELECT TOP 20 'giai_dau' AS loai, ma_giai_dau AS ma, ten_giai_dau AS ten, trang_thai,
    (SELECT tc.ten_game FROM TRO_CHOI tc WHERE tc.ma_tro_choi = g.ma_tro_choi) AS ten_game
FROM GIAI_DAU g
WHERE ten_giai_dau LIKE @TuKhoa AND ISNULL(is_deleted,0) = 0
UNION ALL
SELECT TOP 20 'doi' AS loai, ma_doi AS ma, ten_doi AS ten, trang_thai, NULL
FROM DOI
WHERE ten_doi LIKE @TuKhoa AND trang_thai = 'dang_hoat_dong'
ORDER BY ten;";
            return DataProvider.ExecuteQuery(query, new[] { new SqlParameter("@TuKhoa", SqlDbType.NVarChar) { Value = "%" + tuKhoa.Trim() + "%" } });
        }

        // Dashboard: Giải nổi bật (by likes + follows DESC)
        public DataTable LayGiaiNoiBat(int top)
        {
            string query = @"
SELECT TOP " + top + @"
    g.ma_giai_dau, g.ten_giai_dau, g.trang_thai, g.tong_giai_thuong, g.ngay_bat_dau,
    tc.ma_tro_choi, tc.ten_game,
    ISNULL(v.tong_like,0) AS tong_like, ISNULL(v.tong_theo_doi,0) AS tong_theo_doi
FROM GIAI_DAU g
LEFT JOIN TRO_CHOI tc ON tc.ma_tro_choi = g.ma_tro_choi
LEFT JOIN VW_TUONG_TAC_TONG_HOP v ON v.ma_giai_dau = g.ma_giai_dau
WHERE g.trang_thai IN ('mo_dang_ky','sap_dien_ra','dang_dien_ra')
  AND ISNULL(g.is_deleted,0) = 0
ORDER BY (ISNULL(v.tong_like,0) + ISNULL(v.tong_theo_doi,0)) DESC;";
            return DataProvider.ExecuteQuery(query, null);
        }

        // Dashboard: Sắp bắt đầu (start date ASC, > now)
        public DataTable LayGiaiSapBatDau(int top)
        {
            string query = @"
SELECT TOP " + top + @"
    g.ma_giai_dau, g.ten_giai_dau, g.trang_thai, g.tong_giai_thuong, g.ngay_bat_dau,
    tc.ma_tro_choi, tc.ten_game,
    ISNULL(v.tong_like,0) AS tong_like, ISNULL(v.tong_theo_doi,0) AS tong_theo_doi
FROM GIAI_DAU g
LEFT JOIN TRO_CHOI tc ON tc.ma_tro_choi = g.ma_tro_choi
LEFT JOIN VW_TUONG_TAC_TONG_HOP v ON v.ma_giai_dau = g.ma_giai_dau
WHERE g.ngay_bat_dau > GETDATE()
  AND g.trang_thai IN ('mo_dang_ky','sap_dien_ra')
  AND ISNULL(g.is_deleted,0) = 0
ORDER BY g.ngay_bat_dau ASC;";
            return DataProvider.ExecuteQuery(query, null);
        }

        // Dashboard: Đang mở đăng ký
        public DataTable LayGiaiDangMoDangKy(int top)
        {
            string query = @"
SELECT TOP " + top + @"
    g.ma_giai_dau, g.ten_giai_dau, g.trang_thai, g.tong_giai_thuong, g.ngay_bat_dau,
    tc.ma_tro_choi, tc.ten_game,
    ISNULL(v.tong_like,0) AS tong_like, ISNULL(v.tong_theo_doi,0) AS tong_theo_doi
FROM GIAI_DAU g
LEFT JOIN TRO_CHOI tc ON tc.ma_tro_choi = g.ma_tro_choi
LEFT JOIN VW_TUONG_TAC_TONG_HOP v ON v.ma_giai_dau = g.ma_giai_dau
WHERE g.trang_thai = 'mo_dang_ky'
  AND ISNULL(g.is_deleted,0) = 0
ORDER BY g.ngay_bat_dau ASC;";
            return DataProvider.ExecuteQuery(query, null);
        }

        // Game page: tournaments by game with optional status filter
        public DataTable LayGiaiTheoGame(int maTroChoi, string trangThai)
        {
            string query = @"
SELECT g.ma_giai_dau, g.ten_giai_dau, g.trang_thai, g.tong_giai_thuong, g.ngay_bat_dau,
    tc.ma_tro_choi, tc.ten_game,
    ISNULL(v.tong_like,0) AS tong_like, ISNULL(v.tong_theo_doi,0) AS tong_theo_doi
FROM GIAI_DAU g
LEFT JOIN TRO_CHOI tc ON tc.ma_tro_choi = g.ma_tro_choi
LEFT JOIN VW_TUONG_TAC_TONG_HOP v ON v.ma_giai_dau = g.ma_giai_dau
WHERE g.ma_tro_choi = @MaTroChoi
  AND ISNULL(g.is_deleted,0) = 0";

            var parms = new List<SqlParameter> { new SqlParameter("@MaTroChoi", SqlDbType.Int) { Value = maTroChoi } };

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query += " AND g.trang_thai = @TrangThai";
                parms.Add(new SqlParameter("@TrangThai", SqlDbType.NVarChar) { Value = trangThai });
            }
            query += " ORDER BY g.ngay_bat_dau DESC;";
            return DataProvider.ExecuteQuery(query, parms.ToArray());
        }

        // Giải đã tham gia (qua đội)
        public DataTable LayGiaiDaThamGia(int maNguoiDung)
        {
            const string query = @"
SELECT DISTINCT g.ma_giai_dau, g.ten_giai_dau, g.trang_thai, g.tong_giai_thuong, g.ngay_bat_dau,
    tc.ten_game, d.ten_doi, nd.ten_nhom
FROM THANH_VIEN_DOI tv
JOIN NHOM_DOI nd ON nd.ma_nhom = tv.ma_nhom
JOIN DOI d ON d.ma_doi = nd.ma_doi
JOIN TRO_CHOI tc ON tc.ma_tro_choi = nd.ma_tro_choi
JOIN THAM_GIA_GIAI tgg ON tgg.ma_nhom = nd.ma_nhom AND tgg.trang_thai_duyet = 'da_duyet'
JOIN GIAI_DAU g ON g.ma_giai_dau = tgg.ma_giai_dau
WHERE tv.ma_nguoi_dung = @MaNguoiDung AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
  AND ISNULL(g.is_deleted,0) = 0
ORDER BY g.ngay_bat_dau DESC;";
            return DataProvider.ExecuteQuery(query, new[] { new SqlParameter("@MaNguoiDung", SqlDbType.Int) { Value = maNguoiDung } });
        }
    }
}
