// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Vercel.AI.Gateway.Scripts;

/// <summary>
/// Fills the Gateway model id classes from the Gateway model list. Upstream
/// <c>packages/gateway/scripts/generate-model-settings.ts</c>.
/// </summary>
public static class GenerateModelSettings
{
    /// <summary>Upstream <c>API_URL</c>.</summary>
    public const string ApiUrl = "https://ai-gateway.vercel.sh/v1/models";

    private const string Header =
        "// Copyright 2023 Vercel, Inc.\n"
        + "// Copyright 2026 Darío Kondratiuk\n"
        + "// SPDX-License-Identifier: Apache-2.0\n\n"
        + "namespace Vercel.AI.Gateway;\n\n";

    // Upstream MODALITY_CONFIG. outputFile is the C# file that replaces the .ts settings file.
    private static readonly IReadOnlyDictionary<string, ModalityConfig> s_modalityConfig =
        new Dictionary<string, ModalityConfig>
        {
            ["language"] = new("GatewayLanguageModelSettings.cs", "GatewayModelId"),
            ["embedding"] = new("GatewayEmbeddingModelSettings.cs", "GatewayEmbeddingModelId"),
            ["decision"] = new("GatewayDecisionModelSettings.cs", "GatewayDecisionModelId"),
            ["extraction"] = new("GatewayExtractionModelSettings.cs", "GatewayExtractionModelId"),
            ["image"] = new("GatewayImageModelSettings.cs", "GatewayImageModelId"),
            ["video"] = new("GatewayVideoModelSettings.cs", "GatewayVideoModelId"),
            ["realtime"] = new("GatewayRealtimeModelSettings.cs", "GatewayRealtimeModelId"),
            ["reranking"] = new("GatewayRerankingModelSettings.cs", "GatewayRerankingModelId"),
            ["speech"] = new("GatewaySpeechModelSettings.cs", "GatewaySpeechModelId"),
            ["transcription"] = new(
                "GatewayTranscriptionModelSettings.cs",
                "GatewayTranscriptionModelId"
            ),
        };

    /// <summary>Upstream <c>main</c>. The host replaces <c>fetch</c>, <c>fs</c> and <c>execFileSync</c>.</summary>
    public static async Task MainAsync(
        GenerateModelSettingsHost host,
        CancellationToken cancellationToken = default
    )
    {
        host.Log($"Fetching models from {ApiUrl}");
        var response = await FetchModelsAsync(host.Http, cancellationToken).ConfigureAwait(false);

        var modelsByType = new Dictionary<string, List<string>>();
        foreach (var model in response)
        {
            // Gateway's catalog migrates evaluation models to decision one at a time.
            // so treat evaluation as decision for now.
            var type = model.Type == "evaluation" ? "decision" : model.Type;
            if (!modelsByType.TryGetValue(type, out var ids))
            {
                modelsByType[type] = ids = [];
            }

            ids.Add(model.Id);
        }

        var writtenPaths = new List<string>();
        foreach (var (type, modelIds) in modelsByType)
        {
            var config = GetModalityConfig(type);
            var outputPath = Path.Combine(host.OutputDir, config.OutputFile);

            if (!host.FileExists(outputPath))
            {
                throw new InvalidOperationException(
                    $"Output file does not exist for type '{type}': {config.OutputFile}"
                );
            }

            host.WriteFile(outputPath, GenerateTypeFile(modelIds, config.TypeName));
            writtenPaths.Add(outputPath);
            host.Log($"Generated {config.OutputFile} with {modelIds.Count} models");
        }

        // The repo formatter decides the final shape of the generated files.
        if (writtenPaths.Count > 0)
        {
            host.Format(writtenPaths);
        }

        host.Log("Model settings updated successfully");
    }

    /// <summary>Upstream <c>generateTypeFile</c>.</summary>
    public static string GenerateTypeFile(IEnumerable<string> modelIds, string typeName)
    {
        var sortedIds = modelIds
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        var used = new HashSet<string>(StringComparer.Ordinal);
        var sb = new StringBuilder(Header);
        sb.Append("/// <summary>Known AI Gateway model ids. Maps to <c>")
            .Append(typeName)
            .Append("</c>; any other string is accepted too.</summary>\n");
        sb.Append("public static class ").Append(typeName).Append("\n{");

        for (var i = 0; i < sortedIds.Count; i++)
        {
            var id = sortedIds[i];
            sb.Append(i == 0 ? "\n" : "\n\n");
            sb.Append("    /// <summary><c>").Append(id).Append("</c>.</summary>\n");
            sb.Append("    public const string ")
                .Append(UniqueIdentifier(id, used))
                .Append(" = \"")
                .Append(id.Replace("\\", "\\\\").Replace("\"", "\\\""))
                .Append("\";");
        }

        sb.Append("\n}\n");
        return sb.ToString();
    }

