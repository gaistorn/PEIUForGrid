using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEIU.Models
{
    public class SignInResultModel
    {
        [JsonProperty("result")]
        public SignInResult Result { get; set; }

        [JsonProperty("token")]
        public string JWToken { get; set; }
    }

    //
    // 요약:
    //     Represents the result of a sign-in operation.
    public class SignInResult
    {
        public SignInResult() { }

        
        [JsonProperty("succeeded")]
        public bool Succeeded { get; protected set; }
        //
        // 요약:
        //     Returns a flag indication whether the user attempting to sign-in is locked out.
        [JsonProperty("isLockedOut")]
        public bool IsLockedOut { get; protected set; }
        //
        // 요약:
        //     Returns a flag indication whether the user attempting to sign-in is not allowed
        //     to sign-in.
        [JsonProperty("isNotAllowed")]
        public bool IsNotAllowed { get; protected set; }
        //
        // 요약:
        //     Returns a flag indication whether the user attempting to sign-in requires two
        //     factor authentication.
        [JsonProperty("requiresTwoFactor")]
        public bool RequiresTwoFactor { get; protected set; }

        //
        // 요약:
        //     Converts the value of the current Microsoft.AspNetCore.Identity.SignInResult
        //     object to its equivalent string representation.
        //
        // 반환 값:
        //     A string representation of value of the current Microsoft.AspNetCore.Identity.SignInResult
        //     object.
    }
}
