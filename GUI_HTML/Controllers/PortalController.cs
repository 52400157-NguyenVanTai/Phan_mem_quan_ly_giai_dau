using System.Web.Mvc;

namespace GUI_HTML.Controllers
{
    public class PortalController : Controller
    {
        // /dashboard — Private dashboard (requires session)
        public ActionResult Index()
        {
            return View();
        }

        // /login — Public auth page
        public ActionResult Login()
        {
            return View();
        }
    }
}
