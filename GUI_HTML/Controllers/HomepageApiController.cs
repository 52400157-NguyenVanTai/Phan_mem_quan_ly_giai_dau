using System.Web.Mvc;
using BUS;

namespace GUI_HTML.Controllers
{
    public class HomepageApiController : Controller
    {
        private readonly PublicHomepageBUS _bus = new PublicHomepageBUS();

        [HttpGet]
        public JsonResult PublicData()
        {
            return Json(_bus.LayDuLieuTrangChu(), JsonRequestBehavior.AllowGet);
        }
    }
}
