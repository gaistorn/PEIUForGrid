﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using PEIU.Models;
using PEIU.Models.Database;
using PEIU.Models.IdentityModel;
using PEIU.Service;
using PEIU.Service.WebApiService;
using PEIU.Service.WebApiService.Localization;
using PES.Toolkit;
using StackExchange.Redis;

namespace WebApiService.Controllers
{
    [Route("api/statistics")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly UserManager<UserAccount> _userManager;
        AccountDataContext _accountContext;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<Role> roleManager;
        private readonly IHTMLGenerator htmlGenerator;
        private readonly IClaimServiceFactory _claimsManager;
        private readonly PeiuGridDataContext _peiuGridDataContext;
        private readonly IDatabaseAsync _redisDb;
        private readonly ConnectionMultiplexer _redisConn;
        readonly ILogger<StatisticsController> logger;

        public StatisticsController(UserManager<UserAccount> userManager,
            SignInManager<UserAccount> signInManager, RoleManager<Role> _roleManager, IRedisConnectionFactory redis, ILogger<StatisticsController> logger,
            IEmailSender emailSender, IHTMLGenerator _htmlGenerator, IClaimServiceFactory claimsManager, PeiuGridDataContext peiuGridDataContext,
            AccountDataContext accountContext)
        {
            _userManager = userManager;
            _accountContext = accountContext;
            _signInManager = signInManager;
            _emailSender = emailSender;
            htmlGenerator = _htmlGenerator;
            roleManager = _roleManager;
            _claimsManager = claimsManager;
            _peiuGridDataContext = peiuGridDataContext;
            _redisConn = redis.Connection();
            _redisDb = _redisConn.GetDatabase();
            this.logger = logger;
        }

        private IEnumerable<int> GetAvaliableSiteIds()
        {
            IEnumerable<int> siteIds = null;

            if (HttpContext.User.IsInRole(UserRoleTypes.Supervisor))
            {
                siteIds = _accountContext.VwContractorsites.Where(x=>x.UserId != null).Select(x => x.SiteId);
                //string key = $"Supervisor.Statistics.H{DateTime.Now.Hour}";
                //if (await _redisDb.HashExistsAsync(key, "chg") && await _redisDb.HashExistsAsync(key, "dhg"))
                //    return
                // datas.AddRange(await session.CreateCriteria<TodayAccumchgdhg>().ListAsync<TodayAccumchgdhg>());
            }
            else if (HttpContext.User.IsInRole(UserRoleTypes.Contractor))
            {
                string userId = _claimsManager.GetClaimsValue(HttpContext.User, ClaimTypes.NameIdentifier);
                siteIds = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.UserId == userId).Select(x => x.SiteId);
            }
            else if (HttpContext.User.IsInRole(UserRoleTypes.Aggregator))
            {
                string groupId = _claimsManager.GetClaimsValue(HttpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                siteIds = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.AggGroupId == groupId).Select(x => x.SiteId);
            }
            return siteIds;
        }

