using System;
using System.Collections.Generic;

namespace JwtLoginDemo.Models.Dto
{
    public class UserDto
    {
        public UserDto(string fullName, string email, string userName, DateTime createdTime, List<string> role)
        {
            FullName = fullName;
            Email = email;
            UserName = userName;
            Created = createdTime;
            Roles = role;
        }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public DateTime Created { get; set; }
        public string Token { get; set; }

        public string RefreshToken { get; set; }
        public List<string> Roles { get; set; }


    }
}