using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

public static class CustomInputHelpers
{
    public static MvcHtmlString TextBoxFilterFor<TModel, TProperty>(
        this HtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TProperty>> expression,
        object htmlAttributes = null,
        bool withLabel = true,
        string label=null)
    {
        var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
        var name = ExpressionHelper.GetExpressionText(expression);
        var fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
        var id = TagBuilder.CreateSanitizedId(fullName);

        // Атрибуты
        var attributes = htmlAttributes != null
            ? HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes)
            : new RouteValueDictionary();

        // Добавляем id и name, если не указаны
        if (!attributes.ContainsKey("id"))
            attributes["id"] = id;
        if (!attributes.ContainsKey("name"))
            attributes["name"] = fullName;

        // Значение
        var value = metadata.Model == null ? "" : metadata.Model.ToString();

        // Label
        var labelText =
            !string.IsNullOrWhiteSpace(label)
                ? label
                : metadata.DisplayName ?? metadata.PropertyName ?? name;

        var labelHtml = withLabel ? $"<label for='{id}'>{labelText}</label>" : "";

        // Атрибуты как строка
        var attrString = string.Join(" ", attributes.Select(a => $"{a.Key}=\"{a.Value}\""));

        // Генерация HTML
        var html = $@"
<div class='form-group textbox-wrapper'>
    {labelHtml}
    <input type='text' value='{HttpUtility.HtmlEncode(value)}' {attrString} />
</div>";

