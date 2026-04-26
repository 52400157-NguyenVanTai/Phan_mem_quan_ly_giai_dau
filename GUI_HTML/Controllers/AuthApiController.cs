using System.Web.Mvc;
using BUS;
using DTO;
using DAL;

namespace GUI_HTML.Controllers
{
    public class AuthApiController : Controller
    {
        private readonly IdentityBUS _identityBus = new IdentityBUS();
        private readonly IdentityDAL _identityDal = new IdentityDAL();

        [HttpPost]
        public JsonResult DangKy(DangKyNguoiDungDTO dto)
        {
            ServiceResultDTO result = _identityBus.DangKy(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DangNhap(DangNhapDTO dto)
        {
            ServiceResultDTO result = _identityBus.DangNhap(dto);
            if (result.Success)
            {
                var user = _identityDal.LayTheoDinhDanh(dto.DinhDanh);
                UpdateCurrentUserSession(user);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DangXuat()
        {
            Session.Clear();
            return Json(ServiceResultDTO.Ok("Đăng xuất thành công."), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DoiMatKhau(CapNhatMatKhauDTO dto)
        {
            if (Session["CurrentUserId"] == null)
            {
                return Json(ServiceResultDTO.Fail("Bạn chưa đăng nhập."), JsonRequestBehavior.AllowGet);
            }

            dto.MaNguoiDung = (int)Session["CurrentUserId"];
            ServiceResultDTO result = _identityBus.DoiMatKhau(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CapNhatThongTin(CapNhatThongTinCoBanDTO dto)
        {
            if (Session["CurrentUserId"] == null)
            {
                return Json(ServiceResultDTO.Fail("Bạn chưa đăng nhập."), JsonRequestBehavior.AllowGet);
            }

            dto.MaNguoiDung = (int)Session["CurrentUserId"];
            ServiceResultDTO result = _identityBus.CapNhatThongTin(dto);
            if (result.Success)
            {
                UpdateCurrentUserSession(_identityDal.LayTheoId(dto.MaNguoiDung));
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Me()
        {
            if (Session["CurrentUser"] == null)
            {
                return Json(ServiceResultDTO.Fail("Chưa đăng nhập."), JsonRequestBehavior.AllowGet);
            }
            return Json(ServiceResultDTO.Ok("OK", Session["CurrentUser"]), JsonRequestBehavior.AllowGet);
        }

        private void UpdateCurrentUserSession(NguoiDungDTO user)
        {
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
            Session["CurrentUserId"] = user.MaNguoiDung;
            Session["SystemRole"] = user.VaiTroHeThong;
        }
    }
}
