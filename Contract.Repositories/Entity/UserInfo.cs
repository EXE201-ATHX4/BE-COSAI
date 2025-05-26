using Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repositories.Entity
{
    public class UserInfo : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; } // Navigation property to User entity

    }
}
