using BUS;
using DTO;
using System;
using System.Web.Mvc;

namespace GUI_HTML.Controllers
{
    public class TaiKhoanController : Controller
    {
        private NguoiDungBUS _nguoiDungBUS = new NguoiDungBUS();

        // GET: /TaiKhoan/DangNhap — Hiển thị form
        [HttpGet]
        public ActionResult DangNhap()
        {
            if (Session["NguoiDung"] != null)
                return RedirectToAction("Index", "Portal");

            return View();
        }

        // POST: /TaiKhoan/ApiDangNhap — Trả JSON cho JS gọi
        [HttpPost]
        public JsonResult ApiDangNhap(string tenDangNhap, string matKhau)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
            {
                return Json(new { success = false, code = "EMPTY_INPUT", message = "Vui lòng nhập đầy đủ thông tin." });
            }

            try
            {
                NguoiDungDTO nguoiDung = _nguoiDungBUS.KiemTraDangNhap(tenDangNhap, matKhau);

                if (nguoiDung == null)
                {
                    return Json(new { success = false, code = "WRONG_CREDENTIAL", message = "Tên đăng nhập hoặc mật khẩu không chính xác." });
                }

                if (nguoiDung.IsBanned)
                {
                    return Json(new { success = false, code = "ACCOUNT_BANNED", message = "Tài khoản đã bị khóa. Vui lòng liên hệ Admin." });
                }

                // Lưu Session
                Session["NguoiDung"] = nguoiDung;
                Session["MaNguoiDung"] = nguoiDung.MaNguoiDung;
                Session["TenDangNhap"] = nguoiDung.TenDangNhap;
                Session["VaiTro"] = nguoiDung.VaiTroHeThong;

                string redirectUrl = nguoiDung.VaiTroHeThong == "admin"
                    ? Url.Action("Index", "Admin")
                    : Url.Action("Index", "Portal");

                return Json(new
                {
                    success = true,
                    code = "LOGIN_OK",
                    message = "Đăng nhập thành công!",
                    redirectUrl = redirectUrl,
                    user = new
                    {
                        ma = nguoiDung.MaNguoiDung,
                        ten = nguoiDung.TenDangNhap,
                        email = nguoiDung.Email,
                        vaiTro = nguoiDung.VaiTroHeThong
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[LOGIN ERROR] " + ex.Message);
                return Json(new
                {
                    success = false,
                    code = "SERVER_ERROR",
                    message = "Lỗi hệ thống.",
                    debug = ex.Message   // ⚠️ Xóa dòng này khi production
                });
            }
        }

        // GET: /TaiKhoan/DangXuat
        public ActionResult DangXuat()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("DangNhap");
        }
    }
}