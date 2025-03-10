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

      // INFO: A modified version of QuillJs that allows the user to add soft breaks
      // LINK: https://github.com/slab/quill/pull/4565/files
      // This was built locally and added manually
      _manifest
          .DefineScript("quill")
          .SetUrl("~/Buzz.OrchardCore.Quilljs/quill/dist/quill.js")
          .SetVersion("2.0.2");

      _manifest
        .DefineStyle("quill-bubble")
        .SetCdn("~/Buzz.OrchardCore.Quilljs/quill/dist/quill.bubble.css")
        .SetVersion("2.0.2");

      _manifest
        .DefineStyle("quill-snow")
        .SetCdn("~/Buzz.OrchardCore.Quilljs/quill/dist/quill.snow.css")
        .SetVersion("2.0.2");
    }

    public void Configure(ResourceManagementOptions options)
    {
      options.ResourceManifests.Add(_manifest);
    }
  }
}
