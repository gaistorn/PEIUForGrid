using NHibernate;
using NHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PEIU.Models
{
    public interface IDataAccessManager
    {
        ISessionFactory CreateSessionFactory(string connectionString);
    }

    public class MsSqlAccessManager : IDataAccessManager
    {
        public ISessionFactory CreateSessionFactory(string conn)
        {
            var SessionFactory = new Configuration()
                        .AddProperties(new Dictionary<string, string> {
                    {NHibernate.Cfg.Environment.ConnectionDriver, typeof (NHibernate.Driver.SqlClientDriver).FullName},
                   // {NHibernate.Cfg.Environment.ProxyFactoryFactoryClass, typeof (NHibernate.ByteCode.Castle.ProxyFactoryFactory).AssemblyQualifiedName},
                    {NHibernate.Cfg.Environment.Dialect, typeof (NHibernate.Dialect.MsSql2012Dialect).FullName},
                    {NHibernate.Cfg.Environment.ConnectionProvider, typeof (NHibernate.Connection.DriverConnectionProvider).FullName},
                    {NHibernate.Cfg.Environment.ConnectionString, conn},
                    })
                    .AddAssembly(Assembly.GetExecutingAssembly()).BuildSessionFactory();
            return SessionFactory;
        }
    }

    public class SqliteAccessManager : IDataAccessManager
    {
        public ISessionFactory SessionFactory { get; private set; }

        public SqliteAccessManager(string connectionString)
        {
            CreateSessionFactory(connectionString);
        }
        public ISessionFactory CreateSessionFactory(string connectionString)
        {
            SessionFactory = new Configuration()
                        .AddProperties(new Dictionary<string, string> {
                    {NHibernate.Cfg.Environment.ConnectionDriver, typeof (NHibernate.Driver.SQLite20Driver).FullName},
                   // {NHibernate.Cfg.Environment.ProxyFactoryFactoryClass, typeof (NHibernate.ByteCode.Castle.ProxyFactoryFactory).AssemblyQualifiedName},
                    {NHibernate.Cfg.Environment.Dialect, typeof (NHibernate.Dialect.SQLiteDialect).FullName},
                    {NHibernate.Cfg.Environment.ConnectionProvider, typeof (NHibernate.Connection.DriverConnectionProvider).FullName},
                    {NHibernate.Cfg.Environment.ConnectionString, connectionString}
#if DEBUG
                            ,{NHibernate.Cfg.Environment.ShowSql, "true" }
#endif

                        })
                    .AddAssembly(Assembly.GetExecutingAssembly()).BuildSessionFactory();
            return SessionFactory;
        }
    }

    public class MySqlAccessManager
    {
        public ISessionFactory SessionFactory { get; private set; }

        public MySqlAccessManager(string connectionString, params Assembly[] Assemblies)
        {
            CreateSessionFactory(connectionString, Assemblies);
        }

        public ISessionFactory CreateSessionFactory(string connectionString, params Assembly[] Assemblies)
        {
            var conf = new Configuration()
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

//#if DEBUG
//                            {NHibernate.Cfg.Environment.ShowSql, "false" }
//#endif

                       });
            foreach (Assembly type in Assemblies)
                conf = conf.AddAssembly(type);



            SessionFactory = conf.BuildSessionFactory();
            return SessionFactory;
        }
    }

    public class DataAccess : IDisposable
    {
        ISession _session;
        public void Dispose()
        {
            if (_session != null)
                _session.Dispose();
        }

        public DataAccess(ISessionFactory sessionFactory)
        {
            _session = sessionFactory.OpenSession();
        }

        public TEntity Load<TEntity>(object id) where TEntity : class
        {

            return _session.Load<TEntity>(id);
        }

        public TEntity Get<TEntity>(object id) where TEntity : class
        {
            
            return _session.Get<TEntity>(id);
        }

        public IList<TEntity> Select<TEntity>() where TEntity : class
        {
            return _session.CreateCriteria<TEntity>().List<TEntity>();
        }
    }


}
