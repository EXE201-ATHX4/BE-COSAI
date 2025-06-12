

using System.ComponentModel.DataAnnotations;

namespace ModelViews.AuthModelViews
{
    public class CreateAccountModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
