using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PES.Models;
using PES.Service.WebApiService.Localization;
using Power21.PEIUEcosystem.Models;

namespace PES.Service.WebApiService.Controllers
{
    [Route("api/contract")]
    [Produces("application/json")]
    //[Authorize]
    //[EnableCors(origins: "http://www.peiu.co.kr:3011", headers: "*", methods: "*")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly UserManager<AccountModel> userManager;
        ILogger<ContractController> logger;
        AccountRecordContext accountContext;
        private readonly LocalizedIdentityErrorDescriber describer;
        public ContractController(IConfiguration configuration, ILoggerFactory loggerFactory, AccountRecordContext _accountContext, UserManager<AccountModel> _userManager)
        {
            logger = loggerFactory.CreateLogger<ContractController>();
            accountContext = _accountContext;
            userManager = _userManager;
            describer = userManager.ErrorDescriber as LocalizedIdentityErrorDescriber;
        }

        //[HttpPost("register")]
        //public async Task<IActionResult> RegisterContract([FromBody] ContractModel param)
        //{
        //    var cols = monogodb.GetCollection<ContractModel>("ContractInfo");
        //    await cols.InsertOneAsync(param);

        //    return Ok();
        //}
       
        [HttpGet("getAsset")]
        public async Task<IActionResult> GetAllAsset(int? rcc = null)
        {
            IEnumerable<AssetLocation> result = null;
            JArray return_value = new JArray();
            if (rcc != null)
            {
                result = accountContext.AssetLocations.Where(x => x.RCC == rcc.Value);
            }
            else
                result = accountContext.AssetLocations;

            foreach(AssetLocation loc in result)
            {
                JObject obj = JObject.FromObject(loc);
                return_value.Add(obj);
            }
            return Ok(return_value);
        }

        [HttpGet("getClusterValuesByRCC")]
        public async Task<IActionResult> getClusterValuesByRCC()
        {
            JArray result = new JArray();
            for(int rcc = 1;rcc<=15;rcc++)
            {
                AssetLocation ac = accountContext.AssetLocations.FirstOrDefault(x => x.RCC == rcc);
                if(ac == null)
                {
                    result.Add("");
                }
                else
                {

                }
            }
            accountContext.AssetLocations
            
            return Ok(result);
        }

        [HttpGet("getcontractorlist")]
        public async Task<IActionResult> GetContractorList(string modeltypecode)
        {
            ContractModel testModel = new ContractModel();
            testModel.Assets.Add(new AssetModel());

            string model =  Newtonsoft.Json.JsonConvert.SerializeObject(testModel);

            var cols = monogodb.GetCollection<ContractModel>("ContractInfo");
            var filter = "{ ModelTypeCode: '" + modeltypecode + "'}";
            IAsyncCursor<ContractModel> cursor = await cols.FindAsync(filter);
            JArray jArray = new JArray();

            //List<ServiceModel> result_models = new List<ServiceModel>();
            await cursor.ForEachAsync(db =>
            {
                JObject obj = JObject.FromObject(db);
                jArray.Add(obj);
            });
            return Ok(jArray);
        }

        [HttpGet("getservicelist")]
        public async Task<IActionResult> GetServiceList()
        {
            var cols = monogodb.GetCollection<ServiceModel>("ContractInfo");

            //ServiceModel[] defaultModels = new ServiceModel[]
            //{
            //    new ServiceModel() { ServiceCode = 100, ServiceName = "스케쥴링", Describe = "스케쥴링 알고리즘"},
            //    new ServiceModel() { ServiceCode = 101, ServiceName = "Peak-cut", Describe = "피크컷 알고리즘"},
            //    new ServiceModel() { ServiceCode = 102, ServiceName = "주파수 조정(Frequency Regulation)", Describe = "주파수 조정 알고리즘"},
            //    new ServiceModel() { ServiceCode = 100, ServiceName = "DR", Describe = "수요반응"}
            //};

            //await cols.InsertManyAsync(defaultModels);

            var filter = "{ ModelTypeCode: '3'}";
            IAsyncCursor<ServiceModel> cursor = await cols.FindAsync(filter);
            JArray jArray = new JArray();

            //List<ServiceModel> result_models = new List<ServiceModel>();
            await cursor.ForEachAsync(db =>
            {
                JObject obj = JObject.FromObject(db);
                jArray.Add(obj);
            });
            return Ok(jArray);

        }

        //// GET: api/Contract
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET: api/Contract/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/Contract
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Contract/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
