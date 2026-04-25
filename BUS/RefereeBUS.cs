using System;
using System.Collections.Generic;
using System.Linq;
using DAL;
using DTO;

namespace BUS
{
    public class RefereeBUS
    {
        private readonly RefereeDAL _dal = new RefereeDAL();
        private readonly IdentityDAL _identityDal = new IdentityDAL();

        public ServiceResultDTO GanTrongTai(int maNguoiPhanCong, int maTran, int maTrongTai)
        {
            if (maNguoiPhanCong <= 0 || maTran <= 0 || maTrongTai <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu phân công trọng tài không hợp lệ.");
            }

            if (!_dal.TranTonTai(maTran))
            {
                return ServiceResultDTO.Fail("Không tìm thấy trận đấu cần phân công.");
            }

            if (!_dal.NguoiDuocGanLaTrongTaiCuaGiai(maTran, maTrongTai))
            {
                return ServiceResultDTO.Fail("Người được gán chưa có vai trò trọng tài trong giải đấu của trận này.");
            }

            bool laAdmin = LaAdmin(maNguoiPhanCong);
            bool laBtc = _dal.NguoiPhanCongLaBanToChuc(maTran, maNguoiPhanCong);
            if (!laAdmin && !laBtc)
            {
                return ServiceResultDTO.Fail("Chỉ admin hoặc ban tổ chức của giải mới được phân công trọng tài.");
            }

            bool ok = _dal.GanTrongTai(maTran, maTrongTai);
            return ok
                ? ServiceResultDTO.Ok("Phân công trọng tài thành công.", new { maTran, maTrongTai })
                : ServiceResultDTO.Fail("Không thể phân công trọng tài cho trận đấu này.");
        }

        public ServiceResultDTO DanhSachTranCuaToi(int maTrongTai, string tab)
        {
            if (maTrongTai <= 0)
            {
                return ServiceResultDTO.Fail("Phiên đăng nhập không hợp lệ.");
            }

            string tabNorm = string.IsNullOrWhiteSpace(tab) ? "can_nhap_diem" : tab.Trim().ToLowerInvariant();
            List<string> tabsHopLe = new List<string> { "sap_dien_ra", "can_nhap_diem", "da_hoan_thanh" };
            if (!tabsHopLe.Contains(tabNorm))
            {
                return ServiceResultDTO.Fail("Tab không hợp lệ. Chỉ chấp nhận: sap_dien_ra, can_nhap_diem, da_hoan_thanh.");
            }

            return ServiceResultDTO.Ok("Lấy danh sách trận của trọng tài thành công.", _dal.LayTranCuaToi(maTrongTai, tabNorm));
        }

        public ServiceResultDTO ChiTietNhapLieuTran(int maTran)
        {
            if (maTran <= 0)
            {
                return ServiceResultDTO.Fail("Mã trận không hợp lệ.");
            }

            DataRowExt tran = DataRowExt.From(_dal.LayThongTinTran(maTran));
            if (tran == null)
            {
                return ServiceResultDTO.Fail("Không tìm thấy trận đấu.");
            }

            var roster = _dal.LayRosterNhapLieuTheoTran(maTran);
            return ServiceResultDTO.Ok("Lấy dữ liệu nhập điểm thành công.", new
            {
                Tran = tran.Raw,
                Roster = roster,
                CoTheSua = _dal.CoTheSuaTrong12h(maTran)
            });
        }

        public ServiceResultDTO NhapKetQuaLanDau(int maTrongTai, RefereeSubmitResultDTO dto)
        {
            if (dto == null || dto.MaTran <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu nhập kết quả không hợp lệ.");
            }

            ServiceResultDTO validate = KiemTraDuLieuNhap(maTrongTai, dto, batBuocLyDo: false, laSua: false);
            if (!validate.Success)
            {
                return validate;
            }

            if (_dal.DaNhapDiemLanDau(dto.MaTran))
            {
                return ServiceResultDTO.Fail("Trận này đã nhập điểm lần đầu, hãy dùng chức năng sửa kết quả.");
            }

            _dal.LuuKetQuaTran(dto.MaTran, dto.ChiSoNguoiChoi, maTrongTai, null, laSua: false, boQuaKhoa12h: false);
            ThuTuDongSinhVongTiepTheo(dto.MaTran);
            return ServiceResultDTO.Ok("Lưu kết quả trận đấu thành công.");
        }

