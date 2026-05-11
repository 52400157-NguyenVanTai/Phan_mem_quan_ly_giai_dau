using BUS;
using DTO;
using System.Web.Mvc;

namespace GUI.Controllers
{
    public class HoSoThiDauApiController : Controller
    {
        private readonly HoSoThiDauBUS hoSoThiDauBUS = new HoSoThiDauBUS();

        [HttpGet]
        public JsonResult TroChoi()
        {
            if (Session["UserId"] == null)
            {
                return Json(new ApiResponseDTO { success = false, message = "Bạn chưa đăng nhập.", data = null }, JsonRequestBehavior.AllowGet);
            }

            return Json(hoSoThiDauBUS.LayDanhSachTroChoi(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ViTri(int? maTroChoi, string loaiViTri)
        {
            if (Session["UserId"] == null)
            {
                return Json(new ApiResponseDTO { success = false, message = "Bạn chưa đăng nhập.", data = null }, JsonRequestBehavior.AllowGet);
            }

            return Json(hoSoThiDauBUS.LayDanhSachViTri(maTroChoi, loaiViTri), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Current()
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return Json(new ApiResponseDTO { success = false, message = "Bạn chưa đăng nhập.", data = null }, JsonRequestBehavior.AllowGet);
            }

            return Json(hoSoThiDauBUS.LayHoSo(userId.Value), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult All()
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return Json(new ApiResponseDTO { success = false, message = "Báº¡n chÆ°a Ä‘Äƒng nháº­p.", data = null }, JsonRequestBehavior.AllowGet);
            }

            return Json(hoSoThiDauBUS.LayDanhSachHoSo(userId.Value), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Save(HoSoThiDauRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return Json(new ApiResponseDTO { success = false, message = "Bạn chưa đăng nhập.", data = null });
            }

            return Json(hoSoThiDauBUS.LuuHoSo(userId.Value, request));
        }

        [HttpPost]
        public JsonResult Delete(HoSoThiDauDeleteRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return Json(new ApiResponseDTO { success = false, message = "Bạn chưa đăng nhập.", data = null });
            }

            return Json(hoSoThiDauBUS.XoaHoSo(userId.Value, request));
        }
    }
}
