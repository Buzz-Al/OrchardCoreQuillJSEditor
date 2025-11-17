# Test Data & Fixtures Guide

This guide covers organizing and managing test data, fixtures, and test recipes.

## Test Data Organization

```
tests/Buzz.OrchardCore.Quilljs.Tests/
└── Fixtures/
    ├── LegacyToolbarData.json        # Legacy migration test data
    ├── TestDataBuilder.cs             # Test object builders
    └── SampleConfigurations.cs        # Common toolbar configs
```

## Legacy Migration Test Data

**File:** `Fixtures/LegacyToolbarData.json`

```json
{
  "simple_formatting": "[[\"bold\",\"italic\",\"underline\"]]",
  "with_headers": "[[\"bold\",\"italic\"],[{\"header\":1},{\"header\":2}]]",
  "with_lists": "[[{\"list\":\"ordered\"},{\"list\":\"bullet\"}]]",
  "with_custom_colors": "[[{\"color\":[\"#ff0000\",\"#00ff00\",\"#0000ff\"]}]]",
  "full_toolbar": "[[\"bold\",\"italic\",\"underline\",\"strike\"],[\"blockquote\",\"code-block\"],[{\"header\":1},{\"header\":2}],[{\"list\":\"ordered\"},{\"list\":\"bullet\"}],[\"link\",\"image\"],[{\"color\":[]},{\"background\":[]}],[\"clean\"]]",
  "malformed": "[[\"bold\",",
  "empty": "[]"
}
```

**Usage in Tests:**

```csharp
public class LegacyToolbarMigrationTests
{
    private readonly Dictionary<string, string> _testData;

    public LegacyToolbarMigrationTests()
    {
        var json = File.ReadAllText("Fixtures/LegacyToolbarData.json");
        _testData = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }

    [Fact]
    public void ParseLegacyToolbarOptions_WithSimpleFormatting_ParsesCorrectly()
    {
        // Arrange
        var legacyJson = _testData["simple_formatting"];

        // Act
        var config = LegacyToolbarMigration.ParseLegacyToolbarOptions(legacyJson);

        // Assert
        config.Formatting.Should().HaveFlag(FormattingButtons.Bold);
        config.Formatting.Should().HaveFlag(FormattingButtons.Italic);
        config.Formatting.Should().HaveFlag(FormattingButtons.Underline);
    }
}
```

## Test Data Builders

**File:** `Fixtures/TestDataBuilder.cs`

```csharp
namespace Buzz.OrchardCore.Quilljs.Tests.Fixtures;

/// <summary>
/// Fluent builder for creating test configurations
/// </summary>
public class ToolbarConfigBuilder
{
    private FormattingButtons _formatting = FormattingButtons.None;
    private BlockButtons _blocks = BlockButtons.None;
    private ListButtons _lists = ListButtons.None;
    private MediaButtons _media = MediaButtons.None;
    private StyleButtons _styles = StyleButtons.None;
    private AdvancedButtons _advanced = AdvancedButtons.None;
    private List<string> _customColors = new();

    public ToolbarConfigBuilder WithBold()
    {
        _formatting |= FormattingButtons.Bold;
        return this;
    }

    public ToolbarConfigBuilder WithItalic()
    {
        _formatting |= FormattingButtons.Italic;
        return this;
    }

    public ToolbarConfigBuilder WithFormatting(FormattingButtons buttons)
    {
        _formatting = buttons;
        return this;
    }

    public ToolbarConfigBuilder WithColor(string hexColor)
    {
        _styles |= StyleButtons.Color;
        _customColors.Add(hexColor);
        return this;
    }

    public ToolbarConfigBuilder WithColors(params string[] hexColors)
    {
        _styles |= StyleButtons.Color;
        _customColors.AddRange(hexColors);
        return this;
    }

    public QuillToolbarConfig Build()
    {
        return new QuillToolbarConfig
        {
            Formatting = _formatting,
            Blocks = _blocks,
            Lists = _lists,
            Media = _media,
            Styles = _styles,
            Advanced = _advanced,
            CustomColors = _customColors
        };
    }
}

/// <summary>
/// Static factory methods for common configurations
/// </summary>
public static class SampleConfigurations
{
    public static QuillToolbarConfig EmptyToolbar() =>
        new QuillToolbarConfig();

    public static QuillToolbarConfig MinimalToolbar() =>
        new ToolbarConfigBuilder()
            .WithBold()
            .WithItalic()
            .Build();

    public static QuillToolbarConfig WithCustomColors() =>
        new ToolbarConfigBuilder()
            .WithColors("#ff0000", "#00ff00", "#0000ff")
            .Build();

    public static QuillToolbarConfig BlogPostToolbar() =>
        new ToolbarConfigBuilder()
            .WithFormatting(FormattingButtons.Bold | FormattingButtons.Italic | FormattingButtons.Underline)
            .Build();
}
```

**Usage:**

