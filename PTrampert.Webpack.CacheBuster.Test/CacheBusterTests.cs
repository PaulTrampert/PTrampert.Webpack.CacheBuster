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
    public class CacheBusterTests
    {
        private CacheBuster Subject { get; set; }
        private Mock<IFileProvider> Webroot { get; set; }
        private Mock<IFileInfo> FileInfo { get; set; }
        private Mock<IUrlHelper> UrlHelper { get; set; }
        private Mock<IUrlHelperFactory> UrlHelperFactory { get; set; }
        private Mock<IActionContextAccessor> ActionContext { get; set; }
        private Mock<IWebHostEnvironment> Env { get; set; }

        public CacheBusterTests()
        {
            UrlHelper = new Mock<IUrlHelper>();
            UrlHelper.Setup(url => url.Content(It.IsAny<string>()))
                .Returns("/resolved/content");
            UrlHelperFactory = new Mock<IUrlHelperFactory>();
            UrlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(UrlHelper.Object);
            ActionContext = new Mock<IActionContextAccessor>();
            ActionContext.SetupGet(ac => ac.ActionContext)
                .Returns(new ActionContext());
            Webroot = new Mock<IFileProvider>();
            Env = new Mock<IWebHostEnvironment>();
            Env.SetupGet(e => e.WebRootFileProvider)
                .Returns(Webroot.Object);
            FileInfo = new Mock<IFileInfo>();
            Webroot.Setup(r => r.GetFileInfo(It.IsAny<string>()))
                .Returns(FileInfo.Object);

            Subject = new CacheBuster(Env.Object, UrlHelperFactory.Object, ActionContext.Object);
        }

        [Fact]
        public void WhenFileDoesntExistForLink_ItWritesTheHrefAttrib()
        {
            FileInfo.SetupGet(fi => fi.Exists).Returns(false);
            var result = Subject.BustCache("herp.js");
            Assert.Equal("/resolved/content", result);
        }

        [Fact]
        public void WhenFileDoesntExistForScript_ItWritesTheSrcAttrib()
        {
            FileInfo.SetupGet(fi => fi.Exists).Returns(false);
            var result = Subject.BustCache("herp.js");
            Assert.Equal("/resolved/content", result);
        }

        [Fact]
        public void WhenFileExistsForScript_ItWritesTheSrcAttribWithHash()
        {
            FileInfo.SetupGet(fi => fi.Exists).Returns(true);
            FileInfo.Setup(f => f.CreateReadStream())
                .Returns(new MemoryStream(new byte[] {1, 2}));
            var result = Subject.BustCache("herp.js");
            Assert.Equal("/resolved/content?v=oShx_uIQ-4YZKR6uoZRYHL0lMeSyN1nSJfaAaSP2MiI=", result);
        }

        [Fact]
        public void WhenFileExistsForLink_ItWritesTheHrefAttribWithHash()
        {
            FileInfo.SetupGet(fi => fi.Exists).Returns(true);
            FileInfo.Setup(f => f.CreateReadStream())
                .Returns(new MemoryStream(new byte[] {1, 2}));
            var result = Subject.BustCache("herp.js");
            Assert.Equal("/resolved/content?v=oShx_uIQ-4YZKR6uoZRYHL0lMeSyN1nSJfaAaSP2MiI=", result);
        }
    }
}
