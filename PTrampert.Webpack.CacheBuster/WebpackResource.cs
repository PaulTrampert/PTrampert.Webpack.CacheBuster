using System;

namespace PTrampert.Webpack.CacheBuster
{
    [Obsolete("No longer needed when using cache-bust tag helper.")]
    public class WebpackResource
    {
        public string FileName { get; set; }
        public string Hash { get; set; }

        public WebpackResource()
        {
        }

        public WebpackResource(string fileName, string hash)
        {
            FileName = fileName;
            Hash = hash;
        }
    }
}