using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Buzz.OrchardCore.Quilljs.Settings;

/// <summary>
/// Strongly-typed configuration for Quill.js toolbar options.
/// Replaces raw JSON strings with type-safe properties.
/// </summary>
public class QuillToolbarConfig
{
    /// <summary>
    /// Formatting buttons (bold, italic, underline, strike, code)
    /// </summary>
    public FormattingButtons Formatting { get; set; } = FormattingButtons.None;

    /// <summary>
    /// Block-level formatting buttons (blockquote, code-block, headers)
    /// </summary>
    public BlockButtons Blocks { get; set; } = BlockButtons.None;

    /// <summary>
    /// List formatting buttons (ordered, bullet, check)
    /// </summary>
    public ListButtons Lists { get; set; } = ListButtons.None;

    /// <summary>
    /// Media insertion buttons (link, image, video, formula)
    /// </summary>
    public MediaButtons Media { get; set; } = MediaButtons.None;

    /// <summary>
    /// Text styling buttons (color, background, font, size, align)
    /// </summary>
    public StyleButtons Styles { get; set; } = StyleButtons.None;

    /// <summary>
    /// Advanced formatting buttons (script, indent, direction, clean)
    /// </summary>
    public AdvancedButtons Advanced { get; set; } = AdvancedButtons.None;

    /// <summary>
    /// Custom color palette for color/background pickers.
    /// Use hex color codes (e.g., "#84BD00").
    /// Empty list means use Quill theme defaults.
    /// </summary>
    public List<string> CustomColors { get; set; } = new List<string>();

