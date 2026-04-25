using System.Web.Mvc;
using DTO;

namespace GUI_HTML.Filters
{
    public class RequireLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["CurrentUserId"] == null)
            {
                filterContext.Result = new JsonResult
                {
                    Data = ServiceResultDTO.Fail("Bạn chưa đăng nhập."),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }
    }
}
