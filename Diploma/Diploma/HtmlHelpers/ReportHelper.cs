using BackOffice.API.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace BackOffice.Web.HtmlHelpers
{
    public static class ReportHelper
    {
        public static IHtmlString ImageLink(this HtmlHelper html, string fileName, object htmlAttributes = null)
        {
            if (string.IsNullOrEmpty(fileName))
                return MvcHtmlString.Empty;

            var section = (NameValueCollection)ConfigurationManager.GetSection("DepositRequestSetting");
            var relativePath = section["PathFiles"];
            var fullPath = VirtualPathUtility.ToAbsolute(relativePath + fileName);

            var tag = new TagBuilder("a");
            tag.MergeAttribute("href", fullPath);
            tag.MergeAttribute("target", "_blank");
            tag.SetInnerText("Check image");
            tag.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            return new MvcHtmlString(tag.ToString());
        }

        public static MvcHtmlString SortableHeaderFor<TModel, TProperty>(
            this HtmlHelper<IEnumerable<TModel>> html,
            Expression<Func<TModel, TProperty>> expression,
            IPagination pagination,
            string jsFunction = "sortTable",
            object htmlAttributes = null)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, new ViewDataDictionary<TModel>());
            string propertyName = GetPropertyName(expression);
            string displayName = metadata.DisplayName ?? propertyName;

            // Определяем иконку сортировки
            string iconClass = "fa-sort";
            string toggleDirection = "true";

            if (string.Equals(pagination?.SortColumn, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                bool isAscending = pagination.IsAscending ?? false;
                iconClass = isAscending ? "fa-sort-down" : "fa-sort-up";
                toggleDirection = isAscending ? "false" : "true";
            }

            var th = new TagBuilder("th");

            // Преобразуем htmlAttributes в словарь
            var attrs = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes ?? new { });

            // Объединяем class
            string existingClass = attrs.ContainsKey("class") ? attrs["class"].ToString() + " sortable" : "sortable";
            attrs["class"] = existingClass;

            // Устанавливаем onclick
            attrs["onclick"] = $"{jsFunction}('{propertyName}', {toggleDirection})";

            // Применяем атрибуты
            th.MergeAttributes(attrs, replaceExisting: true);

            // Иконка
            var icon = new TagBuilder("i");
            icon.AddCssClass($"fas {iconClass}");

            // Содержимое
            th.InnerHtml = $"{html.Encode(displayName)} {icon.ToString(TagRenderMode.Normal)}";

            return MvcHtmlString.Create(th.ToString());
        }


        public static MvcHtmlString SortableHeader(
            this HtmlHelper html,
            string columnName,
            string displayName,
            IPagination pagination,
            string jsFunction = "sortTable")
        {
            // Иконка сортировки по умолчанию
            string iconClass = "fa-sort";
            string toggleDirection = "true";

            if (string.Equals(pagination.SortColumn, columnName, StringComparison.OrdinalIgnoreCase))
            {
                iconClass = pagination.IsAscending ?? false ? "fa-sort-down" : "fa-sort-up";
                toggleDirection = pagination.IsAscending ?? false ? "false" : "true";
            }

            // Создаём <th>
            var th = new TagBuilder("th");
            th.AddCssClass("sortable");
            th.MergeAttribute("onclick", $"{jsFunction}('{columnName}', {toggleDirection})");

            // Иконка FontAwesome
            var icon = new TagBuilder("i");
            icon.AddCssClass($"fas {iconClass}");

            // Внутренний HTML
            th.InnerHtml = $"{html.Encode(displayName)} {icon.ToString(TagRenderMode.Normal)}";

            return MvcHtmlString.Create(th.ToString());
        }

        public static MvcHtmlString FormatAmount<TModel, TProperty>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression)
        {
            // Получаем метаданные для конкретного свойства
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

            // Значение формата по умолчанию, если DisplayFormat не задан
            string format = "{0:0.00}";

            // Проверяем, если задан DisplayFormat
            if (!string.IsNullOrEmpty(metadata.DisplayFormatString))
            {
                format = metadata.DisplayFormatString;
            }

            // Получаем значение свойства
            var amount = (decimal?)metadata.Model;

            // Форматируем значение
            string formattedAmount = string.Format(format, amount);

            // Возвращаем HTML в зависимости от значения amount
            if (amount > 0)
            {
                return new MvcHtmlString($"<div class=\"positive\">{formattedAmount}</div>");
            }
            else if (amount < 0)
            {
                return new MvcHtmlString($"<div class=\"negative\">{formattedAmount}</div>");
            }
            else
            {
                return new MvcHtmlString(formattedAmount);
            }
        }

        public static MvcHtmlString FormatAmount(this HtmlHelper html, decimal? amount, string format = "{0:N2}", string suffixText = null)
        {
            if (amount != null)
            {
                decimal value = amount.Value;
                string formattedAmount = string.Format(format, value);

                if (!string.IsNullOrEmpty(suffixText))
                {
                    formattedAmount += " " + suffixText;
                }

                if (value > 0)
                {
                    return new MvcHtmlString($"<div class=\"positive\">{formattedAmount}</div>");
                }
                else if (value < 0)
                {
                    return new MvcHtmlString($"<div class=\"negative\">{formattedAmount}</div>");
                }
                else
                {
                    return new MvcHtmlString(formattedAmount);
                }
            }

            return MvcHtmlString.Empty;
        }
        public static MvcHtmlString FormatAmount(this HtmlHelper html, double? amount, string format = "{0:N2}", string suffixText = null)
        {
            if (amount != null)
            {
                double value = amount.Value;
                string formattedAmount = string.Format(format, value);

                if (!string.IsNullOrEmpty(suffixText))
                {
                    formattedAmount += " " + suffixText;
                }

                if (value > 0)
                {
                    return new MvcHtmlString($"<div class=\"positive\">{formattedAmount}</div>");
                }
                else if (value < 0)
                {
                    return new MvcHtmlString($"<div class=\"negative\">{formattedAmount}</div>");
                }
                else
                {
                    return new MvcHtmlString(formattedAmount);
                }
            }
            return new MvcHtmlString("");
        }
        public static MvcHtmlString FormatAmountPlain(this HtmlHelper html, decimal? amount, string format = "{0:N2}", string suffixText = null)
        {
            if (amount == null)
                return new MvcHtmlString(string.Empty);

            decimal value = amount.Value;
            string formattedAmount = string.Format(format, value);

            if (!string.IsNullOrEmpty(suffixText))
            {
                formattedAmount += " " + suffixText;
            }

            return new MvcHtmlString(formattedAmount);
        }

        public static MvcHtmlString DisplayEnumFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, TEnum enumValue) where TEnum : Enum
        {
            var field = typeof(TEnum).GetField(enumValue.ToString());
            var attribute = field?.GetCustomAttribute<DisplayAttribute>();
            var displayName = attribute?.GetName() ?? enumValue.ToString(); // <-- ключевая строчка

            return MvcHtmlString.Create(displayName);
        }
        public static MvcHtmlString DisplayEnumFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression) where TEnum : struct, Enum
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var enumValue = (TEnum)metadata.Model;

            var field = typeof(TEnum).GetField(enumValue.ToString());
            var attribute = field?.GetCustomAttribute<DisplayAttribute>();
            var displayName = attribute?.GetName() ?? enumValue.ToString(); // <-- ключевая строчка

            return MvcHtmlString.Create(displayName);
        }
        // Метод для получения имени последнего свойства в цепочке.
        private static string GetPropertyName<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand)
            {
                return operand.Member.Name;
            }

            throw new ArgumentException("Invalid expression", nameof(expression));
        }
        public static MvcHtmlString FormatInvert<TModel, TProperty>(
             this HtmlHelper<TModel> html,
             Expression<Func<TModel, TProperty>> expression)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

            string format = "{0:N2}";

            if (!string.IsNullOrEmpty(metadata.DisplayFormatString))
            {
                format = metadata.DisplayFormatString;
            }

            var amount = metadata.Model as decimal?;

            if (amount == null)
                return new MvcHtmlString(string.Empty);

            var value = -amount.Value; // просто меняем знак

            string formattedAmount = string.Format(format, value);

            return new MvcHtmlString(formattedAmount);
        }

        public static MvcHtmlString DisplayFormat(this HtmlHelper html, decimal value)
        {
            return new MvcHtmlString(string.Format("{0:N2}", value));
        }

        public static MvcHtmlString DisplayFormat(this HtmlHelper html, double value)
        {
            return new MvcHtmlString(string.Format("{0:N2}", value));
        }
        public static MvcHtmlString DisplayNameForModelProperty<TModel, TValue>(
           this HtmlHelper<IEnumerable<TModel>> html,
           Expression<Func<TModel, TValue>> expression)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, new ViewDataDictionary<TModel>());
            var displayName = metadata.DisplayName ?? metadata.PropertyName ?? string.Empty;

            return new MvcHtmlString(displayName);
        }
    }
}