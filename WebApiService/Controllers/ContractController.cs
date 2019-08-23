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
using PEIU.Models;
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

        [HttpGet("getAssetByCustomer")]
        public async Task<IActionResult> getAssetByCustomer(string email = null)
        {
            IEnumerable<AssetLocation> result = null;
            JArray return_value = new JArray();
            result = accountContext.AssetLocations.Where(x => x.AccountId == email);

            foreach (AssetLocation loc in result)
            {
                JObject obj = JObject.FromObject(loc);
                return_value.Add(obj);
            }
            return Ok(return_value);
        }

        [HttpGet("getAsset")]
        public async Task<IActionResult> GetAllAsset(int? siteId = null)
        {
            IEnumerable<AssetLocation> result = null;
            JArray return_value = new JArray();
            if (siteId != null)
            {
                result = accountContext.AssetLocations.Where(x => x.SiteId == siteId.Value);
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

        [HttpPost, Route("siteregister")]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> SiteRegister([FromBody] ReservedSiteRegisterModel model)
        {

            //model.Email = value["Email"].ToString();
            //model.Password = value["Password"].ToString();
            //model.ConfirmPassword = value["ConfirmPassword"].ToString();
            //model.Username
            if (ModelState.IsValid)
            {
                var user = new ReservedAssetLocation
                {
                    AccountId = model.AccountId,
                    Address1 = model.Address1,
                    Address2 = model.Address2,
                    ControlOwner = model.ControlOwner,
                    SiteInformation = model.SiteInformation,
                    RegisterTimestamp = DateTime.Now
                };
                accountContext.ReservedAssetLocations.Add(user);
                await accountContext.SaveChangesAsync();
                return Ok();
            }

            // If we got this far, something failed, redisplay form
            return Ok(StatusCodes.Status400BadRequest);
        }

        [HttpGet("getreservedregisters")]
        public async Task<IActionResult> GetReservedRegisters()
        {
            JArray array = new JArray();
            foreach (var account in accountContext.ReservedAssetLocations)
            {
                JObject row = JObject.FromObject(account);
                array.Add(row);
            }
            return Ok(array);
        }

        [HttpGet("getcontractorlist")]
        public async Task<IActionResult> GetContractorList()
        {
            JArray array = new JArray();
            foreach(AccountModel account in accountContext.Users)
            {
                JObject row = JObject.FromObject(account);
                array.Add(row);
            }
            return Ok(array);
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
