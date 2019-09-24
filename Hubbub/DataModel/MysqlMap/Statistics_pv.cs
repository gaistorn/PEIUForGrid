using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models
{
    public class Statistics_pv
    {
        public virtual int ID { get; set; }
        public virtual DateTime Timestamp { get; set; }

        public virtual float AccumPvPower { get; set; }

        public virtual int SiteId { get; set; }
        public virtual string DeviceId { get; set; }
    }

    public class MinuteStatistics_pv : Statistics_pv
    {

    }

    public class HourlyStatistics_pv : Statistics_pv
    {

    }

    public class DailyStatistics_pv : Statistics_pv
    {

    }
}
