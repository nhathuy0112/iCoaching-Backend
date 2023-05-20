using Core.Entities;
using Core.Specifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public static class UserManagerExtRepo
{
    public static async Task<AppUser?> FindBySpecAsync(this UserManager<AppUser> userManager, ISpecification<AppUser> specification)
    {
        return await userManager.ApplySpecification(specification).FirstOrDefaultAsync();
    }

    public static async Task<IReadOnlyList<AppUser>> ListAsync(this UserManager<AppUser> userManager,
        ISpecification<AppUser> specification)
    {
        return await userManager.ApplySpecification(specification).ToListAsync();
    }

    public static async Task<int> CountAsync(this UserManager<AppUser> userManager,
        ISpecification<AppUser> specification)
    {
        return await userManager.ApplySpecification(specification).CountAsync();
    }

    private static IQueryable<AppUser> ApplySpecification(this UserManager<AppUser> userManager,ISpecification<AppUser> specification)
    {
        return SpecificationEvaluator<AppUser>.GetQuery(userManager.Users, specification);
    }
}