using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
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

#if NETCOREAPP3_0 || NETCOREAPP3_1
        public CacheBustTagHelper(IWebHostEnvironment env, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContext)
        {
            webroot = env.WebRootFileProvider;
            this.urlHelper = urlHelperFactory.GetUrlHelper(actionContext.ActionContext);
        }
#elif NETSTANDARD2_0
        public CacheBustTagHelper(IHostingEnvironment env, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContext)
        {
            webroot = env.WebRootFileProvider;
            this.urlHelper = urlHelperFactory.GetUrlHelper(actionContext.ActionContext);
        }
#endif

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

    public class CacheBustedFile
    {
        private string hash;
        public string WebPath { get; set; }
        public DateTimeOffset? LastChecked { get; set; }

        public CacheBustedFile(string webPath)
        {
            WebPath = webPath;
        }

        public DateTimeOffset? GetLastModified(IFileProvider webroot)
        {
            return webroot.GetFileInfo(WebPath)?.LastModified;
        }

        public bool Exists(IFileProvider webroot)
        {
            return webroot.GetFileInfo(WebPath)?.Exists ?? false;
        }

        public bool IsModified(IFileProvider webroot)
        {
            var lastModified = GetLastModified(webroot);
            return string.IsNullOrWhiteSpace(hash) 
                   || !lastModified.HasValue 
                   || !LastChecked.HasValue 
                   || lastModified.Value > LastChecked.Value;
        }

        public string GetHash(IFileProvider webroot)
        {
            if (IsModified(webroot))
            {
                var fileInfo = webroot.GetFileInfo(WebPath);
                using (var sha265 = SHA256.Create())
                using (var stream = fileInfo.CreateReadStream())
                {
                    var rawHash = sha265.ComputeHash(stream);
                    hash = Convert.ToBase64String(rawHash)
                        .Replace('+', '-')
                        .Replace('/', '_');
                }
            }

            return hash;
        }
    }
}
