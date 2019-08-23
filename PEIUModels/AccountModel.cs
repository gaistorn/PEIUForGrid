#if !WPF
using Microsoft.AspNetCore.Identity;
#endif
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PEIU.Models
{
    public enum AuthRoles : int
    {
        Operator = 8,
        Aggregator = 16,
        Business = 32,
        Candidator = 64,
    }

#if !WPF
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class AccountModel : IdentityUser
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

        [Display(Name = "authlevel")]
        public ushort AuthRoles { get; set; } = 0;

        [Display(Name = "address")]
        public string Address { get; set; }

        [Display(Name = "status")]
        public int Status { get; set; }

        //public override string UserName { get => base.UserName; set => base.UserName = value; }

        public AccountModel()
        {
            
        }
    }

    
    public class ResetPasswordModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "password")]
        public string NewPassword { get; set; }

        [Required]
        //[DataType(DataType.to)]
        [Display(Name = "token")]
        public string Token { get; set; }
    }

#endif
    public class RegisterViewModel
    {
#if WPF
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string CompanyName { get; set; }

        public string PhoneNumber { get; set; }


        public string Address { get; set; }

        public ushort AuthRoles { get; set; }

        public ObservableCollection<IAssetLocation> Assets { get; } = new ObservableCollection<IAssetLocation>();
#else
        [Required]
        [EmailAddress]
        [Display(Name = "email")]
        public string Email { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        [Display(Name = "firstname")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        [Display(Name = "lastname")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "confirmpassword")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "company")]
        public string CompanyName { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "phonenumber")]
        public string PhoneNumber { get; set; }

        [Display(Name ="registrationnumber")]
        public string RegistrationNumber { get; set; }

        [Display(Name = "address")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "authroles")]
        public ushort AuthRoles { get; set; }



#endif
    }

}
