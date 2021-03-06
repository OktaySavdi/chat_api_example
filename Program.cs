using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Elasticsearch;

namespace Chat.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var app = CreateHostBuilder(args).Build();
                string logDirectory;

                #region Reading Config File for getting Log Directory Location
                using (var serviceScope = app.Services.CreateScope())
                {
                    var services = serviceScope.ServiceProvider;
                    var serviceContext = services.GetRequiredService<IConfiguration>();

                    logDirectory = serviceContext["LOGDIRECTORY"];
                    if (String.IsNullOrEmpty(logDirectory))
                    {
                        logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");

                        //Log File Directory has not been set as an ENVIRONMENT VARIABLE
                        Console.WriteLine(string.Concat(Enumerable.Repeat("=", 80)));
                        Console.WriteLine("LOGDIRECTORY environment variable has not been set! {0} location will be used instead.", logDirectory);
                        Console.WriteLine(string.Concat(Enumerable.Repeat("=", 80)));
                        Console.WriteLine("");

                    }

                }
                #endregion

                if (!Directory.Exists(logDirectory)) Directory.CreateDirectory(logDirectory);

                Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                   .Enrich.FromLogContext()
                   .WriteTo.Console()
                   .WriteTo.File(new ElasticsearchJsonFormatter(), Path.Combine(logDirectory, "api.log"), rollingInterval: RollingInterval.Day)
                   .CreateLogger();


                Log.Information("chat api app has been started");


                #region Connect to a remote chat app
                using (var serviceScope = app.Services.CreateScope())
                {
                    var services = serviceScope.ServiceProvider;
                    var serviceContext = services.GetRequiredService<Chat.Api.Services.ChatService>();
                    await serviceContext.StartConnection();
                    Console.WriteLine("Connection Id:{0}", serviceContext.ConnectionId);
                }
                #endregion

                app.Run();

            }
            catch (Exception ex)
            {

                Log.Fatal(ex.Message);
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
            .UseSerilog();
    }
}
