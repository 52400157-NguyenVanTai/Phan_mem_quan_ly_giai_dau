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
    }
}
