# ASP.NET Core Dashboard - How to implement authentication

This example demonstrates how to implement authentication based on [JWT](https://developer.okta.com/blog/2018/03/23/token-authentication-aspnetcore-complete-guide).

We create an [AccountController](CS/Controllers/AccountController.cs) to generate JWT tokens for the predefined set of users. Once the token is generated, we save it to [sessionStorage](https://www.w3schools.com/jsref/prop_win_sessionstorage.asp) in the [Login](Views/Home/Login.cshtml) view.

The [Dashboard](Views/Home/Dashboard.cshtml) view passes this token to the [CustomDashboardController](CS/Controllers/CustomDashboardController.cs) (it is marked with the [AuthorizeAttribute](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authorization.authorizeattribute?view=aspnetcore-3.1)) by using the [AjaxRemoteService.beforeSend](https://docs.devexpress.com/Dashboard/js-DevExpress.Dashboard.AjaxRemoteService?p=netframework#js_devexpress_dashboard_ajaxremoteservice_beforesend) callback function:

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

Main JWT and Dashboard configurations are defined in the [CS/Startup.cs](Startup.cs) file. We use the [IHttpContextAccessor](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-context?view=aspnetcore-3.0) with dependency injection to access the current user name (`HttpContext.User.Identity.Name`) in code. Note that you can access it in [DashboardConfigurator](https://docs.devexpress.com/Dashboard/DevExpress.DashboardWeb.DashboardConfigurator?p=netframework) events and Dashboard storages. Here are corresponding code parts:

```cs
// Startup.cs:
var contextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
configurator.CustomParameters += (s, e) => {
    e.Parameters.Add(new DashboardParameter("LoggedUser", typeof(string), contextAccessor.HttpContext.User.Identity.Name));
};
...
// CustomDashboardStorage.cs:
protected override XDocument LoadDashboard(string dashboardID) {
    Debug.WriteLine(сontextAccessor.HttpContext.User.Identity.Name);
    return base.LoadDashboard(dashboardID);
}
```

If you open the [Dashboard](CS/Views/Home/Dashboard.cshtml) view without logging in, you will see the following error:

![](img/auth_error.png)

## See Also

- [T590909 - Web Dashboard - How to load dashboards based on user roles](https://supportcenter.devexpress.com/ticket/details/t590909/web-dashboard-how-to-load-dashboards-based-on-user-roles)
- [T954359 - MVC Dashboard - How to implement multi-tenant Dashboard architecture](https://supportcenter.devexpress.com/ticket/details/t954359/mvc-dashboard-how-to-implement-multi-tenant-dashboard-architecture)
- [T983227 - ASP.NET Core Dashboard - How to implement multi-tenant Dashboard architecture](https://supportcenter.devexpress.com/ticket/details/t983227/asp-net-core-dashboard-how-to-implement-multi-tenant-dashboard-architecture)
