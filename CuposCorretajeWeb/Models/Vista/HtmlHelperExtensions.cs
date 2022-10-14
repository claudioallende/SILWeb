using CuposCorretajeWeb.Models.AtributosValidacion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace CuposCorretajeWeb.Models.Vista
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlString DropDownMultiSelect(this HtmlHelper htmlHelper, IList<string> seleccionados, IEnumerable<SelectListItem> items, string style, object htmlAttributes)
        {
            var outerDiv = new TagBuilder("span");
            var select = SelectMultiSelect(htmlHelper, seleccionados, items, htmlAttributes);
            var html = outerDiv.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(html);
        }

        public static IHtmlString SelectMultiSelect(this HtmlHelper htmlHelper, IList<string> seleccionados, IEnumerable<SelectListItem> items, object htmlAttributes)
        {
            var outerDiv = new TagBuilder("select");
            foreach (var attribute in htmlAttributes.GetType().GetProperties().ToList()){
                outerDiv.MergeAttribute(attribute.Name, attribute.GetValue(htmlAttributes, null).ToString());
            }
            outerDiv.MergeAttribute("multiselect", "true");

            foreach (var selectListItem in items)
            {
                TagBuilder option = new TagBuilder("option");
                option.MergeAttribute("value", selectListItem.Value);
                option.InnerHtml = selectListItem.Text;

                if (seleccionados.Any(x => x == selectListItem.Value))
                {
                    option.MergeAttribute("selected", "selected");
                }
                outerDiv.InnerHtml += option.ToString();
            }

            var html = outerDiv.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(html);
        }

        public static IHtmlString BtnGroupDropDownMultiSelect(this HtmlHelper htmlHelper, IList<string> values, IList<string> texts)
        {
            var outerDiv = new TagBuilder("div");
            outerDiv.AddCssClass("btn-group show");

            var html = outerDiv.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(html);
        }

        public static IHtmlString TextBoxEditableAtributoDisabledFor<TModel, TProperty>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression,
            object htmlAttributes
        )
        {
            var attributes = new RouteValueDictionary(htmlAttributes);
            var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

            RouteValueDictionary route = new RouteValueDictionary(htmlAttributes);

            foreach (var key in attributes.Keys)
            {
                if (key.StartsWith("data_"))
                {
                    route.Remove(key);
                    route[string.Format("data-{0}", key.Substring(5))] = attributes[key];
                }
            }

            attributes = route;

            if ((attributes["disabled"] != null && attributes["disabled"] == "disabled_if_not_empty" && metaData.Model != null) || attributes["disabled"] == "disabled")
            {
                attributes["disabled"] = "disabled";
            }
            else
            {
                attributes.Remove("disabled");
            }

            //An example of getting the custom attrivbutes.
            //MemberExpression memberExpression = expression.Body as MemberExpression;
            //foreach (var attribute in memberExpression.Member.CustomAttributes)
            //{
            //    if (attribute.GetType() == typeof(DisabledIfNotEmptyAttribute) && metaData.Model != null)
            //        attributes["disabled"] = "disabled";
            //}
            return htmlHelper.TextBoxFor(expression, attributes);
        }
    }
}