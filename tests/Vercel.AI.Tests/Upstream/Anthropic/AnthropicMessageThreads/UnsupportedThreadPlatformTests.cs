// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicMessageThreads.ThreadTestSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicMessageThreads;

/// <summary>Port of <c>anthropic-message-threads.test.ts</c> &gt; <c>unsupported thread platform %s</c>.</summary>
public sealed class UnsupportedThreadPlatformTests
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (var provider in new[] { "bedrock.anthropic.messages", "googleVertex.anthropic.messages" })
        {
            foreach (var method in Methods)
            {
                yield return new object[] { provider, method };
            }
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/anthropic/src/anthropic-message-threads.test.ts::unsupported thread platform %s::rejects %s before making a request", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Rejects_before_making_a_request(string provider, string method)
    {
        var handler = new CaptureHandler();
        var model = new AnthropicLanguageModel(
            AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-key", Name = provider, BaseUrl = "https://unsupported.example/v1" }, handler),
            "claude-sonnet-4-6");

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(
            () => Call(method, handler, BaseOptions("{\"thread\":{\"type\":\"create\"}}"), model));

        Assert.Equal(provider + " does not support Anthropic message threads.", error.Message);
        Assert.Equal(0, handler.Calls);
    }
}
