﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models.IdentityModel
{
    public class UserClaimTypes
    {
        public const string Issuer = "LOCAL AUTHORITY";
        public const string AggregatorGroupIdentifier = CommonTypes.IDENTITY_NAMESPACE_URI + "/claims/aggregatorgroupidentifier";
        public const string AggregatorGroupName = CommonTypes.IDENTITY_NAMESPACE_URI + "/claims/aggregatorgroupname";
        //public const string UPDATE_CUSTOMER_INFO_CLAIM = "https://www.peiu.co.kr/claims/2019/08/update/customer_info";
        //public const string DELETE_CUSTOMER_INFO_CLAIM = "https://www.peiu.co.kr/claims/2019/08/delete/customer_info";
    }

    public enum CrudClaims
    {
        NONE = 0,
        CREATE = 8,
        READ = 1,
        UPDATE = 2,
        DELETE = 4
    }
}
