using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace PTrampert.Webpack.CacheBuster
{
    [HtmlTargetElement("script")]
    [HtmlTargetElement("link")]
    public class WebpackResourceTagHelper : TagHelper
    {
        [HtmlAttributeName("webpack-resource")]
        public string WebpackResource { get; set; }

        private readonly WebpackResources resources;

        public WebpackResourceTagHelper(IOptionsSnapshot<WebpackResources> resources)
        {
            this.resources = resources.Value;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var resource = resources[WebpackResource];
            output.Attributes.SetAttribute(output.TagName == "script" ? "src" : "href", $"{resource.FileName}?v={resource.Hash}");
        }
    }
}
