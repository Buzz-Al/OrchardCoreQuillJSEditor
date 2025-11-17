# C# Unit Tests Guide

This guide covers setting up and writing unit tests for the Buzz.OrchardCore.Quilljs module using xUnit.

## Table of Contents

- [Project Setup](#project-setup)
- [Test Organization](#test-organization)
- [Critical Test Cases](#critical-test-cases)
- [Example Tests](#example-tests)
- [Running Tests](#running-tests)
- [Code Coverage](#code-coverage)

## Project Setup

### 1. Create Test Project

```bash
# From repository root
cd tests
dotnet new xunit -n Buzz.OrchardCore.Quilljs.Tests
cd Buzz.OrchardCore.Quilljs.Tests

# Add reference to main module
dotnet add reference ../../src/Buzz.OrchardCore.Quilljs/Buzz.OrchardCore.Quilljs.csproj

# Add test dependencies
dotnet add package FluentAssertions
dotnet add package Moq
dotnet add package coverlet.collector
```

### 2. Update .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="Moq" Version="4.20.70" />
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Buzz.OrchardCore.Quilljs\Buzz.OrchardCore.Quilljs.csproj" />
  </ItemGroup>
</Project>
```

### 3. Add to Solution

```bash
# From repository root
dotnet sln add tests/Buzz.OrchardCore.Quilljs.Tests/Buzz.OrchardCore.Quilljs.Tests.csproj
```

## Test Organization

### Folder Structure

```
tests/Buzz.OrchardCore.Quilljs.Tests/
├── Unit/
│   ├── Settings/
│   │   ├── QuillToolbarConfigTests.cs
│   │   ├── LegacyToolbarMigrationTests.cs
│   │   ├── QuillButtonTests.cs
│   │   └── QuillThemeTests.cs
│   └── ViewModels/
│       └── QuillSettingsViewModelTests.cs
├── Fixtures/
│   ├── LegacyToolbarData.json
│   └── TestDataBuilder.cs
├── Helpers/
│   └── TestExtensions.cs
└── GlobalUsings.cs
```

### GlobalUsings.cs

```csharp
global using Xunit;
global using FluentAssertions;
global using Buzz.OrchardCore.Quilljs.Settings;
global using Buzz.OrchardCore.Quilljs.ViewModels;
```

## Critical Test Cases

### Priority 1: QuillToolbarConfig.GenerateQuillJson()

**File:** `Unit/Settings/QuillToolbarConfigTests.cs`

```csharp
using System.Text.Json;

namespace Buzz.OrchardCore.Quilljs.Tests.Unit.Settings;

public class QuillToolbarConfigTests
{
    [Fact]
    public void GenerateQuillJson_WithEmptyConfig_ReturnsEmptyArray()
    {
        // Arrange
        var config = new QuillToolbarConfig();

        // Act
        var json = config.GenerateQuillJson();

        // Assert
        json.Should().Be("[]");
    }

    [Fact]
    public void GenerateQuillJson_WithBoldAndItalic_ReturnsCorrectJson()
    {
        // Arrange
        var config = new QuillToolbarConfig
        {
            Formatting = FormattingButtons.Bold | FormattingButtons.Italic
        };

        // Act
        var json = config.GenerateQuillJson();

        // Assert
        json.Should().Be("[[\"bold\",\"italic\"]]");
    }

    [Fact]
    public void GenerateQuillJson_WithMultipleCategories_GroupsCorrectly()
    {
        // Arrange
        var config = new QuillToolbarConfig
        {
            Formatting = FormattingButtons.Bold | FormattingButtons.Italic,
            Lists = ListButtons.Ordered | ListButtons.Bullet
        };

        // Act
        var json = config.GenerateQuillJson();
        var parsed = JsonDocument.Parse(json);

        // Assert
        parsed.RootElement.GetArrayLength().Should().Be(2);
        // First group: bold, italic
        parsed.RootElement[0].GetArrayLength().Should().Be(2);
        parsed.RootElement[0][0].GetString().Should().Be("bold");
        parsed.RootElement[0][1].GetString().Should().Be("italic");
        // Second group: lists
        parsed.RootElement[1].GetArrayLength().Should().Be(2);
    }

    [Fact]
    public void GenerateQuillJson_WithHeaders_GeneratesObjectSyntax()
    {
        // Arrange
        var config = new QuillToolbarConfig
        {
            Blocks = BlockButtons.Header1 | BlockButtons.Header2
        };

        // Act
        var json = config.GenerateQuillJson();

        // Assert
        json.Should().Contain("{\"header\":1}");
        json.Should().Contain("{\"header\":2}");
    }

    [Fact]
    public void GenerateQuillJson_WithCustomColors_InjectsIntoColorButtons()
    {
        // Arrange
        var config = new QuillToolbarConfig
        {
            Styles = StyleButtons.Color | StyleButtons.Background,
            CustomColors = new List<string> { "#ff0000", "#00ff00", "#0000ff" }
        };

        // Act
        var json = config.GenerateQuillJson();
        var parsed = JsonDocument.Parse(json);

        // Assert
        // Should have color and background with custom colors
        json.Should().Contain("#ff0000");
        json.Should().Contain("#00ff00");
        json.Should().Contain("#0000ff");
    }

    [Fact]
    public void GenerateQuillJson_WithSizeButton_CreatesMixedTypeArray()
    {
        // Arrange
        var config = new QuillToolbarConfig
        {
            Styles = StyleButtons.Size
        };

        // Act
        var json = config.GenerateQuillJson();

        // Assert
        // Size array contains strings and booleans: ["small", false, "large", "huge"]
        json.Should().Contain("\"small\"");
        json.Should().Contain("false");
        json.Should().Contain("\"large\"");
        json.Should().Contain("\"huge\"");
    }

    [Theory]
    [MemberData(nameof(GetAllButtonCombinations))]
    public void GenerateQuillJson_WithVariousConfigurations_GeneratesValidJson(
        QuillToolbarConfig config,
        string description)
    {
        // Act
        var json = config.GenerateQuillJson();

        // Assert
        var action = () => JsonDocument.Parse(json);
        action.Should().NotThrow($"configuration: {description}");
    }

    public static IEnumerable<object[]> GetAllButtonCombinations()
    {
        yield return new object[]
        {
            new QuillToolbarConfig(),
            "Empty config"
        };

        yield return new object[]
        {
            new QuillToolbarConfig
            {
                Formatting = FormattingButtons.Bold | FormattingButtons.Italic |
                            FormattingButtons.Underline | FormattingButtons.Strike | FormattingButtons.Code
            },
            "All formatting buttons"
        };

        yield return new object[]
        {
            new QuillToolbarConfig
            {
                Blocks = BlockButtons.Blockquote | BlockButtons.CodeBlock |
                        BlockButtons.Header1 | BlockButtons.Header2
            },
            "All block buttons"
        };

        yield return new object[]
        {
            QuillToolbarConfig.CreateFull(),
            "Full toolbar (all buttons)"
        };

        yield return new object[]
        {
            QuillToolbarConfig.CreateStandard(),
            "Standard preset"
        };

        yield return new object[]
        {
            QuillToolbarConfig.CreateMinimal(),
            "Minimal preset"
        };
    }

    [Fact]
    public void CreateStandard_ReturnsConfigWithCommonButtons()
    {
        // Act
        var config = QuillToolbarConfig.CreateStandard();

        // Assert
        config.Formatting.Should().HaveFlag(FormattingButtons.Bold);
        config.Formatting.Should().HaveFlag(FormattingButtons.Italic);
        config.Blocks.Should().HaveFlag(BlockButtons.Blockquote);
        config.Lists.Should().HaveFlag(ListButtons.Ordered);
        config.Media.Should().HaveFlag(MediaButtons.Link);
        config.Advanced.Should().HaveFlag(AdvancedButtons.Clean);
    }
}
```

### Priority 2: LegacyToolbarMigration Tests

**File:** `Unit/Settings/LegacyToolbarMigrationTests.cs`

```csharp
namespace Buzz.OrchardCore.Quilljs.Tests.Unit.Settings;

public class LegacyToolbarMigrationTests
{
    [Fact]
    public void ParseLegacyToolbarOptions_WithSimpleButtons_ReturnsCorrectConfig()
    {
        // Arrange
        var legacyJson = "[[\"bold\",\"italic\",\"underline\"]]";

        // Act
        var config = LegacyToolbarMigration.ParseLegacyToolbarOptions(legacyJson);

        // Assert
        config.Formatting.Should().HaveFlag(FormattingButtons.Bold);
        config.Formatting.Should().HaveFlag(FormattingButtons.Italic);
        config.Formatting.Should().HaveFlag(FormattingButtons.Underline);
    }

    [Fact]
    public void ParseLegacyToolbarOptions_WithHeaderObjects_ReturnsCorrectConfig()
    {
        // Arrange
        var legacyJson = "[[{\"header\":1},{\"header\":2}]]";

        // Act
        var config = LegacyToolbarMigration.ParseLegacyToolbarOptions(legacyJson);

        // Assert
        config.Blocks.Should().HaveFlag(BlockButtons.Header1);
        config.Blocks.Should().HaveFlag(BlockButtons.Header2);
    }

    [Fact]
    public void ParseLegacyToolbarOptions_WithListObjects_ReturnsCorrectConfig()
    {
        // Arrange
        var legacyJson = "[[{\"list\":\"ordered\"},{\"list\":\"bullet\"}]]";

        // Act
        var config = LegacyToolbarMigration.ParseLegacyToolbarOptions(legacyJson);

        // Assert
        config.Lists.Should().HaveFlag(ListButtons.Ordered);
        config.Lists.Should().HaveFlag(ListButtons.Bullet);
    }

    [Fact]
    public void ParseLegacyToolbarOptions_WithColorArrays_ExtractsCustomColors()
    {
        // Arrange
        var legacyJson = @"[[{""color"":[""#ff0000"",""#00ff00"",""#0000ff""]}]]";

        // Act
        var config = LegacyToolbarMigration.ParseLegacyToolbarOptions(legacyJson);

        // Assert
        config.Styles.Should().HaveFlag(StyleButtons.Color);
        config.CustomColors.Should().Contain("#ff0000");
        config.CustomColors.Should().Contain("#00ff00");
        config.CustomColors.Should().Contain("#0000ff");
    }

    [Fact]
    public void ParseLegacyToolbarOptions_WithMalformedJson_ThrowsException()
    {
        // Arrange
        var malformedJson = "[[\"bold\",";

        // Act
        var action = () => LegacyToolbarMigration.ParseLegacyToolbarOptions(malformedJson);

        // Assert
        action.Should().Throw<Exception>();
    }

    [Fact]
    public void ParseLegacyToolbarOptions_WithEmptyArray_ReturnsEmptyConfig()
    {
        // Arrange
        var legacyJson = "[]";

        // Act
        var config = LegacyToolbarMigration.ParseLegacyToolbarOptions(legacyJson);

        // Assert
        config.Formatting.Should().Be(FormattingButtons.None);
        config.Blocks.Should().Be(BlockButtons.None);
        config.Lists.Should().Be(ListButtons.None);
        config.Media.Should().Be(MediaButtons.None);
        config.Styles.Should().Be(StyleButtons.None);
        config.Advanced.Should().Be(AdvancedButtons.None);
    }

    [Theory]
    [InlineData("bold", FormattingButtons.Bold)]
    [InlineData("italic", FormattingButtons.Italic)]
    [InlineData("underline", FormattingButtons.Underline)]
    [InlineData("strike", FormattingButtons.Strike)]
    [InlineData("code-block", BlockButtons.CodeBlock)]
    public void ParseLegacyToolbarOptions_WithIndividualButtons_MapsCorrectly(
        string buttonName,
        object expectedFlag)
    {
        // Arrange
        var legacyJson = $"[[\"{buttonName}\"]]";

        // Act
        var config = LegacyToolbarMigration.ParseLegacyToolbarOptions(legacyJson);

        // Assert
        if (expectedFlag is FormattingButtons formatting)
        {
            config.Formatting.Should().HaveFlag(formatting);
        }
        else if (expectedFlag is BlockButtons block)
        {
            config.Blocks.Should().HaveFlag(block);
        }
    }
}
```

### Priority 3: QuillSettingsViewModel Tests

**File:** `Unit/ViewModels/QuillSettingsViewModelTests.cs`

```csharp
namespace Buzz.OrchardCore.Quilljs.Tests.Unit.ViewModels;

public class QuillSettingsViewModelTests
{
    [Fact]
    public void ToToolbarConfig_WithCheckedBold_SetsBoldFlag()
    {
        // Arrange
        var viewModel = new QuillSettingsViewModel
        {
            Bold = true
        };

        // Act
        var config = viewModel.ToToolbarConfig();

        // Assert
        config.Formatting.Should().HaveFlag(FormattingButtons.Bold);
    }

    [Fact]
    public void ToToolbarConfig_WithMultipleCheckboxes_SetsMultipleFlags()
    {
        // Arrange
        var viewModel = new QuillSettingsViewModel
        {
            Bold = true,
            Italic = true,
            Underline = true,
            OrderedList = true,
            BulletList = true
        };

        // Act
        var config = viewModel.ToToolbarConfig();

        // Assert
        config.Formatting.Should().HaveFlag(FormattingButtons.Bold);
        config.Formatting.Should().HaveFlag(FormattingButtons.Italic);
        config.Formatting.Should().HaveFlag(FormattingButtons.Underline);
        config.Lists.Should().HaveFlag(ListButtons.Ordered);
        config.Lists.Should().HaveFlag(ListButtons.Bullet);
    }

    [Fact]
    public void ToToolbarConfig_WithCustomColors_PreservesColors()
    {
        // Arrange
        var viewModel = new QuillSettingsViewModel
        {
            CustomColors = new List<string> { "#ff0000", "#00ff00" }
        };

        // Act
        var config = viewModel.ToToolbarConfig();

        // Assert
        config.CustomColors.Should().BeEquivalentTo(new[] { "#ff0000", "#00ff00" });
    }

    [Fact]
    public void FromToolbarConfig_WithBoldFlag_ChecksBoldCheckbox()
    {
        // Arrange
        var config = new QuillToolbarConfig
        {
            Formatting = FormattingButtons.Bold
        };

        // Act
        var viewModel = QuillSettingsViewModel.FromToolbarConfig(config, QuillTheme.Snow);

        // Assert
        viewModel.Bold.Should().BeTrue();
        viewModel.Italic.Should().BeFalse();
        viewModel.Theme.Should().Be(QuillTheme.Snow);
    }

    [Fact]
    public void RoundTrip_ViewModel_To_Config_To_ViewModel_PreservesData()
    {
        // Arrange
        var original = new QuillSettingsViewModel
        {
            Theme = QuillTheme.Bubble,
            Bold = true,
            Italic = true,
            Link = true,
            OrderedList = true,
            CustomColors = new List<string> { "#ff0000", "#00ff00", "#0000ff" }
        };

        // Act
        var config = original.ToToolbarConfig();
        var roundTripped = QuillSettingsViewModel.FromToolbarConfig(config, QuillTheme.Bubble);

        // Assert
        roundTripped.Should().BeEquivalentTo(original);
    }

    [Theory]
    [InlineData(true, FormattingButtons.Bold)]
    [InlineData(false, FormattingButtons.None)]
    public void ToToolbarConfig_BoldCheckbox_MapsCorrectly(bool isChecked, FormattingButtons expected)
    {
        // Arrange
        var viewModel = new QuillSettingsViewModel { Bold = isChecked };

        // Act
        var config = viewModel.ToToolbarConfig();

        // Assert
        config.Formatting.Should().Be(expected);
    }
}
```

## Running Tests

### Command Line

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~QuillToolbarConfigTests"

# Run single test
dotnet test --filter "FullyQualifiedName~QuillToolbarConfigTests.GenerateQuillJson_WithBoldAndItalic_ReturnsCorrectJson"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Watch mode (re-run on code changes)
dotnet watch test
```

### Visual Studio

- Test Explorer: View → Test Explorer
- Run All Tests: Ctrl+R, A
- Debug All Tests: Ctrl+R, Ctrl+A

### VS Code

- Install "C# Dev Kit" extension
- Tests appear in Test Explorer
- Click play button to run

## Code Coverage

### Generate Coverage Report

```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:"Html"

# Open report
open coverage-report/index.html
```

### Coverage Goals

| Component | Target Coverage |
|-----------|----------------|
| QuillToolbarConfig | 95%+ |
| LegacyToolbarMigration | 90%+ |
| QuillSettingsViewModel | 95%+ |
| QuillButton enums | 100% |
| Overall | 80%+ |

## Test Data Builders

**File:** `Fixtures/TestDataBuilder.cs`

```csharp
namespace Buzz.OrchardCore.Quilljs.Tests.Fixtures;

public static class TestDataBuilder
{
    public static QuillToolbarConfig CreateFullToolbar()
    {
        return QuillToolbarConfig.CreateFull();
    }

    public static QuillToolbarConfig CreateFormattingOnly()
    {
        return new QuillToolbarConfig
        {
            Formatting = FormattingButtons.Bold | FormattingButtons.Italic |
                        FormattingButtons.Underline
        };
    }

    public static QuillSettingsViewModel CreateViewModelWithAllCheckboxes()
    {
        return new QuillSettingsViewModel
        {
            Bold = true,
            Italic = true,
            Underline = true,
            Strike = true,
            Code = true,
            Blockquote = true,
            CodeBlock = true,
            Header1 = true,
            Header2 = true,
            OrderedList = true,
            BulletList = true,
            CheckList = true,
            Link = true,
            Image = true,
            Video = true,
            Formula = true,
            Color = true,
            Background = true,
            Font = true,
            Size = true,
            Align = true,
            Script = true,
            Indent = true,
            Direction = true,
            Clean = true,
            CustomColors = new List<string> { "#ff0000", "#00ff00", "#0000ff" }
        };
    }
}
```

## Next Steps

1. Create the test project
2. Copy the example test files
3. Run `dotnet test` to verify setup
4. Add more test cases as needed
5. Review [Integration Tests Guide](02-csharp-integration-tests.md) for the next phase
