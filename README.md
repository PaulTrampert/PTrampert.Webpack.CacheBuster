# PTrampert.Webpack.CacheBuster
Nuget package and webpack plugin to help with cache busting webpacked resources in your Razor views. 
Designed to work along with the webpack plugin, [`webpack-resources-plugin`](https://www.npmjs.com/package/webpack-resources-plugin).

## Usage
1. Setup the `webpack-resources-plugin`.
2. In your `Startup.cs`, make sure your configuration references the json file that plugin generates.
```c#
var config = new ConfigurationBuilder()
  .AddJsonFile("WebpackResources.json")
  .Build();
```
3. In `Startup.ConfigureServices`, add the following:
```c#
services.Configure<WebpackResources>(config.GetSection("webpackResources"));
```
4. In either your `_ViewImports.cshtml` or your specific view, add the following:
```cshtml
@using PTrampert.Webpack.CacheBuster
@addTagHelper "*, PTrampert.Webpack.CacheBuster"
```

5. You can now use the `webpack-resource` attribute in your script and link tags.
```html
<!-- This assumes your webpack outputs a file called "index.js" -->
<script webpack-resource="index.js"></script>
<!-- Assuming your webpack publicPath is '/dist/', This tag would be rendered as <script src="/dist/index.js?v=[chunkhash]"></script> -->
```
