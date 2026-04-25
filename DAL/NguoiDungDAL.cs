using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace DAL
{
    public class NguoiDungDAL
    {
        public NguoiDungDTO KiemTraDangNhap(string tenDangNhap, string matKhau)
        {
            string query = "SELECT * FROM NGUOI_DUNG WHERE ten_dang_nhap = @TenDangNhap AND mat_khau_ma_hoa = @MatKhau";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@TenDangNhap", SqlDbType.NVarChar) { Value = tenDangNhap.Trim() },
                new SqlParameter("@MatKhau", SqlDbType.NVarChar) { Value = matKhau.Trim() }
            };

            DataTable dt = DataProvider.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new NguoiDungDTO
                {
                    MaNguoiDung = Convert.ToInt32(row["ma_nguoi_dung"]),
                    TenDangNhap = row["ten_dang_nhap"].ToString(),
                    Email = row["email"] != DBNull.Value ? row["email"].ToString() : "",
                    VaiTroHeThong = row["vai_tro_he_thong"].ToString(),
                };
            }
            return null;
        }
    }
}
