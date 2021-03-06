﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models.IdentityModel
{
    public class UserPolicyTypes
    {
        public const string AllUserPolicy = CommonTypes.IDENTITY_NAMESPACE_URI + "/policy/alluserpolicy";
        public const string RequiredManager = CommonTypes.IDENTITY_NAMESPACE_URI + "/policy/requiredmanager";
        public const string OnlySupervisor = CommonTypes.IDENTITY_NAMESPACE_URI + "/policy/onlysupervisor";
    }
}
