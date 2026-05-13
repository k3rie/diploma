using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Parser.SyntaxTree;
namespace Diploma.HtmlHelpers
{
    public static class EnumMultiSelectForHtmlHelper
    {
        public static MvcHtmlString DisplayNameWithAsteriskFor<TModel, TProperty>(
        this HtmlHelper<TModel> html,
        Expression<Func<TModel, TProperty>> expression)
        {
            // Получение имени свойства
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var propertyName = metadata.PropertyName;

            // Проверка на наличие атрибута Required
            var modelType = typeof(TModel);
            var property = modelType.GetProperty(propertyName);
            var isRequired = property?.GetCustomAttributes(typeof(RequiredAttribute), false).Any() ?? false;

            // Формирование HTML
            var displayName = metadata.DisplayName ?? propertyName;
            //var result = isRequired
            //    ? $"{displayName} <span style=\"color: red;\">*</span>"
            //    : displayName;
            var result = new StringBuilder();
            result.AppendFormat("{0} <span style=\"color: red;\">*</span>", displayName);


            return MvcHtmlString.Create(result.ToString());
        }
        // Универсальный хелпер для множественного выбора с использованием IEnumerable<int> и IEnumerable<Tuple<int, string>>
        public static MvcHtmlString MultiSelectFor<TModel>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, IEnumerable<int>>> selectedValuesExpression,
            IEnumerable<Tuple<int, string>> availableValues,
            string containerClass = "custom-select-container",
            string buttonClass = "custom-select-button",
            string dropdownClass = "custom-select-dropdown",
            string optionClass = "custom-option")
        {
            // Получаем имя свойства для выбранных значений
            var propertyName = GetPropertyName(selectedValuesExpression);
            var displayName = GetDisplayNameOrPropertyName(selectedValuesExpression);

            // Получаем выбранные значения из модели
            var selectedValues = selectedValuesExpression.Compile().Invoke(htmlHelper.ViewData.Model) ?? new List<int>();

            var selectListItems = availableValues.Select(item => new SelectListItem
            {
                Value = item.Item1.ToString(),
                Text = item.Item2,
                Selected = selectedValues.Contains(item.Item1) // Проверяем, если значение присутствует в selectedValues
            }).ToList();

            // Подсчитываем количество выбранных элементов
            var selectedCount = selectedValues.Count();

            var container = new TagBuilder("div");
            container.AddCssClass(containerClass);

            // Кнопка
            var button = new TagBuilder("button");
            button.AddCssClass(buttonClass);
            button.Attributes.Add("type", "button");
            button.Attributes.Add("onclick", $"toggleDropdown('{propertyName}')");

            // Текст на кнопке (если выбраны элементы, отображаем их количество)
            button.InnerHtml = selectedCount > 0 ? $"{displayName} Selected: {selectedCount}" : $"{displayName} Select";

            // Выпадающее меню
            var dropdown = new TagBuilder("div");
            dropdown.AddCssClass(dropdownClass);
            dropdown.Attributes.Add("id", $"dropdown-{propertyName}");
            dropdown.Attributes.Add("style", "display: none;");

            foreach (var item in selectListItems)
            {
                var label = new TagBuilder("label");
                label.AddCssClass(optionClass);

                var checkbox = new TagBuilder("input");
                checkbox.Attributes.Add("type", "checkbox");
                checkbox.Attributes.Add("name", propertyName); // Имя поля для передачи значений в массив
                checkbox.Attributes.Add("value", item.Value);
                checkbox.Attributes.Add("onchange", $"updateSelectedCount('{propertyName}','{displayName}')");

                // Если значение выбрано, ставим атрибут checked
                if (item.Selected) checkbox.Attributes.Add("checked", "checked");

                // Уникальный id для каждого чекбокса
                checkbox.Attributes.Add("id", $"{propertyName}-{item.Value}");

                var span = new TagBuilder("span");
                span.SetInnerText(item.Text);

                label.InnerHtml = checkbox.ToString(TagRenderMode.SelfClosing) + span.ToString();
                dropdown.InnerHtml += label.ToString();
            }

            // Генерация финального HTML
            container.InnerHtml = button.ToString() + dropdown.ToString();
            return MvcHtmlString.Create(container.ToString());
        }


