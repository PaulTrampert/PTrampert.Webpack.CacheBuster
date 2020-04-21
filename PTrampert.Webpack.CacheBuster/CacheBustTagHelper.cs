using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.FileProviders;

namespace PTrampert.Webpack.CacheBuster
{
    [HtmlTargetElement("script", Attributes = "cache-bust")]
    [HtmlTargetElement("link", Attributes = "cache-bust")]
    public class CacheBustTagHelper : TagHelper
    {
        private readonly ICacheBuster cacheBuster;

        [HtmlAttributeName("cache-bust")]
        public string Resource { get; set; }

        public CacheBustTagHelper(ICacheBuster cacheBuster)
        {
            this.cacheBuster = cacheBuster;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Resource != null)
            {
                var cacheBustedResource = cacheBuster.BustCache(Resource);

                output.Attributes.SetAttribute(output.TagName == "script" ? "src" : "href", cacheBustedResource);
            }
        }
    }
}
