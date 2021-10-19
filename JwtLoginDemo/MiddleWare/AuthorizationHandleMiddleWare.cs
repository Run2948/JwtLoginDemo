using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using JwtLoginDemo.Models;

namespace JwtLoginDemo.MiddleWare
{
    /// <summary>
    /// 这个是Asp.Net Core 5 新增的授权处理失败  可以直接暴露出请求上下文 省事很多啦！！！
    /// 作者 xxx
    /// </summary>
    public class AuthorizationHandleMiddleWare : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler authorizationHandleMiddleWare = new();
        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            // 当 token失效或者token不存在的时候 authorizeResult.Challenged 为True
            if (authorizeResult.Challenged)
            {
                // todo 拿到上下文user对象后 此处可以check token  区分token是否是过期了
                var a = context.Request.Headers["Authorization"];
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsJsonAsync(new ResponseModel(Enums.ResponseCode.UnAuthorized, "您未授权,请检查Token是否有效！"));
                return;
            }
            // 此时token 校验通过  但是访问的资源的没权限的话 authorizeResult.Forbidden 为true
            if (authorizeResult.Forbidden)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsJsonAsync(new ResponseModel(Enums.ResponseCode.ForBidden, "您无此权限访问哦！"));
                return;
            }
            await authorizationHandleMiddleWare.HandleAsync(next, context, policy, authorizeResult);
        }
    }

}