using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.UserModelViews
{
    public class UserInfoModel
    {
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
    }
    public class CreateUserInfo
    {

        public required string FullName { get; set; }
        public required string Bio { get; set; }
        public required string Gender { get; set; }
        public required string Address { get; set; }
        public required DateTime DateOfBirth { get; set; }
        public required string PhoneNumber { get; set; }
    }

}
