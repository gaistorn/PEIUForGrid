﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using NHibernate template.
// Code is generated on: 2019-10-08 오후 12:58:35
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

namespace PEIU.Events.Alarm
{

    /// <summary>
    /// There are no comments for PEIU.Events.Alarm.ModbusFactory, EventModelLib in the schema.
    /// </summary>
    public partial class ModbusFactory {
    
        #region Extensibility Method Definitions
        
        /// <summary>
        /// There are no comments for OnCreated in the schema.
        /// </summary>
        partial void OnCreated();
        
        #endregion
        /// <summary>
        /// There are no comments for ModbusFactory constructor in the schema.
        /// </summary>
        public ModbusFactory()
        {
            this.DiMaps = new HashSet<DiMap>();
            OnCreated();
        }

    
        /// <summary>
        /// There are no comments for FactoryCode in the schema.
        /// </summary>
        public virtual int FactoryCode
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Name in the schema.
        /// </summary>
        public virtual string Name
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for MajorVersion in the schema.
        /// </summary>
        public virtual sbyte MajorVersion
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for MinorVersion in the schema.
        /// </summary>
        public virtual sbyte MinorVersion
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for Description in the schema.
        /// </summary>
        public virtual string Description
        {
            get;
            set;
        }

    
        /// <summary>
        /// There are no comments for DiMaps in the schema.
        /// </summary>
        public virtual ISet<DiMap> DiMaps
        {
            get;
            set;
        }
    }

}