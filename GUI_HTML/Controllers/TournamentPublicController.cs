using System.Web.Mvc;

namespace GUI_HTML.Controllers
{
    public class TournamentPublicController : Controller
    {
        public ActionResult Index(int id)
        {
            ViewBag.MaGiaiDau = id;
            return View();
        }
    }
}
