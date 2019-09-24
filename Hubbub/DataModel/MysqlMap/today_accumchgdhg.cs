using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models
{
    public class TodayAccumchgdhg : Accumchgdhg
    {
        public virtual DateTime Timestamp { get; set; }
        //     <id name = "UniqueId" column="ID" />
        //<property name = "Timestamp" >
        //  < column name="timestamp" sql-type="datetime" not-null="false" />
        //</property>
        //<property name = "SiteId" >
        //  < column name="siteId" sql-type="int(11)" not-null="true" />
        //</property>
        //<property name = "Charging" >
        //  < column name="chg" sql-type="float" not-null="false" />
        //</property>
        //<property name = "Discharging" >
        //  < column name="dhg" sql-type="float" not-null="false" />
        //</property>    
    }

    public class MonthlyAccumchgdhg : Accumchgdhg
    {
        public virtual int Year { get; set; }
        public virtual int Month { get; set; }
    }

    public class Accumchgdhg
    {
        public virtual int UniqueId { get; set; }
        

        public virtual int SiteId { get; set; }
        public virtual float Charging { get; set; }
        public virtual float Discharging { get; set; }
    }
}
