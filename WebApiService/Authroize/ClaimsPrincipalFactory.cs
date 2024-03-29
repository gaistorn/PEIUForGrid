﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PEIU.Models;
using PEIU.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PEIU.Service.WebApiService.Authroize
{
    public class ClaimsPrincipalFactory : UserClaimsPrincipalFactory<UserAccount, Role>
    {
        readonly ILogger<AuthorizationPolicyProvider> _logger;
        public ClaimsPrincipalFactory(
            UserManager<UserAccount> userManager,
            RoleManager<Role> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<AuthorizationPolicyProvider> logger
            )
            : base(userManager, roleManager, optionsAccessor)
        {
            _logger = logger;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(UserAccount user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            try
            {
                identity.AddClaim(new Claim("CompanyName", user.CompanyName ?? ""));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return identity;
        }
    }
}
