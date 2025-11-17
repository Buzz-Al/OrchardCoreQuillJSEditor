# Quick Start Guide

This guide provides a step-by-step walkthrough to get your first tests running in 30 minutes.

## Prerequisites

- .NET 8.0 SDK installed
- Node.js 20.x installed (for Playwright)
- Your OrchardCore sample app running

## Phase 1: C# Unit Tests (15 minutes)

### Step 1: Create Test Project

```bash
# From repository root
cd tests
dotnet new xunit -n Buzz.OrchardCore.Quilljs.Tests
cd Buzz.OrchardCore.Quilljs.Tests

# Add reference to main module
dotnet add reference ../../src/Buzz.OrchardCore.Quilljs/Buzz.OrchardCore.Quilljs.csproj

# Add test packages
dotnet add package FluentAssertions
dotnet add package coverlet.collector

# Add to solution
cd ../..
dotnet sln add tests/Buzz.OrchardCore.Quilljs.Tests/Buzz.OrchardCore.Quilljs.Tests.csproj
```

### Step 2: Create Your First Test

Create `tests/Buzz.OrchardCore.Quilljs.Tests/QuillToolbarConfigTests.cs`:

```csharp
using Xunit;
using FluentAssertions;
using Buzz.OrchardCore.Quilljs.Settings;

namespace Buzz.OrchardCore.Quilljs.Tests;

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
    public void CreateStandard_ReturnsConfigWithCommonButtons()
    {
        // Act
        var config = QuillToolbarConfig.CreateStandard();

        // Assert
        config.Formatting.Should().HaveFlag(FormattingButtons.Bold);
        config.Formatting.Should().HaveFlag(FormattingButtons.Italic);
    }
}
```

### Step 3: Run Your First Tests

```bash
# From repository root
dotnet test

# Expected output:
# Passed!  - Failed:     0, Passed:     3, Skipped:     0, Total:     3
```

### Step 4: Verify Test Discovery

```bash
# List all tests
dotnet test --list-tests

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

🎉 **Checkpoint**: You now have 3 passing unit tests!

## Phase 2: Playwright E2E Tests (15 minutes)

### Step 1: Create Playwright Project

```bash
# From repository root
mkdir -p tests/Buzz.OrchardCore.Quilljs.UITests
cd tests/Buzz.OrchardCore.Quilljs.UITests

# Initialize Playwright
npm init playwright@latest

