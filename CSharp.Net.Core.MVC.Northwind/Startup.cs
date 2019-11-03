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
//using Microsoft.AspNetCore.Authentication.JwtBearer;



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
            #region �M�gmodel��viewModel
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

            //�D�n���h��y���A�ȡAResourcesPath �O���w�귽�ɪ��ؿ���m�C
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddControllersWithViews()
                //���F�b cshtml ���ϥΦh��y���A�p�G�S���ݭn�b View ���ϥΦh��y���A�i�H���ݭn���U���C
                .AddViewLocalization(options => options.ResourcesPath = "Resources")
                //���F�b Model ���ϥΦh��y���A�p�G�S���ݭn�b Model ���ϥΦh��y���A�i�H���ݭn���U���C
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


            #region �N��Ʈwentities�һݪ��]�w�`�J
            IConfiguration _config = GetSettings();

            //�NSettings.json����T�`�J
            services.Configure<Settings>(_config);


            
            //�̩ۨʪ`�J���oNorthwindContext����
            services.AddDbContext<NorthwindContext>(options =>
                  options.UseSqlServer(_config.GetConnectionString("NorthwindConnection")));
            #endregion


            //�`�J Serilog
            services.AddSingleton(Serilog.Log.Logger);


            //https://blog.johnwu.cc/article/ironman-day04-asp-net-core-dependency-injection.html
            //Transient
            //�p�w���A�C���`�J���O���@�˪���ҡC
            //Scoped
            //�b�P�@�� Requset ���A���׬O�b����Q�`�J�A���O�P�˪���ҡC
            //Singleton
            //���� Requset �h�֦��A���|�O�P�@�ӹ�ҡC
            //�̪ۨ`�JGenericRepository
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IProductsDAL, ProductsDAL>();
            services.AddTransient<IProductsService, ProductsService>();
            services.AddTransient<ICustomerDAL, CustomerDAL>();
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<ILoginService, LoginService>();


            ////�[�J����token
            ////https://blog.miniasp.com/post/2019/10/13/How-to-use-JWT-token-based-auth-in-aspnet-core-22
            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //.AddJwtBearer(options =>
            //{
            //    // �����ҥ��ѮɡA�^�����Y�|�]�t WWW-Authenticate ���Y�A�o�̷|��ܥ��Ѫ��Բӿ��~��]
            //    options.IncludeErrorDetails = true; // �w�]�Ȭ� true�A���ɷ|�S�O����

            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        // �z�L�o���ŧi�A�N�i�H�q "sub" ���Ȩó]�w�� User.Identity.Name
            //        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
            //        // �z�L�o���ŧi�A�N�i�H�q "roles" ���ȡA�åi�� [Authorize] �P�_����
            //        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

            //        // �@��ڭ̳��|���� Issuer
            //        ValidateIssuer = true,
            //        ValidIssuer = _config.GetSection("Tokens").GetSection("ValidIssuer").Value, // "JwtAuthDemo" ���ӱq IConfiguration ���o

            //        // �q�`���ӻݭn���� Audience
            //        ValidateAudience = false,
            //        //ValidAudience = "JwtAuthDemo", // �����ҴN���ݭn��g

            //        // �@��ڭ̳��|���� Token �����Ĵ���
            //        ValidateLifetime = true,

            //        // �p�G Token ���]�t key �~�ݭn���ҡA�@�볣�u��ñ���Ӥw
            //        ValidateIssuerSigningKey = false,

            //        // ���ӱq IConfiguration ���o
            //        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.GetSection("Tokens").GetSection("IssuerSigningKey").Value))

            //    };
            //});

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
                //��w�]Provider��������Τ����W�zCulture���ܡA�N�|�O��
                DefaultRequestCulture = new RequestCulture("zh-TW")//Default UICulture�BCulture 
            });

            //�[�JSerilog�A�n�bUseStaticFiles���W��
            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseAuthentication();//�����[�busermvc�e

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
