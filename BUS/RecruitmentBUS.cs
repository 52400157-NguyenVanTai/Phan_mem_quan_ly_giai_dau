using DAL;
using DTO;
using System;
using System.Data;

namespace BUS
{
    public class RecruitmentBUS
    {
        private readonly RecruitmentDAL _recruitmentDal = new RecruitmentDAL();

        public ServiceResultDTO TaoBaiDang(int maDoi, int maNhom, int maViTri, string noiDung)
        {
            if (maDoi <= 0 || maNhom <= 0 || maViTri <= 0 || string.IsNullOrWhiteSpace(noiDung))
            {
                return ServiceResultDTO.Fail("Dữ liệu bài đăng không hợp lệ.");
            }

            int maBaiDang = _recruitmentDal.TaoBaiDang(maDoi, maNhom, maViTri, noiDung);
            return ServiceResultDTO.Ok("Tạo bài đăng tuyển thành công.", new { maBaiDang });
        }

        public ServiceResultDTO UngTuyen(int maBaiDang, int maUngVien)
        {
            if (maBaiDang <= 0 || maUngVien <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu ứng tuyển không hợp lệ.");
            }

            if (!_recruitmentDal.NguoiDungDangOTrangThaiFreeAgent(maUngVien))
            {
                return ServiceResultDTO.Fail("Bạn đang thuộc đội khác, hãy rời đội hoặc được giải phóng hợp đồng trước khi ứng tuyển.");
            }

            int maDon = _recruitmentDal.TaoDonUngTuyen(maBaiDang, maUngVien);
            return ServiceResultDTO.Ok("Gửi đơn ứng tuyển thành công.", new { maDon });
        }

        public ServiceResultDTO GuiLoiMoi(int maDoi, int maNhom, int maNguoiDuocMoi)
        {
            if (maDoi <= 0 || maNhom <= 0 || maNguoiDuocMoi <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu lời mời không hợp lệ.");
            }

            if (!_recruitmentDal.NguoiDungDangOTrangThaiFreeAgent(maNguoiDuocMoi))
            {
                return ServiceResultDTO.Fail("Người được mời đang thuộc đội khác.");
            }

            int maLoiMoi = _recruitmentDal.TaoLoiMoi(maDoi, maNhom, maNguoiDuocMoi, null);
            return ServiceResultDTO.Ok("Gửi lời mời thành công.", new { maLoiMoi });
        }

        public ServiceResultDTO DuyetDonUngTuyen(int maNguoiDuyet, int maDon, bool chapNhan)
        {
            if (maNguoiDuyet <= 0 || maDon <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu duyệt đơn không hợp lệ.");
            }

            DataRow don = _recruitmentDal.LayDonUngTuyen(maDon);
            if (don == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy đơn ứng tuyển.");
            }

            if (!string.Equals(don["trang_thai"].ToString(), "cho_duyet", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Đơn này đã được xử lý trước đó.");
            }

            int maNhom = Convert.ToInt32(don["ma_nhom"]);
            int maUngVien = Convert.ToInt32(don["ma_ung_vien"]);

            if (!_recruitmentDal.NguoiCoQuyenQuanLyNhom(maNguoiDuyet, maNhom))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền duyệt đơn cho nhóm này.");
            }

            if (!chapNhan)
            {
                bool okReject = _recruitmentDal.CapNhatTrangThaiDon(maDon, "tu_choi");
                return okReject
                    ? ServiceResultDTO.Ok("Đã từ chối đơn ứng tuyển.")
                    : ServiceResultDTO.Fail("Không thể cập nhật trạng thái đơn.");
            }

            if (!_recruitmentDal.NguoiDungDangOTrangThaiFreeAgent(maUngVien))
            {
                return ServiceResultDTO.Fail("Ứng viên không còn ở trạng thái tự do để gia nhập nhóm.");
            }

            bool ok = _recruitmentDal.CapNhatTrangThaiDon(maDon, "chap_nhan");
            if (!ok)
            {
                return ServiceResultDTO.Fail("Không thể duyệt đơn ứng tuyển.");
            }

            _recruitmentDal.TiepNhanUngVienVaoNhom(maNhom, maUngVien);
            _recruitmentDal.TuChoiDonConLaiCuaUngVien(maUngVien, maDon);

            return ServiceResultDTO.Ok("Đã duyệt đơn và thêm ứng viên vào nhóm thi đấu.");
        }

        public ServiceResultDTO PhanHoiLoiMoi(int maNguoiDuocMoi, int maLoiMoi, bool chapNhan)
        {
            if (maNguoiDuocMoi <= 0 || maLoiMoi <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu phản hồi lời mời không hợp lệ.");
            }

            DataRow loiMoi = _recruitmentDal.LayLoiMoi(maLoiMoi);
            if (loiMoi == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy lời mời gia nhập.");
            }

            if (Convert.ToInt32(loiMoi["ma_nguoi_duoc_moi"]) != maNguoiDuocMoi)
            {
                return ServiceResultDTO.Fail("Bạn không phải người nhận lời mời này.");
            }

            if (!string.Equals(loiMoi["trang_thai"].ToString(), "cho_phan_hoi", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Lời mời đã được phản hồi trước đó.");
            }

            if (!chapNhan)
            {
                bool okReject = _recruitmentDal.CapNhatTrangThaiLoiMoi(maLoiMoi, "tu_choi");
                return okReject
                    ? ServiceResultDTO.Ok("Đã từ chối lời mời.")
                    : ServiceResultDTO.Fail("Không thể cập nhật trạng thái lời mời.");
            }

            if (!_recruitmentDal.NguoiDungDangOTrangThaiFreeAgent(maNguoiDuocMoi))
            {
                return ServiceResultDTO.Fail("Bạn không còn ở trạng thái tự do để chấp nhận lời mời.");
            }

            int maNhom = Convert.ToInt32(loiMoi["ma_nhom"]);
            bool ok = _recruitmentDal.CapNhatTrangThaiLoiMoi(maLoiMoi, "chap_nhan");
            if (!ok)
            {
                return ServiceResultDTO.Fail("Không thể chấp nhận lời mời.");
            }

            _recruitmentDal.TiepNhanUngVienVaoNhom(maNhom, maNguoiDuocMoi);
            _recruitmentDal.TuChoiLoiMoiConLaiCuaUngVien(maNguoiDuocMoi, maLoiMoi);

            return ServiceResultDTO.Ok("Bạn đã chấp nhận lời mời và gia nhập nhóm.");
        }
    }
}
