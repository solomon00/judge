using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Solomon.WebUI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Matches /Tournament/1/RegisterForTournament
            routes.MapRoute(
                "RegisterForTournament",
                url: "Tournament/{TournamentID}/Register",
                defaults: new { controller = "Tournament", action = "Register" },
                constraints: new { TournamentID = @"\d+" },
                namespaces: new[] { "Solomon.WebUI.Controllers" }
            );

            // Matches /Tournament/1/Results
            routes.MapRoute(
                "Results",
                url: "Tournament/{TournamentID}/Results",
                defaults: new { controller = "Tournament", action = "Results" },
                constraints: new { TournamentID = @"\d+" },
                namespaces: new[] { "Solomon.WebUI.Controllers" }
            );

            // Matches /Tournament/1/Statistic
            routes.MapRoute(
                "Statistic",
                url: "Tournament/{TournamentID}/Statistic",
                defaults: new { controller = "Tournament", action = "Statistic" },
                constraints: new { TournamentID = @"\d+" },
                namespaces: new[] { "Solomon.WebUI.Controllers" }
            );

            // Matches /Tournament/1/GetSolutionFile/1
            routes.MapRoute(
                "SolutionFile",
                url: "Tournament/{TournamentID}/GetSolutionFile/{SolutionID}",
                defaults: new { controller = "Problem", action = "GetSolutionFile" },
                constraints: new { TournamentID = @"\d+" },
                namespaces: new[] { "Solomon.WebUI.Controllers" }
            );

            // Matches /Tournament/1/Problem/1/Comments
            routes.MapRoute(
                "Comments",
                url: "Tournament/{TournamentID}/Problem/{ProblemID}/Comments",
                defaults: new { controller = "Problem", action = "Comments" },
                constraints: new { TournamentID = @"\d+" },
                namespaces: new[] { "Solomon.WebUI.Controllers" }
            );

            // Matches /Tournament/1/Problem/1
            routes.MapRoute(
                "Problem",
                url: "Tournament/{TournamentID}/{action}/{ProblemID}",
                defaults: new { controller = "Problem", action = "Problem", ProblemID = UrlParameter.Optional },
                constraints: new { TournamentID = @"\d+" },
                namespaces: new[] { "Solomon.WebUI.Controllers" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] {"Solomon.WebUI.Controllers"}
            );
        }
    }
}