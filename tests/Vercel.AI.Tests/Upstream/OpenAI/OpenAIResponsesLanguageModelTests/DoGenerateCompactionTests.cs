// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; compaction</c>.</summary>
public sealed class DoGenerateCompactionTests
{
    private const string Prefix =
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > compaction::";

    private const string ContextManagementOptions =
        """{"store":false,"contextManagement":[{"type":"compaction","compactThreshold":50000}]}""";

    [Fact]
    [UpstreamTest(
        Prefix + "should append an explicit compaction trigger as the final input item",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_append_an_explicit_compaction_trigger_as_the_final_input_item()
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantCustomPart(
                            "openai.compaction",
                            OpenAIUpstream.OpenAIOptionsJson(
                                """{"type":"compaction","itemId":"cmp_123","encryptedContent":"encrypted_compaction_state"}"""
                            )
                        ),
                    }
                ),
                new UserModelMessage("Continue from this context."),
            },
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(
                """{"store":false,"compactionTrigger":true}"""
            ),
        };

        var (_, capture) = await Run(options);

        var expected = JsonNode.Parse(
            """[{"type":"compaction","id":"cmp_123","encrypted_content":"encrypted_compaction_state"},{"role":"user","content":[{"type":"input_text","text":"Continue from this context."}]},{"type":"compaction_trigger"}]"""
        );
        var body = JsonNode.Parse(capture.Body)!;
        Assert.True(JsonNode.DeepEquals(expected, body["input"]), body.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not append a compaction trigger when disabled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_append_a_compaction_trigger_when_disabled()
    {
        var options = OpenAIUpstream.Hello();
        options.ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(
            """{"compactionTrigger":false}"""
        );

        var (_, capture) = await Run(options);

        var expected = JsonNode.Parse(
            """[{"role":"user","content":[{"type":"input_text","text":"Hello"}]}]"""
        );
        var body = JsonNode.Parse(capture.Body)!;
        Assert.True(JsonNode.DeepEquals(expected, body["input"]), body.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should parse compaction output item from real fixture",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_parse_compaction_output_item_from_real_fixture()
    {
        var (result, _) = await Run(WithContextManagement());

        OpenAIResponsesSupport.AssertContentSnapshot(result.Content, "openai-compaction.1");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send context_management in request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_context_management_in_request_body()
    {
        var (_, capture) = await Run(WithContextManagement());

        var body = JsonNode.Parse(capture.Body)!;
        Assert.Equal("gpt-5.2", body["model"]!.GetValue<string>());
        Assert.False(body["store"]!.GetValue<bool>());
        var expected = JsonNode.Parse(
            """[{"type":"compaction","compact_threshold":50000}]"""
        );
        Assert.True(
            JsonNode.DeepEquals(expected, body["context_management"]),
            body.ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include compaction item with encrypted content in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_compaction_item_with_encrypted_content_in_content()
    {
        var (result, _) = await Run(WithContextManagement());

        var compactionPart = result
            .Content.OfType<GeneratedCustom>()
            .FirstOrDefault(part =>
                part.ProviderMetadata?.GetProperty("openai").GetProperty("type").GetString()
                == "compaction"
            );

        Assert.NotNull(compactionPart);
        var metadata = compactionPart.ProviderMetadata!.Value.GetProperty("openai");
        Assert.Equal("compaction", metadata.GetProperty("type").GetString());
        Assert.False(string.IsNullOrEmpty(metadata.GetProperty("itemId").GetString()));
        Assert.False(string.IsNullOrEmpty(metadata.GetProperty("encryptedContent").GetString()));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract usage from compaction response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_usage_from_compaction_response()
    {
        var (result, _) = await Run(WithContextManagement());

        Assert.Equal(51097, result.Usage.InputTokens);
        Assert.Equal(2056, result.Usage.OutputTokens);
    }

    private static LanguageModelCallOptions WithContextManagement()
    {
        var options = OpenAIUpstream.Hello();
        options.ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(ContextManagementOptions);
        return options;
    }

    private static async Task<(LanguageModelGenerateResult Result, OpenAICapture Capture)> Run(
        LanguageModelCallOptions options
    )
    {
        var capture = new OpenAICapture
        {
            ResponseJson = OpenAIResponsesSupport.Fixture("openai-compaction.1.json"),
        };
        var result = await OpenAIUpstream
            .Provider(capture)
            .ResponsesModel("gpt-5.2")
            .DoGenerateAsync(options, CancellationToken.None);
        return (result, capture);
    }
}
