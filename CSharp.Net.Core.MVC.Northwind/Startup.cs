using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.IO;
using Microsoft.EntityFrameworkCore;
using Serilog;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Northwind.Util;
using Northwind.DAL;
using Northwind.DAL.Interfaces;
using Northwind.Service;
using Northwind.Service.Interfaces;
using Northwind.Entities.Models;
using Microsoft.AspNetCore.Mvc.Razor;




namespace CSharp.Net.Core.MVC.Northwind
{
    public class Startup
    {

        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region 映射model及viewModel
            //https://www.cjavapy.com/article/100/
            //Install-Package AutoMapper -Version 9.0.0
            //Install-Package AutoMapper.Extensions.Microsoft.DependencyInjection -Version 7.0.0
            //Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
            #endregion

            //主要的多國語言服務，ResourcesPath 是指定資源檔的目錄位置。
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddControllersWithViews()
                //為了在 cshtml 中使用多國語言，如果沒有需要在 View 中使用多國語言，可以不需要註冊它。
                .AddViewLocalization(options => options.ResourcesPath = "Resources")
                //為了在 Model 中使用多國語言，如果沒有需要在 Model 中使用多國語言，可以不需要註冊它。
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                    factory.Create(typeof(SharedResource));
                });


            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });


            #region 將資料庫entities所需的設定注入
            IConfiguration _config = GetSettings();

            //將Settings.json的資訊注入
            services.Configure<Settings>(_config);


            
            //相依性注入取得NorthwindContext物件
            services.AddDbContext<NorthwindContext>(options =>
                  options.UseSqlServer(_config.GetConnectionString("NorthwindConnection")));
            #endregion


            //注入 Serilog
            services.AddSingleton(Serilog.Log.Logger);


            //https://blog.johnwu.cc/article/ironman-day04-asp-net-core-dependency-injection.html
            //Transient
            //如預期，每次注入都是不一樣的實例。
            //Scoped
            //在同一個 Requset 中，不論是在哪邊被注入，都是同樣的實例。
            //Singleton
            //不管 Requset 多少次，都會是同一個實例。
            //相依注入GenericRepository
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IProductsDAL, ProductsDAL>();
            services.AddTransient<IProductsService, ProductsService>();
            services.AddTransient<ICustomerDAL, CustomerDAL>();
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<ILoginService, LoginService>();




            //加入驗證token
            //https://blog.miniasp.com/post/2019/10/13/How-to-use-JWT-token-based-auth-in-aspnet-core-22
            //Microsoft.AspNetCore.Authentication.JwtBearer的套件，要加入參考「Microsoft.AspNetCore.ApiAuthorization.IdentityServer」
            //另外Configure要加入app.UseAuthorization();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
                options.IncludeErrorDetails = true; // 預設值為 true，有時會特別關閉

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // 透過這項宣告，就可以從 "sub" 取值並設定給 User.Identity.Name
                    NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                    // 透過這項宣告，就可以從 "roles" 取值，並可讓 [Authorize] 判斷角色
                    RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

                    // 一般我們都會驗證 Issuer
                    ValidateIssuer = true,
                    ValidIssuer = _config.GetSection("Tokens").GetSection("ValidIssuer").Value, // "JwtAuthDemo" 應該從 IConfiguration 取得

                    // 通常不太需要驗證 Audience
                    ValidateAudience = false,
                    //ValidAudience = "JwtAuthDemo", // 不驗證就不需要填寫

                    // 一般我們都會驗證 Token 的有效期間
                    ValidateLifetime = true,

                    // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
                    ValidateIssuerSigningKey = false,

                    // 應該從 IConfiguration 取得
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.GetSection("Tokens").GetSection("IssuerSigningKey").Value))

                };
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            var supportedCultures = new CultureInfo[] {
                new CultureInfo("en-US"),
                new CultureInfo("zh-TW"),
                new CultureInfo("zh-CN"),
            };
            app.UseRequestLocalization(new RequestLocalizationOptions()
            {
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
                //當預設Provider偵測不到用戶支持上述Culture的話，就會是↓
                DefaultRequestCulture = new RequestCulture("zh-TW")//Default UICulture、Culture 
            });

            //加入Serilog，要在UseStaticFiles的上面
            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseAuthentication();//必須加在usermvc前
            app.UseAuthorization();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{culture=en-US}/{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }

        private IConfigurationRoot GetSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Configuration"))
                .AddJsonFile(path: "Settings.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }


    }
}
