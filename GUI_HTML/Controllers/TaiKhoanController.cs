using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BUS;
using DTO;
using static System.Collections.Specialized.BitVector32;


namespace GUI_HTML.Controllers
{
    public class TaiKhoanController : Controller
    {
        private NguoiDungBUS nguoiDungBUS = new NguoiDungBUS();

        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(string tenDangNhap, string matKhau)
        {
            NguoiDungDTO user = nguoiDungBUS.KiemTraDangNhap(tenDangNhap, matKhau);

            if (user != null)
            {
                Session["User"] = user;
                Session["Role"] = user.VaiTro;

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Loi = "Tên đăng nhập hoặc mật khẩu không chính xác!";
                return View();
            }
        }

        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("DangNhap", "TaiKhoan");
        }
    }
}