using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using DAL;
using DTO;

namespace BUS
{
    public class MatchmakingBUS
    {
        private readonly MatchmakingDAL _dal = new MatchmakingDAL();
        private readonly TournamentBuilderDAL _tournamentDal = new TournamentBuilderDAL();

        public ServiceResultDTO TaoLichThiDau(int maNguoiDung, TaoLichGiaiDoanDTO dto)
        {
            if (maNguoiDung <= 0)
            {
                return ServiceResultDTO.Fail("Bạn cần đăng nhập để tạo lịch thi đấu.");
            }

            if (dto == null || dto.MaGiaiDau <= 0 || dto.MaGiaiDoan <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu tạo lịch không hợp lệ.");
            }

            if (!_dal.CoQuyenQuanLyGiai(dto.MaGiaiDau, maNguoiDung))
            {
                return ServiceResultDTO.Fail("Bạn không có quyền tạo lịch cho giải đấu này.");
            }

            if (!_dal.DaDongDangKyHoacDaKhoiTranh(dto.MaGiaiDau))
            {
                return ServiceResultDTO.Fail("Chỉ được tạo lịch sau khi đã đóng đăng ký hoặc giải đã sang giai đoạn thi đấu.");
            }

            return TaoLichThiDau(dto);
        }

        public ServiceResultDTO TaoLichThiDau(TaoLichGiaiDoanDTO dto)
        {
            if (dto == null || dto.MaGiaiDau <= 0 || dto.MaGiaiDoan <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu tạo lịch không hợp lệ.");
            }

            DataRow giaiDoan = _dal.LayGiaiDoanTheoId(dto.MaGiaiDoan);
            if (giaiDoan == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giai đoạn.");
            }

            if (Convert.ToInt32(giaiDoan["ma_giai_dau"]) != dto.MaGiaiDau)
            {
                return ServiceResultDTO.Fail("Giai đoạn không thuộc giải đấu đã chọn.");
            }

            if (_dal.DaCoTranTrongGiaiDoan(dto.MaGiaiDoan))
            {
                return ServiceResultDTO.Fail("Giai đoạn này đã được tạo lịch trước đó.");
            }

            int thuTu = Convert.ToInt32(giaiDoan["thu_tu"]);
            if (!_dal.GiaiDoanTruocDaKetThuc(dto.MaGiaiDau, thuTu))
            {
                return ServiceResultDTO.Fail("Chỉ được tạo lịch giai đoạn N+1 khi giai đoạn N đã kết thúc hoàn toàn.");
            }

            string theThuc = giaiDoan["the_thuc"].ToString();
            List<int> dsNhom = _dal.LayDanhSachNhomChoGiaiDoan(dto.MaGiaiDau, thuTu);
            if (dsNhom.Count < 2)
            {
                return ServiceResultDTO.Fail("Không đủ số đội để tạo lịch.");
            }

            int tongTran = 0;
            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        _dal.TaoBangXepHangGiaiDoanNeuChuaCo(conn, tran, dto.MaGiaiDoan, dto.MaGiaiDau, dsNhom);

                        switch (theThuc)
                        {
                            case "loai_truc_tiep":
                                tongTran = TaoSingleElimination(conn, tran, dto.MaGiaiDau, dto.MaGiaiDoan, dsNhom, dto.DungHatGiong);
                                break;
                            case "nhanh_thang_nhanh_thua":
                                tongTran = TaoDoubleElimination(conn, tran, dto.MaGiaiDau, dto.MaGiaiDoan, dsNhom, dto.DungHatGiong);
                                break;
                            case "vong_tron":
                                tongTran = TaoRoundRobin(conn, tran, dto.MaGiaiDau, dto.MaGiaiDoan, dsNhom);
                                break;
                            case "league_bang_cheo":
                                tongTran = TaoLeagueBangCheo(conn, tran, dto.MaGiaiDau, dto.MaGiaiDoan, dsNhom);
                                break;
                            case "thuy_si":
                                tongTran = TaoSwissRound1(conn, tran, dto.MaGiaiDau, dto.MaGiaiDoan, dsNhom);
                                break;
                            case "champion_rush":
                                tongTran = TaoChampionRushGame1(conn, tran, dto.MaGiaiDau, dto.MaGiaiDoan, dsNhom);
                                break;
                            default:
                                throw new InvalidOperationException("Thể thức chưa được hỗ trợ: " + theThuc);
                        }

                        _dal.CapNhatTrangThaiGiaiDoan(conn, tran, dto.MaGiaiDoan, "dang_dien_ra");
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        return ServiceResultDTO.Fail("Tạo lịch thất bại, đã rollback toàn bộ. Chi tiết: " + ex.Message);
                    }
                }
            }

            return ServiceResultDTO.Ok("Tạo lịch thi đấu thành công.", new { tongTran, theThuc });
        }

        public ServiceResultDTO LayTranTheoGiaiDoan(int maGiaiDoan)
        {
            return ServiceResultDTO.Ok("Lấy danh sách trận đấu thành công.", _dal.LayTranTheoGiaiDoan(maGiaiDoan));
        }

        public ServiceResultDTO TaoVongTiepTheo(int maGiaiDau, int maGiaiDoan)
        {
            DataRow giaiDoan = _dal.LayGiaiDoanTheoId(maGiaiDoan);
            if (giaiDoan == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy giai đoạn.");
            }

            if (Convert.ToInt32(giaiDoan["ma_giai_dau"]) != maGiaiDau)
            {
                return ServiceResultDTO.Fail("Giai đoạn không thuộc giải đấu đã chọn.");
            }

            string theThuc = giaiDoan["the_thuc"].ToString();
            if (!string.Equals(theThuc, "thuy_si", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(theThuc, "champion_rush", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResultDTO.Fail("Chỉ thể thức Thụy Sĩ/Champion Rush mới sinh lịch cuốn chiếu theo vòng.");
            }

            int soVongHienTai = _dal.LaySoVongLonNhat(maGiaiDoan);
            if (soVongHienTai <= 0)
            {
                return ServiceResultDTO.Fail("Chưa có vòng nào để sinh tiếp.");
            }

            if (!_dal.TatCaTranTrongVongDaKetThuc(maGiaiDoan, soVongHienTai))
            {
                return ServiceResultDTO.Fail("Vòng hiện tại chưa kết thúc toàn bộ, chưa thể sinh vòng mới.");
            }

            if (string.Equals(theThuc, "thuy_si", StringComparison.OrdinalIgnoreCase))
            {
                _dal.CapNhatTrangThaiThamGiaSwiss(maGiaiDau, maGiaiDoan);
                return TaoSwissRoundTiepTheo(maGiaiDau, maGiaiDoan, soVongHienTai + 1);
            }

            return TaoChampionRushGameTiepTheo(maGiaiDau, maGiaiDoan, soVongHienTai + 1);
        }

        public ServiceResultDTO TongQuanCongKhai(int maGiaiDau)
        {
            DataRow row = _dal.LayTongQuanPublicGiai(maGiaiDau);
            if (row == null)
            {
                return ServiceResultDTO.Fail("Giải đấu không tồn tại hoặc chưa công khai.");
            }

            DateTime? thoiGianDongDangKy = row["thoi_gian_dong_dang_ky"] == DBNull.Value
                ? (DateTime?)null
                : Convert.ToDateTime(row["thoi_gian_dong_dang_ky"]);
            string trangThai = row["trang_thai"].ToString();
            bool dangMoDangKy = row.Table.Columns.Contains("dang_mo_dang_ky")
                && row["dang_mo_dang_ky"] != DBNull.Value
                && Convert.ToBoolean(row["dang_mo_dang_ky"]);
            bool dangChoChotDanhSach = string.Equals(trangThai, "chuan_bi_dien_ra", StringComparison.OrdinalIgnoreCase)
                && dangMoDangKy
                && (!thoiGianDongDangKy.HasValue || thoiGianDongDangKy.Value > DateTime.Now);

            PublicTournamentOverviewDTO dto = new PublicTournamentOverviewDTO
            {
                MaGiaiDau = Convert.ToInt32(row["ma_giai_dau"]),
                TenGiaiDau = row["ten_giai_dau"].ToString(),
                TenGame = row["ten_game"] == DBNull.Value ? "Chưa chọn game" : row["ten_game"].ToString(),
                TenBanToChuc = row["ten_ban_to_chuc"] == DBNull.Value ? "Ban tổ chức" : row["ten_ban_to_chuc"].ToString(),
                BannerUrl = row["banner_url"] == DBNull.Value ? null : row["banner_url"].ToString(),
                MoTa = row["mo_ta"] == DBNull.Value ? null : row["mo_ta"].ToString(),
                LuatGiai = row["luat_giai"] == DBNull.Value ? null : row["luat_giai"].ToString(),
                TongGiaiThuong = row["tong_giai_thuong"] == DBNull.Value ? 0 : Convert.ToDecimal(row["tong_giai_thuong"]),
                TrangThai = trangThai,
                ThoiGianMoDangKy = row["thoi_gian_mo_dang_ky"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["thoi_gian_mo_dang_ky"]),
                ThoiGianDongDangKy = thoiGianDongDangKy,
                NgayBatDau = row["ngay_bat_dau"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ngay_bat_dau"]),
                NgayKetThuc = row["ngay_ket_thuc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ngay_ket_thuc"]),
                SoDoiToiDa = row["so_doi_toi_da"] == DBNull.Value ? 0 : Convert.ToInt32(row["so_doi_toi_da"]),
                SoDoiDaDangKy = row["so_doi_da_dang_ky"] == DBNull.Value ? 0 : Convert.ToInt32(row["so_doi_da_dang_ky"]),
                DangChoChotDanhSach = dangChoChotDanhSach,
                GiaiDoanDangDienRa = row["giai_doan_dang_dien_ra"] == DBNull.Value ? null : row["giai_doan_dang_dien_ra"].ToString(),
                DoiThamGia = _dal.LayDoiThamGiaPublic(maGiaiDau),
                Timeline = TaoTimelineVongDoi(trangThai, dangChoChotDanhSach),
                GiaiDoan = _tournamentDal.LayDanhSachGiaiDoan(maGiaiDau)
            };

            return ServiceResultDTO.Ok("Lấy dữ liệu cổng thông tin giải đấu thành công.", dto);
        }

        private static List<PublicTournamentTimelineItemDTO> TaoTimelineVongDoi(string trangThaiRaw, bool dangChoChotDanhSach)
        {
            string trangThai = string.IsNullOrWhiteSpace(trangThaiRaw)
                ? string.Empty
                : trangThaiRaw.Trim().ToLowerInvariant();

            int step = 0;
            if (trangThai == "cho_xet_duyet")
            {
                step = 1;
            }
            else if (trangThai == "chuan_bi_dien_ra")
            {
                step = dangChoChotDanhSach ? 2 : 3;
            }
            else if (trangThai == "dang_dien_ra")
            {
                step = 4;
            }
            else if (trangThai == "ket_thuc")
            {
                step = 5;
            }

            List<PublicTournamentTimelineItemDTO> list = new List<PublicTournamentTimelineItemDTO>
            {
                new PublicTournamentTimelineItemDTO{ Key = "cho_duyet", Label = "Chờ duyệt" },
                new PublicTournamentTimelineItemDTO{ Key = "mo_dang_ky", Label = "Mở đăng ký" },
                new PublicTournamentTimelineItemDTO{ Key = "dong_dang_ky", Label = "Đóng đăng ký" },
                new PublicTournamentTimelineItemDTO{ Key = "dang_thi_dau", Label = "Đang thi đấu" },
                new PublicTournamentTimelineItemDTO{ Key = "ket_thuc", Label = "Đã kết thúc" }
            };

            for (int i = 0; i < list.Count; i++)
            {
                int idx = i + 1;
                list[i].IsDone = step > idx;
                list[i].IsCurrent = step == idx;
            }

            return list;
        }

        public ServiceResultDTO BangXepHangTheoGiaiDoan(int maGiaiDoan)
        {
            return ServiceResultDTO.Ok("Lấy bảng xếp hạng thành công.", _dal.LayBangXepHangTheoGiaiDoan(maGiaiDoan));
        }

        public ServiceResultDTO VinhDanhTuDong(int maGiaiDau)
        {
            if (maGiaiDau <= 0)
            {
                return ServiceResultDTO.Fail("Mã giải đấu không hợp lệ.");
            }

            var data = new
            {
                MvpTranGanNhat = _dal.LayMvpTheoTranGanNhat(maGiaiDau, 10),
                MvpGiai = _dal.LayMvpGiai(maGiaiDau),
                DoiHinhTieuBieu = _dal.LayDoiHinhTieuBieu(maGiaiDau),
                BanHuanLuyenVoDich = _dal.LayBanHuanLuyenVoDich(maGiaiDau)
            };

            return ServiceResultDTO.Ok("Lấy dữ liệu vinh danh tự động thành công.", data);
        }

        public ServiceResultDTO LiveSnapshot(int maGiaiDau, int maGiaiDoan)
        {
            if (maGiaiDau <= 0 || maGiaiDoan <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu live snapshot không hợp lệ.");
            }

            DataRow gd = _dal.LayGiaiDoanTheoId(maGiaiDoan);
            if (gd == null || Convert.ToInt32(gd["ma_giai_dau"]) != maGiaiDau)
            {
                return ServiceResultDTO.Fail("Giai đoạn không thuộc giải đấu đã chọn.");
            }

            var data = new
            {
                BangXepHang = _dal.LayBangXepHangTheoGiaiDoan(maGiaiDoan),
                TranDau = _dal.LayTranTheoGiaiDoan(maGiaiDoan),
                VinhDanh = new
                {
                    MvpGiai = _dal.LayMvpGiai(maGiaiDau),
                    DoiHinhTieuBieu = _dal.LayDoiHinhTieuBieu(maGiaiDau),
                    BanHuanLuyenVoDich = _dal.LayBanHuanLuyenVoDich(maGiaiDau)
                },
                ServerTime = DateTime.Now
            };

            return ServiceResultDTO.Ok("Lấy dữ liệu Live Engine thành công.", data);
        }

        private int TaoSingleElimination(SqlConnection conn, SqlTransaction tran, int maGiaiDau, int maGiaiDoan, List<int> teams, bool dungHatGiong)
        {
            List<int> ds = new List<int>(teams);
            if (!dungHatGiong)
            {
                TronNgauNhien(ds);
            }

            int p = 1;
            while (p < ds.Count) p *= 2;
            int soBye = p - ds.Count;
            for (int i = 0; i < soBye; i++) ds.Add(-1);

            int tongTran = 0;
            int vong = 1;
            List<int> currentRound = new List<int>();
            for (int i = 0; i < ds.Count; i += 2)
            {
                MatchNodeDTO node = TaoMatchNode(maGiaiDau, maGiaiDoan, vong, "upper", "BO3");
                int maTran = _dal.TaoTran(conn, tran, node);
                tongTran++;

                if (ds[i] > 0) _dal.GanNhomVaoTran(conn, tran, maTran, ds[i]);
                if (ds[i + 1] > 0) _dal.GanNhomVaoTran(conn, tran, maTran, ds[i + 1]);

                currentRound.Add(maTran);
            }

            while (currentRound.Count > 1)
            {
                vong++;
                List<int> nextRound = new List<int>();
                for (int i = 0; i < currentRound.Count; i += 2)
                {
                    MatchNodeDTO nextNode = TaoMatchNode(maGiaiDau, maGiaiDoan, vong, "upper", "BO5");
                    int maTranTiep = _dal.TaoTran(conn, tran, nextNode);
                    tongTran++;

                    nextRound.Add(maTranTiep);
                    CapNhatRouteTran(conn, tran, currentRound[i], maTranTiep, null);
                    CapNhatRouteTran(conn, tran, currentRound[i + 1], maTranTiep, null);
                }
                currentRound = nextRound;
            }

            return tongTran;
        }

        private int TaoDoubleElimination(SqlConnection conn, SqlTransaction tran, int maGiaiDau, int maGiaiDoan, List<int> teams, bool dungHatGiong)
        {
            int tongTran = TaoSingleElimination(conn, tran, maGiaiDau, maGiaiDoan, teams, dungHatGiong);

            List<int> ds = new List<int>(teams);
            if (!dungHatGiong)
            {
                TronNgauNhien(ds);
            }

            int lowerRound = 1;
            for (int i = 0; i + 1 < ds.Count; i += 2)
            {
                MatchNodeDTO lower = TaoMatchNode(maGiaiDau, maGiaiDoan, lowerRound, "lower", "BO3");
                int maTran = _dal.TaoTran(conn, tran, lower);
                tongTran++;
                _dal.GanNhomVaoTran(conn, tran, maTran, ds[i]);
                _dal.GanNhomVaoTran(conn, tran, maTran, ds[i + 1]);
            }

            MatchNodeDTO grandFinal = TaoMatchNode(maGiaiDau, maGiaiDoan, 99, "grand_final", "BO5");
            _dal.TaoTran(conn, tran, grandFinal);
            tongTran++;

            return tongTran;
        }

        private int TaoRoundRobin(SqlConnection conn, SqlTransaction tran, int maGiaiDau, int maGiaiDoan, List<int> teams)
        {
            List<int> ds = new List<int>(teams);
            if (ds.Count % 2 != 0) ds.Add(-1);

            int n = ds.Count;
            int rounds = n - 1;
            int tongTran = 0;

            List<int> rotation = new List<int>(ds);
            for (int round = 1; round <= rounds; round++)
            {
                for (int i = 0; i < n / 2; i++)
                {
                    int a = rotation[i];
                    int b = rotation[n - 1 - i];
                    if (a <= 0 || b <= 0) continue;

                    int maTran = _dal.TaoTran(conn, tran, TaoMatchNode(maGiaiDau, maGiaiDoan, round, "round_robin", "BO1"));
                    _dal.GanNhomVaoTran(conn, tran, maTran, a);
                    _dal.GanNhomVaoTran(conn, tran, maTran, b);
                    tongTran++;
                }

                int first = rotation[0];
                rotation.RemoveAt(0);
                int last = rotation[rotation.Count - 1];
                rotation.RemoveAt(rotation.Count - 1);
                rotation.Insert(0, first);
                rotation.Insert(1, last);
            }

            return tongTran;
        }

        private int TaoLeagueBangCheo(SqlConnection conn, SqlTransaction tran, int maGiaiDau, int maGiaiDoan, List<int> teams)
        {
            if (teams.Count < 4)
            {
                return TaoRoundRobin(conn, tran, maGiaiDau, maGiaiDoan, teams);
            }

            List<int> ds = new List<int>(teams);
            TronNgauNhien(ds);

            int phanBa = Math.Max(1, ds.Count / 3);
            List<int> a = ds.GetRange(0, phanBa);
            List<int> b = ds.GetRange(phanBa, Math.Min(phanBa, ds.Count - phanBa));
            List<int> c = ds.GetRange(phanBa + b.Count, ds.Count - phanBa - b.Count);

            int tongTran = 0;
            tongTran += TaoTranBR(conn, tran, maGiaiDau, maGiaiDoan, 1, HopNhom(a, b));
            tongTran += TaoTranBR(conn, tran, maGiaiDau, maGiaiDoan, 2, HopNhom(b, c));
            tongTran += TaoTranBR(conn, tran, maGiaiDau, maGiaiDoan, 3, HopNhom(a, c));
            return tongTran;
        }

        private int TaoSwissRound1(SqlConnection conn, SqlTransaction tran, int maGiaiDau, int maGiaiDoan, List<int> teams)
        {
            List<int> ds = new List<int>(teams);
            TronNgauNhien(ds);
            if (ds.Count % 2 != 0) ds.Add(-1);

            int tongTran = 0;
            for (int i = 0; i < ds.Count; i += 2)
            {
                if (ds[i] <= 0 || ds[i + 1] <= 0) continue;
                int maTran = _dal.TaoTran(conn, tran, TaoMatchNode(maGiaiDau, maGiaiDoan, 1, "swiss", "BO1"));
                _dal.GanNhomVaoTran(conn, tran, maTran, ds[i]);
                _dal.GanNhomVaoTran(conn, tran, maTran, ds[i + 1]);
                tongTran++;
            }
            return tongTran;
        }

        private int TaoChampionRushGame1(SqlConnection conn, SqlTransaction tran, int maGiaiDau, int maGiaiDoan, List<int> teams)
        {
            int maTran = _dal.TaoTran(conn, tran, TaoMatchNode(maGiaiDau, maGiaiDoan, 1, "champion_rush", "BO1"));
            foreach (int maNhom in teams)
            {
                _dal.GanNhomVaoTran(conn, tran, maTran, maNhom);
            }
            return 1;
        }

        private ServiceResultDTO TaoSwissRoundTiepTheo(int maGiaiDau, int maGiaiDoan, int soVongMoi)
        {
            List<Dictionary<string, object>> bxh = _dal.LayBangXepHangRaw(maGiaiDoan);
            List<int> doiConLai = bxh
                .Where(x => ParseInt(x, "so_tran_thang") < 3 && ParseInt(x, "so_tran_thua") < 3)
                .Select(x => ParseInt(x, "ma_nhom"))
                .Where(x => x > 0)
                .ToList();

            if (doiConLai.Count < 2)
            {
                _dal.KetThucGiaiDoan(maGiaiDoan);
                return ServiceResultDTO.Ok("Không còn đủ đội để ghép cặp, giai đoạn Thụy Sĩ đã kết thúc.");
            }

            // Gom nhóm theo record thắng-thua để ghép cùng nhóm điểm trước
            var grouped = bxh
                .Where(x => doiConLai.Contains(ParseInt(x, "ma_nhom")))
                .GroupBy(x => ParseInt(x, "so_tran_thang") + "-" + ParseInt(x, "so_tran_thua"))
                .OrderByDescending(g => g.Key)
                .ToList();

            List<int> queue = new List<int>();
            foreach (var g in grouped)
            {
                List<int> ids = g.Select(x => ParseInt(x, "ma_nhom")).ToList();
                TronNgauNhien(ids);
                queue.AddRange(ids);
            }

            int tongTran = 0;
            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        while (queue.Count >= 2)
                        {
                            int a = queue[0];
                            queue.RemoveAt(0);

                            int idxB = queue.FindIndex(x => !_dal.DaTungGapNhauTrongGiaiDoan(maGiaiDoan, a, x));
                            if (idxB < 0) idxB = 0;

                            int b = queue[idxB];
                            queue.RemoveAt(idxB);

                            int maTran = _dal.TaoTran(conn, tran, TaoMatchNode(maGiaiDau, maGiaiDoan, soVongMoi, "swiss", "BO1"));
                            _dal.GanNhomVaoTran(conn, tran, maTran, a);
                            _dal.GanNhomVaoTran(conn, tran, maTran, b);
                            tongTran++;
                        }

                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        return ServiceResultDTO.Fail("Tạo vòng Swiss mới thất bại: " + ex.Message);
                    }
                }
            }

            return ServiceResultDTO.Ok("Đã sinh vòng Thụy Sĩ tiếp theo.", new { soVong = soVongMoi, tongTran });
        }

        private ServiceResultDTO TaoChampionRushGameTiepTheo(int maGiaiDau, int maGiaiDoan, int soVongMoi)
        {
            DataRow gd = _dal.LayGiaiDoanTheoId(maGiaiDoan);
            int diemNguong = gd["diem_nguong_match_point"] == DBNull.Value ? 0 : Convert.ToInt32(gd["diem_nguong_match_point"]);

            if (diemNguong > 0)
            {
                _dal.CapNhatCoMatchPoint(maGiaiDoan, diemNguong);
            }

            int soVongTruoc = soVongMoi - 1;
            List<MatchNodeDTO> dsTran = _dal.LayTranTheoGiaiDoan(maGiaiDoan);
            MatchNodeDTO tranMoiNhat = dsTran.FirstOrDefault(x => x.SoVong == soVongTruoc);

            if (tranMoiNhat != null)
            {
                int? top1 = _dal.LayNhomTop1Tran(tranMoiNhat.MaTran);
                if (top1.HasValue && _dal.NhomDangMatchPoint(maGiaiDoan, top1.Value))
                {
                    _dal.KetThucGiaiDoan(maGiaiDoan);
                    return ServiceResultDTO.Ok("Đã chạm điều kiện Champion Rush: đội Match Point thắng Top 1, giai đoạn kết thúc.", new { voDich = top1.Value });
                }
            }

            List<Dictionary<string, object>> bxh = _dal.LayBangXepHangRaw(maGiaiDoan);
            List<int> doiConLai = bxh.Select(x => ParseInt(x, "ma_nhom")).Where(x => x > 0).ToList();
            if (doiConLai.Count < 2)
            {
                return ServiceResultDTO.Fail("Không đủ đội để tạo game Champion Rush tiếp theo.");
            }

            int tongTran = 0;
            using (SqlConnection conn = DataProvider.CreateConnection())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        int maTran = _dal.TaoTran(conn, tran, TaoMatchNode(maGiaiDau, maGiaiDoan, soVongMoi, "champion_rush", "BO1"));
                        foreach (int maNhom in doiConLai)
                        {
                            _dal.GanNhomVaoTran(conn, tran, maTran, maNhom);
                        }
                        tongTran = 1;
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        return ServiceResultDTO.Fail("Tạo game Champion Rush tiếp theo thất bại: " + ex.Message);
                    }
                }
            }

            return ServiceResultDTO.Ok("Đã tạo game Champion Rush tiếp theo.", new { soVong = soVongMoi, tongTran });
        }

        private int TaoTranBR(SqlConnection conn, SqlTransaction tran, int maGiaiDau, int maGiaiDoan, int soVong, List<int> teams)
        {
            if (teams.Count < 2) return 0;
            int maTran = _dal.TaoTran(conn, tran, TaoMatchNode(maGiaiDau, maGiaiDoan, soVong, "battle_royale", "BO1"));
            foreach (int maNhom in teams)
            {
                _dal.GanNhomVaoTran(conn, tran, maTran, maNhom);
            }
            return 1;
        }

        private static List<int> HopNhom(List<int> a, List<int> b)
        {
            List<int> result = new List<int>(a);
            foreach (int x in b)
            {
                if (!result.Contains(x)) result.Add(x);
            }
            return result;
        }

        private static MatchNodeDTO TaoMatchNode(int maGiaiDau, int maGiaiDoan, int soVong, string nhanh, string theThucTran)
        {
            return new MatchNodeDTO
            {
                MaGiaiDau = maGiaiDau,
                MaGiaiDoan = maGiaiDoan,
                SoVong = soVong,
                NhanhDau = nhanh,
                TheThucTran = theThucTran,
                TrangThai = "chua_dau"
            };
        }

        private static void TronNgauNhien(List<int> list)
        {
            Random rnd = new Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                int tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        private static int ParseInt(Dictionary<string, object> row, string key)
        {
            if (!row.ContainsKey(key) || row[key] == null)
            {
                return 0;
            }

            return Convert.ToInt32(row[key]);
        }

        private static void CapNhatRouteTran(SqlConnection conn, SqlTransaction tran, int maTran, int? maThang, int? maThua)
        {
            DataProvider.ExecuteNonQuery(@"
UPDATE TRAN_DAU
SET ma_tran_tiep_theo_thang = @MaThang,
    ma_tran_tiep_theo_thua = @MaThua
WHERE ma_tran = @MaTran", new[]
            {
                new SqlParameter("@MaThang", SqlDbType.Int){ Value = (object)maThang ?? DBNull.Value },
                new SqlParameter("@MaThua", SqlDbType.Int){ Value = (object)maThua ?? DBNull.Value },
                new SqlParameter("@MaTran", SqlDbType.Int){ Value = maTran }
            }, conn, tran);
        }

        /// <summary>
        /// Trả về danh sách giải đấu đang công khai cho trang Dashboard.
        /// Tùy chọn lọc theo tựa game.
        /// </summary>
        public ServiceResultDTO LayDanhSachGiaiCongKhai(int? maTroChoi = null)
        {
            var list = _dal.LayDanhSachGiaiCongKhai(maTroChoi);
            return ServiceResultDTO.Ok("Lấy danh sách giải công khai thành công.", list);
        }
    }
}
