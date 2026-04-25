using System;
using System.Web.Mvc;
using DAL;
using DTO;

namespace GUI_HTML.Filters
{
    public class RequireTeamRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public RequireTeamRoleAttribute(params string[] roles)
        {
            _roles = roles ?? new string[0];
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            object userIdObj = filterContext.HttpContext.Session["CurrentUserId"];
            if (userIdObj == null)
            {
                filterContext.Result = new JsonResult
                {
                    Data = ServiceResultDTO.Fail("Bạn chưa đăng nhập."),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                return;
            }

            int maNguoiDung = Convert.ToInt32(userIdObj);
            object maNhomObj = filterContext.ActionParameters.ContainsKey("maNhom")
                ? filterContext.ActionParameters["maNhom"]
                : null;

            if (maNhomObj == null)
            {
                filterContext.Result = new JsonResult
                {
                    Data = ServiceResultDTO.Fail("Thiếu tham số mã nhóm để kiểm tra quyền."),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                return;
            }

            int maNhom = Convert.ToInt32(maNhomObj);
            var teamDal = new TeamDAL();

            bool authorized = false;
            foreach (string role in _roles)
            {
                if (teamDal.KiemTraRoleNoiBoTrongNhom(maNguoiDung, maNhom, role))
                {
                    authorized = true;
                    break;
                }
            }

            if (!authorized)
            {
                filterContext.Result = new JsonResult
                {
                    Data = ServiceResultDTO.Fail("Bạn không có quyền nội bộ trong nhóm để thao tác."),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }
    }
}
