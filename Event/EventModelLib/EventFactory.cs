using NHibernate;
using NHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PEIU.Models
{
    public interface IEventDispatcher
    {
        void Open(string connectionString);
    }

    public class MysqlEventFactory : IEventDispatcher
    {
        ISessionFactory SessionFactory;
        public void Open(string connectionString)
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
