using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models
{
    public class BSCSampleValue : SampleValue
    {
        /// <summary>
        /// 역률
        /// </summary>
        public double PwrFactor { get; set; }

        /// <summary>
        /// 주파수
        /// </summary>
        public double Frequency { get; set; }
    }
}
