using System;
using System.Web.Mvc;
using DTO;

namespace GUI_HTML.Filters
{
    public class RequireSystemRoleAttribute : ActionFilterAttribute
    {
        private readonly string _role;

        public RequireSystemRoleAttribute(string role)
        {
            _role = role;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            object roleObj = filterContext.HttpContext.Session["SystemRole"];
            if (roleObj == null || !string.Equals(roleObj.ToString(), _role, StringComparison.OrdinalIgnoreCase))
            {
                filterContext.Result = new JsonResult
                {
                    Data = ServiceResultDTO.Fail("Bạn không có quyền hệ thống để thực hiện thao tác này."),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }
    }
}
