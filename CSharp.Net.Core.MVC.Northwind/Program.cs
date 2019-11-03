using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

//�[�JSerilog�Ѧ� https://github.com/serilog/serilog-aspnetcore
//https://github.com/serilog/serilog-aspnetcore/tree/dev/samples/EarlyInitializationSample
//https://derekarends.com/asp-net-core-2-2-and-global-logger/
//http://www.coding.nctu.me/%E7%8D%A8%E7%AB%8B%E4%BD%BF%E7%94%A8-serilog-in-asp-net-core-2-0/
//�u�n�˳o�T��Install-Package Serilog.AspNetCore
//�u�n�˳o�T��Install-Package Serilog.Sinks.Async
//�u�n�˳o�T��Install-Package Serilog.Sinks.RollingFile

namespace CSharp.Net.Core.MVC.Northwind
{
    public class Program
    {
        public static IConfiguration SerilogConfiguration { get; } = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //�p�G�����}�o���ҩΥͲ����ҡA�i�H�[�W�H�U�]�w
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

        public static void Main(string[] args)
        {
            #region serilog�]�w
            Log.Logger = new LoggerConfiguration()
            //info��t�Ϊ�Warning�g�J�ɮ�
            .ReadFrom.Configuration(SerilogConfiguration)
            //debug�g�Jclonsole
            .Enrich.FromLogContext()
            .WriteTo.Debug()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .CreateLogger();
            #endregion

            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (SystemException e)
            {
                Log.Error(e, "Host terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                //���U Serilog
                .UseSerilog();
    }
}
