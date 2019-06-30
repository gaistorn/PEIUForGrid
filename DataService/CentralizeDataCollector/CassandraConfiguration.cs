using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.DataServices
{
    public class CassandraConfiguration
    {
        public string[] Hosts { get; set; }
        public int BatchCount { get; set; }
        public TimeSpan WaitForBatch { get; set; }
    }
}
