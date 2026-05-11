using BUS;
using DTO;
using System.Web.Mvc;

namespace GUI.Controllers
{
    public class YeuCauApiController : Controller
    {
        private readonly YeuCauBUS bus = new YeuCauBUS();
        private readonly GiaiDauBUS gdBus = new GiaiDauBUS();

        [HttpGet]
        public JsonResult All()
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap(), JsonRequestBehavior.AllowGet);
            
            // Get all aggregated requests
            return Json(bus.LayDanhSachYeuCau(userId.Value), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult HandleRequest(XuLyYeuCauRequestDTO req)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());

            return Json(bus.XuLyYeuCau(userId.Value, req));
        }
        
        [HttpGet]
        public JsonResult GetTournamentDetail(int maGiaiDau)
        {
            return Json(gdBus.LayChiTiet(maGiaiDau), JsonRequestBehavior.AllowGet);
        }

        private ApiResponseDTO ChuaDangNhap()
        {
            return new ApiResponseDTO { success = false, message = "Bạn chưa đăng nhập." };
        }
    }
}
