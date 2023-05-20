using System.ComponentModel.DataAnnotations.Schema;
using Core.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Core.Entities;

public class AppUser : IdentityUser
{
    public bool IsLocked { get; set; }
    public string? MailToken { get; set; }
    public DateTime? MailTokenExpiry { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public IReadOnlyCollection<RefreshToken> RefreshTokens { get; set; }
    [Column(TypeName = "nvarchar(20)")]
    public Role Role { get; set; }
    public string Fullname { get; set; }
    //Coach, Client
    public DateTime? Dob  { get; set; }
    //Coach, Client
    [Column(TypeName = "nvarchar(10)")]
    public Gender? Gender   { get; set; }
    //Coach
    public bool? IsVerified { get; set; }
    //Coach
    public string? AboutMe { get; set; }
    public int WarningCount  { get; set; }
    public string? Note { get; set; }
    public ICollection<Report> Reports { get; set; }
    public CertificateSubmission CertificateSubmission { get; set; }
    //Coach
    public ICollection<TrainingCourse> TrainingCourses { get; set; }
    //Coach
    public ICollection<CoachingRequest> CoachRequests { get; set; }
    //Client
    public ICollection<CoachingRequest> ClientRequests { get; set; }
    //All
    public ICollection<MediaAsset> MediasAssets { get; set; }
    //Coach
    public ICollection<Contract> CoachContracts { get; set; }
    //Client
    public ICollection<Contract> ClientContracts { get; set; }
    public ICollection<Voucher> Vouchers { get; set; }
}