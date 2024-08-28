using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace Buzz.OrchardCore.Quilljs
{
  public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
  {
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
      _manifest = new ResourceManifest();

      _manifest
          .DefineScript("quill")
          .SetCdn("https://cdn.jsdelivr.net/npm/quill@2.0.2/dist/quill.js")
          .SetVersion("2.0.2");

      _manifest
        .DefineStyle("quill-bubble")
        .SetCdn("https://cdn.jsdelivr.net/npm/quill@2.0.2/dist/quill.bubble.css")
        .SetVersion("2.0.2");

      _manifest
        .DefineStyle("quill-snow")
        .SetCdn("https://cdn.jsdelivr.net/npm/quill@2.0.2/dist/quill.snow.min.css")
        .SetVersion("2.0.2");
    }

    public void Configure(ResourceManagementOptions options)
    {
      options.ResourceManifests.Add(_manifest);
    }
  }
}
