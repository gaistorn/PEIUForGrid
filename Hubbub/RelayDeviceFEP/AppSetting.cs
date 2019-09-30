using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RelayDeviceFEP
{
    public class AppSetting
    {
        private IConfiguration config;
        public AppSetting(string appsettingjsonfile)
        {
             config = new ConfigurationBuilder()
            .AddJsonFile(appsettingjsonfile, true, true)
            .Build();
        }

        public T GetSection<T>(string sectionName)
        {
            return config.GetSection(sectionName).Get<T>();
        }

        public string GetConnectionString(string sectionName)
        {
            return config.GetConnectionString(sectionName);
        }

        public Assembly GetAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }
    }
}
