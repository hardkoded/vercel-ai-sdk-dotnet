// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.Gateway.Scripts;
using Script = Vercel.AI.Gateway.Scripts.GenerateModelSettings;

namespace Vercel.AI.Tests.GenerateModelSettings;

/// <summary>Port of <c>packages/gateway/scripts/generate-model-settings.test.ts</c>.</summary>
public sealed class GenerateModelSettingsTests
{
    private const string Prefix = "packages/gateway/scripts/generate-model-settings.test.ts::";

    private const string FileHeader =
        "// Copyright 2023 Vercel, Inc.\n"
        + "// Copyright 2026 Darío Kondratiuk\n"
        + "// SPDX-License-Identifier: Apache-2.0\n\n"
        + "namespace Vercel.AI.Gateway;\n\n";

    public static TheoryData<string[]> DecisionTypes =>
        new()
        {
            new[] { "evaluation" },
            new[] { "decision" },
            new[] { "evaluation", "decision" },
            new[] { "decision", "evaluation" },
        };

    [Theory]
    [MemberData(nameof(DecisionTypes))]
    [UpstreamTest(
        Prefix + "writes one decision settings file for catalog types $types",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5"
    )]
    public async Task Writes_one_decision_settings_file_for_catalog_types(string[] types)
    {
        var run = await RunAsync(types.Select(type => ($"provider/{type}", type)));

        var path = Assert.Single(run.Written).Path;
        Assert.Contains("GatewayDecisionModelSettings.cs", path);

        var ids = types.OrderBy(type => type, StringComparer.Ordinal).ToArray();
        Assert.Equal(
            FileHeader
                + "/// <summary>Known AI Gateway model ids. Maps to <c>GatewayDecisionModelId</c>; any other string is accepted too.</summary>\n"
                + "public static class GatewayDecisionModelId\n{"
                + string.Join(
                    "\n",
                    ids.Select(type =>
                        $"\n    /// <summary><c>provider/{type}</c>.</summary>\n"
                        + $"    public const string Provider{char.ToUpperInvariant(type[0])}{type.Substring(1)} = \"provider/{type}\";"
                    )
                )
                + "\n}\n",
            run.Written[0].Content
        );
        Assert.Equal(1, run.FormatCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "writes the extraction settings file for extraction catalog models",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5"
    )]
    public async Task Writes_the_extraction_settings_file_for_extraction_catalog_models()
    {
        var run = await RunAsync([("provider/extract", "extraction")]);

        var written = Assert.Single(run.Written);
        Assert.Contains("GatewayExtractionModelSettings.cs", written.Path);
        Assert.Equal(
            FileHeader
                + "/// <summary>Known AI Gateway model ids. Maps to <c>GatewayExtractionModelId</c>; any other string is accepted too.</summary>\n"
                + "public static class GatewayExtractionModelId\n{\n"
                + "    /// <summary><c>provider/extract</c>.</summary>\n"
                + "    public const string ProviderExtract = \"provider/extract\";\n"
                + "}\n",
            written.Content
        );
        Assert.Equal(1, run.FormatCalls);
    }

    private static async Task<Run> RunAsync(IEnumerable<(string Id, string Type)> models)
    {
        var body = JsonSerializer.Serialize(
            new { data = models.Select(m => new { id = m.Id, type = m.Type }) }
        );
        using var http = new HttpClient(new StubHandler(body));
        var written = new List<(string Path, string Content)>();
        var formatCalls = 0;
        var host = new GenerateModelSettingsHost(http, "out")
        {
            FileExists = _ => true,
            WriteFile = (path, content) => written.Add((path, content)),
            Format = _ => formatCalls++,
            Log = _ => { },
        };

        await Script.MainAsync(host);
        return new Run(written, formatCalls);
    }

    private sealed record Run(List<(string Path, string Content)> Written, int FormatCalls);

    private sealed class StubHandler(string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                }
            );
    }
}
