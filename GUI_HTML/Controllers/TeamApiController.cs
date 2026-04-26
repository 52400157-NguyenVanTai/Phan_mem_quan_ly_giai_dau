using System.Web.Mvc;
using BUS;
using DTO;
using GUI_HTML.Filters;

namespace GUI_HTML.Controllers
{
    public class TeamApiController : Controller
    {
        private readonly TeamBUS _teamBus = new TeamBUS();

        [HttpPost]
        [RequireLogin]
        public JsonResult TaoDoi(TaoDoiDTO dto, int maTroChoiMacDinh, string tenNhomMacDinh)
        {
            dto.MaNguoiDungTao = (int)Session["CurrentUserId"];
            return Json(_teamBus.TaoDoiVaNhomMacDinh(dto, maTroChoiMacDinh, tenNhomMacDinh), JsonRequestBehavior.AllowGet);
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
        [RequireTeamRole("leader", "captain")]
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

        // Chi tiết đội (public)
        [HttpGet]
        public JsonResult ChiTietDoi(int maDoi)
        {
            return Json(_teamBus.LayChiTietDoi(maDoi), JsonRequestBehavior.AllowGet);
        }

        // Thành viên nhóm (public)
        [HttpGet]
        public JsonResult ThanhVienNhom(int maNhom)
        {
            return Json(_teamBus.LayThanhVienNhom(maNhom), JsonRequestBehavior.AllowGet);
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
        public JsonResult DuyetXinGiaNhap(int maDonXin, bool chapNhan)
        {
            int uid = (int)Session["CurrentUserId"];
            return Json(_teamBus.DuyetXinGiaNhap(uid, maDonXin, chapNhan), JsonRequestBehavior.AllowGet);
        }

        // Danh sách đơn xin
        [HttpGet]
        [RequireLogin]
        public JsonResult DanhSachXinGiaNhap(int maNhom)
        {
            int uid = (int)Session["CurrentUserId"];
            return Json(_teamBus.LayDanhSachXinGiaNhap(uid, maNhom), JsonRequestBehavior.AllowGet);
        }

        // Gửi lời mời
        [HttpPost]
        [RequireLogin]
        public JsonResult GuiLoiMoiGiaNhap(int maDoi, int maNhom, string tenNguoiNhan)
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
