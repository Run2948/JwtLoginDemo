using System.ComponentModel.DataAnnotations;

namespace JwtLoginDemo.Models.BindingModel
{
    public class LoginModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}