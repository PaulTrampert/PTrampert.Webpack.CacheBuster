# PTrampert.Webpack.CacheBuster
Cache busts static files referenced in your cshtml files.

## Usage
1. In `Startup.ConfigureServices`, add the following:
```c#
services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
```
2. In either your `_ViewImports.cshtml` or your specific view, add the following:
```cshtml
@using PTrampert.Webpack.CacheBuster
@addTagHelper "*, PTrampert.Webpack.CacheBuster"
```

3. You can now use the `cache-bust` attribute in your script and link tags.
```html
<!-- This assumes you have a file in your webroot called `index.js` -->
<script cache-bust="~/index.js"></script>
<!-- This tag would be rendered as <script src="/web/root/path/index.js?v=[chunkhash]"></script> -->
```
