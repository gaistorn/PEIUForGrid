using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Power21.PEIUEcosystem.Models
{
    public class ReservedSiteRegisterModel
    {
        [Required]
        [Display(Name = "address1")]
        public virtual string Address1 { get; set; }

        [Display(Name = "address2")]
        public virtual string Address2 { get; set; }

        [Required]
        [Display(Name = "accountid")]
        public virtual string AccountId { get; set; }

        [Required]
        [Display(Name = "controlowner")]
        public virtual bool ControlOwner { get; set; }

        [Required]
        [Display(Name = "siteinformation")]
        public virtual string SiteInformation { get; set; }
    }
}
