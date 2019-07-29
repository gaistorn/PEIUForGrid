using System;
using System.Text;
using System.Collections.Generic;


namespace PEIU.Models
{

    public class PcsData
    {
        public virtual  int UniqueId { get; set; }
        public virtual  string groupName { get; set; }
        public virtual  int groupId { get; set; }
        public virtual  float freq { get; set; }
        public virtual  float acVlt { get; set; }
        public virtual  float acCrtLow { get; set; }
        public virtual  float acCrtHigh { get; set; }
        public virtual  float acPwr { get; set; }
        public virtual  float actPwr { get; set; }
        public virtual  float rctPwr { get; set; }
        public virtual  float pf { get; set; }
        public virtual  float acVltR { get; set; }
        public virtual  float acCrtR { get; set; }
        public virtual  float acVltS { get; set; }
        public virtual  float acCrtS { get; set; }
        public virtual  float acVltT { get; set; }
        public virtual  float acCrtT { get; set; }
        public virtual  float actPwrCmdLmtLowDhg { get; set; }
        public virtual  float actPwrCmdLmtHighDhg { get; set; }
        public virtual  float actPwrCmdLmtLowChg { get; set; }
        public virtual  float actPwrCmdLmtHighChg { get; set; }
        public virtual  string deviceId { get; set; }
        public virtual short? siteId { get; set; }
        public virtual  DateTime timestamp { get; set; }
    }
}