        private IEnumerable<VwContractorsite> GetAvaliableSites()
        {
            IEnumerable<VwContractorsite> siteIds = null;

            if (HttpContext.User.IsInRole(UserRoleTypes.Supervisor))
            {
                siteIds = _accountContext.VwContractorsites.Where(x => x.UserId != null);
                //string key = $"Supervisor.Statistics.H{DateTime.Now.Hour}";
                //if (await _redisDb.HashExistsAsync(key, "chg") && await _redisDb.HashExistsAsync(key, "dhg"))
                //    return
                // datas.AddRange(await session.CreateCriteria<TodayAccumchgdhg>().ListAsync<TodayAccumchgdhg>());
            }
            else if (HttpContext.User.IsInRole(UserRoleTypes.Contractor))
            {
                string userId = _claimsManager.GetClaimsValue(HttpContext.User, ClaimTypes.NameIdentifier);
                siteIds = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.UserId == userId);
            }
            else if (HttpContext.User.IsInRole(UserRoleTypes.Aggregator))
            {
                string groupId = _claimsManager.GetClaimsValue(HttpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                siteIds = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.AggGroupId == groupId);
            }
            return siteIds;
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getmonthlyaccumuactivepower")]
        public async Task<IActionResult> GetMinuteStatistics(DateTime date)
        {
            try
            {
                using(var statelessSession = _peiuGridDataContext.SessionFactory.OpenStatelessSession())
                {
                    JObject result = new JObject();

                    var result = statelessSession.CreateCriteria<VwMinuteEssstat>()
                        .Add(Restrictions.)
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getmonthlyaccumuactivepower")]
        public async Task<IActionResult> GetMonthlyAccumuActivePower()
        {
            try
            {
                IEnumerable<int> siteIds = GetAvaliableSiteIds();
                if (siteIds.Count() == 0)
                    return Ok();
                List<MonthlyAccumchgdhg> datas = new List<MonthlyAccumchgdhg>();
                var keys = CommonFactory.SearchKeys(_redisConn, CommonFactory.PVRedisKeyPattern);

                double total_energy_power = 0;
                double total_chg = 0;
                double total_dhg = 0;
                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    IList<MonthlyAccumchgdhg> results = await session.CreateCriteria<MonthlyAccumchgdhg>()
                        .Add(Restrictions.InG<int>("SiteId", siteIds))
                        .ListAsync<MonthlyAccumchgdhg>();
                    total_chg = results.Sum(x => x.Charging);
                    total_dhg = results.Sum(x => x.Discharging);


                    var pvresults = await session.CreateCriteria<MonthlyAccumPv>()
                        .Add(Restrictions.InG<int>("SiteId", siteIds))
                        .ListAsync<MonthlyAccumPv>();
                    total_energy_power += pvresults.Sum(x => x.Accumpvpower);
                    //    var sites = _accountContext.VwContractorsites.Where(x => x.UserId == userId).Select(x=>x.SiteId);
                    //    var result = await session.CreateCriteria<TodayAccumchgdhg>().Add(Restrictions.InG<int>("SiteId", sites)).ListAsync<TodayAccumchgdhg>();
                    //    string userId = _claimsManager.GetClaimsValue(HttpContext.User, ClaimTypes.NameIdentifier);
                    //    source = _accountContext.VwContractorusers.Where(x => x.UserId == userId);
                    //}
                    //else if (HttpContext.User.IsInRole(UserRoleTypes.Aggregator))
                    //{
                    //    string groupId = _claimsManager.GetClaimsValue(HttpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                    //    source = _accountContext.VwContractorusers.Where(x => x.AIEggGroupId == groupId);
                    //}

                    return Ok(new { todayactivepowerresult = results, todaypvpowerresult = pvresults, toady_accum_charging = total_chg, today_accum_discharging = total_dhg, today_accum_pv_energy = total_energy_power });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        private Func<VwContractorsite, string> MakeClusterKey(int lawCodeLevel)
        {
            switch(lawCodeLevel)
            {
                case 3:
                    return new Func<VwContractorsite, string>(x => x.LawFirstCode + x.LawMiddleCode + x.LawLastCode);
                case 2:
                    return new Func<VwContractorsite, string>(x => x.LawFirstCode + x.LawMiddleCode);
                default:
                    return new Func<VwContractorsite, string>(x => x.LawFirstCode);
            }
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getstatisticscurrentvalue")]
        public async Task<IActionResult> GetStatisticsCurrentValue(int lawcodelevel = 1)
        {
            JObject row_j = null;
            try
            {
                
                IEnumerable<VwContractorsite> sites = GetAvaliableSites();
                if (sites.Count() == 0)
                    return Ok();

                var group_sites = sites.GroupBy(MakeClusterKey(lawcodelevel), value => value);
                IEnumerable<int> siteIds = sites.Select(x => x.SiteId);

                JArray result = new JArray();
                List<double> total_socs = new List<double>();
                foreach (IGrouping<string, VwContractorsite> row in group_sites)
                {
                    double total_energy_power = 0;
                    double total_actPwr_charging = 0;
                    double total_actPwr_discharging = 0;
                    List<double> socs = new List<double>();
                    List<double> sohs = new List<double>();
                    int first_siteid = -1;
                    foreach (VwContractorsite site in row)
                    {
                        if (first_siteid == -1)
                            first_siteid = site.SiteId;
                        // PV 
                        string target_redis_key = CommonFactory.CreateRedisKey(site.SiteId, 4, "PV*");
                        var redis_keys = CommonFactory.SearchKeys(_redisConn, target_redis_key);
                        foreach (RedisKey pv_key in redis_keys)
                        {
                            double TotalActivePower = (double)await _redisDb.HashGetAsync(pv_key, "TotalActivePower");
                            total_energy_power += TotalActivePower;
                        }

                        // PCS
                        target_redis_key = CommonFactory.CreateRedisKey(site.SiteId, 1, "PCS*");
                        redis_keys = CommonFactory.SearchKeys(_redisConn, target_redis_key);
                        foreach (RedisKey key in redis_keys)
                        {
                            double TotalActivePower = (double)await _redisDb.HashGetAsync(key, "actPwrKw");
                            if (TotalActivePower > 0)
                                total_actPwr_discharging += TotalActivePower;
                            else
                                total_actPwr_charging += TotalActivePower;

                        }

                        // BMS
                        target_redis_key = CommonFactory.CreateRedisKey(site.SiteId, 2, "BMS*");
                        redis_keys = CommonFactory.SearchKeys(_redisConn, target_redis_key);
                        foreach (RedisKey key in redis_keys)
                        {
                            double soc = (double)await _redisDb.HashGetAsync(key, "bms_soc");
                            socs.Add(soc);
                            total_socs.Add(soc);
                            double soh = (double)await _redisDb.HashGetAsync(key, "bms_soh");
                                sohs.Add(soh);
                        }

                        
                    }

                    JObject weather_obj = new JObject();
                    if(row.Count() > 0)
                    {
                        // Weather
                        weather_obj = await CommonFactory.RetriveWeather(row.FirstOrDefault().SiteId, _redisDb);
                    }

                    row_j = new JObject();
                    row_j.Add("LawCode", row.Key);
                    row_j.Add("total_pvpower", total_energy_power);
                    row_j.Add("total_charging", Math.Abs(total_actPwr_charging));
                    row_j.Add("total_discharging", total_actPwr_discharging);
                    row_j.Add("average_soc", socs.Count() > 0 ? socs.Average() : 0 );
                    row_j.Add("average_soh", sohs.Count() > 0 ? sohs.Average() : 0);
                    row_j.Add("total_count", row.Count());
                    row_j.Add("weather", weather_obj);
                    result.Add(row_j);
                }

                JObject center_weather = await CommonFactory.RetriveWeather(0, _redisDb);

                return Ok(new { group = result, total_average_soc = total_socs.Average(), total_event_count = 0, total_sites_count = siteIds.Count(), controlcenter_weather = center_weather });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }



        private async Task<List<double>> ReadSiteStat(int siteId, int groupNo, string filter, string field)
        {
            List<double> result = new List<double>();
            string target_redis_key = CommonFactory.CreateRedisKey(siteId, groupNo, filter);
            var redis_keys = CommonFactory.SearchKeys(_redisConn, target_redis_key);
            foreach (RedisKey pv_key in redis_keys)
            {
                double TotalActivePower = (double)await _redisDb.HashGetAsync(pv_key, field);
                result.Add(TotalActivePower);
            }
            return result;
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getweatherbysiteid")]
        public async Task<IActionResult> GetWeatherBySiteId(int siteId)
        {
            JObject weather = await CommonFactory.RetriveWeather(siteId, _redisDb);
            if (weather == null)
                return BadRequest();
            else
                return Ok(weather);
        }


        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getcontractassetbyfindsiteid")]
        public async Task<IActionResult> GetContractassetbyFindsiteid(int siteId)
        {
            IEnumerable<int> siteIds = GetAvaliableSiteIds();
            if (siteIds.Contains(siteId) == false)
                return BadRequest();

            List<double> total_actpower = await ReadSiteStat(siteId, 1, "PCS*", "actPwrKw");
            List<double> total_soc = await ReadSiteStat(siteId, 2, "BMS*", "bms_soc");
            List<double> total_pvpower = await ReadSiteStat(siteId, 4, "PV*", "TotalActivePower");

            return Ok(new { activepower = total_actpower.Sum(), soc = total_soc.Count() > 0 ? total_soc.Average() : 0, pvpower = total_pvpower.Sum() });

        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("gettodayaccumuactivepower")]
        public async Task<IActionResult> GetTodayAccumuActivePower()
        {
            try
            {
                IEnumerable<int> siteIds = GetAvaliableSiteIds();
                if (siteIds.Count() == 0)
                    return Ok();
                List<TodayAccumchgdhg> datas = new List<TodayAccumchgdhg>();
                //if (HttpContext.User.IsInRole(UserRoleTypes.Supervisor))
                //{
                //    siteIds = _accountContext.VwContractorsites.Select(x => x.SiteId);
                //    //string key = $"Supervisor.Statistics.H{DateTime.Now.Hour}";
                //    //if (await _redisDb.HashExistsAsync(key, "chg") && await _redisDb.HashExistsAsync(key, "dhg"))
                //    //    return
                //    // datas.AddRange(await session.CreateCriteria<TodayAccumchgdhg>().ListAsync<TodayAccumchgdhg>());
                //}
                //else if (HttpContext.User.IsInRole(UserRoleTypes.Contractor))
                //{
                //    string userId = _claimsManager.GetClaimsValue(HttpContext.User, ClaimTypes.NameIdentifier);
                //    siteIds = _accountContext.VwContractorsites.Where(x => x.UserId == userId).Select(x => x.SiteId);
                //}
                //else if (HttpContext.User.IsInRole(UserRoleTypes.Aggregator))
                //{
                //    string groupId = _claimsManager.GetClaimsValue(HttpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                //    siteIds = _accountContext.VwContractorsites.Where(x => x.AggGroupId == groupId).Select(x => x.SiteId);
                //}

                var keys = CommonFactory.SearchKeys(_redisConn, CommonFactory.PVRedisKeyPattern);

                double total_energy_power = 0;

               
                double total_chg = 0;
                double total_dhg = 0;
                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    IList< TodayAccumchgdhg> results = await session.CreateCriteria<TodayAccumchgdhg>()
                        .Add(Restrictions.InG<int>("SiteId", siteIds))
                        .ListAsync<TodayAccumchgdhg>();
                    total_chg = results.Sum(x => x.Charging);
                    total_dhg = results.Sum(x => x.Discharging);

                    var pvresults = await session.CreateCriteria<TodayAccumPv>()
                       .Add(Restrictions.InG<int>("SiteId", siteIds))
                       .ListAsync<TodayAccumPv>();
                    total_energy_power += pvresults.Sum(x => x.Accumpvpower);

                    //    var sites = _accountContext.VwContractorsites.Where(x => x.UserId == userId).Select(x=>x.SiteId);
                    //    var result = await session.CreateCriteria<TodayAccumchgdhg>().Add(Restrictions.InG<int>("SiteId", sites)).ListAsync<TodayAccumchgdhg>();
                    //    string userId = _claimsManager.GetClaimsValue(HttpContext.User, ClaimTypes.NameIdentifier);
                    //    source = _accountContext.VwContractorusers.Where(x => x.UserId == userId);
                    //}
                    //else if (HttpContext.User.IsInRole(UserRoleTypes.Aggregator))
                    //{
                    //    string groupId = _claimsManager.GetClaimsValue(HttpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                    //    source = _accountContext.VwContractorusers.Where(x => x.AggGroupId == groupId);
                    //}

                    return Ok(new { todayactivepowerresult = results, todaypvpowerresult = pvresults, toady_accum_charging = total_chg, today_accum_discharging = total_dhg, today_accum_pv_energy = total_energy_power });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        //[HttpGet("downloadhistorybyrcc")]
        //public async Task<IActionResult> downloadhistorybyrcc(int rcc, DateTime startdate, DateTime enddate)
        //{

        //    var rcc_map = PEIU.Service.WebApiService.Controllers.PMSController.rcc_list;
        //    if (rcc_map.ContainsKey(rcc) == false)
        //        return BadRequest();
        //    string areaName = rcc_map[rcc];

        //    var rcc_by_site_map = PEIU.Service.WebApiService.Controllers.PMSController.RccBySiteMap;
        //    //converting Pdf file into bytes array  
        //    //adding bytes to memory stream   
        //    //var dataStream = new MemoryStream(dataBytes);

        //    string file = $"{areaName}지역_{DateTime.Now:yyyy-MM-dd HHmmss}.csv";

        //    MongoDB.Driver.IMongoDatabase main_db = mongoClient.GetDatabase("PEIU");
        //    var cols = main_db.GetCollection<BsonDocument>("daegun_meter");
        //    var builder = Builders<BsonDocument>.Filter;
        //    DateTime today = startdate.Date.ToUniversalTime();
        //    DateTime tommorow = enddate.Date.AddDays(1);
        //    var filter = builder.In("sSiteId", rcc_by_site_map[rcc]) & builder.Gte("timestamp", today) & builder.Lt("timestamp", tommorow);
        //    JArray trends = new JArray();
        //    var result = await cols.Find(filter).Sort("{timestamp: 1}").ToListAsync();

        //    List<string> headerStr = new List<string>();
        //    StringBuilder csv_builder = new StringBuilder();
        //    csv_builder.AppendLine("timestamp,siteid,activepower,soc,soh,pvactivepower,pvvoltage,pcscurrent,pcsstatus,freq");
        //    foreach(var db in result)
        //    {
        //        DateTime timeStamp = db["timestamp"].ToUniversalTime();
        //        DateTime localTime = timeStamp.ToLocalTime();

        //        object[] row_datas = new object[]
        //        {
        //            localTime.ToString("yyyy-MM-dd HH:mm:ss"),
        //            db["sSiteId"].AsInt32,
        //            db["Pcs"]["ActivePower"].AsDouble,
        //            db["Bsc"]["Soc"].AsDouble,
        //            db["Bsc"]["Soh"].AsDouble,
        //            db["Pv"]["TotalActivePower"].AsDouble,
        //            db["Pv"]["Voltage"]["R"].AsDouble,
        //            db["Pcs"]["AC_phase_current"]["R"].AsDouble,
        //            db["Pcs"]["Status"].AsInt32,
        //            db["Ess"]["Frequency"].AsDouble
        //        };
        //        csv_builder.AppendLine(string.Join(",", row_datas));
        //    }

        //    string str = csv_builder.ToString();
        //    byte[] dataBytes = Encoding.UTF8.GetBytes(str);
        //    // Response...
        //    System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
        //    {
        //        FileName = file,
        //        Inline = false  // false = prompt the user for downloading;  true = browser to try to show the file inline
        //    };
        //    Response.Headers.Add("Content-Disposition", cd.ToString());
        //    Response.Headers.Add("X-Content-Type-Options", "nosniff");
        //    return File(dataBytes, "application/octet-stream");

        //    //return new FileDownloadAction(dataStream, Request, $"EMS_{DateTime.Now:yyyy-MM-dd HHmmss}.raw");
        //    //return new eBookResult(dataStream, Request, bookName);
        //}

        //[HttpGet("gehistorybyrcc")]
        //public async Task<IActionResult> gehistorybyrcc(int rcc, DateTime startdate, DateTime enddate, int PageNo, int ShowRowCount, int SortNum = -1)
        //{
        //    var rcc_map = PEIU.Service.WebApiService.Controllers.PMSController.RccBySiteMap;
        //    if (rcc_map.ContainsKey(rcc) == false)
        //        return BadRequest();
        //    MongoDB.Driver.IMongoDatabase main_db = mongoClient.GetDatabase("PEIU");
        //    var cols = main_db.GetCollection<BsonDocument>("daegun_meter");
        //    var builder = Builders<BsonDocument>.Filter;
        //    DateTime today = startdate.Date.ToUniversalTime();
        //    DateTime tommorow = enddate.Date.AddDays(1);
        //    var filter = builder.In("sSiteId", rcc_map[rcc]) & builder.Gte("timestamp", today) & builder.Lt("timestamp", tommorow);
        //    JArray trends = new JArray();
        //    await cols.Find(filter).Limit(ShowRowCount).Skip((PageNo - 1) * ShowRowCount).Sort("{timestamp: " + SortNum + "}").ForEachAsync(
        //        db =>
        //        {
        //            DateTime timeStamp = db["timestamp"].ToUniversalTime();
        //            DateTime localTime = timeStamp.ToLocalTime();

        //            JObject trend_data = new JObject();
        //            trend_data.Add("time", localTime);
        //            trend_data.Add("siteid", db["sSiteId"].AsInt32);

        //            trend_data.Add("ActivePower", db["Pcs"]["ActivePower"].AsDouble);
        //            trend_data.Add("Soc", db["Bsc"]["Soc"].AsDouble);
        //            trend_data.Add("Soh", db["Bsc"]["Soh"].AsDouble);
        //            trend_data.Add("PvActivePower", db["Pv"]["TotalActivePower"].AsDouble);
        //            trend_data.Add("pvVoltage", db["Pv"]["Voltage"]["R"].AsDouble);
        //            trend_data.Add("ACCurrent", db["Pcs"]["AC_phase_current"]["R"].AsDouble);
        //            trend_data.Add("pcs_status", db["Pcs"]["Status"].AsInt32);
        //            trend_data.Add("Frequency", db["Ess"]["Frequency"].AsDouble);
        //            trends.Add(trend_data);
        //        }
        //        );
        //    return Ok(trends);
        //    //db.getCollection('daegun_meter').find({ sSiteId: 148}).skip((4 - 1) * 10).limit(10)
        //}

        //[HttpGet("gettrendinfo")]
        //public async Task<IActionResult> gettrenddata(int[] siteid, DateTime startdate, DateTime enddate, int PageNo, int ShowRowCount, int SortNum = -1)
        //{
        //    MongoDB.Driver.IMongoDatabase main_db = mongoClient.GetDatabase("PEIU");
        //    var cols = main_db.GetCollection<BsonDocument>("daegun_meter");
        //    var builder = Builders<BsonDocument>.Filter;
        //    DateTime today = startdate.Date.ToUniversalTime();
        //    DateTime tommorow = enddate.Date.AddDays(1);
        //    var filter = builder.In("sSiteId", siteid) & builder.Gte("timestamp", today) & builder.Lt("timestamp", tommorow);
        //    JArray trends = new JArray();
        //    await cols.Find(filter).Limit(ShowRowCount).Skip((PageNo - 1) * ShowRowCount).Sort("{timestamp: " + SortNum + "}").ForEachAsync(
        //        db =>
        //        {
        //            DateTime timeStamp = db["timestamp"].ToUniversalTime();
        //            DateTime localTime = timeStamp.ToLocalTime();

        //            JObject trend_data = new JObject();
        //            trend_data.Add("time", localTime);
        //            trend_data.Add("siteid", db["sSiteId"].AsInt32);

        //            trend_data.Add("ActivePower", db["Pcs"]["ActivePower"].AsDouble);
        //            trend_data.Add("Soc", db["Bsc"]["Soc"].AsDouble);
        //            trend_data.Add("Soh", db["Bsc"]["Soh"].AsDouble);
        //            trend_data.Add("PvActivePower", db["Pv"]["TotalActivePower"].AsDouble);
        //            trend_data.Add("pvVoltage", db["Pv"]["Voltage"]["R"].AsDouble);
        //            trend_data.Add("ACCurrent", db["Pcs"]["AC_phase_current"]["R"].AsDouble);
        //            trend_data.Add("pcs_status", db["Pcs"]["Status"].AsInt32);
        //            trend_data.Add("Frequency", db["Ess"]["Frequency"].AsDouble);
        //            trends.Add(trend_data);
        //        }
        //        );
        //    return Ok(trends);
        //    //db.getCollection('daegun_meter').find({ sSiteId: 148}).skip((4 - 1) * 10).limit(10)
        //}

        //[HttpGet("gettrenddatabyrcc")]
        //public async Task<IActionResult> gettrenddatabyrcc(int rccCode, DateTime date)
        //{
        //    var rcc_map = PEIU.Service.WebApiService.Controllers.PMSController.RccBySiteMap;
        //    if (rcc_map.ContainsKey(rccCode) == false)
        //        return BadRequest();

        //    MongoDB.Driver.IMongoDatabase main_db = mongoClient.GetDatabase("PEIU");
        //    var cols = main_db.GetCollection<BsonDocument>("daegun_meter");
        //    var builder = Builders<BsonDocument>.Filter;
        //    DateTime today = date.Date.ToUniversalTime();
        //    DateTime tommorow = today.AddDays(1);
        //    var filter = builder.In("sSiteId", rcc_map[rccCode]) & builder.Gte("timestamp", today) & builder.Lt("timestamp", tommorow);
        //    var cursor = cols.Find(filter).Sort("{timestamp: -1}");

        //    ConcurrentDictionary<DateTime, List<JObject>> values = new ConcurrentDictionary<DateTime, List<JObject>>();

        //    await cursor.ForEachAsync(db =>
        //    {
        //        DateTime timeStamp = db["timestamp"].ToUniversalTime();
        //        DateTime localTime = timeStamp.ToLocalTime();
        //        DateTime input_time = new DateTime(localTime.Year, localTime.Month, localTime.Day, localTime.Hour, 0, 0);
        //        if(values.ContainsKey(input_time) == false)
        //        {
        //            values[input_time] = new List<JObject>();
        //        }




        //        JObject trend_data = new JObject();
        //        trend_data.Add("time", localTime);
        //        trend_data.Add("siteid", db["sSiteId"].AsInt32);
        //        trend_data.Add("ActivePower", db["Pcs"]["ActivePower"].AsDouble);
        //        trend_data.Add("Soc", db["Bsc"]["Soc"].AsDouble);
        //        trend_data.Add("Soh", db["Bsc"]["Soh"].AsDouble);
        //        trend_data.Add("PvActivePower", db["Pv"]["TotalActivePower"].AsDouble);
        //        trend_data.Add("pvVoltage", db["Pv"]["Voltage"]["R"].AsDouble);
        //        trend_data.Add("ACCurrent", db["Pcs"]["AC_phase_current"]["R"].AsDouble);
        //        trend_data.Add("pcs_status", db["Pcs"]["Status"].AsInt32);
        //        trend_data.Add("Frequency", db["Ess"]["Frequency"].AsDouble);
        //        values[input_time].Add(trend_data);

        //    });

        //    JArray result_array = new JArray();
        //    var keys = values.Keys.OrderBy(x => x);
        //    foreach(DateTime dt in keys)
        //    {
        //        JObject trend_data = new JObject();
        //        trend_data.Add("time", dt);
        //        trend_data.Add("ActivePower", values[dt].Select(x => x["ActivePower"].Value<double>()).Sum());
        //        trend_data.Add("Soc", values[dt].Select(x => x["Soc"].Value<double>()).Average());
        //        trend_data.Add("Soh", values[dt].Select(x => x["Soh"].Value<double>()).Average());
        //        trend_data.Add("PvActivePower", values[dt].Select(x => x["PvActivePower"].Value<double>()).Average());
        //        trend_data.Add("pvVoltage", values[dt].Select(x => x["pvVoltage"].Value<double>()).Average());
        //        trend_data.Add("ACCurrent", values[dt].Select(x => x["ACCurrent"].Value<double>()).Average());
        //        result_array.Add(trend_data);
        //    }

        //    return Ok(result_array);

        //}

        //[HttpGet("gettrenddata")]
        //public async Task<IActionResult> gettrenddata(int siteid, DateTime date)
        //{
        //    MongoDB.Driver.IMongoDatabase main_db = mongoClient.GetDatabase("PEIU");
        //    var cols = main_db.GetCollection<BsonDocument>("daegun_meter");
        //    var builder = Builders<BsonDocument>.Filter;
        //    DateTime today = date.Date.ToUniversalTime();
        //    DateTime tommorow = today.AddDays(1);
        //    var filter = builder.Eq("sSiteId", siteid) & builder.Gte("timestamp", today) & builder.Lt("timestamp", tommorow);
        //    IAsyncCursor<BsonDocument> cursor = await cols.FindAsync(filter);
        //    JArray trends = new JArray();
        //    await cursor.ForEachAsync(db =>
        //    {
        //        DateTime timeStamp = db["timestamp"].ToUniversalTime();
        //        DateTime localTime = timeStamp.ToLocalTime();

        //        JObject trend_data = new JObject();
        //        trend_data.Add("time", localTime);


        //        trend_data.Add("ActivePower", db["Pcs"]["ActivePower"].AsDouble);
        //        trend_data.Add("Soc", db["Bsc"]["Soc"].AsDouble);
        //        trend_data.Add("Soh", db["Bsc"]["Soh"].AsDouble);
        //        trend_data.Add("PvActivePower", db["Pv"]["TotalActivePower"].AsDouble);
        //        trend_data.Add("pvVoltage", db["Pv"]["Voltage"]["R"].AsDouble);
        //        trend_data.Add("ACCurrent", db["Pcs"]["AC_phase_current"]["R"].AsDouble);
        //        trend_data.Add("pcs_status", db["Pcs"]["Status"].AsInt32);
        //        trend_data.Add("Frequency", db["Ess"]["Frequency"].AsDouble);
        //        trends.Add(trend_data);

        //    });

        //    return Ok(trends);

        //}

        //[HttpGet("allstat")]
        //public async Task<IActionResult> GetAllStat()
        //{
        //    UpdateSiteKeyByBsc();
        //    JArray array = new JArray();
        //    foreach(RedisKey bsc_key in AllSiteKeyByBsc)
        //    {
        //        JObject row = new JObject();
        //        string siteId = bsc_key.ToString().Split('_')[1];
        //        string pv_Key = $"site_{siteId}_PVMETER";
        //        string pcs_key = $"site_{siteId}_PCS";

        //        double pcs_activePwr = (double)await _db.HashGetAsync(pcs_key, "ActivePower");
        //        double pv_activePwr = (double)await _db.HashGetAsync(pv_Key, "TotalActivePower");
        //        double soc = (double)await _db.HashGetAsync(bsc_key, "Soc");
        //        row.Add("siteId", siteId);
        //        row.Add("soc_average", soc);
        //        row.Add("pv_activePower", pv_activePwr);
        //        row.Add("pcs_activePower", pcs_activePwr);
        //        array.Add(row);
        //    }

        //    return Ok(array);

        //}
        //[HttpGet("TotalAccumPower")]
        //public async Task<IActionResult> TotalAccumPower()
        //{
        //    UpdateSiteKeyByBsc();
        //    var contracts = mapreduce_db.GetCollection<BsonDocument>("ContractInfo");
        //    var cols = mapreduce_db.GetCollection<BsonDocument>("Statistics");
        //    string filter = $"_id: /^{DateTime.Now.Year}-{DateTime.Now.Month:0#}/";
        //    IAsyncCursor<BsonDocument> cursor = await cols.FindAsync("{" + filter + "}" );
        //    var result = await cursor.ToListAsync();
        //    double total_pv = 0;
        //    double total_chg = 0;
        //    double total_dhg = 0;
        //    double today_pv = 0;
        //    double today_chg = 0;
        //    double today_dhg = 0;
        //    if (result.Count > 0)
        //    {
        //         total_pv = result.Sum(x => x["value"]["pvactpwr"].AsDouble);
        //         total_chg = result.Sum(x => x["value"]["accumchg"].AsDouble);
        //         total_dhg = result.Sum(x => x["value"]["accumdhg"].AsDouble);
        //        string dt = DateTime.Today.ToString("yyyy-MM-dd");
        //        var today_row = result.FirstOrDefault(x => x["_id"].AsString == dt);
        //        if(today_row != null)
        //        {
        //            today_pv = today_row["value"]["pvactpwr"].AsDouble;
        //            today_chg = today_row["value"]["accumchg"].AsDouble;
        //            today_dhg = today_row["value"]["accumdhg"].AsDouble;
        //        }
        //    }

        //    JObject result_row = new JObject();
        //    result_row.Add("accum_dischargingMW", total_dhg);
        //    result_row.Add("accum_chargingMW", total_chg);
        //    result_row.Add("accum_energyMW", total_pv);
        //    result_row.Add("today_accum_energyMW", today_pv);
        //    result_row.Add("today_accum_chargingMW", today_chg);
        //    result_row.Add("today_accum_dischargingMW", today_dhg);



        //    List<double> soc_all = new List<double>();
        //    foreach (RedisKey key in AllSiteKeyByBsc)
        //    {
        //        double dValue;
        //        if(_db.HashGet(key, "Soc").TryParse(out dValue) && dValue != 0)
        //        {
        //            soc_all.Add(dValue);
        //        }
        //    }

        //    result_row.Add("soc_average", soc_all.Average());
        //    return Ok(result_row);

        //}

        //private void UpdateSiteKeyByBsc()
        //{
        //    if (AllSiteKeyByBsc.Count == 0)
        //    {
        //        //            KeyList.Clear();
        //        foreach (EndPoint endPoint in _redisConn.GetEndPoints())
        //        {
        //            string pattern = "site_*_BSC";
        //            IServer server = _redisConn.GetServer(endPoint);
        //            RedisKey[] Keys = server.Keys(pattern: pattern).ToArray();
        //            AllSiteKeyByBsc.AddRange(Keys);
        //        }
        //    }
        //}

        //[HttpGet("DailyAccumPowerBySite")]
        //public async Task<IActionResult> DailyAccumPowerBySite(DateTime date, int siteid)
        //{
        //    var cols = mapreduce_db.GetCollection<BsonDocument>("StatisticsBySite");
        //    var builder = Builders<BsonDocument>.Filter;
        //    var filter = builder.Eq("_id.siteid", siteid) & builder.Eq("_id.timestamp", date.ToString("yyyy-MM-dd"));
        //    IAsyncCursor<BsonDocument> cursor = await cols.FindAsync(filter);
        //    var result = await cursor.ToListAsync();
        //    if (result.Count > 0)
        //    {
        //        JObject jObject = new JObject();

        //        double accumEnergrykW = 0;
        //        double accum_chargingkW = 0;
        //        double accum_dischargingkW = 0;
        //        BsonValue doc = result.FirstOrDefault()["value"];
        //        accumEnergrykW = doc["accum_energykW"].AsDouble;
        //        accum_chargingkW = doc["accum_chargingkW"].AsDouble;
        //        accum_dischargingkW = doc["accum_dischargingkW"].AsDouble;
        //        jObject.Add("siteid", siteid);
        //        jObject.Add("request_date", date);
        //        jObject.Add("accum_energykW", accumEnergrykW);
        //        jObject.Add("accum_chargingkW", accum_chargingkW);
        //        jObject.Add("accum_dischargingkW", accum_dischargingkW);
        //        jObject.Add("last_update_timestmp", doc["timestamp"].AsString);
        //        return Ok(jObject);
        //    }
        //    else
        //    {
        //        return BadRequest();
        //    }
        //    //////List<ServiceModel> result_models = new List<ServiceModel>();
        //    ////await cursor.ForEachAsync(db =>
        //    ////{
        //    ////    JObject obj = JObject.FromObject(db);
        //    ////    jArray.Add(obj);
        //    ////});
        //    //return Ok(result);
        //}

        //[HttpGet("TotalAccumPowerBySite")]
        //public async Task<IActionResult> TotalAccumPowerBySite(int siteid)
        //{
        //    try
        //    {
        //        Console.WriteLine("CALL TOTALACCUMPOWERBYSITE");
        //        var cols = mapreduce_db.GetCollection<BsonDocument>("StatisticsBySite");
        //        var builder = Builders<BsonDocument>.Filter;
        //        var filter = builder.Eq("_id.siteid", siteid);
        //        IAsyncCursor<BsonDocument> cursor = await cols.FindAsync(filter);
        //        JObject jObject = new JObject();

        //        double accum_energykW = 0;
        //        double accum_chargingkW = 0;
        //        double accum_dischargingkW = 0;
        //        await cursor.ForEachAsync(db =>
        //        {
        //            accum_energykW += db["value"]["accum_energykW"].AsDouble;
        //            accum_chargingkW += db["value"]["accum_chargingkW"].AsDouble;
        //            accum_dischargingkW += db["value"]["accum_dischargingkW"].AsDouble;
        //        });

        //        jObject.Add("siteid", siteid);
        //        jObject.Add("accum_energykW", accum_energykW);
        //        jObject.Add("accum_chargingkW", accum_chargingkW);
        //        jObject.Add("accum_dischargingkW", accum_dischargingkW);
        //        return Ok(jObject);
        //    }
        //    catch(Exception ex)
        //    {
        //        logger.LogError(ex, "ERROR!! Call: TotalAccumPowerBySite / SiteId:" + siteid);
        //        return BadRequest(ex);
        //    }
        //    //////List<ServiceModel> result_models = new List<ServiceModel>();
        //    ////await cursor.ForEachAsync(db =>
        //    ////{
        //    ////    JObject obj = JObject.FromObject(db);
        //    ////    jArray.Add(obj);
        //    ////});
        //    //return Ok(result);
        //}
    }
}