using DTO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DAL
{
    public class YeuCauDAL
    {
        private void DamBaoCotThongBaoMaDoi(SqlConnection conn)
        {
            using (var cmd = new SqlCommand(@"
                IF COL_LENGTH('dbo.THONG_BAO', 'ma_doi') IS NULL
                    ALTER TABLE dbo.THONG_BAO ADD ma_doi INT NULL;", conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        // Lấy tất cả yêu cầu cho User hiện tại
        public List<YeuCauTongHopDTO> LayDanhSachYeuCau(int maNguoiDung)
        {
            var ds = new List<YeuCauTongHopDTO>();
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                conn.Open();
                DamBaoCotThongBaoMaDoi(conn);
                
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
                        WHERE g.trang_thai = 'cho_xet_duyet'";
                    using (var cmd = new SqlCommand(sqlDuyetGiai, conn))
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            ds.Add(new YeuCauTongHopDTO
                            {
                                ma_yeu_cau = r.GetInt32(r.GetOrdinal("ma_giai_dau")),
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
                    SELECT t.ma_giai_dau, t.ma_doi, GETDATE() as ngay_dang_ky, g.ten_giai_dau, d.ten_doi, d.ma_doi_truong, u.ten_dang_nhap as ten_chu_tich
                    FROM THAM_GIA_GIAI t
                    JOIN GIAI_DAU g ON t.ma_giai_dau = g.ma_giai_dau
                    INNER JOIN DOI d ON t.ma_doi = d.ma_doi
                    LEFT JOIN NGUOI_DUNG u ON d.ma_doi_truong = u.ma_nguoi_dung
                    WHERE t.trang_thai_duyet = 'cho_duyet' AND EXISTS (SELECT 1 FROM QUAN_TRI_GIAI_DAU qt WHERE qt.ma_giai_dau = g.ma_giai_dau AND qt.ma_nguoi_dung = @UserId AND qt.vai_tro_giai = 'ban_to_chuc')";
                using (var cmd = new SqlCommand(sqlDuyetDoi, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", maNguoiDung);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            ds.Add(new YeuCauTongHopDTO
                            {
                                ma_yeu_cau = r.GetInt32(r.GetOrdinal("ma_giai_dau")) * 100000 + r.GetInt32(r.GetOrdinal("ma_doi")),
                                loai_yeu_cau = "dang_ky_tham_gia_giai_dau",
                                tieu_de = "Đơn đăng ký tham gia giải",
                                noi_dung = $"Đội {r["ten_doi"]} muốn tham gia giải {r["ten_giai_dau"]}.",
                                ma_nguoi_gui = r["ma_doi_truong"] != DBNull.Value ? (int?)r["ma_doi_truong"] : null,
                                ten_nguoi_gui = r["ten_chu_tich"]?.ToString(),
                                ma_giai_dau = r.GetInt32(r.GetOrdinal("ma_giai_dau")),
                                ten_giai_dau = r["ten_giai_dau"].ToString(),
                                ma_doi = r.GetInt32(r.GetOrdinal("ma_doi")),
                                ten_doi = r["ten_doi"].ToString(),
                                ngay_tao = r["ngay_dang_ky"] != DBNull.Value ? Convert.ToDateTime(r["ngay_dang_ky"]) : DateTime.Now
                            });
                        }
                    }
                }

                // 3. User & Chủ tịch: Lấy từ THONG_BAO (lời mời trọng tài, BTC, tham gia giải)
                string sqlThongBao = @"
                    SELECT tb.ma_thong_bao, tb.tieu_de, tb.noi_dung, tb.loai_thong_bao, tb.ma_entity, tb.ma_doi, tb.ngay_tao,
                           COALESCE(g.ten_giai_dau, gt.ten_giai_dau) AS ten_giai_dau,
                           COALESCE(g.ma_giai_dau, gt.ma_giai_dau) AS ma_giai_dau,
                           d.ten_doi
                    FROM THONG_BAO tb
                    LEFT JOIN GIAI_DAU g ON tb.ma_entity = g.ma_giai_dau AND tb.loai_entity = 'giai_dau'
                    LEFT JOIN TRAN_DAU td ON tb.ma_entity = td.ma_tran AND tb.loai_entity = 'tran_dau'
                    LEFT JOIN GIAI_DAU gt ON td.ma_giai_dau = gt.ma_giai_dau
                    LEFT JOIN DOI d ON tb.ma_doi = d.ma_doi
                    WHERE tb.ma_nguoi_nhan = @UserId AND tb.hanh_dong = 'pending'
                      AND tb.loai_thong_bao IN ('loi_moi_tham_gia_giai', 'loi_moi_trong_tai', 'loi_moi_btc', 'phan_cong_trong_tai', 'yeu_cau_lineup')";
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
                                ten_giai_dau = r["ten_giai_dau"]?.ToString(),
                                ma_doi = r["ma_doi"] != DBNull.Value ? (int?)r["ma_doi"] : null,
                                ten_doi = r["ten_doi"]?.ToString()
                            };
                            ds.Add(dto);
                        }
                    }
                }

                // 4. CT/BĐH: Duyệt yêu cầu mời thành viên do Đội trưởng gửi (YEU_CAU_MOI_THANH_VIEN_DOI)
                string sqlYeuCauMoi = @"
                    SELECT y.ma_yeu_cau, y.ma_nguoi_gui, u.ten_dang_nhap as ten_nguoi_gui, y.ma_nguoi_duoc_moi, un.ten_dang_nhap as ten_nguoi_nhan, y.ma_doi, d.ten_doi, y.mo_ta, y.ngay_tao as thoi_gian_tao
                    FROM YEU_CAU_MOI_THANH_VIEN_DOI y
                    JOIN DOI d ON y.ma_doi = d.ma_doi
                    LEFT JOIN NGUOI_DUNG u ON y.ma_nguoi_gui = u.ma_nguoi_dung
                    LEFT JOIN NGUOI_DUNG un ON y.ma_nguoi_duoc_moi = un.ma_nguoi_dung
                    WHERE y.trang_thai = 'cho_duyet' AND EXISTS (
                        SELECT 1 FROM THANH_VIEN_DOI tv
                        WHERE tv.ma_doi = y.ma_doi AND tv.ma_nguoi_dung = @UserId AND tv.vai_tro_noi_bo IN ('chu_tich','ban_dieu_hanh') AND tv.trang_thai_duyet = 'da_duyet' AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
                    )";
                using (var cmd = new SqlCommand(sqlYeuCauMoi, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", maNguoiDung);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            ds.Add(new YeuCauTongHopDTO
                            {
                                ma_yeu_cau = r.GetInt32(r.GetOrdinal("ma_yeu_cau")),
                                loai_yeu_cau = "yeu_cau_moi",
                                tieu_de = "Yêu cầu duyệt lời mời vào đội",
                                noi_dung = $"Đội trưởng {r["ten_nguoi_gui"]} xin phép mời {r["ten_nguoi_nhan"]} vào đội {r["ten_doi"]}.",
                                ma_doi = r.GetInt32(r.GetOrdinal("ma_doi")),
                                ten_doi = r["ten_doi"].ToString(),
                                ngay_tao = Convert.ToDateTime(r["thoi_gian_tao"])
                            });
                        }
                    }
                }

                // 5. User: Lời mời gia nhập đội (LOI_MOI_GIA_NHAP)
                string sqlLoiMoiGiaNhap = @"
                    SELECT lm.ma_loi_moi as ma_yeu_cau, lm.ma_nguoi_gui, u.ten_dang_nhap as ten_nguoi_gui, lm.ma_doi, d.ten_doi, lm.mo_ta, lm.ngay_tao as thoi_gian_tao
                    FROM LOI_MOI_GIA_NHAP lm
                    JOIN DOI d ON lm.ma_doi = d.ma_doi
                    LEFT JOIN NGUOI_DUNG u ON lm.ma_nguoi_gui = u.ma_nguoi_dung
                    WHERE lm.ma_nguoi_duoc_moi = @UserId AND lm.trang_thai = 'cho_phan_hoi'";
                using (var cmd = new SqlCommand(sqlLoiMoiGiaNhap, conn))
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

                // 6. CT/BĐH: Đơn xin gia nhập (XIN_GIA_NHAP)
                string sqlXinVaoDoi = @"
                    SELECT y.ma_don_xin as ma_yeu_cau, y.ma_nguoi_dung as ma_nguoi_gui, u.ten_dang_nhap as ten_nguoi_gui, y.ma_doi, d.ten_doi, NULL as loi_nhan, y.ngay_tao as thoi_gian_tao
                    FROM XIN_GIA_NHAP y
                    JOIN DOI d ON y.ma_doi = d.ma_doi
                    JOIN NGUOI_DUNG u ON y.ma_nguoi_dung = u.ma_nguoi_dung
                    WHERE y.trang_thai = 'cho_duyet' AND EXISTS (
                        SELECT 1 FROM THANH_VIEN_DOI tv
                        WHERE tv.ma_doi = y.ma_doi AND tv.ma_nguoi_dung = @UserId AND tv.vai_tro_noi_bo IN ('chu_tich','ban_dieu_hanh') AND tv.trang_thai_duyet = 'da_duyet' AND tv.trang_thai_hop_dong = 'dang_hieu_luc'
                    )";
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
                                noi_dung = $"{r["ten_nguoi_gui"]} xin vào đội {r["ten_doi"]}",
                                ma_nguoi_gui = r.GetInt32(r.GetOrdinal("ma_nguoi_gui")),
                                ten_nguoi_gui = r["ten_nguoi_gui"].ToString(),
                                ma_doi = r.GetInt32(r.GetOrdinal("ma_doi")),
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
                DamBaoCotThongBaoMaDoi(conn);
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        switch (req.loai_yeu_cau)
                        {
                            case "yeu_cau_tao_giai_dau":
                                string newStatus = req.chap_nhan ? "sap_dien_ra" : "bi_tu_choi";
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
                                int maGiaiDau = req.ma_yeu_cau / 100000;
                                int maDoi = req.ma_yeu_cau % 100000;
                                string sqlDuyetDoi = "UPDATE THAM_GIA_GIAI SET trang_thai_duyet = @Status WHERE ma_giai_dau = @MaGiaiDau AND ma_doi = @MaDoi";
                                using (var cmd = new SqlCommand(sqlDuyetDoi, conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@Status", req.chap_nhan ? "da_duyet" : "bi_tu_choi");
                                    cmd.Parameters.AddWithValue("@MaGiaiDau", maGiaiDau);
                                    cmd.Parameters.AddWithValue("@MaDoi", maDoi);
                                    cmd.ExecuteNonQuery();
                                }
                                break;

                            case "loi_moi_tham_gia_giai":
                                int? maGiaiDauThongBao = null;
                                int? maDoiThongBao = null;
                                using (var cmd = new SqlCommand("SELECT ma_entity, ma_doi FROM THONG_BAO WHERE ma_thong_bao = @MaTb", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@MaTb", req.ma_yeu_cau);
                                    using (var r = cmd.ExecuteReader())
                                    {
                                        if (r.Read())
                                        {
                                            if (r["ma_entity"] != DBNull.Value) maGiaiDauThongBao = Convert.ToInt32(r["ma_entity"]);
                                            if (r["ma_doi"] != DBNull.Value) maDoiThongBao = Convert.ToInt32(r["ma_doi"]);
                                        }
                                    }
                                }
                                
                                if (req.chap_nhan && maGiaiDauThongBao.HasValue && maDoiThongBao.HasValue)
                                {
                                    // Verify user is actually a captain of this specific team
                                    bool isCaptain = false;
                                    using (var cmd = new SqlCommand(@"
                                        SELECT 1 FROM DOI WHERE ma_doi = @Doi AND ma_doi_truong = @UserId
                                        UNION
                                        SELECT 1 FROM THANH_VIEN_DOI tv
                                        WHERE tv.ma_doi = @Doi AND tv.ma_nguoi_dung = @UserId AND tv.vai_tro_noi_bo IN ('chu_tich', 'doi_truong', 'ban_dieu_hanh')
                                    ", conn, tx))
                                    {
                                        cmd.Parameters.AddWithValue("@Doi", maDoiThongBao.Value);
                                        cmd.Parameters.AddWithValue("@UserId", maNguoiDung);
                                        var res = cmd.ExecuteScalar();
                                        isCaptain = (res != null);
                                    }

                                    if (isCaptain)
                                    {
                                        bool dungGame = false;
                                        using(var cmd = new SqlCommand(@"
                                            SELECT COUNT(1)
                                            FROM DOI d
                                            INNER JOIN GIAI_DAU g ON g.ma_giai_dau = @Gd
                                            WHERE d.ma_doi = @Doi
                                              AND (g.ma_tro_choi IS NULL OR d.ma_tro_choi = g.ma_tro_choi)
                                        ", conn, tx))
                                        {
                                            cmd.Parameters.AddWithValue("@Doi", maDoiThongBao.Value);
                                            cmd.Parameters.AddWithValue("@Gd", maGiaiDauThongBao.Value);
                                            dungGame = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                                        }

                                        if (dungGame)
                                        {
                                            string sqlJoin = @"
                                                IF EXISTS (SELECT 1 FROM THAM_GIA_GIAI WHERE ma_giai_dau = @Gd AND ma_doi = @Doi)
                                                    UPDATE THAM_GIA_GIAI SET trang_thai_duyet = 'da_duyet' WHERE ma_giai_dau = @Gd AND ma_doi = @Doi
                                                ELSE
                                                    INSERT INTO THAM_GIA_GIAI(ma_giai_dau, ma_doi, trang_thai_duyet, trang_thai_tham_gia)
                                                    VALUES(@Gd, @Doi, 'da_duyet', 'dang_thi_dau')";
                                            using (var cmd = new SqlCommand(sqlJoin, conn, tx))
                                            {
                                                cmd.Parameters.AddWithValue("@Gd", maGiaiDauThongBao.Value);
                                                cmd.Parameters.AddWithValue("@Doi", maDoiThongBao.Value);
                                                cmd.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }

                                using (var cmd = new SqlCommand("DELETE FROM THONG_BAO WHERE ma_thong_bao = @MaTb", conn, tx))
                                {
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
                                    var res = cmd.ExecuteScalar();
                                    if(res != DBNull.Value && res != null) maEntity = Convert.ToInt32(res);
                                }

                                if (req.chap_nhan && maEntity.HasValue)
                                {
                                    string roleSql = req.loai_yeu_cau == "loi_moi_trong_tai" 
                                        ? "INSERT INTO TRONG_TAI_GIAI_DAU(ma_giai_dau, ma_nguoi_dung, trang_thai) VALUES(@Gd, @Nd, 'da_chap_nhan')"
                                        : "INSERT INTO QUAN_TRI_GIAI_DAU(ma_giai_dau, ma_nguoi_dung, vai_tro_giai) VALUES(@Gd, @Nd, 'ban_to_chuc')";
                                        
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

                                using (var cmd = new SqlCommand("DELETE FROM THONG_BAO WHERE ma_thong_bao = @MaTb", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@MaTb", req.ma_yeu_cau);
                                    cmd.ExecuteNonQuery();
                                }
                                break;

                            case "phan_cong_trong_tai":
                            case "yeu_cau_lineup":
                                using (var cmd = new SqlCommand("DELETE FROM THONG_BAO WHERE ma_thong_bao = @MaTb AND ma_nguoi_nhan = @UserId", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@MaTb", req.ma_yeu_cau);
                                    cmd.Parameters.AddWithValue("@UserId", maNguoiDung);
                                    cmd.ExecuteNonQuery();
                                }
                                break;

                            case "yeu_cau_moi":
                                if (req.chap_nhan)
                                {
                                    string checkMoTa = "SELECT COL_LENGTH('LOI_MOI_GIA_NHAP', 'mo_ta')";
                                    bool hasMoTa = false;
                                    using (var cmd = new SqlCommand(checkMoTa, conn, tx))
                                    {
                                        var res = cmd.ExecuteScalar();
                                        hasMoTa = (res != DBNull.Value && res != null);
                                    }
                                    string sqlAddMem = hasMoTa ?
                                        "INSERT INTO LOI_MOI_GIA_NHAP (ma_doi, ma_nguoi_duoc_moi, ma_nguoi_gui, ma_vi_tri, mo_ta) SELECT ma_doi, ma_nguoi_duoc_moi, ma_nguoi_gui, ma_vi_tri, mo_ta FROM YEU_CAU_MOI_THANH_VIEN_DOI WHERE ma_yeu_cau = @Ma" :
                                        "INSERT INTO LOI_MOI_GIA_NHAP (ma_doi, ma_nguoi_duoc_moi, ma_nguoi_gui) SELECT ma_doi, ma_nguoi_duoc_moi, ma_nguoi_gui FROM YEU_CAU_MOI_THANH_VIEN_DOI WHERE ma_yeu_cau = @Ma";
                                    try 
                                    {
                                        using (var cmd = new SqlCommand(sqlAddMem, conn, tx))
                                        {
                                            cmd.Parameters.AddWithValue("@Ma", req.ma_yeu_cau);
                                            cmd.ExecuteNonQuery();
                                        }
                                    } catch { /* Ignore */ }
                                }
                                string sqlHandleDoi = "DELETE FROM YEU_CAU_MOI_THANH_VIEN_DOI WHERE ma_yeu_cau = @Ma";
                                using (var cmd = new SqlCommand(sqlHandleDoi, conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@Ma", req.ma_yeu_cau);
                                    cmd.ExecuteNonQuery();
                                }
                                break;
                                
                            case "loi_moi":
                                int? lmNguoiDuocMoi = null, lmMaDoi = null, lmMaViTri = null;
                                using (var cmdRead = new SqlCommand("SELECT ma_nguoi_duoc_moi, ma_doi, ma_vi_tri FROM LOI_MOI_GIA_NHAP WHERE ma_loi_moi = @Ma", conn, tx))
                                {
                                    cmdRead.Parameters.AddWithValue("@Ma", req.ma_yeu_cau);
                                    using (var rdr = cmdRead.ExecuteReader())
                                    {
                                        if (rdr.Read())
                                        {
                                            lmNguoiDuocMoi = rdr.GetInt32(0);
                                            lmMaDoi = rdr["ma_doi"] != DBNull.Value ? (int?)rdr.GetInt32(1) : null;
                                            lmMaViTri = rdr["ma_vi_tri"] != DBNull.Value ? (int?)rdr.GetInt32(2) : null;
                                        }
                                    }
                                }

                                using (var cmd = new SqlCommand("DELETE FROM LOI_MOI_GIA_NHAP WHERE ma_loi_moi = @Ma", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@Ma", req.ma_yeu_cau);
                                    cmd.ExecuteNonQuery();
                                }

                                if (req.chap_nhan && lmNguoiDuocMoi.HasValue && lmMaDoi.HasValue)
                                {
                                    try
                                    {
                                        using (var cmd = new SqlCommand("INSERT INTO THANH_VIEN_DOI (ma_nguoi_dung, ma_doi, ma_vi_tri, vai_tro_noi_bo, phan_he) VALUES (@Nd, @Doi, @ViTri, 'thanh_vien', 'TuyenThu')", conn, tx))
                                        {
                                            cmd.Parameters.AddWithValue("@Nd", lmNguoiDuocMoi.Value);
                                            cmd.Parameters.AddWithValue("@Doi", lmMaDoi.Value);
                                            cmd.Parameters.AddWithValue("@ViTri", lmMaViTri.HasValue ? (object)lmMaViTri.Value : DBNull.Value);
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Insert loi_moi member error: " + ex.Message); }
                                }
                                break;

                            case "xin_gia_nhap":
                                int? xgMaDoi = null, xgMaNguoiDung = null;
                                using (var cmdRead = new SqlCommand("SELECT ma_doi, ma_nguoi_dung FROM XIN_GIA_NHAP WHERE ma_don_xin = @Ma", conn, tx))
                                {
                                    cmdRead.Parameters.AddWithValue("@Ma", req.ma_yeu_cau);
                                    using (var rdr = cmdRead.ExecuteReader())
                                    {
                                        if (rdr.Read())
                                        {
                                            xgMaDoi = rdr["ma_doi"] != DBNull.Value ? (int?)rdr.GetInt32(0) : null;
                                            xgMaNguoiDung = rdr.GetInt32(1);
                                        }
                                    }
                                }

                                using (var cmd = new SqlCommand("DELETE FROM XIN_GIA_NHAP WHERE ma_don_xin = @Ma", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@Ma", req.ma_yeu_cau);
                                    cmd.ExecuteNonQuery();
                                }

                                if (req.chap_nhan && xgMaDoi.HasValue && xgMaNguoiDung.HasValue)
                                {
                                    try
                                    {
                                        using (var cmd = new SqlCommand("INSERT INTO THANH_VIEN_DOI (ma_nguoi_dung, ma_doi, vai_tro_noi_bo, phan_he) VALUES (@Nd, @Doi, 'thanh_vien', 'TuyenThu')", conn, tx))
                                        {
                                            cmd.Parameters.AddWithValue("@Nd", xgMaNguoiDung.Value);
                                            cmd.Parameters.AddWithValue("@Doi", xgMaDoi.Value);
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Insert xin_gia_nhap member error: " + ex.Message); }
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
