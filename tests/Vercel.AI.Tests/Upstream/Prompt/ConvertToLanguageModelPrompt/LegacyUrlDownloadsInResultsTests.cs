// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt;

/// <summary>Port of <c>convert-to-language-model-prompt.test.ts</c> &gt; <c>legacy URL downloads in %s results</c>.</summary>
public sealed class LegacyUrlDownloadsInResultsTests
{
    private const string Id =
        "packages/ai/src/prompt/convert-to-language-model-prompt.test.ts::legacy URL downloads in %s results::";

    [Theory]
    [InlineData("tool", false)]
    [InlineData("tool", true)]
    [InlineData("assistant", false)]
    [InlineData("assistant", true)]
    [UpstreamTest(
        Id + "respects URL support (%s) and preserves file options",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f630e0d735688a2e50c8d904436cb0f3ea25f33c"
    )]
    public async Task Respects_URL_support_and_preserves_file_options(string role, bool supported)
    {
        const string url = "https://example.com";
        var providerOptions = new Dictionary<string, JsonElement>
        {
            ["test"] = JsonDocument.Parse("{\"detail\":\"high\"}").RootElement.Clone(),
        };
        var output = new UserContentPart[]
        {
            new FileUrlContentPart(url, "application/pdf") { ProviderOptions = providerOptions },
        };
        var data = new byte[] { 37, 80, 68, 70 };
        var fetchCalls = 0;
        var download = Download.CreateDownload(
            fetch: (_, _) =>
            {
                Interlocked.Increment(ref fetchCalls);
                return Task.FromResult(DownloadResponse.Bytes(data, "application/pdf"));
            }
        );
        var toolResult = new ToolModelMessage("call", "asset", output);
        ModelMessage message =
            role == "tool"
                ? toolResult
                : new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantToolResultPart("call", "asset", toolResult),
                    }
                );

        var assets = await LanguageModelPrompts.DownloadAssetsAsync(
            new[] { message },
            download,
            supported
                ? new Dictionary<string, string> { ["application/pdf"] = "^https://example\\.com/" }
                : new Dictionary<string, string>()
        );

        Assert.Equal(supported ? 0 : 1, fetchCalls);
        var mapped = LanguageModelPrompts.MapToolResultOutput(output, assets);
        var file = Assert.IsType<FileContentPart>(Assert.Single(mapped));
        if (supported)
        {
            Assert.Equal(new Uri(url).ToString(), new Uri(file.Url!).ToString());
            Assert.Null(file.Data);
        }
        else
        {
            Assert.Null(file.Url);
            Assert.Equal(data, file.Data);
        }

        Assert.Equal("application/pdf", file.MediaType);
        Assert.Same(providerOptions, file.ProviderOptions);
    }
}
