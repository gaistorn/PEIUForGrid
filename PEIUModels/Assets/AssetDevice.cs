using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models
{
    public abstract class AssetDevice
    {
        [BsonId]
        public ObjectId _id { get; set; }

        public abstract DeviceType AssetType { get; }

        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string SiteId { get; set; }
    }

}
