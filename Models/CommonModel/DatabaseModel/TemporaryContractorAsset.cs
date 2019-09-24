using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PEIU.Models.Database
{
    [Table("TemporaryContractorAssets")]
    public class TemporaryContractorAsset
    {
        [Key]
        public string UniqueId { get; set; }

        [ForeignKey("ContractorSite")]
        public string SiteId { get; set; }

        public TemporaryContractorSite ContractorSite { get; set; }

        public AssetTypeCodes AssetType { get; set; }

        public double CapacityKW { get; set; }

        public string AssetName { get; set; }

        public TemporaryContractorAsset() { }
    }
}
