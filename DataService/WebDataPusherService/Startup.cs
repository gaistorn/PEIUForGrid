using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PEIU.DataServices;

namespace WebDataPusherService
{
    public class Startup
    {
        public const string CORS_POLICY = "CorsPolicy";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            MqttAddress data_mqtt_address = Configuration.GetSection("MQTTBrokers:DataBrokerAddress").Get<MqttAddress>();
            services.AddSingleton(data_mqtt_address);
            services.AddHostedService<MqttSubscribeWorker>();

            //var withOrigins = Configuration.GetSection("AllowedOrigins").Get<string[]>();
            var withOrigins = Configuration.GetSection("AllowedOrigins").Get<string[]>();
            services.AddCors(o => o.AddPolicy(CORS_POLICY, builder =>
            {
                builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                    .AllowCredentials()
                     .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                    .WithOrigins(withOrigins);
            }));

            services.AddSignalR(options => options.EnableDetailedErrors = true);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
   
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders =
                    Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                    Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseCors(CORS_POLICY);
            app.UseHttpsRedirection();
            app.UseAuthentication();
            //app.UseHttpsRedirection();
            //app.UseStaticFiles();
            //app.UseCookiePolicy();
            app.UseSignalR(routes =>
            {
                routes.MapHub<DataHub.MeasurementHub>("/measurementhub");
            });
            app.UseMvc();
        }
    }
}
