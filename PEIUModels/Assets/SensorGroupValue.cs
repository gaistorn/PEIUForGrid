﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models
{
    public class SensorGroupValue : ValueTemplate
    {
        public override ValuesCategory Category => ValuesCategory.Sensor;
        public virtual SensorValue[] Sensors { get; set; }
    }
}
