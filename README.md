# ASP.NET Core Dashboard - How to implement authentication

This example demonstrates how to implement authentication based on [JWT](https://developer.okta.com/blog/2018/03/23/token-authentication-aspnetcore-complete-guide).

For this we create the `AccountController` to generate JWT tokens for predefined set of users. Once the token is generated we save it to the [sessionStorage](https://www.w3schools.com/jsref/prop_win_sessionstorage.asp) in the `Login` view.

The `Dashboard` view passes this token to the `CustomDashboardController` (it is marked with the [AuthorizeAttribute](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authorization.authorizeattribute?view=aspnetcore-3.1)) by using the [AjaxRemoteService.beforeSend](https://docs.devexpress.com/Dashboard/js-DevExpress.Dashboard.AjaxRemoteService?p=netframework#js_devexpress_dashboard_ajaxremoteservice_beforesend) callback function:

```js
const tokenKey = "accessToken";
function onBeforeRender(sender) {
    var dashboardControl = sender;
    const token = sessionStorage.getItem(tokenKey);
    dashboardControl.remoteService.beforeSend = function (jqXHR, settings) {
        jqXHR.setRequestHeader("Authorization", "Bearer " + token);
    }
}
```

Main JWT and Dashboard configurations are defined in the `Startup.cs` file. We use the technique from the [A better approach to use HttpContext outside a Controller in .Net Core 2.1 - Quick Dev Notes](https://www.quickdevnotes.com/better-approach-to-use-httpcontext-outside-a-controller-in-net-core-2-1/) webpage to access the current user name (`AppContext.Current.User.Identity.Name`) in code. Note that you can use it in [DashboardConfigurator](https://docs.devexpress.com/Dashboard/DevExpress.DashboardWeb.DashboardConfigurator?p=netframework) events and Dashboard storages. Here are the corresponding code parts:

```cs
// Startup.cs:
configurator.CustomParameters += (s, e) => {
    e.Parameters.Add(new DashboardParameter("LoggedUser", typeof(string), AppContext.Current.User.Identity.Name));
};
...
// CustomDashboardStorage.cs
protected override XDocument LoadDashboard(string dashboardID) {
    Debug.WriteLine(AppContext.Current.User.Identity.Name);
    return base.LoadDashboard(dashboardID);
}
```

If you open the `Dashboard` view without logging in, you will see the following error:

![](auth_error.png)

## See Also

- [T590909 - Web Dashboard - How to load dashboards based on user roles](https://supportcenter.devexpress.com/ticket/details/t590909/web-dashboard-how-to-load-dashboards-based-on-user-roles)