using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PEIU.Models.Database
{
    [System.ComponentModel.DataAnnotations.Schema.Table("UserAccount")]
    public class UserAccount : IdentityUser
    {
        //data.Add("id", 3);
        //data.Add("username", "최고은");
        //data.Add("email", "ccc@ccc.com");
        //data.Add("created_at", DateTime.Now);
        //data.Add("updated_at", DateTime.Now);

        
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string CompanyName { get; set; }

        public string RegistrationNumber { get; set; }

        public string Address { get; set; }

        public DateTime RegistDate { get; set; }

        public DateTime Expire { get; set; }

        public bool SignInConfirm { get; set; }

        public virtual RegisterType UserType { get; set; } = RegisterType.Aggregator;

        [JsonIgnore]
        public ContractorUser Contractor { get; set; }

        [JsonIgnore]
        public AggregatorUser Aggregator { get; set; }

        [JsonIgnore]
        public SupervisorUser Supervisor { get; set; }

        //public override string UserName { get => base.UserName; set => base.UserName = value; }

        public UserAccount()
        {
            
        }
    }
}
