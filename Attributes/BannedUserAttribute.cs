using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace KnowledgeFlowApi.Attributes;
public class BannedUserAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        
        var userIdClaim = context.HttpContext.User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!int.TryParse(userIdClaim, out int userId))
        {
            context.Result = new BadRequestObjectResult("Invalid user ID.");
            return;
        }

        // Resolve the ReportService (or DbContext) from the DI container
        var reportService = context.HttpContext.RequestServices.GetService<ReportService>();

        // Check if the user is banned
        if (reportService.IsBanned(userId))
        {
            context.Result = new ForbidResult("You are banned from accessing this resource.");
        }
    }
}