using System.Web.Mvc;

namespace GUI.Controllers
{
    public class DoiController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public ActionResult ChiTiet(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.MaDoi = id;
            return View();
        }
    }
}
