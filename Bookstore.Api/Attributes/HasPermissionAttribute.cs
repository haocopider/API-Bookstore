using Bookstore.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bookstore.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class HasPermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _permission;

        public HasPermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var permissions = user
                .FindFirst("permissions")?
                .Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim());

            if (permissions == null ||
                !permissions.Any(p => p.Equals(_permission, StringComparison.OrdinalIgnoreCase)))
            {
                context.Result = new JsonResult(new ApiResponse
                {
                    Success = false,
                    Message = $"Bạn không có quyền {_permission}"
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}
