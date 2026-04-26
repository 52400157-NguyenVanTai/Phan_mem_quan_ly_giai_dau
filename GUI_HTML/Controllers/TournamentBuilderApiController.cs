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
        public JsonResult GuiXetDuyet(int maGiaiDau)
        {
            int maNguoiGui = (int)Session["CurrentUserId"];
            return Json(_bus.GuiXetDuyet(maNguoiGui, maGiaiDau), JsonRequestBehavior.AllowGet);
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
        public JsonResult TuChoi(int maGiaiDau)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_bus.TuChoiVaXoaCung(maAdmin, maGiaiDau), JsonRequestBehavior.AllowGet);
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
        public JsonResult XoaGiaiDoan(int maGiaiDau, int maGiaiDoan)
        {
            int maNguoi = (int)Session["CurrentUserId"];
            return Json(_bus.XoaGiaiDoan(maNguoi, maGiaiDau, maGiaiDoan), JsonRequestBehavior.AllowGet);
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
        public JsonResult BatDauGiai(int maGiaiDau)
        {
            int maNguoi = (int)Session["CurrentUserId"];
            return Json(_bus.ChuyenSangDangDienRa(maNguoi, maGiaiDau), JsonRequestBehavior.AllowGet);
        }
    }
}
