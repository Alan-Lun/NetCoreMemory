using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }
        /// <summary>
        /// 環境變數
        /// </summary>
        public IWebHostEnvironment _env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            var version = Configuration.GetSection("version");
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "Service API",
                    Version = $"v{version.Value}"
                });
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            var apiEndPoint = string.Empty;

            if (env.IsEnvironment("Sit"))
            {
                apiEndPoint = "sit";
            }
            else if (env.IsStaging())
            {
                apiEndPoint = "stg";
            }
            else if (env.IsProduction())
            {
                apiEndPoint = "prod";
            }
            //documentName 是代表使用的SwaggerDoc名稱帶入
            app.UseSwagger(x =>
            {
                x.RouteTemplate = "{documentName}/swagger.json";
                x.PreSerializeFilters.Add((doc, requset) =>
                {

                    if (apiEndPoint == string.Empty || !requset.Host.Value.StartsWith("backend")) return;
                    var root = $"{requset.Host}/notify-{apiEndPoint}";
                    doc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{requset.Scheme}://{root}" } };
                });
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/v1/swagger.json", "My API V1");
            });
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            var forwardingOptions = new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            // its loopback by default
            forwardingOptions.KnownNetworks.Clear();
            forwardingOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardingOptions);

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
