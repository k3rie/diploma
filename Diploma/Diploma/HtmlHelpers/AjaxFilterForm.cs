using System;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.WebPages.Html;
using WebGrease.Css.Ast;
using HtmlHelper = System.Web.Mvc.HtmlHelper;

namespace Diploma.HtmlHelpers
{
    public static class AjaxFilterFormHelper
    {
        private class AjaxFormDisposable : IDisposable
        {
            private readonly TextWriter _writer;
            private readonly string _formId;
            private readonly string _containerId;
            private readonly string _functionName;

            public AjaxFormDisposable(TextWriter writer, string formId, string containerId,string functionName)
            {
                _writer = writer;
                _formId = formId;
                _containerId = containerId;
                _functionName = functionName;
            }

            public void Dispose()
            {
                // Закрываем </form>
                _writer.Write("</form>");

                // Контейнер
                _writer.Write($"<section id=\"{_containerId}\"></section>");

                var sb = new StringBuilder();
                sb.AppendLine("<script>");
                sb.AppendLine("$(document).ready(function () {");
                sb.AppendLine("    const pageSizeElement = $('#PageSize');");
                sb.AppendLine("    if (pageSizeElement.length) {");
                sb.AppendLine("        const savedPageSize = parseInt(localStorage.getItem('Default_PageSize'));");
                sb.AppendLine("        if (!isNaN(savedPageSize)) {");
                sb.AppendLine("            pageSizeElement.val(savedPageSize);");
                sb.AppendLine("        }");
                sb.AppendLine("    }");

                sb.AppendLine($"    $('#{_formId}').on('submit', function (e) {{");
                sb.AppendLine("        e.preventDefault();");
                sb.AppendLine($"        {_functionName}();");
                sb.AppendLine("    });");

                sb.AppendLine($"    {_functionName}();");
                sb.AppendLine("});");

                sb.AppendLine();

                // ---- LoadData() ----
                sb.AppendLine($"function {_functionName}(pageNumber = 1, pageSize = window.currentPageSize || 50, sortColumn = '', isAscending = true) {{");
                sb.AppendLine($"    const $form = $('#{_formId}');");
                sb.AppendLine("    const outputFormat = $('#OutputFormat').val ? $('#OutputFormat').val() : 'View';");
                sb.AppendLine("    $('#loader').show();");

                // Экспорт Excel/PDF
                sb.AppendLine("    if (outputFormat === 'Excel' || outputFormat === 'Pdf') {");
                sb.AppendLine("        $.ajax({");
                sb.AppendLine("            url: $form.attr('action'),");
                sb.AppendLine("            type: 'POST',");
                sb.AppendLine("            data: $form.serialize(),");
                sb.AppendLine("            xhrFields: { responseType: 'blob' },");
                sb.AppendLine("            success: function (response, status, xhr) {");
                sb.AppendLine("                const disposition = xhr.getResponseHeader('Content-Disposition');");
                sb.AppendLine("                const contentType = xhr.getResponseHeader('Content-Type');");

                sb.AppendLine("                if (disposition && disposition.indexOf('attachment') !== -1) {");
                sb.AppendLine("                    // Проверяем, что пришёл Blob");
                sb.AppendLine("                    const blob = response instanceof Blob ? response : new Blob([response], { type: contentType });");
                sb.AppendLine("                    const url = window.URL.createObjectURL(blob);");
                sb.AppendLine("                    const a = document.createElement('a');");
                sb.AppendLine("                    a.href = url;");
                sb.AppendLine("                    const filenameMatch = disposition.match(/filename=\"?([^\";]+)\"?/);");
                sb.AppendLine("                    a.download = filenameMatch ? filenameMatch[1] : 'export';");
                sb.AppendLine("                    document.body.appendChild(a);");
                sb.AppendLine("                    a.click();");
                sb.AppendLine("                    a.remove();");
                sb.AppendLine("                    window.URL.revokeObjectURL(url);");
                sb.AppendLine("                } else {");
                sb.AppendLine("                    // Если данных нет, выводим сообщение");
                sb.AppendLine($"                    $('#{_containerId}').html('No data to export.');");
                sb.AppendLine("                }");
                sb.AppendLine("                $('#loader').hide();");
                sb.AppendLine("            },");
                sb.AppendLine("            error: function () {");
                sb.AppendLine($"                $('#{_containerId}').html('<div class=\"error\">Error exporting data</div>');");
                sb.AppendLine("                $('#loader').hide();");
                sb.AppendLine("            }");
                sb.AppendLine("        });");
                sb.AppendLine("        $('#OutputFormat').val('View');");
                sb.AppendLine("        return;");
                sb.AppendLine("    }");



                // ---- Иначе AJAX ----
                sb.AppendLine("    $.ajax({");
                sb.AppendLine("        url: $form.attr('action'),");
                sb.AppendLine("        type: $form.attr('method'),");
                sb.AppendLine("        data: $form.serialize(),");
                sb.AppendLine($"        success: function (response) {{ $('#{_containerId}').html(response); $('#loader').hide(); }},");
                sb.AppendLine($"        error: function (xhr, status, error) {{ $('#{_containerId}').html('Error loading data: ' + error); $('#loader').hide(); }}");
                sb.AppendLine("    });");

                sb.AppendLine("}");
                sb.AppendLine("</script>");

                _writer.Write(sb.ToString());
            }

        }

        /// <summary>
        /// Создаёт AJAX форму с автоматической сериализацией данных (JSON или form-urlencoded)
        /// </summary>
        public static IDisposable AjaxFilterForm(
            this HtmlHelper htmlHelper,
            string actionName,
            string controllerName,
            FormMethod method,
            object htmlAttributes = null,
            string containerId = "data_table",
            string functionName ="LoadData")
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var actionUrl = urlHelper.Action(actionName, controllerName);

            // <form>
            var formTag = new TagBuilder("form");
            formTag.MergeAttribute("action", actionUrl);
            formTag.MergeAttribute("method", method.ToString().ToLower());
            formTag.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));

            var formId = formTag.Attributes.ContainsKey("id")
                ? formTag.Attributes["id"]
                : "form_" + Guid.NewGuid().ToString("N");

            formTag.MergeAttribute("id", formId);

            htmlHelper.ViewContext.Writer.Write(formTag.ToString(TagRenderMode.StartTag));

            // Возвращаем IDisposable, чтобы при выходе из using закрыть форму, div и добавить script
            return new AjaxFormDisposable(htmlHelper.ViewContext.Writer, formId, containerId, functionName);
        }
    }
}