// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doStream &gt; local shell tool</c>.</summary>
public sealed class DoStreamLocalShellToolTests
{
    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doStream > local shell tool::should stream code local shell results", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_code_local_shell_results()
    {
        var events = OpenAIResponsesSupport.Fixture("openai-local-shell-tool.1.chunks.txt")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => JsonNode.Parse(line)!);
        var capture = new OpenAICapture { ServerSentEvents = OpenAIResponsesSupport.Sse(events) };
        var options = OpenAIUpstream.Hello();
        options.Tools = new[] { LanguageModelTool.ProviderDefined("openai.local_shell", "shell", OpenAIUpstream.Json("{}")) };

        var parts = await OpenAIUpstream.Read(await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5-codex").DoStreamAsync(options, CancellationToken.None));

        OpenAIResponsesSupport.AssertStreamSnapshot(parts, "openai-local-shell-tool.1");
    }
}
