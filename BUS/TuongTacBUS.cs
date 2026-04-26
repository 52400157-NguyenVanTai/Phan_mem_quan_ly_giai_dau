using System;
using DAL;
using DTO;

namespace BUS
{
    public class TuongTacBUS
    {
        private readonly TuongTacDAL _dal = new TuongTacDAL();
        private readonly IdentityDAL _identityDal = new IdentityDAL();
        private readonly AdminDAL _adminDal = new AdminDAL();

        // ---- Lấy trạng thái Like + Follow của user với giải + tổng hợp ----
        public ServiceResultDTO LayTrangThaiGiai(int maNguoiDung, int maGiaiDau)
        {
            if (maGiaiDau <= 0) return ServiceResultDTO.Fail("Mã giải đấu không hợp lệ.");

            var caNhan  = maNguoiDung > 0 ? _dal.LayTrangThai(maNguoiDung, maGiaiDau)
                                           : null;
            var tongHop = _dal.LayTongHop(maGiaiDau);

            return ServiceResultDTO.Ok("OK", new
            {
                caNhan,
                TongLike     = tongHop.ContainsKey("tong_like")     ? tongHop["tong_like"]     : 0,
                TongTheoDoi  = tongHop.ContainsKey("tong_theo_doi") ? tongHop["tong_theo_doi"] : 0
            });
        }

        // ---- Toggle Like ----
        public ServiceResultDTO ToggleLike(int maNguoiDung, int maGiaiDau)
        {
            if (maNguoiDung <= 0) return ServiceResultDTO.Fail("Bạn chưa đăng nhập.");
            if (maGiaiDau   <= 0) return ServiceResultDTO.Fail("Mã giải đấu không hợp lệ.");
            if (!GiaiTonTai(maGiaiDau)) return ServiceResultDTO.Fail("Giải đấu không tồn tại.");

            bool newState = _dal.ToggleLike(maNguoiDung, maGiaiDau);
            var tongHop   = _dal.LayTongHop(maGiaiDau);

            return ServiceResultDTO.Ok(
                newState ? "Đã thích giải đấu." : "Đã bỏ thích giải đấu.",
                new
                {
                    DaLike      = newState,
                    TongLike    = tongHop.ContainsKey("tong_like") ? tongHop["tong_like"] : 0,
                    TongTheoDoi = tongHop.ContainsKey("tong_theo_doi") ? tongHop["tong_theo_doi"] : 0
                });
        }

        // ---- Toggle Follow ----
        public ServiceResultDTO ToggleFollow(int maNguoiDung, int maGiaiDau)
        {
            if (maNguoiDung <= 0) return ServiceResultDTO.Fail("Bạn chưa đăng nhập.");
            if (maGiaiDau   <= 0) return ServiceResultDTO.Fail("Mã giải đấu không hợp lệ.");
            if (!GiaiTonTai(maGiaiDau)) return ServiceResultDTO.Fail("Giải đấu không tồn tại.");

            bool newState = _dal.ToggleFollow(maNguoiDung, maGiaiDau);
            var tongHop   = _dal.LayTongHop(maGiaiDau);

            return ServiceResultDTO.Ok(
                newState ? "Đang theo dõi giải đấu." : "Đã bỏ theo dõi giải đấu.",
                new
                {
                    DangTheoDoi = newState,
                    TongLike    = tongHop.ContainsKey("tong_like")     ? tongHop["tong_like"]     : 0,
                    TongTheoDoi = tongHop.ContainsKey("tong_theo_doi") ? tongHop["tong_theo_doi"] : 0
                });
        }

        // ---- Giải đang theo dõi ----
        public ServiceResultDTO LayGiaiDangTheoDoi(int maNguoiDung)
        {
            if (maNguoiDung <= 0) return ServiceResultDTO.Fail("Bạn chưa đăng nhập.");
            var list = _dal.LayGiaiDangTheoDoi(maNguoiDung);
            return ServiceResultDTO.Ok("Lấy danh sách theo dõi thành công.", list);
        }

        private bool GiaiTonTai(int maGiaiDau)
        {
            return _adminDal.GiaiTonTai(maGiaiDau);
        }
    }
}
