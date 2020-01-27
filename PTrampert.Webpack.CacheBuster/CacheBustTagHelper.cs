using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
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

#if NETCOREAPP3_0 || NETCOREAPP3_1
        public CacheBustTagHelper(IWebHostEnvironment env)
        {
            webroot = env.WebRootFileProvider;
        }
#elif NETSTANDARD2_0
        public CacheBustTagHelper(IHostingEnvironment env)
        {
            webroot = env.WebRootFileProvider;
        }
#endif

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Resource != null)
            {
                if (!Cache.ContainsKey(Resource))
                {
                    Cache.Add(Resource, new CacheBustedFile(Resource));
                }

                var cachedFile = Cache[Resource];

                if (!cachedFile.Exists(webroot))
                {
                    output.Attributes.SetAttribute(output.TagName == "script" ? "src" : "href", Resource);
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
