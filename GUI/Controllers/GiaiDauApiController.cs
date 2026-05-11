using BUS;
using DTO;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace GUI.Controllers
{
    public class GiaiDauApiController : Controller
    {
        private readonly GiaiDauBUS bus = new GiaiDauBUS();

        [HttpPost]
        public JsonResult Create(TaoGiaiDauRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(bus.TaoGiaiDau(userId.Value, request));
        }

        [HttpPost]
        public JsonResult Update(CapNhatGiaiDauRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(bus.CapNhatGiaiDau(userId.Value, request));
        }

        [HttpPost]
        public JsonResult Submit(GiaiDauActionRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(bus.GuiPheDuyet(userId.Value, request.ma_giai_dau));
        }

        [HttpPost]
        public JsonResult Approve(GiaiDauActionRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(bus.PheDuyet(userId.Value, request.ma_giai_dau));
        }

        [HttpPost]
        public JsonResult Reject(TuChoiGiaiDauRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(bus.TuChoi(userId.Value, request));
        }

        [HttpPost]
        public JsonResult OpenRegistration(GiaiDauActionRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(bus.MoDangKy(userId.Value, request.ma_giai_dau));
        }

        [HttpPost]
        public JsonResult CloseRegistration(GiaiDauActionRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(bus.DongDangKy(userId.Value, request.ma_giai_dau));
        }

        [HttpPost]
        public JsonResult ReopenRegistration(GiaiDauActionRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(bus.MoLaiDangKy(userId.Value, request.ma_giai_dau));
        }

        [HttpPost]
        public JsonResult Start(GiaiDauActionRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(bus.KhoiTranh(userId.Value, request.ma_giai_dau));
        }

        [HttpPost]
        public JsonResult Complete(GiaiDauActionRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(bus.BeMac(userId.Value, request.ma_giai_dau));
        }

        [HttpPost]
        public JsonResult Cancel(GiaiDauActionRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(bus.HuyGiaiDau(userId.Value, request.ma_giai_dau));
        }

        [HttpGet]
        public JsonResult Mine()
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap(), JsonRequestBehavior.AllowGet);
            return Json(bus.LayGiaiDauCuaToi(userId.Value), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Detail(int maGiaiDau)
        {
            return Json(bus.LayChiTiet(maGiaiDau), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult PendingApproval()
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap(), JsonRequestBehavior.AllowGet);
            return Json(bus.LayDanhSachChoPheDuyet(userId.Value), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult All()
        {
            return Json(bus.LayDanhSachPublic(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UploadBanner()
        {
            if (Session["UserId"] == null) return Json(ChuaDangNhap());
            HttpPostedFileBase file = Request.Files["banner"];
            if (file == null || file.ContentLength == 0)
                return Json(new ApiResponseDTO { success = false, message = "Không có file.", data = null });
            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
                return Json(new ApiResponseDTO { success = false, message = "Chỉ hỗ trợ jpg, png, webp.", data = null });
            string folder = Server.MapPath("~/Uploads/Tournaments");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string fileName = Guid.NewGuid().ToString("N") + ext;
            file.SaveAs(Path.Combine(folder, fileName));
            return Json(new ApiResponseDTO { success = true, message = "Tải banner thành công.", data = Url.Content("~/Uploads/Tournaments/" + fileName) });
        }

        [HttpPost]
        public JsonResult RegisterTeam(DangKyGiaiDauRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(bus.DangKyThamGia(userId.Value, request));
        }

        [HttpPost]
        public JsonResult InviteTeam(MoiThamGiaGiaiRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(bus.MoiDoiThamGia(userId.Value, request));
        }

        [HttpPost]
        public JsonResult InviteNhanSu(MoiNhanSuGiaiDauRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(bus.MoiNhanSu(userId.Value, request));
        }

        private ApiResponseDTO ChuaDangNhap()
        {
            return new ApiResponseDTO { success = false, message = "Bạn chưa đăng nhập.", data = null };
        }
    }
}
