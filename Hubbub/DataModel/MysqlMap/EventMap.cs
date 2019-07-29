using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models
{
    public class EventMap
    {
        public virtual int EventId { get; set; }
        public virtual int Level { get; set; }
        public virtual int Type { get; set; }
        public virtual string Source { get; set; }
        public virtual string Category { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
    }
}
