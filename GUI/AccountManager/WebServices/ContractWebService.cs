using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PEIU.GUI.WebServices
{
    public static class ContractWebService
    {
        static string _address = "";
        public static void InitWebServerAddress(string address)
        {
            _address = address;
        }

        public static async Task<T> RequestGetMethod<T>(string Path, object QueryParams = null)
        {
            // Flurl will use 1 HttpClient instance per host
            var request = await _address
                .AppendPathSegment(Path)
                .SetQueryParams(QueryParams)
                .GetJsonAsync<T>();
            return request;
        }

        public static async Task<List<T>> RequestCollectionGetMethod<T>(string Path, object QueryParams = null)
        {
            // Flurl will use 1 HttpClient instance per host
            List<T> request = await _address
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
