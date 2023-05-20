using System.Text;
using Core.Entities;
using Core.Entities.Auth;
using Infrastructure.EFCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions;

public static class IdentityServiceExtension
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<DataProtectionTokenProviderOptions>(o =>
        {
            o.TokenLifespan = TimeSpan.FromHours(1);
        });
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:AccessTokenSecret"])),
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services
            .AddIdentityCore<AppUser>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddUserManager<UserManager<AppUser>>()
            .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider);
        return services;
    }
}