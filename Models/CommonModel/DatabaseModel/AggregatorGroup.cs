using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PEIU.Models.Database
{
    [Table("AggregatorGroups")]
    public class AggregatorGroup : AggregatorGroupBase
    {
        [Key]
        public override string ID { get; set; }

        public virtual ICollection<AggregatorUser> AggregatorUsers { get; set; } = new HashSet<AggregatorUser>();

        public virtual ICollection<ContractorUser> ContractorUsers { get; set; } = new HashSet<ContractorUser>();

        public AggregatorGroup() { }
    }

    public class AggregatorGroupBase
    {
        public virtual string ID { get; set; }

        /// <summary>
        /// 어그리게이트 명
        /// </summary>
        public virtual string AggName { get; set; }

        /// <summary>
        /// 대표 상호명
        /// </summary>
        public virtual string Representation { get; set; }

        public virtual string Address { get; set; }

        public virtual DateTime CreateDT { get; set; }

        public virtual string PhoneNumber { get; set; }

        

        public AggregatorGroupBase() { }
    }
}
