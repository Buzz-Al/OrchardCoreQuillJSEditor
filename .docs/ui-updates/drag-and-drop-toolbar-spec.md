# **Quill Toolbar Builder UI - Specification Document**

## **Document Information**
- **Feature:** Visual Drag-and-Drop Toolbar Builder for Quill.js Editor
- **Target:** Buzz.OrchardCore.Quilljs Module
- **Version:** 2.0.0 (Breaking Change)
- **Date:** 2025-11-17
- **Status:** Draft for Review

---

## **1. Executive Summary**

### **1.1 Overview**
Replace the existing checkbox-based toolbar configuration UI with a modern drag-and-drop visual builder that allows administrators to construct custom toolbars by dragging buttons from an available palette into customizable groups, with a live preview of the resulting Quill editor toolbar.

### **1.2 Goals**
1. **Intuitive Configuration**: Users visually build toolbars instead of checking boxes
2. **Full Customization**: Complete control over button order, grouping, and toolbar structure
3. **Live Feedback**: Real-time preview shows exactly what editors will see
4. **Maintain Security**: Preserve existing XSS-safe data attribute pattern
5. **Clean Data Model**: Simple, list-based structure for groups and buttons
6. **Breaking Change**: This is v2.0 - replaces checkbox UI and flag enum data model completely

### **1.3 User Personas**
- **Primary**: Site administrators configuring HTML field settings in OrchardCore
- **Technical Level**: Non-technical to moderately technical users
- **Frequency**: Occasional (during initial setup or when adding new content types)

### **1.4 User Requirements**
Based on stakeholder input:
- ✅ Replace checkbox UI completely with drag-and-drop
- ✅ Full control over toolbar groups (create, delete, reorder freely)
- ✅ Live preview showing actual Quill.js toolbar
- ✅ Include preset templates (Minimal/Standard/Full)
- ✅ Support drag buttons between groups and reorder within groups

---

## **2. Requirements**

### **2.1 Functional Requirements**

#### **FR-1: Button Palette**
- Display all available Quill toolbar buttons in a categorized "palette" area
- Categories: Formatting, Blocks, Lists, Media, Styles, Advanced
- Show button icon/label for each available button
- Visual indication of which buttons are currently in use vs. available
- Search/filter buttons by name or category

#### **FR-2: Drag-and-Drop Interface**
- Drag buttons from palette into toolbar groups
- Drag buttons between different groups
- Reorder buttons within the same group
- Drag buttons back to palette to remove from toolbar
- Visual feedback during drag (ghost element, drop zones highlighted)
- Touch-screen support for tablet users

#### **FR-3: Toolbar Group Management**
- Create new empty groups with "Add Group" button
- Assign custom names/labels to groups (optional, for user reference only)
- Reorder groups by dragging group headers
- Delete groups (returns buttons to palette)
- Minimum 1 group required (cannot delete last group)
- Visual separators between groups in preview

#### **FR-4: Live Preview**
- Show real-time rendered Quill.js toolbar using actual Quill theme (Snow or Bubble)
- Preview updates immediately as buttons are added/removed/reordered
- Preview reflects both Snow and Bubble themes (switchable)
- Preview shows button grouping with visual separators
- Scrollable preview container if toolbar becomes wide

#### **FR-5: Preset Templates**
- Quick-start buttons: "Minimal", "Standard", "Full"
- Clicking preset replaces current configuration with template
- Confirmation dialog before replacing existing configuration
- Presets match existing `CreateMinimal()`, `CreateStandard()`, `CreateFull()` methods

#### **FR-6: Custom Colors**
- Preserve existing color palette picker functionality
- Add custom hex colors for text/background color buttons
- Remove colors from custom palette
- Visual preview of color swatches
- Color picker integration (HTML5 input type="color")

#### **FR-7: Theme Selection**
- Preserve existing Snow/Bubble theme dropdown
- Preview updates to show selected theme's appearance
- Theme affects preview rendering but not button availability

#### **FR-8: Validation & Feedback**
- Warn if toolbar is completely empty
- Indicate invalid configurations (e.g., color button without palette)
- Show tooltips explaining button functions
- Success message on save
- Error messages for validation failures

### **2.2 Non-Functional Requirements**

#### **NFR-1: Performance**
- Drag operations respond within 16ms (60fps)
- Preview renders within 200ms of configuration change
- No noticeable lag when dragging with 30+ buttons configured
- Debounce rapid changes to prevent excessive re-rendering

#### **NFR-2: Accessibility**
- Keyboard navigation for all drag-and-drop operations
- Screen reader announcements for button moves
- ARIA labels on all interactive elements
- Keyboard shortcuts: Arrow keys (move), Space (select), Enter (activate)
- Focus management during drag operations

#### **NFR-3: Browser Compatibility**
- Chrome 90+ (primary target, OrchardCore admin default)
- Firefox 88+
- Safari 14+
- Edge 90+
- No Internet Explorer support (OrchardCore requirement)

#### **NFR-4: Responsive Design**
- Full functionality on desktop (1280px+)
- Simplified mobile layout for tablets (768px - 1279px)
- Touch-optimized drag handles and drop zones
- Collapsed palette on mobile (expandable)

#### **NFR-5: Security**
- Maintain existing data attribute + JSON.parse() pattern
- No `@Html.Raw()` injection of user data
- Server-side validation of all toolbar configurations
- HTML encode all user-provided text (group names, if added)

#### **NFR-6: Maintainability**
- Minimal external dependencies (prefer vanilla JS or lightweight libraries)
- Clear separation of concerns (data layer, UI layer, rendering layer)
- Comprehensive inline documentation
- Follow existing codebase patterns (Bootstrap 5, OrchardCore conventions)

---

## **3. User Interface Design**

### **3.1 Layout Structure**

