using DAL;
using DTO;

namespace BUS
{
    public class ProfileBUS
    {
        private readonly ProfileDAL _profileDal = new ProfileDAL();

        public ServiceResultDTO LayDanhSachTroChoi()
        {
            return ServiceResultDTO.Ok("OK", _profileDal.LayDanhSachTroChoi());
        }

        public ServiceResultDTO LayViTriTheoGame(int maTroChoi)
        {
            if (maTroChoi <= 0)
            {
                return ServiceResultDTO.Fail("Ma tro choi khong hop le.");
            }

            return ServiceResultDTO.Ok("OK", _profileDal.LayViTriTheoGame(maTroChoi));
        }

        public ServiceResultDTO LayHoSo(int maNguoiDung, int maTroChoi)
        {
            if (maNguoiDung <= 0 || maTroChoi <= 0)
            {
                return ServiceResultDTO.Fail("Du lieu truy van ho so khong hop le.");
            }

            return ServiceResultDTO.Ok("OK", _profileDal.LayHoSo(maNguoiDung, maTroChoi));
        }

        public ServiceResultDTO TaoHoSo(HoSoInGameDTO dto)
        {
            if (dto == null || dto.MaNguoiDung <= 0 || dto.MaTroChoi <= 0 || dto.MaViTriSoTruong <= 0)
            {
                return ServiceResultDTO.Fail("Du lieu ho so in-game khong hop le.");
            }

            if (string.IsNullOrWhiteSpace(dto.InGameId) || string.IsNullOrWhiteSpace(dto.InGameName))
            {
                return ServiceResultDTO.Fail("In-game ID va In-game Name la bat buoc.");
            }

            if (_profileDal.DaTonTaiHoSo(dto.MaNguoiDung, dto.MaTroChoi))
            {
                bool updated = _profileDal.CapNhatHoSo(dto);
                if (!updated)
                {
                    return ServiceResultDTO.Fail("Khong the cap nhat ho so in-game.");
                }

                return ServiceResultDTO.Ok("Cap nhat ho so in-game thanh cong.", _profileDal.LayHoSo(dto.MaNguoiDung, dto.MaTroChoi));
            }

            int maHoSo = _profileDal.TaoHoSo(dto);
            return ServiceResultDTO.Ok("Tao ho so in-game thanh cong.", _profileDal.LayHoSo(dto.MaNguoiDung, dto.MaTroChoi) ?? (object)new { maHoSo });
        }
    }
}
