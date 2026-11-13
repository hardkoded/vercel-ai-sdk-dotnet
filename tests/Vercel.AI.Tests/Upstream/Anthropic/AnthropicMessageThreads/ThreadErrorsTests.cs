// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicMessageThreads.ThreadTestSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicMessageThreads;

/// <summary>Port of <c>anthropic-message-threads.test.ts</c> &gt; <c>thread errors</c>.</summary>
public sealed class ThreadErrorsTests
{
    public static IEnumerable<object[]> EachMethod() => Each();

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest("packages/anthropic/src/anthropic-message-threads.test.ts::thread errors::preserves recovery details for %s", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Preserves_recovery_details(string method)
    {
        var handler = new CaptureHandler
        {
            Status = HttpStatusCode.NotFound,
            ResponseBody = "{\"type\":\"error\",\"error\":{\"type\":\"not_found_error\",\"message\":\"Thread not found\",\"details\":{\"error_code\":\"thread_not_found\"}}}",
        };
        var model = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-key" }, handler).LanguageModel("claude-sonnet-4-6");
        var input = BaseOptions("{\"thread\":{\"type\":\"continue\",\"previousMessageId\":\"msg_expired\"}}");

        var error = await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            if (method == "generate")
            {
                await model.DoGenerateAsync(input, CancellationToken.None);
            }
            else
            {
                await model.DoStreamAsync(input, CancellationToken.None);
            }
        });

        Assert.Equal(404, error.StatusCode);
        var data = AnthropicErrorData.Parse(error.ResponseBody!);
        Assert.True(data.Success);
        Assert.Equal("thread_not_found", data.ErrorCode);
        Assert.Equal("thread_not_found", data.Value()!["error"]!["details"]!["error_code"]!.GetValue<string>());
    }
}
