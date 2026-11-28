// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Mistral;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.MistralChatLanguageModelTests.MistralChatTestSupport;

namespace Vercel.AI.Tests.MistralChatLanguageModelTests;

/// <summary>Port of <c>mistral-chat-language-model.test.ts</c> &gt; <c>doStream</c>.</summary>
public sealed class DoStreamTests
{
    [Theory]
    [InlineData("missing", FinishReason.Interrupted)]
    [InlineData("unknown_reason", FinishReason.Other)]
    [InlineData("invalid-json", FinishReason.Error)]
    [UpstreamTest(
        "packages/mistral/src/mistral-chat-language-model.test.ts::doStream::handles a stream ending with %s as %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Handles_a_stream_ending_with_ending_as_unified(
        string ending,
        FinishReason unified
    )
    {
        var handler = new SseStreamHandler(
            new[]
            {
                "data: {\"id\":\"response-1\",\"object\":\"chat.completion.chunk\",\"created\":1,\"model\":\"test-model\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Partial answer\"},\"finish_reason\":null}]}\n\n",
            }.Concat(
                ending switch
                {
                    "missing" => Array.Empty<string>(),
                    "invalid-json" => new[] { "data: {invalid-json}\n\n" },
                    _ => new[]
                    {
                        "data: {\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\""
                            + ending
                            + "\"}]}\n\n",
                    },
                }
            )
        );
        var model = (MistralChatLanguageModel)
            MistralProvider
                .Create(new OpenAICompatibleOptions { ApiKey = "secret" }, handler)
                .LanguageModel("mistral-small-latest");

        var parts = await UpstreamChat.Read(
            await model.DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None)
        );

        Assert.Contains(parts.OfType<TextDeltaStreamPart>(), part =>
            part.Delta == "Partial answer"
        );
        Assert.Equal(ending == "invalid-json" ? 1 : 0, parts.OfType<ErrorStreamPart>().Count());
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(unified, finish.FinishReason);
        Assert.Equal(ending == "unknown_reason" ? ending : null, finish.RawFinishReason);
    }

    private const string RawUsage =
        "{\"prompt_tokens\":20,\"completion_tokens\":2,\"total_tokens\":22,\"prompt_audio_seconds\":1,\"request_count\":1,\"service_tier\":\"standard\",\"num_cached_tokens\":0,\"prompt_tokens_details\":{\"cached_tokens\":0,\"audio_tokens\":1,\"messages\":[{\"role\":\"user\",\"total_tokens\":20,\"settings_tokens\":null,\"truncated\":false,\"usage_count\":1}],\"additional_prompt_detail\":{\"value\":true}},\"prompt_token_details\":{\"cached_tokens\":0,\"audio_tokens\":1,\"additional_prompt_token_detail\":[\"value\"]},\"completion_tokens_details\":{\"reasoning_tokens\":7,\"additional_completion_detail\":\"value\"},\"additional_usage_field\":{\"nested\":true}}";

    [Fact]
    [UpstreamTest(
        "packages/mistral/src/mistral-chat-language-model.test.ts::doStream::should pass the messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_messages()
    {
        var handler = ChunksFixture("mistral-text");

        var result = await Model(handler).DoStreamAsync(TestPrompt(), CancellationToken.None);
        await UpstreamChat.Read(result.Stream);

        var expected = JsonNode.Parse(
            "{\"stream\":true,\"model\":\"mistral-small-latest\",\"messages\":[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]}]}"
        );
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(handler.Body), expected), handler.Body);
    }

