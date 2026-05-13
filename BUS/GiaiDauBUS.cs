using DAL;
using DTO;
using System;
using System.Collections.Generic;

namespace BUS
{
    public class GiaiDauBUS
    {
        private readonly GiaiDauDAL dal = new GiaiDauDAL();

        public ApiResponseDTO TaoGiaiDau(int maNguoiDung, TaoGiaiDauRequestDTO req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.ten_giai_dau))
                return Loi("Vui lòng nhập tên giải đấu.");
            if (req.so_doi_toi_thieu < 2) req.so_doi_toi_thieu = 2;
            if (req.giai_doan == null || req.giai_doan.Count == 0)
                return Loi("Vui lòng thêm ít nhất 1 giai đoạn thi đấu.");
            foreach (var gd in req.giai_doan)
            {
                if (string.IsNullOrWhiteSpace(gd.ten_giai_doan))
                    return Loi("Vui lòng nhập tên cho mỗi giai đoạn.");
                if (string.IsNullOrWhiteSpace(gd.the_thuc))
                    return Loi("Vui lòng chọn thể thức cho giai đoạn: " + gd.ten_giai_doan);
            }
            if (req.tong_giai_thuong < 0) return Loi("Tổng giải thưởng không được âm.");
            if (req.danh_sach_giai_thuong != null)
            {
                decimal tongPrizes = 0;
                foreach(var gt in req.danh_sach_giai_thuong)
                {
                    if (string.IsNullOrWhiteSpace(gt.ten_giai)) return Loi("Vui lòng nhập tên cho tất cả hạng mục giải thưởng.");
                    if (gt.gia_tri < 0) return Loi("Giá trị giải thưởng không được âm.");
                    tongPrizes += gt.gia_tri;
                }
                if (tongPrizes > req.tong_giai_thuong)
                {
                    return Loi("Tổng giá trị các giải thưởng chi tiết đang vượt quá Tổng ngân sách công bố!");
                }
            }