        public ServiceResultDTO SuaKetQuaTrong12h(int maTrongTai, RefereeSubmitResultDTO dto)
        {
            if (dto == null || dto.MaTran <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu sửa kết quả không hợp lệ.");
            }

            ServiceResultDTO validate = KiemTraDuLieuNhap(maTrongTai, dto, batBuocLyDo: true, laSua: true);
            if (!validate.Success)
            {
                return validate;
            }

            if (!_dal.CoTheSuaTrong12h(dto.MaTran))
            {
                return ServiceResultDTO.Fail("Kết quả đã bị khóa: chỉ được sửa tối đa 1 lần trong vòng 12 giờ kể từ lần nhập đầu.");
            }

            _dal.LuuKetQuaTran(dto.MaTran, dto.ChiSoNguoiChoi, maTrongTai, dto.LyDo.Trim(), laSua: true, boQuaKhoa12h: false);
            ThuTuDongSinhVongTiepTheo(dto.MaTran);
            return ServiceResultDTO.Ok("Sửa kết quả thành công và đã ghi nhật ký audit.");
        }

        public ServiceResultDTO AdminSuaKetQua(int maAdmin, RefereeSubmitResultDTO dto)
        {
            if (dto == null || dto.MaTran <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu sửa kết quả không hợp lệ.");
            }

            ServiceResultDTO validate = KiemTraDuLieuNhap(maAdmin, dto, batBuocLyDo: true, laSua: true);
            if (!validate.Success)
            {
                return validate;
            }

            _dal.LuuKetQuaTran(dto.MaTran, dto.ChiSoNguoiChoi, maAdmin, dto.LyDo.Trim(), laSua: true, boQuaKhoa12h: true);
            ThuTuDongSinhVongTiepTheo(dto.MaTran);
            return ServiceResultDTO.Ok("Admin đã sửa kết quả thành công và ghi audit log.");
        }

        public ServiceResultDTO TaoKhieuNai(int maNguoiGui, TaoKhieuNaiKetQuaDTO dto)
        {
            if (dto == null || dto.MaTran <= 0 || dto.MaNhom <= 0 || string.IsNullOrWhiteSpace(dto.NoiDung))
            {
                return ServiceResultDTO.Fail("Dữ liệu khiếu nại không hợp lệ.");
            }

            if (!_dal.NguoiDungThuocTran(dto.MaTran, maNguoiGui))
            {
                return ServiceResultDTO.Fail("Bạn không thuộc danh sách thi đấu của trận này nên không thể khiếu nại.");
            }

            if (!_dal.NhomThuocTran(dto.MaTran, dto.MaNhom))
            {
                return ServiceResultDTO.Fail("Nhóm khiếu nại không thuộc trận đấu này.");
            }

            if (!_dal.NguoiDungThuocNhomTrongTran(dto.MaTran, dto.MaNhom, maNguoiGui))
            {
                return ServiceResultDTO.Fail("Bạn chỉ có thể khiếu nại thay cho nhóm của mình trong trận.");
            }

            if (_dal.DaCoKhieuNaiChoXuLy(dto.MaTran, dto.MaNhom))
            {
                return ServiceResultDTO.Fail("Nhóm này đã có khiếu nại đang chờ xử lý cho trận đấu này.");
            }

            int maKhieuNai = _dal.TaoKhieuNaiKetQua(dto, maNguoiGui);
            return ServiceResultDTO.Ok("Gửi khiếu nại kết quả thành công.", new { maKhieuNai });
        }

        public ServiceResultDTO LichSuSuaKetQua(int maNguoiDung, int? maTran)
        {
            if (!LaAdmin(maNguoiDung))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới được xem audit log sửa kết quả.");
            }

            if (maTran.HasValue && maTran.Value <= 0)
            {
                return ServiceResultDTO.Fail("Mã trận không hợp lệ.");
            }

            return ServiceResultDTO.Ok("Lấy lịch sử sửa kết quả thành công.", _dal.LayLichSuSuaKetQua(maTran));
        }

        public bool KiemTraTrongTaiDuocPhep(int maTran, int maNguoiDung)
        {
            return _dal.KiemTraTrongTaiPhuTrach(maTran, maNguoiDung);
        }

        public ServiceResultDTO DanhSachKhieuNai(int maNguoiDung, string trangThai)
        {
            if (!LaAdmin(maNguoiDung))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới được xem danh sách khiếu nại.");
            }

