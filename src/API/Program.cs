using API.Extensions;
using API.Helpers;
using API.Middleware;
using Core.Entities;
using Hangfire;
using Hangfire.Storage.SQLite;
using Infrastructure.EFCore;
using Infrastructure.EFCore.Data;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region Add services

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("iCoachingDevelopmentDb"), 
        new MySqlServerVersion(new Version(8,0,31)), 
        b => b.MigrationsAssembly("Infrastructure"));
});

builder.Services.AddHangfire(options =>
    options.UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSQLiteStorage(builder.Configuration.GetConnectionString("Hangfire")));

builder.Services.AddHangfireServer();
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddApplicationServices();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue;
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
});
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});
#endregion

#region Use services
var app = builder.Build();

app.UseSwaggerDocumentation();

app.UseCors(options =>
{
    options.AllowAnyHeader();
    options.AllowAnyMethod();
    options.AllowAnyOrigin();
});

app.UseHangfireDashboard();
app.MapHangfireDashboard();
app.UseMiddleware(typeof(ExceptionMiddleware));
app.UseStatusCodePagesWithReExecute("/errors/{0}");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id?}");
#endregion

#region Auto update db & seed data

if (app.Environment.IsProduction())
{
    //Wait 10s for db running in docker compose
    Thread.Sleep(10000);
}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
        
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        
        //Add sample data
        await DataSeed.SeedAsync(userManager, loggerFactory, context);
    }
    catch (Exception e)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(e, "An error occured during migration");
    }
}
#endregion

app.Run();