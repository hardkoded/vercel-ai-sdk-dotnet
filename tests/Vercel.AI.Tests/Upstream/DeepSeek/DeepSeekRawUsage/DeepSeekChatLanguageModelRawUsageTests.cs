// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.DeepSeek;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using DeepSeekChatLanguageModelType = Vercel.AI.DeepSeek.DeepSeekChatLanguageModel;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekRawUsage;

/// <summary>Port of <c>deepseek-raw-usage.test.ts</c> &gt; <c>DeepSeekChatLanguageModel raw usage</c>.</summary>
public sealed class DeepSeekChatLanguageModelRawUsageTests
{
    private const string Prefix =
        "packages/deepseek/src/chat/deepseek-raw-usage.test.ts::DeepSeekChatLanguageModel raw usage::";

    private const string CompleteUsage = """
        {
          "prompt_tokens": 100,
          "completion_tokens": 50,
          "total_tokens": 150,
          "prompt_cache_hit_tokens": 20,
          "prompt_cache_miss_tokens": 80,
          "prompt_tokens_details": {
            "cached_tokens": 20,
            "provider_prompt_metadata": { "preserved": true }
          },
          "completion_tokens_details": {
            "reasoning_tokens": 10,
            "provider_completion_metadata": ["preserved", 1]
          },
          "provider_usage_metadata": {
            "nested": { "preserved": true }
          }
        }
        """;

    private static DeepSeekChatLanguageModelType Chat(HttpMessageHandler handler)
    {
        return DeepSeekProvider
            .Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler)
            .ChatModel("deepseek-v4-flash");
    }

    private static UpstreamCapture CreateResponse(string usage)
    {
        var body = JsonNode
            .Parse(
                """
                {
                  "id": "chatcmpl-raw-usage",
                  "object": "chat.completion",
                  "created": 1785880000,
                  "model": "deepseek-v4-flash",
                  "choices": [
                    {
                      "index": 0,
                      "message": { "role": "assistant", "content": "Hello" },
                      "finish_reason": "stop"
                    }
                  ]
                }
                """
            )!
            .AsObject();
        body["usage"] = JsonNode.Parse(usage);
        return new UpstreamCapture { ResponseBody = body.ToJsonString() };
    }

    private static void AssertCompleteUsage(LanguageModelUsage usage)
    {
        JsonAssert.Equal(usage.Raw!.Value, CompleteUsage);
        Assert.Equal(100, usage.InputTokens);
        Assert.Equal(80, usage.NoCacheInputTokens);
        Assert.Equal(20, usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(50, usage.OutputTokens);
        Assert.Equal(40, usage.TextTokens);
        Assert.Equal(10, usage.ReasoningTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves complete raw usage through doGenerate parsing",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_complete_raw_usage_through_doGenerate_parsing()
    {
        var handler = CreateResponse(CompleteUsage);

        var result = await Chat(handler)
            .DoGenerateAsync(UpstreamChat.Prompt("Hello"), CancellationToken.None);

        AssertCompleteUsage(result.Usage);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves complete raw usage from the final doStream usage event",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_complete_raw_usage_from_the_final_doStream_usage_event()
    {
        const string initialUsage = """
            {
              "prompt_tokens": 0,
              "completion_tokens": 0,
              "total_tokens": 0,
              "provider_usage_metadata": { "stage": "initial" }
            }
            """;
        var handler = new SseStreamHandler(
            new[]
            {
                "data: "
                    + JsonNode
                        .Parse(
                            $$"""{"id":"chatcmpl-raw-usage","object":"chat.completion.chunk","choices":[],"usage":{{initialUsage}}}"""
                        )!
                        .ToJsonString()
                    + "\n\n",
                "data: "
                    + JsonNode
                        .Parse(
                            $$"""{"id":"chatcmpl-raw-usage","object":"chat.completion.chunk","choices":[{"index":0,"delta":{},"finish_reason":"stop"}],"usage":{{CompleteUsage}}}"""
                        )!
                        .ToJsonString()
                    + "\n\n",
                "data: [DONE]\n\n",
            }
        );

        var result = await Chat(handler)
            .DoStreamAsync(UpstreamChat.Prompt("Hello"), CancellationToken.None);
        var parts = await UpstreamChat.Read(result.Stream);
        var finishPart = parts.OfType<FinishStreamPart>().FirstOrDefault();

        Assert.NotNull(finishPart);
        AssertCompleteUsage(finishPart.Usage);
    }

    [Theory]
    [InlineData("prompt_tokens")]
    [InlineData("prompt_tokens_details.cached_tokens")]
    [InlineData("completion_tokens_details.reasoning_tokens")]
    [UpstreamTest(
        Prefix + "continues validating known usage field %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Continues_validating_known_usage_field(string field)
    {
        var usage = JsonNode.Parse(CompleteUsage)!.AsObject();
        switch (field)
        {
            case "prompt_tokens":
                usage["prompt_tokens"] = "100";
                break;
            case "prompt_tokens_details.cached_tokens":
                usage["prompt_tokens_details"]!["cached_tokens"] = "20";
                break;
            default:
                usage["completion_tokens_details"]!["reasoning_tokens"] = "10";
                break;
        }

        var handler = CreateResponse(usage.ToJsonString());

        await Assert.ThrowsAnyAsync<Exception>(() =>
            Chat(handler).DoGenerateAsync(UpstreamChat.Prompt("Hello"), CancellationToken.None)
        );
    }
}
