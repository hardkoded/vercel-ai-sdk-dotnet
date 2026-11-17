// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.DeepSeek;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelSupport;
using ChatModel = Vercel.AI.DeepSeek.DeepSeekChatLanguageModel;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; doGenerate &gt; tool call &gt; json response format with structured outputs</c>.</summary>
public sealed class DoGenerateToolCallJsonResponseFormatWithStructuredOutputsTests
{
    private const string Prefix =
        TestFile + " > doGenerate > tool call > json response format with structured outputs::";

    private const string TestSchema =
        """{"type":"object","properties":{"sentiment":{"type":"string"}},"required":["sentiment"],"additionalProperties":false}""";

    /// <summary>
    /// <c>new DeepSeekChatLanguageModel('deepseek-v4-flash', { provider: 'azure.deepseek', url, headers, supportsStructuredOutputs: true })</c>.
    /// The provider supplies the transport that upstream's <c>headers</c> and <c>fetch</c> give.
    /// </summary>
    private static ChatModel StructuredOutputsModel(UpstreamCapture capture)
    {
        return new ChatModel(
            "deepseek-v4-flash",
            new DeepSeekChatConfig("azure.deepseek", CreateProvider(capture), (_, _) =>
                new Uri("https://api.deepseek.com/chat/completions")
            )
            {
                SupportsStructuredOutputs = true,
            }
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send json_schema response format and skip schema injection",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_json_schema_response_format_and_skip_schema_injection()
    {
        var capture = JsonFixture("deepseek-json");
        var options = TestPrompt();
        options.JsonSchema = UpstreamChat.Json(TestSchema);
        options.JsonSchemaName = "sentiment";

        var result = await StructuredOutputsModel(capture)
            .DoGenerateAsync(options, CancellationToken.None);

        AssertJson(
            RequestBody(capture),
            """
            {
              "messages": [{ "content": "Hello", "role": "user" }],
              "model": "deepseek-v4-flash",
              "response_format": {
                "json_schema": {
                  "name": "sentiment",
                  "schema": {
                    "additionalProperties": false,
                    "properties": { "sentiment": { "type": "string" } },
                    "required": ["sentiment"],
                    "type": "object"
                  },
                  "strict": true
                },
                "type": "json_schema"
              }
            }
            """
        );
        AssertWarnings(result);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should honor strictJsonSchema provider option",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_honor_strictJsonSchema_provider_option()
    {
        var capture = JsonFixture("deepseek-json");
        var options = TestPrompt();
        options.JsonSchema = UpstreamChat.Json(TestSchema);
        options.ProviderOptions = UpstreamChat.Bag("azure", """{"strictJsonSchema":false}""");

        await StructuredOutputsModel(capture).DoGenerateAsync(options, CancellationToken.None);

        AssertMatchObject(
            RequestBody(capture)["response_format"],
            """{"type":"json_schema","json_schema":{"strict":false,"name":"response"}}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should fall back to json_object without a schema",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_fall_back_to_json_object_without_a_schema()
    {
        var capture = JsonFixture("deepseek-json");
        var options = TestPrompt();
        options.JsonResponse = true;

        await StructuredOutputsModel(capture).DoGenerateAsync(options, CancellationToken.None);

        var body = RequestBody(capture);
        AssertJson(body["response_format"], """{"type":"json_object"}""");
        AssertJson(body["messages"]![0], """{"role":"system","content":"Return JSON."}""");
    }
}
