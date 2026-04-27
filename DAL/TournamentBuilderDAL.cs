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
    ten_giai_dau, ma_nguoi_tao, ma_tro_choi, the_thuc, banner_url, tong_giai_thuong, mo_ta,
    so_doi_toi_thieu, so_doi_toi_da,
    ngay_bat_dau, ngay_ket_thuc,
    trang_thai, hien_thi_public, dang_mo_dang_ky, is_deleted
)
OUTPUT INSERTED.ma_giai_dau
VALUES
(
    @TenGiaiDau, @MaNguoiTao, @MaTroChoi, @TheThuc, @BannerUrl, @TongGiaiThuong, @MoTa,
    @SoDoiToiThieu, @SoDoiToiDa,
    @NgayBatDau, @NgayKetThuc,
    @TrangThai, 0, 0, 0
);";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@TenGiaiDau", SqlDbType.NVarChar){ Value = dto.TenGiaiDau.Trim() },
                new SqlParameter("@MaNguoiTao", SqlDbType.Int){ Value = dto.MaNguoiTao },
                new SqlParameter("@MaTroChoi", SqlDbType.Int){ Value = (object)dto.MaTroChoi ?? DBNull.Value },
                new SqlParameter("@TheThuc", SqlDbType.NVarChar){ Value = dto.TheThuc },
                new SqlParameter("@BannerUrl", SqlDbType.NVarChar){ Value = (object)dto.BannerUrl ?? DBNull.Value },
                new SqlParameter("@TongGiaiThuong", SqlDbType.Decimal){ Value = dto.TongGiaiThuong },
                new SqlParameter("@MoTa", SqlDbType.NVarChar){ Value = (object)dto.MoTa ?? DBNull.Value },
                new SqlParameter("@SoDoiToiThieu", SqlDbType.Int){ Value = dto.SoDoiToiThieu },
                new SqlParameter("@SoDoiToiDa", SqlDbType.Int){ Value = (object)dto.SoDoiToiDa ?? DBNull.Value },
                new SqlParameter("@NgayBatDau", SqlDbType.DateTime){ Value = (object)dto.NgayBatDau ?? DBNull.Value },
                new SqlParameter("@NgayKetThuc", SqlDbType.DateTime){ Value = (object)dto.NgayKetThuc ?? DBNull.Value },
                new SqlParameter("@TrangThai", SqlDbType.NVarChar){ Value = trangThai }
            });

            return Convert.ToInt32(result);
        }

        public void ThemGiaiThuong(int maGiaiDau, List<TaoGiaiThuongDTO> giaiThuongs)
        {
            if (giaiThuongs == null || giaiThuongs.Count == 0) return;

            for (int i = 0; i < giaiThuongs.Count; i++)
            {
                TaoGiaiThuongDTO item = giaiThuongs[i];
                const string q = @"INSERT INTO GIAI_THUONG(ma_giai_dau, vi_tri_top, so_tien, ten_giai, gia_tri, so_luong, mo_ta) 
                                   VALUES(@MaGiaiDau, @ViTriTop, @SoTien, @TenGiai, @GiaTri, @SoLuong, @MoTa)";
                DataProvider.ExecuteNonQuery(q, new[]
                {
                    new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                    new SqlParameter("@ViTriTop", SqlDbType.Int){ Value = i + 1 },
                    new SqlParameter("@SoTien", SqlDbType.Decimal){ Value = item.GiaTri },
                    new SqlParameter("@TenGiai", SqlDbType.NVarChar){ Value = (object)item.TenGiai ?? DBNull.Value },
                    new SqlParameter("@GiaTri", SqlDbType.Decimal){ Value = item.GiaTri },
                    new SqlParameter("@SoLuong", SqlDbType.Int){ Value = item.SoLuong },
                    new SqlParameter("@MoTa", SqlDbType.NVarChar){ Value = (object)item.MoTa ?? DBNull.Value }
                });
            }
        }

        public void ThemGiaiDoan(int maGiaiDau, List<TaoGiaiDoanDTO> giaiDoan)
        {
            if (giaiDoan == null || giaiDoan.Count == 0) return;

            foreach (var item in giaiDoan)
            {
                const string q = @"INSERT INTO GIAI_DOAN(ma_giai_dau, ten_giai_doan, thu_tu, the_thuc, so_doi_di_tiep, ngay_bat_dau, ngay_ket_thuc) 
                                   VALUES(@MaGiaiDau, @TenGiaiDoan, @ThuTu, @TheThuc, @SoDoiDiTiep, @NgayBatDau, @NgayKetThuc)";
                DataProvider.ExecuteNonQuery(q, new[]
                {
                    new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                    new SqlParameter("@TenGiaiDoan", SqlDbType.NVarChar){ Value = "Giai đoạn " + item.ThuTu },
                    new SqlParameter("@ThuTu", SqlDbType.Int){ Value = item.ThuTu },
                    new SqlParameter("@TheThuc", SqlDbType.NVarChar){ Value = item.TheThuc },
                    new SqlParameter("@SoDoiDiTiep", SqlDbType.Int){ Value = item.SoDoiDiTiep ?? 2 },
                    new SqlParameter("@NgayBatDau", SqlDbType.DateTime){ Value = (object)item.NgayBatDau ?? DBNull.Value },
                    new SqlParameter("@NgayKetThuc", SqlDbType.DateTime){ Value = (object)item.NgayKetThuc ?? DBNull.Value }
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
SET trang_thai = 'cho_xet_duyet',
    hien_thi_public = 0
WHERE ma_giai_dau = @MaGiaiDau
  AND trang_thai = 'nhap'
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
SET trang_thai = 'chuan_bi_dien_ra',
    hien_thi_public = 1,
    thoi_gian_khoa = NULL,
    ma_nguoi_khoa = NULL,
    ly_do_khoa = NULL
WHERE ma_giai_dau = @MaGiaiDau
  AND trang_thai = 'cho_xet_duyet'
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
            // This method is for the old stage addition flow (after tournament creation)
            // It requires the old DTO structure which no longer exists
            // For now, return 0 as this flow is deprecated
            return 0;
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
    hien_thi_public = CASE
        WHEN @TrangThai IN ('chuan_bi_dien_ra', 'dang_dien_ra', 'tong_ket', 'ket_thuc') THEN 1
        ELSE 0
    END
WHERE ma_giai_dau = @MaGiaiDau
  AND is_deleted = 0";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@TrangThai", SqlDbType.NVarChar){ Value = trangThai },
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });

            return affected > 0;
        }

        public bool CapNhatGiaiDau(int maGiaiDau, TaoGiaiDauDTO dto)
        {
            const string query = @"
UPDATE GIAI_DAU
SET ten_giai_dau = @TenGiaiDau,
    ma_tro_choi = @MaTroChoi,
    banner_url = @BannerUrl,
    tong_giai_thuong = @TongGiaiThuong,
    mo_ta = @MoTa,
    ngay_bat_dau = @NgayBatDau,
    ngay_ket_thuc = @NgayKetThuc
WHERE ma_giai_dau = @MaGiaiDau AND is_deleted = 0";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@TenGiaiDau", SqlDbType.NVarChar){ Value = dto.TenGiaiDau.Trim() },
                new SqlParameter("@MaTroChoi", SqlDbType.Int){ Value = (object)dto.MaTroChoi ?? DBNull.Value },
                new SqlParameter("@BannerUrl", SqlDbType.NVarChar){ Value = (object)dto.BannerUrl ?? DBNull.Value },
                new SqlParameter("@TongGiaiThuong", SqlDbType.Decimal){ Value = dto.TongGiaiThuong },
                new SqlParameter("@MoTa", SqlDbType.NVarChar){ Value = (object)dto.MoTa ?? DBNull.Value },
                new SqlParameter("@NgayBatDau", SqlDbType.DateTime){ Value = (object)dto.NgayBatDau ?? DBNull.Value },
                new SqlParameter("@NgayKetThuc", SqlDbType.DateTime){ Value = (object)dto.NgayKetThuc ?? DBNull.Value },
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });

            return affected > 0;
        }

        public bool TamHoanGiaiDau(int maGiaiDau, DateTime ngayBatDauTamHoan, int maAdmin, string lyDo)
        {
            const string query = @"
UPDATE GIAI_DAU
SET trang_thai = 'tam_hoan',
    hien_thi_public = 0,
    thoi_gian_bat_dau_tam_hoan = @NgayBatDauTamHoan,
    thoi_gian_khoa = @NgayBatDauTamHoan,
    ma_nguoi_khoa = @MaAdmin,
    ly_do_khoa = @LyDo
WHERE ma_giai_dau = @MaGiaiDau AND is_deleted = 0";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@NgayBatDauTamHoan", SqlDbType.DateTime){ Value = ngayBatDauTamHoan },
                new SqlParameter("@MaAdmin", SqlDbType.Int){ Value = maAdmin },
                new SqlParameter("@LyDo", SqlDbType.NVarChar){ Value = (object)lyDo ?? DBNull.Value },
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });

            return affected > 0;
        }

        public bool KhoiPhucTuTamHoan(int maGiaiDau, DateTime ngayKetThucMoi)
        {
            const string query = @"
UPDATE GIAI_DAU
SET trang_thai = 'chuan_bi_dien_ra',
    hien_thi_public = 1,
    ngay_ket_thuc = @NgayKetThucMoi,
    thoi_gian_ket_thuc_tam_hoan = GETDATE(),
    thoi_gian_bat_dau_tam_hoan = NULL,
    thoi_gian_khoa = NULL,
    ma_nguoi_khoa = NULL,
    ly_do_khoa = NULL
WHERE ma_giai_dau = @MaGiaiDau AND is_deleted = 0";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@NgayKetThucMoi", SqlDbType.DateTime){ Value = ngayKetThucMoi },
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau }
            });

            return affected > 0;
        }

        public int ExecuteRawNonQuery(string query)
        {
            return DataProvider.ExecuteNonQuery(query, null);
        }

        public bool MoiTrongTai(MoiTrongTaiDTO dto)
        {
            const string query = @"
INSERT INTO TRONG_TAI_GIAI(ma_giai_dau, ma_nguoi_dung, trang_thai, thoi_gian_moi, thoi_gian_duyet, ma_nguoi_moi)
VALUES(@MaGiaiDau, @MaNguoiDung, 'da_duyet', GETDATE(), GETDATE(), @MaNguoiMoi)";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = dto.MaGiaiDau },
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = dto.MaNguoiDung },
                new SqlParameter("@MaNguoiMoi", SqlDbType.Int){ Value = dto.MaNguoiMoi }
            });

            return affected > 0;
        }

        public bool MoiBanToChuc(MoiBanToChucDTO dto)
        {
            const string query = @"
INSERT INTO BAN_TO_CHUC_GIAI(ma_giai_dau, ma_nguoi_dung, trang_thai, thoi_gian_moi, thoi_gian_duyet, ma_nguoi_moi)
VALUES(@MaGiaiDau, @MaNguoiDung, 'da_duyet', GETDATE(), GETDATE(), @MaNguoiMoi)";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = dto.MaGiaiDau },
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = dto.MaNguoiDung },
                new SqlParameter("@MaNguoiMoi", SqlDbType.Int){ Value = dto.MaNguoiMoi }
            });

            return affected > 0;
        }

        public bool CapNhatDangMoDangKy(int maGiaiDau, bool dangMo)
        {
            const string query = @"
UPDATE GIAI_DAU
SET dang_mo_dang_ky = @DangMo
WHERE ma_giai_dau = @MaGiaiDau AND is_deleted = 0";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@DangMo", SqlDbType.Bit){ Value = dangMo },
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

        public DataTable LayDanhSachGiaiTheoTrangThai(string trangThai)
        {
            const string query = @"
SELECT ma_giai_dau
FROM GIAI_DAU
WHERE trang_thai = @TrangThai AND is_deleted = 0;";

            return DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@TrangThai", SqlDbType.NVarChar){ Value = trangThai }
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

        public DataTable LayDanhSachChoXetDuyet()
        {
            const string query = @"
SELECT g.ma_giai_dau,
       g.ten_giai_dau,
       g.ma_nguoi_tao,
       g.ma_tro_choi,
       g.ngay_bat_dau,
       g.ngay_ket_thuc,
       g.trang_thai,
       nd.ten_dang_nhap AS ten_nguoi_tao,
       tc.ten_game
FROM GIAI_DAU g
LEFT JOIN NGUOI_DUNG nd ON nd.ma_nguoi_dung = g.ma_nguoi_tao
LEFT JOIN TRO_CHOI tc ON tc.ma_tro_choi = g.ma_tro_choi
WHERE g.trang_thai = 'cho_xet_duyet'
  AND g.is_deleted = 0
ORDER BY g.ma_giai_dau DESC;";

            return DataProvider.ExecuteQuery(query);
        }

        public List<int> LayDanhSachAdminHeThong()
        {
            const string query = @"
SELECT ma_nguoi_dung
FROM NGUOI_DUNG
WHERE vai_tro_he_thong = 'admin'
  AND ISNULL(is_banned, 0) = 0;";

            DataTable dt = DataProvider.ExecuteQuery(query);
            List<int> admins = new List<int>();
            foreach (DataRow row in dt.Rows)
            {
                admins.Add(Convert.ToInt32(row["ma_nguoi_dung"]));
            }

            return admins;
        }

        public string LayTenGameTheoMa(int? maTroChoi)
        {
            if (!maTroChoi.HasValue || maTroChoi.Value <= 0)
            {
                return null;
            }

            const string query = "SELECT ten_game FROM TRO_CHOI WHERE ma_tro_choi = @MaTroChoi";
            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaTroChoi", SqlDbType.Int){ Value = maTroChoi.Value }
            });

            return result == null || result == DBNull.Value ? null : result.ToString();
        }

        public void TaoThongBao(int maNguoiNhan, string tieuDe, string noiDung, string loaiThongBao, string loaiEntity = null, int? maEntity = null)
        {
            const string query = @"
INSERT INTO THONG_BAO(ma_nguoi_nhan, tieu_de, noi_dung, loai_thong_bao, loai_entity, ma_entity)
VALUES(@MaNguoiNhan, @TieuDe, @NoiDung, @LoaiThongBao, @LoaiEntity, @MaEntity);";

            DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaNguoiNhan", SqlDbType.Int){ Value = maNguoiNhan },
                new SqlParameter("@TieuDe", SqlDbType.NVarChar){ Value = tieuDe },
                new SqlParameter("@NoiDung", SqlDbType.NVarChar){ Value = (object)noiDung ?? DBNull.Value },
                new SqlParameter("@LoaiThongBao", SqlDbType.NVarChar){ Value = (object)loaiThongBao ?? DBNull.Value },
                new SqlParameter("@LoaiEntity", SqlDbType.NVarChar){ Value = (object)loaiEntity ?? DBNull.Value },
                new SqlParameter("@MaEntity", SqlDbType.Int){ Value = (object)maEntity ?? DBNull.Value }
            });
        }
    }
}
