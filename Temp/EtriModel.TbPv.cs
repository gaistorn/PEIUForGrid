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
    /// There are no comments for RelayDeviceFEP.TbPv, RelayDeviceFEP in the schema.
    /// </summary>
    public partial class TbPv {
    
        #region Extensibility Method Definitions
        
        /// <summary>
        /// There are no comments for OnCreated in the schema.
        /// </summary>
        partial void OnCreated();
        
        #endregion
        /// <summary>
        /// There are no comments for TbPv constructor in the schema.
        /// </summary>
        public TbPv()
        {
            this.Energy1 = 0f;
            this.Energy2 = 0f;
            this.Energy3 = 0f;
            this.Energy4 = 0f;
            this.Power1 = 0f;
            this.Power2 = 0f;
            this.Power3 = 0f;
            this.Power4 = 0f;
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
        /// There are no comments for Energy1 in the schema.
        /// </summary>
        public virtual float Energy1
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Energy2 in the schema.
        /// </summary>
        public virtual float Energy2
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Energy3 in the schema.
        /// </summary>
        public virtual float Energy3
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Energy4 in the schema.
        /// </summary>
        public virtual float Energy4
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Power1 in the schema.
        /// </summary>
        public virtual float Power1
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Power2 in the schema.
        /// </summary>
        public virtual float Power2
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Power3 in the schema.
        /// </summary>
        public virtual float Power3
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Power4 in the schema.
        /// </summary>
        public virtual float Power4
        {
            get;
            set;
        }
    }

}
