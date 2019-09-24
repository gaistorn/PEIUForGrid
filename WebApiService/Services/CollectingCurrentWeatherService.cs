using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PEIU.Models;
using PEIU.Models.Database;
using PEIU.Models.OWM;
using PES.Toolkit;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.Service.WebApiService
{
    public class CollectingCurrentWeatherService : BackgroundHostService
    {
        readonly ILogger<CollectingCurrentWeatherService> logger;
        readonly PeiuGridDataContext dbContext;
        readonly StackExchange.Redis.ConnectionMultiplexer connectionMultiplexer;
        readonly IDatabaseAsync redisDb;
        readonly string OpenWeatherMapAppId;
        private readonly IServiceScopeFactory scopeFactory;
        readonly TimeSpan RefreshRate;
        DateTime NextRetriveTime = DateTime.MinValue;
        readonly (double Lat, double Lon) ControlCenterLocation;
        public CollectingCurrentWeatherService(ILogger<CollectingCurrentWeatherService> _logger, 
            PeiuGridDataContext _peiuGridDataContext, IRedisConnectionFactory _redis, IConfiguration configuration, 
            IServiceScopeFactory scopeFactory)
        {
            connectionMultiplexer = _redis.Connection();
            redisDb = connectionMultiplexer.GetDatabase();
            logger = _logger;
            dbContext = _peiuGridDataContext;
            this.scopeFactory = scopeFactory;
            OpenWeatherMapAppId = configuration.GetSection("OpenWeatherAppId").Value;
            RefreshRate = configuration.GetSection("WeatherRefreshInterval").Get<TimeSpan>();
            ControlCenterLocation = (Lat: configuration.GetSection("ControlCenterGps:Lat").Get<double>(), Lon: configuration.GetSection("ControlCenterGps:Lon").Get<double>());
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(true)
            {
                stoppingToken.ThrowIfCancellationRequested();
                if (stoppingToken.IsCancellationRequested)
                    break;
                if (DateTime.Now > NextRetriveTime)
                {
                    try
                    {
                        var allOfSites = GetSites();
                        List<Currentweather> results = new List<Currentweather>();
                        foreach(VwContractorsite site in allOfSites)
                        {
                            ResponseWeather weather = await RequestWeatherInformation(site.Longtidue, site.Latitude);
                            if (weather == null)
                                continue;
                            Currentweather currentweather = ConvertWeather(site.SiteId, weather);
                            results.Add(currentweather);

                            stoppingToken.ThrowIfCancellationRequested();
                            if (stoppingToken.IsCancellationRequested)
                                break;
                        }

                        SaveDbAsync(results, stoppingToken);
                        
                        // Control Center Weather
                        ResponseWeather cc_weather = await RequestWeatherInformation(ControlCenterLocation.Lat, ControlCenterLocation.Lon);
                        if (cc_weather == null)
                            continue;
                        Currentweather cc_currentweather = ConvertWeather(0, cc_weather);
                        results.Add(cc_currentweather);
                        SaveRedis(results);
                    }
                    catch(Exception ex)
                    {
                        logger.LogError(ex, ex.Message);
                    }
                    finally
                    {
                        NextRetriveTime = DateTime.Now.Add(RefreshRate);
                    }
                    
                }
                await Task.Delay(10);
            }
        }

        private async void SaveRedis(List<Currentweather> results)
        {
            foreach(Currentweather weather in results)
            {
                string redisKey = "weather.sid" + weather.Siteid;
                HashEntry[] hashEntries = ConvertHashEntry<Currentweather>(weather, "ID");
                await redisDb.HashSetAsync(redisKey, hashEntries);
            }
        }

        private HashEntry[] ConvertHashEntry<T>(T src, params string[] MissingFields)
        {
            List<HashEntry> hashEntries = new List<HashEntry>();
            foreach(var pi in  typeof(T).GetProperties())
            {
                if (MissingFields.Contains(pi.Name))
                    continue;
                object value = pi.GetValue(src, null);
                if (value != null)
                {
                    HashEntry hashEntry = new HashEntry(pi.Name, value.ToString());
                    hashEntries.Add(hashEntry);
                }
            }
            return hashEntries.ToArray();
        }

        private async void SaveDbAsync(List<Currentweather> results, CancellationToken token)
        {
            using(var session = dbContext.SessionFactory.OpenStatelessSession())
            using(var transaction = session.BeginTransaction())
            {
                foreach(Currentweather responseWeather in results)
                {
                    await session.InsertAsync(responseWeather, token);
                }
                await transaction.CommitAsync(token);
            }
            
        }

        private Currentweather ConvertWeather(int siteId, ResponseWeather responseWeather)
        {
            Currentweather currentweather = new Currentweather();
            currentweather.Clouds = responseWeather.clouds?.all;

            if (responseWeather.weather.Count > 0)
            {
                Weather weather = responseWeather.weather[0];
                currentweather.Code = weather.id;
                currentweather.Description = weather.description;
                currentweather.Main = weather.main;
                currentweather.Icon = weather.icon;

            }
            if(responseWeather.rain != null)
            {
                currentweather.Rain1h = responseWeather.rain.Rain1h;
                currentweather.Rain3h = responseWeather.rain.Rain3h;
            }
            if(responseWeather.snow != null)
            {
                currentweather.Snow1h = responseWeather.snow.Snow1h;
                currentweather.Snow3h = responseWeather.snow.Snow3h;
            }

            currentweather.Temp = responseWeather.main.temp - 273.15f; //섭씨계산
            currentweather.TempMin = responseWeather.main.temp_min - 273.15f;
            currentweather.TempMax = responseWeather.main.temp_max - 273.15f;
            currentweather.Pressure = responseWeather.main.pressure;
            currentweather.Humidity = responseWeather.main.humidity;
            currentweather.Sunrise = ToDateTime(responseWeather.sys.sunrise);
            currentweather.Sunset = ToDateTime(responseWeather.sys.sunset);
            currentweather.Timestamp = ToDateTime(responseWeather.dt);
            currentweather.Siteid = siteId;
            currentweather.Cityname = responseWeather.name;
            currentweather.Lat = responseWeather.coord.lat;
            currentweather.Lon = responseWeather.coord.lon;
            return currentweather;
        }

        private DateTime ToDateTime(int unixtime)
        {
            DateTime utcTime = new DateTime(1970, 1, 1).AddSeconds(unixtime);
            return utcTime.ToLocalTime();
        }

        private IEnumerable<VwContractorsite> GetSites()
        {
            using(var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AccountDataContext>();
                return dbContext.VwContractorsites.ToArray();
            }
        }

        private async Task<ResponseWeather> RequestWeatherInformation(double lat, double lon)
        {
            try
            {
                /*Calling API http://openweathermap.org/api */
                HttpWebRequest apiRequest =
                WebRequest.Create($"http://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={OpenWeatherMapAppId}&lang=kr") as HttpWebRequest;

                string apiResponse = "";
                using (HttpWebResponse response = apiRequest.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    apiResponse = await reader.ReadToEndAsync();
                }
                /*End*/

                /*http://json2csharp.com*/
                ResponseWeather rootObject = JsonConvert.DeserializeObject<ResponseWeather>(apiResponse);
                return rootObject;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return null;
            }
        }
    }
}
