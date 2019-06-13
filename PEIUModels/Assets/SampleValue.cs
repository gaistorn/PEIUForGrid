using System;
using System.Collections.Generic;
using System.Text;

namespace PES.Models
{
    public abstract class SampleValue : ValueTemplate
    {
        public override ValuesCategory Category => ValuesCategory.Sample;
    }
}
