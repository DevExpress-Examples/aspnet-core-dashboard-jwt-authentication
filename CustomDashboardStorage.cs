using ASPNETCore30Dashboard;
using DevExpress.DashboardWeb;
using System.Diagnostics;
using System.Xml.Linq;

public class CustomDashboardFileStorage : DashboardFileStorage {
    public CustomDashboardFileStorage(string workingDirectory) : base(workingDirectory) {

    }

    protected override XDocument LoadDashboard(string dashboardID) {
        Debug.WriteLine(AppContext.Current.User.Identity.Name);

        return base.LoadDashboard(dashboardID);
    }
}