using System.Collections.Generic;
using Buzz.OrchardCore.Quilljs.Settings;

namespace Buzz.OrchardCore.Quilljs.ViewModels;

/// <summary>
/// View model for Quill editor settings form binding
/// </summary>
public class QuillSettingsViewModel
{
    public QuillTheme Theme { get; set; }

    // Formatting buttons
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public bool Strike { get; set; }
    public bool Code { get; set; }

    // Block buttons
    public bool Blockquote { get; set; }
    public bool CodeBlock { get; set; }
    public bool Header1 { get; set; }
    public bool Header2 { get; set; }

    // List buttons
    public bool OrderedList { get; set; }
    public bool BulletList { get; set; }
    public bool CheckList { get; set; }

    // Media buttons
    public bool Link { get; set; }
    public bool Image { get; set; }
    public bool Video { get; set; }
    public bool Formula { get; set; }

    // Style buttons
    public bool Color { get; set; }
    public bool Background { get; set; }
    public bool Font { get; set; }
    public bool Size { get; set; }
    public bool Align { get; set; }

    // Advanced buttons
    public bool Script { get; set; }
    public bool Indent { get; set; }
    public bool Direction { get; set; }
    public bool Clean { get; set; }

    // Custom colors (comma-separated hex codes in UI, parsed to list)
    public List<string> CustomColors { get; set; } = new List<string>();

    /// <summary>
    /// Converts form values to QuillToolbarConfig
    /// </summary>
    public QuillToolbarConfig ToToolbarConfig()
    {
        var config = new QuillToolbarConfig
        {
            CustomColors = CustomColors ?? new List<string>()
        };

        // Formatting
        if (Bold) config.Formatting |= FormattingButtons.Bold;
        if (Italic) config.Formatting |= FormattingButtons.Italic;
        if (Underline) config.Formatting |= FormattingButtons.Underline;
        if (Strike) config.Formatting |= FormattingButtons.Strike;
        if (Code) config.Formatting |= FormattingButtons.Code;

        // Blocks
        if (Blockquote) config.Blocks |= BlockButtons.Blockquote;
        if (CodeBlock) config.Blocks |= BlockButtons.CodeBlock;
        if (Header1) config.Blocks |= BlockButtons.Header1;
        if (Header2) config.Blocks |= BlockButtons.Header2;

        // Lists
        if (OrderedList) config.Lists |= ListButtons.Ordered;
        if (BulletList) config.Lists |= ListButtons.Bullet;
        if (CheckList) config.Lists |= ListButtons.Check;

        // Media
        if (Link) config.Media |= MediaButtons.Link;
        if (Image) config.Media |= MediaButtons.Image;
        if (Video) config.Media |= MediaButtons.Video;
        if (Formula) config.Media |= MediaButtons.Formula;

        // Styles
        if (Color) config.Styles |= StyleButtons.Color;
        if (Background) config.Styles |= StyleButtons.Background;
        if (Font) config.Styles |= StyleButtons.Font;
        if (Size) config.Styles |= StyleButtons.Size;
        if (Align) config.Styles |= StyleButtons.Align;

        // Advanced
        if (Script) config.Advanced |= AdvancedButtons.Script;
        if (Indent) config.Advanced |= AdvancedButtons.Indent;
        if (Direction) config.Advanced |= AdvancedButtons.Direction;
        if (Clean) config.Advanced |= AdvancedButtons.Clean;

        return config;
    }

    /// <summary>
    /// Populates form values from QuillToolbarConfig
    /// </summary>
    public static QuillSettingsViewModel FromToolbarConfig(QuillToolbarConfig config, QuillTheme theme)
    {
        var viewModel = new QuillSettingsViewModel
        {
            Theme = theme,
            CustomColors = config.CustomColors ?? new List<string>()
        };

        // Formatting
        viewModel.Bold = config.Formatting.HasFlag(FormattingButtons.Bold);
        viewModel.Italic = config.Formatting.HasFlag(FormattingButtons.Italic);
        viewModel.Underline = config.Formatting.HasFlag(FormattingButtons.Underline);
        viewModel.Strike = config.Formatting.HasFlag(FormattingButtons.Strike);
        viewModel.Code = config.Formatting.HasFlag(FormattingButtons.Code);

        // Blocks
        viewModel.Blockquote = config.Blocks.HasFlag(BlockButtons.Blockquote);
        viewModel.CodeBlock = config.Blocks.HasFlag(BlockButtons.CodeBlock);
        viewModel.Header1 = config.Blocks.HasFlag(BlockButtons.Header1);
        viewModel.Header2 = config.Blocks.HasFlag(BlockButtons.Header2);

        // Lists
        viewModel.OrderedList = config.Lists.HasFlag(ListButtons.Ordered);
        viewModel.BulletList = config.Lists.HasFlag(ListButtons.Bullet);
        viewModel.CheckList = config.Lists.HasFlag(ListButtons.Check);

        // Media
        viewModel.Link = config.Media.HasFlag(MediaButtons.Link);
        viewModel.Image = config.Media.HasFlag(MediaButtons.Image);
        viewModel.Video = config.Media.HasFlag(MediaButtons.Video);
        viewModel.Formula = config.Media.HasFlag(MediaButtons.Formula);

        // Styles
        viewModel.Color = config.Styles.HasFlag(StyleButtons.Color);
        viewModel.Background = config.Styles.HasFlag(StyleButtons.Background);
        viewModel.Font = config.Styles.HasFlag(StyleButtons.Font);
        viewModel.Size = config.Styles.HasFlag(StyleButtons.Size);
        viewModel.Align = config.Styles.HasFlag(StyleButtons.Align);

        // Advanced
        viewModel.Script = config.Advanced.HasFlag(AdvancedButtons.Script);
        viewModel.Indent = config.Advanced.HasFlag(AdvancedButtons.Indent);
        viewModel.Direction = config.Advanced.HasFlag(AdvancedButtons.Direction);
        viewModel.Clean = config.Advanced.HasFlag(AdvancedButtons.Clean);

        return viewModel;
    }
}
