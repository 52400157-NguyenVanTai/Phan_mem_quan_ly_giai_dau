using DTO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DAL
{
    public class YeuCauDAL
    {
        // Lấy tất cả yêu cầu cho User hiện tại
        public List<YeuCauTongHopDTO> LayDanhSachYeuCau(int maNguoiDung)
        {
            var ds = new List<YeuCauTongHopDTO>();
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                conn.Open();
                
                // 1. ADMIN: Lấy yêu cầu duyệt giải đấu
                bool isAdmin = false;
                string checkAdminSql = "SELECT vai_tro_he_thong FROM NGUOI_DUNG WHERE ma_nguoi_dung = @UserId";
                using (var cmdAdmin = new SqlCommand(checkAdminSql, conn))
                {
                    cmdAdmin.Parameters.AddWithValue("@UserId", maNguoiDung);
                    var role = cmdAdmin.ExecuteScalar()?.ToString();
                    isAdmin = (role == "admin");
                }

                if (isAdmin)
                {
                    string sqlDuyetGiai = @"
                        SELECT g.ma_giai_dau, g.ten_giai_dau, GETDATE() as ngay_bat_dau, g.the_thuc, g.ma_nguoi_tao, u.ten_dang_nhap as ten_nguoi_gui, t.ten_game
                        FROM GIAI_DAU g
                        LEFT JOIN NGUOI_DUNG u ON g.ma_nguoi_tao = u.ma_nguoi_dung
                        LEFT JOIN DANH_MUC_TRO_CHOI t ON g.ma_tro_choi = t.ma_tro_choi
                        WHERE g.trang_thai = 'cho_phe_duyet' AND g.is_deleted = 0";
                    using (var cmd = new SqlCommand(sqlDuyetGiai, conn))
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            ds.Add(new YeuCauTongHopDTO
                            {
                                ma_yeu_cau = r.GetInt32(r.GetOrdinal("ma_giai_dau")), // Dùng ma_giai_dau làm ID
                                loai_yeu_cau = "yeu_cau_tao_giai_dau",
                                tieu_de = "Yêu cầu duyệt giải đấu mới",
                                noi_dung = $"Giải đấu {r["ten_giai_dau"]} đang chờ Admin phê duyệt.",
                                ma_nguoi_gui = r["ma_nguoi_tao"] != DBNull.Value ? (int?)r["ma_nguoi_tao"] : null,
                                ten_nguoi_gui = r["ten_nguoi_gui"]?.ToString(),
                                ma_giai_dau = r.GetInt32(r.GetOrdinal("ma_giai_dau")),
                                ten_giai_dau = r["ten_giai_dau"].ToString(),
                                ten_game = r["ten_game"]?.ToString(),
                                ngay_tao = r["ngay_bat_dau"] != DBNull.Value ? Convert.ToDateTime(r["ngay_bat_dau"]) : DateTime.Now
                            });
                        }
                    }
                }

                // 2. BTC: Lấy yêu cầu đội đăng ký tham gia giải (THAM_GIA_GIAI)
                string sqlDuyetDoi = @"
                    SELECT t.ma_giai_dau, t.ma_nhom, GETDATE() as ngay_dang_ky, g.ten_giai_dau, d.ten_doi, d.ma_doi_truong, u.ten_dang_nhap as ten_chu_tich
                    FROM THAM_GIA_GIAI t
                    JOIN GIAI_DAU g ON t.ma_giai_dau = g.ma_giai_dau
                    JOIN DOI d ON t.ma_nhom = d.ma_doi
                    LEFT JOIN NGUOI_DUNG u ON d.ma_doi_truong = u.ma_nguoi_dung
                    WHERE t.trang_thai_duyet = 'cho_duyet' AND g.ma_nguoi_tao = @UserId";
                using (var cmd = new SqlCommand(sqlDuyetDoi, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", maNguoiDung);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            ds.Add(new YeuCauTongHopDTO
                            {
                                // Kết hợp id giải đấu và mã nhóm để tạo ID duy nhất cho UI xử lý (hoặc dùng THAM_GIA_GIAI không có ID PK riêng, ta sẽ ghép chuỗi)
                                ma_yeu_cau = r.GetInt32(r.GetOrdinal("ma_giai_dau")) * 100000 + r.GetInt32(r.GetOrdinal("ma_nhom")),
                                loai_yeu_cau = "dang_ky_tham_gia_giai_dau",
                                tieu_de = "Đơn đăng ký tham gia giải",
                                noi_dung = $"Đội {r["ten_doi"]} muốn tham gia giải {r["ten_giai_dau"]}.",
                                ma_nguoi_gui = r["ma_doi_truong"] != DBNull.Value ? (int?)r["ma_doi_truong"] : null,
                                ten_nguoi_gui = r["ten_chu_tich"]?.ToString(),
                                ma_giai_dau = r.GetInt32(r.GetOrdinal("ma_giai_dau")),
                                ten_giai_dau = r["ten_giai_dau"].ToString(),
                                ma_doi = r.GetInt32(r.GetOrdinal("ma_nhom")),
                                ten_doi = r["ten_doi"].ToString(),
                                ngay_tao = r["ngay_dang_ky"] != DBNull.Value ? Convert.ToDateTime(r["ngay_dang_ky"]) : DateTime.Now
                            });
                        }
                    }
                }

                // 3. User & Chủ tịch: Lấy từ THONG_BAO (lời mời trọng tài, BTC, tham gia giải)
                string sqlThongBao = @"
                    SELECT tb.ma_thong_bao, tb.tieu_de, tb.noi_dung, tb.loai_thong_bao, tb.ma_entity, tb.ngay_tao,
                           g.ten_giai_dau, g.ma_giai_dau
                    FROM THONG_BAO tb
                    LEFT JOIN GIAI_DAU g ON tb.ma_entity = g.ma_giai_dau AND tb.loai_entity = 'giai_dau'
                    WHERE tb.ma_nguoi_nhan = @UserId AND tb.hanh_dong = 'pending'
                      AND tb.loai_thong_bao IN ('loi_moi_tham_gia_giai', 'loi_moi_trong_tai', 'loi_moi_btc')";
                using (var cmd = new SqlCommand(sqlThongBao, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", maNguoiDung);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var dto = new YeuCauTongHopDTO
                            {
                                ma_yeu_cau = r.GetInt32(r.GetOrdinal("ma_thong_bao")),
                                loai_yeu_cau = r["loai_thong_bao"].ToString(),
                                tieu_de = r["tieu_de"].ToString(),
                                noi_dung = r["noi_dung"].ToString(),
                                ngay_tao = Convert.ToDateTime(r["ngay_tao"]),
                                ma_giai_dau = r["ma_giai_dau"] != DBNull.Value ? (int?)r["ma_giai_dau"] : null,
                                ten_giai_dau = r["ten_giai_dau"]?.ToString()
                            };
                            ds.Add(dto);
                        }
                    }
                }

                // 4. Chủ tịch: Lời mời vào đội / Xin gia nhập đội (Kế thừa từ app-doi cũ)
                // YEU_CAU_MOI_THANH_VIEN_DOI
                string sqlLoiMoiDoi = @"
                    SELECT y.ma_yeu_cau, y.ma_nguoi_gui, y.ma_doi, d.ten_doi, y.mo_ta, y.ngay_tao as thoi_gian_tao
                    FROM YEU_CAU_MOI_THANH_VIEN_DOI y
                    JOIN DOI d ON y.ma_doi = d.ma_doi
                    WHERE y.ma_nguoi_duoc_moi = @UserId AND y.trang_thai = 'cho_duyet'";
                using (var cmd = new SqlCommand(sqlLoiMoiDoi, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", maNguoiDung);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            ds.Add(new YeuCauTongHopDTO
                            {
                                ma_yeu_cau = r.GetInt32(r.GetOrdinal("ma_yeu_cau")),
                                loai_yeu_cau = "loi_moi",
                                tieu_de = "Lời mời vào đội",
                                noi_dung = $"Đội {r["ten_doi"]} gửi lời mời bạn tham gia: {r["mo_ta"]}",
                                ma_doi = r.GetInt32(r.GetOrdinal("ma_doi")),
                                ten_doi = r["ten_doi"].ToString(),
                                ngay_tao = Convert.ToDateTime(r["thoi_gian_tao"])
                            });
                        }
                    }
                }

                // YEU_CAU_THAM_GIA_NHOM
                string sqlXinVaoDoi = @"
                    SELECT y.ma_yeu_cau, y.ma_nguoi_dung as ma_nguoi_gui, u.ten_dang_nhap as ten_nguoi_gui, y.ma_nhom, d.ten_doi, NULL as loi_nhan, y.ngay_tao as thoi_gian_tao
                    FROM YEU_CAU_THAM_GIA_NHOM y
                    JOIN DOI d ON y.ma_nhom = d.ma_doi
                    JOIN NGUOI_DUNG u ON y.ma_nguoi_dung = u.ma_nguoi_dung
                    WHERE d.ma_doi_truong = @UserId AND y.trang_thai = 'cho_duyet'";
                using (var cmd = new SqlCommand(sqlXinVaoDoi, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", maNguoiDung);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            ds.Add(new YeuCauTongHopDTO
                            {
                                ma_yeu_cau = r.GetInt32(r.GetOrdinal("ma_yeu_cau")),
                                loai_yeu_cau = "xin_gia_nhap",
                                tieu_de = "Đơn xin gia nhập đội",
                                noi_dung = $"{r["ten_nguoi_gui"]} xin vào đội {r["ten_doi"]}: {r["loi_nhan"]}",
                                ma_nguoi_gui = r.GetInt32(r.GetOrdinal("ma_nguoi_gui")),
                                ten_nguoi_gui = r["ten_nguoi_gui"].ToString(),
                                ma_doi = r.GetInt32(r.GetOrdinal("ma_nhom")),
                                ten_doi = r["ten_doi"].ToString(),
                                ngay_tao = Convert.ToDateTime(r["thoi_gian_tao"])
                            });
                        }
                    }
                }
            }
            
            // Sort by Date DESC
            ds.Sort((a, b) => b.ngay_tao.CompareTo(a.ngay_tao));
            return ds;
        }

        public ApiResponseDTO XuLyYeuCau(int maNguoiDung, XuLyYeuCauRequestDTO req)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        switch (req.loai_yeu_cau)
                        {
                            case "yeu_cau_tao_giai_dau":
                                // ma_yeu_cau = ma_giai_dau
                                string newStatus = req.chap_nhan ? "sap_dien_ra" : "tu_choi";
                                string sqlGiaiDau = "UPDATE GIAI_DAU SET trang_thai = @TrangThai, ly_do_tu_choi = @LyDo WHERE ma_giai_dau = @MaGiaiDau";
                                using (var cmd = new SqlCommand(sqlGiaiDau, conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@TrangThai", newStatus);
                                    cmd.Parameters.AddWithValue("@LyDo", string.IsNullOrEmpty(req.ly_do) ? (object)DBNull.Value : req.ly_do);
                                    cmd.Parameters.AddWithValue("@MaGiaiDau", req.ma_yeu_cau);
                                    cmd.ExecuteNonQuery();
                                }
                                break;

                            case "dang_ky_tham_gia_giai_dau":
                                // ma_yeu_cau = ma_giai_dau * 100000 + ma_nhom
                                int maGiaiDau = req.ma_yeu_cau / 100000;
                                int maNhom = req.ma_yeu_cau % 100000;
                                string sqlDuyetDoi = "UPDATE THAM_GIA_GIAI SET trang_thai_duyet = @Status WHERE ma_giai_dau = @MaGiaiDau AND ma_nhom = @MaNhom";
                                using (var cmd = new SqlCommand(sqlDuyetDoi, conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@Status", req.chap_nhan ? "da_duyet" : "tu_choi");
                                    cmd.Parameters.AddWithValue("@MaGiaiDau", maGiaiDau);
                                    cmd.Parameters.AddWithValue("@MaNhom", maNhom);
                                    cmd.ExecuteNonQuery();
                                }
                                break;

                            case "loi_moi_tham_gia_giai":
                                // Get notification to find ma_giai_dau and current user's team
                                // Actually we need to know the team ID. The notification must have stored the team ID in `noi_dung` or we can find it.
                                // For simplicity, let's assume team president accepts for their team.
                                int? maGiaiDauThongBao = null;
                                using (var cmd = new SqlCommand("SELECT ma_entity FROM THONG_BAO WHERE ma_thong_bao = @MaTb", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@MaTb", req.ma_yeu_cau);
                                    maGiaiDauThongBao = (int?)cmd.ExecuteScalar();
                                }
                                
                                if (req.chap_nhan && maGiaiDauThongBao.HasValue)
                                {
                                    // Find team where this user is president
                                    int? maDoi = null;
                                    using (var cmd = new SqlCommand("SELECT ma_doi FROM DOI WHERE ma_doi_truong = @UserId AND is_deleted = 0", conn, tx))
                                    {
                                        cmd.Parameters.AddWithValue("@UserId", maNguoiDung);
                                        maDoi = (int?)cmd.ExecuteScalar();
                                    }

                                    if (maDoi.HasValue)
                                    {
                                        // Insert/Update THAM_GIA_GIAI
                                        string sqlJoin = @"
                                            IF EXISTS (SELECT 1 FROM THAM_GIA_GIAI WHERE ma_giai_dau = @Gd AND ma_nhom = @Nhom)
                                                UPDATE THAM_GIA_GIAI SET trang_thai_duyet = 'da_duyet' WHERE ma_giai_dau = @Gd AND ma_nhom = @Nhom
                                            ELSE
                                                INSERT INTO THAM_GIA_GIAI(ma_giai_dau, ma_nhom, trang_thai_duyet, trang_thai_tham_gia)
                                                VALUES(@Gd, @Nhom, 'da_duyet', 'dang_thi_dau')";
                                        using (var cmd = new SqlCommand(sqlJoin, conn, tx))
                                        {
                                            cmd.Parameters.AddWithValue("@Gd", maGiaiDauThongBao.Value);
                                            cmd.Parameters.AddWithValue("@Nhom", maDoi.Value);
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                }

                                // Mark notification handled
                                using (var cmd = new SqlCommand("UPDATE THONG_BAO SET hanh_dong = @Hd, da_doc = 1 WHERE ma_thong_bao = @MaTb", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@Hd", req.chap_nhan ? "accepted" : "declined");
                                    cmd.Parameters.AddWithValue("@MaTb", req.ma_yeu_cau);
                                    cmd.ExecuteNonQuery();
                                }
                                break;

                            case "loi_moi_trong_tai":
                            case "loi_moi_btc":
                                int? maEntity = null;
                                using (var cmd = new SqlCommand("SELECT ma_entity FROM THONG_BAO WHERE ma_thong_bao = @MaTb", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@MaTb", req.ma_yeu_cau);
                                    maEntity = (int?)cmd.ExecuteScalar();
                                }

                                if (req.chap_nhan && maEntity.HasValue)
                                {
                                    string roleSql = req.loai_yeu_cau == "loi_moi_trong_tai" 
                                        ? "INSERT INTO TRONG_TAI_GIAI_DAU(ma_giai_dau, ma_nguoi_dung) VALUES(@Gd, @Nd)"
                                        : "INSERT INTO QUAN_TRI_GIAI_DAU(ma_giai_dau, ma_nguoi_quan_tri, vai_tro) VALUES(@Gd, @Nd, 'btc')";
                                        
                                    try 
                                    {
                                        using (var cmd = new SqlCommand(roleSql, conn, tx))
                                        {
                                            cmd.Parameters.AddWithValue("@Gd", maEntity.Value);
                                            cmd.Parameters.AddWithValue("@Nd", maNguoiDung);
                                            cmd.ExecuteNonQuery();
                                        }
                                    } catch { /* Ignore duplicates */ }
                                }

                                using (var cmd = new SqlCommand("UPDATE THONG_BAO SET hanh_dong = @Hd, da_doc = 1 WHERE ma_thong_bao = @MaTb", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@Hd", req.chap_nhan ? "accepted" : "declined");
                                    cmd.Parameters.AddWithValue("@MaTb", req.ma_yeu_cau);
                                    cmd.ExecuteNonQuery();
                                }
                                break;

                            case "loi_moi":
                            case "xin_gia_nhap":
                                // Cũ từ app-doi
                                string sqlHandleDoi = req.loai_yeu_cau == "loi_moi"
                                    ? "UPDATE YEU_CAU_MOI_THANH_VIEN_DOI SET trang_thai = @St WHERE ma_yeu_cau = @Ma"
                                    : "UPDATE YEU_CAU_THAM_GIA_NHOM SET trang_thai = @St WHERE ma_yeu_cau = @Ma";
                                using (var cmd = new SqlCommand(sqlHandleDoi, conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@St", req.chap_nhan ? "da_xac_nhan" : "tu_choi");
                                    cmd.Parameters.AddWithValue("@Ma", req.ma_yeu_cau);
                                    cmd.ExecuteNonQuery();
                                }

                                if (req.chap_nhan)
                                {
                                    // Thêm vào đội
                                    string sqlAddMem = req.loai_yeu_cau == "loi_moi"
                                        ? "INSERT INTO THANH_VIEN_DOI (ma_doi, ma_nguoi_dung, vai_tro_noi_bo) SELECT ma_doi, ma_nguoi_duoc_moi, 'thanh_vien' FROM YEU_CAU_MOI_THANH_VIEN_DOI WHERE ma_yeu_cau = @Ma"
                                        : "INSERT INTO THANH_VIEN_DOI (ma_doi, ma_nguoi_dung, vai_tro_noi_bo) SELECT ma_nhom, ma_nguoi_dung, 'thanh_vien' FROM YEU_CAU_THAM_GIA_NHOM WHERE ma_yeu_cau = @Ma";
                                    try 
                                    {
                                        using (var cmd = new SqlCommand(sqlAddMem, conn, tx))
                                        {
                                            cmd.Parameters.AddWithValue("@Ma", req.ma_yeu_cau);
                                            cmd.ExecuteNonQuery();
                                        }
                                    } catch { /* Ignore */ }
                                }
                                break;
                        }

                        tx.Commit();
                        return new ApiResponseDTO { success = true, message = "Xử lý thành công." };
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        return new ApiResponseDTO { success = false, message = "Lỗi hệ thống: " + ex.Message };
                    }
                }
            }
        }
    }
}
