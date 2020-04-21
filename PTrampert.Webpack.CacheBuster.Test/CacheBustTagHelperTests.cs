using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.FileProviders;
using Moq;
using Xunit;

namespace PTrampert.Webpack.CacheBuster.Test
{
    public class CacheBustTagHelperTests
    {
        private CacheBustTagHelper Subject { get; set; }
        private Mock<IFileProvider> Webroot { get; set; }
        private Mock<IFileInfo> FileInfo { get; set; }
        private Mock<IUrlHelper> UrlHelper { get; set; }
        private Mock<IUrlHelperFactory> UrlHelperFactory { get; set; }
        private Mock<IActionContextAccessor> ActionContext { get; set; }
        private Mock<IWebHostEnvironment> Env { get; set; }

        private Mock<ICacheBuster> CacheBuster { get; set; }

        public CacheBustTagHelperTests()
        {
            CacheBuster = new Mock<ICacheBuster>();
            CacheBuster.Setup(cb => cb.BustCache(It.IsAny<string>()))
                .Returns("busted/path");
            Subject = new CacheBustTagHelper(CacheBuster.Object);
            Subject.Resource = "/herp.js";
        }

        [Fact]
        public void WhenResourceIsNull_ItDoesNothing()
        {
            Subject.Resource = null;
            Subject.Process(null, null);
            CacheBuster.Verify(cb => cb.BustCache(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void WhenTagIsLink_ItWritesTheHrefAttribWithTheBustedPath()
        {
            var output = new TagHelperOutput("link", new TagHelperAttributeList(), (b, enc) => Task.FromResult(null as TagHelperContent));
            Subject.Process(null, output);
            Assert.Equal("busted/path", output.Attributes["href"].Value);
        }

        [Fact]
        public void WhenTagIsScript_ItWritesTheSrcAttribWithTheBustedPath()
        {
            var output = new TagHelperOutput("script", new TagHelperAttributeList(), (b, enc) => Task.FromResult(null as TagHelperContent));
            Subject.Process(null, output);
            Assert.Equal("busted/path", output.Attributes["src"].Value);
        }
    }
}
