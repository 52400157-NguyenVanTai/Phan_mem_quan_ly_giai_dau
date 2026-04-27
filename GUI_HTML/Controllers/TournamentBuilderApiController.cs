using System.Web.Mvc;
using BUS;
using DTO;
using GUI_HTML.Filters;

namespace GUI_HTML.Controllers
{
    public class TournamentBuilderApiController : Controller
    {
        private readonly TournamentBuilderBUS _bus = new TournamentBuilderBUS();

        [HttpPost]
        [RequireLogin]
        public JsonResult TaoBanNhap(TaoGiaiDauDTO dto)
        {
            dto.MaNguoiTao = (int)Session["CurrentUserId"];
            return Json(_bus.TaoBanNhap(dto), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [RequireLogin]
        public JsonResult DanhSachCuaToi()
        {
            int maNguoiTao = (int)Session["CurrentUserId"];
            return Json(_bus.LayDanhSachGiaiCuaToi(maNguoiTao), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult GuiXetDuyet(MaGiaiDauDTO dto)
        {
            int maNguoiGui = (int)Session["CurrentUserId"];
            return Json(_bus.GuiXetDuyet(maNguoiGui, dto.MaGiaiDau), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult PheDuyet(int maGiaiDau)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_bus.PheDuyet(maAdmin, maGiaiDau), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult BulkPheDuyet()
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_bus.BulkPheDuyet(maAdmin), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult TuChoi(TuChoiYeuCauGiaiDTO dto)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            if (dto == null)
            {
                return Json(ServiceResultDTO.Fail("Dữ liệu từ chối không hợp lệ."), JsonRequestBehavior.AllowGet);
            }

            return Json(_bus.TuChoiYeuCau(maAdmin, dto.MaGiaiDau, dto.LyDo), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult DanhSachChoXetDuyet()
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_bus.LayDanhSachChoXetDuyet(maAdmin), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult KhoaGiai(int maGiaiDau, string lyDo)
        {
            var dto = new CapNhatTrangThaiGiaiDTO
            {
                MaGiaiDau = maGiaiDau,
                MaNguoiThucHien = (int)Session["CurrentUserId"],
                LyDo = lyDo
            };
            return Json(_bus.KhoaGiai(dto), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult MoKhoaGiai(int maGiaiDau)
        {
            var dto = new CapNhatTrangThaiGiaiDTO
            {
                MaGiaiDau = maGiaiDau,
                MaNguoiThucHien = (int)Session["CurrentUserId"]
            };
            return Json(_bus.MoKhoaGiai(dto), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult ThemGiaiDoan(TaoGiaiDoanDTO dto)
        {
            int maNguoi = (int)Session["CurrentUserId"];
            return Json(_bus.ThemGiaiDoan(maNguoi, dto), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult LenThuTuGiaiDoan(int maGiaiDau, int maGiaiDoan)
        {
            int maNguoi = (int)Session["CurrentUserId"];
            return Json(_bus.LenThuTuGiaiDoan(maNguoi, maGiaiDau, maGiaiDoan), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult XuongThuTuGiaiDoan(int maGiaiDau, int maGiaiDoan)
        {
            int maNguoi = (int)Session["CurrentUserId"];
            return Json(_bus.XuongThuTuGiaiDoan(maNguoi, maGiaiDau, maGiaiDoan), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult XoaGiaiDoan(XoaGiaiDoanDTO dto)
        {
            int maNguoi = (int)Session["CurrentUserId"];
            return Json(_bus.XoaGiaiDoan(maNguoi, dto.MaGiaiDau, dto.MaGiaiDoan), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [RequireLogin]
        public JsonResult DanhSachGiaiDoan(int maGiaiDau)
        {
            return Json(_bus.LayDanhSachGiaiDoan(maGiaiDau), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult DuyetDangKyDoi(DuyetThamGiaGiaiDTO dto)
        {
            int maNguoi = (int)Session["CurrentUserId"];
            return Json(_bus.DuyetDangKyDoi(maNguoi, dto), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult CapNhatHatGiong(CapNhatHatGiongDTO dto)
        {
            int maNguoi = (int)Session["CurrentUserId"];
            return Json(_bus.CapNhatHatGiong(maNguoi, dto), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult DongBoRoster(CapNhatDoiHinhGiaiDTO dto)
        {
            int maNguoi = (int)Session["CurrentUserId"];
            return Json(_bus.CapNhatDoiHinhThiDau(maNguoi, dto), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult BatDauGiai(MaGiaiDauDTO dto)
        {
            int maNguoi = (int)Session["CurrentUserId"];
            return Json(_bus.ChuyenSangDangDienRa(maNguoi, dto.MaGiaiDau), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [RequireLogin]
        public JsonResult GiaiCuaToi()
        {
            int maNguoiDung = (int)Session["CurrentUserId"];
            return Json(_bus.LayGiaiCuaToi(maNguoiDung), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [RequireLogin]
        public JsonResult DanhSachDangKyDoi(int maGiaiDau)
        {
            int maNguoiDung = (int)Session["CurrentUserId"];
            return Json(_bus.LayDanhSachDangKyDoi(maNguoiDung, maGiaiDau), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult CapNhatGiaiDau(int maGiaiDau, TaoGiaiDauDTO dto)
        {
            int maNguoi = (int)Session["CurrentUserId"];
            return Json(_bus.CapNhatGiaiDau(maNguoi, dto, maGiaiDau), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult TamHoanGiaiDau(TamHoanGiaiDauDTO dto)
        {
            dto.MaAdmin = (int)Session["CurrentUserId"];
            return Json(_bus.TamHoanGiaiDau(dto), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult KhoiPhucTuTamHoan(int maGiaiDau)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_bus.KhoiPhucTuTamHoan(maAdmin, maGiaiDau), JsonRequestBehavior.AllowGet);
        }

        // Scheduled job endpoints (should be secured with API key in production)
        [HttpPost]
        public JsonResult KiemTraVaChuyenTrangThaiChuanBiDienRa()
        {
            return Json(_bus.KiemTraVaChuyenTrangThaiChuanBiDienRa(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult KiemTraVaChuyenTrangThaiTongKet()
        {
            return Json(_bus.KiemTraVaChuyenTrangThaiTongKet(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult KiemTraVaChuyenTrangThaiKetThuc()
        {
            return Json(_bus.KiemTraVaChuyenTrangThaiKetThuc(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult TuDongHuyYeuCauQuaHan()
        {
            return Json(_bus.TuDongHuyYeuCauQuaHan(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult TuDongKhoiPhucKhiKhongDuDoi()
        {
            return Json(_bus.TuDongKhoiPhucKhiKhongDuDoi(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult TuDongDongDangKyKhiDuDoi()
        {
            return Json(_bus.TuDongDongDangKyKhiDuDoi(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult MoiTrongTai(MoiTrongTaiDTO dto)
        {
            int maNguoi = (int)Session["CurrentUserId"];
            return Json(_bus.MoiTrongTai(maNguoi, dto), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult MoiBanToChuc(MoiBanToChucDTO dto)
        {
            int maNguoi = (int)Session["CurrentUserId"];
            return Json(_bus.MoiBanToChuc(maNguoi, dto), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult CapNhatDangMoDangKy(CapNhatDangMoDangKyDTO dto)
        {
            int maNguoi = (int)Session["CurrentUserId"];
            return Json(_bus.CapNhatDangMoDangKy(maNguoi, dto.MaGiaiDau, dto.DangMo), JsonRequestBehavior.AllowGet);
        }
    }
}
