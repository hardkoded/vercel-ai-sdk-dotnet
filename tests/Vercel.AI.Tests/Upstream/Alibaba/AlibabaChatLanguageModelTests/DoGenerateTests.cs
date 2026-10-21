// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests.AlibabaChatTestSupport;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests;

/// <summary>Port of <c>alibaba-chat-language-model.test.ts</c> &gt; <c>doGenerate</c>, the tests outside a nested <c>describe</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Upstream = "packages/alibaba/src/alibaba-chat-language-model.test.ts::doGenerate::";

    private static UpstreamCapture CaptureUsage(string promptTokensDetails)
    {
        return new UpstreamCapture
        {
            ResponseBody = "{\"id\":\"chatcmpl-cache-test\",\"object\":\"chat.completion\",\"created\":1770764844,\"model\":\"qwen-plus\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"Hello\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":100,\"completion_tokens\":50,\"total_tokens\":150,\"prompt_tokens_details\":" + promptTokensDetails + ",\"completion_tokens_details\":{\"reasoning_tokens\":10}}}",
        };
    }

    [Fact]
    [UpstreamTest(Upstream + "should extract usage with cache tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage_with_cache_tokens()
    {
        var capture = CaptureUsage("{\"cached_tokens\":80,\"cache_creation_input_tokens\":20}");
        var model = Model(capture);

        var usage = (await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None)).Usage;

        Assert.Equal(100, usage.InputTokens);
        Assert.Equal(80, usage.CacheReadTokens);
        Assert.Equal(20, usage.CacheWriteTokens);
        Assert.Equal(0, usage.NoCacheInputTokens);
        Assert.Equal(50, usage.OutputTokens);
        Assert.Equal(10, usage.ReasoningTokens);
        Assert.Equal(40, usage.TextTokens);
        AssertJson("{\"completion_tokens\":50,\"completion_tokens_details\":{\"reasoning_tokens\":10},\"prompt_tokens\":100,\"prompt_tokens_details\":{\"cache_creation_input_tokens\":20,\"cached_tokens\":80},\"total_tokens\":150}", JsonNode.Parse(usage.Raw!.Value.GetRawText()));
    }

    [Fact]
    [UpstreamTest(Upstream + "should preserve unmapped usage fields through raw", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_unmapped_usage_fields_through_raw()
    {
        // Alibaba sends cache_type only under explicit context caching.
        var capture = CaptureUsage("{\"cached_tokens\":80,\"cache_creation_input_tokens\":0,\"cache_type\":\"ephemeral\"}");
        var model = Model(capture);

        var usage = (await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None)).Usage;

        Assert.Equal("ephemeral", JsonNode.Parse(usage.Raw!.Value.GetRawText())!["prompt_tokens_details"]!["cache_type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Upstream + "should send enable_thinking in request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_enable_thinking_in_request_body()
    {
        var capture = CaptureFixture("alibaba-text");
        var model = Model(capture);
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = AlibabaOptions("{\"enableThinking\":true,\"thinkingBudget\":2048}");

        await model.DoGenerateAsync(options, CancellationToken.None);

        var body = Body(capture);
        Assert.Equal("qwen-plus", body["model"]!.GetValue<string>());
        Assert.True(body["enable_thinking"]!.GetValue<bool>());
        Assert.Equal(2048, body["thinking_budget"]!.GetValue<int>());
    }
}
