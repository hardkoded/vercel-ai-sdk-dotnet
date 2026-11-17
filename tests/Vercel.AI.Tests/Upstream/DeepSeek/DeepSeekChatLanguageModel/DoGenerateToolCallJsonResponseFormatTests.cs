// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; doGenerate &gt; tool call &gt; json response format</c>.</summary>
public sealed class DoGenerateToolCallJsonResponseFormatTests
{
    private const string Prefix = TestFile + " > doGenerate > tool call > json response format::";

    private const string ElementsSchema =
        """{"type":"object","properties":{"elements":{"type":"array","items":{"type":"object","properties":{"location":{"type":"string"},"temperature":{"type":"number"},"condition":{"type":"string"}},"required":["location","temperature","condition"],"additionalProperties":false}}},"required":["elements"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}""";

    private const string ExpectedTools = """
        [
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
        """;

    private static LanguageModelCallOptions Options()
    {
        var options = TestPrompt();
        options.JsonResponse = true;
        options.Tools = new[] { WeatherTool() };
        options.ProviderOptions = DeepSeekOptions("""{"thinking":{"type":"enabled"}}""");
        return options;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send correct request body without schema",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_correct_request_body_without_schema()
    {
        var capture = JsonFixture("deepseek-json");

        await Model(capture, "deepseek-reasoner")
            .DoGenerateAsync(Options(), CancellationToken.None);

        AssertJson(
            RequestBody(capture),
            """
            {
              "messages": [
                { "content": "Return JSON.", "role": "system" },
                { "content": "Hello", "role": "user" }
              ],
              "model": "deepseek-reasoner",
              "response_format": { "type": "json_object" },
              "thinking": { "type": "enabled" },
              "tools": __TOOLS__
            }
            """.Replace("__TOOLS__", ExpectedTools)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send correct request body with schema",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_correct_request_body_with_schema()
    {
        var capture = JsonFixture("deepseek-json");
        var options = Options();
        options.JsonSchema = UpstreamChat.Json(ElementsSchema);

        await Model(capture, "deepseek-reasoner").DoGenerateAsync(options, CancellationToken.None);

        var expected = JsonNode.Parse(
            """
            {
              "messages": [
                { "content": "__SYSTEM__", "role": "system" },
                { "content": "Hello", "role": "user" }
              ],
              "model": "deepseek-reasoner",
              "response_format": { "type": "json_object" },
              "thinking": { "type": "enabled" },
              "tools": __TOOLS__
            }
            """.Replace("__TOOLS__", ExpectedTools)
        )!;
        expected["messages"]![0]!["content"] =
            "Return JSON that conforms to the following schema: " + ElementsSchema;
        AssertJson(RequestBody(capture), expected.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract text content",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_extract_text_content()
    {
        var capture = JsonFixture("deepseek-json");

        var result = await Model(capture, "deepseek-reasoner")
            .DoGenerateAsync(Options(), CancellationToken.None);

        MatchSnapshot("deepseek-json.generate.snap.json", result, capture);
    }
}
