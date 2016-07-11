﻿using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pomelo.AspNetCore.Localization;
using YuukoBlog.Models;

namespace YuukoBlog
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            IConfiguration Configuration;
            services.AddConfiguration(out Configuration);
            services.AddDbContext<BlogContext>(x => x.UseMySql(Configuration["Conn"]));

            services.AddSmartCookies();

            services.AddMemoryCache();
            services.AddSession(x => x.IdleTimeout = TimeSpan.FromMinutes(20));

            services.AddBlobStorage()
                .AddEntityFrameworkStorage<BlogContext>()
                .AddSessionUploadAuthorization();

            services.AddPomeloLocalization(x =>
            {
                x.AddCulture(new string[] { "zh", "zh-CN", "zh-Hans", "zh-Hans-CN" }, new JsonLocalizedStringStore(Path.Combine("Localization", "zh-CN.json")));
                x.AddCulture(new string[] { "en", "en-US", "en-GB" }, new JsonLocalizedStringStore(Path.Combine("Localization", "en-US.json")));
            });

            services.AddMvc()
                .AddMultiTemplateEngine()
                .AddCookieTemplateProvider();

            services.AddTimedJob();
        }

        public async void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Warning, true);

            app.UseStaticFiles();
            app.UseSession();
            app.UseBlobStorage("/assets/shared/scripts/jquery.codecomb.fileupload.js");
            app.UseDeveloperExceptionPage();
            app.UseMvcWithDefaultRoute();

            await SampleData.InitializeYuukoBlog(app.ApplicationServices);

            app.UseTimedJob();
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://*:80")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
