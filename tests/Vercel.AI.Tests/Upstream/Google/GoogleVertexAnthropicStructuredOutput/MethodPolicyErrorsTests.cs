// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Google.GoogleVertexAnthropicStructuredOutput.VertexAnthropicStructuredOutputSupport;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexAnthropicStructuredOutput;

/// <summary>Port of <c>describe.each(['generate', 'stream'])('%s policy errors')</c>.</summary>
public sealed class MethodPolicyErrorsTests
{
    private const string Prefix = "packages/google-vertex/src/anthropic/google-vertex-anthropic-structured-output.test.ts::%s policy errors::";
    private const string Message = "Organization Policy constraint constraints/vertexai.allowedPartnerModelFeatures violated: structured_outputs is disallowed.";

    [Theory]
    [InlineData("generate", "anthropic")]
    [InlineData("generate", "google")]
    [InlineData("stream", "anthropic")]
    [InlineData("stream", "google")]
    [UpstreamTest(Prefix + "does not retry a %s-format policy rejection with tool emulation", Coverage = UpstreamCoverage.Covered, Commit = "4b8f00efff97de78e52667359aa33267a1036c46", Note = "isRetryable is not exposed on ApiException; BadRequestException is never retried.")]
    public async Task Does_not_retry_a_format_policy_rejection_with_tool_emulation(string method, string format)
    {
        var handler = new Handler(method, false, Report)
        {
            Status = System.Net.HttpStatusCode.BadRequest,
            ErrorBody = format == "anthropic"
                ? "{\"type\":\"error\",\"error\":{\"type\":\"invalid_request_error\",\"message\":\"" + Message + "\"}}"
                : "{\"error\":{\"code\":400,\"message\":\"" + Message + "\",\"status\":\"INVALID_ARGUMENT\"}}",
        };
        var model = Model("claude-opus-5", handler, true);

        var error = await Assert.ThrowsAsync<BadRequestException>(() => Call(model, method, Options(ReportSchema))).ConfigureAwait(false);

        Assert.Equal(400, error.StatusCode);
        // The port's ApiException has no IsRetryable; a 400 is BadRequestException, which the retry policy never retries.
        if (format == "anthropic")
        {
            Assert.Equal(Message, error.Message);
        }
        else
        {
            Assert.Contains(Message, error.ResponseBody);
        }

        Assert.Equal(1, handler.Calls);
    }
}
