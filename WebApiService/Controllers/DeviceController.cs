using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using PEIU.Models;
using PES.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PES.Service.WebApiService.Controllers
{
    [Route("api/device")]
    [Produces("application/json")]
    //[Authorize]
    //[EnableCors(origins: "http://www.peiu.co.kr:3011", headers: "*", methods: "*")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        readonly PeiuGridDataContext peiuGridDataContext;
        readonly AccountRecordContext peiuDataContext;

        public DeviceController(PeiuGridDataContext context, AccountRecordContext _accountContext)
        {
            peiuGridDataContext = context;
            peiuDataContext = _accountContext;
        }

        [HttpGet("GetRequestActivePower")]
        public async Task<IActionResult> GetRequestActivePower(int Rcc)
        {
            JArray eventList = new JArray();
            using (var session = peiuGridDataContext.SessionFactory.OpenStatelessSession())
            using (var trans = session.BeginTransaction())
            {
                var assets = peiuDataContext.AssetLocations.Where(x => x.RCC == Rcc);
                foreach (AssetLocation assetLocation in assets)
                {
                    var list = await session.CreateCriteria<vw_ActiveEvent>().Add(
                   // Restrictions.Ge("OccurTimestamp", DateTime.Now.AddMinutes(5))).Add(
                   Restrictions.Eq("SiteId", assetLocation.SiteId)

                   )
                  
                   .ListAsync<vw_ActiveEvent>();

                    foreach (vw_ActiveEvent ev in list)
                        eventList.Add(JObject.FromObject(ev));
                }
            }

            return Ok(eventList);
        }

        [HttpGet("EventAck")]
        public async Task<IActionResult> EventAck([FromBody] JObject body)
        {
            //body.TryGetValue()
            Console.WriteLine($"Params: body");
            IEnumerable<string> EventIds = null;
            JToken token;
            if(body.TryGetValue("EventIds", out token))
            {
                EventIds = token.Values<string>();
            }
            using (var session = peiuGridDataContext.SessionFactory.OpenStatelessSession())
            using (var trans = session.BeginTransaction())
            {
                DateTime ackTime = DateTime.Now;

                foreach (string eventid in EventIds)
                {
                    DeviceEvent de = session.Get<DeviceEvent>(eventid);
                    if (de != null && de.IsAck == false)
                    {
                        de.AckTimestamp = ackTime;
                        de.IsAck = true;
                        await session.UpdateAsync(de);
                        Console.WriteLine($"eventId: {eventid} ACK Completes");
                    }
                }
                await trans.CommitAsync();

            }
            return Ok();
        }
    }
}
