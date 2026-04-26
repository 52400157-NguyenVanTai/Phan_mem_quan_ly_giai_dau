using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GUI_HTML
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // /login → Portal/Login (auth page)
            routes.MapRoute(
                name: "Login",
                url: "login",
                defaults: new { controller = "Portal", action = "Login" }
            );

            // /dashboard → Portal/Index (private dashboard)
            routes.MapRoute(
                name: "Dashboard",
                url: "dashboard",
                defaults: new { controller = "Portal", action = "Index" }
            );

            // /giai/{id} → public tournament detail
            routes.MapRoute(
                name: "TournamentPublic",
                url: "giai/{id}",
                defaults: new { controller = "TournamentPublic", action = "Index", id = UrlParameter.Optional }
            );

            // / → Home/Index (public landing)
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
