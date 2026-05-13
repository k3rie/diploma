using System;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace BackOffice.Web.HtmlHelpers
{
    public static class AjaxFormHelper
    {
        private class AjaxFormDisposable : IDisposable
        {
            private readonly TextWriter _writer;
            private readonly string _formId;
            private readonly string _containerId;

            public AjaxFormDisposable(TextWriter writer, string formId, string containerId)
            {
                _writer = writer;
                _formId = formId;
                _containerId = containerId;
            }

            public void Dispose()
            {
                // закрываем форму
                _writer.Write("</form>");

                // JS для ajax submit с поддержкой файлов
                var sb = new StringBuilder();
                sb.AppendLine("<script>");
                sb.AppendLine("$(document).ready(function() {");
                sb.AppendLine($"  $('#{_formId}').submit(function(e) {{");
                sb.AppendLine("    e.preventDefault();");
                sb.AppendLine("    $('#loader').show();");
                sb.AppendLine("    var $form = $(this);");
                sb.AppendLine("    var method = ($form.attr('method') || 'get').toLowerCase();");
                sb.AppendLine("    var ajaxOptions = {");
                sb.AppendLine("        url: $form.attr('action'),");
                sb.AppendLine("        type: method,");
                sb.AppendLine($"        success: function(response) {{ $('#{_containerId}').html(response); $('#loader').hide(); }},");
                sb.AppendLine($"        error: function() {{ $('#{_containerId}').html('<span class=\"error\">Error loading data</span>'); $('#loader').hide(); }}");
                sb.AppendLine("    };");

                // ?? ВОТ ЕДИНСТВЕННОЕ ВАЖНОЕ ИЗМЕНЕНИЕ
                sb.AppendLine("    if (method === 'get') {");
                sb.AppendLine("        ajaxOptions.data = $form.serialize();"); // ? для GET
                sb.AppendLine("    } else {");
                sb.AppendLine("        ajaxOptions.data = new FormData($form[0]);"); // ? для POST (файлы)");
                sb.AppendLine("        ajaxOptions.processData = false;");
                sb.AppendLine("        ajaxOptions.contentType = false;");
                sb.AppendLine("    }");

                sb.AppendLine("    $.ajax(ajaxOptions);");
                sb.AppendLine("  });");
                sb.AppendLine("});");
                sb.AppendLine("</script>");

                _writer.Write(sb.ToString());

                // закрываем контейнер
                _writer.Write("</div>");
            }
        }

        public static IDisposable ModalPageForm(
            this HtmlHelper htmlHelper,
            string actionName,
            string controllerName,
            FormMethod method,
            object htmlAttributes = null,
            string containerId = null)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var actionUrl = urlHelper.Action(actionName, controllerName);

            if (string.IsNullOrEmpty(containerId))
                containerId = "container_" + Guid.NewGuid().ToString("N").Substring(0, 8);

            // открываем контейнер
            htmlHelper.ViewContext.Writer.Write($"<div id=\"{containerId}\">");

            // создаём форму
            var formTag = new TagBuilder("form");
            formTag.MergeAttribute("action", actionUrl);
            formTag.MergeAttribute("method", method.ToString().ToLower());

            // объединяем атрибуты htmlAttributes
            var attrs = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes ?? new { });
            foreach (var attr in attrs)
            {
                if (!formTag.Attributes.ContainsKey(attr.Key))
                    formTag.MergeAttribute(attr.Key, attr.Value?.ToString());
            }

            // id формы
            if (!formTag.Attributes.ContainsKey("id"))
                formTag.MergeAttribute("id", "form_" + Guid.NewGuid().ToString("N").Substring(0, 8));

            htmlHelper.ViewContext.Writer.Write(formTag.ToString(TagRenderMode.StartTag));

            return new AjaxFormDisposable(htmlHelper.ViewContext.Writer, formTag.Attributes["id"], containerId);
        }
    }
}
