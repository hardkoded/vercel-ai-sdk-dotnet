// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Util;
using Vercel.AI.WebFetch;

namespace Vercel.AI.Tests.Upstream.WebFetchTool;

/// <summary>Port of <c>web-fetch-tool.test.ts</c> &gt; <c>createWebFetchTool</c>.</summary>
public sealed class CreateWebFetchToolTests
{
    [Fact]
    [UpstreamTest(WebFetchToolFixture.FilePath + "::createWebFetchTool::executes as a function tool in the standard AI SDK loop", Coverage = UpstreamCoverage.Covered)]
    public async Task ExecutesAsAFunctionToolInTheStandardAiSdkLoop()
    {
        var fetch = new WebFetchToolFixture.RecordingFetch((_, _) => DownloadResponse.Text("# Docs\n\nHello.", "text/markdown"));
        var webFetch = WebFetchTools.Create(new WebFetchToolOptions { Fetch = fetch.Fetch });
        var model = new TestLanguageModel();
        var modelCalls = 0;
        model.OnGenerate = _ => ++modelCalls == 1
            ? new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("fetch-docs", "webFetch", "{\"url\":\"https://example.com/docs\"}") },
                FinishReason.ToolCalls,
                new LanguageModelUsage(1, 1, 2),
                "tool_calls")
            : TestLanguageModel.Text("The docs say hello.");

        var result = await new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = model,
                Tools = new[] { webFetch },
                Prompt = "Read https://example.com/docs",
                StopWhen = StopWhen.IsStepCount(2),
            });

        Assert.Equal("The docs say hello.", result.Text);
        using var output = System.Text.Json.JsonDocument.Parse(result.Steps[0].ToolResults[0].OutputJson);
        Assert.Equal("document", output.RootElement.GetProperty("type").GetString());
        Assert.Equal("# Docs\n\nHello.", output.RootElement.GetProperty("content").GetString());
        Assert.Equal("webFetch", Assert.Single(model.Calls[0].Tools!).Name);
        var toolMessage = Assert.Single(model.Calls[1].Prompt.OfType<ToolModelMessage>());
        Assert.Equal("fetch-docs", toolMessage.ToolCallId);
        Assert.Equal("webFetch", toolMessage.ToolName);
        Assert.Equal("text", toolMessage.OutputType);
        Assert.Equal("URL: https://example.com/docs\nContent-Type: text/markdown\n\n# Docs\n\nHello.", toolMessage.OutputJson);
    }
}
