namespace PTrampert.Webpack.CacheBuster
{
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