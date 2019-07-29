using Microsoft.Extensions.Configuration;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PES.Service.WebApiService
{
    public class PeiuGridDataContext
    {
        public NHibernate.ISessionFactory SessionFactory => _da.SessionFactory;
        MysqlDataAccess _da;
        public PeiuGridDataContext(IConfiguration configuration)
        {
            string mysql_conn = configuration.GetConnectionString("peiudb");
            _da = new MysqlDataAccess(mysql_conn);
        }
    }
}
