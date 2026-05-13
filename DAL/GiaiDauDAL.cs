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


        private bool TableExists(SqlConnection conn, string tableName)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM sys.tables WHERE name=@table", conn))
            {
                cmd.Parameters.AddWithValue("@table", tableName);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private bool ColumnExists(SqlConnection conn, string tableName, string columnName)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM sys.columns WHERE object_id = OBJECT_ID(@table) AND name=@column", conn))
            {
                cmd.Parameters.AddWithValue("@table", "dbo." + tableName);
                cmd.Parameters.AddWithValue("@column", columnName);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public void CapNhatTrangThaiVaXoaLyDoTuChoi(int maGiaiDau, string trangThai)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand("UPDATE GIAI_DAU SET trang_thai=@tt, ly_do_tu_choi=NULL WHERE ma_giai_dau=@id", conn))
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

        private void ExecuteIfTableExists(SqlConnection conn, SqlTransaction tx, string tableName, string sql, int maGiaiDau)
        {
            using (var existsCmd = new SqlCommand("SELECT COUNT(1) FROM sys.tables WHERE name=@table", conn, tx))
            {
                existsCmd.Parameters.AddWithValue("@table", tableName);
                if (Convert.ToInt32(existsCmd.ExecuteScalar()) == 0) return;
            }

            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", maGiaiDau);
                cmd.ExecuteNonQuery();
            }
        }

        public bool XoaGiaiDauCascade(int maGiaiDau, string trangThaiBatBuoc)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        ExecuteIfTableExists(conn, tx, "THONG_BAO", "DELETE FROM THONG_BAO WHERE loai_entity='giai_dau' AND ma_entity=@id", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "TUONG_TAC_GIAI_DAU", "DELETE FROM TUONG_TAC_GIAI_DAU WHERE ma_giai_dau=@id", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "BANG_XEP_HANG_CA_NHAN", "DELETE FROM BANG_XEP_HANG_CA_NHAN WHERE ma_giai_dau=@id", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "BANG_XEP_HANG", "DELETE FROM BANG_XEP_HANG WHERE ma_giai_dau=@id", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "DOI_HINH_THI_DAU", "DELETE FROM DOI_HINH_THI_DAU WHERE ma_giai_dau=@id", maGiaiDau);

                        ExecuteIfTableExists(conn, tx, "YEU_CAU_MO_KHOA_KET_QUA", @"DELETE y
                            FROM YEU_CAU_MO_KHOA_KET_QUA y
                            INNER JOIN TRAN_DAU td ON y.ma_tran=td.ma_tran
                            WHERE td.ma_giai_dau=@id", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "KHIEU_NAI_KET_QUA", @"DELETE k
                            FROM KHIEU_NAI_KET_QUA k
                            INNER JOIN TRAN_DAU td ON k.ma_tran=td.ma_tran
                            WHERE td.ma_giai_dau=@id", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "LICH_SU_SUA_KET_QUA", "IF EXISTS (SELECT 1 FROM sys.triggers WHERE name='TRG_LSSKQ_IMMUTABLE') DISABLE TRIGGER TRG_LSSKQ_IMMUTABLE ON LICH_SU_SUA_KET_QUA", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "LICH_SU_SUA_KET_QUA", @"DELETE l
                            FROM LICH_SU_SUA_KET_QUA l
                            INNER JOIN TRAN_DAU td ON l.ma_tran=td.ma_tran
                            WHERE td.ma_giai_dau=@id", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "LICH_SU_SUA_KET_QUA", "IF EXISTS (SELECT 1 FROM sys.triggers WHERE name='TRG_LSSKQ_IMMUTABLE') ENABLE TRIGGER TRG_LSSKQ_IMMUTABLE ON LICH_SU_SUA_KET_QUA", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "KET_QUA_TRAN", @"DELETE k
                            FROM KET_QUA_TRAN k
                            INNER JOIN TRAN_DAU td ON k.ma_tran=td.ma_tran
                            WHERE td.ma_giai_dau=@id", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "CHI_TIET_NGUOI_CHOI_TRAN", @"DELETE c
                            FROM CHI_TIET_NGUOI_CHOI_TRAN c
                            INNER JOIN TRAN_DAU td ON c.ma_tran=td.ma_tran
                            WHERE td.ma_giai_dau=@id", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "CHI_TIET_TRAN_DAU", @"DELETE c
                            FROM CHI_TIET_TRAN_DAU c
                            INNER JOIN TRAN_DAU td ON c.ma_tran=td.ma_tran
                            WHERE td.ma_giai_dau=@id", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "TRAN_DAU", "UPDATE TRAN_DAU SET ma_tran_tiep_theo_thang=NULL, ma_tran_tiep_theo_thua=NULL WHERE ma_giai_dau=@id", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "TRAN_DAU", "DELETE FROM TRAN_DAU WHERE ma_giai_dau=@id", maGiaiDau);

                        ExecuteIfTableExists(conn, tx, "THAM_GIA_GIAI", "DELETE FROM THAM_GIA_GIAI WHERE ma_giai_dau=@id", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "GIAI_THUONG", "DELETE FROM GIAI_THUONG WHERE ma_giai_dau=@id", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "GIAI_DOAN", "DELETE FROM GIAI_DOAN WHERE ma_giai_dau=@id", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "TRONG_TAI_GIAI_DAU", "DELETE FROM TRONG_TAI_GIAI_DAU WHERE ma_giai_dau=@id", maGiaiDau);
                        ExecuteIfTableExists(conn, tx, "QUAN_TRI_GIAI_DAU", "DELETE FROM QUAN_TRI_GIAI_DAU WHERE ma_giai_dau=@id", maGiaiDau);

                        using (var cmd = new SqlCommand("DELETE FROM GIAI_DAU WHERE ma_giai_dau=@id AND trang_thai=@tt", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", maGiaiDau);
                            cmd.Parameters.AddWithValue("@tt", trangThaiBatBuoc);
                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0) { tx.Rollback(); return false; }
                        }
                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        try
                        {
                            ExecuteIfTableExists(conn, tx, "LICH_SU_SUA_KET_QUA", "IF EXISTS (SELECT 1 FROM sys.triggers WHERE name='TRG_LSSKQ_IMMUTABLE') ENABLE TRIGGER TRG_LSSKQ_IMMUTABLE ON LICH_SU_SUA_KET_QUA", maGiaiDau);
                        }
                        catch { }
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        // Xoa ban nhap (Hard Delete) — chi cho phep khi trang_thai = 'nhap'
        // Ban nhap chua duoc public nen duoc xoa that khoi DB
        public bool XoaBanNhap(int maGiaiDau)
        {
            return XoaGiaiDauCascade(maGiaiDau, "nhap");
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
                cmd.CommandTimeout = 10;
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
                cmd.CommandTimeout = 10;
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
                cmd.CommandTimeout = 10;
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
            {
                conn.Open();

                bool tggHasMaDoi = ColumnExists(conn, "THAM_GIA_GIAI", "ma_doi");
                bool tggHasMaNhom = ColumnExists(conn, "THAM_GIA_GIAI", "ma_nhom");
                bool doiHasMaTroChoi = ColumnExists(conn, "DOI", "ma_tro_choi");
                bool nhomDoiExists = TableExists(conn, "NHOM_DOI");

                string sql;
                if (tggHasMaDoi)
                {
                    string maNhomExpr = "tg.ma_doi";
                    string gameSelect = doiHasMaTroChoi ? "tc.ten_game" : "CAST(NULL AS NVARCHAR(255)) AS ten_game";
                    string gameJoin = doiHasMaTroChoi ? "LEFT JOIN DANH_MUC_TRO_CHOI tc ON d.ma_tro_choi=tc.ma_tro_choi" : "";
                    sql = @"SELECT tg.ma_tham_gia, " + maNhomExpr + @" AS ma_nhom, d.ten_doi, d.logo_url, " + gameSelect + @",
                        tg.trang_thai_duyet, tg.trang_thai_tham_gia
                        FROM THAM_GIA_GIAI tg
                        INNER JOIN DOI d ON tg.ma_doi=d.ma_doi
                        " + gameJoin + @"
                        WHERE tg.ma_giai_dau=@id
                        ORDER BY tg.ma_tham_gia";
                }
                else if (tggHasMaNhom && nhomDoiExists)
                {
                    sql = @"SELECT tg.ma_tham_gia, tg.ma_nhom, d.ten_doi, d.logo_url, tc.ten_game,
                        tg.trang_thai_duyet, tg.trang_thai_tham_gia
                        FROM THAM_GIA_GIAI tg
                        INNER JOIN NHOM_DOI n ON tg.ma_nhom=n.ma_nhom
                        INNER JOIN DOI d ON n.ma_doi=d.ma_doi
                        LEFT JOIN DANH_MUC_TRO_CHOI tc ON n.ma_tro_choi=tc.ma_tro_choi
                        WHERE tg.ma_giai_dau=@id
                        ORDER BY tg.ma_tham_gia";
                }
                else if (tggHasMaNhom)
                {
                    sql = @"SELECT tg.ma_tham_gia, tg.ma_nhom, d.ten_doi, d.logo_url, CAST(NULL AS NVARCHAR(255)) AS ten_game,
                        tg.trang_thai_duyet, tg.trang_thai_tham_gia
                        FROM THAM_GIA_GIAI tg
                        INNER JOIN DOI d ON tg.ma_nhom=d.ma_doi
                        WHERE tg.ma_giai_dau=@id
                        ORDER BY tg.ma_tham_gia";
                }
                else
                {
                    return items;
                }

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 10;
                    cmd.Parameters.AddWithValue("@id", maGiaiDau);

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
                dang_mo_dang_ky = r["trang_thai"].ToString() == "mo_dang_ky",
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
                    IF NOT EXISTS (SELECT 1 FROM THAM_GIA_GIAI WHERE ma_giai_dau = @gd AND ma_doi = @doi)
                    BEGIN
                        INSERT INTO THAM_GIA_GIAI(ma_giai_dau, ma_doi, trang_thai_duyet, trang_thai_tham_gia)
                        VALUES(@gd, @doi, 'cho_duyet', 'dang_thi_dau')
                    END", conn))
                {
                    cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                    cmd.Parameters.AddWithValue("@doi", maDoi);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MoiDoiThamGia(int maGiaiDau, int maDoi, string loiNhan)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                conn.Open();
                
                // Get all captains for the team (both DOI.ma_doi_truong and THANH_VIEN_DOI with captain roles)
                var captains = new HashSet<int>();
                
                using (var cmd = new SqlCommand("SELECT ma_doi_truong FROM DOI WHERE ma_doi = @nhom", conn))
                {
                    cmd.Parameters.AddWithValue("@nhom", maDoi);
                    var res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value) captains.Add(Convert.ToInt32(res));
                }

                using (var cmd = new SqlCommand(@"
                    SELECT tv.ma_nguoi_dung 
                    FROM THANH_VIEN_DOI tv 
                    WHERE tv.ma_doi = @nhom AND tv.vai_tro_noi_bo IN ('chu_tich', 'doi_truong', 'ban_dieu_hanh')
                ", conn))
                {
                    cmd.Parameters.AddWithValue("@nhom", maDoi);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            captains.Add(reader.GetInt32(0));
                        }
                    }
                }

                foreach (var captain in captains)
                {
                    using (var cmd = new SqlCommand(@"
                        INSERT INTO THONG_BAO(ma_nguoi_nhan, tieu_de, noi_dung, loai_thong_bao, loai_entity, ma_entity, hanh_dong, ma_doi)
                        VALUES(@nn, 'Lời mời tham gia giải đấu', @nd, 'loi_moi_tham_gia_giai', 'giai_dau', @gd, 'pending', @md)", conn))
                    {
                        cmd.Parameters.AddWithValue("@nn", captain);
                        cmd.Parameters.AddWithValue("@nd", loiNhan ?? "Đội của bạn được mời tham gia giải đấu.");
                        cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                        cmd.Parameters.AddWithValue("@md", maDoi);
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

        public List<NhanSuGiaiDauDTO> LayNhanSu(int maGiaiDau)
        {
            var items = new List<NhanSuGiaiDauDTO>();
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(@"
                SELECT x.ma_nguoi_dung, nd.ten_dang_nhap, nd.email, nd.avatar_url, x.vai_tro_giai
                FROM (
                    SELECT ma_nguoi_tao AS ma_nguoi_dung, CAST('ban_to_chuc' AS NVARCHAR(50)) AS vai_tro_giai
                    FROM GIAI_DAU
                    WHERE ma_giai_dau = @gd AND ma_nguoi_tao IS NOT NULL

                    UNION

                    SELECT ma_nguoi_dung, CAST('ban_to_chuc' AS NVARCHAR(50)) AS vai_tro_giai
                    FROM QUAN_TRI_GIAI_DAU
                    WHERE ma_giai_dau = @gd AND vai_tro_giai = 'ban_to_chuc'

                    UNION

                    SELECT ma_nguoi_dung, CAST('trong_tai' AS NVARCHAR(50)) AS vai_tro_giai
                    FROM TRONG_TAI_GIAI_DAU
                    WHERE ma_giai_dau = @gd AND trang_thai = 'da_chap_nhan'
                ) x
                INNER JOIN NGUOI_DUNG nd ON x.ma_nguoi_dung = nd.ma_nguoi_dung
                ORDER BY CASE x.vai_tro_giai WHEN 'ban_to_chuc' THEN 0 ELSE 1 END, nd.ten_dang_nhap", conn))
            {
                cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        items.Add(new NhanSuGiaiDauDTO
                        {
                            ma_nguoi_dung = Convert.ToInt32(r["ma_nguoi_dung"]),
                            ten_dang_nhap = r["ten_dang_nhap"].ToString(),
                            email = r["email"] == DBNull.Value ? null : r["email"].ToString(),
                            avatar_url = r["avatar_url"] == DBNull.Value ? null : r["avatar_url"].ToString(),
                            vai_tro_giai = r["vai_tro_giai"].ToString()
                        });
                    }
                }
            }
            return items;
        }

        public string LayLoaiGame(int maGiaiDau)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(@"
                SELECT tc.the_loai
                FROM GIAI_DAU gd
                LEFT JOIN DANH_MUC_TRO_CHOI tc ON gd.ma_tro_choi = tc.ma_tro_choi
                WHERE gd.ma_giai_dau = @gd", conn))
            {
                cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                conn.Open();
                var val = cmd.ExecuteScalar();
                return val == null || val == DBNull.Value ? null : val.ToString();
            }
        }

        public string LayLoaiGameTheoTran(int maTran)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(@"
                SELECT tc.the_loai
                FROM TRAN_DAU td
                INNER JOIN GIAI_DAU gd ON td.ma_giai_dau = gd.ma_giai_dau
                LEFT JOIN DANH_MUC_TRO_CHOI tc ON gd.ma_tro_choi = tc.ma_tro_choi
                WHERE td.ma_tran = @tran", conn))
            {
                cmd.Parameters.AddWithValue("@tran", maTran);
                conn.Open();
                var val = cmd.ExecuteScalar();
                return val == null || val == DBNull.Value ? null : val.ToString();
            }
        }

        public void ToggleRegistration(int maGiaiDau, bool open)
        {
            CapNhatTrangThai(maGiaiDau, open ? "mo_dang_ky" : "khoa_dang_ky");
        }

        public void KhoiTranhVaSinhTran(int maGiaiDau)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int existed;
                        using (var cmd = new SqlCommand("SELECT COUNT(1) FROM TRAN_DAU WHERE ma_giai_dau=@gd", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                            existed = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        if (existed == 0)
                        {
                            var stages = new List<GiaiDoanDTO>();
                            using (var cmd = new SqlCommand("SELECT * FROM GIAI_DOAN WHERE ma_giai_dau=@gd ORDER BY thu_tu", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                                using (var r = cmd.ExecuteReader())
                                {
                                    while (r.Read())
                                    {
                                        stages.Add(new GiaiDoanDTO
                                        {
                                            ma_giai_doan = Convert.ToInt32(r["ma_giai_doan"]),
                                            so_thu_tu = Convert.ToInt32(r["thu_tu"]),
                                            the_thuc = r["the_thuc"].ToString(),
                                            ten_giai_doan = r["ten_giai_doan"].ToString(),
                                            so_doi = r["so_doi"] == DBNull.Value ? 0 : Convert.ToInt32(r["so_doi"]),
                                            so_doi_di_tiep = r["so_doi_di_tiep"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["so_doi_di_tiep"])
                                        });
                                    }
                                }
                            }

                            var teams = new List<int>();
                            using (var cmd = new SqlCommand(@"
                                SELECT ma_doi
                                FROM (
                                    SELECT ma_doi, MIN(ma_tham_gia) AS first_join
                                    FROM THAM_GIA_GIAI
                                    WHERE ma_giai_dau=@gd AND trang_thai_duyet='da_duyet' AND ma_doi IS NOT NULL
                                    GROUP BY ma_doi
                                ) x
                                ORDER BY first_join", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                                using (var r = cmd.ExecuteReader())
                                    while (r.Read()) teams.Add(Convert.ToInt32(r["ma_doi"]));
                            }

                            foreach (var stage in stages)
                            {
                                foreach (var team in teams)
                                {
                                    using (var cmd = new SqlCommand(@"
                                        IF NOT EXISTS (SELECT 1 FROM BANG_XEP_HANG WHERE ma_giai_doan=@stage AND ma_doi=@doi)
                                        INSERT INTO BANG_XEP_HANG(ma_giai_dau, ma_giai_doan, ma_doi, thu_hang_hien_tai)
                                        VALUES(@gd, @stage, @doi, @rank)", conn, tx))
                                    {
                                        cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                                        cmd.Parameters.AddWithValue("@stage", stage.ma_giai_doan);
                                        cmd.Parameters.AddWithValue("@doi", team);
                                        cmd.Parameters.AddWithValue("@rank", teams.IndexOf(team) + 1);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }

                            if (stages.Count > 0)
                            {
                                var stage = stages[0];
                                if (stage.the_thuc == "vong_tron")
                                {
                                    for (int i = 0; i < teams.Count; i++)
                                        for (int j = i + 1; j < teams.Count; j++)
                                            InsertTran(conn, tx, maGiaiDau, stage.ma_giai_doan, "Vòng bảng", "BO1", null, null, new[] { teams[i], teams[j] });
                                }
                                else if (stage.the_thuc == "battle_royale" || stage.the_thuc == "champion_rush")
                                {
                                    InsertTran(conn, tx, maGiaiDau, stage.ma_giai_doan, stage.ten_giai_doan, "SinhTon", 1, null, teams.ToArray());
                                }
                                else
                                {
                                    if (stage.the_thuc == "loai_truc_tiep")
                                        SinhNhanhLoaiTrucTiep(conn, tx, maGiaiDau, stage.ma_giai_doan, teams);
                                    else
                                    {
                                        int round = 1;
                                        for (int i = 0; i < teams.Count; i += 2)
                                        {
                                            if (i + 1 < teams.Count)
                                                InsertTran(conn, tx, maGiaiDau, stage.ma_giai_doan, "Vong " + round, "BO1", null, stage.the_thuc == "nhanh_thang_nhanh_thua" ? "winners" : null, new[] { teams[i], teams[i + 1] });
                                            else
                                                InsertTran(conn, tx, maGiaiDau, stage.ma_giai_doan, "Vong " + round, "BO1", null, "bye", new[] { teams[i] }, "bye");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("Giai dau da sinh lich thi dau, khong the khoi tranh lai.");
                        }

                        using (var cmd = new SqlCommand("UPDATE GIAI_DAU SET trang_thai='dang_dien_ra' WHERE ma_giai_dau=@gd", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        private void SinhNhanhLoaiTrucTiep(SqlConnection conn, SqlTransaction tx, int maGiaiDau, int maGiaiDoan, List<int> teams)
        {
            var previousRound = new List<int>();
            int firstRoundSize = Math.Max(1, (int)Math.Ceiling(teams.Count / 2.0));

            for (int i = 0; i < teams.Count; i += 2)
            {
                if (i + 1 < teams.Count)
                    previousRound.Add(InsertTran(conn, tx, maGiaiDau, maGiaiDoan, "Vong 1", "BO1", null, null, new[] { teams[i], teams[i + 1] }));
                else
                    previousRound.Add(InsertTran(conn, tx, maGiaiDau, maGiaiDoan, "Vong 1", "BO1", null, "bye", new[] { teams[i] }, "bye"));
            }

            int round = 2;
            while (previousRound.Count > 1)
            {
                var nextRound = new List<int>();
                for (int i = 0; i < previousRound.Count; i += 2)
                {
                    string label = GetKnockoutRoundLabel(previousRound.Count, round, firstRoundSize);
                    int nextMatch = InsertTran(conn, tx, maGiaiDau, maGiaiDoan, label, "BO1", null, null, new int[0]);
                    nextRound.Add(nextMatch);

                    CapNhatTranTiepTheo(conn, tx, previousRound[i], nextMatch);
                    if (i + 1 < previousRound.Count) CapNhatTranTiepTheo(conn, tx, previousRound[i + 1], nextMatch);
                }
                previousRound = nextRound;
                round++;
            }
        }

        private string GetKnockoutRoundLabel(int currentMatchCount, int round, int firstRoundSize)
        {
            if (currentMatchCount == 2) return "Chung ket";
            if (currentMatchCount == 4) return "Ban ket";
            if (currentMatchCount == 8) return "Tu ket";
            return "Vong " + round;
        }

        private void CapNhatTranTiepTheo(SqlConnection conn, SqlTransaction tx, int maTran, int maTranTiepTheo)
        {
            using (var cmd = new SqlCommand(@"
                UPDATE TRAN_DAU
                SET ma_tran_tiep_theo_thang = @next
                WHERE ma_tran = @tran", conn, tx))
            {
                cmd.Parameters.AddWithValue("@next", maTranTiepTheo);
                cmd.Parameters.AddWithValue("@tran", maTran);
                cmd.ExecuteNonQuery();
            }
        }

        private int InsertTran(SqlConnection conn, SqlTransaction tx, int maGiaiDau, int maGiaiDoan, string vongDau, string theThucTran, int? soVong, string nhanhDau, int[] teams, string trangThai = "chua_dau")
        {
            int maTran;
            using (var cmd = new SqlCommand(@"
                INSERT INTO TRAN_DAU(ma_giai_dau, ma_giai_doan, vong_dau, the_thuc_tran, so_vong, nhanh_dau, trang_thai)
                VALUES(@gd, @stage, @vong, @format, @soVong, @nhanh, @tt);
                SELECT SCOPE_IDENTITY();", conn, tx))
            {
                cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                cmd.Parameters.AddWithValue("@stage", maGiaiDoan);
                cmd.Parameters.AddWithValue("@vong", vongDau);
                cmd.Parameters.AddWithValue("@format", theThucTran);
                cmd.Parameters.AddWithValue("@soVong", (object)soVong ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@nhanh", (object)nhanhDau ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tt", trangThai);
                maTran = Convert.ToInt32(cmd.ExecuteScalar());
            }

            foreach (var team in teams)
            {
                using (var cmd = new SqlCommand("INSERT INTO CHI_TIET_TRAN_DAU(ma_tran, ma_doi) VALUES(@tran, @doi)", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@tran", maTran);
                    cmd.Parameters.AddWithValue("@doi", team);
                    cmd.ExecuteNonQuery();
                }
            }
            return maTran;
        }

        public List<TranDauDTO> LayTranDau(int maGiaiDau)
        {
            var matches = new List<TranDauDTO>();
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(@"
                SELECT td.*, gd.ten_giai_doan, nd.ten_dang_nhap AS ten_trong_tai
                FROM TRAN_DAU td
                LEFT JOIN GIAI_DOAN gd ON td.ma_giai_doan = gd.ma_giai_doan
                LEFT JOIN NGUOI_DUNG nd ON td.ma_trong_tai = nd.ma_nguoi_dung
                WHERE td.ma_giai_dau=@gd
                ORDER BY td.ma_giai_doan, td.ma_tran", conn))
            {
                cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        matches.Add(new TranDauDTO
                        {
                            ma_tran = Convert.ToInt32(r["ma_tran"]),
                            ma_giai_dau = Convert.ToInt32(r["ma_giai_dau"]),
                            ma_giai_doan = r["ma_giai_doan"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ma_giai_doan"]),
                            ten_giai_doan = r["ten_giai_doan"] == DBNull.Value ? null : r["ten_giai_doan"].ToString(),
                            ma_trong_tai = r["ma_trong_tai"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ma_trong_tai"]),
                            ten_trong_tai = r["ten_trong_tai"] == DBNull.Value ? null : r["ten_trong_tai"].ToString(),
                            vong_dau = r["vong_dau"] == DBNull.Value ? null : r["vong_dau"].ToString(),
                            so_vong = r["so_vong"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["so_vong"]),
                            nhanh_dau = r["nhanh_dau"] == DBNull.Value ? null : r["nhanh_dau"].ToString(),
                            the_thuc_tran = r["the_thuc_tran"].ToString(),
                            trang_thai = r["trang_thai"].ToString(),
                            ma_tran_tiep_theo_thang = r["ma_tran_tiep_theo_thang"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ma_tran_tiep_theo_thang"]),
                            ma_tran_tiep_theo_thua = r["ma_tran_tiep_theo_thua"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ma_tran_tiep_theo_thua"]),
                            id_phong_game = r["id_phong_game"] == DBNull.Value ? null : r["id_phong_game"].ToString(),
                            mat_khau_phong = r["mat_khau_phong"] == DBNull.Value ? null : r["mat_khau_phong"].ToString(),
                            chi_tiet = new List<ChiTietTranDauDTO>(),
                            nguoi_choi = new List<NguoiChoiTranDTO>()
                        });
                    }
                }

                foreach (var match in matches)
                {
                    match.chi_tiet = LayChiTietTran(conn, match.ma_tran);
                    match.nguoi_choi = LayNguoiChoiTran(conn, match.ma_tran);
                }
            }
            return matches;
        }

        private List<ChiTietTranDauDTO> LayChiTietTran(SqlConnection conn, int maTran)
        {
            var items = new List<ChiTietTranDauDTO>();
            using (var cmd = new SqlCommand(@"
                SELECT c.*, d.ten_doi, d.logo_url
                FROM CHI_TIET_TRAN_DAU c
                INNER JOIN DOI d ON c.ma_doi = d.ma_doi
                WHERE c.ma_tran=@tran
                ORDER BY c.ma_doi", conn))
            {
                cmd.Parameters.AddWithValue("@tran", maTran);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        items.Add(new ChiTietTranDauDTO
                        {
                            ma_tran = maTran,
                            ma_nhom = Convert.ToInt32(r["ma_doi"]),
                            ten_doi = r["ten_doi"].ToString(),
                            logo_url = r["logo_url"] == DBNull.Value ? null : r["logo_url"].ToString(),
                            diem_so = Convert.ToDouble(r["diem_so"]),
                            thu_hang = r["thu_hang"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["thu_hang"]),
                            ket_qua = r["ket_qua"] == DBNull.Value ? null : r["ket_qua"].ToString(),
                            so_kill = r["so_kill"] == DBNull.Value ? 0 : Convert.ToInt32(r["so_kill"]),
                            is_check_in = r["is_check_in"] != DBNull.Value && Convert.ToBoolean(r["is_check_in"])
                        });
                    }
                }
            }
            return items;
        }

        private List<NguoiChoiTranDTO> LayNguoiChoiTran(SqlConnection conn, int maTran)
        {
            var items = new List<NguoiChoiTranDTO>();
            using (var cmd = new SqlCommand(@"
                SELECT nc.*, nd.ten_dang_nhap, nd.avatar_url, vt.ten_vi_tri, vt.ky_hieu,
                       tv.ma_doi, d.ten_doi
                FROM CHI_TIET_NGUOI_CHOI_TRAN nc
                INNER JOIN NGUOI_DUNG nd ON nc.ma_nguoi_dung = nd.ma_nguoi_dung
                LEFT JOIN DANH_MUC_VI_TRI vt ON nc.ma_vi_tri = vt.ma_vi_tri
                LEFT JOIN THANH_VIEN_DOI tv ON nc.ma_nguoi_dung = tv.ma_nguoi_dung AND tv.trang_thai_duyet='da_duyet'
                LEFT JOIN CHI_TIET_TRAN_DAU ctd ON ctd.ma_tran = nc.ma_tran AND ctd.ma_doi = tv.ma_doi
                LEFT JOIN DOI d ON ctd.ma_doi = d.ma_doi
                WHERE nc.ma_tran=@tran
                ORDER BY d.ten_doi, vt.ky_hieu, nd.ten_dang_nhap", conn))
            {
                cmd.Parameters.AddWithValue("@tran", maTran);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        items.Add(new NguoiChoiTranDTO
                        {
                            ma_tran = maTran,
                            ma_nguoi_dung = Convert.ToInt32(r["ma_nguoi_dung"]),
                            ma_doi = r["ma_doi"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ma_doi"]),
                            ten_doi = r["ten_doi"] == DBNull.Value ? null : r["ten_doi"].ToString(),
                            ten_dang_nhap = r["ten_dang_nhap"].ToString(),
                            avatar_url = r["avatar_url"] == DBNull.Value ? null : r["avatar_url"].ToString(),
                            ma_vi_tri = r["ma_vi_tri"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ma_vi_tri"]),
                            ten_vi_tri = r["ten_vi_tri"] == DBNull.Value ? null : r["ten_vi_tri"].ToString(),
                            ky_hieu_vi_tri = r["ky_hieu"] == DBNull.Value ? null : r["ky_hieu"].ToString(),
                            so_kill = Convert.ToInt32(r["so_kill"]),
                            so_death = Convert.ToInt32(r["so_death"]),
                            so_assist = Convert.ToInt32(r["so_assist"]),
                            diem_kda_tran = r["diem_kda_tran"] == DBNull.Value ? 0 : Convert.ToDouble(r["diem_kda_tran"]),
                            is_mvp_tran = Convert.ToBoolean(r["is_mvp_tran"])
                        });
                    }
                }
            }
            return items;
        }

        public List<BangXepHangDTO> LayBangXepHang(int maGiaiDau)
        {
            var items = new List<BangXepHangDTO>();
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(@"
                SELECT bxh.*, d.ten_doi, d.logo_url
                FROM BANG_XEP_HANG bxh
                INNER JOIN DOI d ON bxh.ma_doi = d.ma_doi
                WHERE bxh.ma_giai_dau=@gd
                ORDER BY bxh.diem_tong_ket DESC, bxh.so_tran_thang DESC, bxh.hieu_so_phu DESC, bxh.thu_hang_hien_tai", conn))
            {
                cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        items.Add(new BangXepHangDTO
                        {
                            ma_nhom = Convert.ToInt32(r["ma_doi"]),
                            ten_doi = r["ten_doi"].ToString(),
                            logo_url = r["logo_url"] == DBNull.Value ? null : r["logo_url"].ToString(),
                            so_tran_da_dau = Convert.ToInt32(r["so_tran_da_dau"]),
                            so_tran_thang = Convert.ToInt32(r["so_tran_thang"]),
                            so_tran_thua = Convert.ToInt32(r["so_tran_thua"]),
                            hieu_so_phu = Convert.ToInt32(r["hieu_so_phu"]),
                            tong_diem_hang = Convert.ToDouble(r["tong_diem_hang"]),
                            tong_diem_kill = Convert.ToDouble(r["tong_diem_kill"]),
                            diem_tong_ket = Convert.ToDouble(r["diem_tong_ket"]),
                            thu_hang_hien_tai = Convert.ToInt32(r["thu_hang_hien_tai"]),
                            is_match_point = Convert.ToBoolean(r["is_match_point"])
                        });
                    }
                }
            }
            return items;
        }

        public List<NguoiChoiTranDTO> LayThanhVienDoi(int maDoi)
        {
            var items = new List<NguoiChoiTranDTO>();
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(@"
                SELECT tv.ma_nguoi_dung, tv.ma_vi_tri, nd.ten_dang_nhap, nd.avatar_url, vt.ten_vi_tri, vt.ky_hieu
                FROM THANH_VIEN_DOI tv
                INNER JOIN NGUOI_DUNG nd ON tv.ma_nguoi_dung = nd.ma_nguoi_dung
                LEFT JOIN DANH_MUC_VI_TRI vt ON tv.ma_vi_tri = vt.ma_vi_tri
                WHERE tv.ma_doi=@doi AND tv.trang_thai_duyet='da_duyet'
                ORDER BY vt.ky_hieu, nd.ten_dang_nhap", conn))
            {
                cmd.Parameters.AddWithValue("@doi", maDoi);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        items.Add(new NguoiChoiTranDTO
                        {
                            ma_nguoi_dung = Convert.ToInt32(r["ma_nguoi_dung"]),
                            ma_doi = maDoi,
                            ten_dang_nhap = r["ten_dang_nhap"].ToString(),
                            avatar_url = r["avatar_url"] == DBNull.Value ? null : r["avatar_url"].ToString(),
                            ma_vi_tri = r["ma_vi_tri"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ma_vi_tri"]),
                            ten_vi_tri = r["ten_vi_tri"] == DBNull.Value ? null : r["ten_vi_tri"].ToString(),
                            ky_hieu_vi_tri = r["ky_hieu"] == DBNull.Value ? null : r["ky_hieu"].ToString()
                        });
                    }
                }
            }
            return items;
        }

        public List<NguoiChoiTranDTO> LayViTriTheoGame(int maGiaiDau)
        {
            var items = new List<NguoiChoiTranDTO>();
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(@"
                SELECT vt.ma_vi_tri, vt.ten_vi_tri, vt.ky_hieu
                FROM GIAI_DAU gd
                INNER JOIN DANH_MUC_VI_TRI vt ON gd.ma_tro_choi = vt.ma_tro_choi
                WHERE gd.ma_giai_dau=@gd AND vt.loai_vi_tri='TuyenThu'
                ORDER BY vt.ma_vi_tri", conn))
            {
                cmd.Parameters.AddWithValue("@gd", maGiaiDau);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        items.Add(new NguoiChoiTranDTO
                        {
                            ma_vi_tri = Convert.ToInt32(r["ma_vi_tri"]),
                            ten_vi_tri = r["ten_vi_tri"].ToString(),
                            ky_hieu_vi_tri = r["ky_hieu"].ToString()
                        });
                    }
                }
            }
            return items;
        }

        public bool DoiThuocTran(int maTran, int maDoi)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM CHI_TIET_TRAN_DAU WHERE ma_tran=@tran AND ma_doi=@doi", conn))
            {
                cmd.Parameters.AddWithValue("@tran", maTran);
                cmd.Parameters.AddWithValue("@doi", maDoi);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public bool LaDoiTruong(int maDoi, int maNguoiDung)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(@"
                SELECT COUNT(1)
                FROM DOI d
                WHERE d.ma_doi=@doi AND d.ma_doi_truong=@nd", conn))
            {
                cmd.Parameters.AddWithValue("@doi", maDoi);
                cmd.Parameters.AddWithValue("@nd", maNguoiDung);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public void SetupTranDau(SetupTranDauRequestDTO req)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand(@"
                        UPDATE TRAN_DAU
                        SET ma_trong_tai=@ref, the_thuc_tran=@format, so_vong=@soVong, trang_thai='chua_dau'
                        WHERE ma_tran=@tran", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@tran", req.ma_tran);
                        cmd.Parameters.AddWithValue("@ref", req.ma_trong_tai);
                        cmd.Parameters.AddWithValue("@format", req.the_thuc_tran);
                        cmd.Parameters.AddWithValue("@soVong", (object)req.so_vong ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO THONG_BAO(ma_nguoi_nhan, tieu_de, noi_dung, loai_thong_bao, loai_entity, ma_entity, hanh_dong)
                        SELECT @ref, N'Bạn được phân công trọng tài', N'Vui lòng xác nhận và chuẩn bị điều hành trận đấu.', 'phan_cong_trong_tai', 'tran_dau', @tran, 'pending'
                        UNION ALL
                        SELECT DISTINCT d.ma_doi_truong, N'Yêu cầu chốt đội hình', N'Vui lòng gửi đội hình xuất phát cho trận đấu.', 'yeu_cau_lineup', 'tran_dau', @tran, 'pending'
                        FROM CHI_TIET_TRAN_DAU c
                        INNER JOIN DOI d ON c.ma_doi=d.ma_doi
                        WHERE c.ma_tran=@tran AND d.ma_doi_truong IS NOT NULL", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@tran", req.ma_tran);
                        cmd.Parameters.AddWithValue("@ref", req.ma_trong_tai);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
            }
        }

        public void SubmitLineup(SubmitLineupRequestDTO req)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand(@"
                        DELETE nc
                        FROM CHI_TIET_NGUOI_CHOI_TRAN nc
                        INNER JOIN THANH_VIEN_DOI tv ON nc.ma_nguoi_dung=tv.ma_nguoi_dung
                        WHERE nc.ma_tran=@tran AND tv.ma_doi=@doi", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@tran", req.ma_tran);
                        cmd.Parameters.AddWithValue("@doi", req.ma_doi);
                        cmd.ExecuteNonQuery();
                    }

                    foreach (var p in req.thanh_vien)
                    {
                        using (var cmd = new SqlCommand(@"
                            INSERT INTO CHI_TIET_NGUOI_CHOI_TRAN(ma_tran, ma_nguoi_dung, ma_vi_tri)
                            VALUES(@tran, @nd, @vt)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@tran", req.ma_tran);
                            cmd.Parameters.AddWithValue("@nd", p.ma_nguoi_dung);
                            cmd.Parameters.AddWithValue("@vt", (object)p.ma_vi_tri ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    using (var cmd = new SqlCommand("UPDATE CHI_TIET_TRAN_DAU SET is_check_in=1 WHERE ma_tran=@tran AND ma_doi=@doi", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@tran", req.ma_tran);
                        cmd.Parameters.AddWithValue("@doi", req.ma_doi);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SqlCommand(@"
                        IF NOT EXISTS (SELECT 1 FROM CHI_TIET_TRAN_DAU WHERE ma_tran=@tran AND is_check_in=0)
                            UPDATE TRAN_DAU SET trang_thai='san_sang' WHERE ma_tran=@tran", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@tran", req.ma_tran);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
            }
        }

        public void BatDauTran(int maTran)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand("UPDATE TRAN_DAU SET trang_thai='dang_dau' WHERE ma_tran=@tran AND trang_thai='san_sang'", conn))
            {
                cmd.Parameters.AddWithValue("@tran", maTran);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateMatchStats(UpdateMatchStatsRequestDTO req)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    if (req.nguoi_choi != null)
                    {
                        using (var cmd = new SqlCommand("UPDATE CHI_TIET_NGUOI_CHOI_TRAN SET is_mvp_tran=0 WHERE ma_tran=@tran", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@tran", req.ma_tran);
                            cmd.ExecuteNonQuery();
                        }

                        foreach (var p in req.nguoi_choi)
                        {
                            using (var cmd = new SqlCommand(@"
                                UPDATE CHI_TIET_NGUOI_CHOI_TRAN
                                SET so_kill=@k, so_death=@d, so_assist=@a,
                                    diem_kda_tran=CAST((@k + @a) AS FLOAT) / CASE WHEN @d < 1 THEN 1 ELSE @d END,
                                    is_mvp_tran=@mvp
                                WHERE ma_tran=@tran AND ma_nguoi_dung=@nd", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@tran", req.ma_tran);
                                cmd.Parameters.AddWithValue("@nd", p.ma_nguoi_dung);
                                cmd.Parameters.AddWithValue("@k", p.so_kill);
                                cmd.Parameters.AddWithValue("@d", p.so_death);
                                cmd.Parameters.AddWithValue("@a", p.so_assist);
                                cmd.Parameters.AddWithValue("@mvp", p.is_mvp_tran);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    if (req.ma_doi_thang.HasValue)
                    {
                        using (var cmd = new SqlCommand(@"
                            UPDATE CHI_TIET_TRAN_DAU
                            SET ket_qua = CASE WHEN ma_doi=@win THEN 'thang' ELSE 'thua' END,
                                diem_so = CASE WHEN ma_doi=@win THEN 3 ELSE 0 END,
                                so_kill = CASE WHEN ma_doi=@win THEN ISNULL(@score1,0) ELSE ISNULL(@score2,0) END
                            WHERE ma_tran=@tran", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@tran", req.ma_tran);
                            cmd.Parameters.AddWithValue("@win", req.ma_doi_thang.Value);
                            cmd.Parameters.AddWithValue("@score1", (object)req.ty_so_doi_1 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@score2", (object)req.ty_so_doi_2 ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    if (req.ket_qua_br != null)
                    {
                        foreach (var row in req.ket_qua_br)
                        {
                            using (var cmd = new SqlCommand(@"
                                UPDATE CHI_TIET_TRAN_DAU
                                SET thu_hang=@rank, so_kill=@kill, diem_so=(CASE @rank WHEN 1 THEN 10 WHEN 2 THEN 6 WHEN 3 THEN 5 WHEN 4 THEN 4 WHEN 5 THEN 3 ELSE 1 END) + @kill
                                WHERE ma_tran=@tran AND ma_doi=@doi", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@tran", req.ma_tran);
                                cmd.Parameters.AddWithValue("@doi", row.ma_nhom);
                                cmd.Parameters.AddWithValue("@rank", row.thu_hang);
                                cmd.Parameters.AddWithValue("@kill", row.so_kill);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    RebuildBangXepHang(conn, tx, req.ma_tran);

                    using (var cmd = new SqlCommand(@"
                        IF EXISTS (SELECT 1 FROM KET_QUA_TRAN WHERE ma_tran=@tran)
                            UPDATE KET_QUA_TRAN SET so_lan_chinh_sua=so_lan_chinh_sua+1, thoi_gian_sua_cuoi=GETDATE() WHERE ma_tran=@tran
                        ELSE
                            INSERT INTO KET_QUA_TRAN(ma_tran, chi_tiet_phu) VALUES(@tran, N'{}')", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@tran", req.ma_tran);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
            }
        }

        private void RebuildBangXepHang(SqlConnection conn, SqlTransaction tx, int maTran)
        {
            using (var cmd = new SqlCommand(@"
                DECLARE @gd INT, @stage INT;
                SELECT @gd=ma_giai_dau, @stage=ma_giai_doan FROM TRAN_DAU WHERE ma_tran=@tran;

                UPDATE bxh
                SET so_tran_da_dau = x.so_tran,
                    so_tran_thang = x.thang,
                    so_tran_thua = x.thua,
                    hieu_so_phu = x.kill,
                    tong_diem_hang = x.diem_hang,
                    tong_diem_kill = x.kill,
                    diem_tong_ket = x.diem,
                    thu_hang_hien_tai = x.rank_no
                FROM BANG_XEP_HANG bxh
                INNER JOIN (
                    SELECT c.ma_doi,
                           COUNT(CASE WHEN c.ket_qua IS NOT NULL OR c.thu_hang IS NOT NULL THEN 1 END) AS so_tran,
                           SUM(CASE WHEN c.ket_qua='thang' THEN 1 ELSE 0 END) AS thang,
                           SUM(CASE WHEN c.ket_qua='thua' THEN 1 ELSE 0 END) AS thua,
                           SUM(ISNULL(c.so_kill,0)) AS kill,
                           SUM(CASE WHEN c.thu_hang IS NULL THEN 0 ELSE c.diem_so END) AS diem_hang,
                           SUM(c.diem_so) AS diem,
                           ROW_NUMBER() OVER (ORDER BY SUM(c.diem_so) DESC, SUM(ISNULL(c.so_kill,0)) DESC) AS rank_no
                    FROM CHI_TIET_TRAN_DAU c
                    INNER JOIN TRAN_DAU td ON c.ma_tran=td.ma_tran
                    WHERE td.ma_giai_dau=@gd AND (@stage IS NULL OR td.ma_giai_doan=@stage)
                    GROUP BY c.ma_doi
                ) x ON bxh.ma_doi=x.ma_doi AND bxh.ma_giai_doan=@stage
                WHERE bxh.ma_giai_dau=@gd;", conn, tx))
            {
                cmd.Parameters.AddWithValue("@tran", maTran);
                cmd.ExecuteNonQuery();
            }
        }

        public void ChotKetQuaTran(int maTran)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand("UPDATE TRAN_DAU SET trang_thai='da_hoan_thanh' WHERE ma_tran=@tran", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@tran", maTran);
                        cmd.ExecuteNonQuery();
                    }
                    RebuildBangXepHang(conn, tx, maTran);
                    tx.Commit();
                }
            }
        }
    }
}
