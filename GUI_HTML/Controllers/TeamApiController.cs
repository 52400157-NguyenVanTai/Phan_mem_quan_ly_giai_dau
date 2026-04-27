using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BUS;
using DTO;
using GUI_HTML.Filters;

namespace GUI_HTML.Controllers
{
    public class TeamApiController : Controller
    {
        private readonly TeamBUS _teamBus = new TeamBUS();

        private int GetCurrentUser()
        {
            if (Session["CurrentUserId"] == null)
                return 0;
            return (int)Session["CurrentUserId"];
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult TaoDoi(TaoDoiDTO dto, string Squads)
        {
            dto.MaNguoiDungTao = (int)Session["CurrentUserId"];
            // Allow team name reuse if the old team has no active members
            var existingTeam = _teamBus.LayDoiCuaToi(dto.MaNguoiDungTao);
            if (existingTeam.Success && existingTeam.Data != null)
            {
                // User already has a team, delete it first if they want to recreate
                int maDoiCu = ((dynamic)existingTeam.Data).ma_doi;
                _teamBus.XoaDoi(dto.MaNguoiDungTao, maDoiCu);
            }
            return Json(_teamBus.TaoDoiVaNhieuNhom(dto, Squads), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult TaoNhom(int maDoi, int maTroChoi, string tenNhom, int maCaptain)
        {
            int maNguoiTao = (int)Session["CurrentUserId"];
            return Json(_teamBus.TaoNhom(maNguoiTao, maDoi, maTroChoi, tenNhom, maCaptain), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        [RequireTeamRole("chu_tich", "ban_dieu_hanh", "doi_truong")]
        public JsonResult ThemThanhVien(int maNguoiDung, int maNhom, int maTroChoiNhom, int maViTri, string phanHe)
        {
            int maNguoiThucHien = (int)Session["CurrentUserId"];
            return Json(_teamBus.ThemThanhVienVaoNhom(maNguoiThucHien, maNguoiDung, maNhom, maTroChoiNhom, maViTri, phanHe), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult GiaiTanDoi(int maDoi)
        {
            int maNguoiThucHien = (int)Session["CurrentUserId"];
            return Json(_teamBus.GiaiTanDoi(maNguoiThucHien, maDoi), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult XoaDoi(int maDoi)
        {
            int uid = (int)Session["CurrentUserId"];
            return Json(_teamBus.XoaDoi(uid, maDoi), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult XoaNhom(int maDoi, int maNhom)
        {
            int uid = (int)Session["CurrentUserId"];
            return Json(_teamBus.XoaNhom(uid, maDoi, maNhom), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [RequireLogin]
        public JsonResult DoiCuaToi()
        {
            int maNguoiDung = (int)Session["CurrentUserId"];
            return Json(_teamBus.LayDoiCuaToi(maNguoiDung), JsonRequestBehavior.AllowGet);
        }

        // Tất cả đội+nhóm
        [HttpGet]
        [RequireLogin]
        public JsonResult TatCaDoiCuaToi()
        {
            int uid = (int)Session["CurrentUserId"];
            return Json(_teamBus.LayTatCaDoiCuaToi(uid), JsonRequestBehavior.AllowGet);
        }

        // Team Explorer (public)
        [HttpGet]
        public JsonResult DanhSachDoiCongKhai(int? maTroChoi, bool? dangTuyen, string tuKhoa)
        {
            return Json(_teamBus.LayDanhSachDoiCongKhai(maTroChoi, dangTuyen, tuKhoa), JsonRequestBehavior.AllowGet);
        }

        // Chi tiết đội (public, nhưng trả thêm vai_tro_hien_tai nếu đã đăng nhập)
        [HttpGet]
        public JsonResult ChiTietDoi(int maDoi)
        {
            int uid = Session["CurrentUserId"] != null ? (int)Session["CurrentUserId"] : 0;
            return Json(_teamBus.LayChiTietDoi(maDoi, uid), JsonRequestBehavior.AllowGet);
        }

        // Thành viên nhóm (public)
        [HttpGet]
        public JsonResult ThanhVienNhom(int maNhom)
        {
            return Json(_teamBus.LayThanhVienNhom(maNhom), JsonRequestBehavior.AllowGet);
        }

        // Thành viên nhóm quản lý (chairman/leader only)
        [HttpGet]
        [RequireLogin]
        public JsonResult ThanhVienNhomQuanLy(int maDoi)
        {
            int uid = (int)Session["CurrentUserId"];
            return Json(_teamBus.LayThanhVienNhomQuanLy(uid, maDoi), JsonRequestBehavior.AllowGet);
        }

        // Xin gia nhập
        [HttpPost]
        [RequireLogin]
        public JsonResult XinGiaNhap(int maNhom, int? maHoSo)
        {
            int uid = (int)Session["CurrentUserId"];
            return Json(_teamBus.XinGiaNhap(uid, maNhom, maHoSo), JsonRequestBehavior.AllowGet);
        }

        // Duyệt đơn xin
        [HttpPost]
        [RequireLogin]
        public JsonResult DuyetYeuCauThamGiaNhom(int maYeuCau, bool chapNhan)
        {
            int maNguoiDung = GetCurrentUser();
            var result = _teamBus.DuyetYeuCauThamGiaNhom(maNguoiDung, maYeuCau, chapNhan);
            return Json(result);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult CapNhatDoiTruongNhom(int maNhom, int maDoiTruongMoi)
        {
            int maNguoiDung = GetCurrentUser();
            return Json(_teamBus.CapNhatDoiTruongNhom(maNguoiDung, maNhom, maDoiTruongMoi));
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult CapNhatThongTinDoi(int maDoi, string tenDoi, string tenVietTat, string logoUrl, string slogan)
        {
            int maNguoiDung = GetCurrentUser();
            var result = _teamBus.CapNhatThongTinDoi(maNguoiDung, maDoi, tenDoi, tenVietTat, logoUrl, slogan);
            return Json(result);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult CapNhatDangTuyen(int maDoi, bool dangTuyen)
        {
            int maNguoiDung = GetCurrentUser();
            var result = _teamBus.CapNhatDangTuyen(maNguoiDung, maDoi, dangTuyen);
            return Json(result);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult CapNhatLogo(int maDoi)
        {
            int maNguoiDung = GetCurrentUser();
            HttpPostedFileBase logoFile = Request.Files["logo"];
            if (logoFile == null || logoFile.ContentLength == 0)
                return Json(ServiceResultDTO.Fail("Vui lòng chọn file logo."));

            // Validate file type
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            string fileExtension = System.IO.Path.GetExtension(logoFile.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
                return Json(ServiceResultDTO.Fail("Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif)."));

            // Save file to server
            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string uploadPath = Server.MapPath("~/img/");
            if (!System.IO.Directory.Exists(uploadPath))
                System.IO.Directory.CreateDirectory(uploadPath);

            string filePath = System.IO.Path.Combine(uploadPath, fileName);
            logoFile.SaveAs(filePath);

            // Update database
            string logoUrl = "/img/" + fileName;
            var result = _teamBus.CapNhatLogo(maNguoiDung, maDoi, logoUrl);
            return Json(result);
        }

        // Danh sách đơn xin
        [HttpPost]
        [RequireLogin]
        public JsonResult DanhSachXinGiaNhap(int maNhom)
        {
            int uid = (int)Session["CurrentUserId"];
            return Json(_teamBus.LayDanhSachXinGiaNhap(uid, maNhom), JsonRequestBehavior.AllowGet);
        }

        // Gửi lời mời
        [HttpPost]
        [RequireLogin]
        public JsonResult GuiLoiMoiGiaNhap(int maDoi, int? maNhom, string tenNguoiNhan)
        {
            int uid = (int)Session["CurrentUserId"];
            return Json(_teamBus.GuiLoiMoi(uid, maDoi, maNhom, tenNguoiNhan), JsonRequestBehavior.AllowGet);
        }

        // RBAC: cập nhật vai trò
        [HttpPost]
        [RequireLogin]
        public JsonResult CapNhatVaiTro(int maThanhVien, string vaiTroMoi)
        {
            int uid = (int)Session["CurrentUserId"];
            return Json(_teamBus.CapNhatVaiTro(uid, maThanhVien, vaiTroMoi), JsonRequestBehavior.AllowGet);
        }

        // Toggle tuyển dụng
        [HttpPost]
        [RequireLogin]
        public JsonResult ToggleDangTuyen(int maDoi, bool dangTuyen)
        {
            int uid = (int)Session["CurrentUserId"];
            return Json(_teamBus.ToggleDangTuyen(uid, maDoi, dangTuyen), JsonRequestBehavior.AllowGet);
        }

        // Thông báo
        [HttpGet]
        [RequireLogin]
        public JsonResult ThongBao()
        {
            int uid = (int)Session["CurrentUserId"];
            return Json(_teamBus.LayThongBao(uid), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult DanhDauDaDoc(int maThongBao)
        {
            int uid = (int)Session["CurrentUserId"];
            return Json(_teamBus.DanhDauDaDoc(uid, maThongBao), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireLogin]
        public JsonResult DanhDauTatCaDaDoc()
        {
            int uid = (int)Session["CurrentUserId"];
            return Json(_teamBus.DanhDauTatCaDaDoc(uid), JsonRequestBehavior.AllowGet);
        }

        // Tìm kiếm toàn cục
        [HttpGet]
        public JsonResult TimKiem(string q)
        {
            return Json(_teamBus.TimKiem(q), JsonRequestBehavior.AllowGet);
        }

        // Dashboard data
        [HttpGet]
        public JsonResult GiaiNoiBat()
        {
            return Json(_teamBus.LayGiaiNoiBat(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GiaiSapBatDau()
        {
            return Json(_teamBus.LayGiaiSapBatDau(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GiaiDangMoDangKy()
        {
            return Json(_teamBus.LayGiaiDangMoDangKy(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GiaiTheoGame(int maTroChoi, string trangThai)
        {
            return Json(_teamBus.LayGiaiTheoGame(maTroChoi, trangThai), JsonRequestBehavior.AllowGet);
        }

        // Giải đã tham gia
        [HttpGet]
        [RequireLogin]
        public JsonResult GiaiDaThamGia()
        {
            int uid = (int)Session["CurrentUserId"];
            return Json(_teamBus.LayGiaiDaThamGia(uid), JsonRequestBehavior.AllowGet);
        }

        // Giải đang theo dõi
        [HttpGet]
        [RequireLogin]
        public JsonResult GiaiDangTheoDoi()
        {
            int uid = (int)Session["CurrentUserId"];
            return Json(_teamBus.LayGiaiDangTheoDoi(uid), JsonRequestBehavior.AllowGet);
        }
    }
}