```
┌─────────────────────────────────────────────────────────────┐
│  Quill Editor Settings                                       │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  [Theme Dropdown: Snow ▼]  [🎨 Manage Custom Colors]        │
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Preset Templates                                     │   │
│  │  [ Minimal ]  [ Standard ]  [ Full ]                 │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
│  ┌──────────────────────┬────────────────────────────────┐ │
│  │  Available Buttons   │  Toolbar Builder               │ │
│  │  (Palette)           │                                │ │
│  ├──────────────────────┤                                │ │
│  │                      │  ┌──────────────────────────┐ │ │
│  │  [Search: ____]      │  │  Group 1: Formatting     │ │ │
│  │                      │  │  ┌────┬────┬────┬────┐   │ │ │
│  │  📝 Formatting       │  │  │ B  │ I  │ U  │ S  │   │ │ │
│  │  ☐ Bold              │  │  └────┴────┴────┴────┘   │ │ │
│  │  ☐ Italic            │  │  [+ Add Button] [× Delete] │ │
│  │  ☑ Underline  (used) │  └──────────────────────────┘ │ │
│  │  ☐ Strike            │                                │ │
│  │  ☐ Code              │  ┌──────────────────────────┐ │ │
│  │                      │  │  Group 2: Lists          │ │ │
│  │  📦 Blocks           │  │  ┌────┬────┬────┐        │ │ │
│  │  ☐ Blockquote        │  │  │ 1. │ •  │ ☑  │        │ │ │
│  │  ☐ Code Block        │  │  └────┴────┴────┘        │ │ │
│  │  ☐ Header 1          │  │  [+ Add Button] [× Delete] │ │
│  │  ☐ Header 2          │  └──────────────────────────┘ │ │
│  │                      │                                │ │
│  │  📋 Lists            │  [+ Add New Group]             │ │
│  │  ...                 │                                │ │
│  │                      │                                │ │
│  └──────────────────────┴────────────────────────────────┘ │
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Live Preview                                         │   │
│  │  ┌─────────────────────────────────────────────────┐ │   │
│  │  │ [B][I][U][S] │ [1.][•][☑]                       │ │   │
│  │  └─────────────────────────────────────────────────┘ │   │
│  │  This is how the toolbar will appear in editors      │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
│  [Cancel]  [Save Settings]                                   │
└─────────────────────────────────────────────────────────────┘
```

### **3.2 Component Specifications**

#### **3.2.1 Button Palette (Left Panel)**

**Visual Design:**
- Fixed-width left sidebar (300px)
- Scrollable if content exceeds viewport
- Categorized accordion sections (expandable/collapsible)
- Each button shows:
  - Icon or text label (e.g., "B" for Bold, "1." for numbered list)
  - Full descriptive name (e.g., "Bold", "Numbered List")
  - Checkbox or indicator showing if button is "in use"
  - Drag handle icon (⋮⋮) on hover

**Interactions:**
- Click category header to expand/collapse
- Click-and-drag button to toolbar area
- Buttons "in use" show visual distinction (grayed out, checkmark, etc.)
- Search box filters buttons by name across all categories
- Empty state message if search returns no results

**Categories:**
1. **📝 Formatting** - Bold, Italic, Underline, Strike, Code
2. **📦 Blocks** - Blockquote, Code Block, Header 1, Header 2
3. **📋 Lists** - Ordered, Bullet, Checklist
4. **🖼️ Media** - Link, Image, Video, Formula
5. **🎨 Styles** - Text Color, Background, Font, Size, Alignment
6. **⚙️ Advanced** - Superscript, Subscript, Indent, Outdent, Text Direction, Remove Formatting

#### **3.2.2 Toolbar Builder (Right Panel)**

**Visual Design:**
- Flexible-width right panel (grows to fill remaining space)
- Scrollable vertically if groups exceed viewport
- Each group is a card with:
  - Drag handle (⋮⋮) on left for reordering groups
  - Group name input (optional, placeholder: "Group 1", "Group 2", etc.)
  - Drop zone for buttons (visual highlight on hover during drag)
  - Grid/flex layout showing buttons horizontally
  - "Add Button" affordance (dashed border area or button)
  - Delete group button (⨯) on right

**Interactions:**
- Drag group by its handle to reorder
- Drag buttons from palette into group drop zone
- Drag buttons within group to reorder
- Drag buttons between groups
- Drag buttons back to palette (or to trash icon) to remove
- Click "Add Button" shows filtered palette or hotspot for drag
- Click delete (⨯) removes group, returns buttons to palette (with confirmation if group has buttons)
- Groups collapse/expand to save space (optional)

**Visual States:**
- **Default**: White background, subtle border
- **Drag Over**: Highlighted border (blue/green), background tint
- **Empty**: Dashed border, "Drag buttons here" placeholder text
- **Has Buttons**: Solid border, buttons displayed in flex row

#### **3.2.3 Live Preview (Bottom Panel)**

**Visual Design:**
- Full-width section below builder
- Collapsible accordion (starts expanded)
- Shows actual rendered Quill.js toolbar using selected theme
- Bordered container simulating editor appearance
- Theme switcher toggle (Snow/Bubble) if theme affects toolbar significantly