# Choose:
# - TypeScript: Yes
# - Tests folder: tests
# - GitHub Actions: No
# - Install browsers: Yes
```

### Step 2: Configure Playwright

Edit `playwright.config.ts`:

```typescript
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  fullyParallel: false,  // Run tests sequentially for now
  retries: 0,
  workers: 1,
  use: {
    baseURL: process.env.BASE_URL || 'http://localhost:5000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
```

### Step 3: Create Your First E2E Test

Create `tests/color-picker.spec.ts`:

```typescript
import { test, expect } from '@playwright/test';

test.describe('Color Picker Basic Test', () => {
  test('should add and display a color', async ({ page }) => {
    // Login
    await page.goto('/login');
    await page.fill('input[name="UserName"]', 'admin');
    await page.fill('input[name="Password"]', 'Password123!');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL(/.*Admin/);

    // Navigate to field settings
    await page.goto('/Admin/ContentTypes/Edit/BlogPost');
    await page.click('text=QuillJs');

    // Add a color
    await page.fill('#new-color-input', '#ff0000');
    await page.click('#add-color-btn');

    // Verify color appears
    await expect(page.locator('text=#ff0000')).toBeVisible();
    await expect(page.locator('.color-tag-term')).toHaveCount(1);
  });
});
```

### Step 4: Run Playwright Tests

```bash
# Terminal 1: Start Sample.Web
cd samples/Buzz.OrchardCore.Quilljs.Sample.Web
dotnet run

# Terminal 2: Run Playwright tests
cd tests/Buzz.OrchardCore.Quilljs.UITests
npm test

# Or run with UI (recommended for debugging)
npx playwright test --ui
```

🎉 **Checkpoint**: Your first E2E test is passing!

## Commands Cheat Sheet

### C# Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Watch mode (re-run on changes)
dotnet watch test

# Run specific test
dotnet test --filter "FullyQualifiedName~GenerateQuillJson_WithBoldAndItalic_ReturnsCorrectJson"

# Run only unit tests (when you have integration tests too)
dotnet test --filter "FullyQualifiedName~Unit"
```

### Playwright Tests

```bash
# Run all tests
npm test

# Run in UI mode (best for development)
npx playwright test --ui

# Run in headed mode (see browser)
npx playwright test --headed

# Run specific test file
npx playwright test color-picker.spec.ts

# Debug mode
npx playwright test --debug

# Generate code (record interactions)
npx playwright codegen http://localhost:5000
```

## Troubleshooting

### "No tests found"

**Problem**: `dotnet test` says no tests were discovered

**Solutions**:
```bash
# 1. Verify test project is in solution
dotnet sln list

# 2. Rebuild
dotnet clean
dotnet build

# 3. Check test project has correct SDK
# .csproj should have: <Project Sdk="Microsoft.NET.Sdk">
```

### "FluentAssertions not found"

**Problem**: Compiler error about FluentAssertions

**Solution**:
```bash
cd tests/Buzz.OrchardCore.Quilljs.Tests
dotnet add package FluentAssertions
dotnet restore
```

### Playwright: "Timeout waiting for locator"

**Problem**: Test fails with timeout errors

**Solutions**:
```typescript
// 1. Increase timeout for slow operations
await page.click('button', { timeout: 10000 });

// 2. Wait for network to be idle
await page.waitForLoadState('networkidle');

// 3. Add explicit waits
await page.waitForSelector('.color-tag-term');

// 4. Check element exists before interaction
await expect(page.locator('#add-color-btn')).toBeVisible();
await page.click('#add-color-btn');
```

### Sample.Web won't start

**Problem**: Port already in use

**Solution**:
```bash
# Find process using port 5000
lsof -i :5000

# Kill it
kill -9 <PID>

# Or use different port
dotnet run --urls="http://localhost:5001"
```

## Next Steps - Full Implementation

Now that you have basic tests working, expand your test coverage:

### Week 1: Complete Unit Tests
1. Add tests for `LegacyToolbarMigration` (see [C# Unit Tests Guide](01-csharp-unit-tests.md))
2. Add tests for `QuillSettingsViewModel`
3. Add tests for all enum flag combinations
4. Target: 20-30 unit tests, 80%+ coverage

### Week 2: Integration Tests
1. Add tests for `HtmlFieldQuillEditorSettingsDriver`
2. Add tests for `Migrations.UpdateFrom1()`
3. See [Integration Tests Guide](02-csharp-integration-tests.md)

### Week 3: Complete E2E Tests
1. Add color persistence validation test (bug fix verification)
2. Add settings form tests
3. Add editor initialization tests
4. See [Playwright E2E Tests Guide](03-playwright-e2e-tests.md)

### Week 4: CI/CD Integration
1. Update Azure DevOps pipeline
2. Add code coverage reporting
3. Set up PR validation
4. See [CI/CD Integration Guide](04-ci-cd-integration.md)

## Recommended Reading Order

1. ✅ **Quick Start** (you are here)
2. 📖 [C# Unit Tests Guide](01-csharp-unit-tests.md) - Next read this for detailed test examples
3. 📖 [Playwright E2E Tests Guide](03-playwright-e2e-tests.md) - Critical color picker bug validation
4. 📖 [CI/CD Integration Guide](04-ci-cd-integration.md) - Automate test execution
5. 📖 [Test Data & Fixtures](05-test-data-fixtures.md) - Organize test data
6. 📖 [C# Integration Tests Guide](02-csharp-integration-tests.md) - Advanced scenarios

## Success Criteria

You'll know you're on the right track when:

- ✅ `dotnet test` passes with 3+ tests
- ✅ Code coverage report shows >70%
- ✅ Playwright tests pass locally
- ✅ Tests run in < 10 seconds
- ✅ CI pipeline runs tests automatically
- ✅ Failed tests show helpful error messages

## Getting Help

- Review the detailed guides in this folder
- Check troubleshooting sections
- All test code examples are production-ready
- Playwright has excellent documentation: https://playwright.dev

## Celebrate Your Progress!

- 🎯 First test running: You're testing!
- 🎯 10 tests passing: Building momentum
- 🎯 80% coverage: Production quality
- 🎯 E2E tests passing: Full stack validated
- 🎯 CI running tests: Professional setup

You're now ready to implement comprehensive testing! Start with the [C# Unit Tests Guide](01-csharp-unit-tests.md) to expand your test suite.
