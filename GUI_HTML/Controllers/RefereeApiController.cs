using System.Web.Mvc;
using BUS;
using DTO;
using GUI_HTML.Filters;

namespace GUI_HTML.Controllers
{
    public class RefereeApiController : Controller
    {
        private readonly RefereeBUS _bus = new RefereeBUS();

        [HttpPost]
        [RequireLogin]
        public JsonResult GanTrongTai(int maTran, int maTrongTai)
        {
            int maNguoiPhanCong = (int)Session["CurrentUserId"];
            return Json(_bus.GanTrongTai(maNguoiPhanCong, maTran, maTrongTai), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [RequireLogin]
        public JsonResult TranCuaToi(string tab = "can_nhap_diem")
        {
            int maTrongTai = (int)Session["CurrentUserId"];
            return Json(_bus.DanhSachTranCuaToi(maTrongTai, tab), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [RequireLogin]
        [RequireRefereeMatchAccess]
        public JsonResult ChiTietTran(int maTran)
        {
            return Json(_bus.ChiTietNhapLieuTran(maTran), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireRefereeMatchAccess]
        public JsonResult NhapKetQua(RefereeSubmitResultDTO dto)
        {
            int maTrongTai = (int)Session["CurrentUserId"];
            return Json(_bus.NhapKetQuaLanDau(maTrongTai, dto), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireRefereeMatchAccess]
        public JsonResult SuaKetQua(RefereeSubmitResultDTO dto)
        {
            int maTrongTai = (int)Session["CurrentUserId"];
            return Json(_bus.SuaKetQuaTrong12h(maTrongTai, dto), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult AdminSuaKetQua(RefereeSubmitResultDTO dto)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_bus.AdminSuaKetQua(maAdmin, dto), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult TaoKhieuNai(TaoKhieuNaiKetQuaDTO dto)
        {
            int maNguoiGui = (int)Session["CurrentUserId"];
            return Json(_bus.TaoKhieuNai(maNguoiGui, dto), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult DanhSachKhieuNai(string trangThai = null)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_bus.DanhSachKhieuNai(maAdmin, trangThai), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult XuLyKhieuNai(XuLyKhieuNaiDTO dto)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_bus.XuLyKhieuNai(maAdmin, dto), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult LichSuSuaKetQua(int? maTran = null)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_bus.LichSuSuaKetQua(maAdmin, maTran), JsonRequestBehavior.AllowGet);
        }
    }
}