        return MvcHtmlString.Create(html);
    }
    public static MvcHtmlString CustomCheckboxFor<TModel>(
        this HtmlHelper<TModel> html,
        Expression<Func<TModel, bool?>> expression,
        string spanClass = "",
        bool withLabel = false,
        bool autoLoad = false,
        string functionLoad = "LoadData()")
    {
        var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
        var propertyName = ExpressionHelper.GetExpressionText(expression);
        var fullName = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(propertyName);

        string displayName = metadata.DisplayName ?? propertyName;

        bool? modelValue = metadata.Model as bool?;
        bool isChecked = modelValue == true;

        var sb = new StringBuilder();
        sb.AppendLine("<div class=\"form-group\">");

        sb.AppendLine($"<label class='custom-checkbox' for='{propertyName}' style='cursor: pointer;'>");
        if (withLabel)
        {
            sb.AppendLine(html.Encode(displayName));
        }
        sb.AppendLine($"<input type='checkbox' id='{propertyName}' name='{fullName}' value='true' {(isChecked ? "checked" : "")} {(autoLoad ? $"onchange=\"{functionLoad}\"" : "")} />");

        sb.AppendLine($"<span{(string.IsNullOrEmpty(spanClass) ? "" : $" class='{spanClass}'")}></span>");
        sb.AppendLine("</label>");

        sb.AppendLine("</div>");

        return MvcHtmlString.Create(sb.ToString());
    }
    public static MvcHtmlString DisplayCustomCheckboxFor<TModel>(
    this HtmlHelper<TModel> html,
    Expression<Func<TModel, bool?>> expression,
    string spanClass = "",
    bool withLabel = false)
    {
        var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
        var propertyName = ExpressionHelper.GetExpressionText(expression);
        var fullName = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(propertyName);

        string displayName = metadata.DisplayName ?? propertyName;

        bool? modelValue = metadata.Model as bool?;
        bool isChecked = modelValue == true;

        var sb = new StringBuilder();
        sb.AppendLine("<div class=\"form-group\">");
        sb.AppendLine($"<label class='custom-checkbox' for='{propertyName}'>");
        if (withLabel)
        {
            sb.AppendLine(html.Encode(displayName));
        }
        sb.AppendLine($"<input type='checkbox' id='{propertyName}' name='{fullName}' value='true' {(isChecked ? "checked" : "")} disabled />");
        sb.AppendLine($"<span{(string.IsNullOrEmpty(spanClass) ? "" : $" class='{spanClass}'")}></span>");
        sb.AppendLine("</label>");
        sb.AppendLine("</div>");

        return MvcHtmlString.Create(sb.ToString());
    }

    public static MvcHtmlString DisplayCustomCheckboxFor<TModel>(
        this HtmlHelper<TModel> html,
        Expression<Func<TModel, bool>> expression,
        string spanClass = "",
        bool withLabel = false)
    {
        var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
        var propertyName = ExpressionHelper.GetExpressionText(expression);
        var fullName = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(propertyName);

        string displayName = metadata.DisplayName ?? propertyName;

        bool isChecked = metadata.Model is bool modelValue && modelValue;

        var sb = new StringBuilder();
        sb.AppendLine("<div class=\"form-group\">");
        sb.AppendLine($"<label class='custom-checkbox' for='{propertyName}'>");
        if (withLabel)
        {
            sb.AppendLine(html.Encode(displayName));
        }
        sb.AppendLine($"<input type='checkbox' id='{propertyName}' name='{fullName}' value='true' {(isChecked ? "checked" : "")} disabled />");
        sb.AppendLine($"<span{(string.IsNullOrEmpty(spanClass) ? "" : $" class='{spanClass}'")}></span>");
        sb.AppendLine("</label>");
        sb.AppendLine("</div>");

        return MvcHtmlString.Create(sb.ToString());
    }

    public static MvcHtmlString CustomCheckboxFor<TModel>(
        this HtmlHelper<TModel> html,
        Expression<Func<TModel, bool>> expression,
        string spanClass = "",
        bool withLabel = false,
        bool autoLoad = false,
        string functionLoad = "LoadData()")
    {
        var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
        var propertyName = ExpressionHelper.GetExpressionText(expression);
        var fullName = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(propertyName);

        var displayAttr = metadata.ContainerType?
            .GetProperty(propertyName)?
            .GetCustomAttribute<DisplayAttribute>();

        string displayName = metadata.DisplayName ?? propertyName;

        bool isChecked = false;
        if (metadata.Model is bool modelValue)
            isChecked = modelValue;

        var sb = new StringBuilder();
        sb.AppendLine($"<div class=\"form-group\">");
        sb.AppendLine($"<label class='custom-checkbox' for='{propertyName}' style='cursor: pointer;'>");
        if (withLabel)
        {
            sb.AppendLine(html.Encode(displayName));
        }
        sb.AppendLine($"<input type='checkbox' id='{propertyName}' name='{fullName}' value='true' {(isChecked ? "checked" : "")} {(autoLoad ? $"onchange=\"{functionLoad}\"" : "")} />");
        sb.AppendLine($"<span{(string.IsNullOrEmpty(spanClass) ? "" : $" class='{spanClass}'")}></span>");
        sb.AppendLine("</label>");
        sb.AppendLine($"</div>");

        return MvcHtmlString.Create(sb.ToString());
    }

    public static MvcHtmlString MultiSelectCheckboxFor<TModel, TProperty>(
    this HtmlHelper<TModel> htmlHelper,
    Expression<Func<TModel, TProperty>> expression,
    IEnumerable<SelectListItem> selectList,
    object htmlAttributes = null,
    bool withLabel = true)
    {
        var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
        var name = ExpressionHelper.GetExpressionText(expression);
        var fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
        var id = TagBuilder.CreateSanitizedId(fullName);

        // Атрибуты
        var attributes = htmlAttributes != null
            ? HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes)
            : new RouteValueDictionary();

        var attrString = string.Join(" ", attributes.Select(a => $"{a.Key}=\"{a.Value}\""));

        // Значения
        var selectedValues = new List<string>();
        if (metadata.Model != null)
        {
            if (metadata.Model is IEnumerable<string> stringList)
                selectedValues = stringList.ToList();
            else if (metadata.Model is IEnumerable<int> intList)
                selectedValues = intList.Select(x => x.ToString()).ToList();
            else if (metadata.Model is IEnumerable enumList)
                selectedValues = enumList.Cast<object>()
                   .Select(x => x.ToString())
                   .ToList();
        }

        // Опции
        var options = selectList.Select(item => new
        {
            Text = item.Text,
            Value = item.Value,
            Selected = selectedValues.Contains(item.Value) || item.Selected
        }).ToList();

        var selectedJson = System.Web.Helpers.Json.Encode(selectedValues);
        var optionsJson = System.Web.Helpers.Json.Encode(options.Select(o => new { text = o.Text, value = o.Value }).ToList());

        // Label
        var labelText = metadata.DisplayName ?? metadata.PropertyName ?? name;
        var labelHtml = withLabel ? $"<label for='{id}'>{labelText}</label>" : "";

        var html = $@"
<div class='form-group multiselect-wrapper' {attrString} data-name='{fullName}' data-id='{id}'>
    {labelHtml}
    <div class='multiselect-container'>
        <div class='select-header' id='selectHeader_{id}'>
            <span class='selected-text' id='selectedText_{id}'>Select options...</span>
        </div>
        
        <div class='dropdown' id='dropdown_{id}'>
            <div class='search-box'>
                <input type='text' id='searchInput_{id}' placeholder='Search...' autocomplete='off'>
            </div>
            <div class='options-list' id='optionsList_{id}'></div>
        </div>
    </div>
    <div id='hiddenInputs_{id}'></div>
</div>

<script>
    if (typeof initMultiSelect === 'function') {{
        initMultiSelect('{id}', '{fullName}', {optionsJson}, {selectedJson});
    }}
