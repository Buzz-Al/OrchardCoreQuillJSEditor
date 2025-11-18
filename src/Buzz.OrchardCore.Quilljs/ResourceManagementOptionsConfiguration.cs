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

      _manifest
        .DefineStyle("quill-toolbar-builder")
        .SetUrl("~/Buzz.OrchardCore.Quilljs/Styles/quill-toolbar-builder.css")
        .SetVersion("1.0.0");

      _manifest
        .DefineScript("sortablejs")
        .SetUrl("~/Buzz.OrchardCore.Quilljs/Scripts/sortable.min.js", "~/Buzz.OrchardCore.Quilljs/Scripts/sortable.js")
        .SetCdn("https://cdn.jsdelivr.net/npm/sortablejs@1.15.0/Sortable.min.js", "https://cdn.jsdelivr.net/npm/sortablejs@1.15.0/Sortable.js")
        .SetCdnIntegrity("sha384-iU5/4BKXa1kD6gGCmZT5b7TxLXzVgQPKOvQdOb0/C1sVhqZMPcKKJKNwPVbKQiPK", "sha384-iU5/4BKXa1kD6gGCmZT5b7TxLXzVgQPKOvQdOb0/C1sVhqZMPcKKJKNwPVbKQiPK")
        .SetVersion("1.15.0");

      _manifest
        .DefineScript("quill-toolbar-builder")
        .SetUrl("~/Buzz.OrchardCore.Quilljs/Scripts/quill-toolbar-builder.js")
        .SetDependencies("sortablejs")
        .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
      options.ResourceManifests.Add(_manifest);
    }
  }
}
