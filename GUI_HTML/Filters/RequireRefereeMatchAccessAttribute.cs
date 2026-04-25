using System;
using System.Diagnostics;
using System.Web.Mvc;
using BUS;
using DTO;

namespace GUI_HTML.Filters
{
    public class RequireRefereeMatchAccessAttribute : ActionFilterAttribute
    {
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
            int maTran = LayMaTran(filterContext);
            if (maTran <= 0)
            {
                filterContext.Result = new JsonResult
                {
                    Data = ServiceResultDTO.Fail("Thiếu mã trận để kiểm tra thẩm quyền trọng tài."),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                return;
            }

            bool authorized = new RefereeBUS().KiemTraTrongTaiDuocPhep(maTran, maNguoiDung);
            if (authorized)
            {
                return;
            }

            Trace.TraceWarning(
                "[SECURITY] Referee forbidden access. UserId={0}, MaTran={1}, Action={2}, Ip={3}",
                maNguoiDung,
                maTran,
                filterContext.ActionDescriptor.ActionName,
                filterContext.HttpContext.Request.UserHostAddress
            );

            filterContext.HttpContext.Response.StatusCode = 403;
            filterContext.Result = new JsonResult
            {
                Data = ServiceResultDTO.Fail("403 Forbidden: Bạn không được phép thao tác trận đấu này."),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        private static int LayMaTran(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionParameters.ContainsKey("maTran") && filterContext.ActionParameters["maTran"] != null)
            {
                return Convert.ToInt32(filterContext.ActionParameters["maTran"]);
            }

            if (filterContext.ActionParameters.ContainsKey("dto") && filterContext.ActionParameters["dto"] != null)
            {
                object dto = filterContext.ActionParameters["dto"];
                var prop = dto.GetType().GetProperty("MaTran");
                if (prop != null)
                {
                    object value = prop.GetValue(dto, null);
                    if (value != null)
                    {
                        return Convert.ToInt32(value);
                    }
                }
            }

            foreach (var kv in filterContext.ActionParameters)
            {
                if (kv.Value == null)
                {
                    continue;
                }

                var prop = kv.Value.GetType().GetProperty("MaTran");
                if (prop == null)
                {
                    continue;
                }

                object value = prop.GetValue(kv.Value, null);
                if (value != null)
                {
                    return Convert.ToInt32(value);
                }
            }

            return 0;
        }
    }
}
