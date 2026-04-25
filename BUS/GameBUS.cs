using System;
using DAL;
using DTO;

namespace BUS
{
    /// <summary>
    /// Business Logic cho CRUD Game (TRO_CHOI) — Module 2 Trụ cột 5.
    /// Luật chặt: Không được XÓA game đang có giải đấu. Chỉ được ẨN (is_active = 0).
    /// </summary>
    public class GameBUS
    {
        private readonly GameDAL _dal = new GameDAL();
        private readonly IdentityDAL _identityDal = new IdentityDAL();

        public ServiceResultDTO LayDanhSachGame(int maNguoiDung, bool baoCaInactive = false)
        {
            // Chỉ admin mới thấy game đã ẩn
            if (baoCaInactive && !LaAdmin(maNguoiDung))
            {
                baoCaInactive = false;
            }

            var list = _dal.LayTatCaGame(baoCaInactive);
            return ServiceResultDTO.Ok("Lấy danh sách game thành công.", list);
        }

        public ServiceResultDTO ThemGame(int maAdmin, string tenGame, string theLoai)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới được thêm game.");
            }

            if (string.IsNullOrWhiteSpace(tenGame))
            {
                return ServiceResultDTO.Fail("Tên game không được để trống.");
            }

            string[] theLoaiHopLe = { "MOBA", "FPS", "BATTLEROYALE" };
            if (string.IsNullOrWhiteSpace(theLoai) || Array.IndexOf(theLoaiHopLe, theLoai.Trim().ToUpper()) < 0)
            {
                return ServiceResultDTO.Fail("Thể loại game không hợp lệ. Chỉ chấp nhận: MOBA, FPS, BATTLEROYALE.");
            }

            if (_dal.TenGameDaTonTai(tenGame))
            {
                return ServiceResultDTO.Fail("Tên game \"" + tenGame.Trim() + "\" đã tồn tại trong hệ thống.");
            }

            int maGame = _dal.ThemGame(tenGame.Trim(), theLoai.Trim().ToUpper());
            return ServiceResultDTO.Ok("Thêm game thành công.", new { maGame, tenGame = tenGame.Trim(), theLoai = theLoai.Trim().ToUpper() });
        }

        public ServiceResultDTO SuaGame(int maAdmin, int maGame, string tenGame, string theLoai)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới được sửa game.");
            }

            if (!_dal.GameTonTai(maGame))
            {
                return ServiceResultDTO.Fail("Không tìm thấy game cần sửa.");
            }

            if (string.IsNullOrWhiteSpace(tenGame))
            {
                return ServiceResultDTO.Fail("Tên game không được để trống.");
            }

            string[] theLoaiHopLe = { "MOBA", "FPS", "BATTLEROYALE" };
            if (string.IsNullOrWhiteSpace(theLoai) || Array.IndexOf(theLoaiHopLe, theLoai.Trim().ToUpper()) < 0)
            {
                return ServiceResultDTO.Fail("Thể loại game không hợp lệ. Chỉ chấp nhận: MOBA, FPS, BATTLEROYALE.");
            }

            if (_dal.TenGameDaTonTai(tenGame, maGame))
            {
                return ServiceResultDTO.Fail("Tên game \"" + tenGame.Trim() + "\" đã tồn tại ở game khác.");
            }

            bool ok = _dal.SuaGame(maGame, tenGame.Trim(), theLoai.Trim().ToUpper());
            return ok
                ? ServiceResultDTO.Ok("Cập nhật game thành công.", new { maGame })
                : ServiceResultDTO.Fail("Không thể cập nhật game.");
        }

        /// <summary>
        /// Ẩn game. Nếu game đang có giải đấu đang hoạt động → từ chối.
        /// </summary>
        public ServiceResultDTO AnGame(int maAdmin, int maGame)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới được ẩn game.");
            }

            if (!_dal.GameTonTai(maGame))
            {
                return ServiceResultDTO.Fail("Không tìm thấy game.");
            }

            if (_dal.GameDangCoGiaiDau(maGame))
            {
                return ServiceResultDTO.Fail(
                    "Không thể ẩn game này vì đang có giải đấu hoạt động dùng game này. " +
                    "Hãy kết thúc hoặc khóa tất cả giải của game trước.");
            }

            bool ok = _dal.AnGame(maGame);
            return ok
                ? ServiceResultDTO.Ok("Đã ẩn game thành công. Game sẽ không xuất hiện trong danh sách công khai.", new { maGame })
                : ServiceResultDTO.Fail("Không thể ẩn game.");
        }

        public ServiceResultDTO KichHoatGame(int maAdmin, int maGame)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới được kích hoạt game.");
            }

            if (!_dal.GameTonTai(maGame))
            {
                return ServiceResultDTO.Fail("Không tìm thấy game.");
            }

            bool ok = _dal.KichHoatGame(maGame);
            return ok
                ? ServiceResultDTO.Ok("Đã kích hoạt game trở lại.", new { maGame })
                : ServiceResultDTO.Fail("Không thể kích hoạt game.");
        }

        private bool LaAdmin(int maNguoiDung)
        {
            NguoiDungDTO user = _identityDal.LayTheoId(maNguoiDung);
            return user != null && string.Equals(user.VaiTroHeThong, "admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
