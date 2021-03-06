﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PES.Toolkit;
using PES.Toolkit.Config;
using StackExchange.Redis.Extensions.Core.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PES.Toolkit.Auth;
using System.Globalization;
using Microsoft.AspNetCore.Identity.UI.Services;
using PES.Toolkit.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using PEIU.Service.WebApiService.Authroize;
using PEIU.Service.WebApiService.Publisher;
using System.Threading;
using PEIU.Models;
using PEIU.Models.Database;
using PEIU.Models.IdentityModel;
using PEIU.DataServices;
using PEIU.Models.OWM;

namespace PEIU.Service.WebApiService
{
    public class Startup
    {
        IList<CultureInfo> supportedCultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("ko-KR"),
            new CultureInfo("ko"),
        };

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            LoggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }
        public ILoggerFactory LoggerFactory { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //MongoDB.Driver.MongoClient client = new MongoDB.Driver.MongoClient(Configuration.GetConnectionString("mongodb"));

            //services.AddDbContext<AccountRecordContext>(
            //    options => options.UseMySql(Configuration.GetConnectionString("mysqldb"))
            //    );
            
            services.AddDbContext<AccountDataContext>(
                options => options.UseMySql(Configuration.GetConnectionString("peiu_account_connnectionstring")));
            var EmailSettings = Configuration.GetSection("EmailSettings:SenderName");
            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddSingleton<IHTMLGenerator, HTMLGenerator>();
            //services.AddIdentity<UserAccount>()
            services.AddIdentity<UserAccount, Role>(options =>
            options.ClaimsIdentity.UserIdClaimType = "Id")
                .AddEntityFrameworkStores<AccountDataContext>()
                .AddErrorDescriber<Localization.LocalizedIdentityErrorDescriber>()
                .AddDefaultTokenProviders();
            
            //add the following line of code
            services.AddScoped<IUserClaimsPrincipalFactory<UserAccount>, ClaimsPrincipalFactory>();
            //ServiceDescriptor sd = services.FirstOrDefault(x => x.ServiceType == typeof(IdentityErrorDescriber) && x.ImplementationType == typeof(Localization.LocalizedIdentityErrorDescriber));
            //sd.
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = UserClaimTypes.Issuer,
                        ValidAudience = "https://www.peiu.co.kr",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JasonWebTokenManager.Secret))

                    };
                    options.ClaimsIssuer = UserClaimTypes.Issuer;


                });
            //options.Configuration.
                //.AddCookie();

            services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localization");
            services.AddSingleton<PeiuGridDataContext>();
            

            ConfigureIdentity(services);
            ConfigureAuthrozation(services);
            services.AddCors();

            var map_reduces = Configuration.GetSection("MongoMapReduces").Get<IEnumerable<MongoMapReduceConfig>>();

            var redisConfiguration = Configuration.GetSection("redis").Get<RedisConfiguration>();
            services.AddSingleton(redisConfiguration);
            services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "My API", Version = "v1" });
            });

            MqttAddress data_mqtt_address = Configuration.GetSection("MQTTBrokers:DataBrokerAddress").Get<MqttAddress>();
            services.AddSingleton(data_mqtt_address);
            services.AddHostedService<MqttSubscribeWorker>();

            ReservedRegisterNotifyPublisher reservedRegisterNotifyPublisher = new ReservedRegisterNotifyPublisher();
            reservedRegisterNotifyPublisher.Initialize();
            services.AddSingleton(reservedRegisterNotifyPublisher);
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, CollectingCurrentWeatherService>();
            //services.AddSingleton(client);


            //IServiceCollection cols  = services.AddSingleton<IBackgroundMongoTaskQueue, MongoBackgroundTaskQueue>();
            //services.AddSingleton<MQTTDaegunSubscribe>();


            //services.AddCors(options =>
            //{
            //    options.AddDefaultPolicy(
            //        builder =>
            //        {
            //            builder.WithOrigins("http://118.216.255.118:3011")
            //            .AllowAnyHeader()
            //                            .AllowAnyMethod();
            //        });
            //    options.AddPolicy("PeiuPolicy",
            //    builder =>
            //    {
            //        builder.AllowAnyOrigin()
            //        .AllowAnyMethod()
            //        .AllowAnyHeader();
            //        //.AllowCredentials();
            //    });
            //});
            //services.Configure<MvcOptions>(options =>
            //{
            //    options.Filters.Add(new RequireHttpsAttribute());
            //});
            //services.AddTransient<IEmailSender, EmailSender>();
            services.AddPortableObjectLocalization();
            services.AddSingleton<IClaimServiceFactory, ClaimServiceFactory>();
            services.AddMvc()
                .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            var withOrigins = Configuration.GetSection("AllowedOrigins").Get<string[]>();

            app.UseHttpsRedirection();
            app.UseCors(builder =>
            {
                builder
                    //.AllowAnyOrigin()
                    .WithOrigins(withOrigins)
                    .AllowAnyHeader()
                    .WithMethods("GET", "POST", "PUT", "DELETE")
                    .AllowCredentials();
            });
            //app.UseCors("PeiuPolicy");
            //app.UseMiddleware(typeof(CorsMiddleware));
            //var options = new RewriteOptions().AddRedirectToHttps(StatusCodes.Status301MovedPermanently, 3012);
            app.UseRequestLocalization();
            app.UseAuthentication();


            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
           
            // Console.WriteLine("Swagger: https://www.peiu.co.kr/")
            app.UseMvc();
        }

        private void ConfigureAuthrozation(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(UserPolicyTypes.AllUserPolicy,
                     policy => policy.RequireRole(UserRoleTypes.Aggregator, UserRoleTypes.Contractor, UserRoleTypes.Supervisor));
                options.AddPolicy(UserPolicyTypes.RequiredManager,
                    policy => policy.RequireRole(UserRoleTypes.Aggregator, UserRoleTypes.Supervisor));
                options.AddPolicy(UserPolicyTypes.OnlySupervisor,
                    policy => policy.RequireRole(UserRoleTypes.Supervisor));
            });

            // register the scope authorization handler
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
        }

        private void ConfigureIdentity(IServiceCollection services)
        {


            // Configure Localization
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("ko-KR");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Account2/AccessDenied2");
                options.Cookie.Domain = null;
                options.Cookie.Name = "PEIU.Auth.Cookie";
                //options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.HttpOnly = true;
                //options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
               
                //options.LoginPath = new PathString("/api/auth/logintoredirec");
                //options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;


            });
            var pass_options = Configuration.GetSection("PasswordPolicy").Get<PasswordOptions>();
            
            services.Configure<IdentityOptions>(options =>
            {
                options.Password = pass_options;
            });

            //    // Lockout settings
            //    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            //    options.Lockout.MaxFailedAccessAttempts = 10;



            //    //// Cookie settings
            //    //options.Cookies.ApplicationCookie.ExpireTimeSpan = TimeSpan.FromDays(150);
            //    //options.Cookies.ApplicationCookie.LoginPath = "/Account/LogIn";
            //    //options.Cookies.ApplicationCookie.LogoutPath = "/Account/LogOff";

            //    // User settings
            //    options.User.RequireUniqueEmail = true;

            //});

            //services.ConfigureApplicationCookie(options =>
            //{
            //    // Cookie settings
            //    options.Cookie.HttpOnly = true;
            //    options.Cookie.Expiration = TimeSpan.FromDays(150);
            //    // If the LoginPath isn't set, ASP.NET Core defaults 
            //    // the path to /Account/Login.
            //    options.LoginPath = "/Account/Login";
            //    // If the AccessDeniedPath isn't set, ASP.NET Core defaults 
            //    // the path to /Account/AccessDenied.
            //    //options.AccessDeniedPath = "/Account/AccessDenied";
            //    options.AccessDeniedPath = "/home";
            //    options.SlidingExpiration = true;
            //});
        }

    }
}
