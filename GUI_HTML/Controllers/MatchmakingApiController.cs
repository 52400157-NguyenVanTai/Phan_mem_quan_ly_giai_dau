using System.Web.Mvc;
using BUS;
using DTO;
using GUI_HTML.Filters;

namespace GUI_HTML.Controllers
{
    public class MatchmakingApiController : Controller
    {
        private readonly MatchmakingBUS _bus = new MatchmakingBUS();

        [HttpPost]
        [RequireLogin]
        public JsonResult TaoLich(TaoLichGiaiDoanDTO dto)
        {
            int maNguoiDung = (int)Session["CurrentUserId"];
            return Json(_bus.TaoLichThiDau(maNguoiDung, dto), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult TaoVongTiepTheo(int maGiaiDau, int maGiaiDoan)
        {
            return Json(_bus.TaoVongTiepTheo(maGiaiDau, maGiaiDoan), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DanhSachTran(int maGiaiDoan)
        {
            return Json(_bus.LayTranTheoGiaiDoan(maGiaiDoan), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CongKhai(int maGiaiDau)
        {
            return Json(_bus.TongQuanCongKhai(maGiaiDau), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult BangXepHang(int maGiaiDoan)
        {
            return Json(_bus.BangXepHangTheoGiaiDoan(maGiaiDoan), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult VinhDanh(int maGiaiDau)
        {
            return Json(_bus.VinhDanhTuDong(maGiaiDau), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult LiveSnapshot(int maGiaiDau, int maGiaiDoan)
        {
            return Json(_bus.LiveSnapshot(maGiaiDau, maGiaiDoan), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Trả về danh sách giải đấu công khai cho Dashboard (không cần đăng nhập).
        /// Tùy chọn lọc theo game.
        /// </summary>
        [HttpGet]
        public JsonResult DanhSachGiaiCongKhai(int? maTroChoi = null)
        {
            return Json(_bus.LayDanhSachGiaiCongKhai(maTroChoi), JsonRequestBehavior.AllowGet);
        }
    }
}
