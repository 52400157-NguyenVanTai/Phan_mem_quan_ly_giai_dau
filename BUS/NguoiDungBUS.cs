using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using DTO;

namespace BUS
{
    public class NguoiDungBUS
    {
        private NguoiDungDAL nguoiDungDAL = new NguoiDungDAL();

        public NguoiDungDTO KiemTraDangNhap(string tenDangNhap, string matKhau)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
            {
                return null;
            }

            return nguoiDungDAL.KiemTraDangNhap(tenDangNhap, matKhau);
        }
    }
}
