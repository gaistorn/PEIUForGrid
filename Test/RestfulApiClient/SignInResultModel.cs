using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEIU.UI.Model
{
    public class SignInResultModel
    {
        [JsonProperty("result")]
        public SignInModel Result { get; set; }

        [JsonProperty("token")]
        public string JWToken { get; set; }
    }

    public class SignInModel
    {
        //
        // 요약:
        //     Returns a flag indication whether the sign-in was successful.
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
        //     factor authentication
        [JsonProperty("requiresTwoFactor")]

        public bool RequiresTwoFactor { get; protected set; }
    }
}
