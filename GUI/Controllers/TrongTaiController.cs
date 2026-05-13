using BUS;
using DTO;
using System.Web.Mvc;

namespace GUI.Controllers
{
    public class TrongTaiController : Controller
    {
        private readonly GiaiDauBUS giaiDauBUS = new GiaiDauBUS();

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet]
        public JsonResult Matches()
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(new ApiResponseDTO { success = false, message = "Bạn chưa đăng nhập.", data = null }, JsonRequestBehavior.AllowGet);
            return Json(giaiDauBUS.LayTranDauCuaTrongTai(userId.Value), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateResult(UpdateMatchStatsRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(new ApiResponseDTO { success = false, message = "Bạn chưa đăng nhập.", data = null });
            var result = giaiDauBUS.UpdateMatchStatsTrongTai(userId.Value, request);
            if (!result.success) Response.StatusCode = 400;
            return Json(result);
        }
    }
}
