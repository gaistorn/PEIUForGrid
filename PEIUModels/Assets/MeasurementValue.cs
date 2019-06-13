using System;
using System.Collections.Generic;
using System.Text;

namespace PES.Models
{
    public abstract class MeasurementValue : ValueTemplate
    {
        public override ValuesCategory Category => ValuesCategory.Measurement;
    }
}
