﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PEIU.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PEIU.Service.WebApiService
{
    public interface IClaimServiceFactory
    {
        Task<UserAccount> FindUserAccount(ClaimsPrincipal claimsPrincipal);
        string GetClaimsValue(ClaimsPrincipal claimsPrincipal, string ClaimsType);
    }
    public class ClaimServiceFactory : IClaimServiceFactory, IDisposable
    {
        //readonly UserManager<UserAccount> _userManager;
        private readonly IServiceScope _scope;
        public ClaimServiceFactory(IServiceProvider services)
        {
            _scope = services.CreateScope(); // CreateScope is in Microsoft.Extensions.DependencyInjection
        }

        public void Dispose()
        {
            _scope?.Dispose();
        }

        public async Task<UserAccount> FindUserAccount(ClaimsPrincipal claimsPrincipal)
        {
            using (var _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<UserAccount>>())
            {
                var correct_user = await _userManager.FindByIdAsync(GetClaimsValue(claimsPrincipal, ClaimTypes.NameIdentifier));
                return correct_user;
            }
            
        }

        public string GetClaimsValue(ClaimsPrincipal claimsPrincipal, string ClaimsType)
        {
            return claimsPrincipal.FindFirstValue(ClaimsType);
        }
    }

}
