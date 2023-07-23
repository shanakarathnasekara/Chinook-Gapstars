using Chinook.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Chinook.Services.Users
{
    public class UserService: IUserService
    {
        public UserService() { }
        public string GetUserId(ClaimsPrincipal user)
        {
            if (user == null)
            {
                throw new CustomException()
                {
                    CustomMessage = "Failed in retrieving logged in user details"
                };
            }

            string? userId = "";            
            userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;            
            return userId;
        }
    }
}
