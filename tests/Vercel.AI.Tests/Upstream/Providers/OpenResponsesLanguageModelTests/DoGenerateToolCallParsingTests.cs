// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate &gt; tool call parsing</c>.</summary>
public sealed class DoGenerateToolCallParsingTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate > tool call parsing::";

    [Fact]
    [UpstreamTest(Prefix + "should parse tool call from response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_parse_tool_call_from_response()
    {
        var result = await Generate();

        JsonAssert.Equal(ToJson(result.Content), """
            [{"input":"{\"location\":\"San Francisco\"}","providerMetadata":{"lmstudio":{"itemId":"fc_ru0kcno9erlzp8573yub"}},"toolCallId":"call_2866856768160095","toolName":"weather","type":"tool-call"}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return tool-calls finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_tool_calls_finish_reason()
    {
        var result = await Generate();

        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Null(result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract usage correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage_correctly()
    {
        var result = await Generate();

        JsonAssert.Equal(Usage(result.Usage), """
            {
              "inputTokens":{"cacheRead":891,"noCache":298,"total":1189},
              "outputTokens":{"reasoning":0,"text":11,"total":11},
              "raw":{"input_tokens":1189,"input_tokens_details":{"cached_tokens":891},"output_tokens":11,"output_tokens_details":{"reasoning_tokens":0},"total_tokens":1200}
            }
            """);
    }

    private static Task<LanguageModelGenerateResult> Generate()
    {
        var options = UpstreamChat.Prompt();
        options.Tools = new[]
        {
            Tool("weather", "Get the weather in a location", """{"type":"object","properties":{"location":{"type":"string","description":"The location to get the weather for"}},"required":["location"]}"""),
        };
        options.ToolChoice = ToolChoice.Required;
        return Model(JsonFixture("lmstudio-tool-call.1")).DoGenerateAsync(options, CancellationToken.None);
    }
}
