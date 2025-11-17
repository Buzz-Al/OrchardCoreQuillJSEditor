# Decision: Clean Slate Approach for v2.0

**Date**: 2025-11-17
**Status**: Approved
**Impact**: Breaking Change

---

## Decision

We will **remove flag enums completely** and build a clean, simple list-based data model for the drag-and-drop toolbar builder. This is a **v2.0 breaking change** with no backward compatibility.

## Context

The current implementation (on `feat/settings-update` branch) uses flag enums for button configuration:
- `FormattingButtons`, `BlockButtons`, `ListButtons`, etc. (6 enums)
- Checkbox UI for configuration
- Works well for simple on/off selection
- **Limitation**: No control over button order or group structure

The drag-and-drop UI requires:
- Custom button ordering
- Custom group creation and ordering
- Flexible toolbar structure
- Per-button metadata for UI

## Options Considered

### Option A: Hybrid Approach (Keep Flag Enums for Backward Compatibility)
- Keep flag enums, add new groups structure
- Mark enums as `[Obsolete]`
- Support both formats with migration
- Dual-path logic throughout codebase

**Pros**: Backward compatible, smooth migration
**Cons**: Complex codebase, more code to maintain, confusing for developers

### Option B: Clean Slate (Remove Flag Enums) ✅ **CHOSEN**
- Delete flag enums entirely
- Build simple list-based structure from scratch
- Breaking change, v2.0 release
- No migration needed (feature not released yet)

**Pros**: Simple, clean, easy to maintain, one clear pattern
**Cons**: Breaking change (but feature is unreleased, so minimal impact)

## Rationale

**Why Option B (Clean Slate)?**

1. **Feature Not Released**: The checkbox UI is still on feature branch `feat/settings-update`, not in production. No users will be affected.

2. **Simpler Codebase**: One data model, one pattern, less code to maintain long-term.

3. **Better Fit for Requirements**: List-based structure naturally supports ordering, grouping, and drag-and-drop.

4. **Metadata Separation**: Static registry pattern is cleaner than storing metadata in data model.

5. **Future-Proof**: Easier to extend (add new button types, features) without dealing with enum limitations.

## Data Model

### New Structure

```csharp
public class QuillToolbarConfig
{
    public List<ToolbarGroup> Groups { get; set; } = new();
    public List<string> CustomColors { get; set; } = new();
}

public class ToolbarGroup
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Order { get; set; }
    public List<ToolbarButton> Buttons { get; set; } = new();
}

public class ToolbarButton
{
    public string Type { get; set; }   // "bold", "header", "list"
    public string? Value { get; set; } // "1", "ordered", null
    public int Order { get; set; }
}
```

### Metadata Registry (Separate)

```csharp
public static class ButtonRegistry
{
    public static ButtonMetadata Get(string type);
    public static IEnumerable<ButtonMetadata> All { get; }
    public static IEnumerable<ButtonMetadata> GetByCategory(string category);
}

public record ButtonMetadata(
    string DisplayName,
    string Icon,
    string Category,
    bool RequiresValue,
    string[]? AllowedValues
);
```

## Migration Strategy

**No automatic migration** - this is a v2.0 breaking change.

**Options for users:**
1. **Fresh Start (Recommended)**: Reconfigure toolbars using new drag-and-drop UI
2. **One-Time Script (If Needed)**: Create throwaway migration script to convert existing configs

**Note**: Since feature is unreleased, migration likely not needed.

## Impact

### Files to Delete
- `Settings/QuillButton.cs` (flag enums)
- `Settings/LegacyToolbarMigration.cs` (no longer needed)
- Old checkbox UI sections in view

### Files to Create
- `Settings/ToolbarButton.cs` (new class)
- `Settings/ToolbarGroup.cs` (new class)
- `Settings/ButtonMetadata.cs` (metadata record)
- `Settings/ButtonRegistry.cs` (static registry)

### Files to Update
- `Settings/QuillToolbarConfig.cs` (rewrite to use Groups)
- `ViewModels/QuillSettingsViewModel.cs` (remove enum properties)
- `Settings/HtmlFieldQuillEditorSettingsDriver.cs` (remove enum handling)
- `Views/HtmlFieldQuillEditorSettings.Edit.cshtml` (remove checkbox UI)
- `Migrations.cs` (remove/update old migrations)

## Timeline Impact

**Reduces complexity**:
- Phase 1: Simpler (no backward compatibility)
- Phase 2: Reduced (no migration infrastructure)
- Overall: Saves ~1-2 weeks

## Risks & Mitigations

**Risk**: Users with existing configs (if any) will lose them
**Mitigation**: Feature is unreleased, so minimal risk

**Risk**: Can't revert to checkbox UI easily
**Mitigation**: Keep checkbox code in git history, can restore if needed

**Risk**: Breaking change might confuse contributors
**Mitigation**: Clear documentation, CHANGELOG entry explaining v2.0 changes

## Approval

- [x] Technical Lead: Clean slate approved
- [x] Product Owner: Breaking change acceptable (unreleased feature)
- [x] Developer: Ready to implement

## Next Steps

1. Update spec document (DONE)
2. Update implementation plan (DONE)
3. Begin Phase 1.1: Create new data model classes
4. Delete old flag enums file
5. Update QuillToolbarConfig
6. Proceed with remaining phases

---

**End of Decision Document**
