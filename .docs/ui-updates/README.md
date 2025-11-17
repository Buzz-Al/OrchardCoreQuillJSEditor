# Drag-and-Drop Toolbar Builder - Documentation

## Overview

This directory contains all planning and specification documents for the v2.0 Drag-and-Drop Toolbar Builder feature for the Buzz.OrchardCore.Quilljs module.

## Key Documents

### 1. [drag-and-drop-toolbar-spec.md](./drag-and-drop-toolbar-spec.md)
**Comprehensive technical specification** covering:
- Requirements (functional and non-functional)
- UI/UX design with wireframes
- Technical architecture and data model
- Testing strategy
- Security considerations
- Documentation requirements

**Read this to**: Understand what we're building and why

---

### 2. [implementation-plan.md](./implementation-plan.md)
**Detailed 13-week implementation plan** broken into:
- 12 phases with small, manageable increments
- Specific files to modify/create
- Task checklists for each increment
- Time estimates and review checkpoints

**Read this to**: Know how to build it step-by-step

---

### 3. [DECISION-CLEAN-SLATE.md](./DECISION-CLEAN-SLATE.md)
**Architectural decision record** explaining:
- Why we're removing flag enums
- Clean slate approach (breaking change)
- Comparison of options considered
- Impact and rationale

**Read this to**: Understand the key architectural decision

---

## Quick Start

### For Implementers

1. **Read** the [decision document](./DECISION-CLEAN-SLATE.md) (5 min)
2. **Skim** the [spec](./drag-and-drop-toolbar-spec.md) sections 1-4 (15 min)
3. **Follow** the [implementation plan](./implementation-plan.md) phase by phase
4. **Track** progress using the todo list in Claude Code

### For Reviewers

1. **Read** the [decision document](./DECISION-CLEAN-SLATE.md)
2. **Review** data model in spec section 4.1
3. **Check** implementation plan Phase 1 tasks
4. **Validate** that clean slate approach makes sense

### For Stakeholders

1. **Read** spec sections 1-2 (Executive Summary, Requirements)
2. **View** wireframes in spec section 3
3. **Review** timeline in implementation plan summary
4. **Approve** breaking change decision

---

## Key Decision: Clean Slate Approach

**TL;DR**: We're removing the flag enums and checkbox UI completely, building a simple list-based structure from scratch. This is a v2.0 breaking change, but since the feature isn't released yet, it has no impact on users.

### Old Approach (Checkbox UI)
```csharp
// Flag enums - no ordering, rigid structure
public enum FormattingButtons { Bold, Italic, Underline, ... }
config.Formatting = FormattingButtons.Bold | FormattingButtons.Italic;
```

### New Approach (Drag-and-Drop UI)
```csharp
// List-based - full control over ordering and grouping
public class ToolbarButton {
    string Type;   // "bold", "italic", "header"
    string? Value; // "1", "ordered", null
    int Order;
}

config.Groups[0].Buttons.Add(new ToolbarButton("bold", null, 0));
config.Groups[0].Buttons.Add(new ToolbarButton("italic", null, 1));
```

**Benefits**:
- ✅ Simpler codebase (one pattern, not two)
- ✅ Perfect fit for drag-and-drop requirements
- ✅ Easier to maintain long-term
- ✅ No backward compatibility complexity

---

## Data Model

### Configuration (Saved)
```csharp
QuillToolbarConfig
├── Groups: List<ToolbarGroup>
│   ├── Id, Name, Order
│   └── Buttons: List<ToolbarButton>
│       └── Type, Value, Order
└── CustomColors: List<string>
```

### Metadata (Static Registry)
```csharp
ButtonRegistry
└── Dictionary<string, ButtonMetadata>
    └── DisplayName, Icon, Category, RequiresValue, AllowedValues
```

**Separation**: Configuration (user's choices) vs. Metadata (static button info)

---

## Implementation Status

### Current Phase: Phase 1.1
**Status**: Planning complete, ready to start implementation

### Completed
- ✅ Specification document
- ✅ Implementation plan
- ✅ Architectural decision
- ✅ Clean slate approach approved

### Next Steps
1. Create `ToolbarButton.cs` and `ToolbarGroup.cs`
2. Delete `QuillButton.cs` (flag enums)
3. Create `ButtonMetadata.cs` and `ButtonRegistry.cs`
4. Update `QuillToolbarConfig.cs` to use Groups
5. Continue with implementation plan Phase 1

---

## Timeline

| Phase | Duration | Key Deliverable |
|-------|----------|-----------------|
| Phase 1 | Week 1 | Data model foundation |
| Phase 2 | Week 2 | Clean up old code |
| Phase 3 | Week 3 | Backend & ViewModels |
| Phase 4 | Week 4 | Static UI layout |
| Phase 5 | Week 5 | JavaScript foundation |
| Phase 6 | Week 6-7 | Drag-and-drop working |
| Phase 7 | Week 8 | Live preview |
| Phase 8 | Week 9 | Preset templates |
| Phase 9 | Week 10 | Validation & errors |
| Phase 10 | Week 11 | Polish & optimization |
| Phase 11 | Week 12 | Testing & docs |
| Phase 12 | Week 13 | Deployment |

**Total**: ~13 weeks (3 months)

---

## Testing Strategy

### Unit Tests
- Data model classes
- Button registry
- JSON generation
- Factory methods
- ViewModel conversions

### Integration Tests
- Full configuration workflow
- Form submission
- Preset loading
- Validation

### Manual Testing
- Browser compatibility (Chrome, Firefox, Safari, Edge)
- Accessibility (keyboard navigation, screen readers)
- Responsive design (desktop, tablet)
- Performance (large toolbars)

**Target**: 80%+ code coverage

---

## Questions?

### Who to ask:
- **Architecture questions**: Review [DECISION-CLEAN-SLATE.md](./DECISION-CLEAN-SLATE.md)
- **Implementation questions**: Check [implementation-plan.md](./implementation-plan.md)
- **Requirements questions**: See [drag-and-drop-toolbar-spec.md](./drag-and-drop-toolbar-spec.md)

### Common Questions

**Q: Why remove flag enums?**
A: They don't support button ordering or custom grouping, which are core requirements for drag-and-drop. List-based structure is simpler and more flexible.

**Q: What about backward compatibility?**
A: Not needed - feature is unreleased (still on feature branch). This is a clean v2.0.

**Q: Can we revert to checkbox UI?**
A: Yes, it's in git history. But drag-and-drop is the goal.

**Q: How long will this take?**
A: ~13 weeks for full implementation, but it's broken into small increments so we'll have working code throughout.

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-11-17 | Initial spec, plan, and decision docs created |

---

**Last Updated**: 2025-11-17
