using System;
using System.Text;
using System.Collections.Generic;


namespace DataModel
{

    public class INVERTER_DATA_RT
    {
        public virtual long ID_CODE { get; set; }
        public virtual string INVERTER_ID { get; set; }
        public virtual float? ID_POWER { get; set; }
        public virtual float? ID_VOLTAGE { get; set; }
        public virtual float? ID_CURRENT { get; set; }
        public virtual DateTime ID_DATE { get; set; }
        public virtual string CM_KEY_COUNTRY { get; set; }
        public virtual string CM_KEY_ZONE { get; set; }
        public virtual string CM_KEY_EXCHANGE { get; set; }
        public virtual string CM_KEY_NUMBER { get; set; }
        public virtual float? A_VOLTAGE { get; set; }
        public virtual float? B_VOLTAGE { get; set; }
        public virtual float? C_VOLTAGE { get; set; }
        public virtual float? A_CURRENT { get; set; }
        public virtual float? B_CURRENT { get; set; }
        public virtual float? C_CURRENT { get; set; }
        public virtual float? G_POWER { get; set; }
        public virtual float? TOTAL_POWER { get; set; }
        public virtual float? FREQUENCY { get; set; }
        public virtual float? PV_ACCU_POWER { get; set; }
        public virtual float? POWER_FACTOR { get; set; }
        public virtual float? TODAY_POWER { get; set; }
        public virtual float? PREVIOUS_POWER { get; set; }
        public virtual string SENSORBOX_ID { get; set; }
        public virtual float? H_SOLAR { get; set; }
        public virtual float? I_SOLAR { get; set; }
        public virtual float? A_TEMPERATURE { get; set; }
        public virtual float? S_TEMPERATURE { get; set; }
        public virtual string STATE_LPL { get; set; }
        public virtual string STATE_RUN { get; set; }
        public virtual string STATE_PROTECTION { get; set; }
        public virtual string STATE_A { get; set; }
        public virtual string STATE_B { get; set; }
        public virtual string STATE_C { get; set; }
    }
}
