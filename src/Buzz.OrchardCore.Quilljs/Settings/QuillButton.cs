using System;

namespace Buzz.OrchardCore.Quilljs.Settings;

/// <summary>
/// Formatting toolbar buttons (inline text styles)
/// </summary>
[Flags]
public enum FormattingButtons
{
    None = 0,
    Bold = 1 << 0,
    Italic = 1 << 1,
    Underline = 1 << 2,
    Strike = 1 << 3,
    Code = 1 << 4
}

/// <summary>
/// Block-level formatting buttons
/// </summary>
[Flags]
public enum BlockButtons
{
    None = 0,
    Blockquote = 1 << 0,
    CodeBlock = 1 << 1,
    Header1 = 1 << 2,
    Header2 = 1 << 3
}

/// <summary>
/// List formatting buttons
/// </summary>
[Flags]
public enum ListButtons
{
    None = 0,
    Ordered = 1 << 0,
    Bullet = 1 << 1,
    Check = 1 << 2
}

/// <summary>
/// Media insertion buttons
/// </summary>
[Flags]
public enum MediaButtons
{
    None = 0,
    Link = 1 << 0,
    Image = 1 << 1,
    Video = 1 << 2,
    Formula = 1 << 3
}

/// <summary>
/// Text styling buttons
/// </summary>
[Flags]
public enum StyleButtons
{
    None = 0,
    Color = 1 << 0,
    Background = 1 << 1,
    Font = 1 << 2,
    Size = 1 << 3,
    Align = 1 << 4
}

/// <summary>
/// Advanced formatting buttons
/// </summary>
[Flags]
public enum AdvancedButtons
{
    None = 0,
    Script = 1 << 0,
    Indent = 1 << 1,
    Direction = 1 << 2,
    Clean = 1 << 3
}
