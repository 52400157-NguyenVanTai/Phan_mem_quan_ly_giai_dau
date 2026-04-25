using System;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class TeamDAL
    {
        public bool TenDoiTonTai(string tenDoi)
        {
            const string query = "SELECT COUNT(1) FROM DOI WHERE ten_doi = @TenDoi AND trang_thai <> 'giai_tan'";
            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@TenDoi", SqlDbType.NVarChar){ Value = tenDoi.Trim() }
            });
            return Convert.ToInt32(result) > 0;
        }

        public int TaoDoi(int maManager, string tenDoi, string logoUrl, string slogan, SqlConnection conn, SqlTransaction tran)
        {
            const string query = @"
INSERT INTO DOI(ten_doi, ma_manager, logo_url, slogan, trang_thai)
OUTPUT INSERTED.ma_doi
VALUES(@TenDoi, @MaManager, @LogoUrl, @Slogan, 'dang_hoat_dong');";

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

        public bool NguoiDungDangThuocDoiKhac(int maNguoiDung)
        {
            const string query = @"
SELECT COUNT(1)
FROM THANH_VIEN_DOI tv
JOIN NHOM_DOI n ON tv.ma_nhom = n.ma_nhom
JOIN DOI d ON d.ma_doi = n.ma_doi
WHERE tv.ma_nguoi_dung = @MaNguoiDung
  AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
  AND d.trang_thai <> 'giai_tan';";

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
                    const string checkManagerQuery = "SELECT COUNT(1) FROM DOI WHERE ma_doi = @MaDoi AND ma_manager = @MaManager";
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

                    const string dissolveTeamQuery = "UPDATE DOI SET trang_thai = 'giai_tan' WHERE ma_doi = @MaDoi";
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
    }
}
