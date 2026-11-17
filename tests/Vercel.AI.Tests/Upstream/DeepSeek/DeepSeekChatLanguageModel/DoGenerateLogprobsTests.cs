// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; doGenerate &gt; logprobs</c>.</summary>
public sealed class DoGenerateLogprobsTests
{
    private const string Prefix = TestFile + " > doGenerate > logprobs::";

    [Fact]
    [UpstreamTest(
        Prefix + "should send logprobs provider options",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_logprobs_provider_options()
    {
        var capture = JsonFixture("deepseek-logprobs");
        var options = TestPrompt();
        options.ProviderOptions = DeepSeekOptions("""{"logprobs":true,"topLogprobs":1}""");

        await Model(capture, "deepseek-v4-flash").DoGenerateAsync(options, CancellationToken.None);

        AssertJson(
            RequestBody(capture),
            """
            {
              "logprobs": true,
              "messages": [{ "content": "Hello", "role": "user" }],
              "model": "deepseek-v4-flash",
              "top_logprobs": 1
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should enable logprobs when topLogprobs is set",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_enable_logprobs_when_topLogprobs_is_set()
    {
        var capture = JsonFixture("deepseek-logprobs");
        var options = TestPrompt();
        options.ProviderOptions = DeepSeekOptions("""{"topLogprobs":1}""");

        await Model(capture, "deepseek-v4-flash").DoGenerateAsync(options, CancellationToken.None);

        AssertMatchObject(RequestBody(capture), """{"logprobs":true,"top_logprobs":1}""");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract content and reasoning logprobs",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_extract_content_and_reasoning_logprobs()
    {
        var capture = JsonFixture("deepseek-logprobs");
        var options = TestPrompt();
        options.ProviderOptions = DeepSeekOptions("""{"logprobs":true}""");

        var result = await Model(capture, "deepseek-v4-flash")
            .DoGenerateAsync(options, CancellationToken.None);

        AssertJson(
            DeepSeekMetadata(result)["logprobs"],
            """
            {
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
            }
            """
        );
    }
}
