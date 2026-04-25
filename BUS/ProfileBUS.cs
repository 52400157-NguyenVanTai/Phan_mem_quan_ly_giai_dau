using System;
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
                return ServiceResultDTO.Fail("Mã trò chơi không hợp lệ.");
            }
            return ServiceResultDTO.Ok("OK", _profileDal.LayViTriTheoGame(maTroChoi));
        }

        public ServiceResultDTO TaoHoSo(HoSoInGameDTO dto)
        {
            if (dto == null || dto.MaNguoiDung <= 0 || dto.MaTroChoi <= 0 || dto.MaViTriSoTruong <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu hồ sơ in-game không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(dto.InGameId) || string.IsNullOrWhiteSpace(dto.InGameName))
            {
                return ServiceResultDTO.Fail("In-game ID và In-game Name là bắt buộc.");
            }

            if (_profileDal.DaTonTaiHoSo(dto.MaNguoiDung, dto.MaTroChoi))
            {
                return ServiceResultDTO.Fail("Bạn đã có hồ sơ cho tựa game này.");
            }

            int maHoSo = _profileDal.TaoHoSo(dto);
            return ServiceResultDTO.Ok("Tạo hồ sơ in-game thành công.", new { maHoSo });
        }
    }
}
