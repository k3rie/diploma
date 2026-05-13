
using Diploma.Models;
using System.Web;
using System.Web.Security;

namespace Diploma.Helpers
{
    public static class UserIdentityHelper
    {
        public static UserIdentityData GetCurrentUser()
        {
            if (HttpContext.Current?.User?.Identity == null || !HttpContext.Current.User.Identity.IsAuthenticated)
                return null;

            var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie == null)
                return null;

            var ticket = FormsAuthentication.Decrypt(cookie.Value);
            if (ticket == null || ticket.Expired)
                return null;

            var userData = ticket.UserData.Split('|');
            if (userData.Length != 3)
                return null;

            return new UserIdentityData
            {
                UserId = int.Parse(userData[0]),
                Role = (UserRole)int.Parse(userData[1]),
                CompanyId = string.IsNullOrEmpty(userData[2]) ? (int?)null : int.Parse(userData[2])
            };
        }

        public static bool IsInRole(UserRole role)
        {
            var user = GetCurrentUser();
            return user != null && user.Role == role;
        }

        public static bool IsInAnyRole(params UserRole[] roles)
        {
            var user = GetCurrentUser();
            if (user == null) return false;

            foreach (var role in roles)
            {
                if (user.Role == role) return true;
            }
            return false;
        }
    }

    public class UserIdentityData
    {
        public int UserId { get; set; }
        public UserRole Role { get; set; }
        public int? CompanyId { get; set; }
    }
}