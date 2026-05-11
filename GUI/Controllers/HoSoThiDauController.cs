using BUS;
using DTO;
using System.Collections.Generic;
using System.Web.Mvc;

namespace GUI.Controllers
{
    public class HoSoThiDauController : Controller
    {
        private readonly HoSoThiDauBUS hoSoThiDauBUS = new HoSoThiDauBUS();

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ApiResponseDTO troChoiResponse = hoSoThiDauBUS.LayDanhSachTroChoi();
            ViewBag.TroChoi = troChoiResponse.success
                ? troChoiResponse.data as List<TroChoiDTO>
                : new List<TroChoiDTO>();

            return View();
        }
    }
}