**Content:**
- Renders real Quill `<div class="ql-toolbar">` with configured buttons
- Uses Quill's default icons and styling
- Shows button grouping with `<span class="ql-formats">` separators
- Non-interactive (preview only, buttons don't function)
- Updates in real-time as toolbar configuration changes

**Interactions:**
- Expand/collapse preview section
- Switch theme to preview different appearances
- Scroll horizontally if toolbar is very wide
- Tooltip: "This is a preview - buttons are not functional"

#### **3.2.4 Preset Templates**

**Visual Design:**
- Horizontal row of buttons above the builder
- Three buttons: "Minimal", "Standard", "Full"
- Icon + text labels
- Equal width, secondary button style

**Interactions:**
- Click button shows confirmation: "Replace current toolbar with [Preset] template?"
- Confirm: loads preset configuration into builder
- Cancel: no changes
- After loading: user can further customize from preset

**Preset Definitions** (from QuillToolbarConfig.cs):

**Minimal:**
- Formatting: Bold, Italic, Underline
- Lists: Ordered, Bullet
- 2 groups total

**Standard:**
- Formatting: Bold, Italic, Underline, Strike
- Blocks: Blockquote, Code Block
- Lists: Ordered, Bullet, Checklist
- Media: Link, Image
- Styles: Text Color, Background
- 5 groups total

**Full:**
- All buttons enabled
- Organized into 12 groups (current implementation)

#### **3.2.5 Custom Colors Modal**

**Visual Design:**
- Button: "🎨 Manage Custom Colors" in header area
- Opens Bootstrap modal dialog
- Shows current color palette as pill-style tags
- HTML5 color picker input
- "Add Color" button
- Remove (×) button on each color

**Interactions:**
- Same as current implementation (lines 151-294 in HtmlFieldQuillEditorSettings.Edit.cshtml)
- Pick color → Add to list → Updates color/background buttons in toolbar
- Remove color → Updates list → Re-indexes ASP.NET form bindings
- Close modal → Returns to builder
- Changes persist when settings are saved

### **3.3 Drag-and-Drop Interactions**

#### **3.3.1 Drag Sources**
1. **Palette → Toolbar**: Create new button instance in group
2. **Toolbar → Toolbar**: Move button within/between groups
3. **Toolbar → Palette**: Remove button from toolbar (visual only, doesn't delete from palette)

#### **3.3.2 Visual Feedback**

**During Drag:**
- **Ghost Element**: Semi-transparent clone follows cursor
- **Source Element**: Grayed out or hidden (depends on UX preference)
- **Valid Drop Zones**: Highlighted border (green), background tint
- **Invalid Drop Zones**: No highlight or red border
- **Insertion Point**: Vertical line indicator showing where button will be placed

**After Drop:**
- Smooth animation: button slides into position
- "In use" indicator updates in palette
- Preview refreshes to show new configuration
- Focus moves to dropped button (for accessibility)

**Undo/Redo** (Nice-to-Have):
- Browser-native undo (Ctrl+Z) after configuration changes
- "Reset to Last Saved" button to revert all changes

#### **3.3.3 Keyboard Navigation**

- **Tab**: Move focus between buttons/groups/controls
- **Arrow Keys**: Move focus within group or between groups
- **Space**: Select/deselect button (enters drag mode)
- **Arrow Keys (in drag mode)**: Move button position
- **Enter**: Drop button in new position
- **Escape**: Cancel drag operation, return to original position
- **Delete**: Remove focused button from toolbar

### **3.4 Responsive Behavior**

#### **Desktop (1280px+)**
- Side-by-side palette and builder (layout shown above)
- Live preview full-width below
- All features available

#### **Tablet (768px - 1279px)**
- Palette collapses to left drawer (hamburger menu toggle)
- Builder takes full width
- Live preview below builder
- Touch-optimized drag handles (larger hit areas)

#### **Mobile (<768px) - Out of Scope**
- OrchardCore admin typically not used on mobile
- Show warning: "Use desktop for toolbar configuration"
- Or: simplified checkbox fallback (if absolutely needed)

---

## **4. Technical Architecture**

### **4.1 Data Model**

#### **4.1.1 Clean, List-Based Structure**

The data model is designed to be simple, storing only the user's toolbar configuration without metadata:

```csharp
public class QuillToolbarConfig
{
    public List<ToolbarGroup> Groups { get; set; } = new();
    public List<string> CustomColors { get; set; } = new();

    // Factory methods for preset configurations
    public static QuillToolbarConfig CreateMinimal() { /* ... */ }
    public static QuillToolbarConfig CreateStandard() { /* ... */ }
    public static QuillToolbarConfig CreateFull() { /* ... */ }
}

public class ToolbarGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); // For UI binding
    public string Name { get; set; } = string.Empty; // Optional user label
    public int Order { get; set; } // Position in toolbar
    public List<ToolbarButton> Buttons { get; set; } = new();
}

public class ToolbarButton
{
    public string Type { get; set; } // "bold", "header", "list", etc.
    public string? Value { get; set; } // For parameterized buttons: "1", "ordered", null
    public int Order { get; set; } // Position within group
}
```

**Design Principles:**
- **Minimal**: Only stores what's needed (configuration, not metadata)
- **Flexible**: Buttons can be in any order, in any group
- **Serializable**: Simple structure for JSON/database storage
- **Type-safe**: String-based types validated against registry

#### **4.1.2 Button Metadata Registry**

Button metadata (display names, icons, categories) is stored separately in a static registry:

```csharp
public static class ButtonRegistry
{
    private static readonly Dictionary<string, ButtonMetadata> _buttons = new()
    {
        ["bold"] = new("Bold", "B", "Formatting", false),
        ["italic"] = new("Italic", "I", "Formatting", false),
        ["header"] = new("Header", "H", "Blocks", true, new[] { "1", "2" }),
        ["list"] = new("List", "•", "Lists", true, new[] { "ordered", "bullet", "check" }),
        // ... all 22 button types
    };

    public static ButtonMetadata Get(string type) => _buttons[type];
    public static IEnumerable<ButtonMetadata> All => _buttons.Values;
    public static IEnumerable<ButtonMetadata> GetByCategory(string category)
        => _buttons.Values.Where(b => b.Category == category);
}

public record ButtonMetadata(
    string DisplayName,
    string Icon,
    string Category,
    bool RequiresValue,
    string[]? AllowedValues = null
);
```

**Benefits:**
- Metadata defined once, used everywhere (UI, validation, localization)
- Data model stays lean (only user choices, not static metadata)
- Easy to add new button types or update metadata
- Clear separation: configuration (saved) vs. metadata (static)

#### **4.1.3 Migration Strategy**

**Breaking Change - No Automatic Migration:**

This is a v2.0 release with a new data model. Existing configurations using flag enums will need to be recreated:

**Option 1: Fresh Start (Recommended)**
- Users reconfigure toolbars using new drag-and-drop UI
- Existing checkbox configurations are discarded
- Quick and clean

**Option 2: Migration Tool (If Needed)**
- Create one-time migration script that reads old JSON configs
- Convert flag enums → groups structure
- Run manually before deploying v2.0
- Code not included in production (one-time use only)

**Note:** Since this feature hasn't been released to production yet (still on feature branch), migration may not be necessary.

### **4.2 JSON Generation**

#### **4.2.1 Updated GenerateQuillJson() Method**

```csharp
public string GenerateQuillJson()
{
    var toolbarGroups = new List<object>();

    foreach (var group in Groups.OrderBy(g => g.Order))
    {
        var groupArray = new List<object>();

        foreach (var button in group.Buttons.OrderBy(b => b.Order))
        {
            object buttonConfig = button.Type switch
            {
                // Simple string buttons
                "bold" or "italic" or "underline" or "strike" or "code"
                or "blockquote" or "code-block" or "link" or "image"
                or "video" or "formula" or "clean" => button.Type,

                // Parameterized buttons (object notation)
                "header" => new { header = int.Parse(button.Value) },
                "list" => new { list = button.Value }, // "ordered", "bullet", "check"
                "script" => new { script = button.Value }, // "sub", "super"
                "indent" => new { indent = button.Value }, // "-1", "+1"
                "direction" => new { direction = button.Value }, // "rtl"

                // Buttons with arrays
                "color" => new { color = CustomColors.Any() ? CustomColors.ToArray() : Array.Empty<string>() },
                "background" => new { background = CustomColors.Any() ? CustomColors.ToArray() : Array.Empty<string>() },
                "font" => new { font = Array.Empty<string>() }, // Empty = use defaults
                "size" => new { size = new object[] { "small", false, "large", "huge" } },
                "align" => new { align = Array.Empty<string>() }, // Empty = all alignments

                _ => button.Type // Fallback
            };

            groupArray.Add(buttonConfig);
        }

        if (groupArray.Any())
            toolbarGroups.Add(groupArray);
    }

    return JsonSerializer.Serialize(toolbarGroups, new JsonSerializerOptions { WriteIndented = false });
}
```

**Key Changes:**
- Iterate over `Groups` instead of flag enums
- Respect `Order` property for groups and buttons
- Same output format (array of arrays) for Quill.js compatibility
- Special handling for compound buttons (indent, script) now explicit

### **4.3 Frontend Architecture**

#### **4.3.1 Technology Stack**

**Recommended: SortableJS + Vanilla JavaScript**

**SortableJS** (https://github.com/SortableJS/Sortable):
- ✅ Lightweight (~15KB gzipped)
- ✅ No framework dependencies
- ✅ Native HTML5 drag-and-drop
- ✅ Touch support built-in
- ✅ Nested group support
- ✅ Excellent browser compatibility
- ✅ Active maintenance, MIT license
- ✅ Used by Vue.js, React, Angular communities

**Alternative: Alpine.js** (if wanting reactivity):
- Lightweight (15KB), Vue-like syntax
- Good fit for OrchardCore's Razor-heavy approach
- Reactive data binding simplifies state management

**Keep jQuery** for:
- Bootstrap modal interactions (custom colors dialog)
- OrchardCore integration points (`$(document).trigger(...)`)
- Existing patterns consistency

#### **4.3.2 JavaScript Module Structure**

```javascript
// quill-toolbar-builder.js

class QuillToolbarBuilder {
    constructor(container, options) {
        this.container = container;
        this.options = options;
        this.state = {
            groups: [],
            availableButtons: [],
            customColors: []
        };

        this.init();
    }

    init() {
        this.loadState();
        this.initDragDrop();
        this.initPreview();
        this.bindEvents();
    }

    loadState() {
        // Parse server-side data from data attributes
        const configJson = this.container.getAttribute('data-toolbar-config');
        this.state = JSON.parse(configJson);
    }

    initDragDrop() {
        // Initialize SortableJS on palette and groups
        this.initPalette();
        this.initGroups();
    }

    initPalette() {
        // SortableJS: drag from palette to groups
        new Sortable(this.paletteElement, {
            group: { name: 'buttons', pull: 'clone', put: false },
            sort: false, // Can't reorder palette
            onEnd: this.handlePaletteDrag.bind(this)
        });
    }

    initGroups() {
        // SortableJS: reorder groups
        new Sortable(this.groupsContainer, {
            handle: '.group-drag-handle',
            animation: 150,
            onEnd: this.handleGroupReorder.bind(this)
        });

        // SortableJS: drag buttons within/between groups
        this.state.groups.forEach((group, index) => {
            const groupElement = this.getGroupElement(index);
            new Sortable(groupElement, {
                group: 'buttons',
                animation: 150,
                onEnd: this.handleButtonMove.bind(this)
            });
        });
    }

    initPreview() {
        // Initialize real Quill instance for preview
        this.previewQuill = new Quill('#toolbar-preview', {
            theme: this.options.theme,
            modules: { toolbar: this.generateToolbarConfig() },
            readOnly: true
        });
    }

    handlePaletteDrag(evt) {
        // Button dragged from palette to group
        this.addButtonToGroup(evt.to, evt.item, evt.newIndex);
        this.updateState();
        this.refreshPreview();
    }

    handleGroupReorder(evt) {
        // Group order changed
        this.reorderGroup(evt.oldIndex, evt.newIndex);
        this.updateState();
        this.refreshPreview();
    }

    handleButtonMove(evt) {
        // Button moved within/between groups
        this.moveButton(evt.from, evt.to, evt.oldIndex, evt.newIndex);
        this.updateState();
        this.refreshPreview();
    }

    addButtonToGroup(groupElement, buttonElement, index) {
        // Logic to add button to group in state
        const groupId = groupElement.getAttribute('data-group-id');
        const buttonType = buttonElement.getAttribute('data-button-type');
        // Update this.state.groups[...].buttons
    }

    updateState() {
        // Serialize current UI state to hidden form inputs
        // Format: matching ASP.NET model binding (indexed collections)
        this.state.groups.forEach((group, gIdx) => {
            group.buttons.forEach((button, bIdx) => {
                // Create hidden inputs: Groups[0].Buttons[0].Type = "bold", etc.
            });
        });
    }

    refreshPreview() {
        // Regenerate toolbar config and update Quill preview
        const newConfig = this.generateToolbarConfig();
        this.previewQuill.getModule('toolbar').container.innerHTML = '';
        // Re-initialize toolbar module with new config
    }

    generateToolbarConfig() {
        // Generate Quill.js toolbar array from this.state
        return this.state.groups.map(group =>
            group.buttons.map(btn => {
                // Convert button objects to Quill format
                // Same logic as GenerateQuillJson() in C#
            })
        );
    }

    bindEvents() {
        // Event listeners for presets, add group, delete group, etc.
        this.container.querySelector('.btn-preset-minimal').addEventListener('click', () => this.loadPreset('minimal'));
        this.container.querySelector('.btn-add-group').addEventListener('click', () => this.addGroup());
        // ... etc
    }

    loadPreset(presetName) {
        // Fetch preset configuration (embedded in page as JSON data attribute)
        const presets = JSON.parse(document.getElementById('toolbar-presets').getAttribute('data-presets'));
        this.state = presets[presetName];
        this.render();
        this.refreshPreview();
    }

    addGroup() {
        // Add new empty group to state and DOM
        const newGroup = {
            id: this.generateId(),
            name: '',
            order: this.state.groups.length,
            buttons: []
        };
        this.state.groups.push(newGroup);
        this.renderGroup(newGroup);
        this.initGroups(); // Re-initialize SortableJS
    }

    render() {
        // Full re-render of builder UI (used after loading presets)
        this.groupsContainer.innerHTML = '';
        this.state.groups.forEach(group => this.renderGroup(group));
        this.initGroups();
    }

    renderGroup(group) {
        // Create DOM elements for a group
        const groupHtml = `
            <div class="toolbar-group" data-group-id="${group.id}">
                <div class="group-header">
                    <span class="group-drag-handle">⋮⋮</span>
                    <input type="text" class="group-name" placeholder="Group ${group.order + 1}" value="${group.name}">
                    <button class="btn-delete-group">×</button>
                </div>
                <div class="group-buttons" data-sortable="true">
                    ${group.buttons.map(btn => this.renderButton(btn)).join('')}
                </div>
            </div>
        `;
        this.groupsContainer.insertAdjacentHTML('beforeend', groupHtml);
    }

    renderButton(button) {
        // Create DOM element for a button
        return `
            <div class="toolbar-button" data-button-type="${button.type}" data-button-value="${button.value}">
                <span class="button-icon">${this.getButtonIcon(button.type)}</span>
                <span class="button-label">${button.displayName}</span>
            </div>
        `;
    }

    getButtonIcon(type) {
        // Map button types to icons/labels
        const icons = {
            'bold': 'B',
            'italic': 'I',
            'underline': 'U',
            // ... etc
        };
        return icons[type] || type;
    }

    generateId() {
        return 'group-' + Math.random().toString(36).substr(2, 9);
    }
}

// Initialize when DOM ready
document.addEventListener('DOMContentLoaded', () => {
    const builderContainer = document.getElementById('quill-toolbar-builder');
    if (builderContainer) {
        new QuillToolbarBuilder(builderContainer, {
            theme: builderContainer.getAttribute('data-theme')
        });
    }
});
```

#### **4.3.3 ASP.NET Model Binding**

**Hidden Inputs Pattern** (for form submission):

```html
<!-- Generated by JavaScript as user configures -->
<input type="hidden" name="Groups[0].Id" value="group-xyz123" />
<input type="hidden" name="Groups[0].Name" value="" />
<input type="hidden" name="Groups[0].Order" value="0" />
<input type="hidden" name="Groups[0].Buttons[0].Type" value="bold" />
<input type="hidden" name="Groups[0].Buttons[0].Value" value="" />
<input type="hidden" name="Groups[0].Buttons[0].Order" value="0" />
<input type="hidden" name="Groups[0].Buttons[1].Type" value="italic" />
<input type="hidden" name="Groups[0].Buttons[1].Order" value="1" />
<!-- ... etc for all groups and buttons -->
```

**ViewModel Update** (QuillSettingsViewModel.cs):

```csharp
public class QuillSettingsViewModel
{
    public List<QuillToolbarGroupViewModel> Groups { get; set; } = new();
    public List<string> CustomColors { get; set; } = new();
    public QuillTheme Theme { get; set; }

    // Convert ViewModel → Domain Model
    public QuillToolbarConfig ToToolbarConfig()
    {
        var config = new QuillToolbarConfig
        {
            Groups = Groups.Select(g => new QuillToolbarGroup
            {
                Id = g.Id,
                Name = g.Name,
                Order = g.Order,
                Buttons = g.Buttons.Select(b => new QuillButton
                {
                    Type = b.Type,
                    Value = b.Value,
                    Order = b.Order,
                    DisplayName = GetButtonDisplayName(b.Type),
                    Category = GetButtonCategory(b.Type),
                    Icon = GetButtonIcon(b.Type)
                }).ToList()
            }).ToList(),
            CustomColors = CustomColors
        };
        return config;
    }

    // Convert Domain Model → ViewModel
    public static QuillSettingsViewModel FromToolbarConfig(QuillToolbarConfig config)
    {
        return new QuillSettingsViewModel
        {
            Groups = config.Groups.Select(g => new QuillToolbarGroupViewModel
            {
                Id = g.Id,
                Name = g.Name,
                Order = g.Order,
                Buttons = g.Buttons.Select(b => new QuillButtonViewModel
                {
                    Type = b.Type,
                    Value = b.Value,
                    Order = b.Order
                }).ToList()
            }).ToList(),
            CustomColors = config.CustomColors,
            Theme = // ... determine from config
        };
    }
}

public class QuillToolbarGroupViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Order { get; set; }
    public List<QuillButtonViewModel> Buttons { get; set; } = new();
}

public class QuillButtonViewModel
{
    public string Type { get; set; }
    public string Value { get; set; }
    public int Order { get; set; }
}
```

### **4.4 File Changes Summary**

#### **Files to Modify:**

1. **Settings/QuillToolbarConfig.cs**
   - Add `Groups` property
   - Add `QuillToolbarGroup` and `QuillButton` classes
   - Update `GenerateQuillJson()` to use `Groups` instead of flag enums
   - Mark flag enums as `[Obsolete]` for backward compatibility
   - Update factory methods (`CreateStandard()`, etc.)

2. **ViewModels/QuillSettingsViewModel.cs**
   - Add group and button view model classes
   - Update `ToToolbarConfig()` and `FromToolbarConfig()` methods
   - Remove individual checkbox properties

3. **Settings/HtmlFieldQuillEditorSettingsDriver.cs**
   - Update `Edit()` method to pass group data to view
   - Update `UpdateAsync()` to handle new form structure
   - Add embedded JSON data for button metadata (icons, categories, etc.)

4. **Views/HtmlFieldQuillEditorSettings.Edit.cshtml**
   - **Complete rewrite** of the view
   - Replace checkbox grid with drag-and-drop builder
   - Add palette component
   - Add group builder component
   - Add live preview component
   - Add preset buttons
   - Keep custom colors modal (minimal changes)
   - Add SortableJS initialization

5. **Migrations.cs**
   - Add new migration: `UpdateFrom4()` (or next sequential number)
   - Detect old checkbox-based configs (flag enums)
   - Convert to new group-based structure
   - Preserve button selections and custom colors
   - Log migration details

6. **Settings/LegacyToolbarMigration.cs** (optional)
   - Add helper methods for migrating old configs to new format
   - Centralize migration logic for reuse

#### **Files to Create:**

7. **wwwroot/Scripts/quill-toolbar-builder.js**
   - Main JavaScript module (as specified above)
   - ~500-800 lines estimated

8. **wwwroot/Styles/quill-toolbar-builder.css**
   - Custom styles for drag-and-drop UI
   - Palette, groups, buttons, drop zones, drag states
   - ~200-300 lines estimated

#### **Files to Reference:**

9. **ResourceManagementOptionsConfiguration.cs**
   - Register new JS and CSS resources
   - Include SortableJS CDN or bundled version
   - Set dependencies: Quill.js, Bootstrap, jQuery

10. **Button Metadata** (consider separate file or embedded in view)
    - JSON data structure mapping button types to display info
    - Could be: `wwwroot/Data/quill-buttons.json`
    - Or: embedded directly in Razor view as `<script type="application/json">`

---

## **5. Implementation Plan**

### **5.1 Phase 1: Data Model & Migration** (Week 1)

**Tasks:**
1. Update `QuillToolbarConfig.cs` with new `Groups` structure
2. Create `QuillToolbarGroup` and `QuillButton` classes
3. Update `GenerateQuillJson()` method
4. Create migration in `Migrations.cs`
5. Write unit tests for JSON generation with new model
6. Write unit tests for migration logic

**Deliverables:**
- ✅ New data model in place
- ✅ Migration tested and working
- ✅ Backward compatibility maintained (old configs still work)

### **5.2 Phase 2: Backend & ViewModel** (Week 2)

**Tasks:**
1. Update `QuillSettingsViewModel` with group/button classes
2. Update `HtmlFieldQuillEditorSettingsDriver` methods
3. Create button metadata structure (JSON or constants)
4. Add server-side validation for toolbar configurations
5. Write unit tests for ViewModel conversions
6. Test round-trip: load settings → edit → save → verify

**Deliverables:**
- ✅ Form binding working with new structure
- ✅ Server-side validation in place
- ✅ Unit tests passing

### **5.3 Phase 3: Frontend UI (Non-Interactive)** (Week 3)

**Tasks:**
1. Create new `HtmlFieldQuillEditorSettings.Edit.cshtml` layout
2. Build static HTML/CSS for palette, groups, preview
3. Style components with Bootstrap + custom CSS
4. Add preset template buttons (non-functional yet)
5. Add custom colors modal (preserve existing functionality)
6. Test responsive layout on different screen sizes

**Deliverables:**
- ✅ Visual UI matches design spec
- ✅ Responsive layout working
- ✅ No functionality yet (static display)

### **5.4 Phase 4: Drag-and-Drop Implementation** (Week 4-5)

**Tasks:**
1. Integrate SortableJS library
2. Implement `QuillToolbarBuilder` JavaScript class
3. Initialize SortableJS on palette and groups
4. Implement drag-from-palette functionality
5. Implement drag-between-groups functionality
6. Implement group reordering
7. Implement add/delete group functionality
8. Test drag interactions thoroughly
9. Add keyboard navigation support

**Deliverables:**
- ✅ Full drag-and-drop working
- ✅ State updates correctly
- ✅ Hidden inputs generated for form submission
- ✅ Keyboard navigation functional

### **5.5 Phase 5: Live Preview** (Week 6)

**Tasks:**
1. Initialize Quill.js instance for preview
2. Implement `refreshPreview()` method
3. Handle theme switching
4. Optimize preview rendering (debounce)
5. Test preview accuracy vs. actual editor
6. Handle edge cases (empty toolbar, invalid configs)

**Deliverables:**
- ✅ Live preview shows accurate toolbar
- ✅ Updates in real-time during configuration
- ✅ Theme switching works correctly

### **5.6 Phase 6: Preset Templates** (Week 7)

**Tasks:**
1. Implement preset loading functionality
2. Add confirmation dialog before replacing config
3. Test each preset (Minimal, Standard, Full)
4. Ensure presets match current factory methods
5. Add "Reset to Last Saved" functionality

**Deliverables:**
- ✅ All three presets working
- ✅ User confirmation before overwriting
- ✅ Reset functionality implemented

### **5.7 Phase 7: Validation & Error Handling** (Week 8)

**Tasks:**
1. Add client-side validation (empty toolbar, etc.)
2. Add server-side validation
3. Display validation errors in UI
4. Add tooltips and help text
5. Test error scenarios thoroughly
6. Add success/error messages on save

**Deliverables:**
- ✅ Comprehensive validation in place
- ✅ Clear error messages for users
- ✅ Graceful handling of edge cases

### **5.8 Phase 8: Testing & Polish** (Week 9-10)

**Tasks:**
1. Write comprehensive unit tests (target 80% coverage)
2. Write integration tests for full workflow
3. Manual testing on multiple browsers
4. Accessibility audit with screen reader
5. Performance testing (large toolbars, rapid changes)
6. Fix bugs identified during testing
7. Code review and refactoring
8. Update documentation (README, ARCHITECTURE.md)

**Deliverables:**
- ✅ 80%+ test coverage
- ✅ All browsers tested and working
- ✅ Accessibility requirements met
- ✅ Documentation updated
- ✅ Ready for production deployment

---

## **6. Testing Strategy**

### **6.1 Unit Tests**

**Backend (C#):**
- `QuillToolbarConfig.GenerateQuillJson()` with various group configurations
- Factory methods produce correct group structures
- Migration converts old flag enums to new groups correctly
- ViewModel `ToToolbarConfig()` and `FromToolbarConfig()` roundtrip correctly
- Validation rules enforce constraints

**Frontend (JavaScript):**
- `generateToolbarConfig()` produces correct Quill format
- `addButtonToGroup()` updates state correctly
- `reorderGroup()` updates order values correctly
- `loadPreset()` loads correct configuration
- Form serialization produces correct hidden inputs

### **6.2 Integration Tests**

1. **End-to-End Configuration Flow:**
   - Load settings page → Drag buttons → Save → Reload → Verify persistence

2. **Migration Test:**
   - Create content type with old checkbox config
   - Run migration
   - Verify toolbar still works identically
   - Verify settings page shows correct configuration

3. **Preset Loading:**
   - Load each preset → Verify buttons match expected
   - Customize preset → Save → Verify customizations persist

4. **Live Preview Accuracy:**
   - Configure toolbar in builder
   - Compare preview to actual editor toolbar
   - Verify visual appearance matches

### **6.3 Manual Testing**

**Browser Compatibility:**
- Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- Test drag-and-drop in each browser
- Test keyboard navigation

**Accessibility:**
- Screen reader testing (NVDA, JAWS)
- Keyboard-only navigation
- Color contrast validation
- ARIA attribute verification

**Responsive Design:**
- Desktop (1920x1080, 1280x720)
- Tablet (1024x768, 768x1024)
- Touch interactions on tablet

**Edge Cases:**
- Empty toolbar (all buttons in palette)
- Very large toolbar (50+ buttons, 20+ groups)
- Rapid drag operations
- Browser back/forward navigation
- Form validation errors
- Concurrent editing (if applicable)

### **6.4 Performance Testing**

**Metrics:**
- Drag operation response time: <16ms (60fps target)
- Preview render time: <200ms after change
- Initial page load: <2 seconds
- Form submission: <500ms

**Load Testing:**
- Toolbar with 50 buttons across 15 groups
- Rapid drag operations (10 drags per second)
- Large custom color palette (50 colors)

---

## **7. Security Considerations**

### **7.1 XSS Prevention**

**Existing Pattern (MAINTAIN):**
- ✅ Use data attributes for configuration: `data-toolbar-config="@config.GenerateQuillJson()"`
- ✅ Razor automatically HTML-encodes attribute values
- ✅ JavaScript uses `JSON.parse()` to safely parse
- ❌ NEVER use `@Html.Raw()` with user-controlled data

**New Considerations:**
- Group names (if user-provided): HTML encode before rendering
- Button labels: Use predefined constants, never user input
- Custom colors: Validate hex format server-side (`^#[0-9A-F]{6}$`)

### **7.2 Input Validation**

**Client-Side:**
- Validate toolbar not empty (at least 1 button)
- Validate hex color format before adding
- Validate group count (max 20 groups?)
- Validate button count per group (max 15 buttons?)

**Server-Side:**
- Validate all button types are from allowed list
- Validate group structure (no null/empty properties)
- Validate custom colors are valid hex codes
- Sanitize group names (strip HTML, limit length)
- Enforce maximum toolbar complexity (prevent DoS via huge configs)

### **7.3 Authorization**

- Toolbar configuration is admin-only (OrchardCore permission check)
- No additional permissions needed (same as current implementation)
- Consider: custom permission for "Manage Toolbar Presets" (future enhancement)

### **7.4 Data Integrity**

- Server-side validation ensures toolbar config is valid before saving
- Migration logic handles malformed old configs gracefully
- Fallback to default configuration if load fails

---

## **8. Documentation Requirements**

### **8.1 User Documentation**

**Update README.md:**
- Replace checkbox screenshots with drag-and-drop screenshots
- Add section: "Configuring Toolbars with Drag-and-Drop"
- Explain preset templates
- Show examples of custom group creation
- Add troubleshooting section

**Create Video/GIF Tutorials:**
- 30-second demo: basic toolbar configuration
- 1-minute demo: advanced features (groups, presets)
- Host on GitHub repo or link to YouTube

### **8.2 Developer Documentation**

**Update ARCHITECTURE.md:**
- Document new data model (`Groups`, `QuillButton`)
- Explain JSON generation algorithm
- Document JavaScript architecture (SortableJS, class structure)
- Add sequence diagrams for drag-and-drop flow
- Document extension points for custom button types

**Add MIGRATION-GUIDE.md:**
- Explain changes from v1.x to v2.0
- Migration process (automatic vs. manual)
- Breaking changes (if any)
- Upgrade checklist for existing sites

### **8.3 Code Documentation**

- XML comments on all public methods
- JSDoc comments on JavaScript functions
- Inline comments explaining complex logic
- README in `/wwwroot/Scripts/` explaining JS architecture

---

## **9. Rollout Strategy**

### **9.1 Beta Testing**

**Phase 1: Internal Testing** (2 weeks)
- Deploy to staging environment
- Test by development team
- Fix critical bugs

**Phase 2: Limited Beta** (2 weeks)
- Invite 5-10 users for early access
- Collect feedback via survey
- Monitor for issues, gather feature requests

**Phase 3: Public Beta** (4 weeks)
- Release as beta version (v2.0.0-beta.1)
- Announce in OrchardCore forums/Discord
- Collect feedback from wider audience
- Iterate based on feedback

### **9.2 Production Release**

**Pre-Release Checklist:**
- ✅ All tests passing (unit, integration, manual)
- ✅ Accessibility audit completed
- ✅ Security review completed
- ✅ Documentation updated
- ✅ Migration tested on real production data
- ✅ Performance benchmarks met
- ✅ Browser compatibility verified
- ✅ No critical bugs outstanding

**Release Process:**
1. Tag release: v2.0.0
2. Update NuGet package
3. Update GitHub releases page
4. Announce on OrchardCore community channels
5. Monitor for issues in first 48 hours
6. Hotfix if critical issues arise

### **9.3 Monitoring & Support**

**Post-Release:**
- Monitor GitHub issues for bug reports
- Track usage analytics (if telemetry added)
- Collect user feedback via surveys
- Plan for v2.1.0 based on feedback

---

## **10. Future Enhancements**

*(Out of scope for v2.0, but document for future consideration)*

### **10.1 Advanced Features**

1. **Button Configuration Panel**
   - Click button to open config panel (e.g., set font list for font button)
   - Configure color palettes per-button (different colors for text vs. background)

2. **Template Sharing**
   - Export toolbar configuration as JSON file
   - Import toolbar configurations from other sites
   - Community-contributed templates gallery

3. **Conditional Buttons**
   - Show/hide buttons based on user roles or content type
   - A/B testing different toolbar configurations

4. **Toolbar Themes**
   - Custom button icons and styling
   - Different visual themes for toolbar (beyond Snow/Bubble)

5. **Analytics Integration**
   - Track which buttons are actually used in editors
   - Recommend removing unused buttons
   - Heat map of button usage

### **10.2 Performance Optimizations**

1. **Virtual Scrolling**
   - For very large button palettes (if custom buttons added)

2. **Lazy Loading**
   - Load preview Quill instance only when preview section expanded

3. **Optimistic UI Updates**
   - Instant visual feedback before server validation

### **10.3 UX Improvements**

1. **Undo/Redo Stack**
   - Browser-native undo (Ctrl+Z) for configuration changes
   - "Undo last change" button

2. **Duplicate Group**
   - Quickly create similar groups

3. **Button Search**
   - Fuzzy search in palette
   - Recent/frequently-used buttons section

4. **Drag-and-Drop from Editor**
   - Configure toolbar while looking at actual content
   - "Add this button to toolbar" link in editor

---

## **11. Success Metrics**

### **11.1 Adoption Metrics**

- **Target**: 80% of users successfully configure toolbar on first try
- **Measure**: Completion rate (users who save settings without errors)

### **11.2 Usability Metrics**

- **Target**: Average configuration time < 3 minutes
- **Measure**: Time from page load to successful save

### **11.3 Quality Metrics**

- **Target**: <5 critical bugs in first month post-release
- **Measure**: GitHub issues tagged "bug" + "critical"

### **11.4 User Satisfaction**

- **Target**: 4.5/5 average satisfaction rating
- **Measure**: Post-configuration survey (optional modal)

---

## **12. Risks & Mitigations**

### **12.1 Technical Risks**

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| SortableJS compatibility issues | Low | Medium | Test early, have fallback to native drag-and-drop |
| Performance issues with large toolbars | Medium | Medium | Implement virtual scrolling, optimize rendering |
| Browser compatibility bugs | Medium | High | Thorough cross-browser testing, polyfills if needed |
| Migration edge cases | Medium | High | Extensive migration testing, manual fallback option |
| Quill preview rendering issues | Low | Low | Use actual Quill instance, match production config |

### **12.2 User Experience Risks**

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Users confused by new UI | Medium | High | Comprehensive documentation, video tutorials, tooltips |
| Users miss checkbox simplicity | Low | Medium | Gather feedback during beta, consider Simple/Advanced toggle |
| Drag-and-drop not discoverable | Low | Medium | Onboarding tooltip, clear affordances, animated demo |
| Touch screen usability issues | Medium | Medium | Test on tablets, optimize touch targets, alternative UI |

### **12.3 Project Risks**

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Scope creep (too many features) | High | Medium | Strict adherence to spec, defer enhancements to v2.1 |
| Timeline overrun | Medium | Medium | Phased implementation, MVP first, polish later |
| Resource constraints | Low | High | Clearly define dependencies, involve team early |

---

## **13. Appendix**

### **13.1 Button Type Reference**

Complete list of Quill buttons and their properties:

| Button Type | Display Name | Category | JSON Format | Notes |
|-------------|--------------|----------|-------------|-------|
| bold | Bold | Formatting | "bold" | Simple string |
| italic | Italic | Formatting | "italic" | Simple string |
| underline | Underline | Formatting | "underline" | Simple string |
| strike | Strikethrough | Formatting | "strike" | Simple string |
| code | Inline Code | Formatting | "code" | Simple string |
| blockquote | Blockquote | Blocks | "blockquote" | Simple string |
| code-block | Code Block | Blocks | "code-block" | Simple string |
| header | Header 1 | Blocks | {"header": 1} | Object with value |
| header | Header 2 | Blocks | {"header": 2} | Object with value |
| list | Ordered List | Lists | {"list": "ordered"} | Object with value |
| list | Bullet List | Lists | {"list": "bullet"} | Object with value |
| list | Checklist | Lists | {"list": "check"} | Object with value |
| link | Insert Link | Media | "link" | Simple string |
| image | Insert Image | Media | "image" | Custom handler |
| video | Insert Video | Media | "video" | Simple string |
| formula | Insert Formula | Media | "formula" | Requires KaTeX |
| color | Text Color | Styles | {"color": [...]} | Array of colors |
| background | Background Color | Styles | {"background": [...]} | Array of colors |
| font | Font Family | Styles | {"font": []} | Empty = defaults |
| size | Font Size | Styles | {"size": [sizes]} | Predefined sizes |
| align | Text Alignment | Styles | {"align": []} | Empty = all options |
| script | Superscript | Advanced | {"script": "super"} | Object with value |
| script | Subscript | Advanced | {"script": "sub"} | Object with value |
| indent | Indent | Advanced | {"indent": "+1"} | Object with value |
| indent | Outdent | Advanced | {"indent": "-1"} | Object with value |
| direction | RTL/LTR | Advanced | {"direction": "rtl"} | Object with value |
| clean | Remove Formatting | Advanced | "clean" | Simple string |

### **13.2 Sample Configurations**

**Minimal Toolbar (JSON):**
```json
[
  ["bold", "italic", "underline"],
  [{"list": "ordered"}, {"list": "bullet"}]
]
```

**Standard Toolbar (JSON):**
```json
[
  ["bold", "italic", "underline", "strike"],
  ["blockquote", "code-block"],
  [{"list": "ordered"}, {"list": "bullet"}, {"list": "check"}],
  ["link", "image"],
  [{"color": []}, {"background": []}]
]
```

**Full Toolbar (JSON):**
```json
[
  ["bold", "italic", "underline", "strike", "code"],
  ["blockquote", "code-block"],
  [{"header": 1}, {"header": 2}],
  ["link", "image", "video", "formula"],
  [{"list": "ordered"}, {"list": "bullet"}, {"list": "check"}],
  [{"color": []}, {"background": []}],
  [{"font": []}, {"size": ["small", false, "large", "huge"]}],
  [{"align": []}],
  [{"script": "sub"}, {"script": "super"}],
  [{"indent": "-1"}, {"indent": "+1"}],
  [{"direction": "rtl"}],
  ["clean"]
]
```

### **13.3 Current Implementation Reference**

**Key Files:**
- Current Settings View: `src/Buzz.OrchardCore.Quilljs/Views/HtmlFieldQuillEditorSettings.Edit.cshtml`
- Current Data Model: `src/Buzz.OrchardCore.Quilljs/Settings/QuillToolbarConfig.cs`
- Button Enums: `src/Buzz.OrchardCore.Quilljs/Settings/QuillButton.cs`
- Settings Driver: `src/Buzz.OrchardCore.Quilljs/Settings/HtmlFieldQuillEditorSettingsDriver.cs`
- View Model: `src/Buzz.OrchardCore.Quilljs/ViewModels/QuillSettingsViewModel.cs`

**Current UI Pattern:**
- Two-column checkbox grid layout using Bootstrap
- 6 sections: Formatting, Blocks, Lists, Media, Styles, Advanced
- Custom colors managed via modal with HTML5 color picker
- Theme selection via dropdown
- No control over button order or grouping

---

## **14. Approval & Sign-Off**

### **14.1 Stakeholder Review**

- [ ] **Product Owner**: Approves feature scope and UX design
- [ ] **Technical Lead**: Approves architecture and technical approach
- [ ] **QA Lead**: Approves testing strategy
- [ ] **UX Designer**: Approves UI/UX design (if applicable)

### **14.2 Acceptance Criteria**

**Feature is considered complete when:**
1. ✅ All functional requirements (FR-1 through FR-8) are implemented
2. ✅ All non-functional requirements (NFR-1 through NFR-6) are met
3. ✅ Unit test coverage ≥80%
4. ✅ All manual test scenarios pass
5. ✅ Accessibility audit passes (WCAG 2.1 Level AA)
6. ✅ Documentation is complete and accurate
7. ✅ Beta testing feedback has been addressed
8. ✅ No critical or high-priority bugs remain

### **14.3 Document History**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 (Draft) | 2025-11-17 | Claude Code | Initial specification based on user requirements |

---

**END OF SPECIFICATION DOCUMENT**
