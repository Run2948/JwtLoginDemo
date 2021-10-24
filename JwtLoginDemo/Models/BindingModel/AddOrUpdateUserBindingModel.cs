using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JwtLoginDemo.Models.BindingModel
{
    public class AddOrUpdateUserModel
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        //注册的时候设置角色
        [Required]
        public List<string> Roles { get; set; }
    }
}