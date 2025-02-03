using System.Threading.Tasks;
using Buzz.OrchardCore.Quilljs.ViewModels;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.Utilities;

namespace Buzz.OrchardCore.Quilljs.Settings
{
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

                model.Theme = settings.Theme;
                model.ToolbarOptions = settings.ToolbarOptions;
            })
            .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.Editor() == "Quill")
            {
                var model = new QuillSettingsViewModel();
                var settings = new HtmlFieldQuillEditorSettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.Theme = model.Theme;
                settings.ToolbarOptions = model.ToolbarOptions;

                context.Builder.WithSettings(settings);
            }

            return Edit(partFieldDefinition, context);
        }
    }
}
