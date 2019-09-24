using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using PEIU.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PEIU.Hubbub.Services;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Receiving;
using NHibernate;
using StackExchange.Redis.Extensions.Core.Configuration;
using PEIU.DataServices;

namespace PEIU.Hubbub
{
    public class Startup
    {
        public static int SiteId { get; set; }
        static readonly ISessionFactory factory =
new NHibernate.Cfg.Configuration().Configure().AddAssembly(
   Assembly.Load("DataModel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")).AddAssembly(
        System.Reflection.Assembly.GetExecutingAssembly()).BuildSessionFactory();

        readonly ILogger<Startup> logger;

        public Startup(IConfiguration configuration, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            logger = loggerFactory.CreateLogger<Startup>();
        }

        public IConfiguration Configuration { get; }
        public static TimeSpan NotifyEventInterval { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            var redisConfiguration = Configuration.GetSection("redis").Get<RedisConfiguration>();
            services.AddSingleton(redisConfiguration);
            var mysql_conn = Configuration.GetConnectionString("mysql");
            NotifyEventInterval = Configuration.GetSection("NotifyEventInterval").Get<TimeSpan>();
            SiteId = Configuration.GetSection("SiteId").Get<int>();

            var mqtt_informations = Configuration.GetSection("MQTTBrokers").Get<MqttConfig>();
            //services.AddSingleton(mqtt_informations);
            services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();
            LoadMqttConfig(services);
            LoadConfigModbusMapper(services);

           
#if CONTROL_TEST
            //services.AddHostedService<ControlLogService>();
#endif
            IDataAccess mysql_access = new MysqlDataAccess(mysql_conn);
            services.AddSingleton(mysql_access);

            //MsSqlAccessManager mssql_access = new MsSqlAccessManager(mssql_conn);
            //using (DataAccess da = new DataAccess(dam))
            //{
            //    var list = da.Select<ModbusSystem>();
            //    foreach (var aimap in list)
            //    {
            //        //string tpNAme = aimap.DataType.TypeName;
            //    }
            //}
            //MqttDataPacket packet = new MqttDataPacket();
            //packet.SetGroupName("FAULT INFO");

            //List<MqttRegister> registers = new List<MqttRegister>();
            //foreach(SlaveInfoYaml obj in config.slaves)
            //{
            //    registers.AddRange(obj.registers.Select(x => x.ConvertRegister(77.1f)));

            //}
            //packet.SetDateTime(DateTimeOffset.Now);
            //packet.registerCnt = registers.Count;
            //packet.registers = registers.ToArray();

            //byte[] buffer = packet.ToByteArray();



            services.AddCors();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            //MqttDataPacket copyPacket = MqttDataPacket.Parse(buffer);
        }

        private async void LoadMqttConfig(IServiceCollection services)
        {
            var mqtt_config = Configuration.GetSection("MQTTBrokers").Get<MqttConfig>();
            var Mqtt = await TryInitializeMqtt(mqtt_config);
            services.AddSingleton(Mqtt);
        }

        private void LoadConfigModbusMapper(IServiceCollection services)
        {
            string sqlite_conn_str = Configuration.GetConnectionString("sqlite");
            SqliteAccessManager dam = new SqliteAccessManager(sqlite_conn_str);
            ModbusSystem modbusList = Configuration.GetSection("Modbus").Get<ModbusSystem>();
            using (DataAccess da = new DataAccess(dam.SessionFactory))
            {

                modbusList.GroupPoints = da.Select<GroupPoint>();
                modbusList.GroupDigitalPoints = da.Select<EventGroupPoint>();
                services.AddSingleton(modbusList);
                services.AddSingleton<IModbusFactory, ModbusConnectionFactory>();
                services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, ModbusBackgroundService>();
                services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, ModbusDigitalProcessingService>();
                //services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, ModbusDigitalProcessingService>();
            }
        }

        private async Task<MqttClientProxyCollection> TryInitializeMqtt(MqttConfig config)
        {
            MqttClientProxyCollection result = new MqttClientProxyCollection();

            IMqttClient peiu_client = await CreateMqttClient(config.PEIUEventBrokerAddress);
            result.PeiuEventBrokerProxy = new MqttClientProxy(peiu_client, config.PEIUEventBrokerAddress);

            foreach (MqttAddress addr in config.DataBrokerAddress)
            {
                IMqttClient client = await CreateMqttClient(addr);
                if (client != null)
                    result.Add(new MqttClientProxy(client, addr));
            }
            return result;
        }

        private async Task<IMqttClient> CreateMqttClient(MqttAddress addr)
        {
            var ClientOptions = new MqttClientOptions
            {
                ClientId = addr.ClientId,
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = addr.BindAddress,
                    Port = addr.Port,

                }

            };

            var mqttClient = new MqttFactory().CreateMqttClient();
            
            mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(async e =>
            {
                //Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(1));

                try
                {
                    await mqttClient.ConnectAsync(ClientOptions);
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            });
            bool IsSuccess = false;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await mqttClient.ConnectAsync(ClientOptions);
                    IsSuccess = true;
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"#### MQTT BROKER CONNECTING FAILED ###");
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                    continue;
                }
            }

            if (IsSuccess == false)
            {
                logger.LogError("#### 브로커 접속에 실패했습니다. 다시 실행해주세요. ####");
                return null;
            }
            return mqttClient;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //var withOrigins = Configuration.GetSection("AllowedOrigins").Get<string[]>();
            var withOrigins = Configuration.GetSection("AllowedOrigins").Get<string[]>();
            app.UseHttpsRedirection();
            app.UseCors(builder =>
            {
                builder
                    .WithOrigins(withOrigins)
                    .AllowAnyHeader()
                    .WithMethods("GET", "POST", "PUT", "DELETE")
                    .AllowCredentials();
            });
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