    /// <summary>Upstream <c>getModalityConfig</c>.</summary>
    internal static ModalityConfig GetModalityConfig(string type)
    {
        if (s_modalityConfig.TryGetValue(type, out var config))
        {
            return config;
        }

        var capitalized =
            type.Length == 0 ? type : char.ToUpperInvariant(type[0]) + type.Substring(1);
        return new ModalityConfig(
            $"Gateway{capitalized}ModelSettings.cs",
            $"Gateway{capitalized}ModelId"
        );
    }

    /// <summary>Upstream <c>fetchModels</c>, including the <c>modelsResponseSchema</c> check.</summary>
    internal static async Task<IReadOnlyList<ModelEntry>> FetchModelsAsync(
        HttpClient http,
        CancellationToken cancellationToken
    )
    {
        using var response = await http.GetAsync(ApiUrl, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Failed to fetch models: HTTP {(int)response.StatusCode} from {ApiUrl}"
            );
        }

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        using var document = JsonDocument.Parse(json);
        if (
            document.RootElement.ValueKind != JsonValueKind.Object
            || !document.RootElement.TryGetProperty("data", out var data)
            || data.ValueKind != JsonValueKind.Array
        )
        {
            throw new InvalidOperationException($"Invalid models response from {ApiUrl}");
        }

        var models = new List<ModelEntry>();
        foreach (var item in data.EnumerateArray())
        {
            if (
                item.ValueKind != JsonValueKind.Object
                || !item.TryGetProperty("id", out var id)
                || id.ValueKind != JsonValueKind.String
                || !item.TryGetProperty("type", out var type)
                || type.ValueKind != JsonValueKind.String
            )
            {
                throw new InvalidOperationException($"Invalid models response from {ApiUrl}");
            }

            models.Add(new ModelEntry(id.GetString()!, type.GetString()!));
        }

        return models;
    }

    private static string UniqueIdentifier(string id, HashSet<string> used)
    {
        var name = Identifier(id);
        var candidate = name;
        for (var n = 2; !used.Add(candidate); n++)
        {
            candidate = name + n;
        }

        return candidate;
    }

    private static string Identifier(string id)
    {
        var parts = Regex.Split(id, "[^A-Za-z0-9]+", RegexOptions.None, TimeSpan.FromSeconds(1));
        var sb = new StringBuilder();
        foreach (var part in parts)
        {
            if (part.Length > 0)
            {
                sb.Append(char.ToUpperInvariant(part[0])).Append(part, 1, part.Length - 1);
            }
        }

        if (sb.Length == 0 || char.IsDigit(sb[0]))
        {
            sb.Insert(0, '_');
        }

        return sb.ToString();
    }

    /// <summary>Entry of the <c>data</c> array. Upstream <c>modelSchema</c>.</summary>
    internal sealed record ModelEntry(string Id, string Type);

    /// <summary>Output file and type name of one modality.</summary>
    internal sealed record ModalityConfig(string OutputFile, string TypeName);
}

/// <summary>The side effects of <see cref="GenerateModelSettings"/>, replaceable in tests.</summary>
public sealed class GenerateModelSettingsHost
{
    public GenerateModelSettingsHost(HttpClient http, string outputDir)
    {
        Http = http;
        OutputDir = outputDir;
    }

    public HttpClient Http { get; }

    /// <summary>Upstream <c>OUTPUT_DIR</c>.</summary>
    public string OutputDir { get; }

    public Func<string, bool> FileExists { get; init; } = File.Exists;

    public Action<string, string> WriteFile { get; init; } =
        (path, content) => File.WriteAllText(path, content, new UTF8Encoding(false));

    /// <summary>Upstream <c>execFileSync('oxfmt', …)</c>. Runs CSharpier here.</summary>
    public Action<IReadOnlyList<string>> Format { get; init; } = FormatWithCSharpier;

    public Action<string> Log { get; init; } = Console.WriteLine;

    private static void FormatWithCSharpier(IReadOnlyList<string> paths)
    {
        var info = new ProcessStartInfo("dotnet") { UseShellExecute = false };
        info.ArgumentList.Add("csharpier");
        info.ArgumentList.Add("format");
        foreach (var path in paths)
        {
            info.ArgumentList.Add(path);
        }

        using var process = Process.Start(info)
            ?? throw new InvalidOperationException("Could not start csharpier");
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"csharpier exited with code {process.ExitCode}");
        }
    }
}
