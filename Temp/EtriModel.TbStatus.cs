﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using NHibernate template.
// Code is generated on: 2019-09-30 오후 8:23:41
//
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace RelayDeviceFEP
{

    /// <summary>
    /// There are no comments for RelayDeviceFEP.TbStatus, RelayDeviceFEP in the schema.
    /// </summary>
    public partial class TbStatus {
    
        #region Extensibility Method Definitions
        
        /// <summary>
        /// There are no comments for OnCreated in the schema.
        /// </summary>
        partial void OnCreated();
        
        #endregion
        /// <summary>
        /// There are no comments for TbStatus constructor in the schema.
        /// </summary>
        public TbStatus()
        {
            this.Pv = 0f;
            this.PvEng = 0f;
            this.Ess = 0f;
            this.EssCh = 0f;
            this.EssDch = 0f;
            this.Soc1 = 0f;
            this.Soc2 = 0f;
            this.Soc3 = 0f;
            this.Soc4 = 0f;
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for Date in the schema.
        /// </summary>
        public virtual System.DateTime Date
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Pv in the schema.
        /// </summary>
        public virtual float Pv
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for PvEng in the schema.
        /// </summary>
        public virtual float PvEng
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Ess in the schema.
        /// </summary>
        public virtual float Ess
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for EssCh in the schema.
        /// </summary>
        public virtual float EssCh
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for EssDch in the schema.
        /// </summary>
        public virtual float EssDch
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Soc1 in the schema.
        /// </summary>
        public virtual float Soc1
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Soc2 in the schema.
        /// </summary>
        public virtual float Soc2
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Soc3 in the schema.
        /// </summary>
        public virtual float Soc3
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Soc4 in the schema.
        /// </summary>
        public virtual float Soc4
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Stat1 in the schema.
        /// </summary>
        public virtual string Stat1
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Stat2 in the schema.
        /// </summary>
        public virtual string Stat2
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Stat3 in the schema.
        /// </summary>
        public virtual string Stat3
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Stat4 in the schema.
        /// </summary>
        public virtual string Stat4
        {
            get;
            set;
        }
    }

}
