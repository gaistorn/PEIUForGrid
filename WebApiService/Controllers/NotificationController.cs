using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Criterion;
using PEIU.Models;
using PEIU.Models.IdentityModel;
using PEIU.Service.WebApiService;
using PES.Toolkit;
using StackExchange.Redis;

namespace WebApiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        readonly PeiuGridDataContext peiuGridDataContext;
        readonly IClaimServiceFactory claimServiceFactory;
        readonly IDatabaseAsync db;
        readonly AccountDataContext _accountContext;
        public NotificationController(PeiuGridDataContext peiuGridDataContext, IRedisConnectionFactory redis, 
            AccountDataContext accountContext,
            IClaimServiceFactory claimServiceFactory)
        {
            this.peiuGridDataContext = peiuGridDataContext;
            db = redis.Connection().GetDatabase(1);
            this.claimServiceFactory = claimServiceFactory;
            this._accountContext = accountContext;
        }

        private ICriteria ApplyEventFilter(ICriteria criteria, int siteId, string deviceName, bool OnlyGetCompleteEvent)
        {
            ICriteria newcriteria = criteria;
            if (siteId != -1)
                newcriteria = newcriteria.Add(Restrictions.Eq("SiteId", siteId));
            else
            {
                var avaliableSiteIds = GetAvaliableSiteIds();
                newcriteria = newcriteria.Add(Restrictions.InG<int>("SiteId", avaliableSiteIds));
            }
            if(deviceName != null)
                newcriteria = newcriteria.Add(Restrictions.Eq("DeviceId", deviceName));
            if (OnlyGetCompleteEvent)
                newcriteria = newcriteria.Add(Restrictions.IsNotNull("RecoveryDT")).Add(Restrictions.IsNotNull("AckDT"));
            else
            {
                newcriteria = newcriteria.Add(Restrictions.IsNull("RecoveryDT") || Restrictions.IsNull("AckDT"));
            }
            return newcriteria;
        }

        private IEnumerable<int> GetAvaliableSiteIds()
        {
            IEnumerable<int> siteIds = null;

            if (HttpContext.User.IsInRole(UserRoleTypes.Supervisor))
            {
                siteIds = _accountContext.VwContractorsites.Where(x => x.UserId != null).Select(x => x.SiteId);
                //string key = $"Supervisor.Statistics.H{DateTime.Now.Hour}";
                //if (await _redisDb.HashExistsAsync(key, "chg") && await _redisDb.HashExistsAsync(key, "dhg"))
                //    return
                // datas.AddRange(await session.CreateCriteria<TodayAccumchgdhg>().ListAsync<TodayAccumchgdhg>());
            }
            else if (HttpContext.User.IsInRole(UserRoleTypes.Contractor))
            {
                string userId = claimServiceFactory.GetClaimsValue(HttpContext.User, ClaimTypes.NameIdentifier);
                siteIds = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.UserId == userId).Select(x => x.SiteId);
            }
            else if (HttpContext.User.IsInRole(UserRoleTypes.Aggregator))
            {
                string groupId = claimServiceFactory.GetClaimsValue(HttpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                siteIds = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.AggGroupId == groupId).Select(x => x.SiteId);
            }
            return siteIds;
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getactiveeventlist")]
        public async Task<IActionResult> GetActiveEventList(int siteId = -1, string deviceId = null, bool onlycompletedevent = false)
        {
            using (var session = peiuGridDataContext.SessionFactory.OpenStatelessSession())
            {
                var map_row = await ApplyEventFilter(session.CreateCriteria<VwEventRecord>(), siteId, deviceId, onlycompletedevent).ListAsync<VwEventRecord>();
                return Ok(map_row);
            }
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("setackevent")]
        public async Task<IActionResult> SetAckEvent(int eventId)
        {
            try
            {
                string userId = claimServiceFactory.GetClaimsValue(HttpContext.User, ClaimTypes.NameIdentifier);
                using (var session = peiuGridDataContext.SessionFactory.OpenStatelessSession())
                using (var Transaction = session.BeginTransaction())
                {
                    var map_row = await session.GetAsync<EventRecord>(eventId);
                    if (map_row == null)
                        return BadRequest();
                    map_row.AckDT = DateTime.Now;
                    map_row.AckUserId = userId;
                    await session.UpdateAsync(map_row);
                    await Transaction.CommitAsync();

                }
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("updatemapredis")]
        public async Task<IActionResult> GetEventMapList(int siteid, string source, int count )
        {
            using (var session = peiuGridDataContext.SessionFactory.OpenStatelessSession())
            {
                var map_row = await session.CreateCriteria<EventMap>()
                    .Add(Restrictions.Eq("Source", source))
                    .ListAsync<EventMap>();

                foreach (EventMap map in map_row)
                {
                    for (int idx = 1; idx <= count; idx++)
                    {
                        string redisKey = $"SID{siteid}.EVENT.{source}{idx}";
                        if (await db.HashExistsAsync(redisKey, map.EventId) == false)
                            await db.HashSetAsync(redisKey,  map.EventId, bool.FalseString);
                    }
                }
            }
            return Ok();
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("geteventmaplist")]
        public async Task<IActionResult> GetEventMapList()
        {
            JArray result = new JArray();
            using (var session = peiuGridDataContext.SessionFactory.OpenStatelessSession())
            {
                var map_row = await session.CreateCriteria<EventMap>()
                    .ListAsync<EventMap>();
                foreach (EventMap map in map_row)
                {
                    JObject row = new JObject();
                    row.Add("code", map.EventId);
                    row.Add("name", map.Description);
                    row.Add("category", map.Source);
                    row.Add("level", map.Level);
                    result.Add(row);
                }
            }
            return Ok(result);
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getcurrenteventstatus")]
        public async Task<IActionResult> GetCurrentEventStatus(int siteId, string deviceId)
        {
            JArray result = new JArray();
            string redisKey = $"SID{siteId}.EVENT.{deviceId}";
            if (await db.KeyExistsAsync(redisKey) == false)
                return BadRequest("잘못된 SiteId 또는 잘못된 DeviceName");
            var allhash = await db.HashGetAllAsync(redisKey);
            foreach(HashEntry en in allhash)
            {
                JObject row = new JObject();
                row.Add("code", en.Name.ToString().TrimStart('e'));
                row.Add("status", en.Value.ToString());
                result.Add(row);
            }
            return Ok(result);
        }
    }
}