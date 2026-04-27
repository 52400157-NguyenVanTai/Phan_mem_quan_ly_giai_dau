using System.Web.Mvc;
using BUS;
using GUI_HTML.Filters;

namespace GUI_HTML.Controllers
{
    public class RecruitmentApiController : Controller
    {
        private readonly RecruitmentBUS _bus = new RecruitmentBUS();

        [HttpPost]
        [RequireLogin]
        [RequireTeamRole("chu_tich", "ban_dieu_hanh", "doi_truong")]
        public JsonResult TaoBaiDang(int maDoi, int maNhom, int maViTri, string noiDung)
        {
            return Json(_bus.TaoBaiDang(maDoi, maNhom, maViTri, noiDung), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult UngTuyen(int maBaiDang)
        {
            int maUngVien = (int)Session["CurrentUserId"];
            return Json(_bus.UngTuyen(maBaiDang, maUngVien), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireTeamRole("chu_tich", "ban_dieu_hanh", "doi_truong")]
        public JsonResult GuiLoiMoi(int maDoi, int maNhom, int maNguoiDuocMoi)
        {
            return Json(_bus.GuiLoiMoi(maDoi, maNhom, maNguoiDuocMoi), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult DuyetDon(int maDon, bool chapNhan)
        {
            int maNguoiDuyet = (int)Session["CurrentUserId"];
            return Json(_bus.DuyetDonUngTuyen(maNguoiDuyet, maDon, chapNhan), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult PhanHoiLoiMoi(int maLoiMoi, bool chapNhan)
        {
            int maNguoiDuocMoi = (int)Session["CurrentUserId"];
            return Json(_bus.PhanHoiLoiMoi(maNguoiDuocMoi, maLoiMoi, chapNhan), JsonRequestBehavior.AllowGet);
        }
    }
}
