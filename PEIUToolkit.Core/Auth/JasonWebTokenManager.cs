﻿using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PES.Toolkit.Auth
{
    public class JasonWebTokenManager
    {
        public static readonly string Secret;

        static JasonWebTokenManager()
        {
            HMACSHA256 hmac = new HMACSHA256();
            string secret = Convert.ToBase64String(hmac.Key);
            Secret = secret;
        }

        public static ClaimsPrincipal GetPrincipal(string userid, string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
                if (jwtToken == null)
                    return null;
                byte[] key = Convert.FromBase64String(Secret);
                TokenValidationParameters parameters = new TokenValidationParameters()
                {
                    NameClaimType = userid,
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                SecurityToken securityToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out securityToken);
                return principal;
            }
            catch/* (Exception ex)*/
            {
                return null;
            }
        }

        public static string ValidateToken(string userid, string token, string ClaimType)
        {
            string username = null;
            ClaimsPrincipal principal = GetPrincipal(userid, token);

            if (principal == null)
                return null;
            ClaimsIdentity identity = null;
            try
            {
                identity = (ClaimsIdentity)principal.Identity;
            }
            catch /*(Exception ex)*/
            {
                return null;
            }
            Claim usernameClaim = identity.FindFirst(ClaimType);
            username = usernameClaim.Value;
            return username;
        }

        public static ClaimsIdentity ValidateToken(string userid, string token)
        {
            ClaimsPrincipal principal = GetPrincipal(userid, token);

            if (principal == null)
                return null;
            ClaimsIdentity identity = null;
            try
            {
                identity = (ClaimsIdentity)principal.Identity;
            }
            catch /*(Exception ex)*/
            {
                return null;
            }
            return identity;
        }

        public static string GenerateToken(string userid, string issuer, IList<Claim> claims)
        {
        //    var claims = new[]
        //{
        //    new Claim(ClaimTypes.Name, username),
        //    new Claim(ClaimTypes.sc)
        //};

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                
                issuer: issuer,
                audience: "https://www.peiu.co.kr",
                claims: claims,
                expires: DateTime.Now.AddYears(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
        // public static string Secret => 
        // {
        //     HMACSHA256 hmac = new HMACSHA256();

        // }
    }
}
