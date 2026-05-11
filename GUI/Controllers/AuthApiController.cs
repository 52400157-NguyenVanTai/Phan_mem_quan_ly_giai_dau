using BUS;
using DTO;
using System;
using System.IO;
using System.Web.Mvc;

namespace GUI.Controllers
{
    public class AuthApiController : Controller
    {
        private readonly NguoidungBUS nguoidungBUS = new NguoidungBUS();

        [HttpGet]
        public JsonResult Search(string keyword)
        {
            return Json(nguoidungBUS.TimKiemNguoiDung(keyword), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CurrentUser()
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return Json(new ApiResponseDTO { success = false, message = "Bạn chưa đăng nhập.", data = null }, JsonRequestBehavior.AllowGet);
            }

            return Json(nguoidungBUS.LayHoSo(userId.Value), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Login(LoginRequestDTO request)
        {
            ApiResponseDTO response = nguoidungBUS.DangNhap(request);
            if (response.success)
            {
                NguoidungDTO user = response.data as NguoidungDTO;
                Session["UserId"] = user.ma_nguoi_dung;
                Session["Username"] = user.ten_dang_nhap;
                Session["Role"] = user.vai_tro_he_thong;
            }

            return Json(response);
        }

        [HttpPost]
        public JsonResult Register(RegisterRequestDTO request)
        {
            ApiResponseDTO response = nguoidungBUS.DangKy(request);
            return Json(response);
        }

        [HttpPost]
        public JsonResult UpdateProfile(UpdateProfileRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return Json(new ApiResponseDTO { success = false, message = "Bạn chưa đăng nhập.", data = null });
            }

            ApiResponseDTO response = nguoidungBUS.CapNhatHoSo(userId.Value, request);
            if (response.success)
            {
                NguoidungDTO user = response.data as NguoidungDTO;
                Session["Username"] = user.ten_dang_nhap;
            }

            return Json(response);
        }

        [HttpPost]
        public JsonResult UploadAvatar()
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return Json(new ApiResponseDTO { success = false, message = "Bạn chưa đăng nhập.", data = null });
            }

            if (Request.Files.Count == 0 || Request.Files["avatar"] == null)
            {
                return Json(new ApiResponseDTO { success = false, message = "Vui lòng chọn ảnh avatar.", data = null });
            }

            var file = Request.Files["avatar"];
            if (file.ContentLength <= 0)
            {
                return Json(new ApiResponseDTO { success = false, message = "File ảnh không hợp lệ.", data = null });
            }

            if (file.ContentLength > 2 * 1024 * 1024)
            {
                return Json(new ApiResponseDTO { success = false, message = "Ảnh avatar không được lớn hơn 2MB.", data = null });
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".webp")
            {
                return Json(new ApiResponseDTO { success = false, message = "Chỉ hỗ trợ ảnh JPG, PNG hoặc WEBP.", data = null });
            }

            string folderPath = Server.MapPath("~/Uploads/Avatars");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = "avatar_" + userId.Value + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + extension;
            string fullPath = Path.Combine(folderPath, fileName);
            file.SaveAs(fullPath);

            string avatarUrl = Url.Content("~/Uploads/Avatars/" + fileName);
            return Json(new ApiResponseDTO { success = true, message = "Tải ảnh avatar thành công.", data = avatarUrl });
        }

        [HttpPost]
        public JsonResult ChangePassword(ChangePasswordRequestDTO request)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return Json(new ApiResponseDTO { success = false, message = "Bạn chưa đăng nhập.", data = null });
            }

            return Json(nguoidungBUS.DoiMatKhau(userId.Value, request));
        }

        [HttpPost]
        public JsonResult ForgotPassword(ForgotPasswordRequestDTO request)
        {
            return Json(nguoidungBUS.QuenMatKhau(request));
        }

        [HttpPost]
        public JsonResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return Json(new ApiResponseDTO { success = true, message = "Đăng xuất thành công.", data = null });
        }
    }
}
