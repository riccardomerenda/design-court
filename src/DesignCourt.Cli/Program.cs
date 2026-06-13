using System.Text.Json;
using System.Text.Json.Serialization;
using DesignCourt.Agents;
using DesignCourt.Core;
using DesignCourt.Parsing;
using DesignCourt.Reporting;

namespace DesignCourt.Cli;

internal static class Program
{
    private const string DefaultOutputDirectory = "design-court-output";

    public static async Task<int> Main(string[] args)
    {
        if (args.Length < 2 || !string.Equals(args[0], "review", StringComparison.OrdinalIgnoreCase))
        {
            PrintUsage();
            return 2;
        }

        var targetPath = args[1];

        if (!File.Exists(targetPath))
        {
            Console.Error.WriteLine($"Input document not found: {targetPath}");
            return 2;
        }

        var content = await File.ReadAllTextAsync(targetPath);
        var parser = new MarkdownDocumentParser();
        var document = parser.Parse(targetPath, content);
        var workflow = new LocalReviewWorkflow(
            [new DeterministicOperationsEngineerAgent()],
            new EvidenceGatedJudgeAgent(),
            new MarkdownEvidenceVerifier());
        var result = await workflow.RunAsync(document, CancellationToken.None);

        Directory.CreateDirectory(DefaultOutputDirectory);

        var report = new MarkdownReportWriter().Write(result);
        await File.WriteAllTextAsync(Path.Combine(DefaultOutputDirectory, "report.md"), report);
        await File.WriteAllTextAsync(
            Path.Combine(DefaultOutputDirectory, "findings.json"),
            JsonSerializer.Serialize(result.Findings, JsonOptions()));

        Console.WriteLine($"Parsed {document.Sections.Count} section(s).");
        Console.WriteLine($"Produced {result.Findings.Count} judged finding(s).");
        Console.WriteLine($"Wrote {DefaultOutputDirectory}/report.md");
        Console.WriteLine($"Wrote {DefaultOutputDirectory}/findings.json");

        return 0;
    }

    private static JsonSerializerOptions JsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
        return options;
    }

    private static void PrintUsage()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  design-court review <markdown-file>");
    }
}
