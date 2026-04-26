using System;
using System.Collections.Generic;
using System.Data;
using DAL;
using DTO;

namespace BUS
{
    public class TournamentBuilderBUS
    {
        private readonly TournamentBuilderDAL _dal = new TournamentBuilderDAL();
        private readonly IdentityDAL _identityDal = new IdentityDAL();

        private const string TrangThaiBanNhap = "ban_nhap";
        private const string TrangThaiChoPheDuyet = "cho_phe_duyet";
        private const string TrangThaiMoDangKy = "mo_dang_ky";
        private const string TrangThaiDangDienRa = "dang_dien_ra";
        private const string TrangThaiKhoa = "khoa";

        public ServiceResultDTO TaoBanNhap(TaoGiaiDauDTO dto)
        {
            if (dto == null || dto.MaNguoiTao <= 0 || string.IsNullOrWhiteSpace(dto.TenGiaiDau))
            {
                return ServiceResultDTO.Fail("Dữ liệu khởi tạo giải đấu không hợp lệ.");
            }

            dto.TenGiaiDau = dto.TenGiaiDau.Trim();
            if (System.Text.RegularExpressions.Regex.IsMatch(dto.TenGiaiDau, @"^[\d\W_]"))
            {
                return ServiceResultDTO.Fail("Tên giải đấu không được bắt đầu bằng số hoặc ký tự đặc biệt.");
            }

            if (dto.NgayBatDau <= DateTime.Now)
            {
                return ServiceResultDTO.Fail("Ngày bắt đầu phải là ngày trong tương lai.");
            }

            if (dto.NgayBatDau >= dto.NgayKetThuc)
            {
                return ServiceResultDTO.Fail("Thời gian bắt đầu giải phải nhỏ hơn thời gian kết thúc giải.");
            }

            if (dto.ThoiGianDongDangKy.HasValue && dto.ThoiGianDongDangKy.Value >= dto.NgayBatDau)
            {
                return ServiceResultDTO.Fail("Thời gian đóng đăng ký phải nhỏ hơn thời gian diễn ra giải.");
            }

            if (string.IsNullOrWhiteSpace(dto.TheThuc))
            {
                return ServiceResultDTO.Fail("Vui lòng chọn thể thức thi đấu.");
            }

            string trangThai = LaAdmin(dto.MaNguoiTao) ? TrangThaiMoDangKy : TrangThaiChoPheDuyet;

            int maGiaiDau = _dal.TaoGiaiDau(dto, trangThai);

            if (dto.GiaiThuongs != null && dto.GiaiThuongs.Count > 0)
            {
                _dal.ThemGiaiThuong(maGiaiDau, dto.GiaiThuongs);
            }

            string msg = LaAdmin(dto.MaNguoiTao) ? "Tạo giải đấu thành công. Giải đấu đã được tự động phê duyệt." : "Tạo giải đấu thành công. Vui lòng chờ admin phê duyệt.";
            return ServiceResultDTO.Ok(msg, new { maGiaiDau, trangThai });
        }

        public ServiceResultDTO GuiXetDuyet(int maNguoiGui, int maGiaiDau)
        {
            DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiGui, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền gửi xét duyệt giải đấu này.");
            }

            bool ok = _dal.GuiXetDuyet(maGiaiDau);
            return ok
                ? ServiceResultDTO.Ok("Đã gửi xét duyệt thành công.", new { maGiaiDau, trangThai = "chờ phê duyệt" })
                : ServiceResultDTO.Fail("Giải đấu phải ở trạng thái bản nháp để gửi xét duyệt.");
        }

