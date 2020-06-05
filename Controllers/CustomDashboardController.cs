using DevExpress.DashboardAspNetCore;
using DevExpress.DashboardWeb;
using Microsoft.AspNetCore.Authorization;

namespace ASPNETCore30Dashboard.Controllers {
    [Authorize]
    public class CustomDashboardController : DashboardController {
        public CustomDashboardController(DashboardConfigurator configurator)
            : base(configurator) {

        }
    }
}