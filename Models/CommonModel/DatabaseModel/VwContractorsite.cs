﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using NHibernate template.
// Code is generated on: 2019-09-09 오전 10:39:15
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
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PEIU.Models.Database
{
    [Table("vw_contractorsites")]
    /// <summary>
    /// VIEW
    /// </summary>
    public partial class VwContractorsite : VwContractorsiteBase
    {
    
        #region Extensibility Method Definitions
        
        /// <summary>
        /// There are no comments for OnCreated in the schema.
        /// </summary>
        partial void OnCreated();
        
        #endregion
        /// <summary>
        /// There are no comments for VwContractorsite constructor in the schema.
        /// </summary>
        public VwContractorsite()
        {
            OnCreated();
        }

        [Key]
        /// <summary>
        /// There are no comments for SiteId in the schema.
        /// </summary>
        public override int SiteId
        {
            get;
            set;
        }
    }

    /// <summary>
    /// VIEW
    /// </summary>
    public class VwContractorsiteBase
    {

      
        /// <summary>
        /// There are no comments for VwContractorsite constructor in the schema.
        /// </summary>
        public VwContractorsiteBase()
        {
        }

        
        /// <summary>
        /// There are no comments for SiteId in the schema.
        /// </summary>
        public virtual int SiteId
        {
            get;
            set;
        }

        public virtual string AggGroupId { get; set; }
        public virtual string AggName { get; set; }
        public virtual string AggRepresentation { get; set; }

        public virtual int RCC { get; set; }
        public virtual string Email { get; set; }
        public virtual string Firstname { get; set; }
        public virtual string Lastname { get; set; }
        public virtual string Company { get; set; }


        /// <summary>
        /// There are no comments for Longtidue in the schema.
        /// </summary>
        public virtual double Longtidue
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for Latitude in the schema.
        /// </summary>
        public virtual double Latitude
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for LawFirstCode in the schema.
        /// </summary>
        public virtual string LawFirstCode
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for LawMiddleCode in the schema.
        /// </summary>
        public virtual string LawMiddleCode
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for LawLastCode in the schema.
        /// </summary>
        public virtual string LawLastCode
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for Address1 in the schema.
        /// </summary>
        public virtual string Address1
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for Address2 in the schema.
        /// </summary>
        public virtual string Address2
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for UserId in the schema.
        /// </summary>
        public virtual string UserId
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for ServiceCode in the schema.
        /// </summary>
        public virtual int ServiceCode
        {
            get;
            set;
        }

        public virtual string Represenation { get; set; }


        /// <summary>
        /// There are no comments for Comment in the schema.
        /// </summary>
        public virtual string Comment
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for RegisterTimestamp in the schema.
        /// </summary>
        public virtual System.DateTime? RegisterTimestamp
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for TotalPcsCapacity in the schema.
        /// </summary>
        public virtual double? TotalPcsCapacity
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for PcsCount in the schema.
        /// </summary>
        public virtual decimal? PcsCount
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for TotalBmsCapacity in the schema.
        /// </summary>
        public virtual double? TotalBmsCapacity
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for BmsCount in the schema.
        /// </summary>
        public virtual decimal? BmsCount
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for TotalPvCapacity in the schema.
        /// </summary>
        public virtual double? TotalPvCapacity
        {
            get;
            set;
        }


        /// <summary>
        /// There are no comments for PvCount in the schema.
        /// </summary>
        public virtual decimal? PvCount
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"Site Id: {SiteId} / {Address1}";
        }
    }

}
