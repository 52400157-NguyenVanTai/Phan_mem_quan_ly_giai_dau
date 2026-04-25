using System.Web.Mvc;
using BUS;
using DTO;
using GUI_HTML.Filters;

namespace GUI_HTML.Controllers
{
    public class TournamentApiController : Controller
    {
        private readonly TournamentRequestBUS _bus = new TournamentRequestBUS();

        [HttpPost]
        [RequireLogin]
        public JsonResult GuiYeuCau(YeuCauTaoGiaiDTO dto)
        {
            dto.MaNguoiGui = (int)Session["CurrentUserId"];
            return Json(_bus.GuiYeuCauTaoGiai(dto), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult Duyet(int maYeuCau)
        {
            return Json(_bus.DuyetYeuCau((int)Session["CurrentUserId"], maYeuCau), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult TuChoi(int maYeuCau, string lyDo)
        {
            return Json(_bus.TuChoiYeuCau((int)Session["CurrentUserId"], maYeuCau, lyDo), JsonRequestBehavior.AllowGet);
        }
    }
}