    [Fact]
    [UpstreamTest(
        "packages/mistral/src/mistral-chat-language-model.test.ts::doStream::should pass promptCacheKey as prompt_cache_key",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_promptCacheKey_as_prompt_cache_key()
    {
        var handler = ChunksFixture("mistral-text");
        var options = TestPrompt();
        options.ProviderOptions = UpstreamChat.Bag(
            "mistral",
            "{\"promptCacheKey\":\"classification-workflow-123\"}"
        );

        var result = await Model(handler).DoStreamAsync(options, CancellationToken.None);
        await UpstreamChat.Read(result.Stream);

        var body = JsonNode.Parse(handler.Body)!;
        Assert.Equal("classification-workflow-123", body["prompt_cache_key"]!.GetValue<string>());
        Assert.True(body["stream"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest(
        "packages/mistral/src/mistral-chat-language-model.test.ts::doStream::should pass headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_headers()
    {
        var handler = ChunksFixture("mistral-text");
        var options = TestPrompt();
        options.Headers = new Dictionary<string, string?>
        {
            ["Custom-Request-Header"] = "request-header-value",
        };

        var result = await Model(handler, configure: provider =>
                provider.Headers["Custom-Provider-Header"] = "provider-header-value"
            )
            .DoStreamAsync(options, CancellationToken.None);
        await UpstreamChat.Read(result.Stream);

        Assert.Equal("Bearer test-api-key", handler.RequestHeaders["Authorization"]);
        Assert.StartsWith("application/json", handler.RequestHeaders["Content-Type"]);
        Assert.Equal("provider-header-value", handler.RequestHeaders["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.RequestHeaders["Custom-Request-Header"]);
        Assert.Contains("ai-sdk/mistral/", handler.RequestHeaders["User-Agent"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/mistral/src/mistral-chat-language-model.test.ts::doStream::should expose the raw response headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_the_raw_response_headers()
    {
        var handler = ChunksFixture(
            "mistral-text",
            new Dictionary<string, string> { ["test-header"] = "test-value" }
        );

        var result = await Model(handler).DoStreamAsync(TestPrompt(), CancellationToken.None);
        await UpstreamChat.Read(result.Stream);

        var headers = result.Response!.Headers;
        Assert.Equal("no-cache", headers["cache-control"]);
        Assert.Equal("keep-alive", headers["connection"]);
        Assert.StartsWith("text/event-stream", headers["content-type"]);
        Assert.Equal("test-value", headers["test-header"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/mistral/src/mistral-chat-language-model.test.ts::doStream::should preserve the final complete raw usage object without changing normalized usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_the_final_complete_raw_usage_object_without_changing_normalized_usage()
    {
        var handler = ChunksFixture("mistral-usage-details");

        var result = await Model(handler).DoStreamAsync(TestPrompt(), CancellationToken.None);
        var parts = await UpstreamChat.Read(result.Stream);

        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("stop", finish.RawFinishReason);
        var usage = finish.Usage;
        Assert.Equal(20, usage.InputTokens);
        Assert.Equal(20, usage.NoCacheInputTokens);
        Assert.Null(usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(2, usage.OutputTokens);
        Assert.Equal(2, usage.TextTokens);
        Assert.Null(usage.ReasoningTokens);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(usage.Raw!.Value.GetRawText()),
                JsonNode.Parse(RawUsage)
            ),
            usage.Raw!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/mistral/src/mistral-chat-language-model.test.ts::doStream::should send request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_request_body()
    {
        var handler = ChunksFixture("mistral-text");

        var result = await Model(handler).DoStreamAsync(TestPrompt(), CancellationToken.None);
        await UpstreamChat.Read(result.Stream);

        var expected = JsonNode.Parse(
            "{\"messages\":[{\"content\":[{\"text\":\"Hello\",\"type\":\"text\"}],\"role\":\"user\"}],\"model\":\"mistral-small-latest\",\"stream\":true}"
        );
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(result.Request!.Body!.Value.GetRawText()), expected),
            result.Request!.Body!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/mistral/src/mistral-chat-language-model.test.ts::doStream::should avoid duplication when trailing assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_avoid_duplication_when_trailing_assistant_message()
    {
        var handler = Chunks(
            "{\"id\":\"53ff663126294946a6b7a4747b70597e\",\"object\":\"chat.completion.chunk\",\"created\":1750537996,\"model\":\"mistral-small-latest\",\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"\"},\"finish_reason\":null,\"logprobs\":null}]}",
            "{\"id\":\"53ff663126294946a6b7a4747b70597e\",\"object\":\"chat.completion.chunk\",\"created\":1750537996,\"model\":\"mistral-small-latest\",\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"prefix\"},\"finish_reason\":null,\"logprobs\":null}]}",
            "{\"id\":\"53ff663126294946a6b7a4747b70597e\",\"object\":\"chat.completion.chunk\",\"created\":1750537996,\"model\":\"mistral-small-latest\",\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\" and\"},\"finish_reason\":null,\"logprobs\":null}]}",
            "{\"id\":\"53ff663126294946a6b7a4747b70597e\",\"object\":\"chat.completion.chunk\",\"created\":1750537996,\"model\":\"mistral-small-latest\",\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\" more content\"},\"finish_reason\":null,\"logprobs\":null}]}",
            "{\"id\":\"53ff663126294946a6b7a4747b70597e\",\"object\":\"chat.completion.chunk\",\"created\":1750537996,\"model\":\"mistral-small-latest\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"\"},\"finish_reason\":\"stop\",\"logprobs\":null}],\"usage\":{\"prompt_tokens\":4,\"total_tokens\":36,\"completion_tokens\":32}}"
        );
        var model = Model(handler, "mistral-small-latest", options =>
            options.GenerateId = MockId()
        );

        var result = await model.DoStreamAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new UserModelMessage("Hello"),
                    new AssistantModelMessage("prefix ", null, null),
                },
            },
            CancellationToken.None
        );
        var parts = Describe(await UpstreamChat.Read(result.Stream));

        Assert.Equal(
            new[]
            {
                "stream-start:0",
                "response-metadata:53ff663126294946a6b7a4747b70597e:mistral-small-latest:2025-06-21T20:33:16.000Z",
                "text-start:0",
                "text-delta:0:prefix",
                "text-delta:0: and",
                "text-delta:0: more content",
                "text-end:0",
                "finish:Stop:stop:4/4/:32/32",
            },
            parts
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/mistral/src/mistral-chat-language-model.test.ts::doStream::should stream text with content objects",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_text_with_content_objects()
    {
        var handler = Chunks(
            "{\"id\":\"b9e43f82d6c74a1e9f5b2c8e7a9d4f6b\",\"object\":\"chat.completion.chunk\",\"created\":1750538500,\"model\":\"mistral-small-latest\",\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"\"}]},\"finish_reason\":null,\"logprobs\":null}]}",
            "{\"id\":\"b9e43f82d6c74a1e9f5b2c8e7a9d4f6b\",\"object\":\"chat.completion.chunk\",\"created\":1750538500,\"model\":\"mistral-small-latest\",\"choices\":[{\"index\":0,\"delta\":{\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]},\"finish_reason\":null,\"logprobs\":null}]}",
            "{\"id\":\"b9e43f82d6c74a1e9f5b2c8e7a9d4f6b\",\"object\":\"chat.completion.chunk\",\"created\":1750538500,\"model\":\"mistral-small-latest\",\"choices\":[{\"index\":0,\"delta\":{\"content\":[{\"type\":\"text\",\"text\":\", world!\"}]},\"finish_reason\":\"stop\",\"logprobs\":null}],\"usage\":{\"prompt_tokens\":4,\"total_tokens\":36,\"completion_tokens\":32}}"
        );
        var model = Model(handler, "mistral-small-latest", options =>
            options.GenerateId = MockId()
        );

        var result = await model.DoStreamAsync(TestPrompt(), CancellationToken.None);
        var parts = Describe(await UpstreamChat.Read(result.Stream));

        Assert.Equal(
            new[]
            {
                "stream-start:0",
                "response-metadata:b9e43f82d6c74a1e9f5b2c8e7a9d4f6b:mistral-small-latest:2025-06-21T20:41:40.000Z",
                "text-start:0",
                "text-delta:0:Hello",
                "text-delta:0:, world!",
                "text-end:0",
                "finish:Stop:stop:4/4/:32/32",
            },
            parts
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/mistral/src/mistral-chat-language-model.test.ts::doStream::should handle interleaved thinking and text",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_interleaved_thinking_and_text()
    {
        var handler = Chunks(
            "{\"id\":\"interleaved-test\",\"object\":\"chat.completion.chunk\",\"created\":1750538000,\"model\":\"magistral-small-2507\",\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":[{\"type\":\"thinking\",\"thinking\":[{\"type\":\"text\",\"text\":\"First thought.\"}]}]},\"finish_reason\":null}]}",
            "{\"id\":\"interleaved-test\",\"object\":\"chat.completion.chunk\",\"created\":1750538000,\"model\":\"magistral-small-2507\",\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Partial answer.\"}]},\"finish_reason\":null}]}",
            "{\"id\":\"interleaved-test\",\"object\":\"chat.completion.chunk\",\"created\":1750538000,\"model\":\"magistral-small-2507\",\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":[{\"type\":\"thinking\",\"thinking\":[{\"type\":\"text\",\"text\":\"Second thought.\"}]}]},\"finish_reason\":null}]}",
            "{\"id\":\"interleaved-test\",\"object\":\"chat.completion.chunk\",\"created\":1750538000,\"model\":\"magistral-small-2507\",\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Final answer.\"}]},\"finish_reason\":null}]}",
            "{\"id\":\"interleaved-test\",\"object\":\"chat.completion.chunk\",\"created\":1750538000,\"model\":\"magistral-small-2507\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":10,\"total_tokens\":40,\"completion_tokens\":30}}"
        );
        var model = Model(handler, "magistral-small-2507", options =>
            options.GenerateId = MockId()
        );

        var result = await model.DoStreamAsync(TestPrompt(), CancellationToken.None);
        var parts = Describe(await UpstreamChat.Read(result.Stream));

        Assert.Equal(
            new[]
            {
                "stream-start:0",
                "response-metadata:interleaved-test:magistral-small-2507:2025-06-21T20:33:20.000Z",
                "reasoning-start:id-0",
                "reasoning-delta:id-0:First thought.",
                "reasoning-end:id-0",
                "text-start:0",
                "text-delta:0:Partial answer.",
                "text-end:0",
                "reasoning-start:id-1",
                "reasoning-delta:id-1:Second thought.",
                "reasoning-end:id-1",
                "text-start:0",
                "text-delta:0:Final answer.",
                "text-end:0",
                "finish:Stop:stop:10/10/:30/30",
            },
            parts
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/mistral/src/mistral-chat-language-model.test.ts::doStream::should stream raw chunks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_raw_chunks()
    {
        var chunks = new[]
        {
            "{\"id\":\"c7d54e93f8a64b2e9c1f5a8b7d9e2f4c\",\"object\":\"chat.completion.chunk\",\"created\":1750538600,\"model\":\"mistral-large-latest\",\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"Hello\"},\"finish_reason\":null,\"logprobs\":null}]}",
            "{\"id\":\"d8e65fa4g9b75c3f0d2g6b9c8e0f3g5d\",\"object\":\"chat.completion.chunk\",\"created\":1750538601,\"model\":\"mistral-large-latest\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\" world\"},\"finish_reason\":null,\"logprobs\":null}]}",
            "{\"id\":\"e9f76gb5h0c86d4g1e3h7c0d9f1g4h6e\",\"object\":\"chat.completion.chunk\",\"created\":1750538602,\"model\":\"mistral-large-latest\",\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\",\"logprobs\":null}],\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":5,\"total_tokens\":15}}",
        };
        var handler = Chunks(chunks.Append("[DONE]").ToArray());
        var options = TestPrompt();
        options.IncludeRawChunks = true;

        var result = await Model(handler, "mistral-large-latest")
            .DoStreamAsync(options, CancellationToken.None);
        var parts = Describe(await UpstreamChat.Read(result.Stream));

        Assert.Equal(
            new[]
            {
                "stream-start:0",
                "raw:" + chunks[0],
                "response-metadata:c7d54e93f8a64b2e9c1f5a8b7d9e2f4c:mistral-large-latest:2025-06-21T20:43:20.000Z",
                "text-start:0",
                "text-delta:0:Hello",
                "raw:" + chunks[1],
                "text-delta:0: world",
                "raw:" + chunks[2],
                "text-end:0",
                "finish:Stop:stop:10/10/:5/5",
            },
            parts
        );
    }
}
