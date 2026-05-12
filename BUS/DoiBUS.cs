using DAL;
using DTO;
using System;
using System.Collections.Generic;

namespace BUS
{
    public class DoiBUS
    {
        private readonly DoiDAL doiDAL = new DoiDAL();
        private readonly HoSoThiDauDAL hoSoThiDauDAL = new HoSoThiDauDAL();

        public ApiResponseDTO LayDanhSachDoi(string tuKhoa, int? maTroChoi, int? maNguoiDung)
        {
            return ThanhCong("Láº¥y danh sÃ¡ch Ä‘á»™i thÃ nh cÃ´ng.", doiDAL.TimKiemDoi(tuKhoa, maTroChoi, maNguoiDung));
        }

        public ApiResponseDTO TimKiemDoiTuyChon(string keyword)
        {
            return ThanhCong("Tìm kiếm đội thành công.", doiDAL.TimKiemDoiTuyChon(keyword));
        }

        public ApiResponseDTO LayDoiCuaToi(int maNguoiDung)
        {
            return ThanhCong("Láº¥y danh sÃ¡ch Ä‘á»™i cá»§a tÃ´i thÃ nh cÃ´ng.", doiDAL.LayDoiCuaToi(maNguoiDung));
        }

        public ApiResponseDTO LayChiTietDoi(int maDoi, int? maNguoiDung)
        {
            DoiDTO doi;
            try
            {
                doi = doiDAL.LayDoi(maDoi, maNguoiDung);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LayDoi error: " + ex.Message);
                return Loi("Lỗi khi tải thông tin đội: " + ex.Message);
            }
            if (doi == null) return Loi("Không tìm thấy đội.");

            List<ThanhVienDoiDTO> thanhVien = new List<ThanhVienDoiDTO>();
            try { thanhVien = doiDAL.LayThanhVien(maDoi); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("LayThanhVien error: " + ex.Message); }

            List<DoiTranDauDTO> lichSu = new List<DoiTranDauDTO>();
            try { lichSu = doiDAL.LayTranDau(maDoi, false); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("LayTranDau(false) error: " + ex.Message); }

            List<DoiGiaiDauDTO> giaiDau = new List<DoiGiaiDauDTO>();
            try { giaiDau = doiDAL.LayGiaiDau(maDoi); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("LayGiaiDau error: " + ex.Message); }

            List<DoiTranDauDTO> tranTiep = new List<DoiTranDauDTO>();
            try { tranTiep = doiDAL.LayTranDau(maDoi, true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("LayTranDau(true) error: " + ex.Message); }

            DoiThongKeDTO thongKe = new DoiThongKeDTO
            {
                tong_tran = lichSu.Count,
                so_tran_thang = lichSu.FindAll(x => x.ket_qua == "thang").Count,
                so_tran_thua = lichSu.FindAll(x => x.ket_qua == "thua").Count,
                so_giai_tham_gia = giaiDau.Count,
                giai_thuong = new List<string>()
            };

            return ThanhCong("Lấy chi tiết đội thành công.", new DoiChiTietDTO
            {
                doi = doi,
                thanh_vien = thanhVien,
                lich_su_thi_dau = lichSu,
                giai_dau = giaiDau,
                tran_dau_tiep_theo = tranTiep,
                thong_ke = thongKe
            });
        }

        public ApiResponseDTO TaoDoi(int maNguoiDung, TaoDoiRequestDTO request)
        {
            ApiResponseDTO validation = ValidateTaoDoi(request);
            if (!validation.success) return validation;
            if (doiDAL.NguoiDungDaCoDoiTheoGame(maNguoiDung, request.ma_tro_choi)) return Loi("Báº¡n Ä‘Ã£ táº¡o hoáº·c tham gia má»™t Ä‘á»™i trong game nÃ y.");
            if (doiDAL.TenDoiDaTonTaiTrongGame(request.ten_doi, request.ma_tro_choi)) return Loi("Tên đội đã tồn tại trong game này. Các đội khác game có thể dùng trùng tên.");

            int maDoi = doiDAL.TaoDoi(maNguoiDung, request);
            return ThanhCong("Táº¡o Ä‘á»™i thÃ nh cÃ´ng.", doiDAL.LayDoi(maDoi, maNguoiDung));
        }

        public ApiResponseDTO CapNhatDoi(int maNguoiDung, CapNhatDoiRequestDTO request)
        {
            if (request == null || request.ma_doi <= 0) return Loi("Dá»¯ liá»‡u Ä‘á»™i khÃ´ng há»£p lá»‡.");
            if (string.IsNullOrWhiteSpace(request.ten_doi)) return Loi("Vui lÃ²ng nháº­p tÃªn Ä‘á»™i.");
            if (!LaChuTich(request.ma_doi, maNguoiDung)) return Loi("Chá»‰ chá»§ tá»‹ch má»›i Ä‘Æ°á»£c sá»­a thÃ´ng tin Ä‘á»™i.");
            DoiDTO doiHienTai = doiDAL.LayDoi(request.ma_doi, maNguoiDung);
            if (doiHienTai == null) return Loi("Không tìm thấy đội.");
            if (doiDAL.TenDoiDaTonTaiTrongGame(request.ten_doi, doiHienTai.ma_tro_choi, request.ma_doi)) return Loi("Tên đội đã tồn tại trong game này.");
            doiDAL.CapNhatDoi(request);
            return ThanhCong("Cáº­p nháº­t Ä‘á»™i thÃ nh cÃ´ng.", doiDAL.LayDoi(request.ma_doi, maNguoiDung));
        }

        public ApiResponseDTO CapNhatTuyenDung(int maNguoiDung, BatTuyenDungRequestDTO request)
        {
            if (request == null || request.ma_doi <= 0) return Loi("Dá»¯ liá»‡u Ä‘á»™i khÃ´ng há»£p lá»‡.");
            if (!LaChuTich(request.ma_doi, maNguoiDung)) return Loi("Chá»‰ chá»§ tá»‹ch má»›i Ä‘Æ°á»£c báº­t hoáº·c táº¯t tuyá»ƒn dá»¥ng.");
            doiDAL.CapNhatTuyenDung(request.ma_doi, request.dang_tuyen);
            return ThanhCong("Cáº­p nháº­t tuyá»ƒn dá»¥ng thÃ nh cÃ´ng.", null);
        }

        public ApiResponseDTO XoaDoi(int maNguoiDung, int maDoi)
        {
            if (!LaChuTich(maDoi, maNguoiDung)) return Loi("Chá»‰ chá»§ tá»‹ch má»›i Ä‘Æ°á»£c xÃ³a Ä‘á»™i.");
            doiDAL.XoaDoi(maDoi);
            return ThanhCong("ÄÃ£ giáº£i thá»ƒ Ä‘á»™i.", null);
        }

        public ApiResponseDTO MoiThanhVien(int maNguoiDung, MoiThanhVienRequestDTO request)
        {
            if (request == null || request.ma_doi <= 0 || string.IsNullOrWhiteSpace(request.username_or_email)) return Loi("Vui lÃ²ng nháº­p ngÆ°á»i cáº§n má»i.");
            string vaiTro = doiDAL.LayVaiTro(request.ma_doi, maNguoiDung);
            if (vaiTro != "chu_tich" && vaiTro != "ban_dieu_hanh" && vaiTro != "doi_truong") return Loi("Báº¡n khÃ´ng cÃ³ quyá»n má»i thÃ nh viÃªn.");

            int? maNguoiNhan = doiDAL.TimNguoiDung(request.username_or_email);
            if (!maNguoiNhan.HasValue) return Loi("KhÃ´ng tÃ¬m tháº¥y ngÆ°á»i dÃ¹ng cáº§n má»i.");
            if (maNguoiNhan.Value == maNguoiDung) return Loi("Báº¡n khÃ´ng thá»ƒ tá»± má»i chÃ­nh mÃ¬nh.");
            int maTroChoi = doiDAL.LayGameTheoDoi(request.ma_doi);
            if (maTroChoi <= 0) return Loi("Không xác định được game của đội.");
            if (doiDAL.NguoiDungDaCoDoiTheoGame(maNguoiNhan.Value, maTroChoi)) return Loi("Người này đã có đội trong game này.");
            if (!doiDAL.LayHoSoTheoGame(maNguoiNhan.Value, maTroChoi).HasValue) return Loi("Người được mời chưa có hồ sơ thi đấu của game mà đội đang thi đấu.");

            int maNhom = doiDAL.LayMaNhomTheoDoi(request.ma_doi);
            if (vaiTro == "doi_truong")
            {
                doiDAL.TaoYeuCauMoiThanhVien(request.ma_doi, maNhom, maNguoiNhan.Value, maNguoiDung, request.ma_vi_tri, request.mo_ta);
                return ThanhCong("ÄÃ£ gá»­i yÃªu cáº§u má»i thÃ nh viÃªn cho chá»§ tá»‹ch hoáº·c ban Ä‘iá»u hÃ nh duyá»‡t.", null);
            }

            doiDAL.TaoLoiMoi(request.ma_doi, maNhom, maNguoiNhan.Value, maNguoiDung, request.ma_vi_tri, request.mo_ta);
            return ThanhCong("ÄÃ£ gá»­i lá»i má»i vÃ o Ä‘á»™i.", null);
        }

        public ApiResponseDTO XinGiaNhap(int maNguoiDung, XinGiaNhapDoiRequestDTO request)
        {
            if (request == null || request.ma_doi <= 0) return Loi("Dữ liệu xin gia nhập không hợp lệ.");
            if (!doiDAL.DoiDangTuyen(request.ma_doi)) return Loi("Đội này hiện chưa bật tuyển dụng.");

            int maTroChoi = doiDAL.LayGameTheoDoi(request.ma_doi);
            if (maTroChoi <= 0) return Loi("Không xác định được game của đội.");
            if (doiDAL.NguoiDungDaCoDoiTheoGame(maNguoiDung, maTroChoi)) return Loi("Bạn đã có đội trong game này.");

            int? maHoSo = doiDAL.LayHoSoTheoGame(maNguoiDung, maTroChoi);
            if (!maHoSo.HasValue) return Loi("Bạn cần tạo hồ sơ thi đấu của game này trước khi xin gia nhập đội.");
            if (doiDAL.DaCoDonXinGiaNhap(request.ma_doi, maNguoiDung)) return Loi("Bạn đã gửi đơn xin gia nhập đội này và đang chờ duyệt.");

            int maNhom = doiDAL.LayMaNhomTheoDoi(request.ma_doi);
            doiDAL.TaoDonXinGiaNhap(request.ma_doi, maNhom, maNguoiDung, maHoSo.Value);
            return ThanhCong("Đã gửi đơn xin gia nhập đội.", null);
        }
        public ApiResponseDTO CapNhatVaiTroThanhVien(int maNguoiDung, CapNhatVaiTroThanhVienRequestDTO request)
        {
            if (request == null || request.ma_doi <= 0 || request.ma_nguoi_dung <= 0) return Loi("Dá»¯ liá»‡u thÃ nh viÃªn khÃ´ng há»£p lá»‡.");
            if (!LaChuTich(request.ma_doi, maNguoiDung)) return Loi("Chá»‰ chá»§ tá»‹ch má»›i Ä‘Æ°á»£c phÃ¢n quyá»n thÃ nh viÃªn.");
            if (request.vai_tro_noi_bo != "ban_dieu_hanh" && request.vai_tro_noi_bo != "doi_truong" && request.vai_tro_noi_bo != "thanh_vien") return Loi("Vai trÃ² khÃ´ng há»£p lá»‡.");
            doiDAL.CapNhatVaiTro(request.ma_doi, request.ma_nguoi_dung, request.vai_tro_noi_bo);
            return ThanhCong("Cáº­p nháº­t vai trÃ² thÃ nh cÃ´ng.", null);
        }

        public ApiResponseDTO LoaiThanhVien(int maNguoiDung, LoaiThanhVienRequestDTO request)
        {
            if (request == null || request.ma_doi <= 0 || request.ma_nguoi_dung <= 0) return Loi("Dá»¯ liá»‡u thÃ nh viÃªn khÃ´ng há»£p lá»‡.");
            if (!LaChuTich(request.ma_doi, maNguoiDung)) return Loi("Chá»‰ chá»§ tá»‹ch má»›i Ä‘Æ°á»£c loáº¡i thÃ nh viÃªn.");
            doiDAL.LoaiThanhVien(request.ma_doi, request.ma_nguoi_dung);
            return ThanhCong("ÄÃ£ loáº¡i thÃ nh viÃªn khá»i Ä‘á»™i.", null);
        }

        public ApiResponseDTO RoiDoi(int maNguoiDung, RoiDoiRequestDTO request)
        {
            if (request == null || request.ma_doi <= 0) return Loi("Dữ liệu rời đội không hợp lệ.");
            string vaiTro = doiDAL.LayVaiTro(request.ma_doi, maNguoiDung);
            if (string.IsNullOrWhiteSpace(vaiTro)) return Loi("Bạn không phải thành viên của đội này.");
            if (vaiTro == "chu_tich") return Loi("Chủ tịch không thể rời đội. Vui lòng chuyển quyền hoặc xóa đội.");
            doiDAL.LoaiThanhVien(request.ma_doi, maNguoiDung);
            return ThanhCong("Bạn đã rời đội.", null);
        }

        public ApiResponseDTO LayYeuCau(int maNguoiDung)
        {
            return ThanhCong("Láº¥y danh sÃ¡ch yÃªu cáº§u thÃ nh cÃ´ng.", doiDAL.LayYeuCau(maNguoiDung));
        }

        public ApiResponseDTO XuLyYeuCau(int maNguoiDung, XuLyYeuCauDoiRequestDTO request)
        {
            if (request == null || request.ma_yeu_cau <= 0 || string.IsNullOrWhiteSpace(request.loai_yeu_cau)) return Loi("YÃªu cáº§u khÃ´ng há»£p lá»‡.");
            if (request.loai_yeu_cau == "loi_moi") doiDAL.XuLyLoiMoi(request.ma_yeu_cau, maNguoiDung, request.chap_nhan);
            else if (request.loai_yeu_cau == "yeu_cau_moi") doiDAL.XuLyYeuCauMoi(request.ma_yeu_cau, maNguoiDung, request.chap_nhan);
            else if (request.loai_yeu_cau == "xin_gia_nhap") doiDAL.XuLyDonXinGiaNhap(request.ma_yeu_cau, maNguoiDung, request.chap_nhan);
            else if (request.loai_yeu_cau == "loi_moi_tham_gia_giai")
            {
                var yDal = new YeuCauDAL();
                var req = new XuLyYeuCauRequestDTO { ma_yeu_cau = request.ma_yeu_cau, loai_yeu_cau = request.loai_yeu_cau, chap_nhan = request.chap_nhan };
                yDal.XuLyYeuCau(maNguoiDung, req);
            }
            else return Loi("Loáº¡i yÃªu cáº§u khÃ´ng há»£p lá»‡.");
            return ThanhCong("Xá»­ lÃ½ yÃªu cáº§u thÃ nh cÃ´ng.", null);
        }

        public ApiResponseDTO LayTroChoi()
        {
            return ThanhCong("Láº¥y danh sÃ¡ch trÃ² chÆ¡i thÃ nh cÃ´ng.", hoSoThiDauDAL.GetTroChoi());
        }

        public ApiResponseDTO LayViTri(int? maTroChoi)
        {
            return ThanhCong("Láº¥y danh sÃ¡ch vá»‹ trÃ­ thÃ nh cÃ´ng.", hoSoThiDauDAL.GetViTri(maTroChoi, "TuyenThu"));
        }

        private ApiResponseDTO ValidateTaoDoi(TaoDoiRequestDTO request)
        {
            if (request == null) return Loi("Dá»¯ liá»‡u Ä‘á»™i khÃ´ng há»£p lá»‡.");
            if (string.IsNullOrWhiteSpace(request.ten_doi)) return Loi("Vui lÃ²ng nháº­p tÃªn Ä‘á»™i.");
            if (request.ma_tro_choi <= 0) return Loi("Vui lÃ²ng chá»n game cá»§a Ä‘á»™i.");
            return ThanhCong("Dá»¯ liá»‡u há»£p lá»‡.", null);
        }

        private bool LaChuTich(int maDoi, int maNguoiDung)
        {
            return doiDAL.LayVaiTro(maDoi, maNguoiDung) == "chu_tich";
        }

        private ApiResponseDTO ThanhCong(string message, object data)
        {
            return new ApiResponseDTO { success = true, message = message, data = data };
        }

        private ApiResponseDTO Loi(string message)
        {
            return new ApiResponseDTO { success = false, message = message, data = null };
        }
    }
}

