using System;
using System.Data;
using System.Data.SqlClient;
using DAL;

namespace DAL
{
    public class RecruitmentDAL
    {
        public int TaoBaiDang(int maDoi, int maNhom, int maViTri, string noiDung)
        {
            const string query = @"
INSERT INTO BAI_DANG_TUYEN_DUNG(ma_doi, ma_nhom, ma_vi_tri, noi_dung, trang_thai)
OUTPUT INSERTED.ma_bai_dang
VALUES(@MaDoi, @MaNhom, @MaViTri, @NoiDung, 'dang_mo');";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaDoi", SqlDbType.Int){ Value = maDoi },
                new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom },
                new SqlParameter("@MaViTri", SqlDbType.Int){ Value = maViTri },
                new SqlParameter("@NoiDung", SqlDbType.NVarChar){ Value = noiDung.Trim() }
            });

            return Convert.ToInt32(result);
        }

        public int TaoDonUngTuyen(int maBaiDang, int maUngVien)
        {
            const string query = @"
INSERT INTO DON_UNG_TUYEN(ma_bai_dang, ma_ung_vien, trang_thai)
OUTPUT INSERTED.ma_don
VALUES(@MaBaiDang, @MaUngVien, 'cho_duyet');";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaBaiDang", SqlDbType.Int){ Value = maBaiDang },
                new SqlParameter("@MaUngVien", SqlDbType.Int){ Value = maUngVien }
            });

            return Convert.ToInt32(result);
        }

        public int TaoLoiMoi(int maDoi, int? maNhom, int maNguoiDuocMoi, int? maNguoiGui = null)
        {
            const string query = @"
INSERT INTO LOI_MOI_GIA_NHAP(ma_doi, ma_nhom, ma_nguoi_duoc_moi, ma_nguoi_gui, trang_thai)
OUTPUT INSERTED.ma_loi_moi
VALUES(@MaDoi, @MaNhom, @MaNguoiDuocMoi, @MaNguoiGui, 'cho_phan_hoi');";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaDoi", SqlDbType.Int){ Value = maDoi },
                new SqlParameter("@MaNhom", SqlDbType.Int){ Value = (object)maNhom ?? DBNull.Value },
                new SqlParameter("@MaNguoiDuocMoi", SqlDbType.Int){ Value = maNguoiDuocMoi },
                new SqlParameter("@MaNguoiGui", SqlDbType.Int){ Value = (object)maNguoiGui ?? DBNull.Value }
            });

            return Convert.ToInt32(result);
        }

        public bool NguoiDungDangOTrangThaiFreeAgent(int maNguoiDung)
        {
            const string query = @"
SELECT COUNT(1)
FROM THANH_VIEN_DOI
WHERE ma_nguoi_dung = @MaNguoiDung
  AND trang_thai_hop_dong = 'dang_hieu_luc';";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung }
            });

            return Convert.ToInt32(result) == 0;
        }

        public DataRow LayDonUngTuyen(int maDon)
        {
            const string query = @"
SELECT du.ma_don, du.ma_bai_dang, du.ma_ung_vien, du.trang_thai,
       bd.ma_doi, bd.ma_nhom
FROM DON_UNG_TUYEN du
JOIN BAI_DANG_TUYEN_DUNG bd ON bd.ma_bai_dang = du.ma_bai_dang
WHERE du.ma_don = @MaDon;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaDon", SqlDbType.Int){ Value = maDon }
            });

            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        public DataRow LayLoiMoi(int maLoiMoi)
        {
            const string query = @"
SELECT ma_loi_moi, ma_doi, ma_nhom, ma_nguoi_duoc_moi, trang_thai
FROM LOI_MOI_GIA_NHAP
WHERE ma_loi_moi = @MaLoiMoi;";

            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaLoiMoi", SqlDbType.Int){ Value = maLoiMoi }
            });

            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        public bool CapNhatTrangThaiDon(int maDon, string trangThai)
        {
            const string query = "UPDATE DON_UNG_TUYEN SET trang_thai = @TrangThai WHERE ma_don = @MaDon";
            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@TrangThai", SqlDbType.NVarChar){ Value = trangThai },
                new SqlParameter("@MaDon", SqlDbType.Int){ Value = maDon }
            });
            return affected > 0;
        }

        public bool CapNhatTrangThaiLoiMoi(int maLoiMoi, string trangThai)
        {
            const string query = "UPDATE LOI_MOI_GIA_NHAP SET trang_thai = @TrangThai WHERE ma_loi_moi = @MaLoiMoi";
            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@TrangThai", SqlDbType.NVarChar){ Value = trangThai },
                new SqlParameter("@MaLoiMoi", SqlDbType.Int){ Value = maLoiMoi }
            });
            return affected > 0;
        }

        public bool NguoiCoQuyenQuanLyNhom(int maNguoiDung, int maNhom)
        {
            const string query = @"
SELECT COUNT(1)
FROM THANH_VIEN_DOI
WHERE ma_nguoi_dung = @MaNguoiDung
  AND ma_nhom = @MaNhom
  AND vai_tro_noi_bo IN ('ban_dieu_hanh', 'doi_truong')
  AND trang_thai_hop_dong = 'dang_hieu_luc';";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung },
                new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom }
            });

            return Convert.ToInt32(result) > 0;
        }

        public bool TiepNhanUngVienVaoNhom(int maNhom, int maNguoiDung)
        {
            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        DataProvider.ExecuteNonQuery(@"
UPDATE THANH_VIEN_DOI
SET trang_thai_hop_dong = 'tu_do'
WHERE ma_nguoi_dung = @MaNguoiDung
  AND trang_thai_hop_dong = 'dang_hieu_luc';", new[]
                        {
                            new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung }
                        }, conn, tran);

                        DataProvider.ExecuteNonQuery(@"
INSERT INTO THANH_VIEN_DOI(ma_nguoi_dung, ma_nhom, vai_tro_noi_bo, phan_he, ma_vi_tri, trang_thai_duyet, trang_thai_hop_dong)
VALUES(@MaNguoiDung, @MaNhom, 'member', 'thi_dau', NULL, 'da_duyet', 'dang_hieu_luc');", new[]
                        {
                            new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung },
                            new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom }
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
        }

        public void TuChoiDonConLaiCuaUngVien(int maUngVien, int maDonDuocChapNhan)
        {
            const string query = @"
UPDATE DON_UNG_TUYEN
SET trang_thai = 'tu_choi'
WHERE ma_ung_vien = @MaUngVien
  AND ma_don <> @MaDonDuocChapNhan
  AND trang_thai = 'cho_duyet';";

            DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaUngVien", SqlDbType.Int){ Value = maUngVien },
                new SqlParameter("@MaDonDuocChapNhan", SqlDbType.Int){ Value = maDonDuocChapNhan }
            });
        }

        public void TuChoiLoiMoiConLaiCuaUngVien(int maUngVien, int maLoiMoiChapNhan)
        {
            const string query = @"
UPDATE LOI_MOI_GIA_NHAP
SET trang_thai = 'tu_choi'
WHERE ma_nguoi_duoc_moi = @MaUngVien
  AND ma_loi_moi <> @MaLoiMoiChapNhan
  AND trang_thai = 'cho_phan_hoi';";

            DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaUngVien", SqlDbType.Int){ Value = maUngVien },
                new SqlParameter("@MaLoiMoiChapNhan", SqlDbType.Int){ Value = maLoiMoiChapNhan }
            });
        }
    }
}
