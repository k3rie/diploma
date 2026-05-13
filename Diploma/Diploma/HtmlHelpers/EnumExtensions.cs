using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        if (enumValue == null)
            return string.Empty;
        return enumValue
            .GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()?
            .GetName() ?? enumValue.ToString();
    }
    public static MvcHtmlString DisplayEnumFor<TModel, TEnum>(
         this HtmlHelper<TModel> htmlHelper,
         Expression<Func<TModel, TEnum>> expression
     ) where TEnum : Enum
    {
        if (expression == null)
            return MvcHtmlString.Empty;


        var func = expression.Compile();
        var enumValue = func(htmlHelper.ViewData.Model);

        if (enumValue == null)
            return MvcHtmlString.Empty;

        // Берём DisplayAttribute
        var member = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();
        if (member == null)
            return new MvcHtmlString(enumValue.ToString());

        var displayAttr = member.GetCustomAttribute<DisplayAttribute>();
        var name = displayAttr?.GetName() ?? enumValue.ToString();

        return new MvcHtmlString(name);
    }
}