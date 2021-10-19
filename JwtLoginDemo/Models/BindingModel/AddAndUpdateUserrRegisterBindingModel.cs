using System.Collections.Generic;

namespace JwtLoginDemo.Models.BindingModel
{
    public class AddAndUpdateUserrRegisterModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        //注册的时候设置角色
        public List<string> Roles { get; set; }
    }
}