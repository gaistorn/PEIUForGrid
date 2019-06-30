﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DataModel;
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
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace PEIU.Hubbub
{
    public class Startup
    {
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            var deserializer_yaml = new DeserializerBuilder()
                .Build();

            var redisConfiguration = Configuration.GetSection("redis").Get<RedisConfiguration>();
            services.AddSingleton(redisConfiguration);
            var mysql_conn = Configuration.GetConnectionString("mysql");
            var mqtt_informations = Configuration.GetSection("MQTTBrokers").Get<MqttConfig>();
            //services.AddSingleton(mqtt_informations);
            services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();
            LoadMqttConfig(services);
            LoadConfigModbusMapper(services);
            
            IDataAccess mysql_access = new MysqlDataAccess(mysql_conn);
            services.AddSingleton(mysql_access);
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
            DataAccessManager dam = new DataAccessManager(sqlite_conn_str);
            ModbusSystem modbusList = Configuration.GetSection("Modbus").Get<ModbusSystem>();
            using (DataAccess da = new DataAccess(dam))
            {

                modbusList.GroupPoints = da.Select<GroupPoint>();
                modbusList.GroupDigitalPoints = da.Select<EventGroupPoint>();
                services.AddSingleton(modbusList);
                services.AddSingleton<IModbusFactory, ModbusConnectionFactory>();
                services.AddHostedService<ModbusBackgroundService>();
                services.AddHostedService<ModbusDigitalProcessingService>();
            }
        }

        private async Task<MqttClientProxyCollection> TryInitializeMqtt(MqttConfig config)
        {
            MqttClientProxyCollection result = new MqttClientProxyCollection();
            int idx = 0;
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
                    logger.LogError(ex, $"#### MQTT BROKER CONNECTING FAILED ### \n{addr.ToJson()}");
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
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .WithMethods("GET", "POST", "PUT", "DELETE")
                    .AllowCredentials();
            });
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
