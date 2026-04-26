using System.Web.Mvc;
using BUS;
using GUI_HTML.Filters;

namespace GUI_HTML.Controllers
{
    public class TuongTacApiController : Controller
    {
        private readonly TuongTacBUS _bus = new TuongTacBUS();

        // ---- Lấy trạng thái like/follow + số tổng của 1 giải ----
        [HttpGet]
        public JsonResult TrangThai(int maGiaiDau)
        {
            int maNguoiDung = Session["CurrentUserId"] != null ? (int)Session["CurrentUserId"] : 0;
            return Json(_bus.LayTrangThaiGiai(maNguoiDung, maGiaiDau), JsonRequestBehavior.AllowGet);
        }

        // ---- Toggle Like ----
        [HttpPost]
        [RequireLogin]
        public JsonResult Like(int maGiaiDau)
        {
            int maNguoiDung = (int)Session["CurrentUserId"];
            return Json(_bus.ToggleLike(maNguoiDung, maGiaiDau), JsonRequestBehavior.AllowGet);
        }

        // ---- Toggle Follow ----
        [HttpPost]
        [RequireLogin]
        public JsonResult Follow(int maGiaiDau)
        {
            int maNguoiDung = (int)Session["CurrentUserId"];
            return Json(_bus.ToggleFollow(maNguoiDung, maGiaiDau), JsonRequestBehavior.AllowGet);
        }

        // ---- Danh sách giải đang theo dõi của user hiện tại ----
        [HttpGet]
        [RequireLogin]
        public JsonResult DanhSachTheoDoi()
        {
            int maNguoiDung = (int)Session["CurrentUserId"];
            return Json(_bus.LayGiaiDangTheoDoi(maNguoiDung), JsonRequestBehavior.AllowGet);
        }
    }
}
