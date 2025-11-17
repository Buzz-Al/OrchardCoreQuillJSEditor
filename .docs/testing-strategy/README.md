# Testing Strategy for Buzz.OrchardCore.Quilljs

This folder contains comprehensive documentation for implementing automated testing for the Quill.js OrchardCore module.

## Overview

The testing strategy covers three main areas:

1. **C# Unit Tests** - Fast, isolated tests for business logic
2. **C# Integration Tests** - Tests that verify OrchardCore integration
3. **Playwright E2E Tests** - Browser automation tests for UI functionality

## Current Status

✅ **Code Review Complete** - Identified critical areas needing test coverage
⏳ **Tests Not Implemented** - Documentation ready for implementation
📊 **Test Coverage** - Currently 0% (no tests exist)

## Priority Areas for Testing

Based on the code review, these areas have the highest priority:

### Critical (Must Test)
1. **Color Picker Persistence** - Bug fix validation (we fixed the prefix issue)
2. **JSON Generation** - `QuillToolbarConfig.GenerateQuillJson()`
3. **Legacy Migration** - `LegacyToolbarMigration` parser

### High Priority
4. **Settings Driver** - Form binding and model conversion
5. **ViewModel Conversion** - `ToToolbarConfig()` / `FromToolbarConfig()`
6. **Editor Initialization** - Quill.js configuration

### Medium Priority
7. **Flag Enum Logic** - Button configuration
8. **Media Integration** - Image picker (if Media module enabled)
9. **Theme Switching** - Snow vs Bubble themes

## Documentation Structure

### 📖 Getting Started

1. **[Quick Start Guide](06-quick-start.md)** ⭐ START HERE
   - Step-by-step implementation
   - Commands and shortcuts
   - Common troubleshooting

### 🧪 C# Backend Tests

2. **[C# Unit Tests](01-csharp-unit-tests.md)**
   - xUnit setup
   - Test organization
   - Specific test cases with code examples
   - Mock/fixture patterns

3. **[C# Integration Tests](02-csharp-integration-tests.md)**
   - OrchardCore test integration
   - Settings driver tests
   - Migration tests with real data

### 🎭 Frontend Tests

4. **[Playwright E2E Tests](03-playwright-e2e-tests.md)**
   - Playwright setup
   - Page object patterns
   - UI test cases
   - Running against Sample.Web

### 🚀 CI/CD

5. **[CI/CD Integration](04-ci-cd-integration.md)**
   - Azure DevOps pipeline updates
   - Test result publishing
   - Code coverage reporting
   - Test stages and dependencies

### 📊 Test Data

6. **[Test Data & Fixtures](05-test-data-fixtures.md)**
   - Test recipes
   - Legacy migration fixtures
   - Mock data helpers

## Recommended Implementation Order

When you're ready to implement testing, follow this order:

### Phase 1: Foundation (Week 1)
1. Create test project structure
2. Implement 3-5 critical unit tests
   - `QuillToolbarConfig.GenerateQuillJson()` tests
   - `LegacyToolbarMigration` parser tests
3. Set up CI pipeline for unit tests

### Phase 2: Core Coverage (Week 2)
4. Complete unit test suite (~20-30 tests)
5. Add integration tests for settings driver
6. Add integration tests for migrations

### Phase 3: E2E Tests (Week 3)
7. Set up Playwright project
8. Implement critical E2E tests:
   - Color picker persistence validation
   - Settings form tests
   - Editor initialization tests

### Phase 4: Complete & Optimize (Week 4)
9. Add remaining test cases
10. Achieve 80%+ code coverage
11. Optimize test execution time
12. Document test patterns for future contributors

## Quick Reference

### Test Commands

```bash
# Run all C# tests
dotnet test

# Run specific test project
dotnet test tests/Buzz.OrchardCore.Quilljs.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run Playwright tests (requires Sample.Web running)
cd tests/Buzz.OrchardCore.Quilljs.UITests
npm test

# Run Playwright in UI mode
npx playwright test --ui
```

### Project Structure

```
Buzz.OrchardCore.Quilljs/
├── src/
│   └── Buzz.OrchardCore.Quilljs/          # Main module
├── tests/                                   # NEW - Tests go here
│   ├── Buzz.OrchardCore.Quilljs.Tests/     # C# tests
│   └── Buzz.OrchardCore.Quilljs.UITests/   # Playwright tests
├── samples/
│   └── Buzz.OrchardCore.Quilljs.Sample.Web/ # Used by Playwright
└── .docs/
    └── testing-strategy/                    # This folder
```

## Test Metrics Goals

| Metric | Target | Notes |
|--------|--------|-------|
| **Line Coverage** | 80%+ | Focus on critical paths |
| **Branch Coverage** | 70%+ | Test all enum combinations |
| **Unit Test Speed** | <5 seconds | Fast feedback loop |
| **E2E Test Speed** | <3 minutes | Parallel execution |
| **Build Time Impact** | <30 seconds | Tests shouldn't slow CI significantly |

## Benefits of This Testing Approach

### Reliability
- Catch bugs before they reach production
- Prevent regressions when refactoring
- Validate bug fixes (like color picker persistence)

### Documentation
- Tests serve as executable documentation
- Show how to use the module correctly
- Clarify expected behavior

### Confidence
- Safe refactoring
- Safe dependency updates
- Confident code reviews

### Development Speed
- Faster debugging (failing test shows exact problem)
- Faster onboarding (tests show how things work)
- Faster feature development (no manual testing loop)

## Key Technologies

| Technology | Version | Purpose |
|-----------|---------|---------|
| **xUnit** | Latest | C# test framework |
| **Moq** | Latest | Mocking library |
| **FluentAssertions** | Latest | Readable assertions |
| **Playwright** | Latest | Browser automation |
| **Coverlet** | Latest | Code coverage |

## Next Steps

1. Read the [Quick Start Guide](06-quick-start.md)
2. Review specific guides based on what you want to implement
3. Create test projects following the examples
4. Run your first test!

## Questions or Issues?

- Check the [Quick Start Guide](06-quick-start.md) for troubleshooting
- Review example code in each specific guide
- All test code examples are production-ready

## Last Updated

November 2025 - Initial testing strategy documentation
