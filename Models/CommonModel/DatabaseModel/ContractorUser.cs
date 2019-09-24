using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models.Database
{
    [System.ComponentModel.DataAnnotations.Schema.Table("ContractorUsers")]
    public class ContractorUser
    {
        [System.ComponentModel.DataAnnotations.Key]
        public string UserId { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("AggregatorGroup")]
        public string AggGroupId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual AggregatorGroup AggregatorGroup { get; set; }

        ////[System.ComponentModel.DataAnnotations.Schema.ForeignKey("User")]
        //public string UserId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual UserAccount User { get; set; }

        public virtual ICollection<ContractorSite> ContractorSite { get; set; } = new HashSet<ContractorSite>();


        public virtual ContractStatusCodes ContractStatus { get; set; } = ContractStatusCodes.Signing;

        

        public ContractorUser() { }
    }
}
