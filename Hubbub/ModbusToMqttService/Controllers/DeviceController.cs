﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NModbus;
using StackExchange.Redis;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PEIU.Hubbub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        IDatabase redis_ai;
        IDatabase redis_di;
        IModbusFactory modbus;
        IModbusMaster modbus_master = null;
        ILogger<DeviceController> logger;
        IDataAccess dataAccess;
        public DeviceController(IRedisConnectionFactory redisConnectionFactory,
            IDataAccess mysql_dataAccess,
            IModbusFactory modbusFactory, ILoggerFactory loggerFactory)
        {
            redis_ai = redisConnectionFactory.Connection().GetDatabase(1);
            redis_di = redisConnectionFactory.Connection().GetDatabase();
            modbus = modbusFactory;
            modbus_master = modbus.GetModbusMaster();
            logger = loggerFactory.CreateLogger<DeviceController>();
            dataAccess = mysql_dataAccess;
        }

        [HttpPost("EventAck")]
        public async Task<IActionResult> EventAck([FromBody] JObject EventIds)
        {
            using (var session = dataAccess.SessionFactory.OpenSession())
            {
                var ids = EventIds["EventIds"].Select(x => x.Value<string>());
                using (var transaction = session.BeginTransaction())
                {
                    foreach(string key in ids)
                    {
                        ActiveEvent evt = await session.GetAsync<ActiveEvent>(key);
                        if (evt == null || evt.IsAck)
                            continue;
                        evt.IsAck = true;
                        evt.AckTimestamp = DateTime.Now;
                        await session.SaveOrUpdateAsync(evt);
                    }
                    await transaction.CommitAsync();
                    
                }
                
            }
            return Ok();
            
        }

        [HttpGet("manualcontrol")]
        public async Task<IActionResult> ManualControl(byte deviceId, ushort DocumentAddress, short Value)
        {
            ushort uvalue = (ushort)Value;
            if (modbus.ReconnectWhenDisconnected() == false)
            {
                return NoContent();
            }
            await modbus.WriteMultipleRegistersAsync(DocumentAddress, new ushort[] { uvalue });
            return Ok();
        }

        [HttpPost("ReadHoldingRegister")]
        public async Task<IActionResult> ReadHoldingRegister([FromBody] ManualQueryParameter parameter)
        {
            if (modbus.ReconnectWhenDisconnected() == false)
            {
                return NoContent();
            }

            ushort[] datas = await modbus.ReadHoldingRegistersAsync(parameter.StartAddress, parameter.Length);
            ushort newAddr = parameter.StartAddress;

            JObject data = new JObject();
            foreach(ushort value in datas)
            {
                data.Add(newAddr.ToString(), value);
                newAddr++;
            }
            return Ok(data);
        }

        //[HttpPost("Query")]
        //public async Task<IActionResult> Query([FromBody] JArray requests)
        //{
        //    return Ok();
        //}

        [HttpPost("Query")]
        public async Task<IActionResult> Query([FromBody] string[] Fields)
        {
            string deviceName = modbus.GetModbusSystem().DeviceName;
            JObject row = new JObject();
            row.Add("deviceId", deviceName);
            string timeStamp = await redis_ai.HashGetAsync(deviceName, "timestamp");
            row.Add("timestamp", timeStamp);
            if (await redis_ai.KeyExistsAsync(deviceName) == false)
            {
                return NoContent();
            }
            foreach (string field in Fields)
            {
                if(await redis_ai.HashExistsAsync(deviceName, field))
                {
                    RedisValue value = await redis_ai.HashGetAsync(deviceName, field);
                    row.Add(field, (float)value);
                }
            }
            return Ok(row);
        }
    }
}
