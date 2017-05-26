using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;

namespace PTrampert.Webpack.CacheBuster.Test
{
    public class WebpackResourceTagHelperTests
    {
        private WebpackResources resources;
        private WebpackResourceTagHelper subject;

        public WebpackResourceTagHelperTests()
        {
            var resources = new WebpackResources
            {
                {"herp.js", new WebpackResource("/path/to/herp.js", "somehash")},
            };
            var opts = new Mock<IOptionsSnapshot<WebpackResources>>();
            opts.Setup(o => o.Value).Returns(resources);
            
            subject = new WebpackResourceTagHelper(opts.Object);
        }

        [Fact]
        public void WhenResourceIsFoundHrefIsSet()
        {
            var output = new TagHelperOutput("link", new TagHelperAttributeList(), (b, enc) => Task.FromResult(null as TagHelperContent));
            subject.WebpackResource = "herp.js";
            subject.Process(null, output);
            Assert.Equal("/path/to/herp.js?v=somehash", output.Attributes["href"].Value);
        }

        [Fact]
        public void WhenResourceIsFoundForScriptTagSrcIsSet()
        {
            var output = new TagHelperOutput("script", new TagHelperAttributeList(), (b, enc) => Task.FromResult(null as TagHelperContent));
            subject.WebpackResource = "herp.js";
            subject.Process(null, output);
            Assert.Equal("/path/to/herp.js?v=somehash", output.Attributes["src"].Value);
        }

        [Fact]
        public void WhenResourceIsNotFoundKeyNotFoundExceptionIsThrown()
        {
            var output = new TagHelperOutput("link", new TagHelperAttributeList(), (b, enc) => Task.FromResult(null as TagHelperContent));
            subject.WebpackResource = "derp.js";
            Assert.Throws<KeyNotFoundException>(() => subject.Process(null, output));
        }
    }
}
