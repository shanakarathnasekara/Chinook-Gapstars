using System.Security.Claims;

namespace Chinook.Services.Users
{
    public interface IUserService
    {
        string GetUserId(ClaimsPrincipal user);
    }
}
