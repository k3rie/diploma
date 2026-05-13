using System.Linq;
using System.Security.Principal;
using Diploma.Core.Enums;

namespace Diploma.App_Start
{
    public static class AuthHelper
    {
        // Достаём Id текущего пользователя
        public static int GetUserId(IIdentity identity)
        {
            var part = identity.Name.Split('|').ElementAtOrDefault(0);
            return int.TryParse(part, out var id) ? id : 0;
        }

        // Достаём роль текущего пользователя
        public static UserRole GetRole(IIdentity identity)
        {
            var part = identity.Name.Split('|').ElementAtOrDefault(1);
            return int.TryParse(part, out var r) ? (UserRole)r : UserRole.Owner;
        }

        // Достаём полное имя
        public static string GetFullName(IIdentity identity)
        {
            return identity.Name.Split('|').ElementAtOrDefault(2) ?? "";
        }
    }
}