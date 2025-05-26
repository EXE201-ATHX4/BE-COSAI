using System.ComponentModel.DataAnnotations;

namespace ModelViews.AuthModelViews
{
    public class AddRoleModel
    {
        [Required]
        public int UserId { get; set; } 

        [Required]
        public string RoleName { get; set; }
    }
}
