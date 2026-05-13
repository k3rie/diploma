using System.Collections.Generic;
using System.Linq;
namespace BackOffice.Web.HtmlHelpers
{
    public static class PermissionExtensions
    {
        public static bool AnyOf<T>(this IEnumerable<T> source, params T[] values)
        {
            return source.Intersect(values).Any();
        }
    }
}