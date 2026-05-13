using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using BackOffice.API.Model.AccessControl;

namespace BackOffice.Web.HtmlHelpers
{
    public static class RolePermissionsHelper
    {
        public static MvcHtmlString RolePermissionsFor<TModel>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, IEnumerable<Permission>>> expression, IEnumerable<Permission> permissions)
        {
            // Получаем список прав из модели
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var selectedPermissions = metadata.Model as List<Permission>;

            var sb = new StringBuilder();
            sb.AppendLine("<ul class='role-actions-list permissions-list'>");

            foreach (Permission permission in permissions)
            {
                bool hasPermission = selectedPermissions != null && selectedPermissions.Contains(permission);

                string iconClass = hasPermission ? "fas fa-check-circle permission-yes" : "fas fa-times-circle permission-no";
                string textClass = hasPermission ? "permission-text-yes" : "permission-text-no";

                sb.AppendLine($"<li class='permission-item'>" +
                              $"<i class='{iconClass}'></i> " +
                              $"<span class='{textClass}'>{permission.GetDisplayName()}</span>" +
                              $"</li>");
            }

            sb.AppendLine("</ul>");
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}