</script>";

        return MvcHtmlString.Create(html);
    }

    public static MvcHtmlString SingleSelectFor<TModel, TProperty>(
        this HtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TProperty>> expression,
        IEnumerable<SelectListItem> selectList,
        string placeholderText = "Select option...",
        object htmlAttributes = null,
        bool withLabel = false,
        bool allowNull = false)
    {
        var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
        var name = ExpressionHelper.GetExpressionText(expression);
        var fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
        var id = TagBuilder.CreateSanitizedId(fullName);

        // Атрибуты
        var attributes = htmlAttributes != null
            ? HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes)
            : new RouteValueDictionary();

        var attrString = string.Join(" ", attributes.Select(a => $"{a.Key}=\"{a.Value}\""));

        // Выбранное значение
        object model = metadata.Model;
        string selectedValue;

        var modelType = Nullable.GetUnderlyingType(typeof(TProperty)) ?? typeof(TProperty);

        if (modelType.IsEnum && model != null)
        {
            selectedValue = Convert.ToInt32(model).ToString();
        }
        else
        {
            selectedValue = model?.ToString() ?? "";
        }

        // Опции
        var options = selectList.Select(item => new
        {
            Text = item.Text,
            Value = item.Value,
            Selected = item.Value == selectedValue || item.Selected
        }).ToList();
        if (allowNull)
        {
            options.Insert(0, new
            {
                Text = "None",
                Value = "",
                Selected = string.IsNullOrEmpty(selectedValue)
            });
        }
        var selectedJson = System.Web.Helpers.Json.Encode(selectedValue);
        var optionsJson = System.Web.Helpers.Json.Encode(options.Select(o => new { text = o.Text, value = o.Value }).ToList());
        var placeholderJson = System.Web.Helpers.Json.Encode(placeholderText);

        // Label
        var labelText = metadata.DisplayName ?? metadata.PropertyName ?? name;
        var labelHtml = withLabel ? $"<label for='{id}'>{labelText}</label>" : "";

        var html = $@"
<div class='form-group singleselect-wrapper' {attrString} data-name='{fullName}' data-id='{id}'>
    {labelHtml}
    <div class='singleselect-container'>
        <div class='select-header' id='singleSelectHeader_{id}'>
            <span class='selected-text' id='singleSelectedText_{id}'>{placeholderText}</span>
        </div>
        
        <div class='dropdown' id='singleDropdown_{id}'>
            <div class='search-box'>
                <input type='text' id='singleSearchInput_{id}' placeholder='Search...' autocomplete='off'>
            </div>
            <div class='options-list' id='singleOptionsList_{id}'></div>
        </div>
    </div>
    <input type='hidden' id='singleHiddenInput_{id}' name='{fullName}' value='{selectedValue}' />
</div>

<script>
    if (typeof initSingleSelect === 'function') {{
        initSingleSelect('{id}', '{fullName}', {optionsJson}, {selectedJson}, {placeholderJson});
    }}
    
    document.getElementById('singleHiddenInput_{id}').addEventListener('singleSelectChanged', function(e) {{
        if (typeof handleSingleSelectChange === 'function') {{
            handleSingleSelectChange(e.detail);
        }}
    }});
</script>";

        return MvcHtmlString.Create(html);
    }

    public static MvcHtmlString SingleSelectFor<TModel, TEnum>(
        this HtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TEnum>> expression,
        string placeholderText = "Select option...",
        object htmlAttributes = null,
        bool withLabel = false,
        bool allowNull = false)
    {
        var enumType = Nullable.GetUnderlyingType(typeof(TEnum)) ?? typeof(TEnum);

        if (!enumType.IsEnum)
            throw new ArgumentException("TEnum must be an enum type.");

        var values = Enum.GetValues(enumType).Cast<object>();

        var selectList = values.Select(v => new SelectListItem
        {
            Text = ((Enum)v).GetDisplayName(),
            Value = Convert.ToInt32(v).ToString()
        });

        return SingleSelectFor(htmlHelper, expression, selectList, placeholderText, htmlAttributes, withLabel, allowNull);
    }

    public static MvcHtmlString MultiSelectCheckboxFor<TModel, TEnum>(
        this HtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, IEnumerable<TEnum>>> expression,
        object htmlAttributes = null,
        bool withLabel = true,
        bool allowNull = false)
    {
        var enumType = Nullable.GetUnderlyingType(typeof(TEnum)) ?? typeof(TEnum);

        if (!enumType.IsEnum)
            throw new ArgumentException("TEnum must be an enum type.");

        var values = Enum.GetValues(enumType).Cast<object>();

        var selectList = values.Select(v => new SelectListItem
        {
            Text = ((Enum)v).GetDisplayName(),
            Value = Convert.ToInt32(v).ToString()
        });

        return MultiSelectCheckboxFor<TModel, IEnumerable<TEnum>>(
            htmlHelper, expression, selectList, htmlAttributes, withLabel);
    }
}
