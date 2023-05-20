using API.ErrorResponses;
using API.Filters;
using API.Helpers;
using Core.Interfaces;
using Core.Interfaces.Base;
using Core.Interfaces.User;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;

namespace API.Extensions;

public static class ApplicationServiceExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        #region DI

        services.AddScoped<IUserService,UserService>();
        services.AddScoped<IMailService, SendGridMailService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ITrainingService, TrainingService>();
        services.AddScoped<IContractService, ContractService>();
        services.AddScoped<CoachVerificationFilter>();
        services.AddScoped<IVnPayService, VnPayService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<ContractFilter>();
        services.AddScoped<IsLockedFilter>();
        
        #endregion

        //Configure response when a model state is invalid
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                var errors = new Dictionary<string, string[]>();
                foreach (var (key, modelStateEntry) in actionContext.ModelState)
                {
                    errors.Add(key, modelStateEntry.Errors
                        .Where(e => !string.IsNullOrEmpty(e.ErrorMessage))
                        .Select(valueError => valueError.ErrorMessage)
                        .ToArray());
                }
                var errorResponses = new ValidationErrorResponse()
                {
                    Errors = errors
                };
                return new BadRequestObjectResult(errorResponses);
            };
        });
        return services;
    }
}