using DevExpress.DashboardWeb;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Xml.Linq;

public class CustomDashboardFileStorage : DashboardFileStorage {
    private readonly IHttpContextAccessor сontextAccessor;

    public CustomDashboardFileStorage(IWebHostEnvironment hostingEnvironment, IHttpContextAccessor contextAccessor) 
        : base(hostingEnvironment.ContentRootFileProvider.GetFileInfo("App_Data/Dashboards").PhysicalPath) {
        this.сontextAccessor = contextAccessor;
    }

    protected override XDocument LoadDashboard(string dashboardID) {
        Debug.WriteLine(сontextAccessor.HttpContext.User.Identity.Name);

        return base.LoadDashboard(dashboardID);
    }
}
