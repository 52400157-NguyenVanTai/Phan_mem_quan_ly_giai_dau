using System;
using System.Collections.Generic;
using System.Data;
using DAL;
using DTO;

namespace BUS
{
    public class PublicHomepageBUS
    {
        private readonly PublicHomepageDAL _dal = new PublicHomepageDAL();

        public ServiceResultDTO LayDuLieuTrangChu()
        {
            PublicHomepageDataDTO data = new PublicHomepageDataDTO
            {
                HeroStats = ParseHeroStats(_dal.LayHeroStats()),
                FeaturedTournaments = ParseTournamentCards(_dal.LayGiaiNoiBat(6)),
                SupportedGames = ParseGameCards(_dal.LayGameHoTro(8)),
                FeaturedTeams = ParseTeamCards(_dal.LayDoiNoiBat(6)),
                OpenRegistrationTournaments = ParseTournamentCards(_dal.LayGiaiMoDangKy(4)),
                UpcomingTournaments = ParseTournamentCards(_dal.LayGiaiSapDienRa(4)),
                RecentOrLiveMatches = ParseMatchCards(_dal.LayTranGanDayHoacDangDau(6))
            };

            return ServiceResultDTO.Ok("Lấy dữ liệu trang chủ công khai thành công.", data);
        }

        private static PublicHeroStatsDTO ParseHeroStats(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return new PublicHeroStatsDTO();
            }

            DataRow row = dt.Rows[0];
            return new PublicHeroStatsDTO
            {
                TongGiaiDangHoatDong = ToInt(row, "TongGiaiDangHoatDong"),
                TongDoiTuyenThamGia = ToInt(row, "TongDoiTuyenThamGia"),
                TongGameHoTro = ToInt(row, "TongGameHoTro"),
                TongLuotTheoDoi = ToInt(row, "TongLuotTheoDoi"),
                TongGiaiMoDangKy = ToInt(row, "TongGiaiMoDangKy"),
                TongGiaiSapDienRa = ToInt(row, "TongGiaiSapDienRa")
            };
        }

        private static List<PublicTournamentCardDTO> ParseTournamentCards(DataTable dt)
        {
            List<PublicTournamentCardDTO> list = new List<PublicTournamentCardDTO>();
            if (dt == null)
            {
                return list;
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new PublicTournamentCardDTO
                {
                    MaGiaiDau = ToInt(row, "ma_giai_dau"),
                    TenGiaiDau = ToStringSafe(row, "ten_giai_dau"),
                    MaTroChoi = ToNullableInt(row, "ma_tro_choi"),
                    TenGame = ToStringSafe(row, "ten_game"),
                    TrangThai = ToStringSafe(row, "trang_thai"),
                    NgayBatDau = ToNullableDateTime(row, "ngay_bat_dau"),
                    ThoiGianDongDangKy = ToNullableDateTime(row, "thoi_gian_dong_dang_ky"),
                    TongGiaiThuong = ToDecimal(row, "tong_giai_thuong"),
                    SoDoiDaDangKy = ToInt(row, "SoDoiDaDangKy"),
                    SoLuongDoiToiDa = ToInt(row, "so_luong_doi_toi_da")
                });
            }

            return list;
        }

        private static List<PublicGameCardDTO> ParseGameCards(DataTable dt)
        {
            List<PublicGameCardDTO> list = new List<PublicGameCardDTO>();
            if (dt == null)
            {
                return list;
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new PublicGameCardDTO
                {
                    MaTroChoi = ToInt(row, "ma_tro_choi"),
                    TenGame = ToStringSafe(row, "ten_game"),
                    TheLoai = ToStringSafe(row, "the_loai"),
                    SoGiaiDangVanHanh = ToInt(row, "SoGiaiDangVanHanh")
                });
            }

            return list;
        }

        private static List<PublicTeamCardDTO> ParseTeamCards(DataTable dt)
        {
            List<PublicTeamCardDTO> list = new List<PublicTeamCardDTO>();
            if (dt == null)
            {
                return list;
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new PublicTeamCardDTO
                {
                    MaDoi = ToInt(row, "ma_doi"),
                    TenDoi = ToStringSafe(row, "ten_doi"),
                    LogoUrl = ToStringSafe(row, "logo_url"),
                    Slogan = ToStringSafe(row, "slogan"),
                    SoThanhVienActive = ToInt(row, "SoThanhVienActive"),
                    SoGiaiDaThamGia = ToInt(row, "SoGiaiDaThamGia"),
                    DangTuyen = ToInt(row, "dang_tuyen") == 1
                });
            }

            return list;
        }

        private static List<PublicMatchCardDTO> ParseMatchCards(DataTable dt)
        {
            List<PublicMatchCardDTO> list = new List<PublicMatchCardDTO>();
            if (dt == null)
            {
                return list;
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new PublicMatchCardDTO
                {
                    MaTran = ToInt(row, "ma_tran"),
                    MaGiaiDau = ToInt(row, "ma_giai_dau"),
                    TenGiaiDau = ToStringSafe(row, "ten_giai_dau"),
                    TenGiaiDoan = ToStringSafe(row, "ten_giai_doan"),
                    TrangThai = ToStringSafe(row, "trang_thai"),
                    TenDoiA = ToStringSafe(row, "ten_doi_a"),
                    TenDoiB = ToStringSafe(row, "ten_doi_b"),
                    ThoiGianBatDau = ToNullableDateTime(row, "thoi_gian_bat_dau"),
                    ThoiGianKetThuc = ToNullableDateTime(row, "thoi_gian_ket_thuc")
                });
            }

            return list;
        }

        private static int ToInt(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? 0 : Convert.ToInt32(row[column]);
        }

        private static int? ToNullableInt(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? (int?)null : Convert.ToInt32(row[column]);
        }

        private static decimal ToDecimal(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? 0 : Convert.ToDecimal(row[column]);
        }

        private static DateTime? ToNullableDateTime(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row[column]);
        }

        private static string ToStringSafe(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? null : row[column].ToString();
        }
    }
}
