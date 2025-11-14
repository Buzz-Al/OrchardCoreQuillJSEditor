# Buzz.OrchardCore.Quilljs

Adds Quill.js v2.0.2 as a rich text editor option for OrchardCore HTML fields.

## Features

- **Visual Toolbar Configuration** - User-friendly checkbox interface, no JSON editing required
- **Two Themes** - Snow (persistent toolbar) and Bubble (selection-based toolbar)
- **Custom Color Palettes** - Add custom colors with native HTML5 color picker
- **Media Integration** - Insert images from OrchardCore Media Library
- **Type-Safe Configuration** - Strongly-typed settings with compile-time validation
- **Secure** - No XSS vulnerabilities from configuration injection

## Installation

1. Add the module to your OrchardCore project
2. Enable the `Buzz.OrchardCore.Quilljs` feature in the admin dashboard

## Usage

### Configure an HTML Field

1. Go to **Content Definition** → **Content Types**
2. Edit a content type and add or edit an HTML Field
3. Set the **Editor** to **Quill**
4. Configure the toolbar:
   - Select a theme (Snow or Bubble)
   - Check the toolbar buttons you want to enable
   - Optionally add custom colors to the color palette
5. Save your changes

### Available Toolbar Buttons

**Text Formatting:** Bold, Italic, Underline, Strikethrough, Inline Code
**Blocks:** Blockquote, Code Block, Header 1, Header 2
**Lists:** Numbered List, Bullet List, Checklist
**Media:** Link, Image, Video, Formula
**Styles:** Text Color, Background Color, Font Family, Font Size, Text Alignment
**Advanced:** Superscript/Subscript, Indent/Outdent, Text Direction (RTL), Remove Formatting

## Custom Quill Build

This module uses a modified Quill.js v2.0.2 build that adds soft-break support (Shift+Enter for line breaks).
See: https://github.com/slab/quill/pull/4565

## Documentation

- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Technical architecture and design decisions
- **[CHANGELOG.md](CHANGELOG.md)** - Development history and changes

## Requirements

- OrchardCore 2.1.5+
- .NET 8.0+

## License

[Add your license here]