        public ServiceResultDTO PheDuyet(int maAdmin, int maGiaiDau)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới có quyền phê duyệt.");
            }

            bool ok = _dal.PheDuyetGiai(maGiaiDau);
            return ok
                ? ServiceResultDTO.Ok("Phê duyệt giải đấu thành công.", new { maGiaiDau, trangThai = "mở đăng ký" })
                : ServiceResultDTO.Fail("Không thể phê duyệt. Hãy kiểm tra giải đang ở trạng thái chờ phê duyệt.");
        }

        public ServiceResultDTO TuChoiVaXoaCung(int maAdmin, int maGiaiDau)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới có quyền từ chối.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            string trangThai = giaiDau["trang_thai"].ToString();
            if (!string.Equals(trangThai, TrangThaiChoPheDuyet, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(trangThai, TrangThaiBanNhap, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Chỉ được từ chối khi giải ở trạng thái bản nháp hoặc chờ phê duyệt.");
            }

            bool ok = _dal.XoaCungGiai(maGiaiDau);
            return ok
                ? ServiceResultDTO.Ok("Admin đã từ chối và xóa cứng giải đấu ngay lập tức.")
                : ServiceResultDTO.Fail("Không thể xóa giải đấu.");
        }

        public ServiceResultDTO KhoaGiai(CapNhatTrangThaiGiaiDTO dto)
        {
            if (dto == null || dto.MaGiaiDau <= 0 || dto.MaNguoiThucHien <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu khóa giải không hợp lệ.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(dto.MaGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(dto.MaNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền khóa giải đấu này.");
            }

            dto.TrangThaiMoi = TrangThaiKhoa;
            bool ok = _dal.CapNhatTrangThaiGiai(dto);

            return ok
                ? ServiceResultDTO.Ok("Đã khóa giải đấu và ẩn khỏi người dùng thường.")
                : ServiceResultDTO.Fail("Không thể khóa giải đấu.");
        }

        public ServiceResultDTO MoKhoaGiai(CapNhatTrangThaiGiaiDTO dto)
        {
            if (dto == null || dto.MaGiaiDau <= 0 || dto.MaNguoiThucHien <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu mở khóa giải không hợp lệ.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(dto.MaGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(dto.MaNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền mở khóa giải đấu này.");
            }

            string trangThaiHienTai = giaiDau["trang_thai"].ToString();
            if (!string.Equals(trangThaiHienTai, TrangThaiKhoa, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Giải đấu hiện không ở trạng thái khóa.");
            }

            dto.TrangThaiMoi = TrangThaiMoDangKy;
            bool ok = _dal.CapNhatTrangThaiGiai(dto);

            return ok
                ? ServiceResultDTO.Ok("Đã mở khóa giải đấu.")
                : ServiceResultDTO.Fail("Không thể mở khóa giải đấu.");
        }

        public ServiceResultDTO ThemGiaiDoan(int maNguoiThucHien, TaoGiaiDoanDTO dto)
        {
            if (dto == null || dto.MaGiaiDau <= 0 || string.IsNullOrWhiteSpace(dto.TenGiaiDoan) || string.IsNullOrWhiteSpace(dto.TheThuc))
            {
                return ServiceResultDTO.Fail("Dữ liệu giai đoạn không hợp lệ.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(dto.MaGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền thêm giai đoạn cho giải đấu này.");
            }

            int soDoiDaDuyet = _dal.LaySoLuongThamGiaDaDuyet(dto.MaGiaiDau);
            if (dto.SoDoiDiTiep < 0 || dto.SoDoiDiTiep > soDoiDaDuyet)
            {
                return ServiceResultDTO.Fail("Số đội đi tiếp phải nhỏ hơn hoặc bằng số đội đã duyệt tham gia.");
            }

            if (string.Equals(dto.TheThuc, "champion_rush", StringComparison.OrdinalIgnoreCase)
                && (!dto.DiemNguongMatchPoint.HasValue || dto.DiemNguongMatchPoint.Value <= 0))
            {
                return ServiceResultDTO.Fail("Thể thức Champion Rush bắt buộc cấu hình điểm Match Point.");
            }

            int maGiaiDoan = _dal.TaoGiaiDoan(dto);
            return ServiceResultDTO.Ok("Thêm giai đoạn thành công.", new { maGiaiDoan });
        }

        public ServiceResultDTO LenThuTuGiaiDoan(int maNguoiThucHien, int maGiaiDau, int maGiaiDoan)
        {
            return DoiThuTuGiaiDoan(maNguoiThucHien, maGiaiDau, maGiaiDoan, true);
        }

        public ServiceResultDTO XuongThuTuGiaiDoan(int maNguoiThucHien, int maGiaiDau, int maGiaiDoan)
        {
            return DoiThuTuGiaiDoan(maNguoiThucHien, maGiaiDau, maGiaiDoan, false);
        }

        public ServiceResultDTO XoaGiaiDoan(int maNguoiThucHien, int maGiaiDau, int maGiaiDoan)
        {
            DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền xóa giai đoạn.");
            }

            bool ok = _dal.XoaGiaiDoan(maGiaiDoan);
            return ok ? ServiceResultDTO.Ok("Xóa giai đoạn thành công.") : ServiceResultDTO.Fail("Không thể xóa giai đoạn.");
        }

        public ServiceResultDTO LayDanhSachGiaiCuaToi(int maNguoiTao)
        {
            DataTable dt = _dal.LayDanhSachGiaiCuaToi(maNguoiTao);
            List<object> list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    ma_giai_dau = Convert.ToInt32(row["ma_giai_dau"]),
                    ten_giai_dau = row["ten_giai_dau"].ToString(),
                    ma_tro_choi = row["ma_tro_choi"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ma_tro_choi"]),
                    ten_game = row["ten_game"].ToString(),
                    tong_giai_thuong = Convert.ToDecimal(row["tong_giai_thuong"]),
                    trang_thai = row["trang_thai"].ToString(),
                    ngay_bat_dau = row["ngay_bat_dau"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ngay_bat_dau"])
                });
            }
            return ServiceResultDTO.Ok("Thành công", list);
        }

        public ServiceResultDTO LayDanhSachGiaiDoan(int maGiaiDau)
        {
            List<GiaiDoanDTO> data = _dal.LayDanhSachGiaiDoan(maGiaiDau);
            return ServiceResultDTO.Ok("Lấy danh sách giai đoạn thành công.", data);
        }

        public ServiceResultDTO DuyetDangKyDoi(int maNguoiThucHien, DuyetThamGiaGiaiDTO dto)
        {
            if (dto == null || dto.MaGiaiDau <= 0 || dto.MaNhom <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu duyệt đăng ký không hợp lệ.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(dto.MaGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền duyệt đăng ký đội.");
            }

            if (dto.ChapNhan && _dal.NhomKhacGameGiai(dto.MaGiaiDau, dto.MaNhom))
            {
                return ServiceResultDTO.Fail("Không thể duyệt: Tựa game của đội không khớp với tựa game của giải đấu.");
            }

            bool ok = _dal.CapNhatDuyetThamGia(dto.MaGiaiDau, dto.MaNhom, dto.ChapNhan);
            return ok
                ? ServiceResultDTO.Ok(dto.ChapNhan ? "Đã duyệt đội tham gia." : "Đã từ chối đội tham gia.")
                : ServiceResultDTO.Fail("Không thể cập nhật trạng thái đăng ký đội.");
        }

        public ServiceResultDTO CapNhatHatGiong(int maNguoiThucHien, CapNhatHatGiongDTO dto)
        {
            if (dto == null || dto.MaGiaiDau <= 0 || dto.MaNhom <= 0 || dto.HatGiong <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu hạt giống không hợp lệ.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(dto.MaGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền cập nhật hạt giống.");
            }

            bool ok = _dal.CapNhatHatGiong(dto.MaGiaiDau, dto.MaNhom, dto.HatGiong);
            return ok
                ? ServiceResultDTO.Ok("Cập nhật hạt giống thành công.")
                : ServiceResultDTO.Fail("Không thể cập nhật hạt giống.");
        }

        public ServiceResultDTO CapNhatDoiHinhThiDau(int maNguoiThucHien, CapNhatDoiHinhGiaiDTO dto)
        {
            if (dto == null || dto.MaGiaiDau <= 0 || dto.MaNhom <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu đồng bộ đội hình không hợp lệ.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(dto.MaGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền cập nhật đội hình thi đấu.");
            }

            string trangThai = giaiDau["trang_thai"].ToString();
            if (!string.Equals(trangThai, TrangThaiChoPheDuyet, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(trangThai, TrangThaiMoDangKy, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Danh sách thi đấu đã bị khóa. Bạn không thể thay đổi nhân sự lúc này!");
            }

            int soNguoi = _dal.DongBoDoiHinhThiDau(dto.MaGiaiDau, dto.MaNhom);
            if (soNguoi <= 0)
            {
                return ServiceResultDTO.Fail("Không có thành viên hợp lệ để tạo danh sách thi đấu.");
            }

            return ServiceResultDTO.Ok("Đồng bộ roster thành công.", new { soNguoi });
        }

        public ServiceResultDTO ChuyenSangDangDienRa(int maNguoiThucHien, int maGiaiDau)
        {
            if (maNguoiThucHien <= 0 || maGiaiDau <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu chuyển trạng thái giải không hợp lệ.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền thao tác giải đấu này.");
            }

            string trangThai = giaiDau["trang_thai"].ToString();
            if (string.Equals(trangThai, TrangThaiKhoa, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Giải đấu đang bị khóa, không thể chuyển sang đang diễn ra.");
            }

            List<int> dsNhom = _dal.LayDanhSachNhomDaDuyet(maGiaiDau);
            if (dsNhom.Count < 2)
            {
                return ServiceResultDTO.Fail("Cần ít nhất 2 đội đã duyệt để bắt đầu giải.");
            }

            int tongNguoi = 0;
            foreach (int maNhom in dsNhom)
            {
                int soNguoi = _dal.DongBoDoiHinhThiDau(maGiaiDau, maNhom);
                if (soNguoi <= 0)
                {
                    return ServiceResultDTO.Fail("Không thể khóa roster cho nhóm " + maNhom + ". Hãy kiểm tra đội hình hợp lệ trước khi bắt đầu giải.");
                }
                tongNguoi += soNguoi;
            }

            bool ok = _dal.CapNhatTrangThaiGiaiTheoMa(maGiaiDau, TrangThaiDangDienRa);
            return ok
                ? ServiceResultDTO.Ok("Giải đã chuyển sang đang diễn ra và roster toàn bộ đội đã được khóa.", new { maGiaiDau, tongNhom = dsNhom.Count, tongNguoi })
                : ServiceResultDTO.Fail("Không thể cập nhật trạng thái giải sang đang diễn ra.");
        }

        private ServiceResultDTO DoiThuTuGiaiDoan(int maNguoiThucHien, int maGiaiDau, int maGiaiDoan, bool len)
        {
            DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giải đấu.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền sắp xếp giai đoạn.");
            }

            List<GiaiDoanDTO> list = _dal.LayDanhSachGiaiDoan(maGiaiDau);
            GiaiDoanDTO current = list.Find(x => x.MaGiaiDoan == maGiaiDoan);
            if (current == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giai đoạn cần sắp xếp.");
            }

            int thuTuDich = len ? current.ThuTu - 1 : current.ThuTu + 1;
            GiaiDoanDTO target = list.Find(x => x.ThuTu == thuTuDich);
            if (target == null)
            {
                return ServiceResultDTO.Fail(len ? "Giai đoạn này đã ở vị trí đầu." : "Giai đoạn này đã ở vị trí cuối.");
            }

            bool ok = _dal.HoanDoiThuTuGiaiDoan(maGiaiDau, current.ThuTu, target.ThuTu);
            return ok
                ? ServiceResultDTO.Ok("Sắp xếp giai đoạn thành công.")
                : ServiceResultDTO.Fail("Không thể đổi thứ tự giai đoạn.");
        }

        private bool LaAdmin(int maNguoiDung)
        {
            NguoiDungDTO user = _identityDal.LayTheoId(maNguoiDung);
            return user != null && string.Equals(user.VaiTroHeThong, "admin", StringComparison.OrdinalIgnoreCase);
        }

        private bool CoQuyenQuanLyGiai(int maNguoiDung, DataRow giaiDau)
        {
            if (LaAdmin(maNguoiDung))
            {
                return true;
            }

            if (giaiDau["ma_nguoi_tao"] == DBNull.Value)
            {
                return false;
            }

            return Convert.ToInt32(giaiDau["ma_nguoi_tao"]) == maNguoiDung;
        }

        public ServiceResultDTO LayGiaiCuaToi(int maNguoiDung)
        {
            if (maNguoiDung <= 0) return ServiceResultDTO.Fail("Người dùng không hợp lệ.");

            System.Data.DataTable dt = _dal.LayGiaiCuaToi(maNguoiDung);
            var list = new System.Collections.Generic.List<object>();
            foreach (System.Data.DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    ma_giai_dau      = Convert.ToInt32(row["ma_giai_dau"]),
                    ten_giai_dau     = row["ten_giai_dau"].ToString(),
                    trang_thai       = row["trang_thai"].ToString(),
                    tong_giai_thuong = row["tong_giai_thuong"] == DBNull.Value ? 0 : Convert.ToDouble(row["tong_giai_thuong"]),
                    ngay_bat_dau     = row["ngay_bat_dau"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ngay_bat_dau"]),
                    ngay_ket_thuc    = row["ngay_ket_thuc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ngay_ket_thuc"]),
                    ten_game         = row["ten_game"] == DBNull.Value ? null : row["ten_game"].ToString(),
                    ma_tro_choi      = row["ma_tro_choi"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ma_tro_choi"])
                });
            }
            return ServiceResultDTO.Ok("Lấy danh sách giải thành công.", list);
        }
        public ServiceResultDTO LayDanhSachDangKyDoi(int maNguoiThucHien, int maGiaiDau)
        {
            if (maNguoiThucHien <= 0 || maGiaiDau <= 0)
            {
                return ServiceResultDTO.Fail("Du lieu dang ky doi khong hop le.");
            }

            DataRow giaiDau = _dal.LayGiaiTheoId(maGiaiDau);
            if (giaiDau == null)
            {
                return ServiceResultDTO.Fail("Khong tim thay giai dau.");
            }

            if (!CoQuyenQuanLyGiai(maNguoiThucHien, giaiDau))
            {
                return ServiceResultDTO.Fail("Ban khong co quyen xem danh sach dang ky doi.");
            }

            System.Data.DataTable dt = _dal.LayDanhSachDangKyDoi(maGiaiDau);
            var list = new System.Collections.Generic.List<object>();
            foreach (System.Data.DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    ma_nhom = Convert.ToInt32(row["ma_nhom"]),
                    ten_nhom = row["ten_nhom"].ToString(),
                    ma_doi = Convert.ToInt32(row["ma_doi"]),
                    ten_doi = row["ten_doi"].ToString(),
                    slogan = row["slogan"] == DBNull.Value ? null : row["slogan"].ToString(),
                    ten_game = row["ten_game"] == DBNull.Value ? null : row["ten_game"].ToString(),
                    trang_thai_duyet = row["trang_thai_duyet"].ToString(),
                    hat_giong = row["hat_giong"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["hat_giong"])
                });
            }

            return ServiceResultDTO.Ok("Lay danh sach dang ky doi thanh cong.", list);
        }
    }
}
