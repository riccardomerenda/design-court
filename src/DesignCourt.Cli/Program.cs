using System.Globalization;
using System.Reflection;
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
    private const string DefaultBenchmarkManifest = "samples/benchmark.json";

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 1 && IsVersionOption(args[0]))
        {
            Console.WriteLine($"design-court {GetVersion()}");
            return 0;
        }

        if (args.Length >= 1 && string.Equals(args[0], "review", StringComparison.OrdinalIgnoreCase))
        {
            return await RunReviewAsync(args);
        }

        if (args.Length >= 1 && string.Equals(args[0], "eval", StringComparison.OrdinalIgnoreCase))
        {
            return await RunEvalAsync(args);
        }

        PrintUsage();
        return 2;
    }

    private static async Task<int> RunReviewAsync(string[] args)
    {
        if (args.Length < 2)
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

        var document = await ParseDocumentAsync(targetPath, targetPath);
        var result = await CreateWorkflow().RunAsync(document, CancellationToken.None);

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

    private static async Task<int> RunEvalAsync(string[] args)
    {
        var manifestPath = args.Length >= 2 ? args[1] : DefaultBenchmarkManifest;

        if (!File.Exists(manifestPath))
        {
            Console.Error.WriteLine($"Benchmark manifest not found: {manifestPath}");
            return 2;
        }

        var manifestRoot = Path.GetDirectoryName(Path.GetFullPath(manifestPath)) ?? ".";

        BenchmarkManifest? manifest;

        try
        {
            var manifestJson = await File.ReadAllTextAsync(manifestPath);
            manifest = JsonSerializer.Deserialize<BenchmarkManifest>(manifestJson, JsonOptions());
        }
        catch (JsonException exception)
        {
            Console.Error.WriteLine($"Could not parse benchmark manifest: {exception.Message}");
            return 2;
        }

        if (manifest?.Cases is null || manifest.Cases.Count == 0)
        {
            Console.Error.WriteLine("Benchmark manifest does not define any cases.");
            return 2;
        }

        var workflow = CreateWorkflow();
        var cases = new List<EvaluationCase>();

        foreach (var manifestCase in manifest.Cases)
        {
            if (string.IsNullOrWhiteSpace(manifestCase.Document))
            {
                Console.Error.WriteLine("Benchmark manifest contains a case without a document path.");
                return 2;
            }

            var documentPath = Path.Combine(manifestRoot, manifestCase.Document);

            if (!File.Exists(documentPath))
            {
                Console.Error.WriteLine($"Benchmark document not found: {documentPath}");
                return 2;
            }

            var document = await ParseDocumentAsync(documentPath, manifestCase.Document);
            var result = await workflow.RunAsync(document, CancellationToken.None);

            IReadOnlyList<ExpectedFinding> expectedFindings;

            try
            {
                expectedFindings = await LoadExpectedFindingsAsync(manifestRoot, manifestCase.Expected);
            }
            catch (Exception exception) when (exception is JsonException or FileNotFoundException)
            {
                Console.Error.WriteLine(
                    $"Could not load expected findings for {manifestCase.Document}: {exception.Message}");
                return 2;
            }

            cases.Add(new EvaluationCase(manifestCase.Document, expectedFindings, result.Findings));
        }

        var evaluation = new BenchmarkEvaluator().Evaluate(cases);

        Directory.CreateDirectory(DefaultOutputDirectory);

        var report = new MarkdownEvaluationReportWriter().Write(evaluation);
        await File.WriteAllTextAsync(Path.Combine(DefaultOutputDirectory, "eval-report.md"), report);
        await File.WriteAllTextAsync(
            Path.Combine(DefaultOutputDirectory, "eval-metrics.json"),
            JsonSerializer.Serialize(evaluation.Metrics, JsonOptions()));

        var metrics = evaluation.Metrics;
        Console.WriteLine($"Evaluated {metrics.DocumentCount} benchmark case(s).");
        Console.WriteLine($"Precision: {Format(metrics.Precision)}");
        Console.WriteLine($"Recall: {Format(metrics.Recall)}");
        Console.WriteLine($"F1: {Format(metrics.F1)}");
        Console.WriteLine($"False-positive rate: {Format(metrics.FalsePositiveRate)}");
        Console.WriteLine(
            $"True positives: {metrics.TruePositives}, " +
            $"false positives: {metrics.FalsePositives}, " +
            $"false negatives: {metrics.FalseNegatives}");
        Console.WriteLine($"Wrote {DefaultOutputDirectory}/eval-report.md");
        Console.WriteLine($"Wrote {DefaultOutputDirectory}/eval-metrics.json");

        return 0;
    }

    private static async Task<ReviewedDocument> ParseDocumentAsync(string readPath, string documentPath)
    {
        var content = await File.ReadAllTextAsync(readPath);
        return new MarkdownDocumentParser().Parse(documentPath, content);
    }

    private static async Task<IReadOnlyList<ExpectedFinding>> LoadExpectedFindingsAsync(
        string manifestRoot,
        string? expectedRelativePath)
    {
        if (string.IsNullOrWhiteSpace(expectedRelativePath))
        {
            return Array.Empty<ExpectedFinding>();
        }

        var expectedPath = Path.Combine(manifestRoot, expectedRelativePath);

        if (!File.Exists(expectedPath))
        {
            throw new FileNotFoundException($"Expected findings file not found: {expectedPath}");
        }

        var json = await File.ReadAllTextAsync(expectedPath);
        return JsonSerializer.Deserialize<List<ExpectedFinding>>(json, JsonOptions())
            ?? (IReadOnlyList<ExpectedFinding>)Array.Empty<ExpectedFinding>();
    }

    private static LocalReviewWorkflow CreateWorkflow()
    {
        return new LocalReviewWorkflow(
            [new DeterministicOperationsEngineerAgent()],
            new EvidenceGatedJudgeAgent(),
            new MarkdownEvidenceVerifier());
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

    private static string Format(double value)
    {
        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static bool IsVersionOption(string value)
    {
        return string.Equals(value, "--version", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "-v", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetVersion()
    {
        return typeof(Program).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
            ?? "0.0.0-local";
    }

    private static void PrintUsage()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  design-court --version");
        Console.Error.WriteLine("  design-court review <markdown-file>");
        Console.Error.WriteLine("  design-court eval [benchmark-manifest]");
    }

    private sealed record BenchmarkManifest(IReadOnlyList<BenchmarkManifestCase> Cases);

    private sealed record BenchmarkManifestCase(string Document, string? Expected);
}
