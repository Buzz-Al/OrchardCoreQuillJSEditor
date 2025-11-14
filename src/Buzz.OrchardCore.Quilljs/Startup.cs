using System;
using Buzz.OrchardCore.Quilljs.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace Buzz.OrchardCore.Quilljs;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
      services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
      services.AddScoped<IContentPartFieldDefinitionDisplayDriver, HtmlFieldQuillEditorSettingsDriver>();
      services.AddScoped<IDataMigration, Migrations>();
    }
}
