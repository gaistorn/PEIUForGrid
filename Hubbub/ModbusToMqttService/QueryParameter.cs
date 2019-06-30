using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PEIU.Hubbub
{
    public class QueryParameter
    {
        public string DeviceId { get; set; }
        public string[] Fields { get; set; }
    }

    public class ManualQueryParameter
    {
        public ushort StartAddress { get; set; }
        public ushort Length { get; set; }
    }
}
