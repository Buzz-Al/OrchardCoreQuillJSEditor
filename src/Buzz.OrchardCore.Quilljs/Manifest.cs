using OrchardCore.Modules.Manifest;

[assembly: Module(
	Name = "Quill Editor",
    Id = "Buzz.OrchardCore.Quilljs",
	Author = "Buzz Interactive",
	Website = "https://buzzinteractive.co.uk",
	Version = "1.0.0",
	Category = "Buzz CMS",
	Description = "Adds Quill as an editor choice.",
	Dependencies = new[] { "OrchardCore.Html", "OrchardCore.ContentFields"}
)]
