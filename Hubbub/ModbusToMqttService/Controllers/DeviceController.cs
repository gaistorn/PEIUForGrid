using System;
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
        ILogger<DeviceController> logger;
        IDataAccess dataAccess;
        public DeviceController(IRedisConnectionFactory redisConnectionFactory,
            IDataAccess mysql_dataAccess,
            IModbusFactory modbusFactory, ILoggerFactory loggerFactory)
        {
            redis_ai = redisConnectionFactory.Connection().GetDatabase(1);
            redis_di = redisConnectionFactory.Connection().GetDatabase();
            modbus = modbusFactory;
            logger = loggerFactory.CreateLogger<DeviceController>();
            dataAccess = mysql_dataAccess;
        }

        [HttpGet("EventAck")]
        public async Task<IActionResult> EventAck(int[] EventIds)
        {
            using (var session = dataAccess.SessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    foreach(int key in EventIds)
                    {
                        ActiveEvent evt = await session.GetAsync<ActiveEvent>(key);
                        if (evt.IsAck)
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
        public async Task<IActionResult> ManualControl(string deviceName, ushort DocumentAddress, ushort Value)
        {
            if (await modbus.WriteMultipleRegisters(deviceName, DocumentAddress, new ushort[] { Value }))
                return Ok();
            else
                return base.BadRequest();

        }

        [HttpPost("ReadHoldingRegister")]
        public async Task<IActionResult> ReadHoldingRegister([FromBody] ManualQueryParameter parameter)
        {
            
            
            ushort[] datas = await modbus.ReadHoldingRegisters(parameter.DeviceName, parameter.StartAddress, parameter.Length);
            if (datas == null)
                return BadRequest();
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
        public async Task<IActionResult> Query([FromBody] QueryParameter[] requests)
        {
            JArray jArray = new JArray();
            foreach (QueryParameter p in requests)
            {
                JObject row = new JObject();
                jArray.Add(row);
                row.Add("deviceId", p.DeviceId);
                if (await redis_ai.KeyExistsAsync(p.DeviceId) == false)
                {
                    continue;
                }
                string timeStamp = await redis_ai.HashGetAsync(p.DeviceId, "timestamp");
                row.Add("timestamp", timeStamp);
                foreach (string field in p.Fields)
                {
                    if (await redis_ai.HashExistsAsync(p.DeviceId, field))
                    {
                        RedisValue value = await redis_ai.HashGetAsync(p.DeviceId, field);
                        row.Add(field, (float)value);
                    }
                }
            }
            return Ok(jArray);
        }
    }
}
