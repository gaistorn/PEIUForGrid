using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PEIU.Models.Database
{
    public class UserModel : IdentityUser
    {
        //data.Add("id", 3);
        //data.Add("username", "최고은");
        //data.Add("email", "ccc@ccc.com");
        //data.Add("created_at", DateTime.Now);
        //data.Add("updated_at", DateTime.Now);

        [Required]
        [StringLength(10, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        [Display(Name = "firstname")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        [Display(Name = "lastname")]
        public string LastName { get; set; }

        [Display(Name = "company")]
        public string CompanyName { get; set; }

        [Display(Name = "registrationnumber")]
        public string RegistrationNumber { get; set; }

        [Display(Name = "address")]
        public string Address { get; set; }

        [Display(Name = "status")]
        public int Status { get; set; }

        [Display(Name = "registdate")]
        public DateTime RegistDate { get; set; }

        //public override string UserName { get => base.UserName; set => base.UserName = value; }

        public UserModel()
        {
            
        }
    }
}
