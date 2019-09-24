using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PEIU.Models.Database
{
    [Table("SupervisorUsers")]
    public class SupervisorUser
    {
        [Key]
        public string UserId { get; set; }

        public virtual UserAccount User { get; set; }

        public SupervisorUser() { }
    }
}
