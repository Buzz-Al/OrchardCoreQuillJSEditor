# Buzz.OrchardCore.Quilljs - Architecture Documentation

## Overview

This module integrates the Quill.js rich text editor (v2.0.2) as an HTML field editor option in OrchardCore CMS. It provides a modern, customizable WYSIWYG editing experience with support for media integration and configurable toolbar options.

## Project Structure

```
/Buzz.OrchardCore.Quilljs/
├── src/Buzz.OrchardCore.Quilljs/
│   ├── Media/                    # Media picker integration
│   ├── Recipes/                  # OrchardCore setup recipes
│   ├── Settings/                 # Field editor settings
│   ├── ViewModels/              # Form view models
│   ├── Views/                   # Razor templates
│   ├── wwwroot/quill/dist/      # Quill.js distribution (modified v2.0.2)
│   ├── Manifest.cs              # Module definition
│   ├── ResourceManagementOptionsConfiguration.cs  # Asset registration
│   └── Startup.cs               # Service registration
└── samples/                     # Sample OrchardCore web application
```

## Core Components

### 1. Module Manifest (`Manifest.cs`)
- **Module ID:** `Buzz.OrchardCore.Quilljs`
- **Dependencies:** `OrchardCore.Html`, `OrchardCore.ContentFields`
- Registers the module with OrchardCore's module system

