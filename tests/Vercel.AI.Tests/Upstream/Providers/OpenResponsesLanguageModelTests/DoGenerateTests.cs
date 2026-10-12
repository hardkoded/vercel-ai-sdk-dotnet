// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate::";

    [Fact]
    [UpstreamTest(Prefix + "should throw a descriptive error when the response has no output", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_a_descriptive_error_when_the_response_has_no_output()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = """{"id":"resp_no_output","created_at":1741257730,"model":"gemma-7b-it","status":"incomplete","incomplete_details":{"reason":"content_filter"},"usage":{"input_tokens":10,"output_tokens":0}}""",
        };

        var error = await Assert.ThrowsAsync<ApiException>(() => Model(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None));

        Assert.Contains("Responses API returned no output (content_filter)", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should surface response.error message before the no-output fallback", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_surface_response_error_message_before_the_no_output_fallback()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = """{"id":"resp_error","created_at":1741257730,"model":"gemma-7b-it","status":"failed","error":{"code":"server_error","message":"The upstream provider failed to generate a response."},"usage":{"input_tokens":10,"output_tokens":0}}""",
        };

        var error = await Assert.ThrowsAsync<ApiException>(() => Model(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None));

        Assert.Contains("The upstream provider failed to generate a response.", error.Message, StringComparison.Ordinal);
    }
}
