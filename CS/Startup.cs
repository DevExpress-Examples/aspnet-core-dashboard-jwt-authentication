using ASPNETCore30Dashboard.Models;
using DevExpress.AspNetCore;
using DevExpress.DashboardAspNetCore;
using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;
using DevExpress.DataAccess.Sql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;

namespace ASPNETCore30Dashboard {
    public class Startup {
        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment) {
            Configuration = configuration;
            FileProvider = hostingEnvironment.ContentRootFileProvider;
        }

        public IConfiguration Configuration { get; }
        public IFileProvider FileProvider { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            // Configures services to use the Web Dashboard Control.
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,
                        ValidateAudience = true,
                        ValidAudience = AuthOptions.AUDIENCE,
                        ValidateLifetime = true,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true,
                    };
                });

            services
                .AddDevExpressControls()
                .AddControllersWithViews()
                .AddDefaultDashboardController(configurator => {
                    configurator.SetConnectionStringsProvider(new DashboardConnectionStringsProvider(Configuration));
                    //configurator.SetDashboardStorage(new DashboardFileStorage(FileProvider.GetFileInfo("App_Data/Dashboards").PhysicalPath));
                    configurator.SetDashboardStorage(new CustomDashboardFileStorage(FileProvider.GetFileInfo("App_Data/Dashboards").PhysicalPath));

                    DataSourceInMemoryStorage dataSourceStorage = new DataSourceInMemoryStorage();
                    DashboardSqlDataSource sqlDataSource = new DashboardSqlDataSource("SQL Data Source", "NWindConnectionString");
                    SelectQuery query = SelectQueryFluentBuilder
                        .AddTable("Products")
                        .SelectAllColumnsFromTable()
                        .Build("Products");
                    sqlDataSource.Queries.Add(query);
                    dataSourceStorage.RegisterDataSource("sqlDataSource", sqlDataSource.SaveToXml());
                    configurator.SetDataSourceStorage(dataSourceStorage);

                    configurator.CustomParameters += (s, e) => {
                        e.Parameters.Add(new DashboardParameter("LoggedUser", typeof(string), AppContext.Current.User.Identity.Name));
                    };
                });

            services.AddHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            // Registers the DevExpress middleware.
            app.UseDevExpressControls();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => {
                // Maps the dashboard route.
                //EndpointRouteBuilderExtension.MapDashboardRoute(endpoints, "api/dashboards");
                EndpointRouteBuilderExtension.MapDashboardRoute(endpoints, "CustomDashboard", "CustomDashboard");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Login}/{id?}");
            });

            AppContext.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
        }
    }
}
