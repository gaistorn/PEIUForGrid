using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PEIU.Models.ExchangeModel
{
    public abstract class RegisterViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [Required]
        [EmailAddress]
        [Display(Name = "email")]
        public string Email { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        [Display(Name = "firstname")]
        public string FirstName { get; set; }

        //[Required]
        //[StringLength(10, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
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
        public string Company { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "phonenumber")]
        public string PhoneNumber { get; set; }

        [Display(Name = "address")]
        public string Address { get; set; }

        [Display(Name = "type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public abstract RegisterType Type { get; }

        public DateTime RegistDate { get; set; }
        public DateTime Expire { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ContractStatusCodes ContractStatus { get; set; }

    }

    public class AggregatorGroupRegistModel
    {
        [Required]
        [Display(Name = "name")]
        public string AggregatorGroupName { get; set; }


        /// <summary>
        /// 상호 또는 대표 이름
        /// </summary>
        [Required]
        [Display(Name = "representation")]
        
        public string Represenation { get; set; }

        [Display(Name = "address")]
        public string Address { get; set; }

        [Display(Name = "phonenumber")]
        public string PhoneNumber { get; set; }



    }

    public class SupervisorRegistModel : RegisterViewModel
    {
        public override RegisterType Type => RegisterType.Supervisor;
    }

    public abstract class AggregatorRegistModelBase : RegisterViewModel
    {
        [Required(ErrorMessage = "계약자의 상위 Aggregator의 ID를 입력해야 합니다")]
        [Display(Name = "aggregatorid")]
        public string AggregatorId { get; set; }
    }

    public class AggregatorRegistModel : RegisterViewModel
    {
        public override RegisterType Type => RegisterType.Aggregator;

        
    }

    public class ContractorRegistModel : AggregatorRegistModelBase
    {
        public override RegisterType Type => RegisterType.Contrator;

        //[JsonIgnore]
        public bool NotifyEmail { get; set; }
    }
}