```csharp
[Fact]
public void Test_WithCustomConfiguration()
{
    // Arrange
    var config = new ToolbarConfigBuilder()
        .WithBold()
        .WithItalic()
        .WithColor("#ff0000")
        .WithColor("#00ff00")
        .Build();

    // Act & Assert
    config.Formatting.Should().HaveFlag(FormattingButtons.Bold);
    config.CustomColors.Should().Contain("#ff0000");
}

[Fact]
public void Test_WithPresetConfiguration()
{
    // Arrange
    var config = SampleConfigurations.BlogPostToolbar();

    // Act & Assert
    config.Formatting.Should().HaveFlag(FormattingButtons.Bold);
}
```

## OrchardCore Test Recipes

For Playwright tests, create a test recipe in Sample.Web:

**File:** `samples/Buzz.OrchardCore.Quilljs.Sample.Web/Recipes/test-setup.recipe.json`

```json
{
  "name": "TestSetup",
  "displayName": "Test Data Setup",
  "description": "Sets up test data for automated E2E tests",
  "author": "Test Suite",
  "website": "",
  "version": "1.0",
  "issetuprecipe": false,
  "categories": [ "test" ],
  "tags": [ "test" ],
  "steps": [
    {
      "name": "feature",
      "enable": [
        "Buzz.OrchardCore.Quilljs",
        "OrchardCore.Contents",
        "OrchardCore.ContentFields"
      ]
    },
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        {
          "Name": "TestBlogPost",
          "DisplayName": "Test Blog Post",
          "Settings": {
            "ContentTypeSettings": {
              "Creatable": true,
              "Listable": true
            }
          },
          "ContentTypePartDefinitionRecords": [
            {
              "PartName": "TestBlogPost",
              "Name": "TestBlogPost",
              "Settings": {},
              "ContentPartFieldDefinitionRecords": [
                {
                  "FieldName": "HtmlField",
                  "Name": "Content",
                  "Settings": {
                    "ContentPartFieldSettings": {
                      "DisplayName": "Content",
                      "Editor": "Quill"
                    },
                    "HtmlFieldQuillEditorSettings": {
                      "Theme": 10,
                      "ToolbarConfig": {
                        "Formatting": 31,
                        "Blocks": 0,
                        "Lists": 0,
                        "Media": 1,
                        "Styles": 0,
                        "Advanced": 0,
                        "CustomColors": []
                      }
                    }
                  }
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}
```

## Playwright Test Data

**File:** `tests/Buzz.OrchardCore.Quilljs.UITests/fixtures/test-data.ts`

```typescript
export const TEST_USERS = {
  admin: {
    username: 'admin',
    password: 'Password123!'
  }
};

export const TEST_COLORS = {
  red: '#ff0000',
  green: '#00ff00',
  blue: '#0000ff',
  custom: '#84BD00'
};

export const TEST_CONTENT_TYPES = {
  blogPost: 'BlogPost',
  page: 'Page'
};

// Helper to login
export async function loginAsAdmin(page: any) {
  await page.goto('/login');
  await page.fill('input[name="UserName"]', TEST_USERS.admin.username);
  await page.fill('input[name="Password"]', TEST_USERS.admin.password);
  await page.click('button[type="submit"]');
  await page.waitForURL(/.*Admin/);
}
```

**Usage:**

```typescript
import { test, expect } from '@playwright/test';
import { loginAsAdmin, TEST_COLORS, TEST_CONTENT_TYPES } from '../fixtures/test-data';

test('color picker test', async ({ page }) => {
  await loginAsAdmin(page);
  await page.goto(`/Admin/ContentTypes/Edit/${TEST_CONTENT_TYPES.blogPost}`);

  await page.fill('#new-color-input', TEST_COLORS.red);
  await page.click('#add-color-btn');

  await expect(page.locator(`text=${TEST_COLORS.red}`)).toBeVisible();
});
```

## Mock Data for Unit Tests

**File:** `Fixtures/MockDataHelpers.cs`

```csharp
using Moq;
using Microsoft.Extensions.Localization;

namespace Buzz.OrchardCore.Quilljs.Tests.Fixtures;

public static class MockDataHelpers
{
    public static Mock<IStringLocalizer<T>> CreateMockLocalizer<T>()
    {
        var mock = new Mock<IStringLocalizer<T>>();
        mock.Setup(x => x[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));
        return mock;
    }

    public static ContentPartFieldDefinition CreateMockFieldDefinition(
        string partName = "TestPart",
        string fieldName = "TestField",
        HtmlFieldQuillEditorSettings settings = null)
    {
        settings ??= new HtmlFieldQuillEditorSettings();

        var partDefinition = new ContentPartDefinition(partName);
        var fieldDefinition = new ContentFieldDefinition("HtmlField");

        return new ContentPartFieldDefinition(
            fieldDefinition,
            fieldName,
            new Dictionary<string, JToken>
            {
                { nameof(HtmlFieldQuillEditorSettings), JToken.FromObject(settings) }
            });
    }
}
```

## Best Practices

1. **Reuse test data** - Don't duplicate test data across files
2. **Use builders** - Fluent builders make tests more readable
3. **Organize by scenario** - Group related test data together
4. **Version test data** - Include version info in fixtures
5. **Clean up** - Playwright tests should clean up created content

## Next Steps

See the [Quick Start Guide](06-quick-start.md) to begin implementing tests.
