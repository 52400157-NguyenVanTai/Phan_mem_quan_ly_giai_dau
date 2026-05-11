using DTO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DAL
{
    public class DoiDAL
    {
        public List<DoiDTO> TimKiemDoi(string tuKhoa, int? maTroChoi, int? maNguoiDung)
        {
            List<DoiDTO> items = new List<DoiDTO>();
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT d.ma_doi, n.ma_nhom, n.ma_tro_choi, tc.ten_game, d.ten_doi, d.ten_viet_tat,
                                                               d.ma_doi_truong, nd.ten_dang_nhap AS ten_chu_tich, d.logo_url, d.slogan, d.mo_ta,
                                                               d.trang_thai, d.dang_tuyen, d.ngay_tao,
                                                               COUNT(tv.ma_thanh_vien) AS so_thanh_vien,
                                                               MAX(CASE WHEN tv2.ma_nguoi_dung IS NULL THEN NULL ELSE tv2.vai_tro_noi_bo END) AS vai_tro_cua_toi
                                                        FROM DOI d
                                                        INNER JOIN NHOM_DOI n ON d.ma_doi = n.ma_doi AND n.ma_tro_choi IS NOT NULL
                                                        INNER JOIN DANH_MUC_TRO_CHOI tc ON n.ma_tro_choi = tc.ma_tro_choi
                                                        INNER JOIN NGUOI_DUNG nd ON d.ma_doi_truong = nd.ma_nguoi_dung
                                                        LEFT JOIN THANH_VIEN_DOI tv ON n.ma_nhom = tv.ma_nhom AND tv.trang_thai_duyet = 'da_duyet' AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
                                                        LEFT JOIN THANH_VIEN_DOI tv2 ON n.ma_nhom = tv2.ma_nhom AND tv2.ma_nguoi_dung = @maNguoiDung AND tv2.trang_thai_duyet = 'da_duyet' AND tv2.trang_thai_hop_dong = 'dang_hieu_luc'
                                                        WHERE d.trang_thai = 'dang_hoat_dong'
                                                          AND (@tuKhoa IS NULL OR d.ten_doi LIKE '%' + @tuKhoa + '%' OR d.ten_viet_tat LIKE '%' + @tuKhoa + '%')
                                                          AND (@maTroChoi IS NULL OR n.ma_tro_choi = @maTroChoi)
                                                        GROUP BY d.ma_doi, n.ma_nhom, n.ma_tro_choi, tc.ten_game, d.ten_doi, d.ten_viet_tat,
                                                                 d.ma_doi_truong, nd.ten_dang_nhap, d.logo_url, d.slogan, d.mo_ta, d.trang_thai, d.dang_tuyen, d.ngay_tao
                                                        ORDER BY d.ngay_tao DESC", connection))
            {
                command.Parameters.AddWithValue("@tuKhoa", string.IsNullOrWhiteSpace(tuKhoa) ? (object)DBNull.Value : tuKhoa.Trim());
                command.Parameters.AddWithValue("@maTroChoi", (object)maTroChoi ?? DBNull.Value);
                command.Parameters.AddWithValue("@maNguoiDung", (object)maNguoiDung ?? DBNull.Value);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read()) items.Add(MapDoi(reader));
                }
            }
            return items;
        }

        public List<DoiDTO> TimKiemDoiTuyChon(string keyword)
        {
            List<DoiDTO> items = new List<DoiDTO>();
            if (string.IsNullOrWhiteSpace(keyword)) return items;

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT TOP 10 ma_doi, ten_doi, ten_viet_tat, logo_url
                                                        FROM DOI
                                                        WHERE trang_thai = 'dang_hoat_dong'
                                                          AND (ten_doi LIKE @kw OR ten_viet_tat LIKE @kw)", connection))
            {
                command.Parameters.AddWithValue("@kw", "%" + keyword.Trim() + "%");
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new DoiDTO
                        {
                            ma_doi = Convert.ToInt32(reader["ma_doi"]),
                            ten_doi = reader["ten_doi"].ToString(),
                            ten_viet_tat = reader["ten_viet_tat"] == DBNull.Value ? null : reader["ten_viet_tat"].ToString(),
                            logo_url = reader["logo_url"] == DBNull.Value ? null : reader["logo_url"].ToString()
                        });
                    }
                }
            }
            return items;
        }

        public List<DoiDTO> LayDoiCuaToi(int maNguoiDung)
        {
            List<DoiDTO> items = new List<DoiDTO>();
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT d.ma_doi, n.ma_nhom, n.ma_tro_choi, tc.ten_game, d.ten_doi, d.ten_viet_tat,
                                                               d.ma_doi_truong, nd.ten_dang_nhap AS ten_chu_tich, d.logo_url, d.slogan, d.mo_ta,
                                                               d.trang_thai, d.dang_tuyen, d.ngay_tao,
                                                               COUNT(tv_all.ma_thanh_vien) AS so_thanh_vien,
                                                               tv.vai_tro_noi_bo AS vai_tro_cua_toi
                                                        FROM THANH_VIEN_DOI tv
                                                        INNER JOIN NHOM_DOI n ON tv.ma_nhom = n.ma_nhom
                                                        INNER JOIN DOI d ON n.ma_doi = d.ma_doi
                                                        INNER JOIN DANH_MUC_TRO_CHOI tc ON n.ma_tro_choi = tc.ma_tro_choi
                                                        INNER JOIN NGUOI_DUNG nd ON d.ma_doi_truong = nd.ma_nguoi_dung
                                                        LEFT JOIN THANH_VIEN_DOI tv_all ON n.ma_nhom = tv_all.ma_nhom AND tv_all.trang_thai_duyet = 'da_duyet' AND tv_all.trang_thai_hop_dong = 'dang_hieu_luc'
                                                        WHERE tv.ma_nguoi_dung = @maNguoiDung AND tv.trang_thai_duyet = 'da_duyet' AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
                                                          AND d.trang_thai = 'dang_hoat_dong'
                                                        GROUP BY d.ma_doi, n.ma_nhom, n.ma_tro_choi, tc.ten_game, d.ten_doi, d.ten_viet_tat,
                                                                 d.ma_doi_truong, nd.ten_dang_nhap, d.logo_url, d.slogan, d.mo_ta, d.trang_thai, d.dang_tuyen, d.ngay_tao, tv.vai_tro_noi_bo
                                                        ORDER BY tc.ten_game, d.ten_doi", connection))
            {
                command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read()) items.Add(MapDoi(reader));
                }
            }
            return items;
        }

        public DoiDTO LayDoi(int maDoi, int? maNguoiDung)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT d.ma_doi, n.ma_nhom, n.ma_tro_choi, tc.ten_game, d.ten_doi, d.ten_viet_tat,
                                                               d.ma_doi_truong, nd.ten_dang_nhap AS ten_chu_tich, d.logo_url, d.slogan, d.mo_ta,
                                                               d.trang_thai, d.dang_tuyen, d.ngay_tao,
                                                               (SELECT COUNT(1) FROM THANH_VIEN_DOI tv WHERE tv.ma_nhom = n.ma_nhom AND tv.trang_thai_duyet = 'da_duyet' AND tv.trang_thai_hop_dong = 'dang_hieu_luc') AS so_thanh_vien,
                                                               (SELECT TOP 1 tv2.vai_tro_noi_bo FROM THANH_VIEN_DOI tv2 WHERE tv2.ma_nhom = n.ma_nhom AND tv2.ma_nguoi_dung = @maNguoiDung AND tv2.trang_thai_duyet = 'da_duyet' AND tv2.trang_thai_hop_dong = 'dang_hieu_luc') AS vai_tro_cua_toi
                                                        FROM DOI d
                                                        INNER JOIN NHOM_DOI n ON d.ma_doi = n.ma_doi AND n.ma_tro_choi IS NOT NULL
                                                        INNER JOIN DANH_MUC_TRO_CHOI tc ON n.ma_tro_choi = tc.ma_tro_choi
                                                        INNER JOIN NGUOI_DUNG nd ON d.ma_doi_truong = nd.ma_nguoi_dung
                                                        WHERE d.ma_doi = @maDoi", connection))
            {
                command.Parameters.AddWithValue("@maDoi", maDoi);
                command.Parameters.AddWithValue("@maNguoiDung", (object)maNguoiDung ?? DBNull.Value);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read()) return MapDoi(reader);
                }
            }
            return null;
        }

        public int TaoDoi(int maNguoiDung, TaoDoiRequestDTO request)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"DECLARE @maDoi INT;
                                                        INSERT INTO DOI (ten_doi, ten_viet_tat, ma_doi_truong, logo_url, slogan, mo_ta)
                                                        VALUES (@tenDoi, @tenVietTat, @maNguoiDung, @logoUrl, @slogan, @moTa);
                                                        SET @maDoi = SCOPE_IDENTITY();
                                                        INSERT INTO NHOM_DOI (ma_doi, ma_tro_choi, ten_nhom, ma_doi_truong_nhom)
                                                        VALUES (@maDoi, @maTroChoi, @tenDoi, @maNguoiDung);
                                                        INSERT INTO THANH_VIEN_DOI (ma_nguoi_dung, ma_nhom, vai_tro_noi_bo, phan_he)
                                                        VALUES (@maNguoiDung, SCOPE_IDENTITY(), 'chu_tich', 'TuyenThu');
                                                        SELECT @maDoi;", connection))
            {
                command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                command.Parameters.AddWithValue("@tenDoi", request.ten_doi.Trim());
                command.Parameters.AddWithValue("@tenVietTat", string.IsNullOrWhiteSpace(request.ten_viet_tat) ? (object)DBNull.Value : request.ten_viet_tat.Trim());
                command.Parameters.AddWithValue("@maTroChoi", request.ma_tro_choi);
                command.Parameters.AddWithValue("@logoUrl", string.IsNullOrWhiteSpace(request.logo_url) ? (object)DBNull.Value : request.logo_url.Trim());
                command.Parameters.AddWithValue("@slogan", string.IsNullOrWhiteSpace(request.slogan) ? (object)DBNull.Value : request.slogan.Trim());
                command.Parameters.AddWithValue("@moTa", string.IsNullOrWhiteSpace(request.mo_ta) ? (object)DBNull.Value : request.mo_ta.Trim());
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public bool NguoiDungDaCoDoiTheoGame(int maNguoiDung, int maTroChoi)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT COUNT(1)
                                                        FROM THANH_VIEN_DOI tv
                                                        INNER JOIN NHOM_DOI n ON tv.ma_nhom = n.ma_nhom
                                                        INNER JOIN DOI d ON n.ma_doi = d.ma_doi
                                                        WHERE tv.ma_nguoi_dung = @maNguoiDung AND n.ma_tro_choi = @maTroChoi
                                                          AND tv.trang_thai_duyet = 'da_duyet' AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
                                                          AND d.trang_thai = 'dang_hoat_dong'", connection))
            {
                command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                command.Parameters.AddWithValue("@maTroChoi", maTroChoi);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public bool TenDoiDaTonTaiTrongGame(string tenDoi, int maTroChoi, int? boQuaMaDoi = null)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT COUNT(1)
                                                        FROM DOI d
                                                        INNER JOIN NHOM_DOI n ON d.ma_doi = n.ma_doi
                                                        WHERE d.ten_doi = @tenDoi
                                                          AND n.ma_tro_choi = @maTroChoi
                                                          AND (@boQuaMaDoi IS NULL OR d.ma_doi <> @boQuaMaDoi)", connection))
            {
                command.Parameters.AddWithValue("@tenDoi", tenDoi.Trim());
                command.Parameters.AddWithValue("@maTroChoi", maTroChoi);
                command.Parameters.AddWithValue("@boQuaMaDoi", (object)boQuaMaDoi ?? DBNull.Value);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public int LayMaNhomTheoDoi(int maDoi)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand("SELECT TOP 1 ma_nhom FROM NHOM_DOI WHERE ma_doi = @maDoi AND ma_tro_choi IS NOT NULL", connection))
            {
                command.Parameters.AddWithValue("@maDoi", maDoi);
                connection.Open();
                object value = command.ExecuteScalar();
                return value == null ? 0 : Convert.ToInt32(value);
            }
        }

        public int LayGameTheoDoi(int maDoi)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand("SELECT TOP 1 ma_tro_choi FROM NHOM_DOI WHERE ma_doi = @maDoi AND ma_tro_choi IS NOT NULL", connection))
            {
                command.Parameters.AddWithValue("@maDoi", maDoi);
                connection.Open();
                object value = command.ExecuteScalar();
                return value == null ? 0 : Convert.ToInt32(value);
            }
        }


        public int? LayHoSoTheoGame(int maNguoiDung, int maTroChoi)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT TOP 1 ma_ho_so
                                                        FROM HO_SO_IN_GAME
                                                        WHERE ma_nguoi_dung = @maNguoiDung AND ma_tro_choi = @maTroChoi
                                                        ORDER BY ngay_cap_nhat DESC", connection))
            {
                command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                command.Parameters.AddWithValue("@maTroChoi", maTroChoi);
                connection.Open();
                object value = command.ExecuteScalar();
                return value == null ? (int?)null : Convert.ToInt32(value);
            }
        }

        public bool DoiDangTuyen(int maDoi)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand("SELECT COUNT(1) FROM DOI WHERE ma_doi = @maDoi AND trang_thai = 'dang_hoat_dong' AND dang_tuyen = 1", connection))
            {
                command.Parameters.AddWithValue("@maDoi", maDoi);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public bool DaCoDonXinGiaNhap(int maDoi, int maNguoiDung)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand("SELECT COUNT(1) FROM XIN_GIA_NHAP WHERE ma_doi = @maDoi AND ma_nguoi_dung = @maNguoiDung AND trang_thai = 'cho_duyet'", connection))
            {
                command.Parameters.AddWithValue("@maDoi", maDoi);
                command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public void TaoDonXinGiaNhap(int maDoi, int maNhom, int maNguoiDung, int maHoSo)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"INSERT INTO XIN_GIA_NHAP (ma_nguoi_dung, ma_doi, ma_nhom, ma_ho_so)
                                                        VALUES (@maNguoiDung, @maDoi, @maNhom, @maHoSo)", connection))
            {
                command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                command.Parameters.AddWithValue("@maDoi", maDoi);
                command.Parameters.AddWithValue("@maNhom", maNhom);
                command.Parameters.AddWithValue("@maHoSo", maHoSo);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public string LayVaiTro(int maDoi, int maNguoiDung)

        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT TOP 1 tv.vai_tro_noi_bo
                                                        FROM THANH_VIEN_DOI tv
                                                        INNER JOIN NHOM_DOI n ON tv.ma_nhom = n.ma_nhom
                                                        WHERE n.ma_doi = @maDoi AND tv.ma_nguoi_dung = @maNguoiDung
                                                          AND tv.trang_thai_duyet = 'da_duyet' AND tv.trang_thai_hop_dong = 'dang_hieu_luc'", connection))
            {
                command.Parameters.AddWithValue("@maDoi", maDoi);
                command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                connection.Open();
                object value = command.ExecuteScalar();
                return value == null ? null : value.ToString();
            }
        }

        public List<ThanhVienDoiDTO> LayThanhVien(int maDoi)
        {
            List<ThanhVienDoiDTO> items = new List<ThanhVienDoiDTO>();
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT tv.ma_thanh_vien, tv.ma_nguoi_dung, nd.ten_dang_nhap, nd.avatar_url,
                                                               vt.ten_vi_tri, tv.vai_tro_noi_bo, tv.phan_he, tv.ngay_tham_gia
                                                        FROM THANH_VIEN_DOI tv
                                                        INNER JOIN NHOM_DOI n ON tv.ma_nhom = n.ma_nhom
                                                        INNER JOIN NGUOI_DUNG nd ON tv.ma_nguoi_dung = nd.ma_nguoi_dung
                                                        LEFT JOIN DANH_MUC_VI_TRI vt ON tv.ma_vi_tri = vt.ma_vi_tri
                                                        WHERE n.ma_doi = @maDoi AND tv.trang_thai_duyet = 'da_duyet' AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
                                                        ORDER BY CASE tv.vai_tro_noi_bo WHEN 'chu_tich' THEN 1 WHEN 'ban_dieu_hanh' THEN 2 WHEN 'doi_truong' THEN 3 ELSE 4 END, nd.ten_dang_nhap", connection))
            {
                command.Parameters.AddWithValue("@maDoi", maDoi);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new ThanhVienDoiDTO
                        {
                            ma_thanh_vien = Convert.ToInt32(reader["ma_thanh_vien"]),
                            ma_nguoi_dung = Convert.ToInt32(reader["ma_nguoi_dung"]),
                            username = reader["ten_dang_nhap"].ToString(),
                            ho_ten = null,
                            avatar_url = reader["avatar_url"] == DBNull.Value ? null : reader["avatar_url"].ToString(),
                            ten_vi_tri = reader["ten_vi_tri"] == DBNull.Value ? null : reader["ten_vi_tri"].ToString(),
                            vai_tro_noi_bo = reader["vai_tro_noi_bo"].ToString(),
                            phan_he = reader["phan_he"].ToString(),
                            ngay_tham_gia = Convert.ToDateTime(reader["ngay_tham_gia"])
                        });
                    }
                }
            }
            return items;
        }

        public void CapNhatDoi(CapNhatDoiRequestDTO request)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"UPDATE DOI SET ten_doi = @tenDoi, ten_viet_tat = @tenVietTat, logo_url = @logoUrl,
                                                                       slogan = @slogan, mo_ta = @moTa, dang_tuyen = @dangTuyen
                                                        WHERE ma_doi = @maDoi", connection))
            {
                command.Parameters.AddWithValue("@maDoi", request.ma_doi);
                command.Parameters.AddWithValue("@tenDoi", request.ten_doi.Trim());
                command.Parameters.AddWithValue("@tenVietTat", string.IsNullOrWhiteSpace(request.ten_viet_tat) ? (object)DBNull.Value : request.ten_viet_tat.Trim());
                command.Parameters.AddWithValue("@logoUrl", string.IsNullOrWhiteSpace(request.logo_url) ? (object)DBNull.Value : request.logo_url.Trim());
                command.Parameters.AddWithValue("@slogan", string.IsNullOrWhiteSpace(request.slogan) ? (object)DBNull.Value : request.slogan.Trim());
                command.Parameters.AddWithValue("@moTa", string.IsNullOrWhiteSpace(request.mo_ta) ? (object)DBNull.Value : request.mo_ta.Trim());
                command.Parameters.AddWithValue("@dangTuyen", request.dang_tuyen);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void CapNhatTuyenDung(int maDoi, bool dangTuyen)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand("UPDATE DOI SET dang_tuyen = @dangTuyen WHERE ma_doi = @maDoi", connection))
            {
                command.Parameters.AddWithValue("@maDoi", maDoi);
                command.Parameters.AddWithValue("@dangTuyen", dangTuyen);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void XoaDoi(int maDoi)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"DECLARE @NhomCanXoa TABLE (ma_nhom INT PRIMARY KEY);
                                                        INSERT INTO @NhomCanXoa (ma_nhom)
                                                        SELECT ma_nhom FROM NHOM_DOI WHERE ma_doi = @maDoi;

                                                        DELETE FROM YEU_CAU_XAC_NHAN_LOI_MOI WHERE ma_doi = @maDoi OR ma_nhom IN (SELECT ma_nhom FROM @NhomCanXoa);
                                                        DELETE FROM YEU_CAU_MOI_THANH_VIEN_DOI WHERE ma_doi = @maDoi OR ma_nhom IN (SELECT ma_nhom FROM @NhomCanXoa);
                                                        DELETE FROM LOI_MOI_GIA_NHAP WHERE ma_doi = @maDoi OR ma_nhom IN (SELECT ma_nhom FROM @NhomCanXoa);
                                                        DELETE FROM XIN_GIA_NHAP WHERE ma_doi = @maDoi OR ma_nhom IN (SELECT ma_nhom FROM @NhomCanXoa);
                                                        DELETE FROM YEU_CAU_THAM_GIA_NHOM WHERE ma_nhom IN (SELECT ma_nhom FROM @NhomCanXoa);
                                                        DELETE FROM BAI_DANG_TUYEN_DUNG WHERE ma_doi = @maDoi OR ma_nhom IN (SELECT ma_nhom FROM @NhomCanXoa);
                                                        DELETE FROM THANH_VIEN_DOI WHERE ma_nhom IN (SELECT ma_nhom FROM @NhomCanXoa);
                                                        DELETE FROM NHOM_DOI WHERE ma_doi = @maDoi;
                                                        DELETE FROM DOI WHERE ma_doi = @maDoi;", connection))
            {
                command.Parameters.AddWithValue("@maDoi", maDoi);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public int? TimNguoiDung(string usernameOrEmail)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand("SELECT TOP 1 ma_nguoi_dung FROM NGUOI_DUNG WHERE ten_dang_nhap = @q OR email = @q", connection))
            {
                command.Parameters.AddWithValue("@q", usernameOrEmail.Trim());
                connection.Open();
                object value = command.ExecuteScalar();
                return value == null ? (int?)null : Convert.ToInt32(value);
            }
        }

        public void TaoLoiMoi(int maDoi, int maNhom, int maNguoiNhan, int maNguoiGui, int? maViTri, string moTa)
        {
            bool coCotMoRong = CotTonTai("LOI_MOI_GIA_NHAP", "ma_vi_tri") && CotTonTai("LOI_MOI_GIA_NHAP", "mo_ta");
            string sql = coCotMoRong
                ? @"INSERT INTO LOI_MOI_GIA_NHAP (ma_doi, ma_nhom, ma_nguoi_duoc_moi, ma_nguoi_gui, ma_vi_tri, mo_ta)
                    VALUES (@maDoi, @maNhom, @maNguoiNhan, @maNguoiGui, @maViTri, @moTa)"
                : @"INSERT INTO LOI_MOI_GIA_NHAP (ma_doi, ma_nhom, ma_nguoi_duoc_moi, ma_nguoi_gui)
                    VALUES (@maDoi, @maNhom, @maNguoiNhan, @maNguoiGui)";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@maDoi", maDoi);
                command.Parameters.AddWithValue("@maNhom", maNhom);
                command.Parameters.AddWithValue("@maNguoiNhan", maNguoiNhan);
                command.Parameters.AddWithValue("@maNguoiGui", maNguoiGui);
                if (coCotMoRong)
                {
                    command.Parameters.AddWithValue("@maViTri", (object)maViTri ?? DBNull.Value);
                    command.Parameters.AddWithValue("@moTa", string.IsNullOrWhiteSpace(moTa) ? (object)DBNull.Value : moTa.Trim());
                }
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void TaoYeuCauMoiThanhVien(int maDoi, int maNhom, int maNguoiNhan, int maNguoiGui, int? maViTri, string moTa)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"INSERT INTO YEU_CAU_MOI_THANH_VIEN_DOI (ma_doi, ma_nhom, ma_nguoi_duoc_moi, ma_nguoi_gui, ma_vi_tri, mo_ta)
                                                        VALUES (@maDoi, @maNhom, @maNguoiNhan, @maNguoiGui, @maViTri, @moTa)", connection))
            {
                command.Parameters.AddWithValue("@maDoi", maDoi);
                command.Parameters.AddWithValue("@maNhom", maNhom);
                command.Parameters.AddWithValue("@maNguoiNhan", maNguoiNhan);
                command.Parameters.AddWithValue("@maNguoiGui", maNguoiGui);
                command.Parameters.AddWithValue("@maViTri", (object)maViTri ?? DBNull.Value);
                command.Parameters.AddWithValue("@moTa", string.IsNullOrWhiteSpace(moTa) ? (object)DBNull.Value : moTa.Trim());
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void CapNhatVaiTro(int maDoi, int maNguoiDung, string vaiTro)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"IF @vaiTro = 'doi_truong'
                                                        BEGIN
                                                            UPDATE tv SET vai_tro_noi_bo = 'thanh_vien'
                                                            FROM THANH_VIEN_DOI tv INNER JOIN NHOM_DOI n ON tv.ma_nhom = n.ma_nhom
                                                            WHERE n.ma_doi = @maDoi AND tv.vai_tro_noi_bo = 'doi_truong';
                                                        END
                                                        UPDATE tv SET vai_tro_noi_bo = @vaiTro
                                                        FROM THANH_VIEN_DOI tv INNER JOIN NHOM_DOI n ON tv.ma_nhom = n.ma_nhom
                                                        WHERE n.ma_doi = @maDoi AND tv.ma_nguoi_dung = @maNguoiDung AND tv.vai_tro_noi_bo <> 'chu_tich';", connection))
            {
                command.Parameters.AddWithValue("@maDoi", maDoi);
                command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                command.Parameters.AddWithValue("@vaiTro", vaiTro);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void LoaiThanhVien(int maDoi, int maNguoiDung)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"DELETE tv
                                                        FROM THANH_VIEN_DOI tv
                                                        INNER JOIN NHOM_DOI n ON tv.ma_nhom = n.ma_nhom
                                                        WHERE n.ma_doi = @maDoi AND tv.ma_nguoi_dung = @maNguoiDung AND tv.vai_tro_noi_bo <> 'chu_tich'", connection))
            {
                command.Parameters.AddWithValue("@maDoi", maDoi);
                command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<DoiGiaiDauDTO> LayGiaiDau(int maDoi)
        {
            List<DoiGiaiDauDTO> items = new List<DoiGiaiDauDTO>();
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT gd.ma_giai_dau, gd.ten_giai_dau, gd.trang_thai, tgg.trang_thai_tham_gia
                                                        FROM THAM_GIA_GIAI tgg
                                                        INNER JOIN NHOM_DOI n ON tgg.ma_nhom = n.ma_nhom
                                                        INNER JOIN GIAI_DAU gd ON tgg.ma_giai_dau = gd.ma_giai_dau
                                                        WHERE n.ma_doi = @maDoi
                                                        ORDER BY gd.ma_giai_dau DESC", connection))
            {
                command.Parameters.AddWithValue("@maDoi", maDoi);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new DoiGiaiDauDTO
                        {
                            ma_giai_dau = Convert.ToInt32(reader["ma_giai_dau"]),
                            ten_giai_dau = reader["ten_giai_dau"].ToString(),
                            trang_thai = reader["trang_thai"].ToString(),
                            trang_thai_tham_gia = reader["trang_thai_tham_gia"].ToString(),
                            ngay_bat_dau = null,
                            ngay_ket_thuc = null
                        });
                    }
                }
            }
            return items;
        }

        public List<DoiTranDauDTO> LayTranDau(int maDoi, bool sapToi)
        {
            List<DoiTranDauDTO> items = new List<DoiTranDauDTO>();
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT td.ma_tran, gd.ten_giai_dau, td.vong_dau, td.thoi_gian_bat_dau, td.trang_thai, ctd.ket_qua, ctd.diem_so
                                                        FROM CHI_TIET_TRAN_DAU ctd
                                                        INNER JOIN TRAN_DAU td ON ctd.ma_tran = td.ma_tran
                                                        INNER JOIN GIAI_DAU gd ON td.ma_giai_dau = gd.ma_giai_dau
                                                        INNER JOIN NHOM_DOI n ON ctd.ma_nhom = n.ma_nhom
                                                        WHERE n.ma_doi = @maDoi AND ((@sapToi = 1 AND td.trang_thai = 'chua_dau' AND (td.thoi_gian_bat_dau IS NULL OR td.thoi_gian_bat_dau >= GETDATE())) OR (@sapToi = 0 AND td.trang_thai <> 'chua_dau'))
                                                        ORDER BY CASE WHEN @sapToi = 1 THEN DATEDIFF(MINUTE, GETDATE(), ISNULL(td.thoi_gian_bat_dau, GETDATE())) ELSE -DATEDIFF(MINUTE, GETDATE(), ISNULL(td.thoi_gian_bat_dau, GETDATE())) END", connection))
            {
                command.Parameters.AddWithValue("@maDoi", maDoi);
                command.Parameters.AddWithValue("@sapToi", sapToi);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new DoiTranDauDTO
                        {
                            ma_tran = Convert.ToInt32(reader["ma_tran"]),
                            ten_giai_dau = reader["ten_giai_dau"].ToString(),
                            vong_dau = reader["vong_dau"] == DBNull.Value ? null : reader["vong_dau"].ToString(),
                            thoi_gian_bat_dau = reader["thoi_gian_bat_dau"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["thoi_gian_bat_dau"]),
                            trang_thai = reader["trang_thai"].ToString(),
                            ket_qua = reader["ket_qua"] == DBNull.Value ? null : reader["ket_qua"].ToString(),
                            diem_so = reader["diem_so"] == DBNull.Value ? 0 : Convert.ToDouble(reader["diem_so"])
                        });
                    }
                }
            }
            return items;
        }

        public List<YeuCauDoiDTO> LayYeuCau(int maNguoiDung)
        {
            List<YeuCauDoiDTO> items = new List<YeuCauDoiDTO>();
            bool coCotMoRong = CotTonTai("LOI_MOI_GIA_NHAP", "ma_vi_tri") && CotTonTai("LOI_MOI_GIA_NHAP", "mo_ta");
            bool coBangYeuCauMoi = BangTonTai("YEU_CAU_MOI_THANH_VIEN_DOI");
            string sqlLoiMoi = coCotMoRong
                ? @"SELECT lm.ma_loi_moi AS ma_yeu_cau, 'loi_moi' AS loai_yeu_cau, lm.ma_doi, d.ten_doi, tc.ten_game,
                           lm.ma_nguoi_gui, ng.ten_dang_nhap AS ten_nguoi_gui, lm.ma_nguoi_duoc_moi AS ma_nguoi_nhan,
                           nn.ten_dang_nhap AS ten_nguoi_nhan, vt.ten_vi_tri AS vi_tri, lm.mo_ta, lm.trang_thai, lm.ngay_tao,
                           CAST(NULL AS NVARCHAR(100)) AS ho_so_in_game_id, CAST(NULL AS NVARCHAR(100)) AS ho_so_in_game_name,
                           CAST(NULL AS NVARCHAR(100)) AS ho_so_vi_tri, CAST(NULL AS NVARCHAR(500)) AS ho_so_thanh_tich
                    FROM LOI_MOI_GIA_NHAP lm
                    INNER JOIN DOI d ON lm.ma_doi = d.ma_doi
                    INNER JOIN NHOM_DOI n ON lm.ma_nhom = n.ma_nhom
                    INNER JOIN DANH_MUC_TRO_CHOI tc ON n.ma_tro_choi = tc.ma_tro_choi
                    LEFT JOIN NGUOI_DUNG ng ON lm.ma_nguoi_gui = ng.ma_nguoi_dung
                    INNER JOIN NGUOI_DUNG nn ON lm.ma_nguoi_duoc_moi = nn.ma_nguoi_dung
                    LEFT JOIN DANH_MUC_VI_TRI vt ON lm.ma_vi_tri = vt.ma_vi_tri
                    WHERE lm.ma_nguoi_duoc_moi = @maNguoiDung AND lm.trang_thai = 'cho_phan_hoi'"
                : @"SELECT lm.ma_loi_moi AS ma_yeu_cau, 'loi_moi' AS loai_yeu_cau, lm.ma_doi, d.ten_doi, tc.ten_game,
                           lm.ma_nguoi_gui, ng.ten_dang_nhap AS ten_nguoi_gui, lm.ma_nguoi_duoc_moi AS ma_nguoi_nhan,
                           nn.ten_dang_nhap AS ten_nguoi_nhan, CAST(NULL AS NVARCHAR(100)) AS vi_tri, CAST(NULL AS NVARCHAR(500)) AS mo_ta, lm.trang_thai, lm.ngay_tao,
                           CAST(NULL AS NVARCHAR(100)) AS ho_so_in_game_id, CAST(NULL AS NVARCHAR(100)) AS ho_so_in_game_name,
                           CAST(NULL AS NVARCHAR(100)) AS ho_so_vi_tri, CAST(NULL AS NVARCHAR(500)) AS ho_so_thanh_tich
                    FROM LOI_MOI_GIA_NHAP lm
                    INNER JOIN DOI d ON lm.ma_doi = d.ma_doi
                    INNER JOIN NHOM_DOI n ON lm.ma_nhom = n.ma_nhom
                    INNER JOIN DANH_MUC_TRO_CHOI tc ON n.ma_tro_choi = tc.ma_tro_choi
                    LEFT JOIN NGUOI_DUNG ng ON lm.ma_nguoi_gui = ng.ma_nguoi_dung
                    INNER JOIN NGUOI_DUNG nn ON lm.ma_nguoi_duoc_moi = nn.ma_nguoi_dung
                    WHERE lm.ma_nguoi_duoc_moi = @maNguoiDung AND lm.trang_thai = 'cho_phan_hoi'";
            string sql = sqlLoiMoi;
            if (coBangYeuCauMoi)
            {
                sql += @" UNION ALL
                    SELECT yc.ma_yeu_cau, 'yeu_cau_moi' AS loai_yeu_cau, yc.ma_doi, d.ten_doi, tc.ten_game,
                           yc.ma_nguoi_gui, ng.ten_dang_nhap, yc.ma_nguoi_duoc_moi, nn.ten_dang_nhap, vt.ten_vi_tri, yc.mo_ta, yc.trang_thai, yc.ngay_tao,
                           CAST(NULL AS NVARCHAR(100)) AS ho_so_in_game_id, CAST(NULL AS NVARCHAR(100)) AS ho_so_in_game_name,
                           CAST(NULL AS NVARCHAR(100)) AS ho_so_vi_tri, CAST(NULL AS NVARCHAR(500)) AS ho_so_thanh_tich
                    FROM YEU_CAU_MOI_THANH_VIEN_DOI yc
                    INNER JOIN DOI d ON yc.ma_doi = d.ma_doi
                    INNER JOIN NHOM_DOI n ON yc.ma_nhom = n.ma_nhom
                    INNER JOIN DANH_MUC_TRO_CHOI tc ON n.ma_tro_choi = tc.ma_tro_choi
                    INNER JOIN NGUOI_DUNG ng ON yc.ma_nguoi_gui = ng.ma_nguoi_dung
                    INNER JOIN NGUOI_DUNG nn ON yc.ma_nguoi_duoc_moi = nn.ma_nguoi_dung
                    LEFT JOIN DANH_MUC_VI_TRI vt ON yc.ma_vi_tri = vt.ma_vi_tri
                    WHERE yc.trang_thai = 'cho_duyet' AND EXISTS (
                        SELECT 1 FROM THANH_VIEN_DOI tv INNER JOIN NHOM_DOI nx ON tv.ma_nhom = nx.ma_nhom
                        WHERE nx.ma_doi = yc.ma_doi AND tv.ma_nguoi_dung = @maNguoiDung AND tv.vai_tro_noi_bo IN ('chu_tich','ban_dieu_hanh')
                    )";
            }
            sql += @" UNION ALL
                    SELECT xg.ma_don_xin AS ma_yeu_cau, 'xin_gia_nhap' AS loai_yeu_cau, xg.ma_doi, d.ten_doi, tc.ten_game,
                           xg.ma_nguoi_dung AS ma_nguoi_gui, ng.ten_dang_nhap AS ten_nguoi_gui,
                           d.ma_doi_truong AS ma_nguoi_nhan, nd.ten_dang_nhap AS ten_nguoi_nhan,
                           CAST(NULL AS NVARCHAR(100)) AS vi_tri, CAST(NULL AS NVARCHAR(500)) AS mo_ta, xg.trang_thai, xg.ngay_tao,
                           hs.in_game_id AS ho_so_in_game_id, hs.in_game_name AS ho_so_in_game_name,
                           vt.ten_vi_tri AS ho_so_vi_tri, hs.thanh_tich AS ho_so_thanh_tich
                    FROM XIN_GIA_NHAP xg
                    INNER JOIN DOI d ON xg.ma_doi = d.ma_doi
                    INNER JOIN NHOM_DOI n ON xg.ma_nhom = n.ma_nhom
                    INNER JOIN DANH_MUC_TRO_CHOI tc ON n.ma_tro_choi = tc.ma_tro_choi
                    INNER JOIN NGUOI_DUNG ng ON xg.ma_nguoi_dung = ng.ma_nguoi_dung
                    INNER JOIN NGUOI_DUNG nd ON d.ma_doi_truong = nd.ma_nguoi_dung
                    LEFT JOIN HO_SO_IN_GAME hs ON xg.ma_ho_so = hs.ma_ho_so
                    LEFT JOIN DANH_MUC_VI_TRI vt ON hs.ma_vi_tri_so_truong = vt.ma_vi_tri
                    WHERE xg.trang_thai = 'cho_duyet' AND d.trang_thai = 'dang_hoat_dong' AND EXISTS (
                        SELECT 1 FROM THANH_VIEN_DOI tv INNER JOIN NHOM_DOI nx ON tv.ma_nhom = nx.ma_nhom
                        WHERE nx.ma_doi = xg.ma_doi AND tv.ma_nguoi_dung = @maNguoiDung
                          AND tv.vai_tro_noi_bo IN ('chu_tich','ban_dieu_hanh')
                          AND tv.trang_thai_duyet = 'da_duyet'
                          AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
                    )";
            sql += " ORDER BY ngay_tao DESC";
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new YeuCauDoiDTO
                        {
                            ma_yeu_cau = Convert.ToInt32(reader["ma_yeu_cau"]),
                            loai_yeu_cau = reader["loai_yeu_cau"].ToString(),
                            ma_doi = Convert.ToInt32(reader["ma_doi"]),
                            ten_doi = reader["ten_doi"].ToString(),
                            ten_game = reader["ten_game"].ToString(),
                            ma_nguoi_gui = reader["ma_nguoi_gui"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ma_nguoi_gui"]),
                            ten_nguoi_gui = reader["ten_nguoi_gui"] == DBNull.Value ? null : reader["ten_nguoi_gui"].ToString(),
                            ma_nguoi_nhan = reader["ma_nguoi_nhan"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ma_nguoi_nhan"]),
                            ten_nguoi_nhan = reader["ten_nguoi_nhan"] == DBNull.Value ? null : reader["ten_nguoi_nhan"].ToString(),
                            vi_tri = reader["vi_tri"] == DBNull.Value ? null : reader["vi_tri"].ToString(),
                            mo_ta = reader["mo_ta"] == DBNull.Value ? null : reader["mo_ta"].ToString(),
                            trang_thai = reader["trang_thai"].ToString(),
                            ngay_tao = Convert.ToDateTime(reader["ngay_tao"]),
                            ho_so_in_game_id = reader["ho_so_in_game_id"] == DBNull.Value ? null : reader["ho_so_in_game_id"].ToString(),
                            ho_so_in_game_name = reader["ho_so_in_game_name"] == DBNull.Value ? null : reader["ho_so_in_game_name"].ToString(),
                            ho_so_vi_tri = reader["ho_so_vi_tri"] == DBNull.Value ? null : reader["ho_so_vi_tri"].ToString(),
                            ho_so_thanh_tich = reader["ho_so_thanh_tich"] == DBNull.Value ? null : reader["ho_so_thanh_tich"].ToString()
                        });
                    }
                }
            }
            return items;
        }

        public void XuLyLoiMoi(int maYeuCau, int maNguoiDung, bool chapNhan)
        {
            bool coCotViTri = CotTonTai("LOI_MOI_GIA_NHAP", "ma_vi_tri");
            string sql = coCotViTri
                ? @"DECLARE @maNhom INT, @maViTri INT;
                    SELECT @maNhom = ma_nhom, @maViTri = ma_vi_tri FROM LOI_MOI_GIA_NHAP WHERE ma_loi_moi = @maYeuCau AND ma_nguoi_duoc_moi = @maNguoiDung AND trang_thai = 'cho_phan_hoi';
                    IF @maNhom IS NOT NULL
                    BEGIN
                        DELETE FROM LOI_MOI_GIA_NHAP WHERE ma_loi_moi = @maYeuCau;
                        IF @chapNhan = 1
                        BEGIN
                            INSERT INTO THANH_VIEN_DOI (ma_nguoi_dung, ma_nhom, ma_vi_tri, vai_tro_noi_bo, phan_he)
                            VALUES (@maNguoiDung, @maNhom, @maViTri, 'thanh_vien', 'TuyenThu');
                        END
                    END"
                : @"DECLARE @maNhom INT;
                    SELECT @maNhom = ma_nhom FROM LOI_MOI_GIA_NHAP WHERE ma_loi_moi = @maYeuCau AND ma_nguoi_duoc_moi = @maNguoiDung AND trang_thai = 'cho_phan_hoi';
                    IF @maNhom IS NOT NULL
                    BEGIN
                        DELETE FROM LOI_MOI_GIA_NHAP WHERE ma_loi_moi = @maYeuCau;
                        IF @chapNhan = 1
                        BEGIN
                            INSERT INTO THANH_VIEN_DOI (ma_nguoi_dung, ma_nhom, vai_tro_noi_bo, phan_he)
                            VALUES (@maNguoiDung, @maNhom, 'thanh_vien', 'TuyenThu');
                        END
                    END";
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@maYeuCau", maYeuCau);
                command.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                command.Parameters.AddWithValue("@chapNhan", chapNhan);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void XuLyYeuCauMoi(int maYeuCau, int maNguoiDuyet, bool chapNhan)
        {
            bool coCotMoRong = CotTonTai("LOI_MOI_GIA_NHAP", "ma_vi_tri") && CotTonTai("LOI_MOI_GIA_NHAP", "mo_ta");
            string sql = coCotMoRong
                ? @"DECLARE @maDoi INT, @maNhom INT, @maNguoiNhan INT, @maNguoiGui INT, @maViTri INT, @moTa NVARCHAR(500);
                    SELECT @maDoi = ma_doi, @maNhom = ma_nhom, @maNguoiNhan = ma_nguoi_duoc_moi, @maNguoiGui = ma_nguoi_gui, @maViTri = ma_vi_tri, @moTa = mo_ta
                    FROM YEU_CAU_MOI_THANH_VIEN_DOI WHERE ma_yeu_cau = @maYeuCau AND trang_thai = 'cho_duyet';
                    IF @maDoi IS NOT NULL
                    BEGIN
                        DELETE FROM YEU_CAU_MOI_THANH_VIEN_DOI WHERE ma_yeu_cau = @maYeuCau;
                        IF @chapNhan = 1
                        BEGIN
                            INSERT INTO LOI_MOI_GIA_NHAP (ma_doi, ma_nhom, ma_nguoi_duoc_moi, ma_nguoi_gui, ma_vi_tri, mo_ta)
                            VALUES (@maDoi, @maNhom, @maNguoiNhan, @maNguoiGui, @maViTri, @moTa);
                        END
                    END"
                : @"DECLARE @maDoi INT, @maNhom INT, @maNguoiNhan INT, @maNguoiGui INT;
                    SELECT @maDoi = ma_doi, @maNhom = ma_nhom, @maNguoiNhan = ma_nguoi_duoc_moi, @maNguoiGui = ma_nguoi_gui
                    FROM YEU_CAU_MOI_THANH_VIEN_DOI WHERE ma_yeu_cau = @maYeuCau AND trang_thai = 'cho_duyet';
                    IF @maDoi IS NOT NULL
                    BEGIN
                        DELETE FROM YEU_CAU_MOI_THANH_VIEN_DOI WHERE ma_yeu_cau = @maYeuCau;
                        IF @chapNhan = 1
                        BEGIN
                            INSERT INTO LOI_MOI_GIA_NHAP (ma_doi, ma_nhom, ma_nguoi_duoc_moi, ma_nguoi_gui)
                            VALUES (@maDoi, @maNhom, @maNguoiNhan, @maNguoiGui);
                        END
                    END";
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@maYeuCau", maYeuCau);
                command.Parameters.AddWithValue("@maNguoiDuyet", maNguoiDuyet);
                command.Parameters.AddWithValue("@chapNhan", chapNhan);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void XuLyDonXinGiaNhap(int maDonXin, int maNguoiDuyet, bool chapNhan)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"DECLARE @maDoi INT, @maNhom INT, @maNguoiDung INT;
                                                        SELECT @maDoi = xg.ma_doi, @maNhom = xg.ma_nhom, @maNguoiDung = xg.ma_nguoi_dung
                                                        FROM XIN_GIA_NHAP xg
                                                        WHERE xg.ma_don_xin = @maDonXin AND xg.trang_thai = 'cho_duyet'
                                                          AND EXISTS (
                                                              SELECT 1
                                                              FROM THANH_VIEN_DOI tv
                                                              INNER JOIN NHOM_DOI n ON tv.ma_nhom = n.ma_nhom
                                                              WHERE n.ma_doi = xg.ma_doi
                                                                AND tv.ma_nguoi_dung = @maNguoiDuyet
                                                                AND tv.vai_tro_noi_bo IN ('chu_tich','ban_dieu_hanh')
                                                                AND tv.trang_thai_duyet = 'da_duyet'
                                                                AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
                                                          );
                                                        IF @maDoi IS NOT NULL
                                                        BEGIN
                                                            DELETE FROM XIN_GIA_NHAP WHERE ma_don_xin = @maDonXin;

                                                            IF @chapNhan = 1
                                                            BEGIN
                                                                INSERT INTO THANH_VIEN_DOI (ma_nguoi_dung, ma_nhom, vai_tro_noi_bo, phan_he)
                                                                VALUES (@maNguoiDung, @maNhom, 'thanh_vien', 'TuyenThu');
                                                            END
                                                        END", connection))
            {
                command.Parameters.AddWithValue("@maDonXin", maDonXin);
                command.Parameters.AddWithValue("@maNguoiDuyet", maNguoiDuyet);
                command.Parameters.AddWithValue("@chapNhan", chapNhan);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private bool CotTonTai(string tenBang, string tenCot)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand("SELECT COL_LENGTH(@tenBang, @tenCot)", connection))
            {
                command.Parameters.AddWithValue("@tenBang", tenBang);
                command.Parameters.AddWithValue("@tenCot", tenCot);
                connection.Open();
                return command.ExecuteScalar() != DBNull.Value;
            }
        }

        private bool BangTonTai(string tenBang)
        {
            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand("SELECT COUNT(1) FROM sys.tables WHERE name = @tenBang", connection))
            {
                command.Parameters.AddWithValue("@tenBang", tenBang);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private DoiDTO MapDoi(SqlDataReader reader)
        {
            return new DoiDTO
            {
                ma_doi = Convert.ToInt32(reader["ma_doi"]),
                ma_nhom = Convert.ToInt32(reader["ma_nhom"]),
                ma_tro_choi = Convert.ToInt32(reader["ma_tro_choi"]),
                ten_game = reader["ten_game"].ToString(),
                ten_doi = reader["ten_doi"].ToString(),
                ten_viet_tat = reader["ten_viet_tat"] == DBNull.Value ? null : reader["ten_viet_tat"].ToString(),
                ma_chu_tich = Convert.ToInt32(reader["ma_doi_truong"]),
                ten_chu_tich = reader["ten_chu_tich"].ToString(),
                logo_url = reader["logo_url"] == DBNull.Value ? null : reader["logo_url"].ToString(),
                slogan = reader["slogan"] == DBNull.Value ? null : reader["slogan"].ToString(),
                mo_ta = reader["mo_ta"] == DBNull.Value ? null : reader["mo_ta"].ToString(),
                trang_thai = reader["trang_thai"].ToString(),
                dang_tuyen = Convert.ToBoolean(reader["dang_tuyen"]),
                ngay_tao = Convert.ToDateTime(reader["ngay_tao"]),
                so_thanh_vien = Convert.ToInt32(reader["so_thanh_vien"]),
                vai_tro_cua_toi = reader["vai_tro_cua_toi"] == DBNull.Value ? null : reader["vai_tro_cua_toi"].ToString()
            };
        }
    }
}

