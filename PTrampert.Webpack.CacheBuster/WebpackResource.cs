namespace PTrampert.Webpack.CacheBuster
{
    public class WebpackResource
    {
        public string PublicPath { get; set; }
        public string Hash { get; set; }

        public WebpackResource()
        {
        }

        public WebpackResource(string publicPath, string hash)
        {
            PublicPath = publicPath;
            Hash = hash;
        }
    }
}