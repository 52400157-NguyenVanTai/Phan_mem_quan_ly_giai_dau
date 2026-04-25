using System.Web.Mvc;
using BUS;
using GUI_HTML.Filters;

namespace GUI_HTML.Controllers
{
    public class AdminApiController : Controller
    {
        private readonly AdminBUS _adminBus = new AdminBUS();
        private readonly GameBUS _gameBus = new GameBUS();

        // ================================================================
        // MODULE 1: GLOBAL DASHBOARD
        // ================================================================

        [HttpGet]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult Dashboard()
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_adminBus.LayThongKeDashboard(maAdmin), JsonRequestBehavior.AllowGet);
        }

        // ================================================================
        // MODULE 3: QUẢN LÝ NGƯỜI DÙNG
        // ================================================================

        [HttpGet]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult TimKiemUser(string tuKhoa = null, bool? isBanned = null)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_adminBus.TimKiemNguoiDung(maAdmin, tuKhoa, isBanned), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult BanUser(int maNguoiDung, string lyDo)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_adminBus.BanNguoiDung(maAdmin, maNguoiDung, lyDo), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult BanDoi(int maDoi, string lyDo)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_adminBus.BanDoi(maAdmin, maDoi, lyDo), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult UnbanUser(int maNguoiDung)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_adminBus.UnbanNguoiDung(maAdmin, maNguoiDung), JsonRequestBehavior.AllowGet);
        }

        // ================================================================
        // MODULE 2: QUẢN LÝ GAME
        // ================================================================

        [HttpGet]
        public JsonResult DanhSachGame(bool baoCaInactive = false)
        {
            int maNguoiDung = Session["CurrentUserId"] != null ? (int)Session["CurrentUserId"] : 0;
            return Json(_gameBus.LayDanhSachGame(maNguoiDung, baoCaInactive), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult ThemGame(string tenGame, string theLoai)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_gameBus.ThemGame(maAdmin, tenGame, theLoai), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult SuaGame(int maGame, string tenGame, string theLoai)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_gameBus.SuaGame(maAdmin, maGame, tenGame, theLoai), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult AnGame(int maGame)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_gameBus.AnGame(maAdmin, maGame), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult KichHoatGame(int maGame)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_gameBus.KichHoatGame(maAdmin, maGame), JsonRequestBehavior.AllowGet);
        }

        // ================================================================
        // MODULE 5: HARD WIPE
        // ================================================================

        [HttpPost]
        [RequireLogin]
        [RequireSystemRole("admin")]
        public JsonResult HardWipeGiai(int maGiaiDau)
        {
            int maAdmin = (int)Session["CurrentUserId"];
            return Json(_adminBus.XoaCungGiaiDau(maAdmin, maGiaiDau), JsonRequestBehavior.AllowGet);
        }
    }
}