            int id = dal.TaoGiaiDau(maNguoiDung, req);
            return Ok("Tạo giải đấu thành công.", dal.LayGiaiDau(id));
        }

        public ApiResponseDTO CapNhatGiaiDau(int maNguoiDung, CapNhatGiaiDauRequestDTO req)
        {
            if (req == null || req.ma_giai_dau <= 0) return Loi("Dữ liệu không hợp lệ.");
            if (string.IsNullOrWhiteSpace(req.ten_giai_dau)) return Loi("Vui lòng nhập tên giải đấu.");
            if (!dal.LaBTC(req.ma_giai_dau, maNguoiDung) && !dal.LaAdmin(maNguoiDung)) return Loi("Bạn không có quyền chỉnh sửa giải đấu này.");
            string tt = dal.LayTrangThai(req.ma_giai_dau);
            if (tt != "nhap" && tt != "bi_tu_choi") return Loi("Chỉ được chỉnh sửa khi giải ở trạng thái Bản nháp hoặc Bị từ chối.");
            if (req.giai_doan == null || req.giai_doan.Count == 0) return Loi("Vui lòng thêm ít nhất 1 giai đoạn.");
            if (req.tong_giai_thuong < 0) return Loi("Tổng giải thưởng không được âm.");
            if (req.danh_sach_giai_thuong != null)
            {
                decimal tongPrizes = 0;
                foreach(var gt in req.danh_sach_giai_thuong)
                {
                    if (string.IsNullOrWhiteSpace(gt.ten_giai)) return Loi("Vui lòng nhập tên cho tất cả hạng mục giải thưởng.");
                    if (gt.gia_tri < 0) return Loi("Giá trị giải thưởng không được âm.");
                    tongPrizes += gt.gia_tri;
                }
                if (tongPrizes > req.tong_giai_thuong)
                {
                    return Loi("Tổng giá trị các giải thưởng chi tiết đang vượt quá Tổng ngân sách công bố!");
                }
            }

            dal.CapNhatGiaiDau(req);
            return Ok("Cập nhật giải đấu thành công.", dal.LayGiaiDau(req.ma_giai_dau));
        }

        public ApiResponseDTO LuuBanNhap(int maNguoiDung, CapNhatGiaiDauRequestDTO req)
        {
            var res = CapNhatGiaiDau(maNguoiDung, req);
            if (!res.success) return res;
            dal.CapNhatTrangThaiVaXoaLyDoTuChoi(req.ma_giai_dau, "nhap");
            return Ok("Đã lưu bản nháp.", dal.LayGiaiDau(req.ma_giai_dau));
        }

        public ApiResponseDTO GuiPheDuyet(int maNguoiDung, int maGiaiDau)
        {
            if (!dal.LaBTC(maGiaiDau, maNguoiDung)) return Loi("Bạn không có quyền.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "nhap" && tt != "bi_tu_choi") return Loi("Chỉ gửi phê duyệt từ trạng thái Bản nháp hoặc Bị từ chối.");
            dal.CapNhatTrangThaiVaXoaLyDoTuChoi(maGiaiDau, "cho_xet_duyet");
            return Ok("Đã gửi yêu cầu phê duyệt.", null);
        }

        public ApiResponseDTO PheDuyet(int maNguoiDung, int maGiaiDau)
        {
            if (!dal.LaAdmin(maNguoiDung)) return Loi("Chỉ Admin mới được phê duyệt.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "cho_xet_duyet") return Loi("Giải đấu không ở trạng thái chờ phê duyệt.");
            dal.CapNhatTrangThai(maGiaiDau, "sap_dien_ra");
            return Ok("Đã phê duyệt giải đấu.", null);
        }

        public ApiResponseDTO TuChoi(int maNguoiDung, TuChoiGiaiDauRequestDTO req)
        {
            if (req == null || req.ma_giai_dau <= 0) return Loi("Dữ liệu không hợp lệ.");
            if (string.IsNullOrWhiteSpace(req.ly_do)) return Loi("Vui lòng nhập lý do từ chối.");
            if (!dal.LaAdmin(maNguoiDung)) return Loi("Chỉ Admin mới được từ chối.");
            string tt = dal.LayTrangThai(req.ma_giai_dau);
            if (tt != "cho_xet_duyet") return Loi("Giải đấu không ở trạng thái chờ phê duyệt.");
            dal.TuChoi(req.ma_giai_dau, req.ly_do);
            return Ok("Đã từ chối giải đấu.", null);
        }

        public ApiResponseDTO MoDangKy(int maNguoiDung, int maGiaiDau)
        {
            if (!dal.LaBTC(maGiaiDau, maNguoiDung)) return Loi("Bạn không có quyền.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "sap_dien_ra") return Loi("Chỉ mở đăng ký từ trạng thái Sắp diễn ra.");
            GiaiDauDTO gd = dal.LayGiaiDau(maGiaiDau);
            int soDoi = dal.DemDoiDaDuyet(maGiaiDau);
            if (gd != null && gd.so_doi_toi_da.HasValue && soDoi >= gd.so_doi_toi_da.Value)
                return Loi("Giải đấu đã đủ số lượng đội, không thể mở đăng ký");
            dal.CapNhatTrangThai(maGiaiDau, "mo_dang_ky");
            return Ok("Da mo dang ky.", dal.LayGiaiDau(maGiaiDau));
        }

        public ApiResponseDTO DongDangKy(int maNguoiDung, int maGiaiDau)
        {
            if (!dal.LaBTC(maGiaiDau, maNguoiDung)) return Loi("Bạn không có quyền.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "mo_dang_ky") return Loi("Chỉ đóng đăng ký từ trạng thái Mở đăng ký.");
            dal.CapNhatTrangThai(maGiaiDau, "khoa_dang_ky");
            return Ok("Da chot so dang ky.", dal.LayGiaiDau(maGiaiDau));
        }

        public ApiResponseDTO MoLaiDangKy(int maNguoiDung, int maGiaiDau)
        {
            if (!dal.LaBTC(maGiaiDau, maNguoiDung)) return Loi("Bạn không có quyền.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "khoa_dang_ky") return Loi("Chỉ mở lại từ trạng thái Khóa đăng ký.");
            GiaiDauDTO gd = dal.LayGiaiDau(maGiaiDau);
            int soDoi = dal.DemDoiDaDuyet(maGiaiDau);
            if (gd != null && gd.so_doi_toi_da.HasValue && soDoi >= gd.so_doi_toi_da.Value)
                return Loi("Giải đấu đã đủ số lượng đội, không thể mở đăng ký");
            dal.CapNhatTrangThai(maGiaiDau, "mo_dang_ky");
            return Ok("Da mo lai dang ky.", dal.LayGiaiDau(maGiaiDau));
        }

        public ApiResponseDTO ToggleRegistration(int maNguoiDung, ToggleRegistrationRequestDTO req)
        {
            if (req == null || req.ma_giai_dau <= 0) return Loi("Dữ liệu không hợp lệ.");
            if (!dal.LaBTC(req.ma_giai_dau, maNguoiDung)) return Loi("Bạn không có quyền.");
            string tt = dal.LayTrangThai(req.ma_giai_dau);

            if (req.mo_dang_ky)
            {
                if (tt != "sap_dien_ra" && tt != "khoa_dang_ky") return Loi("Chỉ mở đăng ký từ trạng thái Sắp diễn ra hoặc Đã dừng đăng ký.");
                GiaiDauDTO gd = dal.LayGiaiDau(req.ma_giai_dau);
                int soDoi = dal.DemDoiDaDuyet(req.ma_giai_dau);
                if (gd != null && gd.so_doi_toi_da.HasValue && soDoi >= gd.so_doi_toi_da.Value)
                    return Loi("Giải đấu đã đủ số lượng đội, không thể mở đăng ký");
                dal.ToggleRegistration(req.ma_giai_dau, true);
                return Ok("Da mo dang ky.", dal.LayGiaiDau(req.ma_giai_dau));
            }

            if (tt != "mo_dang_ky") return Loi("Chỉ dừng đăng ký khi giải đang mở đăng ký.");
            dal.ToggleRegistration(req.ma_giai_dau, false);
            return Ok("Da dung dang ky.", dal.LayGiaiDau(req.ma_giai_dau));
        }

        public ApiResponseDTO KhoiTranh(int maNguoiDung, int maGiaiDau)
        {
            if (!dal.LaBTC(maGiaiDau, maNguoiDung)) return Loi("Bạn không có quyền.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "khoa_dang_ky") return Loi("Chỉ khởi tranh từ trạng thái Khóa đăng ký.");
            GiaiDauDTO gd = dal.LayGiaiDau(maGiaiDau);
            int soDoi = dal.DemDoiDaDuyet(maGiaiDau);
            if (soDoi < gd.so_doi_toi_thieu)
                return Loi("Chưa đủ số đội tối thiểu (" + gd.so_doi_toi_thieu + " đội). Hiện có " + soDoi + " đội.");
            try
            {
                dal.KhoiTranhVaSinhTran(maGiaiDau);
            }
            catch (InvalidOperationException ex)
            {
                return Loi(ex.Message);
            }
            return Ok("Da khoi tranh giai dau!", null);
        }

        public ApiResponseDTO BeMac(int maNguoiDung, int maGiaiDau)
        {
            if (!dal.LaBTC(maGiaiDau, maNguoiDung)) return Loi("Bạn không có quyền.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "dang_dien_ra") return Loi("Chỉ bế mạc từ trạng thái Đang diễn ra.");
            dal.CapNhatTrangThai(maGiaiDau, "ket_thuc");
            return Ok("Đã bế mạc giải đấu.", null);
        }

        public ApiResponseDTO HuyGiaiDau(int maNguoiDung, int maGiaiDau)
        {
            bool isBTC = dal.LaBTC(maGiaiDau, maNguoiDung);
            bool isAdmin = dal.LaAdmin(maNguoiDung);
            if (!isBTC && !isAdmin) return Loi("Bạn không có quyền hủy giải đấu.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt == "ket_thuc" || tt == "da_huy") return Loi("Không thể hủy giải đấu ở trạng thái này.");
            // Bản nháp chưa public → xóa thật (hard delete)
            if (tt == "nhap")
            {
                bool ok = dal.XoaBanNhap(maGiaiDau);
                return ok ? Ok("Đã xóa bản nháp thành công.", null) : Loi("Không thể xóa bản nháp này.");
            }
            if (tt == "sap_dien_ra")
            {
                if (!isBTC) return Loi("Chỉ Ban tổ chức của giải mới được hủy giải sắp diễn ra.");
                bool ok = dal.XoaGiaiDauCascade(maGiaiDau, "sap_dien_ra");
                return ok ? Ok("Đã hủy và xóa giải đấu sắp diễn ra.", null) : Loi("Không thể hủy giải đấu này.");
            }
            // Các trạng thái đã public → soft delete
            dal.CapNhatTrangThai(maGiaiDau, "da_huy");
            return Ok("Đã hủy giải đấu.", null);
        }

        // Xoa ban nhap (Hard Delete) — chi khi trang_thai = 'nhap', chua public
        public ApiResponseDTO XoaBanNhap(int maNguoiDung, int maGiaiDau)
        {
            if (!dal.LaBTC(maGiaiDau, maNguoiDung)) return Loi("Bạn không có quyền xóa giải đấu này.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "nhap") return Loi("Chỉ được xóa bản nháp. Giải đã public thì dùng Hủy giải.");
            bool ok = dal.XoaBanNhap(maGiaiDau);
            return ok ? Ok("Đã xóa bản nháp thành công.", null) : Loi("Không thể xóa. Vui lòng thử lại.");
        }

        public ApiResponseDTO ToggleKhoaDangKy(int maNguoiDung, int maGiaiDau, bool locked)
        {
            if (!dal.LaBTC(maGiaiDau, maNguoiDung)) return Loi("Bạn không có quyền.");
            string tt = dal.LayTrangThai(maGiaiDau);
            if (tt != "mo_dang_ky") return Loi("Chỉ toggle khóa khi đang Mở đăng ký.");
            dal.ToggleRegistrationLock(maGiaiDau, locked);
            return Ok(locked ? "Đã khóa đăng ký tạm thời." : "Đã mở khóa đăng ký.", null);
        }

        public ApiResponseDTO LayGiaiDauCuaToi(int maNguoiDung)
        {
            return Ok("Lấy danh sách giải đấu thành công.", dal.LayGiaiDauCuaToi(maNguoiDung));
        }

        public ApiResponseDTO LayChiTiet(int maGiaiDau, int? maNguoiDung = null)
        {
            GiaiDauDTO gd = dal.LayGiaiDau(maGiaiDau);
            if (gd == null) return Loi("Không tìm thấy giải đấu.");
            
            if (maNguoiDung.HasValue)
            {
                gd.is_btc = dal.LaBTC(maGiaiDau, maNguoiDung.Value);
            }

            return Ok("Lấy chi tiết thành công.", new GiaiDauChiTietDTO
            {
                giai_dau = gd,
                giai_doan = dal.LayGiaiDoan(maGiaiDau),
                doi_tham_gia = dal.LayDoiThamGia(maGiaiDau),
                danh_sach_giai_thuong = dal.LayDanhSachGiaiThuong(maGiaiDau),
                nhan_su = dal.LayNhanSu(maGiaiDau),
                tran_dau = dal.LayTranDau(maGiaiDau),
                bang_xep_hang = dal.LayBangXepHang(maGiaiDau)
            });
        }

        public ApiResponseDTO LayTranDauCuaTrongTai(int maNguoiDung)
        {
            return Ok("Lấy danh sách trận được phân công thành công.", dal.LayTranDauCuaTrongTai(maNguoiDung));
        }

        public ApiResponseDTO LayDanhSachChoPheDuyet(int maNguoiDung)
        {
            if (!dal.LaAdmin(maNguoiDung)) return Loi("Chỉ Admin mới xem được danh sách này.");
            return Ok("Lấy danh sách thành công.", dal.LayDanhSachChoPheDuyet());
        }

        public ApiResponseDTO LayDanhSachPublic()
        {
            return Ok("Lấy danh sách thành công.", dal.LayDanhSachPublic());
        }

        private ApiResponseDTO Ok(string msg, object data)
        {
            return new ApiResponseDTO { success = true, message = msg, data = data };
        }

        private ApiResponseDTO Loi(string msg)
        {
            return new ApiResponseDTO { success = false, message = msg, data = null };
        }

        public ApiResponseDTO DangKyThamGia(int maNguoiDung, DangKyGiaiDauRequestDTO req)
        {
            if (req == null || req.ma_giai_dau <= 0) return Loi("Dữ liệu không hợp lệ.");

            int maDoiToRegister = req.ma_doi;

            if (maDoiToRegister <= 0)
            {
                GiaiDauDTO gd = dal.LayGiaiDau(req.ma_giai_dau);
                if (gd == null) return Loi("Không tìm thấy giải đấu.");

                using (var conn = DbConnectionFactory.CreateConnection())
                {
                    string sql = @"
                        SELECT TOP 1 d.ma_doi
                        FROM DOI d
                        WHERE d.ma_doi_truong = @u 
                          AND (d.ma_tro_choi = @game OR @game IS NULL)
                          AND d.trang_thai = 'dang_hoat_dong'";
                    
                    using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", maNguoiDung);
                        cmd.Parameters.AddWithValue("@game", (object)gd.ma_tro_choi ?? DBNull.Value);
                        conn.Open();
                        var res = cmd.ExecuteScalar();
                        if (res == null || res == DBNull.Value)
                            return Loi("Bạn chưa có đội thi đấu cho game này. Hãy tạo đội trước khi đăng ký.");
                        
                        maDoiToRegister = System.Convert.ToInt32(res);
                    }
                }
            }
            else
            {
                bool isPresident = false;
                using (var conn = DbConnectionFactory.CreateConnection())
                {
                    string sql = @"
                        SELECT COUNT(1)
                        FROM DOI d
                        INNER JOIN GIAI_DAU gd ON gd.ma_giai_dau = @giaiDau
                        WHERE d.ma_doi = @doi
                          AND d.ma_doi_truong = @u
                          AND d.trang_thai = 'dang_hoat_dong'
                          AND (gd.ma_tro_choi IS NULL OR d.ma_tro_choi = gd.ma_tro_choi)";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@doi", maDoiToRegister);
                        cmd.Parameters.AddWithValue("@giaiDau", req.ma_giai_dau);
                        cmd.Parameters.AddWithValue("@u", maNguoiDung);
                        conn.Open();
                        isPresident = System.Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
                if (!isPresident) return Loi("Chỉ Chủ tịch đội mới được đăng ký giải cùng game với đội.");
            }

            string tt = dal.LayTrangThai(req.ma_giai_dau);
            if (tt != "mo_dang_ky") return Loi("Giải đấu hiện không mở đăng ký.");

            dal.DangKyThamGiaGiai(req.ma_giai_dau, maDoiToRegister);
            return Ok("Đã nộp đơn đăng ký thành công. Vui lòng chờ BTC xét duyệt.", null);
        }

        public ApiResponseDTO MoiDoiThamGia(int maNguoiDung, MoiThamGiaGiaiRequestDTO req)
        {
            if (req == null || req.ma_giai_dau <= 0 || req.ma_doi <= 0) return Loi("Dữ liệu không hợp lệ.");
            if (!dal.LaBTC(req.ma_giai_dau, maNguoiDung)) return Loi("Chỉ BTC mới được mời đội.");
            
            dal.MoiDoiThamGia(req.ma_giai_dau, req.ma_doi, req.loi_nhan);
            return Ok("Đã gửi lời mời đến đội.", null);
        }

        public ApiResponseDTO MoiNhanSu(int maNguoiDung, MoiNhanSuGiaiDauRequestDTO req)
        {
            if (req == null || req.ma_giai_dau <= 0 || string.IsNullOrWhiteSpace(req.username_or_email)) return Loi("Dữ liệu không hợp lệ.");
            if (!dal.LaBTC(req.ma_giai_dau, maNguoiDung)) return Loi("Chỉ BTC mới được mời nhân sự.");
            if (req.vai_tro != "btc" && req.vai_tro != "trong_tai") return Loi("Vai trò không hợp lệ.");

            dal.MoiNhanSu(req.ma_giai_dau, req.username_or_email, req.vai_tro, req.loi_nhan);
            return Ok("Đã gửi lời mời.", null);
        }

        public ApiResponseDTO LayThanhVienDoi(int maNguoiDung, int maTran, int maDoi)
        {
            if (!dal.DoiThuocTran(maTran, maDoi)) return Loi("Đội không thuộc trận đấu này.");
            if (!dal.LaDoiTruong(maDoi, maNguoiDung)) return Loi("Chỉ đội trưởng mới được chốt đội hình.");
            return Ok("Lấy danh sách thành viên thành công.", dal.LayThanhVienDoi(maDoi));
        }

        public ApiResponseDTO LayViTriTheoGiai(int maGiaiDau)
        {
            return Ok("Lấy danh sách vị trí thành công.", dal.LayViTriTheoGame(maGiaiDau));
        }

        public ApiResponseDTO SetupTranDau(int maNguoiDung, SetupTranDauRequestDTO req)
        {
            if (req == null || req.ma_tran <= 0) return Loi("Dữ liệu không hợp lệ.");
            if (req.ma_trong_tai <= 0) return Loi("Vui lòng chọn trọng tài.");
            if (string.IsNullOrWhiteSpace(req.the_thuc_tran)) return Loi("Vui lòng chọn thể thức trận.");
            dal.SetupTranDau(req);
            return Ok("Đã lưu cấu hình trận và gửi thông báo xác nhận.", null);
        }

        public ApiResponseDTO SubmitLineup(int maNguoiDung, SubmitLineupRequestDTO req)
        {
            if (req == null || req.ma_tran <= 0 || req.ma_doi <= 0) return Loi("Dữ liệu không hợp lệ.");
            if (!dal.DoiThuocTran(req.ma_tran, req.ma_doi)) return Loi("Đội không thuộc trận đấu này.");
            if (!dal.LaDoiTruong(req.ma_doi, maNguoiDung)) return Loi("Chỉ đội trưởng mới được gửi đội hình.");
            if (req.thanh_vien == null || req.thanh_vien.Count == 0) return Loi("Vui lòng chọn đội hình xuất phát.");

            string loaiGame = dal.LayLoaiGameTheoTran(req.ma_tran);
            int required = (loaiGame == "MOBA" || loaiGame == "FPS") ? 5 : 0;
            if (required > 0 && req.thanh_vien.Count != required) return Loi("Đội hình phải có đúng " + required + " người.");
            if (loaiGame == "BATTLEROYALE" && req.thanh_vien.Count > 4) return Loi("Đội hình sinh tồn tối đa 4 người.");

            dal.SubmitLineup(req);
            return Ok("Đã gửi đội hình xuất phát.", null);
        }

        public ApiResponseDTO BatDauTran(int maNguoiDung, GiaiDauActionRequestDTO req)
        {
            int maTran = req == null ? 0 : (req.ma_tran > 0 ? req.ma_tran : req.ma_giai_dau);
            if (maTran <= 0) return Loi("Du lieu khong hop le.");
            dal.BatDauTran(maTran);
            return Ok("Trận đấu đã bắt đầu.", null);
        }

        public ApiResponseDTO UpdateMatchStats(int maNguoiDung, UpdateMatchStatsRequestDTO req)
        {
            if (req == null || req.ma_tran <= 0) return Loi("Dữ liệu không hợp lệ.");
            dal.UpdateMatchStats(req);
            return Ok("Đã lưu kết quả ván đấu và cập nhật bảng xếp hạng.", null);
        }

        public ApiResponseDTO UpdateMatchStatsTrongTai(int maNguoiDung, UpdateMatchStatsRequestDTO req)
        {
            if (req == null || req.ma_tran <= 0) return Loi("Dữ liệu không hợp lệ.");
            if (!dal.LaTrongTaiCuaTranDaXacNhan(req.ma_tran, maNguoiDung)) return Loi("Bạn chưa được xác nhận điều hành trận đấu này.");
            dal.UpdateMatchStats(req);
            return Ok("Đã gửi kết quả trận đấu cho Ban tổ chức xác nhận.", null);
        }

        public ApiResponseDTO SaveMatchResults(int maNguoiDung, UpdateMatchStatsRequestDTO req)
        {
            if (req == null || req.ma_tran <= 0) return Loi("Du lieu khong hop le.");
            if (!dal.LaTrongTaiCuaTranDaXacNhan(req.ma_tran, maNguoiDung)) return Loi("Ban chua duoc xac nhan dieu hanh tran dau nay.");
            bool completed = dal.SaveMatchResults(req);
            return Ok(completed ? "Da luu ket qua va ket thuc tran dau." : "Da luu ket qua van dau.", new { match_completed = completed });
        }

        public ApiResponseDTO ChotKetQuaTran(int maNguoiDung, GiaiDauActionRequestDTO req)
        {
            int maTran = req == null ? 0 : (req.ma_tran > 0 ? req.ma_tran : req.ma_giai_dau);
            if (maTran <= 0) return Loi("Du lieu khong hop le.");
            dal.ChotKetQuaTran(maTran);
            return Ok("Đã xác nhận kết thúc trận.", null);
        }
    }
}
