using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtLoginDemo.Models.Dto;
using JwtLoginDemo.Models;
using JwtLoginDemo.Models.BindingModel;
using JwtLoginDemo.Data.Entities;

namespace JwtLoginDemo.Controllers
{
    /// <summary>
    /// 用户控制器 
    /// </summary>
    // [Authorize(Roles ="Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        private readonly UserManager<AppUser> _userManger;  // 用户服务
        private readonly SignInManager<AppUser> _signInManger;  // 登录服务

        private readonly RoleManager<IdentityRole> _roleManger; // 角色服务
        private readonly JWTConfig _jwtConfig;  // 配置框架将配置文件注入实体类

        public UserController(ILogger<UserController> logger, UserManager<AppUser> userManager,
                SignInManager<AppUser> signInManager, IOptions<JWTConfig> jwtConfig, RoleManager<IdentityRole> roleManger)
        {
            _logger = logger;
            _userManger = userManager;
            _signInManger = signInManager;
            _jwtConfig = jwtConfig.Value;
            _roleManger = roleManger;
        }


        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("RegisterUser")]
        public async Task<object> RegisterUser(AddOrUpdateUserModel model)
        {
            try
            {
                // check  注册的时候是否包含角色
                if (model.Roles is null || model.Roles.Count <= 0)
                {
                    return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Error, "角色不能为空"));
                }
                // 循环判断用户所注册的角色时候存在 创建角色的方法  AddRole()
                foreach (var item in model.Roles)
                {
                    if (!await _roleManger.RoleExistsAsync(item))
                    {
                        return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Error, "不存在创建的角色"));
                    }
                }
                // 生成一个用户类
                var user = new AppUser()
                {
                    UserName = model.Email,
                    FullName = model.FullName,
                    Email = model.Email,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.UtcNow
                };
                // 注册用户 
                var result = await _userManger.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // 注册成功后 获取临时刚刚创建的用户  
                    var tempUser = await _userManger.FindByEmailAsync(model.Email);
                    // 循环给创建的用户添加角色
                    foreach (var role in model.Roles)
                    {
                        await _userManger.AddToRoleAsync(tempUser, role); // 添加角色
                    }
                    return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Ok, "用户被成功注册！", null));
                }
                // 创建用户失败返回
                return await Task.FromResult(string.Join(",", result.Errors.Select(x => x.Description).ToArray()));
            }
            catch (Exception ex)
            {
                // 异常返回
                return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Error, ex.Message, null));
            }
        }


        /// <summary>
        /// 获取所有的用户
        /// </summary>
        /// <returns></returns>
        //[Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllUsers")]
        public async Task<object> GetAllUsers()
        {
            try
            {
                List<UserDto> userDtos = new();
                foreach (var item in _userManger.Users.ToList())
                {
                    var role = (await _userManger.GetRolesAsync(item)).ToList();
                    userDtos.Add(new UserDto(item.FullName, item.Email, item.UserName, item.DateCreated, role));
                };
                //var user =_userManger.Users.Select(x=>new UserDto(x.FullName,x.Email,x.UserName,x.DateCreated));
                return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Ok, "", userDtos));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Error, ex.Message, null));
            }
        }


        /// <summary>
        /// 获取包含用户角色用户
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles ="Admin,User")]
        [Authorize(Policy = "PolicyGroup")]
        [HttpGet("GetUsersContainUserRole")]
        public async Task<object> GetUserList()
        {
            try
            {
                List<UserDto> userDtos = new();
                var users = _userManger.Users.ToList();
                foreach (var item in users)
                {
                    // 获取对应用户的角色
                    var userRoles = (await _userManger.GetRolesAsync(item)).ToList();
                    if (userRoles.Any(x => x == "User"))
                    {
                        userDtos.Add(new UserDto(item.FullName, item.Email, item.UserName, item.DateCreated, userRoles));
                    }
                }
                return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Ok, "User角色用户获取成功！", userDtos));

            }
            catch (Exception ex)
            {
                return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Error, ex.Message));
            }
        }


        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<object> Login(LoginModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Error, "参数不合法!", null));
                }
                var result = await _signInManger.PasswordSignInAsync(model.UserName, model.Password, false, false);
                if (result.Succeeded)
                {
                    // 成功的话获取用户
                    var appUser = await _userManger.FindByNameAsync(model.UserName);
                    var roles = (await _userManger.GetRolesAsync(appUser)).ToList();
                    // await _userManger.GetRolesAsync(appUser);
                    var user = new UserDto(appUser.FullName, appUser.Email, appUser.UserName, appUser.DateCreated, roles)
                    {
                        // 生成Token 
                        Token = GenarateToken(appUser, roles)
                    };
                    return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Ok, "登录成功", user));
                }
                else
                {
                    return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Error, "登录失败", null));
                }

            }
            catch (Exception ex)
            {
                return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Error, ex.Message, null));
            }
        }


        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("GetRoles")]
        public async Task<object> GetRoles()
        {
            try
            {
                var roleList = _roleManger.Roles.Select(s => s.Name).ToList();
                return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Ok, "获取角色成功", roleList));
            }
            catch (Exception)
            {

                throw;
            }
        }


        /// <summary>
        /// 添加角色
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")] // 只有Admin角色的用户可以访问
        [HttpPost("AddRole")]
        public async Task<object> AddRole(AddRoleModel model)
        {
            try
            {
                if (model is null || string.IsNullOrWhiteSpace(model.Role))
                {
                    return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Error, "角色不能为空"));
                }
                // 判断【AspNetRoles】 表里  角色是否存在  
                if (await _roleManger.RoleExistsAsync(model.Role))
                {
                    return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Ok, "角色存在了"));
                }
                var role = new IdentityRole()
                {
                    Name = model.Role,
                };
                // 创建角色
                var result = await _roleManger.CreateAsync(role);
                if (result.Succeeded)
                {
                    return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Ok, "角色创建成功!"));
                }
                else
                {
                    return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Error, "角色创建失败!"));
                }

            }
            catch (Exception)
            {
                return await Task.FromResult(new ResponseModel(Enums.ResponseCode.Error));
            }

        }


        /// <summary>
        /// 生成Token
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        private string GenarateToken(AppUser user, List<string> roles)
        {
            var jwtTokenHandle = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Key);

            // 配置 Subject
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.NameId,user.Id),
                new Claim(JwtRegisteredClaimNames.Email,user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // 多重角色
                Subject = new ClaimsIdentity(claims),

                // 单一角色
                // Subject = new ClaimsIdentity(new[]
                // {
                //     new System.Security.Claims.Claim(JwtRegisteredClaimNames.NameId,user.Id),
                //     new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email,user.Email),
                //     new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                //     // new System.Security.Claims.Claim(ClaimTypes.Role,"role")
                // }),

                // 过期时间 6s 为了验证Token失效我这边设置了比较短   最终过期时间为 五分钟6s 因为配置的时候有个五分钟缓冲期
                Expires = DateTime.UtcNow.AddSeconds(6),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = _jwtConfig.Audience,  // 这里不配置 也会返回UnAuthorized 
                Issuer = _jwtConfig.Issuer // 同上
            };
            // 创建token
            var token = jwtTokenHandle.CreateToken(tokenDescriptor);
            return jwtTokenHandle.WriteToken(token);
        }
    }
}