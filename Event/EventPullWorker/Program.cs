using FireworksFramework.Mqtt;
using NHibernate;
using NHibernate.Cfg;
using PEIU.Event;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.Events
{
    class Program
    {
        readonly static CancellationTokenSource cancellationTokenSource;
        public readonly static NLog.ILogger logger;
        static Program()
        {
            cancellationTokenSource = new CancellationTokenSource();
            logger = NLog.LogManager.Configuration.LogFactory.GetLogger("");
        }
        static void Main(string[] args)
        {
            AppSetting appconfig = new AppSetting("appsettings.json");
            AbsMqttBase.SetDefaultLoggerName("nlog.config", true);
            string mysql_connectionString = appconfig.GetConnectionString("peiudb");
            ISessionFactory sessionFactory = CreateSessionFactory(mysql_connectionString, appconfig.GetAssembly());
            var redisConfiguration = appconfig.GetSection<RedisConfiguration>("redis");

            ConnectionMultiplexer conn =  ConnectionMultiplexer.Connect(redisConfiguration.ConfigurationOptions);
            // NLog: setup the logger first to catch all errors
            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("nlog.config");
            Console.CancelKeyPress += Console_CancelKeyPress;
            EventSubscribeWorker worker = new EventSubscribeWorker(sessionFactory, conn);

            CancellationToken token = cancellationTokenSource.Token;
            worker.Initialize();
            while(true)
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    Task workTask = worker.MqttSubscribeAsync(token);
                    workTask.Wait();
                    Task.Delay(100);
                }
                catch(Exception ex)
                {

                }
                
            }
           
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            logger.Warn("Terminating...");
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        }

        private static ISessionFactory CreateSessionFactory(string connString, Assembly ModelAssembly)
        {
            return new Configuration()
                            .AddProperties(new Dictionary<string, string> {
                    {NHibernate.Cfg.Environment.ConnectionDriver, typeof (NHibernate.Driver.MySqlDataDriver).FullName},
                   // {NHibernate.Cfg.Environment.ProxyFactoryFactoryClass, typeof (NHibernate.ByteCode.Castle.ProxyFactoryFactory).AssemblyQualifiedName},
                    {NHibernate.Cfg.Environment.Dialect, typeof (NHibernate.Dialect.MySQLDialect).FullName},
                    {NHibernate.Cfg.Environment.ConnectionProvider, typeof (NHibernate.Connection.DriverConnectionProvider).FullName},
                    {NHibernate.Cfg.Environment.ConnectionString, connString},
                    //{NHibernate.Cfg.Environment., connectionString},
                            {"hibernate.connection.CharSet", "utf-8"},
                            {"hibernate.connection.characterEncoding", "utf-8" },
                            {"hibernate.connection.useUnicode", "true" },

#if DEBUG
                            {NHibernate.Cfg.Environment.ShowSql, "false" }
#endif

                            })
                        .AddAssembly(ModelAssembly)

                        // .AddAssembly(Assembly.LoadFrom())
                        .BuildSessionFactory();
        }
    }
}
