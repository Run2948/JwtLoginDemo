using System.ComponentModel.DataAnnotations;

namespace JwtLoginDemo.Models.BindingModel
{
    public class AddRoleModel
    {
        [Required]
        public string Role { get; set; }
    }
}