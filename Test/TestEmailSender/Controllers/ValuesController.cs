using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PEIU.Models.OWM;
using PEIU.Service.WebApiService;

namespace TestEmailSender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        IEmailSender _mailSender;
        public ValuesController(IEmailSender sender)
        {
            _mailSender = sender;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost, Route("post")]
        public void Post([FromBody] JObject value)
        {
            float lat = value["lat"].Value<float>();
            float lon = value["lon"].Value<float>();
            /*Calling API http://openweathermap.org/api */
            string apiKey = "0e24126ab1639fb0301e58fb0f2a7009";
            HttpWebRequest apiRequest =
            WebRequest.Create($"http://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={apiKey}") as HttpWebRequest;

            string apiResponse = "";
            using (HttpWebResponse response = apiRequest.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                apiResponse = reader.ReadToEnd();
            }
            /*End*/

            /*http://json2csharp.com*/
            ResponseWeather rootObject = JsonConvert.DeserializeObject<ResponseWeather>(apiResponse);
        }

        [HttpPost, Route("mailsender")]
        public async void MailSender([FromBody] JObject value)
        {
            string[] address_list = value["address"].ToObject<string[]>();
            string title = value["title"].Value<string>();
            string contents = value["contents"].Value<string>();
            Console.WriteLine($"Try sending emails...{string.Join(',', address_list)}");
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            await _mailSender.SendEmailAsync("PIEU 운영팀", title, contents, address_list);
            Console.WriteLine($"Complete. Ellapse. {sw.Elapsed}");
            //foreach (var address in address_list)
            //{
            //    Console.WriteLine($"Try sending email...{address}");
            //    System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            //    await _mailSender.SendEmailAsync(address, title, contents);
            //    Console.WriteLine($"Complete. Ellapse. {sw.Elapsed}");
            //}
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
