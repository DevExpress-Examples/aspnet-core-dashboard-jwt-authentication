using DevExpress.DashboardAspNetCore;
using DevExpress.DashboardWeb;
using Microsoft.AspNetCore.Authorization;

namespace AspNetCoreDashboard.Controllers {
    [Authorize]
    public class CustomDashboardController : DashboardController {
        public CustomDashboardController(DashboardConfigurator configurator)
            : base(configurator) {

        }
    }
}
