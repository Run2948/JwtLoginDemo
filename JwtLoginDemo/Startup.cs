using System;
using System.IO;
using System.Reflection;
using System.Text;
using JwtLoginDemo.Data;
using JwtLoginDemo.Data.Entities;
using JwtLoginDemo.MiddleWare;
using JwtLoginDemo.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace JwtLoginDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 将配置文件key 映射到实体类 key 这里JWT的key一定要复杂点 不能过短
            services.Configure<JWTConfig>(Configuration.GetSection("JWTConfig"));
            services.AddDbContext<AppDBContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"), MySqlServerVersion.LatestSupportedServerVersion);
            });
            // AddEntityFrameworkStores 用来创建 用户和密码之间的服务
            services.AddIdentity<AppUser, IdentityRole>(opt => { }).AddEntityFrameworkStores<AppDBContext>();
            // 采用官方授权
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    // jwt的 key 需要设置复杂点 
                    var key = Encoding.ASCII.GetBytes(Configuration["JWTConfig:Key"]);
                    var issure = Configuration["JWTConfig:Issuer"];   // 发行人 
                    var audience = Configuration["JWTConfig:Audience"];  // 受众   
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true, // 设置为True时 ValidIssure 属性设置下 不然jwt验证不会通过
                        ValidateAudience = true, // 同上 ValidAudience 属性设置下  
                        RequireExpirationTime = true,
                        ValidateLifetime = true,   //  token失效缓冲时间 默认是五分钟 失效时间需要加上这五分钟缓冲
                        //  如果 上面 ValidateIssuer ValidateAudience   配置为false 则不需要下面两个属性
                        ValidIssuer = issure,
                        ValidAudience = audience,

                    };
                });
            // 多角色时 可以这样配置  [Authorize(Policy ="PolicyGroup")] 动作方法上可以简写
            services.AddAuthorization(options =>
            {
                options.AddPolicy("PolicyGroup", policy => policy.RequireRole("Admin", "User"));
            });
            // 配置跨域
            services.AddCors(option =>
            {
                option.AddPolicy("any", builder =>
                {
                    builder.AllowAnyOrigin();
                    builder.AllowAnyHeader();
                    builder.AllowAnyHeader();
                });
            });
            // .net 5 新增的权限验证中间件  在此处依赖注入一下  详见 AuthorizationHandleMiddleWare.cs 文件
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationHandleMiddleWare>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "JwtLoginDemo", Version = "v1", Description = "Demo API for showing Swagger" });

                // 下面两步配置 实现 swagger 上面 “锁”
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,  // 位于Header
                    Description = "请于此处直接填写token 无需 Bearer然后再加空格的形式",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement{
                    {
                        new OpenApiSecurityScheme{
                            Reference=new OpenApiReference{
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },Array.Empty<string>()
                    }
                });

                // swagger接口注释显示 
                // 注意 vscode 用户需要在项目的csproj文件里面手动配置生成注释文档的属性  
                // 具体参见项目文件里的PropertyGroup
                var fileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
                c.IncludeXmlComments(filePath);
            });
        }
        ///
        /// // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // swagger 中间件使用
                app.UseSwagger();
                // 此处的 v1 必须与上面c.SwaggerDoc("v1") 里的一致
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "JwtLoginDemo v1"));
            }

            // app.UseHttpsRedirection();
            app.UseCors("any");
            app.UseRouting();
            // 注意顺序
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
