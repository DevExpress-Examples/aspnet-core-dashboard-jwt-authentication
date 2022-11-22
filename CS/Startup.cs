using AspNetCoreDashboard.Models;
using DevExpress.AspNetCore;
using DevExpress.DashboardAspNetCore;
using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;
using DevExpress.DataAccess.Sql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Net;
using System.Linq;

namespace AspNetCoreDashboard {
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
                    options.Events = new JwtBearerEvents() {
                        OnMessageReceived = async context => {
                            // Pass authentication token from the FormData to the context on the export action
                            if (string.IsNullOrEmpty(context.Token) && context.Request.HasFormContentType) {
                                var formData = await context.Request.ReadFormAsync();
                                var accessToken = formData?["Authorization"];
                                var path = context.HttpContext.Request.Path;
                                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/CustomDashboard")) {
                                    context.Token = WebUtility.UrlDecode(accessToken).Replace("Bearer ", "");
                                }
                            }
                        }
                    };
                });

            services
                .AddDevExpressControls()
                .AddControllersWithViews()
				.ConfigureApplicationPartManager((manager) => {
					var dashboardApplicationParts = manager.ApplicationParts.Where(part => 
						part is AssemblyPart && ((AssemblyPart)part).Assembly == typeof(DashboardController).Assembly).ToList();
					foreach(var partToRemove in dashboardApplicationParts) {
					  manager.ApplicationParts.Remove(partToRemove);
					}
				});
            services.AddScoped<DashboardConfigurator>((IServiceProvider serviceProvider) => {
                DashboardConfigurator configurator = new DashboardConfigurator();
                configurator.SetConnectionStringsProvider(new DashboardConnectionStringsProvider(Configuration));
                    //configurator.SetDashboardStorage(new DashboardFileStorage(FileProvider.GetFileInfo("App_Data/Dashboards").PhysicalPath));
                    configurator.SetDashboardStorage(serviceProvider.GetService<CustomDashboardFileStorage>());

                    DataSourceInMemoryStorage dataSourceStorage = new DataSourceInMemoryStorage();
                    DashboardSqlDataSource sqlDataSource = new DashboardSqlDataSource("SQL Data Source", "NWindConnectionString");
                    SelectQuery query = SelectQueryFluentBuilder
                        .AddTable("Products")
                        .SelectAllColumnsFromTable()
                        .Build("Products");
                    sqlDataSource.Queries.Add(query);
                    dataSourceStorage.RegisterDataSource("sqlDataSource", sqlDataSource.SaveToXml());
                    configurator.SetDataSourceStorage(dataSourceStorage);

                    var contextAccessor = serviceProvider.GetService<IHttpContextAccessor>();

                    configurator.CustomParameters += (s, e) => {
                        e.Parameters.Add(new DashboardParameter("LoggedUser", typeof(string), contextAccessor.HttpContext.User.Identity.Name));
                    };
                return configurator;
                });

            services.AddHttpContextAccessor();
            services.AddTransient<CustomDashboardFileStorage>();
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
                //endpoints.MapDashboardRoute("api/dashboards");
                endpoints.MapDashboardRoute("CustomDashboard", "CustomDashboard");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Login}/{id?}");
            });
        }
    }
}
