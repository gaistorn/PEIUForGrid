using System;
using System.Text;
using System.Collections.Generic;


namespace DataModel {
    
    public class PcsSystem : DataObject
    {
        public virtual Deviceinfo Deviceinfo { get; set; }
        public virtual float? Frequency { get; set; }
        public virtual float? Acvlt { get; set; }
        public virtual float? Accrtlow { get; set; }
        public virtual float? Accrthigh { get; set; }
        public virtual float? Acpwr { get; set; }
        public virtual float? Actpwr { get; set; }
        public virtual float? Rctpwr { get; set; }
        public virtual float? Pf { get; set; }
        public virtual float? Acvltr { get; set; }
        public virtual float? Accrtr { get; set; }
        public virtual float? Acvlts { get; set; }
        public virtual float? Accrts { get; set; }
        public virtual float? Acvltt { get; set; }
        public virtual float? Accrtt { get; set; }
        public virtual float? Actpwrcmdlmtlowdhg { get; set; }
        public virtual float? Actpwrcmdlmthighdhg { get; set; }
        public virtual float? Actpwrcmdlmtlowchg { get; set; }
        public virtual float? Actpwrcmdlmthighchg { get; set; }
        public virtual float? Soc { get; set; }
        public virtual float? Soh { get; set; }
        public virtual float? Dcpwrkw { get; set; }
        public virtual float? Dcvlt { get; set; }
        public virtual float? Dccrt { get; set; }
        public virtual float? Cellmintemp { get; set; }
        public virtual float? Cellmaxtemp { get; set; }
        public virtual float? Cellminvlt { get; set; }
        public virtual float? Cellmaxvlt { get; set; }
    }
}
