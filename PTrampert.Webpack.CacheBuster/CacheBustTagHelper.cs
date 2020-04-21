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
        private static readonly IDictionary<string, CacheBustedFile> Cache = new ConcurrentDictionary<string, CacheBustedFile>();

        [HtmlAttributeName("cache-bust")]
        public string Resource { get; set; }

        private readonly IFileProvider webroot;

        private readonly IUrlHelper urlHelper;

        public CacheBustTagHelper(IWebHostEnvironment env, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContext)
        {
            webroot = env.WebRootFileProvider;
            this.urlHelper = urlHelperFactory.GetUrlHelper(actionContext.ActionContext);
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Resource != null)
            {
                var absoluteUrl = urlHelper.Content(Resource);
                if (!Cache.ContainsKey(absoluteUrl))
                {
                    Cache.Add(absoluteUrl, new CacheBustedFile(absoluteUrl));
                }

                var cachedFile = Cache[absoluteUrl];

                if (!cachedFile.Exists(webroot))
                {
                    output.Attributes.SetAttribute(output.TagName == "script" ? "src" : "href", absoluteUrl);
                    return;
                }

                cachedFile.LastChecked = DateTimeOffset.Now;

                output.Attributes.SetAttribute(output.TagName == "script" ? "src" : "href",
                    $"{cachedFile.WebPath}?v={cachedFile.GetHash(webroot)}");
            }
        }
    }
}
