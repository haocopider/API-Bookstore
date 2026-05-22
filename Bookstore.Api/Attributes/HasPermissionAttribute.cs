using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bookstore.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class HasPermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _permission;

        // Truyền vào mã quyền cần thiết (VD: "CREATE_BOOK")
        public HasPermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Kiểm tra xem User đã đăng nhập chưa (Token có hợp lệ không)
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult(); // 401 Unauthorized
                return;
            }

            // Tìm tất cả các Claim có type là "Permission" trong JWT Token
            var userPermissions = user.FindAll("Permission").Select(c => c.Value).ToList();

            // Nếu user không có quyền được yêu cầu, chặn truy cập
            if (!userPermissions.Contains(_permission))
            {
                context.Result = new ForbidResult(); // 403 Forbidden
            }
        }
    }
}
