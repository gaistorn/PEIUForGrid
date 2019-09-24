using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

using Flurl.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PEIU.GUI.WebServices
{
    public static class ContractWebService
    {
        public static string WebAddress => Properties.Settings.Default.WebServiceUrl;
        private static string Token = null;

        public static async Task<JObject> LoginRequest(string email, string password)
        {
            try
            {
                string fulurl = Properties.Settings.Default.WebServiceUrl;

                HttpClientHandler httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain,
                  errors) => true;
                
                HttpClient httpClient = new HttpClient(httpClientHandler);
                httpClient.BaseAddress = new Uri(fulurl);
                var flurlClient = new FlurlClient(httpClient);
                var cr = await flurlClient.AllowHttpStatus(HttpStatusCode.NotFound, HttpStatusCode.Conflict).AllowHttpStatus("400-404,6xx")
                    .Request("api", "auth", "login").PostJsonAsync(new { Email = email, Password = password, RememberMe = false }).ReceiveJson<JObject>();

                //using (FlurlClient cli = new FlurlClient(fulurl))//.WithOAUthBearerToken(token))
                //{
                //    var cr = await cli.Request("api", "auth", "login").PostJsonAsync(new { Email = email, Password = password, RememberMe = false }).ReceiveJson();

                //}


                return cr;
            }
            catch(FlurlHttpException ex)
            {
                JObject e = await ex.GetResponseJsonAsync<JObject>();

                // For error responses that take an unknown shape
                dynamic d = await ex.GetResponseJsonAsync();
                return null;
            }
        }

        public static async Task RequestPostMethod(Action<bool, IEnumerable<(string code, string description)>> ErrorHandler, string Path, object QueryBody)
        {
            if (Token == null)
            {
                dynamic login_request = await LoginRequest("cge@power21.co.kr", "power211234/");
                bool result = login_request.result.succeeded;
                if (result == false)
                {
                    ErrorHandler(result, login_request.errors);
                }
                else
                {
                    Token = login_request.token;
                }
            }
            string bearer = "Bearer " + Token;
            var request = await WebAddress
                .WithHeader("Accept", "application/json")
                .WithHeader("Authorization", bearer)
                .AppendPathSegment(Path)
                .PostJsonAsync(QueryBody).ReceiveJson();

        }

        public static async Task<T> RequestGetMethod<T>(Action<bool,IEnumerable<(string code,string description)>> ErrorHandler, string Path, object QueryParams = null)
        {
            if(Token == null)
            {
                dynamic login_request = await LoginRequest("cge@power21.co.kr", "power211234/");
                bool result = login_request.result.succeeded;
                if(result == false)
                {
                    ErrorHandler(result, login_request.errors);
                }
                else
                {
                    Token = login_request.token;
                }
            }
            string bearer = "Bearer " + Token;

            // Flurl will use 1 HttpClient instance per host
            var request = await WebAddress
                .WithHeader("Accept", "application/json")
                .WithHeader("Authorization", bearer)
                .AppendPathSegment(Path)
                .SetQueryParams(QueryParams)
                .GetJsonAsync<T>();
            return request;
        }

        public static async Task<List<T>> RequestCollectionGetMethod<T>(Action<bool, IEnumerable<(string code, string description)>> ErrorHandler, string Path, object QueryParams = null)
        {
            if (Token == null)
            {
                dynamic login_request = await LoginRequest("cge@power21.co.kr", "power211234/");
                bool result = login_request.result.succeeded;
                if (result == false)
                {
                    ErrorHandler(result, login_request.errors);
                }
                else
                {
                    Token = login_request.token;
                }
            }
            string bearer = "Bearer " + Token;
            // Flurl will use 1 HttpClient instance per host
            List<T> request = await WebAddress
                .WithHeader("Accept", "application/json")
                .WithHeader("Authorization", bearer)
                .AppendPathSegment(Path)
                .SetQueryParams(QueryParams)
                .GetJsonAsync<List<T>>();


            //List<T> data = new List<T>();
            //foreach(JObject item in request)
            //{
            //    string str_data = item.ToString();
            //    T oioi = item.ToObject<T>();
            //    T t_data = JsonConvert.DeserializeObject<T>(str_data);
            //    data.Add(t_data);
            //}
            return request;
        }
    }
}
