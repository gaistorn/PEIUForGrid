using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Power21.PEIUEcosystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PES.Models
{
    public class ContractModelBase
    {
        public virtual string ModelTypeCode { get; set; }
    }

    [JsonObject]
    public class ContractModel : ContractModelBase
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string CustomerFirstName { get; set; } = "";
        public string CustomerLastName { get; set; } = "";
        public string CustomerCompanyName { get; set; } = "";
        public string CustomerAddress1 { get; set; } = "";
        public string CustomerAddress2 { get; set; } = "";
        public string CustomerEmail { get; set; }
        public string ContactPhoneNumber { get; set; } = "";
        public string RegistrationNumber { get; set; } = "";
        public override string ModelTypeCode => "1";

        public List<AssetModel> Assets { get; set; } = new List<AssetModel>();
    }

    [JsonObject]
    public class LocationModel
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public static LocationModel Empty =>
            new LocationModel() { RCC = 0 };

        public short RCC { get; set; }
        public double Longtidue { get; set; }
        public double Latitude { get; set; }
        public string LawFirstCode { get; set; }
        public string LawMiddleCode { get; set; }
        public string LawLasttCode { get; set; }
        public string Address1 { get; set; } = "";
        public string Address2 { get; set; } = "";
    }

    [JsonObject]
    public class ServiceModel : ContractModelBase
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public string Describe { get; set; }

        public override string ModelTypeCode => "3";
    }

    [JsonObject]
    public class AssetModel
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string AssetName { get; set; } = "";
        public short DLNo { get; set; } = 0;
        public short SiteId { get; set; }
        public DateTime InstallDate { get; set; }
        public LocationModel Address { get; set; } = LocationModel.Empty;
        public float TotalAvaliableESSMountKW { get; set; } = 0;
        public float TotalAvaliablePVMountKW { get; set; } = 0;
        public float TotalAvaliablePCSMountKW { get; set; } = 0;
        public int ServiceCode { get; set; } = 0;
    }

    [Table("Assets")]
    public class AssetDBModel
    {
        [Key]
        public int PK { get; set; }
        public string AssetName { get; set; } = "";
        public short DLNo { get; set; } = 0;
        public short SiteId { get; set; }
        public DateTime InstallDate { get; set; }

        [ForeignKey("Address")]
        public int AddressId { get; set; }
        public AddressModel Address { get; set; }

        [ForeignKey("Account")]
        public string AccountId { get; set; }

        public AccountModel Account { get; set; }


        public float TotalAvaliableESSMountKW { get; set; } = 0;
        public float TotalAvaliablePVMountKW { get; set; } = 0;
        public float TotalAvaliablePCSMountKW { get; set; } = 0;
        public int ServiceCode { get; set; } = 0;
    }

    [Table("Address")]
    public class AddressModel
    {
        [Key]
        public int PK { get; set; }
        public short RCC { get; set; }
        public double Longtidue { get; set; }
        public double Latitude { get; set; }
        public string LawFirstCode { get; set; }
        public string LawMiddleCode { get; set; }
        public string LawLasttCode { get; set; }
        public string Address1 { get; set; } = "";
        public string Address2 { get; set; } = "";
    }
}
