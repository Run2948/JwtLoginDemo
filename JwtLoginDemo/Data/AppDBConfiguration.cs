using JwtLoginDemo.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace JwtLoginDemo.Data
{
    /// <summary>
    /// Migrations and Seed Data with Entity Framework Core: https://code-maze.com/migrations-and-seed-data-efcore/
    /// </summary>
    public class IdentityRoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            //builder.ToTable("AspNetRoles");
            //builder.HasKey(s => s.Id);

            //builder.Property(s => s.Name)
            //    .IsRequired();
            //builder.Property(s => s.NormalizedName)
            //    .IsRequired();
            //builder.Property(s => s.ConcurrencyStamp)
            //    .IsRequired();

            //builder.HasData(
            //    new IdentityRole
            //    {
            //        Name = "Admin"
            //    },
            //    new IdentityRole
            //    {
            //        Name = "User"
            //    }
            //    );
        }
    }

    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            //builder.ToTable("AspUsers");
            //builder.HasKey(s => s.Id);

            //builder.Property(s => s.Name)
            //    .IsRequired();
            //builder.Property(s => s.NormalizedName)
            //    .IsRequired();
            //builder.Property(s => s.ConcurrencyStamp)
            //    .IsRequired();

            //builder.HasData(
            //    new AppUser
            //    {
            //        UserName = "john.zhujr@outlook.com",
            //        FullName = "John Zhu",
            //        Email = "john.zhujr@outlook.com",
            //        PasswordHash = "AQAAAAEAACcQAAAAENclRM5kcUP4VkiT9HQv6PAGYNMAj+aSc99ZnN/1tK9sJcdyFY8BY3xDtUtyT6Pkog==", // 1qaz@WSX
            //        DateCreated = DateTime.Now,
            //        DateModified = DateTime.UtcNow
            //    }
            //    );
        }
    }
}
