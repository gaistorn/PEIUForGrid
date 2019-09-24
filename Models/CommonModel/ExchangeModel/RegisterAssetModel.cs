using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models.ExchangeModel
{
    public class RegisterAssetModel
    {
        public AssetTypeCodes Type { get; set; }
        public double CapacityMW { get; set; }
        
        public int Index { get; set; }
    }
}
