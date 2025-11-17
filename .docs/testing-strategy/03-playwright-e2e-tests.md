# Playwright E2E Tests Guide

This guide covers setting up and writing end-to-end tests using Playwright to test the Quill.js module's UI functionality.

## Why Playwright?

- Tests real user interactions in a browser
- Validates the complete stack (backend + frontend)
- Critical for testing JavaScript-heavy features like the color picker
- Supports multiple browsers (Chromium, Firefox, WebKit)

## Project Setup

### 1. Create Playwright Project

```bash
# From repository root
mkdir -p tests/Buzz.OrchardCore.Quilljs.UITests
cd tests/Buzz.OrchardCore.Quilljs.UITests

# Initialize Playwright project
npm init playwright@latest

# Choose these options:
# - TypeScript: Yes
# - Tests folder: tests
# - GitHub Actions: No (we use Azure DevOps)
# - Install browsers: Yes
```

### 2. Project Structure

```
tests/Buzz.OrchardCore.Quilljs.UITests/
├── tests/
│   ├── color-picker.spec.ts          # Color picker tests (CRITICAL)
│   ├── settings-form.spec.ts         # Settings form tests
│   ├── editor-initialization.spec.ts # Editor tests
│   └── helpers/
│       └── page-objects.ts           # Page object models
├── fixtures/
│   └── test-data.ts                  # Test data and utilities
├── playwright.config.ts              # Playwright configuration
├── package.json
└── README.md
```

### 3. playwright.config.ts

```typescript
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['html'],
    ['junit', { outputFile: 'test-results/junit.xml' }]
  ],
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

## Critical Test: Color Picker Bug Fix Validation

**File:** `tests/color-picker.spec.ts`

This test validates the color picker persistence bug fix we implemented.

```typescript
import { test, expect } from '@playwright/test';

