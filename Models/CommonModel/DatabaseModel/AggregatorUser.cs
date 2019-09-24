using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PEIU.Models.Database
{
    [Table("AggregatorUsers")]
    public class AggregatorUser : AggregatorUserBase
    {
        [Key]
        public override string UserId { get; set; }

        [ForeignKey("AggregatorGroup")]
        public override string AggGroupId { get; set; }


        [Newtonsoft.Json.JsonIgnore]
        public virtual AggregatorGroup AggregatorGroup { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual UserAccount User { get; set; }

        public AggregatorUser() { }
    }

    public class AggregatorUserBase
    {
        public virtual string UserId { get; set; }

        [ForeignKey("AggregatorGroup")]
        public virtual string AggGroupId { get; set; }

        public AggregatorUserBase() { }
    }
}
