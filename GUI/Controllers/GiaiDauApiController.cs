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
        public JsonResult SaveDraft(CapNhatGiaiDauRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(bus.LuuBanNhap(userId.Value, request));
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

        // Xoa han (Hard Delete) — chi danh cho ban nhap (trang_thai = 'nhap')
        // Bản nháp chưa public nên được phép xóa thật khỏi DB
        [HttpPost]
        public JsonResult DeleteDraft(GiaiDauActionRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(bus.XoaBanNhap(userId.Value, request.ma_giai_dau));
        }

        [HttpGet]
        public JsonResult Mine()
        {
            try
            {
                int? userId = Session["UserId"] as int?;
                if (!userId.HasValue) return Json(ChuaDangNhap(), JsonRequestBehavior.AllowGet);
                return Json(bus.LayGiaiDauCuaToi(userId.Value), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(LoiHeThong(ex), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult Detail(int maGiaiDau)
        {
            try
            {
                int? userId = Session["UserId"] as int?;
                return Json(bus.LayChiTiet(maGiaiDau, userId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(LoiHeThong(ex), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult PendingApproval()
        {
            try
            {
                int? userId = Session["UserId"] as int?;
                if (!userId.HasValue) return Json(ChuaDangNhap(), JsonRequestBehavior.AllowGet);
                return Json(bus.LayDanhSachChoPheDuyet(userId.Value), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(LoiHeThong(ex), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult All()
        {
            try
            {
                return Json(bus.LayDanhSachPublic(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(LoiHeThong(ex), JsonRequestBehavior.AllowGet);
            }
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
        private ApiResponseDTO LoiHeThong(Exception ex)
        {
            return new ApiResponseDTO
            {
                success = false,
                message = "Loi he thong khi tai du lieu giai dau: " + ex.Message,
                data = null
            };
        }
    }
}
