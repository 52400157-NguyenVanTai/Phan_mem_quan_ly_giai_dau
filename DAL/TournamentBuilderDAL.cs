using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DTO;

namespace DAL
{
    public class TournamentBuilderDAL
    {
        public int TaoGiaiDau(TaoGiaiDauDTO dto, string trangThai)
        {
            const string query = @"
INSERT INTO GIAI_DAU
(
    ten_giai_dau, ma_nguoi_tao, ma_tro_choi, the_thuc, banner_url, tong_giai_thuong, mo_ta, so_nguoi_moi_doi,
    thoi_gian_mo_dang_ky, thoi_gian_dong_dang_ky, ngay_bat_dau, ngay_ket_thuc,
    trang_thai, hien_thi_public, is_deleted
)
OUTPUT INSERTED.ma_giai_dau
VALUES
(
    @TenGiaiDau, @MaNguoiTao, @MaTroChoi, @TheThuc, @BannerUrl, @TongGiaiThuong, @MoTa, @SoNguoiMoiDoi,
    @ThoiGianMoDangKy, @ThoiGianDongDangKy, @NgayBatDau, @NgayKetThuc,
    @TrangThai, 0, 0
);";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@TenGiaiDau", SqlDbType.NVarChar){ Value = dto.TenGiaiDau.Trim() },
                new SqlParameter("@MaNguoiTao", SqlDbType.Int){ Value = dto.MaNguoiTao },
                new SqlParameter("@MaTroChoi", SqlDbType.Int){ Value = (object)dto.MaTroChoi ?? DBNull.Value },
                new SqlParameter("@TheThuc", SqlDbType.NVarChar){ Value = (object)dto.TheThuc ?? DBNull.Value },
                new SqlParameter("@BannerUrl", SqlDbType.NVarChar){ Value = (object)dto.BannerUrl ?? DBNull.Value },
                new SqlParameter("@TongGiaiThuong", SqlDbType.Decimal){ Value = dto.TongGiaiThuong },
                new SqlParameter("@MoTa", SqlDbType.NVarChar){ Value = (object)dto.MoTa ?? DBNull.Value },
                new SqlParameter("@SoNguoiMoiDoi", SqlDbType.Int){ Value = (object)dto.SoNguoiMoiDoi ?? DBNull.Value },
                new SqlParameter("@ThoiGianMoDangKy", SqlDbType.DateTime){ Value = (object)dto.ThoiGianMoDangKy ?? DBNull.Value },
                new SqlParameter("@ThoiGianDongDangKy", SqlDbType.DateTime){ Value = (object)dto.ThoiGianDongDangKy ?? DBNull.Value },
                new SqlParameter("@NgayBatDau", SqlDbType.DateTime){ Value = dto.NgayBatDau },
                new SqlParameter("@NgayKetThuc", SqlDbType.DateTime){ Value = dto.NgayKetThuc },
                new SqlParameter("@TrangThai", SqlDbType.NVarChar){ Value = trangThai }
            });

            return Convert.ToInt32(result);
        }

        public void ThemGiaiThuong(int maGiaiDau, List<TaoGiaiThuongDTO> giaiThuongs)
        {
            if (giaiThuongs == null || giaiThuongs.Count == 0) return;

            foreach (var item in giaiThuongs)
            {
                const string q = @"INSERT INTO GIAI_THUONG(ma_giai_dau, ten_giai, phan_thuong, mo_ta) 
                                   VALUES(@MaGiaiDau, @TenGiai, @PhanThuong, @MoTa)";
                DataProvider.ExecuteNonQuery(q, new[]
                {
                    new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                    new SqlParameter("@TenGiai", SqlDbType.NVarChar){ Value = (object)item.TenGiai ?? DBNull.Value },
                    new SqlParameter("@PhanThuong", SqlDbType.NVarChar){ Value = (object)item.PhanThuong ?? DBNull.Value },
                    new SqlParameter("@MoTa", SqlDbType.NVarChar){ Value = (object)item.MoTa ?? DBNull.Value }
                });
            }
        }

        public DataRow LayGiaiTheoId(int maGiaiDau)
        {
            const string query = "SELECT * FROM GIAI_DAU WHERE ma_giai_dau = @MaGiaiDau AND is_deleted = 0";
            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });

            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        public bool CapNhatTrangThaiGiai(CapNhatTrangThaiGiaiDTO dto)
        {
            const string query = @"
UPDATE GIAI_DAU
SET trang_thai = @TrangThaiMoi,
    hien_thi_public = @HienThiPublic,
    thoi_gian_khoa = @ThoiGianKhoa,
    ma_nguoi_khoa = @MaNguoiKhoa,
    ly_do_khoa = @LyDoKhoa
WHERE ma_giai_dau = @MaGiaiDau AND is_deleted = 0";

            bool laTrangThaiKhoa = string.Equals(dto.TrangThaiMoi, "khoa", StringComparison.OrdinalIgnoreCase);

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@TrangThaiMoi", SqlDbType.NVarChar){ Value = dto.TrangThaiMoi },
                new SqlParameter("@HienThiPublic", SqlDbType.Bit){ Value = laTrangThaiKhoa ? 0 : 1 },
                new SqlParameter("@ThoiGianKhoa", SqlDbType.DateTime){ Value = laTrangThaiKhoa ? (object)DateTime.Now : DBNull.Value },
                new SqlParameter("@MaNguoiKhoa", SqlDbType.Int){ Value = laTrangThaiKhoa ? (object)dto.MaNguoiThucHien : DBNull.Value },
                new SqlParameter("@LyDoKhoa", SqlDbType.NVarChar){ Value = laTrangThaiKhoa ? (object)(dto.LyDo ?? "") : DBNull.Value },
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = dto.MaGiaiDau }
            });

            return affected > 0;
        }

        public bool GuiXetDuyet(int maGiaiDau)
        {
            const string query = @"
UPDATE GIAI_DAU
SET trang_thai = 'cho_phe_duyet',
    hien_thi_public = 0
WHERE ma_giai_dau = @MaGiaiDau
  AND trang_thai = 'ban_nhap'
  AND is_deleted = 0";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });

            return affected > 0;
        }

        public bool PheDuyetGiai(int maGiaiDau)
        {
            const string query = @"
UPDATE GIAI_DAU
SET trang_thai = 'mo_dang_ky',
    hien_thi_public = 1,
    thoi_gian_khoa = NULL,
    ma_nguoi_khoa = NULL,
    ly_do_khoa = NULL
WHERE ma_giai_dau = @MaGiaiDau
  AND trang_thai = 'cho_phe_duyet'
  AND is_deleted = 0";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });

            return affected > 0;
        }

        public bool XoaCungGiai(int maGiaiDau)
        {
            const string query = "EXEC SP_XoaXachGiaiDau @MaGiaiDau";
            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });
            return affected >= 0;
        }

        public int TaoGiaiDoan(TaoGiaiDoanDTO dto)
        {
            const string query = @"
DECLARE @ThuTuMoi INT = ISNULL((SELECT MAX(thu_tu) FROM GIAI_DOAN WHERE ma_giai_dau = @MaGiaiDau), 0) + 1;
INSERT INTO GIAI_DOAN(ma_giai_dau, ten_giai_doan, the_thuc, thu_tu, so_doi_di_tiep, diem_nguong_match_point)
OUTPUT INSERTED.ma_giai_doan
VALUES(@MaGiaiDau, @TenGiaiDoan, @TheThuc, @ThuTuMoi, @SoDoiDiTiep, @DiemNguongMatchPoint);";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = dto.MaGiaiDau },
                new SqlParameter("@TenGiaiDoan", SqlDbType.NVarChar){ Value = dto.TenGiaiDoan.Trim() },
                new SqlParameter("@TheThuc", SqlDbType.NVarChar){ Value = dto.TheThuc.Trim() },
                new SqlParameter("@SoDoiDiTiep", SqlDbType.Int){ Value = dto.SoDoiDiTiep },
                new SqlParameter("@DiemNguongMatchPoint", SqlDbType.Int){ Value = (object)dto.DiemNguongMatchPoint ?? DBNull.Value }
            });

            return Convert.ToInt32(result);
        }

        public bool XoaGiaiDoan(int maGiaiDoan)
        {
            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        DataTable target = DataProvider.ExecuteQuery(
                            "SELECT ma_giai_dau, thu_tu FROM GIAI_DOAN WHERE ma_giai_doan = @MaGiaiDoan",
                            new[] { new SqlParameter("@MaGiaiDoan", SqlDbType.Int) { Value = maGiaiDoan } }, conn, tran);

                        if (target.Rows.Count == 0)
                        {
                            tran.Rollback();
                            return false;
                        }

                        int maGiaiDau = Convert.ToInt32(target.Rows[0]["ma_giai_dau"]);
                        int thuTu = Convert.ToInt32(target.Rows[0]["thu_tu"]);

                        DataProvider.ExecuteNonQuery(
                            "DELETE FROM GIAI_DOAN WHERE ma_giai_doan = @MaGiaiDoan",
                            new[] { new SqlParameter("@MaGiaiDoan", SqlDbType.Int) { Value = maGiaiDoan } }, conn, tran);

                        DataProvider.ExecuteNonQuery(
                            "UPDATE GIAI_DOAN SET thu_tu = thu_tu - 1 WHERE ma_giai_dau = @MaGiaiDau AND thu_tu > @ThuTu",
                            new[]
                            {
                                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                                new SqlParameter("@ThuTu", SqlDbType.Int){ Value = thuTu }
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

        public bool HoanDoiThuTuGiaiDoan(int maGiaiDau, int thuTuA, int thuTuB)
        {
            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        int affectedA = DataProvider.ExecuteNonQuery(
                            "UPDATE GIAI_DOAN SET thu_tu = -1 WHERE ma_giai_dau = @MaGiaiDau AND thu_tu = @ThuTuA",
                            new[]
                            {
                                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                                new SqlParameter("@ThuTuA", SqlDbType.Int){ Value = thuTuA }
                            }, conn, tran);

                        int affectedB = DataProvider.ExecuteNonQuery(
                            "UPDATE GIAI_DOAN SET thu_tu = @ThuTuA WHERE ma_giai_dau = @MaGiaiDau AND thu_tu = @ThuTuB",
                            new[]
                            {
                                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                                new SqlParameter("@ThuTuA", SqlDbType.Int){ Value = thuTuA },
                                new SqlParameter("@ThuTuB", SqlDbType.Int){ Value = thuTuB }
                            }, conn, tran);

                        int affectedC = DataProvider.ExecuteNonQuery(
                            "UPDATE GIAI_DOAN SET thu_tu = @ThuTuB WHERE ma_giai_dau = @MaGiaiDau AND thu_tu = -1",
                            new[]
                            {
                                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                                new SqlParameter("@ThuTuB", SqlDbType.Int){ Value = thuTuB }
                            }, conn, tran);

                        if (affectedA == 0 || affectedB == 0 || affectedC == 0)
                        {
                            tran.Rollback();
                            return false;
                        }

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

        public List<GiaiDoanDTO> LayDanhSachGiaiDoan(int maGiaiDau)
        {
            const string query = "SELECT * FROM GIAI_DOAN WHERE ma_giai_dau = @MaGiaiDau ORDER BY thu_tu";
            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });

            List<GiaiDoanDTO> list = new List<GiaiDoanDTO>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new GiaiDoanDTO
                {
                    MaGiaiDoan = Convert.ToInt32(row["ma_giai_doan"]),
                    MaGiaiDau = Convert.ToInt32(row["ma_giai_dau"]),
                    TenGiaiDoan = row["ten_giai_doan"].ToString(),
                    TheThuc = row["the_thuc"].ToString(),
                    ThuTu = Convert.ToInt32(row["thu_tu"]),
                    SoDoiDiTiep = Convert.ToInt32(row["so_doi_di_tiep"]),
                    DiemNguongMatchPoint = row["diem_nguong_match_point"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["diem_nguong_match_point"])
                });
            }

            return list;
        }

        public int LaySoLuongThamGiaDaDuyet(int maGiaiDau)
        {
            const string query = "SELECT COUNT(1) FROM THAM_GIA_GIAI WHERE ma_giai_dau = @MaGiaiDau AND trang_thai_duyet = 'da_duyet'";
            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });
            return Convert.ToInt32(result);
        }

        public bool NhomKhacGameGiai(int maGiaiDau, int maNhom)
        {
            const string query = @"
SELECT COUNT(1)
FROM GIAI_DAU g
JOIN NHOM_DOI n ON n.ma_nhom = @MaNhom
WHERE g.ma_giai_dau = @MaGiaiDau
  AND g.ma_tro_choi IS NOT NULL
  AND n.ma_tro_choi <> g.ma_tro_choi;";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom }
            });
            return Convert.ToInt32(result) > 0;
        }

        public bool CapNhatDuyetThamGia(int maGiaiDau, int maNhom, bool chapNhan)
        {
            const string query = @"
UPDATE THAM_GIA_GIAI
SET trang_thai_duyet = @TrangThai
WHERE ma_giai_dau = @MaGiaiDau
  AND ma_nhom = @MaNhom";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@TrangThai", SqlDbType.NVarChar){ Value = chapNhan ? "da_duyet" : "bi_tu_choi" },
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom }
            });
            return affected > 0;
        }

        public bool CapNhatHatGiong(int maGiaiDau, int maNhom, int hatGiong)
        {
            const string query = @"
UPDATE THAM_GIA_GIAI
SET hat_giong = @HatGiong
WHERE ma_giai_dau = @MaGiaiDau
  AND ma_nhom = @MaNhom
  AND trang_thai_duyet = 'da_duyet';";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@HatGiong", SqlDbType.Int){ Value = hatGiong },
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom }
            });
            return affected > 0;
        }

        public int DongBoDoiHinhThiDau(int maGiaiDau, int maNhom)
        {
            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        DataTable thamGia = DataProvider.ExecuteQuery(@"
SELECT ma_tham_gia
FROM THAM_GIA_GIAI
WHERE ma_giai_dau = @MaGiaiDau
  AND ma_nhom = @MaNhom
  AND trang_thai_duyet = 'da_duyet'", new[]
                        {
                            new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                            new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom }
                        }, conn, tran);

                        if (thamGia.Rows.Count == 0)
                        {
                            tran.Rollback();
                            return 0;
                        }

                        int maThamGia = Convert.ToInt32(thamGia.Rows[0]["ma_tham_gia"]);

                        DataProvider.ExecuteNonQuery(@"
DELETE FROM DOI_HINH_THI_DAU
WHERE ma_giai_dau = @MaGiaiDau
  AND ma_tham_gia = @MaThamGia", new[]
                        {
                            new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                            new SqlParameter("@MaThamGia", SqlDbType.Int){ Value = maThamGia }
                        }, conn, tran);

                        int inserted = DataProvider.ExecuteNonQuery(@"
INSERT INTO DOI_HINH_THI_DAU(ma_tham_gia, ma_nguoi_dung, ma_vi_tri, is_du_bi, ma_giai_dau)
SELECT @MaThamGia, tv.ma_nguoi_dung, tv.ma_vi_tri, 0, @MaGiaiDau
FROM THANH_VIEN_DOI tv
WHERE tv.ma_nhom = @MaNhom
  AND tv.trang_thai_duyet = 'da_duyet'
  AND tv.trang_thai_hop_dong = 'dang_hieu_luc';", new[]
                        {
                            new SqlParameter("@MaThamGia", SqlDbType.Int){ Value = maThamGia },
                            new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                            new SqlParameter("@MaNhom", SqlDbType.Int){ Value = maNhom }
                        }, conn, tran);

                        tran.Commit();
                        return inserted;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool CapNhatTrangThaiGiaiTheoMa(int maGiaiDau, string trangThai)
        {
            const string query = @"
UPDATE GIAI_DAU
SET trang_thai = @TrangThai,
    hien_thi_public = CASE WHEN @TrangThai = 'khoa' THEN 0 ELSE 1 END
WHERE ma_giai_dau = @MaGiaiDau
  AND is_deleted = 0";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@TrangThai", SqlDbType.NVarChar){ Value = trangThai },
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });

            return affected > 0;
        }

        public DataTable LayDanhSachGiaiCuaToi(int maNguoiTao)
        {
            const string query = @"
SELECT g.*, t.ten_game 
FROM GIAI_DAU g
LEFT JOIN TRO_CHOI t ON g.ma_tro_choi = t.ma_tro_choi
WHERE g.ma_nguoi_tao = @MaNguoiTao AND g.is_deleted = 0
ORDER BY g.ma_giai_dau DESC;";

            return DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaNguoiTao", SqlDbType.Int){ Value = maNguoiTao }
            });
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

        public DataTable LayGiaiCuaToi(int maNguoiDung)
        {
            const string query = @"
SELECT DISTINCT
       gd.ma_giai_dau,
       gd.ten_giai_dau,
       gd.trang_thai,
       gd.tong_giai_thuong,
       gd.ngay_bat_dau,
       gd.ngay_ket_thuc,
       tc.ten_game,
       gd.ma_tro_choi,
       CASE WHEN gd.ma_nguoi_tao = @MaNguoiDung THEN 1 ELSE 0 END AS is_owner
FROM GIAI_DAU gd
LEFT JOIN TRO_CHOI tc ON gd.ma_tro_choi = tc.ma_tro_choi
LEFT JOIN THAM_GIA_GIAI tg ON tg.ma_giai_dau = gd.ma_giai_dau
LEFT JOIN NHOM_DOI nd ON nd.ma_nhom = tg.ma_nhom
LEFT JOIN THANH_VIEN_DOI tv ON tv.ma_nhom = nd.ma_nhom
WHERE gd.is_deleted = 0
  AND
  (
      gd.ma_nguoi_tao = @MaNguoiDung
      OR
      (
          tv.ma_nguoi_dung = @MaNguoiDung
          AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
          AND tg.trang_thai_duyet IN ('cho_duyet', 'da_duyet')
      )
  )
ORDER BY gd.ma_giai_dau DESC;";

            return DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung }
            });
        }

        public DataTable LayDanhSachDangKyDoi(int maGiaiDau)
        {
            const string query = @"
SELECT tg.ma_nhom,
       tg.trang_thai_duyet,
       tg.hat_giong,
       nd.ten_nhom,
       d.ma_doi,
       d.ten_doi,
       d.slogan,
       tc.ten_game
FROM THAM_GIA_GIAI tg
JOIN NHOM_DOI nd ON nd.ma_nhom = tg.ma_nhom
JOIN DOI d ON d.ma_doi = nd.ma_doi
LEFT JOIN TRO_CHOI tc ON tc.ma_tro_choi = nd.ma_tro_choi
WHERE tg.ma_giai_dau = @MaGiaiDau
ORDER BY CASE tg.trang_thai_duyet
            WHEN 'cho_duyet' THEN 0
            WHEN 'da_duyet' THEN 1
            ELSE 2
         END,
         nd.ten_nhom ASC;";

            return DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });
        }
    }
}
