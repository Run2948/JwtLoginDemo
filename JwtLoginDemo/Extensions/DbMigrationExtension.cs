using JwtLoginDemo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace JwtLoginDemo.Extensions
{
    public static class DbMigrationExtension
    {
        public static IHost MigrateDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                using var context = scope.ServiceProvider.GetRequiredService<AppDBContext>();
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    //Log errors or do anything you think it's needed
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
            return host;
        }
    }
}
