using System;
using Microsoft.AspNetCore.Identity;

namespace JwtLoginDemo.Data.Entities
{
    /// <summary>
    /// 登录用户实体类  继承Identiy框架提供的 IdentityUser类
    /// </summary>
    public class AppUser : IdentityUser
    {
        // 自己再扩充三个字段
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

        public string FullName { get; set; }
    }
}