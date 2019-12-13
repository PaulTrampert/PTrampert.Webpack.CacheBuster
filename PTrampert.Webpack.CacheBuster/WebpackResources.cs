using System;
using System.Collections.Generic;

namespace PTrampert.Webpack.CacheBuster
{
    [Obsolete("No longer needed when using cache-bust tag helper.")]
    public class WebpackResources : Dictionary<string, WebpackResource>
    {
    }
}
