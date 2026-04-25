using System;
using System.Data;
using DAL;
using DTO;

namespace BUS
{
    public class TournamentRequestBUS
    {
        private readonly TournamentRequestDAL _dal = new TournamentRequestDAL();
        private readonly IdentityDAL _identityDal = new IdentityDAL();

        public ServiceResultDTO GuiYeuCauTaoGiai(YeuCauTaoGiaiDTO dto)
        {
            if (dto == null || dto.MaNguoiGui <= 0 || string.IsNullOrWhiteSpace(dto.TenGiaiDau) || string.IsNullOrWhiteSpace(dto.TheThuc))
            {
                return ServiceResultDTO.Fail("Dữ liệu yêu cầu tạo giải không hợp lệ.");
            }

            if (dto.NgayBatDau >= dto.NgayKetThuc)
            {
                return ServiceResultDTO.Fail("Ngày kết thúc phải lớn hơn ngày bắt đầu.");
            }

            int maYeuCau = _dal.TaoYeuCau(dto);
            return ServiceResultDTO.Ok("Gửi yêu cầu tạo giải thành công, vui lòng chờ admin duyệt.", new { maYeuCau });
        }

        public ServiceResultDTO DuyetYeuCau(int maAdmin, int maYeuCau)
        {
            if (maAdmin <= 0 || maYeuCau <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu duyệt yêu cầu không hợp lệ.");
            }

            NguoiDungDTO admin = _identityDal.LayTheoId(maAdmin);
            if (admin == null || !string.Equals(admin.VaiTroHeThong, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới có quyền duyệt yêu cầu tạo giải.");
            }

            DataRow row = _dal.LayYeuCauTheoId(maYeuCau);
            if (row == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy yêu cầu.");
            }

            if (!string.Equals(row["trang_thai"].ToString(), "cho_duyet", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Yêu cầu này đã được xử lý trước đó.");
            }

            int maGiaiDau = _dal.TaoGiaiDauTuYeuCau(row);
            _dal.CapNhatTrangThaiYeuCau(maYeuCau, "da_duyet", maAdmin, null);
            _dal.GanRoleBanToChuc(maGiaiDau, Convert.ToInt32(row["ma_nguoi_gui"]));

            return ServiceResultDTO.Ok("Duyệt yêu cầu thành công và đã tạo giải đấu.", new { maGiaiDau });
        }

        public ServiceResultDTO TuChoiYeuCau(int maAdmin, int maYeuCau, string lyDo)
        {
            if (maAdmin <= 0 || maYeuCau <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu từ chối yêu cầu không hợp lệ.");
            }

            NguoiDungDTO admin = _identityDal.LayTheoId(maAdmin);
            if (admin == null || !string.Equals(admin.VaiTroHeThong, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới có quyền từ chối yêu cầu tạo giải.");
            }

            DataRow row = _dal.LayYeuCauTheoId(maYeuCau);
            if (row == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy yêu cầu.");
            }

            if (!string.Equals(row["trang_thai"].ToString(), "cho_duyet", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Yêu cầu này đã được xử lý trước đó.");
            }

            bool ok = _dal.CapNhatTrangThaiYeuCau(maYeuCau, "tu_choi", maAdmin, string.IsNullOrWhiteSpace(lyDo) ? "Không phù hợp" : lyDo.Trim());
            return ok
                ? ServiceResultDTO.Ok("Đã từ chối yêu cầu. Dữ liệu giải đấu không được lưu.")
                : ServiceResultDTO.Fail("Không thể từ chối yêu cầu.");
        }
    }
}
