# C# Integration Tests Guide

Integration tests verify that the Quill.js module integrates correctly with OrchardCore infrastructure (settings drivers, migrations, etc.).

## Overview

Integration tests are added to the same test project as unit tests but in an `Integration/` folder. They test:

- Settings driver (Edit/UpdateAsync methods)
- Migrations (UpdateFrom1)
- OrchardCore service integration

## Key Differences from Unit Tests

| Aspect | Unit Tests | Integration Tests |
|--------|-----------|-------------------|
| **Scope** | Single class/method | Multiple components |
| **Dependencies** | Mocked | Real OrchardCore services |
| **Speed** | Very fast (<1ms) | Slower (10-100ms) |
| **Setup** | Minimal | Requires OrchardCore context |

## Test Organization

```
tests/Buzz.OrchardCore.Quilljs.Tests/
└── Integration/
    ├── HtmlFieldQuillEditorSettingsDriverTests.cs
    ├── MigrationsTests.cs
    └── Helpers/
        └── OrchardTestHelper.cs
```

## Example: Settings Driver Integration Tests

```csharp
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using Moq;

namespace Buzz.OrchardCore.Quilljs.Tests.Integration;

public class HtmlFieldQuillEditorSettingsDriverTests
{
    private readonly HtmlFieldQuillEditorSettingsDriver _driver;
    private readonly Mock<IStringLocalizer<HtmlFieldQuillEditorSettingsDriver>> _localizerMock;

    public HtmlFieldQuillEditorSettingsDriverTests()
    {
        _localizerMock = new Mock<IStringLocalizer<HtmlFieldQuillEditorSettingsDriver>>();
        _localizerMock.Setup(x => x[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _driver = new HtmlFieldQuillEditorSettingsDriver(_localizerMock.Object);
    }

    [Fact]
    public async Task Edit_WithExistingSettings_PopulatesViewModel()
    {
        // Arrange
        var settings = new HtmlFieldQuillEditorSettings
        {
            Theme = QuillTheme.Bubble,
            ToolbarConfig = new QuillToolbarConfig
            {
                Formatting = FormattingButtons.Bold | FormattingButtons.Italic,
                CustomColors = new List<string> { "#ff0000" }
            }
        };

        var partFieldDefinition = CreatePartFieldDefinition(settings);
        var context = new BuildEditorContext(
            partFieldDefinition,
            null, // typePartDefinition
            "", // groupId
            false, // isNew
            Mock.Of<IShapeFactory>());

        // Act
        var result = _driver.Edit(partFieldDefinition, context);

        // Assert
        result.Should().NotBeNull();
        // Verify the result is an Initialize shape result
        var initializeResult = result.Should().BeOfType<InitializeShapeResult>().Subject;
        // Additional assertions would go here
    }

    [Fact]
    public async Task UpdateAsync_WithValidViewModel_SavesSettings()
    {
        // Arrange
        var partFieldDefinition = CreatePartFieldDefinition(new HtmlFieldQuillEditorSettings());
        var builderMock = new Mock<IContentPartFieldDefinitionBuilder>();
        var updaterMock = new Mock<IUpdateModel>();

        var viewModel = new QuillSettingsViewModel
        {
            Theme = QuillTheme.Snow,
            Bold = true,
            Italic = true,
            CustomColors = new List<string> { "#ff0000", "#00ff00" }
        };

        updaterMock.Setup(x => x.TryUpdateModelAsync(
                It.IsAny<QuillSettingsViewModel>(),
                It.IsAny<string>(),
                It.IsAny<string[]>()))
            .Callback<object, string, string[]>((model, prefix, include) =>
            {
                var vm = (QuillSettingsViewModel)model;
                vm.Theme = viewModel.Theme;
                vm.Bold = viewModel.Bold;
                vm.Italic = viewModel.Italic;
                vm.CustomColors = viewModel.CustomColors;
            })
            .ReturnsAsync(true);

        var context = new UpdatePartFieldEditorContext(
            partFieldDefinition,
            builderMock.Object,
            updaterMock.Object,
            Mock.Of<IServiceProvider>());

        // Act
        await _driver.UpdateAsync(partFieldDefinition, context);

        // Assert
        builderMock.Verify(x => x.WithSettings(
            It.Is<HtmlFieldQuillEditorSettings>(s =>
                s.Theme == QuillTheme.Snow &&
                s.ToolbarConfig.Formatting.HasFlag(FormattingButtons.Bold) &&
                s.ToolbarConfig.CustomColors.Count == 2)),
            Times.Once);
    }

    private ContentPartFieldDefinition CreatePartFieldDefinition(HtmlFieldQuillEditorSettings settings)
    {
        var partDefinition = new ContentPartDefinition("TestPart");
        var fieldDefinition = new ContentFieldDefinition("HtmlField");

        var partFieldDefinition = new ContentPartFieldDefinition(
            fieldDefinition,
            "TestField",
            new Dictionary<string, JToken>
            {
                { nameof(HtmlFieldQuillEditorSettings), JToken.FromObject(settings) }
            });

        return partFieldDefinition;
    }
}
```

## Example: Migration Integration Tests

```csharp
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Buzz.OrchardCore.Quilljs.Tests.Integration;

public class MigrationsTests
{
    [Fact]
    public async Task UpdateFrom1_WithLegacySettings_MigratesSuccessfully()
    {
        // Arrange
        var contentDefinitionManagerMock = new Mock<IContentDefinitionManager>();
        var loggerMock = new Mock<ILogger<Migrations>>();

        var legacySettings = new HtmlFieldQuillEditorSettings
        {
            // Simulate legacy format with ToolbarOptions
        };

        // Setup mock to return content types with legacy settings
        contentDefinitionManagerMock
            .Setup(x => x.ListTypeDefinitionsAsync())
            .ReturnsAsync(new[] { /* test content types */ });

        var migration = new Migrations(
            contentDefinitionManagerMock.Object,
            loggerMock.Object);

        // Act
        var result = await migration.UpdateFrom1Async();

        // Assert
        result.Should().Be(2); // New version number

        // Verify migration was performed
        contentDefinitionManagerMock.Verify(
            x => x.AlterTypeDefinitionAsync(
                It.IsAny<string>(),
                It.IsAny<Action<ContentTypeDefinitionBuilder>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateFrom1_WithMalformedLegacyData_LogsErrorAndContinues()
    {
        // Test error handling during migration
    }
}
```

## Running Integration Tests

```bash
# Run all integration tests
dotnet test --filter "FullyQualifiedName~Integration"

# Run only unit tests (faster)
dotnet test --filter "FullyQualifiedName~Unit"

# Run both
dotnet test
```

## Next Steps

Review the [Playwright E2E Tests Guide](03-playwright-e2e-tests.md) for frontend testing.
