using BackOffice.API.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BackOffice.Web.HtmlHelpers
{
    public static class PaginationHelper
    {
        public static MvcHtmlString TablePagination(this HtmlHelper html, IPagination pagination,string functionName="LoadData")
        {
            var sb = new StringBuilder();

            sb.AppendLine("<div class='pagination-container' style='display:flex; justify-content:space-between; align-items:center; gap:10px;'>");

            sb.AppendLine("<div class='pagination-size'>#Records");
            sb.AppendLine("<select id='rowsPerPage' class='pagination-select' onchange=\"UpdatePageSize(this.value)\">");
            var pageSizes = new[] { 10, 20, 50, 100 };
            foreach (var size in pageSizes)
            {
                sb.AppendLine($"<option value='{size}' {(size == pagination.PageSize ? "selected" : "")}>{size}</option>");
            }
            sb.AppendLine("</select>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='pagination-controls'>");

            int maxLinks = 9;
            int halfMaxLinks = maxLinks / 2;
            int startPage = Math.Max(1, pagination.CurrentPage - halfMaxLinks);
            int endPage = Math.Min(pagination.TotalPages, pagination.CurrentPage + halfMaxLinks);

            if (endPage - startPage + 1 < maxLinks)
            {
                if (startPage == 1)
                    endPage = Math.Min(pagination.TotalPages, startPage + maxLinks - 1);
                else if (endPage == pagination.TotalPages)
                    startPage = Math.Max(1, endPage - maxLinks + 1);
            }

            sb.AppendLine($"<button class='pagination-btn' onclick='PrevPage()' {(pagination.CurrentPage == 1 ? "disabled" : "")}>Previous</button>");

            if (startPage > 1)
                sb.AppendLine("<button class='pagination-btn' onclick='ChangePage(1)'>1</button><span class='pagination-dots'>...</span>");

            for (int i = startPage; i <= endPage; i++)
            {
                if (i == pagination.CurrentPage)
                    sb.AppendLine($"<button class='pagination-btn active'>{i}</button>");
                else
                    sb.AppendLine($"<button class='pagination-btn' onclick='ChangePage({i})'>{i}</button>");
            }

            if (endPage < pagination.TotalPages)
                sb.AppendLine($"<span class='pagination-dots'>...</span><button class='pagination-btn' onclick='ChangePage({pagination.TotalPages})'>{pagination.TotalPages}</button>");

            sb.AppendLine($"<button class='pagination-btn' onclick='NextPage()' {(pagination.CurrentPage == pagination.TotalPages ? "disabled" : "")}>Next</button>");

            sb.AppendLine("</div>"); // controls

            sb.AppendLine("<div class='pagination-info'>");
            sb.AppendLine($"Total <span id='totalRows'>{pagination.TotalItem}</span> rows in <span id='totalPages'>{pagination.TotalPages}</span> pages");
            sb.AppendLine("</div>");

            sb.AppendLine("</div>"); // container

            sb.AppendLine("<script>");
            sb.AppendLine($@"
        function UpdatePageSize(size) {{
            $('#PageSize').val(size);
            $('#CurrentPage').val(1);
            {functionName}();
        }}

        function ChangePage(page) {{
            $('#CurrentPage').val(page);
            {functionName}();
        }}

        function PrevPage() {{
            let current = parseInt($('#CurrentPage').val());
            if (current > 1) {{
                $('#CurrentPage').val(current - 1);
                {functionName}();
            }}
        }}

        function NextPage() {{
            let current = parseInt($('#CurrentPage').val());
            let total = parseInt($('#totalPages').text());
            if (current < total) {{
                $('#CurrentPage').val(current + 1);
                {functionName}();
            }}
        }}

        function sortTable(column,isAscending) {{
            const currentColumn = $('#SortColumn').val();
            const currentAsc = $('#IsAscending').val();
            if (currentColumn === column) {{
                $('#IsAscending').val(isAscending);
            }} else {{
                $('#SortColumn').val(column);
                $('#IsAscending').val(isAscending);
            }}
            $('#CurrentPage').val(1);
            {functionName}();
        }}
    ");
            sb.AppendLine("</script>");

            return MvcHtmlString.Create(sb.ToString());
        }

        public static MvcHtmlString Pagination(this HtmlHelper html, IPagination pagination)
        {
            if (pagination.TotalPages <= 1) return MvcHtmlString.Empty;

            var paginationHtml = new StringBuilder();

            paginationHtml.Append("<div class='pagination-container' style='display:flex; justify-content:space-between; align-items:center; gap:10px;'>");

            // ---------- Left: Page size ----------
            paginationHtml.Append("<div class='pagination-size'>");
            paginationHtml.Append("Page size: <select id='rowsPerPage' class='pagination-select' onchange=\"UpdatePageSize(this.value)\">");
            var pageSizes = new[] { 10, 20, 50, 100 };
            foreach (var size in pageSizes)
            {
                paginationHtml.Append($"<option value='{size}' {(size == pagination.PageSize ? "selected" : "")}>{size}</option>");
            }
            paginationHtml.Append("</select>");
            paginationHtml.Append("</div>");

            // ---------- Center: Pagination buttons ----------
            paginationHtml.Append("<div class='pagination-controls'>");

            int prevPage = Math.Max(1, pagination.CurrentPage - 1);
            int nextPage = Math.Min(pagination.TotalPages, pagination.CurrentPage + 1);

            // Previous
            paginationHtml.Append($"<button class='pagination-btn' onclick=\"LoadItemList({prevPage}, {pagination.PageSize}, '{pagination.SortColumn}', {(pagination.IsAscending ?? false ? "true" : "false")})\" {(pagination.CurrentPage == 1 ? "disabled" : "")}>Previous</button>");

            // Numbered pages
            int maxLinks = 9;
            int halfMaxLinks = maxLinks / 2;
            int startPage = Math.Max(1, pagination.CurrentPage - halfMaxLinks);
            int endPage = Math.Min(pagination.TotalPages, pagination.CurrentPage + halfMaxLinks);

            if (endPage - startPage + 1 < maxLinks)
            {
                if (startPage == 1)
                    endPage = Math.Min(pagination.TotalPages, startPage + maxLinks - 1);
                else if (endPage == pagination.TotalPages)
                    startPage = Math.Max(1, endPage - maxLinks + 1);
            }

            if (startPage > 1)
                paginationHtml.Append($"<button class='pagination-btn' onclick=\"LoadItemList(1, {pagination.PageSize}, '{pagination.SortColumn}', {(pagination.IsAscending ?? false ? "true" : "false")})\">1</button><span class='pagination-dots'>...</span>");

            for (int i = startPage; i <= endPage; i++)
            {
                if (i == pagination.CurrentPage)
                    paginationHtml.Append($"<button class='pagination-btn active'>{i}</button>");
                else
                    paginationHtml.Append($"<button class='pagination-btn' onclick=\"LoadItemList({i}, {pagination.PageSize}, '{pagination.SortColumn}', {(pagination.IsAscending ?? false ? "true" : "false")})\">{i}</button>");
            }

            if (endPage < pagination.TotalPages)
                paginationHtml.Append($"<span class='pagination-dots'>...</span><button class='pagination-btn' onclick=\"LoadItemList({pagination.TotalPages}, {pagination.PageSize}, '{pagination.SortColumn}', {(pagination.IsAscending ?? false ? "true" : "false")})\">{pagination.TotalPages}</button>");

            // Next
            paginationHtml.Append($"<button class='pagination-btn' onclick=\"LoadItemList({nextPage}, {pagination.PageSize}, '{pagination.SortColumn}', {(pagination.IsAscending ?? false ? "true" : "false")})\" {(pagination.CurrentPage == pagination.TotalPages ? "disabled" : "")}>Next</button>");

            paginationHtml.Append("</div>"); // end pagination-controls

            // ---------- Right: Info ----------
            paginationHtml.Append("<div class='pagination-info'>");
            paginationHtml.Append($"Total <span id='totalRows'>{pagination.TotalItem}</span> rows in <span id='totalPages'>{pagination.TotalPages}</span> pages");
            paginationHtml.Append("</div>");

            paginationHtml.Append("</div>"); // end container

            return MvcHtmlString.Create(paginationHtml.ToString());
        }
    }
}