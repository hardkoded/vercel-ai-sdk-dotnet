// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; on-demand compaction</c>.</summary>
public sealed class DoGenerateOnDemandCompactionTests
{
    private const string Prefix = "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > on-demand compaction::";

    [Fact]
    [UpstreamTest(Prefix + "should send compaction in the request body and add the beta header", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_compaction_in_the_request_body_and_add_the_beta_header()
    {
        var handler = AnthropicParity.Ok();
        var options = AnthropicParity.Hello();
        options.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["anthropic"] = AnthropicParity.Json("{\"compaction\":{\"type\":\"summarize\",\"instructions\":\"Preserve decisions and unresolved questions.\"}}"),
        };

        await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-haiku-4-5", options);

        var body = JsonNode.Parse(handler.Body)!;
        Assert.Equal("summarize", body["compaction"]!["type"]!.GetValue<string>());
        Assert.Equal("Preserve decisions and unresolved questions.", body["compaction"]!["instructions"]!.GetValue<string>());
        Assert.Contains("compact-2026-09-04", handler.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject compaction together with context management", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_compaction_together_with_context_management()
    {
        var handler = AnthropicParity.Ok();
        var options = AnthropicParity.Hello();
        options.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["anthropic"] = AnthropicParity.Json("{\"compaction\":{\"type\":\"summarize\"},\"contextManagement\":{\"edits\":[{\"type\":\"clear_tool_uses_20250919\"}]}}"),
        };

        var exception = await Assert.ThrowsAsync<InvalidArgumentException>(
            () => AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-haiku-4-5", options));

        Assert.Equal("providerOptions", exception.Parameter);
        Assert.Contains("Anthropic provider options `compaction` and `contextManagement` cannot be used together.", exception.Message);
        Assert.Equal(string.Empty, handler.Body);
    }

    [Fact]
    [UpstreamTest(Prefix + "should replay signed compaction blocks and add the beta header", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_replay_signed_compaction_blocks_and_add_the_beta_header()
    {
        var handler = AnthropicParity.Ok();
        var compactionOptions = new Dictionary<string, JsonElement>
        {
            ["anthropic"] = AnthropicParity.Json("{\"type\":\"compaction\",\"signature\":\"compaction-signature\"}"),
        };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("Summary of the conversation.", compactionOptions) }),
                new UserModelMessage("Continue the conversation."),
            },
        };

        await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-haiku-4-5", options);

        var body = JsonNode.Parse(handler.Body)!;
        var messages = body["messages"]!.AsArray();
        Assert.Equal("assistant", messages[0]!["role"]!.GetValue<string>());
        var block = messages[0]!["content"]![0]!;
        Assert.Equal("compaction", block["type"]!.GetValue<string>());
        Assert.Equal("Summary of the conversation.", block["content"]!.GetValue<string>());
        Assert.Equal("compaction-signature", block["signature"]!.GetValue<string>());
        Assert.Equal("user", messages[1]!["role"]!.GetValue<string>());
        Assert.Equal("Continue the conversation.", messages[1]!["content"]![0]!["text"]!.GetValue<string>());
        Assert.Contains("compact-2026-09-04", handler.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve the signature on compaction responses", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_the_signature_on_compaction_responses()
    {
        var handler = AnthropicParity.Ok(
            "{\"id\":\"msg_compaction\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"compaction\",\"content\":\"Summary of the conversation.\",\"signature\":\"compaction-signature\"}],\"model\":\"claude-opus-5\",\"stop_reason\":\"compaction\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":0,\"output_tokens\":0,\"iterations\":[{\"type\":\"compaction\",\"input_tokens\":120,\"output_tokens\":30}]}}");

        var result = await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-opus-5");

        var text = Assert.IsAssignableFrom<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Summary of the conversation.", text.Text);
        JsonAssert.Equal(text.ProviderMetadata!.Value, "{\"anthropic\":{\"type\":\"compaction\",\"signature\":\"compaction-signature\"}}");
        Assert.Equal(FinishReason.Other, result.FinishReason);
        Assert.Equal("compaction", result.RawFinishReason);
    }
}