test.describe('Color Picker Persistence', () => {
  test.beforeEach(async ({ page }) => {
    // Login to OrchardCore admin
    await page.goto('/login');
    await page.fill('input[name="UserName"]', 'admin');
    await page.fill('input[name="Password"]', 'Password123!');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL(/.*Admin/);
  });

  test('should persist colors after save - bug fix validation', async ({ page }) => {
    // Navigate to content type with Quill field
    await page.goto('/Admin/ContentTypes/Edit/BlogPost');

    // Click to edit the QuillJs field
    await page.click('text=QuillJs');
    await expect(page.locator('text=Quill Settings')).toBeVisible();

    // Add 3 colors
    const colors = ['#ff0000', '#00ff00', '#0000ff'];

    for (const color of colors) {
      await page.fill('#new-color-input', color);
      await page.click('#add-color-btn');

      // Verify color appears in UI
      await expect(page.locator(`text=${color}`)).toBeVisible();
    }

    // Verify 3 color tags are visible
    await expect(page.locator('.color-tag-term')).toHaveCount(3);

    // Intercept the form submission to verify correct field names
    const responsePromise = page.waitForResponse(
      response => response.url().includes('Edit') && response.request().method() === 'POST'
    );

    // Save the settings
    await page.click('button:has-text("Save")');
    const response = await responsePromise;

    // Get the form data to verify field naming
    const postData = response.request().postData();

    // CRITICAL: Verify the form data includes the correct prefixed field names
    // This validates our bug fix for the missing prefix
    expect(postData).toContain('CustomColors[0]');
    expect(postData).toContain('CustomColors[1]');
    expect(postData).toContain('CustomColors[2]');

    // Verify the colors are URL-encoded correctly
    expect(postData).toContain(encodeURIComponent('#ff0000'));
    expect(postData).toContain(encodeURIComponent('#00ff00'));
    expect(postData).toContain(encodeURIComponent('#0000ff'));

    // Wait for save confirmation
    await expect(page.locator('.alert-success, .message-success')).toBeVisible();

    // Reload the page
    await page.reload();
    await page.waitForLoadState('networkidle');

    // Re-open the field settings
    await page.click('text=QuillJs');

    // CRITICAL ASSERTION: Verify all 3 colors are still present
    await expect(page.locator('.color-tag-term')).toHaveCount(3);
    await expect(page.locator('text=#ff0000')).toBeVisible();
    await expect(page.locator('text=#00ff00')).toBeVisible();
    await expect(page.locator('text=#0000ff')).toBeVisible();
  });

  test('should maintain consecutive indexes after removing colors', async ({ page }) => {
    // Navigate and add colors
    await page.goto('/Admin/ContentTypes/Edit/BlogPost');
    await page.click('text=QuillJs');

    // Add 4 colors
    const colors = ['#ff0000', '#00ff00', '#0000ff', '#ff00ff'];
    for (const color of colors) {
      await page.fill('#new-color-input', color);
      await page.click('#add-color-btn');
    }

    await expect(page.locator('.color-tag-term')).toHaveCount(4);

    // Remove the 2nd color (index 1, which is #00ff00)
    const secondColorTag = page.locator('.color-tag-term').nth(1);
    await secondColorTag.locator('.remove-color-btn').click();

    // Verify only 3 colors remain
    await expect(page.locator('.color-tag-term')).toHaveCount(3);

    // Inspect the hidden inputs to verify re-indexing
    const hiddenInputs = page.locator('.color-tag-term input[type="hidden"]');

    // Get all hidden input names
    const names = await hiddenInputs.evaluateAll(
      inputs => inputs.map(i => (i as HTMLInputElement).name)
    );

    // CRITICAL: Verify names are consecutive [0], [1], [2]
    expect(names).toHaveLength(3);
    expect(names[0]).toMatch(/CustomColors\[0\]$/);
    expect(names[1]).toMatch(/CustomColors\[1\]$/);
    expect(names[2]).toMatch(/CustomColors\[2\]$/);

    // Save and verify persistence
    await page.click('button:has-text("Save")');
    await page.waitForLoadState('networkidle');
    await page.reload();
    await page.click('text=QuillJs');

    // Verify 3 colors persisted (not 4, not 0)
    await expect(page.locator('.color-tag-term')).toHaveCount(3);
  });

  test('should show/hide empty state correctly', async ({ page }) => {
    await page.goto('/Admin/ContentTypes/Edit/BlogPost');
    await page.click('text=QuillJs');

    // If there are no colors, empty state should be visible
    const colorCount = await page.locator('.color-tag-term').count();
    const emptyState = page.locator('#empty-state');

    if (colorCount === 0) {
      await expect(emptyState).toBeVisible();
    } else {
      await expect(emptyState).not.toBeVisible();
    }

    // Add a color
    await page.fill('#new-color-input', '#ff0000');
    await page.click('#add-color-btn');

    // Empty state should now be hidden
    await expect(emptyState).not.toBeVisible();

    // Remove the color
    await page.locator('.remove-color-btn').first().click();

    // Empty state should reappear
    await expect(emptyState).toBeVisible();
  });
});
```

## Settings Form Tests

**File:** `tests/settings-form.spec.ts`

```typescript
import { test, expect } from '@playwright/test';

test.describe('Quill Settings Form', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.fill('input[name="UserName"]', 'admin');
    await page.fill('input[name="Password"]', 'Password123!');
    await page.click('button[type="submit"]');
  });

  test('should save checkbox selections', async ({ page }) => {
    await page.goto('/Admin/ContentTypes/Edit/BlogPost');
    await page.click('text=QuillJs');

    // Check Bold, Italic, Underline
    await page.check('input[name*="Bold"]');
    await page.check('input[name*="Italic"]');
    await page.check('input[name*="Underline"]');

    await page.click('button:has-text("Save")');
    await page.waitForLoadState('networkidle');

    // Reload and verify
    await page.reload();
    await page.click('text=QuillJs');

    await expect(page.locator('input[name*="Bold"]')).toBeChecked();
    await expect(page.locator('input[name*="Italic"]')).toBeChecked();
    await expect(page.locator('input[name*="Underline"]')).toBeChecked();
  });

  test('should change theme and persist', async ({ page }) => {
    await page.goto('/Admin/ContentTypes/Edit/BlogPost');
    await page.click('text=QuillJs');

    // Select Bubble theme
    await page.selectOption('select[name*="Theme"]', 'Bubble');

    await page.click('button:has-text("Save")');
    await page.reload();
    await page.click('text=QuillJs');

    // Verify Bubble is still selected
    await expect(page.locator('select[name*="Theme"]')).toHaveValue('20'); // Bubble = 20
  });
});
```

## Editor Initialization Tests

**File:** `tests/editor-initialization.spec.ts`

```typescript
import { test, expect } from '@playwright/test';

