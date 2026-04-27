using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using BUS;
using DAL;
using GUI_HTML.Filters;

namespace GUI_HTML.Controllers
{
    public class UploadApiController : Controller
    {
        private readonly IdentityBUS _identityBus = new IdentityBUS();
        private readonly IdentityDAL _identityDal = new IdentityDAL();

        private static readonly string[] AllowedExts = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        [HttpPost]
        [RequireLogin]
        public JsonResult UploadAvatar()
        {
            try
            {
                HttpPostedFileBase file = Request.Files["avatar"];
                if (file == null || file.ContentLength == 0)
                {
                    return Json(new { Success = false, Message = "Khong co file duoc gui len." });
                }

                if (file.ContentLength > MaxFileSizeBytes)
                {
                    return Json(new { Success = false, Message = "File vuot qua gioi han 5 MB." });
                }

                string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (Array.IndexOf(AllowedExts, ext) < 0)
                {
                    return Json(new { Success = false, Message = "Chi chap nhan file anh jpg, jpeg, png, gif hoac webp." });
                }

                string imgDir = Server.MapPath("~/img/");
                if (!Directory.Exists(imgDir))
                {
                    Directory.CreateDirectory(imgDir);
                }

                string newFileName = Guid.NewGuid().ToString("N") + ext;
                string fullPath = Path.Combine(imgDir, newFileName);
                file.SaveAs(fullPath);

                string avatarUrl = "/img/" + newFileName;
                int maNguoiDung = (int)Session["CurrentUserId"];
                var result = _identityBus.CapNhatAvatarUrl(maNguoiDung, avatarUrl);

                if (!result.Success)
                {
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }

                    return Json(result);
                }

                RefreshCurrentUserSession(maNguoiDung);

                return Json(new
                {
                    Success = true,
                    Message = "Cap nhat avatar thanh cong.",
                    Data = new { AvatarUrl = avatarUrl }
                });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Loi upload: " + ex.Message });
            }
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult UploadBanner()
        {
            try
            {
                HttpPostedFileBase file = Request.Files["file"];
                if (file == null || file.ContentLength == 0)
                {
                    return Json(new { Success = false, Message = "Không có file banner được gửi lên." });
                }

                if (file.ContentLength > MaxFileSizeBytes)
                {
                    return Json(new { Success = false, Message = "Banner vượt quá giới hạn 5 MB." });
                }

                string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (Array.IndexOf(AllowedExts, ext) < 0)
                {
                    return Json(new { Success = false, Message = "Banner chỉ chấp nhận ảnh jpg, jpeg, png, gif hoặc webp." });
                }

                string imgDir = Server.MapPath("~/img/banners/");
                if (!Directory.Exists(imgDir))
                {
                    Directory.CreateDirectory(imgDir);
                }

                string newFileName = "banner_" + Guid.NewGuid().ToString("N") + ext;
                string fullPath = Path.Combine(imgDir, newFileName);
                file.SaveAs(fullPath);

                string bannerUrl = "/img/banners/" + newFileName;
                return Json(new
                {
                    Success = true,
                    Message = "Upload banner thành công.",
                    Data = new { Url = bannerUrl }
                });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Lỗi upload banner: " + ex.Message });
            }
        }

        private void RefreshCurrentUserSession(int maNguoiDung)
        {
            var user = _identityDal.LayTheoId(maNguoiDung);
            if (user == null)
            {
                return;
            }

            Session["CurrentUser"] = new
            {
                user.MaNguoiDung,
                user.TenDangNhap,
                user.Email,
                user.VaiTroHeThong,
                user.AvatarUrl,
                user.Bio
            };
        }
    }
}
