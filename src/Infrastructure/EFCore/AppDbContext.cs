using System.Reflection;
using Core.Entities;
using Core.Entities.Auth;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EFCore;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<CoachingRequest> CoachingRequests { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<MediaAsset> MediaAssets { get; set; }
    public DbSet<TrainingCourse> TrainingCourses { get; set; }
    public DbSet<CertificateSubmission> CertificateSubmissions { get; set; }
    public DbSet<FileAsset> FileAssets { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<TrainingLog> TrainingLogs { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}