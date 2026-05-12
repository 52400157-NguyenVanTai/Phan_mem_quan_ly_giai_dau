using DTO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DAL
{
    public class GiaiDauDAL
    {
        public int TaoGiaiDau(int maNguoiTao, TaoGiaiDauRequestDTO req)
        {
            int maGiaiDau;
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"INSERT INTO GIAI_DAU (ten_giai_dau, ma_tro_choi, ma_nguoi_tao, the_thuc, banner_url, mo_ta,
                    so_doi_toi_thieu, so_doi_toi_da, min_members_per_team, tong_giai_thuong, trang_thai)
                    VALUES (@ten, @game, @nguoiTao, 'loai_truc_tiep', @banner, @moTa,
                    @minTeams, @maxTeams, @minMembers, @tongGiaiThuong, 'nhap');
                    SELECT SCOPE_IDENTITY();", conn))
                {
                    cmd.Parameters.AddWithValue("@ten", req.ten_giai_dau.Trim());
                    cmd.Parameters.AddWithValue("@game", (object)req.ma_tro_choi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@nguoiTao", maNguoiTao);
                    cmd.Parameters.AddWithValue("@banner", string.IsNullOrWhiteSpace(req.banner_url) ? (object)DBNull.Value : req.banner_url.Trim());
                    cmd.Parameters.AddWithValue("@moTa", string.IsNullOrWhiteSpace(req.mo_ta) ? (object)DBNull.Value : req.mo_ta.Trim());
                    cmd.Parameters.AddWithValue("@minTeams", req.so_doi_toi_thieu > 0 ? req.so_doi_toi_thieu : 2);
                    cmd.Parameters.AddWithValue("@maxTeams", (object)req.so_doi_toi_da ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@minMembers", req.min_members_per_team > 0 ? req.min_members_per_team : 1);
                    cmd.Parameters.AddWithValue("@tongGiaiThuong", req.tong_giai_thuong);
                    maGiaiDau = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Insert BTC role
                using (var cmd = new SqlCommand(@"INSERT INTO QUAN_TRI_GIAI_DAU (ma_giai_dau, ma_nguoi_dung, vai_tro_giai)
                    VALUES (@gd, @nd, 'ban_to_chuc')", conn))
                {
                    cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                    cmd.Parameters.AddWithValue("@nd", maNguoiTao);
                    cmd.ExecuteNonQuery();
                }

                // Insert stages
                if (req.giai_doan != null)
                {
                    foreach (var gd in req.giai_doan)
                    {
                        ThemGiaiDoan(conn, maGiaiDau, gd);
                    }
                }

                // Insert prizes
                if (req.danh_sach_giai_thuong != null)
                {
                    foreach (var gt in req.danh_sach_giai_thuong)
                    {
                        ThemGiaiThuong(conn, maGiaiDau, gt);
                    }
                }
            }
            return maGiaiDau;
        }

        private void ThemGiaiDoan(SqlConnection conn, int maGiaiDau, GiaiDoanRequestDTO gd)
        {
            using (var cmd = new SqlCommand(@"INSERT INTO GIAI_DOAN (ma_giai_dau, ten_giai_doan, the_thuc, thu_tu, so_doi, so_doi_di_tiep, nguong_match_point, bang_diem_json)
                VALUES (@gd, @ten, @theThuc, @thuTu, @soDoi, @diTiep, @matchPoint, @bangDiem)", conn))
            {
                cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                cmd.Parameters.AddWithValue("@ten", gd.ten_giai_doan.Trim());
                cmd.Parameters.AddWithValue("@theThuc", gd.the_thuc);
                cmd.Parameters.AddWithValue("@thuTu", gd.so_thu_tu);
                cmd.Parameters.AddWithValue("@soDoi", gd.so_doi > 0 ? gd.so_doi : 0);
                cmd.Parameters.AddWithValue("@diTiep", gd.so_doi_di_tiep.HasValue ? gd.so_doi_di_tiep.Value : 0);
                cmd.Parameters.AddWithValue("@matchPoint", (object)gd.nguong_match_point ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@bangDiem", string.IsNullOrWhiteSpace(gd.bang_diem_json) ? (object)DBNull.Value : gd.bang_diem_json);
                cmd.ExecuteNonQuery();
            }
        }

        private void ThemGiaiThuong(SqlConnection conn, int maGiaiDau, GiaiThuongRequestDTO gt)
        {
            using (var cmd = new SqlCommand(@"INSERT INTO GIAI_THUONG (ma_giai_dau, ten_giai, gia_tri, so_luong)
                VALUES (@gd, @ten, @giaTri, 1)", conn))
            {
                cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                cmd.Parameters.AddWithValue("@ten", gt.ten_giai.Trim());
                cmd.Parameters.AddWithValue("@giaTri", gt.gia_tri);
                cmd.ExecuteNonQuery();
            }
        }

        public void CapNhatGiaiDau(CapNhatGiaiDauRequestDTO req)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"UPDATE GIAI_DAU SET ten_giai_dau=@ten, banner_url=@banner, mo_ta=@moTa,
                    ma_tro_choi=@game, so_doi_toi_thieu=@minTeams, so_doi_toi_da=@maxTeams, min_members_per_team=@minMembers,
                    tong_giai_thuong=@tongGiaiThuong
                    WHERE ma_giai_dau=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", req.ma_giai_dau);
                    cmd.Parameters.AddWithValue("@ten", req.ten_giai_dau.Trim());
                    cmd.Parameters.AddWithValue("@banner", string.IsNullOrWhiteSpace(req.banner_url) ? (object)DBNull.Value : req.banner_url.Trim());
                    cmd.Parameters.AddWithValue("@moTa", string.IsNullOrWhiteSpace(req.mo_ta) ? (object)DBNull.Value : req.mo_ta.Trim());
                    cmd.Parameters.AddWithValue("@game", (object)req.ma_tro_choi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@minTeams", req.so_doi_toi_thieu > 0 ? req.so_doi_toi_thieu : 2);
                    cmd.Parameters.AddWithValue("@maxTeams", (object)req.so_doi_toi_da ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@minMembers", req.min_members_per_team > 0 ? req.min_members_per_team : 1);
                    cmd.Parameters.AddWithValue("@tongGiaiThuong", req.tong_giai_thuong);
                    cmd.ExecuteNonQuery();
                }

                // Replace stages
                using (var cmd = new SqlCommand("DELETE FROM GIAI_DOAN WHERE ma_giai_dau=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", req.ma_giai_dau);
                    cmd.ExecuteNonQuery();
                }
                if (req.giai_doan != null)
                {
                    foreach (var gd in req.giai_doan)
                        ThemGiaiDoan(conn, req.ma_giai_dau, gd);
                }

                // Replace prizes
                using (var cmd = new SqlCommand("DELETE FROM GIAI_THUONG WHERE ma_giai_dau=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", req.ma_giai_dau);
                    cmd.ExecuteNonQuery();
                }
                if (req.danh_sach_giai_thuong != null)
                {
                    foreach (var gt in req.danh_sach_giai_thuong)
                        ThemGiaiThuong(conn, req.ma_giai_dau, gt);
                }
            }
        }

        public void CapNhatTrangThai(int maGiaiDau, string trangThai)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand("UPDATE GIAI_DAU SET trang_thai=@tt WHERE ma_giai_dau=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", maGiaiDau);
                cmd.Parameters.AddWithValue("@tt", trangThai);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void TuChoi(int maGiaiDau, string lyDo)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand("UPDATE GIAI_DAU SET trang_thai='bi_tu_choi', ly_do_tu_choi=@lyDo WHERE ma_giai_dau=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", maGiaiDau);
                cmd.Parameters.AddWithValue("@lyDo", lyDo);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void ToggleRegistrationLock(int maGiaiDau, bool locked)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand("UPDATE GIAI_DAU SET is_registration_locked=@locked WHERE ma_giai_dau=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", maGiaiDau);
                cmd.Parameters.AddWithValue("@locked", locked);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Xoa ban nhap (Hard Delete) — chi cho phep khi trang_thai = 'nhap'
        // Ban nhap chua duoc public nen duoc xoa that khoi DB
        public bool XoaBanNhap(int maGiaiDau)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // Xoa giai doan truoc (FK)
                        using (var cmd = new SqlCommand("DELETE FROM GIAI_DOAN WHERE ma_giai_dau=@id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", maGiaiDau);
                            cmd.ExecuteNonQuery();
                        }
                        // Xoa quan tri giai dau
                        using (var cmd = new SqlCommand("DELETE FROM QUAN_TRI_GIAI_DAU WHERE ma_giai_dau=@id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", maGiaiDau);
                            cmd.ExecuteNonQuery();
                        }
                        // Xoa ban than giai dau
                        using (var cmd = new SqlCommand("DELETE FROM GIAI_DAU WHERE ma_giai_dau=@id AND trang_thai='nhap'", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", maGiaiDau);
                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0) { tx.Rollback(); return false; }
                        }
                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public string LayTrangThai(int maGiaiDau)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand("SELECT trang_thai FROM GIAI_DAU WHERE ma_giai_dau=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", maGiaiDau);
                conn.Open();
                object val = cmd.ExecuteScalar();
                return val == null ? null : val.ToString();
            }
        }

        public int? LayNguoiTao(int maGiaiDau)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand("SELECT ma_nguoi_tao FROM GIAI_DAU WHERE ma_giai_dau=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", maGiaiDau);
                conn.Open();
                object val = cmd.ExecuteScalar();
                return val == null || val == DBNull.Value ? (int?)null : Convert.ToInt32(val);
            }
        }

        public bool LaBTC(int maGiaiDau, int maNguoiDung)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(@"SELECT COUNT(1) FROM QUAN_TRI_GIAI_DAU
                WHERE ma_giai_dau=@gd AND ma_nguoi_dung=@nd AND vai_tro_giai='ban_to_chuc'", conn))
            {
                cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                cmd.Parameters.AddWithValue("@nd", maNguoiDung);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public bool LaAdmin(int maNguoiDung)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM NGUOI_DUNG WHERE ma_nguoi_dung=@nd AND vai_tro_he_thong='admin'", conn))
            {
                cmd.Parameters.AddWithValue("@nd", maNguoiDung);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public int DemDoiDaDuyet(int maGiaiDau)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM THAM_GIA_GIAI WHERE ma_giai_dau=@id AND trang_thai_duyet='da_duyet'", conn))
            {
                cmd.Parameters.AddWithValue("@id", maGiaiDau);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public GiaiDauDTO LayGiaiDau(int maGiaiDau)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(@"SELECT gd.*, tc.ten_game, nd.ten_dang_nhap AS ten_nguoi_tao,
                (SELECT COUNT(1) FROM THAM_GIA_GIAI tg WHERE tg.ma_giai_dau=gd.ma_giai_dau) AS so_doi_dang_ky,
                (SELECT COUNT(1) FROM THAM_GIA_GIAI tg WHERE tg.ma_giai_dau=gd.ma_giai_dau AND tg.trang_thai_duyet='da_duyet') AS so_doi_da_duyet
                FROM GIAI_DAU gd
                LEFT JOIN DANH_MUC_TRO_CHOI tc ON gd.ma_tro_choi=tc.ma_tro_choi
                LEFT JOIN NGUOI_DUNG nd ON gd.ma_nguoi_tao=nd.ma_nguoi_dung
                WHERE gd.ma_giai_dau=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", maGiaiDau);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read()) return MapGiaiDau(r);
                }
            }
            return null;
        }

        public List<GiaiDauDTO> LayGiaiDauCuaToi(int maNguoiDung)
        {
            var items = new List<GiaiDauDTO>();
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(@"SELECT gd.*, tc.ten_game, nd.ten_dang_nhap AS ten_nguoi_tao,
                (SELECT COUNT(1) FROM THAM_GIA_GIAI tg WHERE tg.ma_giai_dau=gd.ma_giai_dau) AS so_doi_dang_ky,
                (SELECT COUNT(1) FROM THAM_GIA_GIAI tg WHERE tg.ma_giai_dau=gd.ma_giai_dau AND tg.trang_thai_duyet='da_duyet') AS so_doi_da_duyet
                FROM GIAI_DAU gd
                LEFT JOIN DANH_MUC_TRO_CHOI tc ON gd.ma_tro_choi=tc.ma_tro_choi
                LEFT JOIN NGUOI_DUNG nd ON gd.ma_nguoi_tao=nd.ma_nguoi_dung
                WHERE gd.ma_nguoi_tao=@nd AND gd.trang_thai<>'da_huy'
                ORDER BY gd.ma_giai_dau DESC", conn))
            {
                cmd.Parameters.AddWithValue("@nd", maNguoiDung);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) items.Add(MapGiaiDau(r));
            }
            return items;
        }

        public List<GiaiDauDTO> LayDanhSachChoPheDuyet()
        {
            var items = new List<GiaiDauDTO>();
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(@"SELECT gd.*, tc.ten_game, nd.ten_dang_nhap AS ten_nguoi_tao,
                (SELECT COUNT(1) FROM THAM_GIA_GIAI tg WHERE tg.ma_giai_dau=gd.ma_giai_dau) AS so_doi_dang_ky,
                (SELECT COUNT(1) FROM THAM_GIA_GIAI tg WHERE tg.ma_giai_dau=gd.ma_giai_dau AND tg.trang_thai_duyet='da_duyet') AS so_doi_da_duyet
                FROM GIAI_DAU gd
                LEFT JOIN DANH_MUC_TRO_CHOI tc ON gd.ma_tro_choi=tc.ma_tro_choi
                LEFT JOIN NGUOI_DUNG nd ON gd.ma_nguoi_tao=nd.ma_nguoi_dung
                WHERE gd.trang_thai='cho_xet_duyet'
                ORDER BY gd.ma_giai_dau DESC", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) items.Add(MapGiaiDau(r));
            }
            return items;
        }

        public List<GiaiDauDTO> LayDanhSachPublic()
        {
            var items = new List<GiaiDauDTO>();
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(@"SELECT gd.*, tc.ten_game, nd.ten_dang_nhap AS ten_nguoi_tao,
                (SELECT COUNT(1) FROM THAM_GIA_GIAI tg WHERE tg.ma_giai_dau=gd.ma_giai_dau) AS so_doi_dang_ky,
                (SELECT COUNT(1) FROM THAM_GIA_GIAI tg WHERE tg.ma_giai_dau=gd.ma_giai_dau AND tg.trang_thai_duyet='da_duyet') AS so_doi_da_duyet
                FROM GIAI_DAU gd
                LEFT JOIN DANH_MUC_TRO_CHOI tc ON gd.ma_tro_choi=tc.ma_tro_choi
                LEFT JOIN NGUOI_DUNG nd ON gd.ma_nguoi_tao=nd.ma_nguoi_dung
                WHERE gd.trang_thai IN ('sap_dien_ra','mo_dang_ky','khoa_dang_ky','dang_dien_ra','ket_thuc')
                ORDER BY gd.ma_giai_dau DESC", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) items.Add(MapGiaiDau(r));
            }
            return items;
        }

        public List<GiaiDoanDTO> LayGiaiDoan(int maGiaiDau)
        {
            var items = new List<GiaiDoanDTO>();
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(@"SELECT * FROM GIAI_DOAN WHERE ma_giai_dau=@id ORDER BY thu_tu", conn))
            {
                cmd.Parameters.AddWithValue("@id", maGiaiDau);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        items.Add(new GiaiDoanDTO
                        {
                            ma_giai_doan = Convert.ToInt32(r["ma_giai_doan"]),
                            ma_giai_dau = Convert.ToInt32(r["ma_giai_dau"]),
                            so_thu_tu = Convert.ToInt32(r["thu_tu"]),
                            ten_giai_doan = r["ten_giai_doan"].ToString(),
                            the_thuc = r["the_thuc"].ToString(),
                            so_doi = r["so_doi"] == DBNull.Value ? 0 : Convert.ToInt32(r["so_doi"]),
                            so_doi_di_tiep = r["so_doi_di_tiep"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["so_doi_di_tiep"]),
                            nguong_match_point = r["nguong_match_point"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["nguong_match_point"]),
                            bang_diem_json = r["bang_diem_json"] == DBNull.Value ? null : r["bang_diem_json"].ToString(),
                            trang_thai = r["trang_thai"].ToString()
                        });
                    }
                }
            }
            return items;
        }

        public List<GiaiThuongDTO> LayDanhSachGiaiThuong(int maGiaiDau)
        {
            var items = new List<GiaiThuongDTO>();
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(@"SELECT * FROM GIAI_THUONG WHERE ma_giai_dau=@id ORDER BY gia_tri DESC", conn))
            {
                cmd.Parameters.AddWithValue("@id", maGiaiDau);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        items.Add(new GiaiThuongDTO
                        {
                            ma_giai_thuong = Convert.ToInt32(r["ma_giai_thuong"]),
                            ten_giai = r["ten_giai"].ToString(),
                            gia_tri = Convert.ToDecimal(r["gia_tri"])
                        });
                    }
                }
            }
            return items;
        }

        public List<DoiThamGiaDTO> LayDoiThamGia(int maGiaiDau)
        {
            var items = new List<DoiThamGiaDTO>();
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(@"SELECT tg.ma_tham_gia, tg.ma_nhom, d.ten_doi, d.logo_url, tc.ten_game,
                tg.trang_thai_duyet, tg.trang_thai_tham_gia
                FROM THAM_GIA_GIAI tg
                INNER JOIN NHOM_DOI n ON tg.ma_nhom=n.ma_nhom
                INNER JOIN DOI d ON n.ma_doi=d.ma_doi
                LEFT JOIN DANH_MUC_TRO_CHOI tc ON n.ma_tro_choi=tc.ma_tro_choi
                WHERE tg.ma_giai_dau=@id ORDER BY tg.ma_tham_gia", conn))
            {
                cmd.Parameters.AddWithValue("@id", maGiaiDau);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        items.Add(new DoiThamGiaDTO
                        {
                            ma_tham_gia = Convert.ToInt32(r["ma_tham_gia"]),
                            ma_nhom = Convert.ToInt32(r["ma_nhom"]),
                            ten_doi = r["ten_doi"].ToString(),
                            logo_url = r["logo_url"] == DBNull.Value ? null : r["logo_url"].ToString(),
                            ten_game = r["ten_game"] == DBNull.Value ? null : r["ten_game"].ToString(),
                            trang_thai_duyet = r["trang_thai_duyet"].ToString(),
                            trang_thai_tham_gia = r["trang_thai_tham_gia"].ToString()
                        });
                    }
                }
            }
            return items;
        }

        // Helper: kiem tra column co ton tai trong reader khong
        private static bool HasColumn(SqlDataReader r, string colName)
        {
            for (int i = 0; i < r.FieldCount; i++)
                if (r.GetName(i).Equals(colName, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private GiaiDauDTO MapGiaiDau(SqlDataReader r)
        {
            return new GiaiDauDTO
            {
                ma_giai_dau = Convert.ToInt32(r["ma_giai_dau"]),
                ten_giai_dau = r["ten_giai_dau"].ToString(),
                ma_tro_choi = r["ma_tro_choi"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ma_tro_choi"]),
                ten_game = r["ten_game"] == DBNull.Value ? null : r["ten_game"].ToString(),
                the_thuc = r["the_thuc"].ToString(),
                banner_url = r["banner_url"] == DBNull.Value ? null : r["banner_url"].ToString(),
                mo_ta = r["mo_ta"] == DBNull.Value ? null : r["mo_ta"].ToString(),
                so_doi_toi_thieu = r["so_doi_toi_thieu"] == DBNull.Value ? 2 : Convert.ToInt32(r["so_doi_toi_thieu"]),
                so_doi_toi_da = r["so_doi_toi_da"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["so_doi_toi_da"]),
                min_members_per_team = r["min_members_per_team"] == DBNull.Value ? 1 : Convert.ToInt32(r["min_members_per_team"]),
                trang_thai = r["trang_thai"].ToString(),
                ma_nguoi_tao = r["ma_nguoi_tao"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ma_nguoi_tao"]),
                ten_nguoi_tao = r["ten_nguoi_tao"] == DBNull.Value ? null : r["ten_nguoi_tao"].ToString(),
                ly_do_tu_choi = r["ly_do_tu_choi"] == DBNull.Value ? null : r["ly_do_tu_choi"].ToString(),
                is_registration_locked = r["is_registration_locked"] != DBNull.Value && Convert.ToBoolean(r["is_registration_locked"]),
                dang_mo_dang_ky = r["dang_mo_dang_ky"] != DBNull.Value && Convert.ToBoolean(r["dang_mo_dang_ky"]),
                tong_giai_thuong = r["tong_giai_thuong"] == DBNull.Value ? 0 : Convert.ToDecimal(r["tong_giai_thuong"]),
                // Đọc ngay_tao an toàn — cột có thể chưa tồn tại trong DB cũ (chưa chạy migration)
                ngay_tao = HasColumn(r, "ngay_tao") && r["ngay_tao"] != DBNull.Value
                    ? Convert.ToDateTime(r["ngay_tao"])
                    : DateTime.Now,
                so_doi_dang_ky = Convert.ToInt32(r["so_doi_dang_ky"]),
                so_doi_da_duyet = Convert.ToInt32(r["so_doi_da_duyet"])
            };
        }

        public void DangKyThamGiaGiai(int maGiaiDau, int maDoi)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    IF NOT EXISTS (SELECT 1 FROM THAM_GIA_GIAI WHERE ma_giai_dau = @gd AND ma_nhom = @nhom)
                    BEGIN
                        INSERT INTO THAM_GIA_GIAI(ma_giai_dau, ma_nhom, trang_thai_duyet, trang_thai_tham_gia)
                        VALUES(@gd, @nhom, 'cho_duyet', 'dang_thi_dau')
                    END", conn))
                {
                    cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                    cmd.Parameters.AddWithValue("@nhom", maDoi);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MoiDoiThamGia(int maGiaiDau, int maDoi, string loiNhan)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                conn.Open();
                // Get team president
                int? chuTich = null;
                using (var cmd = new SqlCommand("SELECT ma_doi_truong FROM DOI WHERE ma_doi = @nhom", conn))
                {
                    cmd.Parameters.AddWithValue("@nhom", maDoi);
                    var res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value) chuTich = Convert.ToInt32(res);
                }

                if (chuTich.HasValue)
                {
                    using (var cmd = new SqlCommand(@"
                        INSERT INTO THONG_BAO(ma_nguoi_nhan, tieu_de, noi_dung, loai_thong_bao, loai_entity, ma_entity, hanh_dong)
                        VALUES(@nn, 'Lời mời tham gia giải đấu', @nd, 'loi_moi_tham_gia_giai', 'giai_dau', @gd, 'pending')", conn))
                    {
                        cmd.Parameters.AddWithValue("@nn", chuTich.Value);
                        cmd.Parameters.AddWithValue("@nd", loiNhan ?? "Đội của bạn được mời tham gia giải đấu.");
                        cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void MoiNhanSu(int maGiaiDau, string usernameOrEmail, string vaiTro, string loiNhan)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                conn.Open();
                // Find user
                int? maNguoiNhan = null;
                using (var cmd = new SqlCommand("SELECT ma_nguoi_dung FROM NGUOI_DUNG WHERE ten_dang_nhap = @q OR email = @q", conn))
                {
                    cmd.Parameters.AddWithValue("@q", usernameOrEmail);
                    var res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value) maNguoiNhan = Convert.ToInt32(res);
                }

                if (maNguoiNhan.HasValue)
                {
                    string loaiTb = vaiTro == "btc" ? "loi_moi_btc" : "loi_moi_trong_tai";
                    string tieuDe = vaiTro == "btc" ? "Lời mời làm Ban tổ chức" : "Lời mời làm Trọng tài";
                    using (var cmd = new SqlCommand(@"
                        INSERT INTO THONG_BAO(ma_nguoi_nhan, tieu_de, noi_dung, loai_thong_bao, loai_entity, ma_entity, hanh_dong)
                        VALUES(@nn, @td, @nd, @loai, 'giai_dau', @gd, 'pending')", conn))
                    {
                        cmd.Parameters.AddWithValue("@nn", maNguoiNhan.Value);
                        cmd.Parameters.AddWithValue("@td", tieuDe);
                        cmd.Parameters.AddWithValue("@nd", loiNhan ?? tieuDe);
                        cmd.Parameters.AddWithValue("@loai", loaiTb);
                        cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
