using System;
using System.Security.Cryptography;
using Microsoft.Extensions.FileProviders;

namespace PTrampert.Webpack.CacheBuster
{
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