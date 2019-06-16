using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PES.Toolkit;
using PES.Toolkit.Config;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace PES.Service.DataService
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory logger)
        {
            Configuration = configuration;
            loggerFactory = logger;
        }

//        public Startup(IConfiguration configuration) { }

        public IConfiguration Configuration { get; }
        public ILoggerFactory loggerFactory { get; }
   

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
           NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("nlog.config");
            NLog.ILogger errorLogger = NLog.LogManager.Configuration.LogFactory.GetLogger("");
            //errorLogger.Info("Looo");
            services.AddSingleton(errorLogger);
            //Models.DaegunPcsPacket pcsPacket = new Models.DaegunPcsPacket();
            //pcsPacket.ActivePower = 100;
            //pcsPacket.Temp = new float[] { 1.1f, 1.2f, 5.1f, 3.4f };
            //NLog.Logger logger =  NLog.LogManager.Configuration.LogFactory.GetLogger("record.pcs");
            //NLog.LogEventInfo logEvent = LogEventMaker.CreateLogEvent("record.pcs", pcsPacket);
            //logEvent.Properties["SiteId"] = 100;
            //logger.Log(logEvent);

            var mqttOptions = Configuration.GetSection("MQTTSubscribeConfig").Get<MqttSubscribeConfig>();
            services.AddSingleton(mqttOptions);

#if RASPIAN
            //var redisConfiguration = Configuration.GetSection("redis").Get<RedisConfiguration>();
            //services.AddSingleton(redisConfiguration);
            //services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();

            //IBackgroundMongoTaskQueue queue_service = new MongoBackgroundTaskQueue();
            //services.AddSingleton(queue_service);

            //services.AddHostedService<MongoBackgroundHostService>();
            //MQTTDaegunSubscribe describe = new MQTTDaegunSubscribe(loggerFactory, queue_service, mqttOptions);
#endif
            //services.AddHostedService<MQTTDaegunSubscribe>();
            MQTTDaegunSubscribe describe = new MQTTDaegunSubscribe(loggerFactory, mqttOptions, errorLogger);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder =>
            {
                builder
                    .WithOrigins("http://www.peiu.co.kr")
                    .WithOrigins("http://www.peiu.co.kr:30000")
                    .WithOrigins("http://www.peiu.co.kr:30001")
                    .WithOrigins("http://192.168.0.88:3535")
                    .WithOrigins("http://192.168.0.25:3535")
                    .WithOrigins("http://192.168.0.25:30000")
                    //.WithOrigins("http://localhost")
                    //.WithOrigins("http://localhost:3333")
                    //.WithOrigins("http://127.0.0.1:3333")
                    //.WithOrigins("http://127.0.0.1:3333")
                    //.WithOrigins("http://192.168.0.17:3333")
                    //.WithOrigins("http://210.96.71.134:3333")
                    .AllowAnyHeader()
                    .WithMethods("GET", "POST", "PUT", "DELETE")
                    .AllowCredentials();
            });

            app.UseMvc();
        }
    }
}
