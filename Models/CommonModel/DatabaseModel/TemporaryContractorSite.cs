using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PEIU.Models.Database
{
    [Table("TemporaryContractorSites")]
    public class TemporaryContractorSite 
    {
        public TemporaryContractorSite() { }

        [Key]
        public virtual string UniqueId { get; set; }
        public virtual double Longtidue { get; set; }
        public virtual double Latitude { get; set; }
        public virtual string LawFirstCode { get; set; }
        public virtual string LawMiddleCode { get; set; }
        public virtual string LawLastCode { get; set; }
        public virtual string Address1 { get; set; }
        public virtual string Address2 { get; set; }

        [ForeignKey("ContractUser")]
        public  string ContractUserId { get; set; }

        public virtual ContractorUser ContractUser { get; set; }

        public virtual ServiceCodes ServiceCode { get; set; }
        public virtual DateTime RegisterTimestamp { get; set; }

        public virtual ICollection<TemporaryContractorAsset> ContractorAssets { get; set; }

    }

    //public interface ISiteLocation
    //{
    //    string Address1 { get; set; }
    //    string Address2 { get; set; }
    //    ContractorUser ContractUser { get; set; }

    //    DateTime RegisterTimestamp { get; set; }
    //}
}