    /// <summary>
    /// Generates Quill-compatible toolbar configuration JSON.
    /// </summary>
    /// <returns>JSON array of toolbar button groups</returns>
    public string GenerateQuillJson()
    {
        var toolbarGroups = new List<object>();

        // Formatting group
        var formattingGroup = new List<string>();
        if (Formatting.HasFlag(FormattingButtons.Bold)) formattingGroup.Add("bold");
        if (Formatting.HasFlag(FormattingButtons.Italic)) formattingGroup.Add("italic");
        if (Formatting.HasFlag(FormattingButtons.Underline)) formattingGroup.Add("underline");
        if (Formatting.HasFlag(FormattingButtons.Strike)) formattingGroup.Add("strike");
        if (Formatting.HasFlag(FormattingButtons.Code)) formattingGroup.Add("code");
        if (formattingGroup.Any()) toolbarGroups.Add(formattingGroup);

        // Block group
        var blockGroup = new List<object>();
        if (Blocks.HasFlag(BlockButtons.Blockquote)) blockGroup.Add("blockquote");
        if (Blocks.HasFlag(BlockButtons.CodeBlock)) blockGroup.Add("code-block");
        if (blockGroup.Any()) toolbarGroups.Add(blockGroup);

        // Headers group (if any header selected)
        var headerGroup = new List<object>();
        if (Blocks.HasFlag(BlockButtons.Header1)) headerGroup.Add(new { header = 1 });
        if (Blocks.HasFlag(BlockButtons.Header2)) headerGroup.Add(new { header = 2 });
        if (headerGroup.Any()) toolbarGroups.Add(headerGroup);

        // Media group
        var mediaGroup = new List<string>();
        if (Media.HasFlag(MediaButtons.Link)) mediaGroup.Add("link");
        if (Media.HasFlag(MediaButtons.Image)) mediaGroup.Add("image");
        if (Media.HasFlag(MediaButtons.Video)) mediaGroup.Add("video");
        if (Media.HasFlag(MediaButtons.Formula)) mediaGroup.Add("formula");
        if (mediaGroup.Any()) toolbarGroups.Add(mediaGroup);

        // Lists group
        var listGroup = new List<object>();
        if (Lists.HasFlag(ListButtons.Ordered)) listGroup.Add(new { list = "ordered" });
        if (Lists.HasFlag(ListButtons.Bullet)) listGroup.Add(new { list = "bullet" });
        if (Lists.HasFlag(ListButtons.Check)) listGroup.Add(new { list = "check" });
        if (listGroup.Any()) toolbarGroups.Add(listGroup);

        // Style group - Colors
        var colorGroup = new List<object>();
        if (Styles.HasFlag(StyleButtons.Color))
        {
            colorGroup.Add(new { color = CustomColors.Any() ? CustomColors.ToArray() : Array.Empty<string>() });
        }
        if (Styles.HasFlag(StyleButtons.Background))
        {
            colorGroup.Add(new { background = CustomColors.Any() ? CustomColors.ToArray() : Array.Empty<string>() });
        }
        if (colorGroup.Any()) toolbarGroups.Add(colorGroup);

        // Style group - Font and Size
        var fontGroup = new List<object>();
        if (Styles.HasFlag(StyleButtons.Font)) fontGroup.Add(new { font = Array.Empty<string>() });
        if (Styles.HasFlag(StyleButtons.Size))
        {
            fontGroup.Add(new { size = new[] { "small", false, "large", "huge" } });
        }
        if (fontGroup.Any()) toolbarGroups.Add(fontGroup);

        // Style group - Alignment
        if (Styles.HasFlag(StyleButtons.Align))
        {
            toolbarGroups.Add(new List<object> { new { align = Array.Empty<string>() } });
        }

        // Advanced group - Script
        if (Advanced.HasFlag(AdvancedButtons.Script))
        {
            toolbarGroups.Add(new List<object>
            {
                new { script = "sub" },
                new { script = "super" }
            });
        }

        // Advanced group - Indent
        if (Advanced.HasFlag(AdvancedButtons.Indent))
        {
            toolbarGroups.Add(new List<object>
            {
                new { indent = "-1" },
                new { indent = "+1" }
            });
        }

        // Advanced group - Direction
        if (Advanced.HasFlag(AdvancedButtons.Direction))
        {
            toolbarGroups.Add(new List<object> { new { direction = "rtl" } });
        }

        // Clean button (always in its own group)
        if (Advanced.HasFlag(AdvancedButtons.Clean))
        {
            toolbarGroups.Add(new List<string> { "clean" });
        }

        // Serialize to JSON
        return JsonSerializer.Serialize(toolbarGroups, new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }

    /// <summary>
    /// Creates a default "Standard" toolbar configuration
    /// </summary>
    public static QuillToolbarConfig CreateStandard()
    {
        return new QuillToolbarConfig
        {
            Formatting = FormattingButtons.Bold | FormattingButtons.Italic,
            Blocks = BlockButtons.Blockquote | BlockButtons.Header1 | BlockButtons.Header2,
            Lists = ListButtons.Ordered | ListButtons.Bullet,
            Media = MediaButtons.Link,
            Advanced = AdvancedButtons.Clean
        };
    }

    /// <summary>
    /// Creates a minimal toolbar configuration
    /// </summary>
    public static QuillToolbarConfig CreateMinimal()
    {
        return new QuillToolbarConfig
        {
            Formatting = FormattingButtons.Bold | FormattingButtons.Italic,
            Advanced = AdvancedButtons.Clean
        };
    }

    /// <summary>
    /// Creates a full-featured toolbar configuration
    /// </summary>
    public static QuillToolbarConfig CreateFull()
    {
        return new QuillToolbarConfig
        {
            Formatting = FormattingButtons.Bold | FormattingButtons.Italic |
                        FormattingButtons.Underline | FormattingButtons.Strike,
            Blocks = BlockButtons.Blockquote | BlockButtons.CodeBlock |
                    BlockButtons.Header1 | BlockButtons.Header2,
            Lists = ListButtons.Ordered | ListButtons.Bullet | ListButtons.Check,
            Media = MediaButtons.Link | MediaButtons.Image | MediaButtons.Video,
            Styles = StyleButtons.Color | StyleButtons.Background | StyleButtons.Align,
            Advanced = AdvancedButtons.Script | AdvancedButtons.Indent | AdvancedButtons.Clean
        };
    }
}
