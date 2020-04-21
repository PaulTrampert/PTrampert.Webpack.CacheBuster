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
#if NETCOREAPP3_0 || NETCOREAPP3_1
        private Mock<IWebHostEnvironment> Env { get; set; }
#elif NETCOREAPP2_0
        private Mock<IHostingEnvironment> Env { get; set; }
#endif

        public CacheBustTagHelperTests()
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
#if NETCOREAPP3_0 || NETCOREAPP3_1
            Env = new Mock<IWebHostEnvironment>();
            Env.SetupGet(e => e.WebRootFileProvider)
                .Returns(Webroot.Object);
#elif NETCOREAPP2_0
            Env = new Mock<IHostingEnvironment>();
            Env.SetupGet(e => e.WebRootFileProvider)
                .Returns(Webroot.Object);
#endif
            FileInfo = new Mock<IFileInfo>();
            Webroot.Setup(r => r.GetFileInfo(It.IsAny<string>()))
                .Returns(FileInfo.Object);
            Subject = new CacheBustTagHelper(Env.Object, UrlHelperFactory.Object, ActionContext.Object);
            Subject.Resource = "/herp.js";
        }

        [Fact]
        public void WhenFileDoesntExistForLink_ItWritesTheHrefAttrib()
        {
            FileInfo.SetupGet(fi => fi.Exists).Returns(false);
            var output = new TagHelperOutput("link", new TagHelperAttributeList(), (b, enc) => Task.FromResult(null as TagHelperContent));
            Subject.Process(null, output);
            Assert.Equal("/resolved/content", output.Attributes["href"].Value);
        }

        [Fact]
        public void WhenFileDoesntExistForScript_ItWritesTheSrcAttrib()
        {
            FileInfo.SetupGet(fi => fi.Exists).Returns(false);
            var output = new TagHelperOutput("script", new TagHelperAttributeList(), (b, enc) => Task.FromResult(null as TagHelperContent));
            Subject.Process(null, output);
            Assert.Equal("/resolved/content", output.Attributes["src"].Value);
        }

        [Fact]
        public void WhenFileExistsForScript_ItWritesTheSrcAttribWithHash()
        {
            FileInfo.SetupGet(fi => fi.Exists).Returns(true);
            FileInfo.Setup(f => f.CreateReadStream())
                .Returns(new MemoryStream(new byte[] {1, 2}));
            var output = new TagHelperOutput("script", new TagHelperAttributeList(), (b, enc) => Task.FromResult(null as TagHelperContent));
            Subject.Process(null, output);
            Assert.Equal("/resolved/content?v=oShx_uIQ-4YZKR6uoZRYHL0lMeSyN1nSJfaAaSP2MiI=", output.Attributes["src"].Value);
        }

        [Fact]
        public void WhenFileExistsForLink_ItWritesTheHrefAttribWithHash()
        {
            FileInfo.SetupGet(fi => fi.Exists).Returns(true);
            FileInfo.Setup(f => f.CreateReadStream())
                .Returns(new MemoryStream(new byte[] { 1, 2 }));
            var output = new TagHelperOutput("link", new TagHelperAttributeList(), (b, enc) => Task.FromResult(null as TagHelperContent));
            Subject.Process(null, output);
            Assert.Equal("/resolved/content?v=oShx_uIQ-4YZKR6uoZRYHL0lMeSyN1nSJfaAaSP2MiI=", output.Attributes["href"].Value);
        }
    }
}
