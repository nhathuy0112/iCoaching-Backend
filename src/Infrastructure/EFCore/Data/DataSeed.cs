using System.Text.Json;
using Core.Entities;
using Core.Entities.Auth;
using Core.Entities.Status;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.EFCore.Data;

public class DataSeed
{
    private const string Password = "Abc12345!";
    private static UserManager<AppUser> _userManager = null!;
    private static AppDbContext _context = null!;
    
    public static async Task SeedAsync(UserManager<AppUser> userManager, ILoggerFactory loggerFactory,
        AppDbContext context)
    {
        _userManager = userManager;
        _context = context;

        try
        {
            if (!context.Users.Any())
            {
                await AddSuperAdminAsync();
                await AddSampleCoachesAsync();
                await AddSampleClientAsync();
                await AddAllUsersDefaultAvatarAsync();
                await AddSampleTrainingCourseAsync();
                await AddSampleContractAsync();
                await AddSampleCertAsync();
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception e)
        {
            var logger = loggerFactory.CreateLogger<DataSeed>();
            logger.LogError(e.Message);
        }
    }
    
    private static async Task AddSuperAdminAsync()
    {
            var superAdmin = new AppUser()
            {
                Fullname = "iCoaching super admin",
                UserName = "admin",
                Email = "capstoneforall@gmail.com",
                EmailConfirmed = true,
                Role = Role.SUPER_ADMIN,
            };
            
            var admin1 = new AppUser()
            {
                Fullname = "iCoaching admin 1",
                UserName = "admin123",
                Email = "",
                EmailConfirmed = true,
                Role = Role.ADMIN,
            };

            await _userManager.CreateAsync(admin1, Password);
            await _userManager.CreateAsync(superAdmin, Password);
    }
    
    private static async Task AddSampleCoachesAsync()
    {
            const string verifiedCoachesData = "[{\"UserName\":\"coach1\",\"Fullname\":\"Malone\",\"Email\":\"hlv1@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach2\",\"Fullname\":\"Christian\",\"Email\":\"hlv2@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach3\",\"Fullname\":\"Burgess\",\"Email\":\"hlv3@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach4\",\"Fullname\":\"Laurie\",\"Email\":\"hlv4@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach5\",\"Fullname\":\"Helga\",\"Email\":\"hlv5@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach6\",\"Fullname\":\"Carpenter\",\"Email\":\"hlv6@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true}, {\"UserName\":\"coach7\",\"Fullname\":\"Lauri\",\"Email\":\"hlv7@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach8\",\"Fullname\":\"Galloway\",\"Email\":\"hlv8@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach9\",\"Fullname\":\"Eve\",\"Email\":\"hlv9@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach10\",\"Fullname\":\"Megan\",\"Email\":\"hlv10@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach11\",\"Fullname\":\"Justine\",\"Email\":\"hlv11@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach12\",\"Fullname\":\"Herman\",\"Email\":\"hlv12@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach13\",\"Fullname\":\"Hebert\",\"Email\":\"hlv13@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach14\",\"Fullname\":\"Wall\",\"Email\":\"hlv14@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach15\",\"Fullname\":\"Dorothea\",\"Email\":\"hlv15@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach16\",\"Fullname\":\"Marva\",\"Email\":\"hlv16@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach17\",\"Fullname\":\"Marina\",\"Email\":\"hlv17@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach18\",\"Fullname\":\"Jewell\",\"Email\":\"hlv18@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach19\",\"Fullname\":\"Haley\",\"Email\":\"hlv19@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach20\",\"Fullname\":\"Brandi\",\"Email\":\"hlv20@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach21\",\"Fullname\":\"West\",\"Email\":\"hlv21@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach22\",\"Fullname\":\"Shirley\",\"Email\":\"hlv22@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach23\",\"Fullname\":\"Elsa\",\"Email\":\"hlv23@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach24\",\"Fullname\":\"Francis\",\"Email\":\"hlv24@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach25\",\"Fullname\":\"Richard\",\"Email\":\"hlv25@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach26\",\"Fullname\":\"Joseph\",\"Email\":\"hlv26@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach27\",\"Fullname\":\"Della\",\"Email\":\"hlv27@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach28\",\"Fullname\":\"Evangelina\",\"Email\":\"hlv28@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach29\",\"Fullname\":\"Hurst\",\"Email\":\"hlv29@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach30\",\"Fullname\":\"Kidd\",\"Email\":\"hlv30@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach31\",\"Fullname\":\"Kelly\",\"Email\":\"hlv31@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach32\",\"Fullname\":\"Alma\",\"Email\":\"hlv32@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach33\",\"Fullname\":\"Monique\",\"Email\":\"hlv33@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach34\",\"Fullname\":\"Mcgowan\",\"Email\":\"hlv34@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true},{\"UserName\":\"coach35\",\"Fullname\":\"Sofia\",\"Email\":\"hlv35@gmail.com\",\"IsVerified\":true,\"EmailConfirmed\":true}]";
            const string unverifiedCoachesData = "[{\"UserName\":\"coach36\",\"Fullname\":\"Avery\",\"Email\":\"hlv36@gmail.com\",\"IsVerified\":false,\"EmailConfirmed\":true},{\"UserName\":\"coach37\",\"Fullname\":\"Isabella\",\"Email\":\"hlv37@gmail.com\",\"IsVerified\":false,\"EmailConfirmed\":true},{\"UserName\":\"coach38\",\"Fullname\":\"Kinney\",\"Email\":\"hlv38@gmail.com\",\"IsVerified\":false,\"EmailConfirmed\":true},{\"UserName\":\"coach39\",\"Fullname\":\"Jennifer\",\"Email\":\"hlv39@gmail.com\",\"IsVerified\":false,\"EmailConfirmed\":true},{\"UserName\":\"coach40\",\"Fullname\":\"Hilda\",\"Email\":\"hlv40@gmail.com\",\"IsVerified\":false,\"EmailConfirmed\":true},{\"UserName\":\"coach41\",\"Fullname\":\"Chase\",\"Email\":\"hlv41@gmail.com\",\"IsVerified\":false,\"EmailConfirmed\":true},{\"UserName\":\"coach42\",\"Fullname\":\"Geraldine\",\"Email\":\"hlv42@gmail.com\",\"IsVerified\":false,\"EmailConfirmed\":true},{\"UserName\":\"coach43\",\"Fullname\":\"Buckner\",\"Email\":\"hlv43@gmail.com\",\"IsVerified\":false,\"EmailConfirmed\":true},{\"UserName\":\"coach44\",\"Fullname\":\"Sofia\",\"Email\":\"hlv44@gmail.com\",\"IsVerified\":false,\"EmailConfirmed\":true},{\"UserName\":\"coach45\",\"Fullname\":\"Roxie\",\"Email\":\"hlv45@gmail.com\",\"IsVerified\":false,\"EmailConfirmed\":true},{\"UserName\":\"coach46\",\"Fullname\":\"Farmer\",\"Email\":\"hlv46@gmail.com\",\"IsVerified\":false,\"EmailConfirmed\":true}]";
            var rd = new Random();
            var coaches = JsonSerializer.Deserialize<List<AppUser>>(verifiedCoachesData);
            var unverifiedCoaches = JsonSerializer.Deserialize<List<AppUser>>(unverifiedCoachesData);
            foreach (var c in coaches!.Concat(unverifiedCoaches!))
            {
                var num = rd.Next(1, 100);
                c.Gender = Gender.Female;
                c.PhoneNumber = "";
                if (num % 2 == 0) c.Gender = Gender.Male;
                if (num % 3 == 0) c.Gender = Gender.Other;
                c.Role = Role.COACH;
                c.Dob = new DateTime(rd.Next(1991, 2001), 1, 1);
                await _userManager.CreateAsync(c, Password);
            }
            
    }

    private static async Task AddSampleClientAsync()
    { 
        const string clientsData = 
            "[{\"UserName\":\"client1\",\"Fullname\":\"Madeleine\",\"Email\":\"client1@gmail.com\",\"PhoneNumber\":\"862584221\",\"EmailConfirmed\":true},{\"UserName\":\"client2\",\"Fullname\":\"Leanne\",\"Email\":\"client2@gmail.com\",\"PhoneNumber\":\"969470239\",\"EmailConfirmed\":true},{\"UserName\":\"client3\",\"Fullname\":\"Sharp\",\"Email\":\"client3@gmail.com\",\"PhoneNumber\":\"923491378\",\"EmailConfirmed\":true},{\"UserName\":\"client4\",\"Fullname\":\"Shari\",\"Email\":\"client4@gmail.com\",\"PhoneNumber\":\"897455275\",\"EmailConfirmed\":true},{\"UserName\":\"client5\",\"Fullname\":\"Luisa\",\"Email\":\"client5@gmail.com\",\"PhoneNumber\":\"839425354\",\"EmailConfirmed\":true},{\"UserName\":\"client6\",\"Fullname\":\"Lula\",\"Email\":\"client6@gmail.com\",\"PhoneNumber\":\"856555245\",\"EmailConfirmed\":true},{\"UserName\":\"client7\",\"Fullname\":\"Samantha\",\"Email\":\"client7@gmail.com\",\"PhoneNumber\":\"816427391\",\"EmailConfirmed\":true},{\"UserName\":\"client8\",\"Fullname\":\"Tammi\",\"Email\":\"client8@gmail.com\",\"PhoneNumber\":\"926512350\",\"EmailConfirmed\":true},{\"UserName\":\"client9\",\"Fullname\":\"Angelica\",\"Email\":\"client9@gmail.com\",\"PhoneNumber\":\"898536223\",\"EmailConfirmed\":true},{\"UserName\":\"client10\",\"Fullname\":\"Byers\",\"Email\":\"client10@gmail.com\",\"PhoneNumber\":\"839600366\",\"EmailConfirmed\":true},{\"UserName\":\"client11\",\"Fullname\":\"Sanders\",\"Email\":\"client11@gmail.com\",\"PhoneNumber\":\"848495366\",\"EmailConfirmed\":true},{\"UserName\":\"client12\",\"Fullname\":\"Shields\",\"Email\":\"client12@gmail.com\",\"PhoneNumber\":\"965530394\",\"EmailConfirmed\":true},{\"UserName\":\"client13\",\"Fullname\":\"Mcgowan\",\"Email\":\"client13@gmail.com\",\"PhoneNumber\":\"873404397\",\"EmailConfirmed\":true},{\"UserName\":\"client14\",\"Fullname\":\"Allie\",\"Email\":\"client14@gmail.com\",\"PhoneNumber\":\"851559385\",\"EmailConfirmed\":true},{\"UserName\":\"client15\",\"Fullname\":\"Franklin\",\"Email\":\"client15@gmail.com\",\"PhoneNumber\":\"965441339\",\"EmailConfirmed\":true},{\"UserName\":\"client16\",\"Fullname\":\"Jody\",\"Email\":\"client16@gmail.com\",\"PhoneNumber\":\"923567265\",\"EmailConfirmed\":true},{\"UserName\":\"client17\",\"Fullname\":\"Roth\",\"Email\":\"client17@gmail.com\",\"PhoneNumber\":\"880542392\",\"EmailConfirmed\":true},{\"UserName\":\"client18\",\"Fullname\":\"Blackburn\",\"Email\":\"client18@gmail.com\",\"PhoneNumber\":\"922524312\",\"EmailConfirmed\":true}]";

        var rd = new Random();

        var clients = JsonSerializer.Deserialize<List<AppUser>>(clientsData);
        
        foreach (var c in clients)
        {
            var num = rd.Next(1, 100);
            c.Gender = Gender.Female;
            c.PhoneNumber = "0" + c.PhoneNumber;
            if (num % 2 == 0) c.Gender = Gender.Male;
            if (num % 3 == 0) c.Gender = Gender.Other;
            c.Role = Role.CLIENT;
            c.Dob = new DateTime(rd.Next(1991, 2001), 1, 1);
            await _userManager.CreateAsync(c, Password);
        }
        
        var newUser = new AppUser()
        {
            Fullname = "iCoaching client",
            UserName = "client",
            Email = "firstclient@gmail.com",
            PhoneNumber = "0906111111",
            EmailConfirmed = true,
            Dob = new DateTime(2001,1,1),
            Role = Role.CLIENT
        };

        await _userManager.CreateAsync(newUser, Password);
    }

    private static async Task AddAllUsersDefaultAvatarAsync()
    {
        var users = await _context.Users.ToListAsync();
        foreach (var user in users)
        {
            _context.MediaAssets.Add(new MediaAsset()
            {
                UserId = user.Id,
                PublicId = "ICoaching-Photos/base-ava_dp9skj.jpg",
                Url = "https://res.cloudinary.com/dh8i0cvtv/image/upload/v1678204037/ICoaching-Photos/base-ava_dp9skj.jpg",
                IsAvatar = true
            });
        }
    }

    private static async Task AddSampleTrainingCourseAsync()
    {
        var random = new Random();
        var coachIdList = await _context.Users
            .Where(u => u.Role == Role.COACH)
            .Select(u => u.Id)
            .ToListAsync();

        var season = new string[] { "xuân", "hè", "thu", "đông" };
        
        var courseList = coachIdList.Select(id =>
            {
                var ss = season[random.Next(season.Length)];
                return new TrainingCourse()
                {
                    Name = $"Khoá tập mùa {ss}",
                    Duration = 2,
                    CoachId = id,
                    Description = $"Tập để trở thành chiến binh mùa {ss}",
                    Price = random.Next(1000000, 10000000)
                };
            })
            .ToList();

        await _context.TrainingCourses.AddRangeAsync(courseList);
    }

    private static async Task AddSampleContractAsync()
    {
        var coaches = await _context.Users
            .Where(u => u.IsVerified == true)
            .Take(6)
            .Include(u => u.TrainingCourses)
            .ToListAsync();
        
        var client = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "client");
        
        foreach (var coach in coaches)
        {
            var course = coach.TrainingCourses.First();
            var newContract = new Contract()
            {
                CoachId = coach.Id,
                ClientId = client!.Id,
                Status = ContractStatus.Active,
                CourseName = course.Name,
                Duration = course.Duration,
                Price = course.Price,
                Description = course.Description,
                CreatedDate = DateTime.Now,
                Logs = new List<TrainingLog>()
            };
            for (var i = 1; i <= newContract.Duration; i++)
            {
                newContract.Logs.Add(new TrainingLog()
                {
                    DateNo = i,
                    Status = TrainingLogStatus.Init
                });
            }
            _context.Contracts.Add(newContract);
        }
    }

    private static async Task AddSampleCertAsync()
    {
        var verifiedCoachId = await _context.Users
            .Where(u => u.IsVerified == true)
            .Select(u => u.Id)
            .ToListAsync();
        foreach (var id in verifiedCoachId)
        {
            var cert = new CertificateSubmission()
            {
                CoachId = id,
                Status = CertStatus.Accepted,
                MediaAssets = new List<MediaAsset>()
                {
                    new MediaAsset()
                    {
                        UserId = id,
                        PublicId = "ICoaching-Photos/pid_wuhywb.jpg",
                        Url = "https://res.cloudinary.com/dh8i0cvtv/image/upload/v1682181073/ICoaching-Photos/pid_wuhywb.jpg",
                    },
                    new MediaAsset()
                    {
                        UserId = id,
                        PublicId = "ICoaching-Photos/poliquin_hvdurt.jpg",
                        Url = "https://res.cloudinary.com/dh8i0cvtv/image/upload/v1682181081/ICoaching-Photos/poliquin_hvdurt.jpg",
                        IsCert = true
                    }
                }
            };
            _context.CertificateSubmissions.Add(cert);
        }
    }
    
    
}