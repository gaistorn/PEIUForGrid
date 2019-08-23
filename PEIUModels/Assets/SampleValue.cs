using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models
{
    public abstract class SampleValue : ValueTemplate
    {
        public override ValuesCategory Category => ValuesCategory.Sample;
    }
}
