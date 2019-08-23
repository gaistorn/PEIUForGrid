using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models
{
    public class Site
    {
        [BsonId]
        public ObjectId _id { get; set; }

        public string SiteId { get; set; }

        public GeoLocation Location { get; set; }
        
        public string CustomerId { get; set; }

        public Site()
        {
        }
    }
}
