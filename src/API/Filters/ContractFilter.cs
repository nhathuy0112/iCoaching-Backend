using System.Security.Claims;
using API.ErrorResponses;
using Core.Entities;
using Core.Entities.Status;
using Core.Interfaces;
using Core.Interfaces.Base;
using Core.Specifications.Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class ContractFilter : IAsyncActionFilter
{
    private readonly IUnitOfWork _unitOfWork;

    public ContractFilter(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;
        var userId = context.HttpContext.User.FindFirstValue("Id");
        var isAdmin = context.HttpContext.User.IsInRole("ADMIN");
        var arguments = context.ActionArguments;
        var contractId = (int) arguments["contractId"];
        var contract = isAdmin ? 
            await _unitOfWork.Repository<Contract>().GetBySpecificationAsync(new ContractSpec(contractId)) :
            await _unitOfWork.Repository<Contract>().GetBySpecificationAsync(new ContractByUserIdWithReportSpec(userId, contractId));
        
        if (contract == null)
        {
            context.Result = new BadRequestObjectResult(new ErrorResponse(400, "Không hợp lệ"));
            return;
        }

        if (!isAdmin)
        {
            if (contract!.Status is (ContractStatus.Complete or ContractStatus.Canceled) && method != "GET")
            {
                context.Result = new BadRequestObjectResult(new ErrorResponse(400, "Hợp đồng đã kêt thúc"));
                return;
            }

            if (contract.Reports.Count != 0 && method != "GET")
            {
                context.Result =
                    new BadRequestObjectResult(new ErrorResponse(400, "Hợp đồng đã bị báo cáo và đang được xử lí"));
                return;
            }
        
            if (contract.Status == ContractStatus.Pending && method != "GET")
            {
                if (contract.Reports.Count != 0)
                {
                    context.Result = new BadRequestObjectResult(new ErrorResponse(400, "Hợp đồng đã bị khiếu nại và đang chờ xử lí"));
                }
                else
                {
                    var endpoint = context.HttpContext.GetEndpoint()?.DisplayName;
                    if (!string.IsNullOrEmpty(endpoint) && endpoint.Contains("ReportContract")) await next();
                    context.Result =
                        new BadRequestObjectResult(new ErrorResponse(400, "Hợp đồng chờ được hoàn thành. Bạn không thể thực hiện tính năng này."));
                }
                return;
            }
        }

        await next();
    }
}