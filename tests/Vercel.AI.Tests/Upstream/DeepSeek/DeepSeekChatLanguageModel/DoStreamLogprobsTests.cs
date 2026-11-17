// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.DeepSeek;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelStreamSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; doStream &gt; logprobs</c>.</summary>
public sealed class DoStreamLogprobsTests
{
    private const string Prefix = TestFile + " > doStream > logprobs::";

    [Fact]
    [UpstreamTest(
        Prefix + "should send logprobs provider options and collect streamed logprobs",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_logprobs_provider_options_and_collect_streamed_logprobs()
    {
        var handler = ChunksFixture("deepseek-logprobs");
        var options = TestPrompt();
        options.ProviderOptions = new DeepSeekLanguageModelChatOptions
        {
            Logprobs = true,
            TopLogprobs = 1,
        }.ToProviderOptions();

        var parts = await Stream(Chat(handler, "deepseek-v4-flash"), options);

        var body = RequestBody(handler);
        Assert.True((bool)body["logprobs"]!);
        Assert.Equal(1, (int)body["top_logprobs"]!);
        AssertJson(
            JsonNode.Parse(Finish(parts).ProviderMetadata!.Value.GetRawText()),
            """
            {
              "deepseek": {
                "choiceIndex": 0,
                "logprobs": {
                  "content": [
                    {
                      "bytes": [79, 75],
                      "logprob": -0.00002467602,
                      "token": "OK",
                      "top_logprobs": [
                        { "bytes": [79, 75], "logprob": -0.00002467602, "token": "OK" }
                      ]
                    }
                  ],
                  "reasoning_content": [
                    {
                      "bytes": null,
                      "logprob": -0.1,
                      "token": "Reasoning",
                      "top_logprobs": [
                        { "bytes": null, "logprob": -0.1, "token": "Reasoning" }
                      ]
                    }
                  ]
                },
                "messageRole": "assistant",
                "promptCacheHitTokens": 0,
                "promptCacheMissTokens": 9,
                "responseObject": "chat.completion.chunk"
              }
            }
            """
        );
    }
}
