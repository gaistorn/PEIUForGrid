using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PEIU.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using NModbus;
using StackExchange.Redis;
using System.Threading;

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

        [HttpGet("RequestActiveEvent")]
        public async Task<IActionResult> RequestActiveEvent()
        {
            JArray arr = new JArray();
            using (var session = dataAccess.SessionFactory.OpenSession())
            {

                var list = await session.CreateCriteria<ActiveEvent>().Add(
                   // Restrictions.Ge("OccurTimestamp", DateTime.Now.AddMinutes(5))).Add(
                    Restrictions.Eq("IsAck", false)
                   
                    )
                    .Add(Restrictions.Lt("EventLevel", 3))
                    .ListAsync<ActiveEvent>();
                foreach(ActiveEvent ev in list)
                {
                    DateTime occurTime = ev.OccurTimestamp.Add(Startup.NotifyEventInterval);
                    if (occurTime > DateTime.Now)
                        continue;
                    JObject obj = JObject.FromObject(ev);
                    arr.Add(obj);
                }
            }

            return Ok(arr);
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

        public static bool TestMode = false;

        [HttpGet("SwitchTestMode")]
        public async Task<IActionResult> SwitchTestMode(bool OnOff)
        {
            TestMode = OnOff;
            return Ok();
        }

        [HttpGet("GetLocalRemoteStatus")]
        public async Task<IActionResult> GetLocalRemoteStatus()
        {
            string deviceName = $"{Startup.SiteId}.{modbus.GetModbusSystem().DeviceName}";
            var status = await redis_ai.HashGetAsync(deviceName, "LocalRemote");
            return Ok(status);
        }

        [HttpGet("SetLocalRemoteStatus")]
        public async Task<IActionResult> SetLocalRemoteStatus(int IsRemote)
        {
            string deviceName = $"{Startup.SiteId}.{modbus.GetModbusSystem().DeviceName}";

            await redis_ai.HashSetAsync(deviceName, "LocalRemote", IsRemote);
            //await modbus.WriteMultipleRegistersAsync(189, new ushort[] { IsRemote == 1 ? (ushort)16 : (ushort)0 });
            return Ok();
        }

#if CONTROL_TEST
        public static float LastControl = 0;
#endif

        [HttpGet("manualcontrol")]
        public async Task<IActionResult> ManualControl(byte deviceId, ushort DocumentAddress, short Value)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            ushort uvalue = (ushort)Value;
            if (modbus.ReconnectWhenDisconnected(cancellationTokenSource.Token, 1) == false)
            {
                return NoContent();
            }

#if CONTROL_TEST
            if(DocumentAddress == 190)
            {
                LastControl = (Value / 10);
            }
#endif


            await modbus.WriteMultipleRegistersAsync(cancellationTokenSource.Token, DocumentAddress, new ushort[] { uvalue });
            return Ok();
        }

        [HttpPost("ReadHoldingRegister")]
        public async Task<IActionResult> ReadHoldingRegister([FromBody] ManualQueryParameter parameter)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            if (modbus.ReconnectWhenDisconnected(cancellationTokenSource.Token, 1) == false)
            {
                return BadRequest();
            }

            ushort[] datas = await modbus.ReadHoldingRegistersAsync(cancellationTokenSource.Token, parameter.StartAddress, parameter.Length);
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
            string lastReadField = "";
            try
            {
                string deviceName = $"{Startup.SiteId}.{modbus.GetModbusSystem().DeviceName}";
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
                    lastReadField = field;
                    if (await redis_ai.HashExistsAsync(deviceName, field))
                    {
                        RedisValue value = await redis_ai.HashGetAsync(deviceName, field);
                        row.Add(field, value.ToString());
                    }
                }
                return Ok(row);
            }
            catch
            {
                return NoContent();
            }
        }
    }
}
