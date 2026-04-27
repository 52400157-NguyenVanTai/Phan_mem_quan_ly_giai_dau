using System.Web.Mvc;
using BUS;
using DTO;
using GUI_HTML.Filters;

namespace GUI_HTML.Controllers
{
    public class ProfileApiController : Controller
    {
        private readonly ProfileBUS _profileBus = new ProfileBUS();

        [HttpGet]
        public JsonResult TroChoi()
        {
            return Json(_profileBus.LayDanhSachTroChoi(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ViTri(int maTroChoi)
        {
            return Json(_profileBus.LayViTriTheoGame(maTroChoi), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [RequireLogin]
        public JsonResult LayHoSo(int maTroChoi)
        {
            int maNguoiDung = (int)Session["CurrentUserId"];
            return Json(_profileBus.LayHoSo(maNguoiDung, maTroChoi), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [RequireLogin]
        public JsonResult LayTatCaHoSo()
        {
            int maNguoiDung = (int)Session["CurrentUserId"];
            return Json(_profileBus.LayTatCaHoSoCuaNguoiDung(maNguoiDung), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult TaoHoSo(HoSoInGameDTO dto)
        {
            dto.MaNguoiDung = (int)Session["CurrentUserId"];
            return Json(_profileBus.TaoHoSo(dto), JsonRequestBehavior.AllowGet);
        }
    }
}
