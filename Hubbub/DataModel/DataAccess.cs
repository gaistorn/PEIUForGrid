using NHibernate;
using NHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataModel
{
    public class DataAccessManager
    {
        internal ISessionFactory SessionFactory { get; private set; }
        public DataAccessManager(string connectionString)
        {
            SessionFactory = new Configuration()
                        .AddProperties(new Dictionary<string, string> {
                    {NHibernate.Cfg.Environment.ConnectionDriver, typeof (NHibernate.Driver.SQLite20Driver).FullName},
                   // {NHibernate.Cfg.Environment.ProxyFactoryFactoryClass, typeof (NHibernate.ByteCode.Castle.ProxyFactoryFactory).AssemblyQualifiedName},
                    {NHibernate.Cfg.Environment.Dialect, typeof (NHibernate.Dialect.SQLiteDialect).FullName},
                    {NHibernate.Cfg.Environment.ConnectionProvider, typeof (NHibernate.Connection.DriverConnectionProvider).FullName},
                    {NHibernate.Cfg.Environment.ConnectionString, connectionString},
                    })
                    .AddAssembly(Assembly.GetExecutingAssembly()).BuildSessionFactory();


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

        public DataAccess(DataAccessManager manager)
        {
            _session = manager.SessionFactory.OpenSession();
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
