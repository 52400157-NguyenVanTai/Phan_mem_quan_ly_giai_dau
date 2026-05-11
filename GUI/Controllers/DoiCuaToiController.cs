using System.Web.Mvc;

namespace GUI.Controllers
{
    public class DoiCuaToiController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}
