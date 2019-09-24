using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PEIU.Models.Database
{
    [Table("ContractorAssets")]
    public class ContractorAsset
    {
        [Key]
        public int UniqueId { get; set; }

        [ForeignKey("ContractorSite")]
        public int SiteId { get; set; }

        [JsonIgnore]
        public ContractorSite ContractorSite { get; set; }

        public AssetTypeCodes AssetType { get; set; }

        public double CapacityKW { get; set; }

        public DateTime InstallDate { get; set; }
        public DateTime LastMaintenance { get; set; }

        public string DeviceId { get; set; }

        public ContractorAsset() { }
    }
}
