using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using WebApp.Services;

namespace WebApp.TagHelpers;

[HtmlTargetElement("currency")]
public class CurrencyTagHelper : TagHelper
{
    private readonly CurrencyFormatterService _currencyFormatter;

    public CurrencyTagHelper(CurrencyFormatterService currencyFormatter)
    {
        _currencyFormatter = currencyFormatter;
    }

    [HtmlAttributeName("value")]
    public decimal Value { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.Content.SetHtmlContent(await _currencyFormatter.FormatCurrencyAsync(Value));
    }
} 