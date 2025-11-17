using System.Collections.Generic;
using System.Linq;

namespace Buzz.OrchardCore.Quilljs.Settings;

/// <summary>
/// Static registry of all available Quill toolbar button types and their metadata.
/// </summary>
public static class ButtonRegistry
{
    private static readonly Dictionary<string, ButtonMetadata> _buttons = new()
    {
        // Formatting buttons
        ["bold"] = new("Bold", "B", "Formatting", false),
        ["italic"] = new("Italic", "I", "Formatting", false),
        ["underline"] = new("Underline", "U", "Formatting", false),
        ["strike"] = new("Strikethrough", "S", "Formatting", false),
        ["code"] = new("Inline Code", "<>", "Formatting", false),

        // Block buttons
        ["blockquote"] = new("Blockquote", "\"", "Blocks", false),
        ["code-block"] = new("Code Block", "{ }", "Blocks", false),
        ["header"] = new("Header", "H", "Blocks", true, new[] { "1", "2" }),

        // List buttons
        ["list"] = new("List", "•", "Lists", true, new[] { "ordered", "bullet", "check" }),

        // Media buttons
        ["link"] = new("Link", "🔗", "Media", false),
        ["image"] = new("Image", "🖼", "Media", false),
        ["video"] = new("Video", "🎥", "Media", false),
        ["formula"] = new("Formula", "∑", "Media", false),

        // Style buttons
        ["color"] = new("Text Color", "A", "Styles", false),
        ["background"] = new("Background Color", "■", "Styles", false),
        ["font"] = new("Font Family", "Font", "Styles", false),
        ["size"] = new("Font Size", "Size", "Styles", false),
        ["align"] = new("Alignment", "≡", "Styles", false),

        // Advanced buttons
        ["script"] = new("Script", "x²", "Advanced", true, new[] { "sub", "super" }),
        ["indent"] = new("Indent", "→", "Advanced", true, new[] { "-1", "+1" }),
        ["direction"] = new("Text Direction", "RTL", "Advanced", true, new[] { "rtl" }),
        ["clean"] = new("Remove Formatting", "⌧", "Advanced", false)
    };

    /// <summary>
    /// Get metadata for a specific button type.
    /// </summary>
    public static ButtonMetadata Get(string type)
    {
        return _buttons.TryGetValue(type, out var metadata)
            ? metadata
            : new ButtonMetadata(type, "?", "Unknown", false);
    }

    /// <summary>
    /// Get all available button types.
    /// </summary>
    public static IEnumerable<ButtonMetadata> All => _buttons.Values;

    /// <summary>
    /// Get buttons filtered by category.
    /// </summary>
    public static IEnumerable<ButtonMetadata> GetByCategory(string category)
    {
        return _buttons.Values.Where(b => b.Category == category);
    }

    /// <summary>
    /// Check if a button type is valid.
    /// </summary>
    public static bool IsValid(string type)
    {
        return _buttons.ContainsKey(type);
    }

    /// <summary>
    /// Get all available categories.
    /// </summary>
    public static IEnumerable<string> Categories => _buttons.Values
        .Select(b => b.Category)
        .Distinct()
        .OrderBy(c => c);
}