            return ServiceResultDTO.Ok("Lấy danh sách khiếu nại thành công.", _dal.LayDanhSachKhieuNai(trangThai));
        }

        public ServiceResultDTO XuLyKhieuNai(int maAdmin, XuLyKhieuNaiDTO dto)
        {
            if (!LaAdmin(maAdmin))
            {
                return ServiceResultDTO.Fail("Chỉ admin hệ thống mới được xử lý khiếu nại.");
            }

            if (dto == null || dto.MaKhieuNai <= 0)
            {
                return ServiceResultDTO.Fail("Dữ liệu xử lý khiếu nại không hợp lệ.");
            }

            bool ok = _dal.XuLyKhieuNai(dto.MaKhieuNai, maAdmin, dto.ChapNhan, dto.PhanHoiAdmin);
            return ok
                ? ServiceResultDTO.Ok("Đã xử lý khiếu nại thành công.")
                : ServiceResultDTO.Fail("Không thể xử lý khiếu nại (có thể đã được xử lý trước đó).");
        }

        private ServiceResultDTO KiemTraDuLieuNhap(int maNguoiThucHien, RefereeSubmitResultDTO dto, bool batBuocLyDo, bool laSua)
        {
            if (maNguoiThucHien <= 0)
            {
                return ServiceResultDTO.Fail("Phiên đăng nhập không hợp lệ.");
            }

            if (dto.ChiSoNguoiChoi == null || dto.ChiSoNguoiChoi.Count == 0)
            {
                return ServiceResultDTO.Fail("Thiếu dữ liệu chỉ số người chơi.");
            }

            if (dto.ChiSoNguoiChoi.Count > 16)
            {
                return ServiceResultDTO.Fail("Một trận chỉ hỗ trợ nhập tối đa 16 tuyển thủ.");
            }

            if (batBuocLyDo && string.IsNullOrWhiteSpace(dto.LyDo))
            {
                return ServiceResultDTO.Fail("Bắt buộc nhập lý do khi sửa kết quả để lưu audit log.");
            }

            var duplicated = dto.ChiSoNguoiChoi
                .GroupBy(x => x.MaNguoiDung)
                .Where(g => g.Key > 0 && g.Count() > 1)
                .Select(g => g.Key)
                .FirstOrDefault();
            if (duplicated > 0)
            {
                return ServiceResultDTO.Fail("Danh sách chỉ số có tuyển thủ bị trùng: " + duplicated);
            }

            foreach (RefereePlayerStatInputDTO p in dto.ChiSoNguoiChoi)
            {
                if (p.MaNguoiDung <= 0 || p.SoKill < 0 || p.SoDeath < 0 || p.SoAssist < 0)
                {
                    return ServiceResultDTO.Fail("Chỉ số K/D/A không hợp lệ. Mọi giá trị phải >= 0.");
                }

                if (p.DiemSinhTon.HasValue && p.DiemSinhTon.Value < 0)
                {
                    return ServiceResultDTO.Fail("Điểm sinh tồn không hợp lệ. Giá trị phải >= 0.");
                }

                if (!_dal.NguoiDungThuocTran(dto.MaTran, p.MaNguoiDung))
                {
                    return ServiceResultDTO.Fail("Tuyển thủ " + p.MaNguoiDung + " không thuộc trận đấu này.");
                }
            }

            return ServiceResultDTO.Ok(laSua ? "Dữ liệu sửa hợp lệ." : "Dữ liệu nhập hợp lệ.");
        }

        private bool LaAdmin(int maNguoiDung)
        {
            NguoiDungDTO user = _identityDal.LayTheoId(maNguoiDung);
            return user != null && string.Equals(user.VaiTroHeThong, "admin", StringComparison.OrdinalIgnoreCase);
        }

        private void ThuTuDongSinhVongTiepTheo(int maTran)
        {
            System.Data.DataRow row = _dal.LayThongTinDongBoTheoTran(maTran);
            if (row == null)
            {
                return;
            }

            int maGiaiDau = Convert.ToInt32(row["ma_giai_dau"]);
            int maGiaiDoan = Convert.ToInt32(row["ma_giai_doan"]);
            string theThuc = row["the_thuc"] == DBNull.Value ? string.Empty : row["the_thuc"].ToString();

            if (!string.Equals(theThuc, "thuy_si", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(theThuc, "champion_rush", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            new MatchmakingBUS().TaoVongTiepTheo(maGiaiDau, maGiaiDoan);
        }

        private class DataRowExt
        {
            public object Raw { get; private set; }

            public static DataRowExt From(System.Data.DataRow row)
            {
                if (row == null)
                {
                    return null;
                }

                Dictionary<string, object> result = new Dictionary<string, object>();
                foreach (System.Data.DataColumn col in row.Table.Columns)
                {
                    result[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                }

                return new DataRowExt { Raw = result };
            }
        }
    }
}
