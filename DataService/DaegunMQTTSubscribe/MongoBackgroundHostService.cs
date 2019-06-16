﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PES.Models;
using PES.Toolkit;
using PES.Toolkit.Config;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PES.Service.DataService
{
    public class MongoBackgroundHostService : BackgroundService
    {
        private readonly ILogger _logger;
        public IBackgroundMongoTaskQueue TaskQueue { get; }
        private readonly MongoClient _client;
        private IMongoDatabase _db;
        private readonly MqttSubscribeConfig _mqttConfig;
        readonly IDatabase _redisDb;
        readonly IRedisConnectionFactory _redisFactory;
        IMongoCollection<DaegunPacket> cols;
       

        public MongoBackgroundHostService(
            IBackgroundMongoTaskQueue taskQueue,
            IConfiguration configuration, 
            IRedisConnectionFactory redis_factory,
            MqttSubscribeConfig mqttConfig,
            ILoggerFactory loggerFactory
            )
        {
            string conn_str = configuration.GetConnectionString("mongodb");
            _client = new MongoDB.Driver.MongoClient(conn_str);

            TaskQueue = taskQueue;
            _redisFactory = redis_factory;
            _mqttConfig = mqttConfig;
            _redisDb = _redisFactory.Connection().GetDatabase();

            _logger = loggerFactory.CreateLogger<MongoBackgroundHostService>();
        }
        Dictionary<int, DateTime> lastRecordTime = new Dictionary<int, DateTime>();
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            while (!stoppingToken.IsCancellationRequested)
            {
                //var workItem = await TaskQueue.DequeueAsync(stoppingToken);
                DaegunPacket modbusWorkItem = await TaskQueue.DequeueAsync(stoppingToken);
                try
                {
                    //modbusWorkItem.timestamp = DateTime.Now;
                    
                    string pcs_key = $"site_{modbusWorkItem.sSiteId}_PCS";
                    string bsc_key = $"site_{modbusWorkItem.sSiteId}_BSC";
                    string ess_key = $"site_{modbusWorkItem.sSiteId}_ESSMETER";
                    string pv_key = $"site_{modbusWorkItem.sSiteId}_PVMETER";
                    HashEntry lastUpdate = new HashEntry("timestamp", DateTime.Now.ToString());

                    var hashPcsMap = RedisConnectionFactory.CreateHashEntries(modbusWorkItem.Pcs);
                    hashPcsMap.Add(lastUpdate);
                    var hashbmsMap = RedisConnectionFactory.CreateHashEntries(modbusWorkItem.Bsc, lastUpdate);
                    var hashmeter_ess_Map = RedisConnectionFactory.CreateHashEntries(modbusWorkItem.Ess, lastUpdate);
                    var hashmeter_pv_Map = RedisConnectionFactory.CreateHashEntries(modbusWorkItem.Pv, lastUpdate);

                    
                    await _redisDb.HashSetAsync(pcs_key, hashPcsMap.ToArray(), CommandFlags.FireAndForget);
                    await _redisDb.HashSetAsync(bsc_key, hashbmsMap.ToArray(), CommandFlags.FireAndForget);
                    await _redisDb.HashSetAsync(ess_key, hashmeter_ess_Map.ToArray(), CommandFlags.FireAndForget);
                    await _redisDb.HashSetAsync(pv_key, hashmeter_pv_Map.ToArray(), CommandFlags.FireAndForget);

                    if (lastRecordTime.ContainsKey(modbusWorkItem.sSiteId) == false)
                    {
                        lastRecordTime.Add(modbusWorkItem.sSiteId, DateTime.MinValue);
                    }

                    if (DateTime.Now > lastRecordTime[modbusWorkItem.sSiteId])
                    {
                        _db = _client.GetDatabase("PEIU");
                        cols = _db.GetCollection<DaegunPacket>("daegun_meter");
                        //lastRecord = DateTime.Now.Add(_mqttConfig.RecordInterval);
                        await cols.InsertOneAsync(modbusWorkItem);
                        _logger.LogInformation("Store packet from daegun");
                        lastRecordTime[modbusWorkItem.sSiteId] = DateTime.Now.Add(_mqttConfig.RecordInterval);
                    }

                    

                    //Console.WriteLine($"SAVE DB");
                    //  if(workItem != null)
                    //    await workItem(stoppingToken);
                    //if (modbusWorkItem != null)
                    //    await modbusWorkItem.RunAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                       $"Error occurred executing {nameof(ex)}");
                    Console.WriteLine(ex);
                }


            }

            _logger.LogInformation("Queued Hosted Service is stopping.");
        }
    }
}
