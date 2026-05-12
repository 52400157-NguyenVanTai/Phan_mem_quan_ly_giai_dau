using System.Web.Mvc;

namespace GUI.Controllers
{
    public class GiaiDauController : Controller
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

            ViewBag.MaGiaiDau = id;
            return View();
        }
    }
}
