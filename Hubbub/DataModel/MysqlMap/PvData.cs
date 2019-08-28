using System;
using System.Text;
using System.Collections.Generic;


namespace PEIU.Models
{

    public class PvData
    {
        public virtual  int UniqueId { get; set; }
        public virtual  string groupName { get; set; }
        public virtual  int groupId { get; set; }
        public virtual  float TotalActivePower { get; set; }
        public virtual  float TotalReactivePower { get; set; }
        public virtual  float ReverseActivePower { get; set; }
        public virtual  float ReverseReactivePower { get; set; }
        public virtual  float vltR { get; set; }
        public virtual  float vltS { get; set; }
        public virtual  float vltT { get; set; }
        public virtual  float crtR { get; set; }
        public virtual  float crtS { get; set; }
        public virtual  float crtT { get; set; }
        public virtual  float Frequency { get; set; }
        public virtual  float EnergyTotalActivePower { get; set; }
        public virtual  float EnergyTotalReactivePower { get; set; }
        public virtual  float EnergyTotalReverseActivePower { get; set; }
        public virtual  string deviceId { get; set; }
        public virtual short? siteId { get; set; }
        public virtual  DateTime timestamp { get; set; }
    }
}
