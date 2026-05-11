using DAL;
using DTO;
using System.Collections.Generic;

namespace BUS
{
    public class YeuCauBUS
    {
        private readonly YeuCauDAL dal = new YeuCauDAL();

        public ApiResponseDTO LayDanhSachYeuCau(int maNguoiDung)
        {
            var ds = dal.LayDanhSachYeuCau(maNguoiDung);
            return new ApiResponseDTO { success = true, data = ds };
        }

        public ApiResponseDTO XuLyYeuCau(int maNguoiDung, XuLyYeuCauRequestDTO req)
        {
            return dal.XuLyYeuCau(maNguoiDung, req);
        }
    }
}
