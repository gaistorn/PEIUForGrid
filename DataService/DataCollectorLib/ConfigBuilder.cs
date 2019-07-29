using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PEIU.MicroProcessor
{
    public class ConfigBuilder
    {
        //private IConfigurationBuilder builder = null;
        //private readonly IConfigurationRoot Builder =  null;

        //public ConfigBuilder()
        //{
        //    builder = new ConfigurationBuilder()
        //        .SetBasePath(Directory.GetCurrentDirectory())
        //        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        //        .AddUserSecrets<ConfigBuilder>()
        //        .AddEnvironmentVariables();
        //    Builder = builder.Build();
           
        //}

        //public T GetValue<T>(string sectionName)
        //{
        //    return Builder.GetSection(sectionName).Get<T>();
        //}

        //public T Bind<T>(string sectionName) where T : class, new()
        //{
        //    T NewValue = new T();
        //    Builder.GetSection(sectionName).Bind(NewValue);
        //    return NewValue;
        //}

        //public string GetConnectionString(string sectionName)
        //{
        //    return Builder.GetConnectionString(sectionName);
        //}
    }
}
