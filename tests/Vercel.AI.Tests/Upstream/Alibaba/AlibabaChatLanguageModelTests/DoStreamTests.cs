// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Alibaba;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests;

/// <summary>Port of <c>alibaba-chat-language-model.test.ts</c> &gt; <c>doStream</c>.</summary>
public sealed class DoStreamTests
{
    [Theory]
    [InlineData("missing", FinishReason.Interrupted)]
    [InlineData("unknown_reason", FinishReason.Other)]
    [InlineData("invalid-json", FinishReason.Error)]
    [UpstreamTest("packages/alibaba/src/alibaba-chat-language-model.test.ts::doStream::handles a stream ending with %s as %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_a_stream_ending_with_ending_as_unified(string ending, FinishReason unified)
    {
        var handler = new SseStreamHandler(
            new[]
            {
                "data: {\"id\":\"response-1\",\"object\":\"chat.completion.chunk\",\"created\":1,\"model\":\"test-model\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Partial answer\"},\"finish_reason\":null}]}\n\n",
            }.Concat(ending switch
            {
                "missing" => Array.Empty<string>(),
                "invalid-json" => new[] { "data: {invalid-json}\n\n" },
                _ => new[] { "data: {\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"" + ending + "\"}]}\n\n" },
            }));
        var model = (OpenAICompatibleLanguageModel)AlibabaProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, handler).LanguageModel("qwen-plus");

        var parts = await UpstreamChat.Read(await model.DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));

        Assert.Contains(parts.OfType<TextDeltaStreamPart>(), part => part.Delta == "Partial answer");
        Assert.Equal(ending == "invalid-json" ? 1 : 0, parts.OfType<ErrorStreamPart>().Count());
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(unified, finish.FinishReason);
        Assert.Equal(ending == "unknown_reason" ? ending : null, finish.RawFinishReason);
    }
}
