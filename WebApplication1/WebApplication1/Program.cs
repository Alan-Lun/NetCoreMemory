using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// IHostingEnvironment
        /// </summary>
        public static IWebHostEnvironment HostingEnvionment { get; set; }

        /// <summary>
        /// CreateWebHostBuilder
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults((webBuilder) =>
                {
                    webBuilder.ConfigureKestrel((serverOptions) => { serverOptions.AddServerHeader = false; })
                        .ConfigureAppConfiguration((webHostBuilder, configurationBinder) =>
                        {
                            HostingEnvionment = webHostBuilder.HostingEnvironment;
                            //configurationBinder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            configurationBinder.AddJsonFile($"settings.{HostingEnvionment.EnvironmentName}.json",
                                optional: true,
                                reloadOnChange: true);
                        })
                        .UseStartup<Startup>();
                }).UseServiceProviderFactory(new AutofacServiceProviderFactory());
    }
}
