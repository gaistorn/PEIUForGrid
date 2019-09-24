using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models.Database
{
    public class UserClaim : IdentityUserClaim<string>
    {
        public UserClaim() { }
    }
}