### 2. Resource Management
- **Quill.js v2.0.2** - Modified build with soft-break support (PR #4565)
- **Themes:** Snow (standard) and Bubble (minimal)
- Resources registered via `ResourceManagementOptionsConfiguration`

### 3. Settings System
Configures toolbar options and theme per HTML field:

**Classes:**
- `HtmlFieldQuillEditorSettings` - Persisted settings (theme + toolbar config)
- `HtmlFieldQuillEditorSettingsDriver` - Handles edit/update operations
- `QuillSettingsViewModel` - Form binding for settings UI

### 4. Field Editor View (`HtmlField-Quill.Edit.cshtml`)
- Renders the Quill editor for HTML field editing
- Initializes Quill with configured toolbar and theme
- Integrates with OrchardCore media picker
- Syncs editor content to hidden textarea for form submission

### 5. Media Integration
- `Media/MediaShapes.cs` - Wraps editor with media picker modal
- Allows inserting images from OrchardCore's media library
- Requires `OrchardCore.Media` feature

## Data Flow

### Settings Configuration Flow
```
User edits field settings
    ↓
HtmlFieldQuillEditorSettings.Edit.cshtml (settings UI)
    ↓
QuillSettingsViewModel (form data)
    ↓
HtmlFieldQuillEditorSettingsDriver.UpdateAsync() (processes form)
    ↓
HtmlFieldQuillEditorSettings (persisted to content type definition)
```

### Content Editing Flow
```
User edits HTML field
    ↓
HtmlField-Quill.Edit.cshtml (renders editor)
    ↓
Quill initialization with toolbar config
    ↓
User makes edits in Quill editor
    ↓
Quill.getSemanticHTML() syncs to hidden textarea
    ↓
Form submission saves HTML content
```

## Historical Implementation (Pre-Refactor)

### Security Concerns

**Issue 1: JSON Injection in Editor Initialization**
```cshtml
<!-- HtmlField-Quill.Edit.cshtml:55 -->
const toolbarSettings = @Html.Raw(quillSettings.ToolbarOptions);
```
- Directly injected user-provided JSON string into JavaScript
- Potential XSS if malicious JSON/JavaScript entered in settings
- Similar to OrchardCore's Trumbowyg implementation (accepted pattern for admin-only)

**Issue 2: No Input Validation**
- `ToolbarOptions` accepted any string without JSON validation
- Could cause JavaScript errors if invalid JSON entered

**Issue 3: Poor User Experience**
- Users had to manually edit complex JSON in CodeMirror editor
- High barrier to entry for non-technical users
- Error-prone (syntax errors, missing commas, etc.)

### Why This Was "Acceptable"
- Admin-only context (content type editors have elevated permissions)
- Consistent with OrchardCore core modules (Trumbowyg uses same pattern)
- OrchardCore treats this similar to template editing (allows arbitrary code)

### Why We're Changing It Anyway
1. **Better UX** - Visual checkbox interface is far more user-friendly
2. **Type Safety** - Strongly-typed configuration prevents invalid values
3. **Defense in Depth** - Eliminates attack vector even in admin context
4. **Maintainability** - Structured data easier to work with than JSON strings

## New Architecture (Post-Refactor)

### Design Principles

1. **Type Safety** - Use C# types instead of raw JSON strings
2. **Security** - Use data attributes with `JSON.parse()` instead of `@Html.Raw()`
3. **User Experience** - Visual UI instead of JSON editing
4. **Consistency** - Follow OrchardCore UI patterns (Bootstrap 5.3, native color inputs)

### New Data Model

```
QuillToolbarConfig (new)
├── Formatting: FormattingButtons (flags enum)
├── Blocks: BlockButtons (flags enum)
├── Lists: ListButtons (flags enum)
├── Media: MediaButtons (flags enum)
├── Styles: StyleButtons (flags enum)
├── Advanced: AdvancedButtons (flags enum)
└── CustomColors: List<string> (hex color codes)
```

### Settings Persistence

**Old:**
```csharp
public class HtmlFieldQuillEditorSettings
{
    public QuillTheme Theme { get; set; }
    public string ToolbarOptions { get; set; }  // Raw JSON string
}
```

**New:**
```csharp
public class HtmlFieldQuillEditorSettings
{
    public QuillTheme Theme { get; set; }
    public QuillToolbarConfig ToolbarConfig { get; set; }  // Strongly-typed

    /// <summary>
    /// Generates Quill-compatible toolbar configuration JSON
    /// </summary>
    public string GenerateQuillJson() { ... }
}
```

### Secure Data Passing

**Old (Vulnerable):**
```cshtml
<script>
const toolbarSettings = @Html.Raw(quillSettings.ToolbarOptions);
</script>
```

**New (Secure):**
```cshtml
<div id="editor" data-toolbar='@quillSettings.GenerateQuillJson()'></div>
<script>
const element = document.getElementById('editor');
const toolbarSettings = JSON.parse(element.dataset.toolbar);
</script>
```

**Why this is better:**
- HTML attributes are automatically HTML-encoded by Razor
- `JSON.parse()` safely parses the JSON client-side
- No raw code injection possible

## Key Design Decisions

### 1. Native HTML5 Color Inputs
**Decision:** Use `<input type="color">` instead of JavaScript color picker library

**Rationale:**
- Matches OrchardCore core pattern (TextField-Color.Edit.cshtml)
- No external dependencies needed
- Native OS color picker provides consistent UX
- Bootstrap 5.3 provides built-in styling via `.form-control`

### 2. Checkbox UI Instead of Drag-and-Drop
**Decision:** Start with checkbox interface, not full drag-and-drop builder

**Rationale:**
- 80% of users need simple button enable/disable
- Much simpler to implement and maintain
- Consistent with OrchardCore's preference for simplicity
- Can add drag-and-drop later if needed

### 3. No Backwards Compatibility
**Decision:** Migrate all existing configurations to new format

**Rationale:**
- Cleaner codebase without legacy code paths
- Simplifies testing and maintenance
- Migration is one-time, automatic during update
- Project is likely low adoption (internal or limited use)

### 4. Flags Enums for Button Groups
**Decision:** Use `[Flags]` enums for button categories

**Rationale:**
- Compact storage (bitwise operations)
- Easy to check if button is enabled: `config.Formatting.HasFlag(FormattingButtons.Bold)`
- Type-safe with IntelliSense support
- Natural mapping to checkbox groups

## Dependencies

### NuGet Packages
- **OrchardCore.*** - v2.1.5 (OrchardCore framework)
- **Microsoft.Extensions.Http** - HTTP client factory
- **Microsoft.AspNetCore.Rewrite** - URL rewriting
- ~~**FluentExcel**~~ - Unused, removed during refactor

### Frontend Assets
- **Quill.js v2.0.2** - Custom build with soft-break support
- **Bootstrap 5.3.3** - Via OrchardCore's TheAdmin theme
- **CodeMirror** - For advanced JSON editing (removed in refactor)

### OrchardCore Features
- `OrchardCore.Html` - HTML field support
- `OrchardCore.ContentFields` - Content field infrastructure
- `OrchardCore.Media` - Media picker integration (optional)

## Extension Points

### Adding New Toolbar Buttons
1. Add to appropriate `QuillButton` enum
2. Update `QuillToolbarConfig.GenerateQuillJson()` to include in output
3. Add checkbox to settings UI view
4. Update ViewModel with property for new button

### Custom Themes
1. Add to `QuillTheme` enum
2. Register CSS in `ResourceManagementOptionsConfiguration`
3. Update theme selection in settings UI

### Advanced Configuration
For features not covered by checkboxes (e.g., custom fonts, specific header levels), users would need to:
1. Use recipe JSON for initial setup
2. Or extend the UI with additional configuration options

## Future Enhancements

### Potential Improvements
- Drag-and-drop button ordering
- Button grouping configuration
- Custom font selection UI
- Preset toolbar templates (minimal/standard/full)
- Import/export toolbar configurations
- Live preview of toolbar in settings UI

### Migration Path
If drag-and-drop UI is needed:
1. Keep existing data model (already supports ordering)
2. Add Vue.js component for visual builder
3. Enhance `QuillToolbarConfig` with group/order properties
4. Maintain checkbox fallback for simple cases

## Testing Strategy

### Manual Testing Checklist
- [ ] Settings UI renders correctly
- [ ] Checkbox selections save properly
- [ ] Color pickers add/remove colors
- [ ] Editor initializes with correct toolbar
- [ ] All toolbar buttons function correctly
- [ ] Media picker integration works
- [ ] Content saves and loads correctly
- [ ] Recipe configurations import properly

### Security Testing
- [ ] Verify no `@Html.Raw()` injection of user data
- [ ] Test with malicious HTML in content
- [ ] Test with special characters in color codes
- [ ] Verify data attributes are HTML-encoded

## Maintenance Notes

### Quill.js Updates
The module uses a **custom Quill.js build** with soft-break support (PR #4565). When updating Quill:
1. Check if soft-break feature is merged upstream
2. If yes, switch to official build
3. If no, rebuild custom version from fork
4. Update version in `ResourceManagementOptionsConfiguration`

### OrchardCore Updates
When updating OrchardCore:
1. Update all `OrchardCore.*` package references together
2. Test settings UI (API changes in display drivers are rare but possible)
3. Verify Bootstrap compatibility (OrchardCore uses Bootstrap 5.3+)

## References

- [Quill.js Documentation](https://quilljs.com/docs/)
- [Quill Soft-Break PR #4565](https://github.com/slab/quill/pull/4565)
- [OrchardCore Documentation](https://docs.orchardcore.net/)
- [OrchardCore TextField-Color Implementation](https://github.com/OrchardCMS/OrchardCore)
- [Bootstrap 5.3 Documentation](https://getbootstrap.com/docs/5.3/)

---

**Document Version:** 1.0
**Last Updated:** 2025-11-14
**Refactor Status:** In Progress (feat/settings-update branch)
