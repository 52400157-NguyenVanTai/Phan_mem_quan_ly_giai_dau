using BUS;
using DTO;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace GUI.Controllers
{
    public class DoiApiController : Controller
    {
        private readonly DoiBUS doiBUS = new DoiBUS();

        [HttpGet]
        public JsonResult All(string q, int? maTroChoi)
        {
            int? userId = Session["UserId"] as int?;
            return Json(doiBUS.LayDanhSachDoi(q, maTroChoi, userId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Mine()
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap(), JsonRequestBehavior.AllowGet);
            return Json(doiBUS.LayDoiCuaToi(userId.Value), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Detail(int maDoi)
        {
            try
            {
                int? userId = Session["UserId"] as int?;
                return Json(doiBUS.LayChiTietDoi(maDoi, userId), JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DoiApi.Detail error: " + ex.ToString());
                return Json(new ApiResponseDTO { success = false, message = "Lỗi server: " + ex.Message, data = null }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult TroChoi()
        {
            return Json(doiBUS.LayTroChoi(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ViTri(int? maTroChoi)
        {
            return Json(doiBUS.LayViTri(maTroChoi), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(TaoDoiRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(doiBUS.TaoDoi(userId.Value, request));
        }

        [HttpPost]
        public JsonResult Update(CapNhatDoiRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(doiBUS.CapNhatDoi(userId.Value, request));
        }

        [HttpPost]
        public JsonResult ToggleRecruiting(BatTuyenDungRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(doiBUS.CapNhatTuyenDung(userId.Value, request));
        }

        [HttpPost]
        public JsonResult Invite(MoiThanhVienRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(doiBUS.MoiThanhVien(userId.Value, request));
        }

        [HttpPost]
        public JsonResult Join(XinGiaNhapDoiRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(doiBUS.XinGiaNhap(userId.Value, request));
        }

        [HttpPost]
        public JsonResult SetRole(CapNhatVaiTroThanhVienRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(doiBUS.CapNhatVaiTroThanhVien(userId.Value, request));
        }

        [HttpPost]
        public JsonResult RemoveMember(LoaiThanhVienRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(doiBUS.LoaiThanhVien(userId.Value, request));
        }

        [HttpPost]
        public JsonResult LeaveTeam(RoiDoiRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(doiBUS.RoiDoi(userId.Value, request));
        }

        [HttpPost]
        public JsonResult Delete(int maDoi)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(doiBUS.XoaDoi(userId.Value, maDoi));
        }

        [HttpGet]
        public JsonResult Requests()
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap(), JsonRequestBehavior.AllowGet);
            return Json(doiBUS.LayYeuCau(userId.Value), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult HandleRequest(XuLyYeuCauDoiRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue) return Json(ChuaDangNhap());
            return Json(doiBUS.XuLyYeuCau(userId.Value, request));
        }

        [HttpPost]
        public JsonResult UploadLogo()
        {
            if (Session["UserId"] == null) return Json(ChuaDangNhap());
            HttpPostedFileBase file = Request.Files["logo"];
            if (file == null || file.ContentLength == 0) return Json(new ApiResponseDTO { success = true, message = "Không có logo.", data = null });
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".webp" && extension != ".gif") return Json(new ApiResponseDTO { success = false, message = "Logo chỉ hỗ trợ jpg, png, webp hoặc gif.", data = null });
            string folder = Server.MapPath("~/Uploads/Teams");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string fileName = Guid.NewGuid().ToString("N") + extension;
            string path = Path.Combine(folder, fileName);
            file.SaveAs(path);
            return Json(new ApiResponseDTO { success = true, message = "Tải logo thành công.", data = Url.Content("~/Uploads/Teams/" + fileName) });
        }

        private ApiResponseDTO ChuaDangNhap()
        {
            return new ApiResponseDTO { success = false, message = "Bạn chưa đăng nhập.", data = null };
        }
    }
}
