using System;
using System.Text;
using System.Collections.Generic;


namespace PEIU.Models
{

    public class BmsData
    {
        public virtual int UniqueId { get; set; }
        public virtual int groupId { get; set; }
        public virtual string groupName { get; set; }
        public virtual float? soc { get; set; }
        public virtual float? soh { get; set; }
        public virtual float? dcPwr { get; set; }
        public virtual float? dcVlt { get; set; }
        public virtual float? dcCrt { get; set; }
        public virtual float? cellMaxTemp { get; set; }
        public virtual float? cellMinTemp { get; set; }
        public virtual float? cellMaxVlt { get; set; }
        public virtual float? cellMinVlt { get; set; }
        public virtual string deviceId { get; set; }
        public virtual short? siteId { get; set; }
        public virtual DateTime? timestamp { get; set; }
    }
}
