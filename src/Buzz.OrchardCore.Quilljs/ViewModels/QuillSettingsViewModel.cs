using System.Collections.Generic;
using Buzz.OrchardCore.Quilljs.Settings;

namespace Buzz.OrchardCore.Quilljs.ViewModels;

/// <summary>
/// View model for Quill editor settings form binding.
/// TODO: Phase 3 - Add groups-based properties for drag-and-drop UI.
/// </summary>
public class QuillSettingsViewModel
{
    public QuillTheme Theme { get; set; }
    public List<string> CustomColors { get; set; } = new();

    // TODO Phase 3: Add Groups property for new UI
    // public List<ToolbarGroupViewModel> Groups { get; set; } = new();

    /// <summary>
    /// Converts form values to QuillToolbarConfig.
    /// Currently returns a standard config as placeholder.
    /// </summary>
    public QuillToolbarConfig ToToolbarConfig()
    {
        // TODO Phase 3: Convert Groups to QuillToolbarConfig
        // For now, return standard config as placeholder
        var config = QuillToolbarConfig.CreateStandard();
        config.CustomColors = CustomColors ?? new List<string>();
        return config;
    }

    /// <summary>
    /// Creates ViewModel from QuillToolbarConfig.
    /// </summary>
    public static QuillSettingsViewModel FromToolbarConfig(QuillToolbarConfig config, QuillTheme theme)
    {
        // TODO Phase 3: Convert QuillToolbarConfig.Groups to ViewModel
        return new QuillSettingsViewModel
        {
            Theme = theme,
            CustomColors = config?.CustomColors ?? new List<string>()
        };
    }
}
