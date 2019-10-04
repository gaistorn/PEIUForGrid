using Microsoft.Extensions.Configuration;
using NHibernate;
using NHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EtriCommandAgent
{
    public interface IDataAccess
    {
        ISessionFactory SessionFactory { get; }
    }

    public class MysqlDataAccessSingleton
    {
        private ISessionFactory sessionFactory;
        public IStatelessSession GetStatelessSession()
        {
            return sessionFactory.OpenStatelessSession();
        }
        public MysqlDataAccessSingleton(IConfiguration config)
        {
            string connstr = config.GetConnectionString("etridb");
            InitSessionFactory(connstr);
        }

        private void InitSessionFactory(string connectionString)
        {
            sessionFactory = new Configuration()
                        .AddProperties(new Dictionary<string, string> {
                    {NHibernate.Cfg.Environment.ConnectionDriver, typeof (NHibernate.Driver.MySqlDataDriver).FullName},
                   // {NHibernate.Cfg.Environment.ProxyFactoryFactoryClass, typeof (NHibernate.ByteCode.Castle.ProxyFactoryFactory).AssemblyQualifiedName},
                    {NHibernate.Cfg.Environment.Dialect, typeof (NHibernate.Dialect.MySQLDialect).FullName},
                    {NHibernate.Cfg.Environment.ConnectionProvider, typeof (NHibernate.Connection.DriverConnectionProvider).FullName},
                    {NHibernate.Cfg.Environment.ConnectionString, connectionString},
                    //{NHibernate.Cfg.Environment., connectionString},
                            {"hibernate.connection.CharSet", "utf-8"},
                            {"hibernate.connection.characterEncoding", "utf-8" },
                            {"hibernate.connection.useUnicode", "true" },

#if DEBUG
                            {NHibernate.Cfg.Environment.ShowSql, "false" }
#endif

                        })
                    .AddAssembly(Assembly.GetExecutingAssembly())

                    // .AddAssembly(Assembly.LoadFrom())
                    .BuildSessionFactory();
        }
    }

    public class MysqlDataAccess : IDataAccess
    {
        public ISessionFactory SessionFactory { get; private set; }
        public MysqlDataAccess(string connectionString)
        {
            SessionFactory = new Configuration()
                        .AddProperties(new Dictionary<string, string> {
                    {NHibernate.Cfg.Environment.ConnectionDriver, typeof (NHibernate.Driver.MySqlDataDriver).FullName},
                   // {NHibernate.Cfg.Environment.ProxyFactoryFactoryClass, typeof (NHibernate.ByteCode.Castle.ProxyFactoryFactory).AssemblyQualifiedName},
                    {NHibernate.Cfg.Environment.Dialect, typeof (NHibernate.Dialect.MySQLDialect).FullName},
                    {NHibernate.Cfg.Environment.ConnectionProvider, typeof (NHibernate.Connection.DriverConnectionProvider).FullName},
                    {NHibernate.Cfg.Environment.ConnectionString, connectionString},
                    //{NHibernate.Cfg.Environment., connectionString},
                            {"hibernate.connection.CharSet", "utf-8"},
                            {"hibernate.connection.characterEncoding", "utf-8" },
                            {"hibernate.connection.useUnicode", "true" },

#if DEBUG
                            {NHibernate.Cfg.Environment.ShowSql, "false" }
#endif

                        })
                    .AddAssembly(Assembly.GetExecutingAssembly())
                   
                   // .AddAssembly(Assembly.LoadFrom())
                    .BuildSessionFactory();


        }
    }
}
