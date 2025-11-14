using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Buzz.OrchardCore.Quilljs.Settings;

/// <summary>
/// Handles migration of legacy ToolbarOptions string format to new QuillToolbarConfig object format.
/// Legacy format: JSON string containing Quill.js toolbar array (e.g., "[['bold', 'italic'], [{'header': 1}]]")
/// New format: Strongly-typed QuillToolbarConfig with enum flags
/// </summary>
public static class LegacyToolbarMigration
{
   
    public static QuillToolbarConfig ParseLegacyToolbarOptions(string toolbarOptionsJson)
    {
        var config = new QuillToolbarConfig();

        if (string.IsNullOrWhiteSpace(toolbarOptionsJson))
        {
            return config;
        }

        try
        {

            var toolbar = JArray.Parse(toolbarOptionsJson);

            foreach (var group in toolbar)
            {

                if (group is JArray groupArray)
                {
                    foreach (var button in groupArray)
                    {
                        ParseButton(button, config);
                    }
                }
                else
                {
                    ParseButton(group, config);
                }
            }
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to parse legacy toolbar options: {ex.Message}");
        }

        return config;
    }

    private static void ParseButton(JToken button, QuillToolbarConfig config)
    {
        if (button.Type == JTokenType.String)
        {
            string buttonName = button.ToString();
            MapStringButton(buttonName, config);
        }
        else if (button.Type == JTokenType.Object)
        {
            var buttonObj = (JObject)button;

            var property = buttonObj.Properties().FirstOrDefault();

            if (property != null)
            {
                MapObjectButton(property.Name, property.Value, config);
            }
        }
    }

    private static void MapStringButton(string buttonName, QuillToolbarConfig config)
    {
        switch (buttonName.ToLowerInvariant())
        {
            // Formatting buttons
            case "bold":
                config.Formatting |= FormattingButtons.Bold;
                break;
            case "italic":
                config.Formatting |= FormattingButtons.Italic;
                break;
            case "underline":
                config.Formatting |= FormattingButtons.Underline;
                break;
            case "strike":
                config.Formatting |= FormattingButtons.Strike;
                break;
            case "code":
                config.Formatting |= FormattingButtons.Code;
                break;

            // Block buttons
            case "blockquote":
                config.Blocks |= BlockButtons.Blockquote;
                break;
            case "code-block":
                config.Blocks |= BlockButtons.CodeBlock;
                break;

            // Media buttons
            case "link":
                config.Media |= MediaButtons.Link;
                break;
            case "image":
                config.Media |= MediaButtons.Image;
                break;
            case "video":
                config.Media |= MediaButtons.Video;
                break;
            case "formula":
                config.Media |= MediaButtons.Formula;
                break;

            // Advanced buttons
            case "clean":
                config.Advanced |= AdvancedButtons.Clean;
                break;

        }
    }

    private static void MapObjectButton(string propertyName, JToken propertyValue, QuillToolbarConfig config)
    {
        switch (propertyName.ToLowerInvariant())
        {
            case "header":
                // Can be: {"header": 1}, {"header": 2}, or {"header": [1, 2, false]}
                if (propertyValue.Type == JTokenType.Integer)
                {
                    int headerLevel = propertyValue.Value<int>();
                    if (headerLevel == 1)
                    {
                        config.Blocks |= BlockButtons.Header1;
                    }
                    else if (headerLevel == 2)
                    {
                        config.Blocks |= BlockButtons.Header2;
                    }
                }
                else if (propertyValue.Type == JTokenType.Array)
                {
                    // Array format: [1, 2, false] means both h1 and h2 are available
                    var headerArray = (JArray)propertyValue;
                    foreach (var item in headerArray)
                    {
                        if (item.Type == JTokenType.Integer)
                        {
                            int level = item.Value<int>();
                            if (level == 1) config.Blocks |= BlockButtons.Header1;
                            else if (level == 2) config.Blocks |= BlockButtons.Header2;
                        }
                    }
                }
                break;

            case "list":
                // Can be: {"list": "ordered"}, {"list": "bullet"}, {"list": "check"}
                string listType = propertyValue.ToString().ToLowerInvariant();
                if (listType == "ordered")
                {
                    config.Lists |= ListButtons.Ordered;
                }
                else if (listType == "bullet")
                {
                    config.Lists |= ListButtons.Bullet;
                }
                else if (listType == "check")
                {
                    config.Lists |= ListButtons.Check;
                }
                break;

            case "color":
                // Can be: {"color": []}, {"color": ["#000", "#fff"]}, or {"color": []}
                config.Styles |= StyleButtons.Color;

                // If there are custom colors, extract them
                if (propertyValue.Type == JTokenType.Array)
                {
                    var colorArray = (JArray)propertyValue;
                    if (colorArray.Count > 0)
                    {
                        config.CustomColors = colorArray
                            .Where(c => c.Type == JTokenType.String)
                            .Select(c => c.ToString())
                            .ToList();
                    }
                }
                break;

            case "background":
                config.Styles |= StyleButtons.Background;
                break;

            case "font":
                config.Styles |= StyleButtons.Font;
                break;

            case "size":
                config.Styles |= StyleButtons.Size;
                break;

            case "align":
                config.Styles |= StyleButtons.Align;
                break;

            case "script":
                // Can be: {"script": "sub"}, {"script": "super"}
                // Both map to the Script button (which allows both subscript and superscript)
                config.Advanced |= AdvancedButtons.Script;
                break;

            case "indent":
                // Can be: {"indent": "-1"}, {"indent": "+1"}
                config.Advanced |= AdvancedButtons.Indent;
                break;

            case "direction":
                config.Advanced |= AdvancedButtons.Direction;
                break;
        }
    }
}
