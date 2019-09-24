using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models.Database
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Roles")]
    public class Role : IdentityRole<string>
    {
       public string Category { get; set; }
        public Role()
        {
            
        }
    }
}
