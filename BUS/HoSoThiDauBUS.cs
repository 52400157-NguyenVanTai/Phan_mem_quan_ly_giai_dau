using DAL;
using DTO;
using System.Collections.Generic;

namespace BUS
{
    public class HoSoThiDauBUS
    {
        private readonly HoSoThiDauDAL hoSoThiDauDAL = new HoSoThiDauDAL();

        public ApiResponseDTO LayDanhSachTroChoi()
        {
            List<TroChoiDTO> items = hoSoThiDauDAL.GetTroChoi();
            return ThanhCong("Lấy danh sách trò chơi thành công.", items);
        }

        public ApiResponseDTO LayDanhSachViTri(int? maTroChoi, string loaiViTri)
        {
            List<ViTriDTO> items = hoSoThiDauDAL.GetViTri(maTroChoi, loaiViTri);
            return ThanhCong("Lấy danh sách vị trí thành công.", items);
        }

        public ApiResponseDTO LayHoSo(int maNguoiDung)
        {
            HoSoThiDauDTO hoSo = hoSoThiDauDAL.GetByUserId(maNguoiDung);
            return ThanhCong("Lấy hồ sơ thi đấu thành công.", hoSo);
        }

        public ApiResponseDTO LayDanhSachHoSo(int maNguoiDung)
        {
            List<HoSoThiDauDTO> items = hoSoThiDauDAL.GetAllByUserId(maNguoiDung);
            return ThanhCong("Lay danh sach ho so thi dau thanh cong.", items);
        }

        public ApiResponseDTO LuuHoSo(int maNguoiDung, HoSoThiDauRequestDTO request)
        {
            ApiResponseDTO validation = ValidateHoSo(request);
            if (!validation.success)
            {
                return validation;
            }

            hoSoThiDauDAL.Save(maNguoiDung, request);
            HoSoThiDauDTO hoSo = hoSoThiDauDAL.GetByUserIdAndGame(maNguoiDung, request.ma_tro_choi);
            return ThanhCong("Lưu hồ sơ thi đấu thành công.", hoSo);
        }

        public ApiResponseDTO XoaHoSo(int maNguoiDung, HoSoThiDauDeleteRequestDTO request)
        {
            if (request == null || request.ma_tro_choi <= 0)
            {
                return Loi("Vui long chon tro choi can xoa ho so.");
            }

            hoSoThiDauDAL.Delete(maNguoiDung, request.ma_tro_choi);
            return ThanhCong("Xóa hồ sơ thi đấu thành công.", null);
        }

        private ApiResponseDTO ValidateHoSo(HoSoThiDauRequestDTO request)
        {
            if (request == null)
            {
                return Loi("Dữ liệu hồ sơ thi đấu không hợp lệ.");
            }

            if (request.ma_tro_choi <= 0)
            {
                return Loi("Vui lòng chọn trò chơi.");
            }

            if (string.IsNullOrWhiteSpace(request.in_game_id))
            {
                return Loi("Vui lòng nhập ID trong game.");
            }

            if (string.IsNullOrWhiteSpace(request.in_game_name))
            {
                return Loi("Vui lòng nhập tên trong game.");
            }

            if (string.IsNullOrWhiteSpace(request.loai_vi_tri))
            {
                return Loi("Vui lòng chọn loại vị trí.");
            }

            if (request.loai_vi_tri != "TuyenThu" && request.loai_vi_tri != "HuanLuyen")
            {
                return Loi("Loại vị trí không hợp lệ.");
            }

            if (!request.ma_vi_tri_so_truong.HasValue || request.ma_vi_tri_so_truong.Value <= 0)
            {
                return Loi("Vui lòng chọn vị trí.");
            }

            return ThanhCong("Dữ liệu hợp lệ.", null);
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