test.describe('Quill Editor Initialization', () => {
  test('should initialize with configured toolbar buttons', async ({ page }) => {
    // First, configure the field with specific buttons
    await page.goto('/login');
    await page.fill('input[name="UserName"]', 'admin');
    await page.fill('input[name="Password"]', 'Password123!');
    await page.click('button[type="submit"]');

    // Configure Bold, Italic, Link only
    await page.goto('/Admin/ContentTypes/Edit/BlogPost');
    await page.click('text=QuillJs');

    await page.check('input[name*="Bold"]');
    await page.check('input[name*="Italic"]');
    await page.check('input[name*="Link"]');
    await page.click('button:has-text("Save")');

    // Now create a blog post and verify the editor
    await page.goto('/Admin/Contents/ContentItems/BlogPost/Create');

    // Wait for Quill editor to initialize
    await page.waitForSelector('.ql-editor');

    // Verify only Bold, Italic, Link buttons are present
    await expect(page.locator('.ql-bold')).toBeVisible();
    await expect(page.locator('.ql-italic')).toBeVisible();
    await expect(page.locator('.ql-link')).toBeVisible();

    // Verify other buttons are NOT present
    await expect(page.locator('.ql-underline')).not.toBeVisible();
    await expect(page.locator('.ql-strike')).not.toBeVisible();
  });

  test('should apply custom colors to color picker', async ({ page }) => {
    // Configure custom colors
    await page.goto('/login');
    await page.fill('input[name="UserName"]', 'admin');
    await page.fill('input[name="Password"]', 'Password123!');
    await page.click('button[type="submit"]');

    await page.goto('/Admin/ContentTypes/Edit/BlogPost');
    await page.click('text=QuillJs');

    await page.check('input[name*="Color"]');
    await page.fill('#new-color-input', '#ff0000');
    await page.click('#add-color-btn');
    await page.click('button:has-text("Save")');

    // Create content and check color picker
    await page.goto('/Admin/Contents/ContentItems/BlogPost/Create');
    await page.waitForSelector('.ql-editor');

    // Click color button
    await page.click('.ql-color');

    // Verify custom color appears in picker
    await expect(page.locator('.ql-picker-item[value="#ff0000"]')).toBeVisible();
  });
});
```

## Running Playwright Tests

### Locally

```bash
# Terminal 1: Start Sample.Web
cd samples/Buzz.OrchardCore.Quilljs.Sample.Web
dotnet run

# Terminal 2: Run tests
cd tests/Buzz.OrchardCore.Quilljs.UITests
npm test

# Run in UI mode (recommended for development)
npx playwright test --ui

# Run in headed mode (see browser)
npx playwright test --headed

# Run specific test file
npx playwright test color-picker.spec.ts

# Debug mode
npx playwright test --debug
```

### In CI/CD

See [CI/CD Integration Guide](04-ci-cd-integration.md) for Azure DevOps setup.

## Tips & Best Practices

### Use Page Objects

**File:** `tests/helpers/page-objects.ts`

```typescript
import { Page } from '@playwright/test';

export class QuillFieldSettingsPage {
  constructor(private page: Page) {}

  async goto(contentType: string) {
    await this.page.goto(`/Admin/ContentTypes/Edit/${contentType}`);
    await this.page.click('text=QuillJs');
  }

  async addColor(hexColor: string) {
    await this.page.fill('#new-color-input', hexColor);
    await this.page.click('#add-color-btn');
  }

  async getColorCount() {
    return await this.page.locator('.color-tag-term').count();
  }

  async save() {
    await this.page.click('button:has-text("Save")');
    await this.page.waitForLoadState('networkidle');
  }
}

// Usage in tests:
// const settingsPage = new QuillFieldSettingsPage(page);
// await settingsPage.goto('BlogPost');
// await settingsPage.addColor('#ff0000');
```

### Debugging Failed Tests

```bash
# Run with trace
npx playwright test --trace on

# View trace for failed test
npx playwright show-trace trace.zip

# Take screenshots on failure (already configured)
# Check test-results/ folder
```

## Next Steps

Review the [CI/CD Integration Guide](04-ci-cd-integration.md) to set up automated test execution.
