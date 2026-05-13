using BackOffice.API.Model.PersonalAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
namespace BackOffice.Web.HtmlHelpers
{
    public static class ToolTipHelpers
    {
        public static MvcHtmlString BusinessCard(this HtmlHelper html, UserInfo info)
        {
            var bc = new StringBuilder();

            if (info == null)
            {
                bc.Append("<td class=\"business-card\" data-info=\"No information available\">No information available</td>");
                return MvcHtmlString.Create(bc.ToString());
            }

            // Формируем data-info строку
            var dataInfo = new List<string>
            {
                $"{info.FirstName} {info.LastName}" ?? "",
                info.LastName ?? "",
                info.Address1 ?? "",
                info.Email ?? "",
                info.PhoneNumber ?? ""
            };

            // Генерируем HTML
            bc.AppendFormat("<td class=\"business-card\" data-info=\"{0}\">{1}</td>",
                string.Join(", ", dataInfo),
                $"{info.FirstName} {info.LastName}".Trim());

            return MvcHtmlString.Create(bc.ToString());
        }
    }
}