        // Универсальный хелпер для Enum с множественным выбором
        public static MvcHtmlString EnumMultiSelectFor<TModel, TEnum>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, IEnumerable<TEnum>>> expression,
            string containerClass = "custom-select-container",
            string buttonClass = "custom-select-button",
            string dropdownClass = "custom-select-dropdown",
            string optionClass = "custom-option")
            where TEnum : Enum  // Ограничение типа TEnum, чтобы он был перечислением
        {
            // Получаем имя свойства из выражения
            var propertyName = GetPropertyName(expression);

            var displayName = GetDisplayNameOrPropertyName(expression);

            // Получаем все значения перечисления
            var enumValues = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();

            // Извлекаем выбранные значения из модели
            var selectedValues = expression.Compile().Invoke(htmlHelper.ViewData.Model) ?? new List<TEnum>();

            var selectListItems = enumValues.Select(value => new SelectListItem
            {
                Value = Convert.ToInt32(value).ToString(),
                Text = GetEnumDescription(value),
                Selected = selectedValues.Contains(value) // Проверяем, если значение присутствует в selectedValues
            }).ToList();

            // Подсчитываем количество выбранных элементов
            var selectedCount = selectedValues.Count();

            var container = new TagBuilder("div");
            container.AddCssClass(containerClass);

            // Кнопка
            var button = new TagBuilder("button");
            button.AddCssClass(buttonClass);
            button.Attributes.Add("type", "button");
            button.Attributes.Add("onclick", $"toggleDropdown('{propertyName}')");

            // Текст на кнопке (если выбраны элементы, отображаем их количество)
            button.InnerHtml = selectedCount > 0 ? $"{displayName} Selected: {selectedCount}" : $"{displayName} Select";

            // Выпадающее меню
            var dropdown = new TagBuilder("div");
            dropdown.AddCssClass(dropdownClass);
            dropdown.Attributes.Add("id", $"dropdown-{propertyName}");
            dropdown.Attributes.Add("style", "display: none;");

            foreach (var item in selectListItems)
            {
                var label = new TagBuilder("label");
                label.AddCssClass(optionClass);

                var checkbox = new TagBuilder("input");
                checkbox.Attributes.Add("type", "checkbox");
                checkbox.Attributes.Add("value", item.Value);
                checkbox.Attributes.Add("onchange", $"updateSelectedCount('{propertyName}','{displayName}')");
                if (item.Selected) checkbox.Attributes.Add("checked", "checked"); // Устанавливаем checked, если значение выбрано

                var span = new TagBuilder("span");
                span.SetInnerText(item.Text);

                label.InnerHtml = checkbox.ToString(TagRenderMode.SelfClosing) + span.ToString();
                dropdown.InnerHtml += label.ToString();
            }

            // Генерация финального HTML
            container.InnerHtml = button.ToString() + dropdown.ToString();
            return MvcHtmlString.Create(container.ToString());
        }

        private static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (System.ComponentModel.DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(System.ComponentModel.DescriptionAttribute));
            return attribute == null ? value.ToString() : attribute.Description;
        }

        private static string GetPropertyName<TModel, TEnum>(Expression<Func<TModel, IEnumerable<TEnum>>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }
            throw new ArgumentException("Expression is not a member expression", nameof(expression));
        }
        public static string GetDisplayNameOrPropertyName<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                var property = memberExpression.Member as PropertyInfo;
                if (property != null)
                {
                    var displayAttribute = property
                        .GetCustomAttributes(typeof(DisplayAttribute), true)
                        .FirstOrDefault() as DisplayAttribute;

                    if (displayAttribute != null)
                    {
                        // Если атрибут `Display` найден, возвращаем его локализованное имя
                        return displayAttribute.GetName();
                    }

                    // Если атрибута `Display` нет, возвращаем имя свойства
                    return property.Name;
                }
            }

            throw new ArgumentException("Expression must be a member expression", nameof(expression));
        }
    }
}