using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Buzz.OrchardCore.Quilljs.Settings;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Data.Migration;

namespace Buzz.OrchardCore.Quilljs;

/// <summary>
/// Data migrations for the Quill editor module.
/// Handles schema changes and data format migrations.
/// </summary>
public class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ILogger<Migrations> _logger;

    public Migrations(
        IContentDefinitionManager contentDefinitionManager,
        ILogger<Migrations> logger)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _logger = logger;
    }

    public int Create()
    {
        return 1;
    }

    public async Task<int> UpdateFrom1()
    {
        _logger.LogInformation("Starting migration: Quill editor settings ToolbarOptions -> ToolbarConfig");

        var migratedCount = 0;
        var errorCount = 0;
        var skippedCount = 0;

        var allTypes = await _contentDefinitionManager.ListTypeDefinitionsAsync();

        foreach (var typeDefinition in allTypes)
        {
            var typeName = typeDefinition.Name;
            var typeHasChanges = false;

            foreach (var typePart in typeDefinition.Parts)
            {
                var partName = typePart.PartDefinition.Name;

                foreach (var fieldDefinition in typePart.PartDefinition.Fields)
                {
                    var fieldName = fieldDefinition.Name;

                    if (fieldDefinition.FieldDefinition.Name == "HtmlField" && fieldDefinition.Editor() == "Quill")
                    {
                        try
                        {
                            // Check the raw JSON settings to detect legacy format
                            var settingsNode = fieldDefinition.Settings[nameof(HtmlFieldQuillEditorSettings)];

                            if (settingsNode != null)
                            {
                                var settingsJson = settingsNode.ToJsonString();

                                // Detect legacy format by looking for "ToolbarOptions" property
                                if (settingsJson.Contains("\"ToolbarOptions\"", StringComparison.OrdinalIgnoreCase))
                                {
                                    _logger.LogInformation(
                                        "Migrating field '{FieldName}' in part '{PartName}' of type '{TypeName}' from legacy ToolbarOptions format",
                                        fieldName, partName, typeName);

                                    // Deserialize to dynamic object to extract ToolbarOptions
                                    using var document = JsonDocument.Parse(settingsJson);
                                    var root = document.RootElement;

                                    // Extract the legacy ToolbarOptions string
                                    if (root.TryGetProperty("ToolbarOptions", out var toolbarOptionsElement))
                                    {
                                        var legacyToolbarOptions = toolbarOptionsElement.GetString();
                                        var theme = QuillTheme.Snow; // Default

                                        // Extract theme if present
                                        if (root.TryGetProperty("Theme", out var themeElement))
                                        {
                                            theme = (QuillTheme)themeElement.GetInt32();
                                        }

                                        // Parse legacy toolbar options into new format
                                        var migratedToolbarConfig = LegacyToolbarMigration.ParseLegacyToolbarOptions(legacyToolbarOptions);

                                        // Create new settings object with migrated data
                                        var newSettings = new HtmlFieldQuillEditorSettings
                                        {
                                            Theme = theme,
                                            ToolbarConfig = migratedToolbarConfig
                                        };

                                        // Serialize and save in new format
                                        var newSettingsJson = JsonSerializer.Serialize(newSettings);
                                        fieldDefinition.Settings[nameof(HtmlFieldQuillEditorSettings)] =
                                            JsonNode.Parse(newSettingsJson);

                                        migratedCount++;
                                        typeHasChanges = true;
                                    }
                                    else
                                    {
                                        _logger.LogWarning(
                                            "Field '{FieldName}' in part '{PartName}' of type '{TypeName}' has ToolbarOptions key but no value",
                                            fieldName, partName, typeName);
                                        skippedCount++;
                                    }
                                }
                                else if (settingsJson.Contains("\"ToolbarConfig\"", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Already in new format, skip
                                    _logger.LogDebug(
                                        "Field '{FieldName}' in part '{PartName}' of type '{TypeName}' already in new ToolbarConfig format",
                                        fieldName, partName, typeName);
                                    skippedCount++;
                                }
                                else
                                {
                                    // Empty or malformed settings
                                    _logger.LogDebug(
                                        "Field '{FieldName}' in part '{PartName}' of type '{TypeName}' has no toolbar configuration",
                                        fieldName, partName, typeName);
                                    skippedCount++;
                                }
                            }
                            else
                            {
                                _logger.LogDebug(
                                    "Field '{FieldName}' in part '{PartName}' of type '{TypeName}' has no settings node",
                                    fieldName, partName, typeName);
                                skippedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            _logger.LogError(ex,
                                "Error migrating field '{FieldName}' in part '{PartName}' of type '{TypeName}'",
                                fieldName, partName, typeName);
                        }
                    }
                }
            }

            if (typeHasChanges)
            {
                // Persist the updated type definition back to the database
                await _contentDefinitionManager.StoreTypeDefinitionAsync(typeDefinition);
                _logger.LogInformation("Content type '{TypeName}' updated with migrated field settings", typeName);
            }
        }

        return 2;
    }
}
