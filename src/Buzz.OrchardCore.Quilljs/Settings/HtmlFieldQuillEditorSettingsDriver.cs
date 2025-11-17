using System.Threading.Tasks;
using Buzz.OrchardCore.Quilljs.ViewModels;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.Utilities;

namespace Buzz.OrchardCore.Quilljs.Settings;

public class HtmlFieldQuillEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<HtmlField>
{
    protected readonly IStringLocalizer S;

    public HtmlFieldQuillEditorSettingsDriver(IStringLocalizer<HtmlFieldQuillEditorSettingsDriver> localizer)
    {
        S = localizer;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<QuillSettingsViewModel>("HtmlFieldQuillEditorSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<HtmlFieldQuillEditorSettings>();

            // Populate ViewModel from settings using factory method
            var viewModel = QuillSettingsViewModel.FromToolbarConfig(
                settings.ToolbarConfig,
                settings.Theme
            );

            // Copy properties to model (required by OrchardCore's Initialize pattern)
            model.Theme = viewModel.Theme;
            model.CustomColors = viewModel.CustomColors;
            model.Groups = viewModel.Groups;
        })
        .Location("Editor");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        if (partFieldDefinition.Editor() == "Quill")
        {
            var model = new QuillSettingsViewModel();

            // Bind form data to ViewModel
            await context.Updater.TryUpdateModelAsync(model, Prefix);

            // Convert ViewModel to strongly-typed configuration
            var toolbarConfig = model.ToToolbarConfig();

            // Create settings with new configuration
            var settings = new HtmlFieldQuillEditorSettings
            {
                Theme = model.Theme,
                ToolbarConfig = toolbarConfig
            };

            context.Builder.WithSettings(settings);
        }

        return Edit(partFieldDefinition, context);
    }
}
