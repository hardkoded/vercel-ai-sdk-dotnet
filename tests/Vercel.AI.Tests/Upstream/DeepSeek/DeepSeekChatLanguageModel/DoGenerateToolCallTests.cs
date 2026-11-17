// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; doGenerate &gt; tool call</c>.</summary>
public sealed class DoGenerateToolCallTests
{
    private const string Prefix = TestFile + " > doGenerate > tool call::";

    [Fact]
    [UpstreamTest(
        Prefix + "should send correct request body",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_correct_request_body()
    {
        var capture = JsonFixture("deepseek-tool-call");
        var options = TestPrompt();
        options.Tools = new[] { WeatherTool() };
        options.ProviderOptions = DeepSeekOptions("""{"thinking":{"type":"enabled"}}""");

        await Model(capture, "deepseek-reasoner").DoGenerateAsync(options, CancellationToken.None);

        AssertJson(
            RequestBody(capture),
            """
            {
              "messages": [{ "content": "Hello", "role": "user" }],
              "model": "deepseek-reasoner",
              "thinking": { "type": "enabled" },
              "tools": [
                {
                  "function": {
                    "name": "weather",
                    "parameters": {
                      "$schema": "http://json-schema.org/draft-07/schema#",
                      "additionalProperties": false,
                      "properties": { "location": { "type": "string" } },
                      "required": ["location"],
                      "type": "object"
                    }
                  },
                  "type": "function"
                }
              ]
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract tool call content",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_extract_tool_call_content()
    {
        var capture = JsonFixture("deepseek-tool-call");
        var options = TestPrompt();
        options.Tools = new[] { WeatherTool() };
        options.ProviderOptions = DeepSeekOptions("""{"thinking":{"type":"enabled"}}""");

        var result = await Model(capture, "deepseek-reasoner")
            .DoGenerateAsync(options, CancellationToken.None);

        MatchSnapshot("deepseek-tool-call.generate.snap.json", result, capture);
    }
}
