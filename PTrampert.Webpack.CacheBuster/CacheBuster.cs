﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace PTrampert.Webpack.CacheBuster
{
    public class CacheBuster : ICacheBuster
    {
        private readonly IDictionary<string, CacheBustedFile> cache = new ConcurrentDictionary<string, CacheBustedFile>();

        private readonly IWebHostEnvironment env;
        private readonly IUrlHelperFactory urlHelperFactory;
        private readonly IActionContextAccessor actionContextAccessor;

        public CacheBuster(IWebHostEnvironment env, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            this.env = env;
            this.urlHelperFactory = urlHelperFactory;
            this.actionContextAccessor = actionContextAccessor;
        }

        public string BustCache(string url)
        {
            var webroot = env.WebRootFileProvider;
            var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            var absoluteUrl = urlHelper.Content(url);
            if (!cache.ContainsKey(absoluteUrl))
            {
                cache.Add(absoluteUrl, new CacheBustedFile(absoluteUrl));
            }

            var cachedFile = cache[absoluteUrl];

            if (!cachedFile.Exists(webroot))
            {
                return absoluteUrl;
            }

            cachedFile.LastChecked = DateTimeOffset.Now;

            return $"{cachedFile.WebPath}?v={cachedFile.GetHash(webroot)}";
        }
    }
}
