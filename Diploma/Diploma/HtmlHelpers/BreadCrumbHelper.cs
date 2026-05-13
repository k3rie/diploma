
using BackOffice.Web.Models;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace BackOffice.Web.HtmlHelpers
{
    public static class HtmlHelpers
    {
        public static MvcHtmlString BreadCrumbHelper(this HtmlHelper htmlHelper, IList<BreadCrumb> items)
        {
            if (items == null || items.Count == 0)
                return MvcHtmlString.Empty;

            var sb = new StringBuilder();
            sb.Append("<nav class=\"breadcrumb\">");

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                bool isLast = (i == items.Count - 1);

                if (!isLast)
                {
                    sb.AppendFormat("<a href=\"{0}\">{1}</a>", item.Url, item.Name);
                    sb.Append("<i data-lucide=\"chevron-right\"></i>");
                }
                else
                {
                    sb.AppendFormat("<span>{0}</span>", item.Name);
                }
            }

            sb.Append("</nav>");
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}
