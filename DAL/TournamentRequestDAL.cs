using System;
using System.Data;
using System.Data.SqlClient;
using DTO;

namespace DAL
{
    public class TournamentRequestDAL
    {
        public int TaoYeuCau(YeuCauTaoGiaiDTO dto)
        {
            const string query = @"
INSERT INTO YEU_CAU_TAO_GIAI_DAU(ma_nguoi_gui, ten_giai_dau, ma_tro_choi, the_thuc, ngay_bat_dau, ngay_ket_thuc, tong_giai_thuong, trang_thai)
OUTPUT INSERTED.ma_yeu_cau
VALUES(@MaNguoiGui, @TenGiaiDau, @MaTroChoi, @TheThuc, @NgayBatDau, @NgayKetThuc, @TongGiaiThuong, 'cho_duyet');";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@MaNguoiGui", SqlDbType.Int){ Value = dto.MaNguoiGui },
                new SqlParameter("@TenGiaiDau", SqlDbType.NVarChar){ Value = dto.TenGiaiDau.Trim() },
                new SqlParameter("@MaTroChoi", SqlDbType.Int){ Value = (object)dto.MaTroChoi ?? DBNull.Value },
                new SqlParameter("@TheThuc", SqlDbType.NVarChar){ Value = dto.TheThuc.Trim() },
                new SqlParameter("@NgayBatDau", SqlDbType.DateTime){ Value = dto.NgayBatDau },
                new SqlParameter("@NgayKetThuc", SqlDbType.DateTime){ Value = dto.NgayKetThuc },
                new SqlParameter("@TongGiaiThuong", SqlDbType.Decimal){ Value = dto.TongGiaiThuong }
            });

            return Convert.ToInt32(result);
        }

        public DataRow LayYeuCauTheoId(int maYeuCau)
        {
            const string query = "SELECT * FROM YEU_CAU_TAO_GIAI_DAU WHERE ma_yeu_cau = @MaYeuCau";
            DataTable dt = DataProvider.ExecuteQuery(query, new[]
            {
                new SqlParameter("@MaYeuCau", SqlDbType.Int){ Value = maYeuCau }
            });
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        public bool CapNhatTrangThaiYeuCau(int maYeuCau, string trangThai, int maAdmin, string lyDo)
        {
            const string query = @"
UPDATE YEU_CAU_TAO_GIAI_DAU
SET trang_thai = @TrangThai,
    ma_admin_duyet = @MaAdmin,
    ly_do_huy = @LyDo,
    thoi_gian_duyet = GETDATE()
WHERE ma_yeu_cau = @MaYeuCau";

            int affected = DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@TrangThai", SqlDbType.NVarChar){ Value = trangThai },
                new SqlParameter("@MaAdmin", SqlDbType.Int){ Value = maAdmin },
                new SqlParameter("@LyDo", SqlDbType.NVarChar){ Value = (object)lyDo ?? DBNull.Value },
                new SqlParameter("@MaYeuCau", SqlDbType.Int){ Value = maYeuCau }
            });

            return affected > 0;
        }

        public int TaoGiaiDauTuYeuCau(DataRow row)
        {
            const string query = @"
INSERT INTO GIAI_DAU(ten_giai_dau, ma_tro_choi, the_thuc, ngay_bat_dau, ngay_ket_thuc, tong_giai_thuong, trang_thai, hien_thi_public, is_deleted)
OUTPUT INSERTED.ma_giai_dau
VALUES(@TenGiaiDau, @MaTroChoi, @TheThuc, @NgayBatDau, @NgayKetThuc, @TongGiaiThuong, 'sap_dien_ra', 1, 0);";

            object result = DataProvider.ExecuteScalar(query, new[]
            {
                new SqlParameter("@TenGiaiDau", SqlDbType.NVarChar){ Value = row["ten_giai_dau"].ToString() },
                new SqlParameter("@MaTroChoi", SqlDbType.Int){ Value = row["ma_tro_choi"] == DBNull.Value ? (object)DBNull.Value : Convert.ToInt32(row["ma_tro_choi"]) },
                new SqlParameter("@TheThuc", SqlDbType.NVarChar){ Value = row["the_thuc"].ToString() },
                new SqlParameter("@NgayBatDau", SqlDbType.DateTime){ Value = Convert.ToDateTime(row["ngay_bat_dau"]) },
                new SqlParameter("@NgayKetThuc", SqlDbType.DateTime){ Value = Convert.ToDateTime(row["ngay_ket_thuc"]) },
                new SqlParameter("@TongGiaiThuong", SqlDbType.Decimal){ Value = Convert.ToDecimal(row["tong_giai_thuong"]) }
            });

            return Convert.ToInt32(result);
        }

        public void GanRoleBanToChuc(int maGiaiDau, int maNguoiDung)
        {
            const string query = @"
INSERT INTO QUAN_TRI_GIAI_DAU(ma_giai_dau, ma_nguoi_dung, vai_tro_giai)
VALUES(@MaGiaiDau, @MaNguoiDung, 'ban_to_chuc');";

            DataProvider.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@MaGiaiDau", SqlDbType.Int){ Value = maGiaiDau },
                new SqlParameter("@MaNguoiDung", SqlDbType.Int){ Value = maNguoiDung }
            });
        }
    }
}
