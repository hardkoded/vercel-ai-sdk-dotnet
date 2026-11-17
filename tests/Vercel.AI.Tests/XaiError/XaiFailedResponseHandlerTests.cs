// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;

namespace Vercel.AI.Tests.XaiError;

/// <summary>Port of <c>xai-error.test.ts</c> &gt; <c>xaiFailedResponseHandler</c>.</summary>
public sealed class XaiFailedResponseHandlerTests
{
    private const string Id =
        "packages/xai/src/xai-error.test.ts::xaiFailedResponseHandler::";

    [Fact]
    [UpstreamTest(
        Id + "extracts message from the nested API error shape",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ExtractsMessageFromTheNestedApiErrorShape()
    {
        var error = Vercel.AI.Xai.XaiError.FailedResponseHandler(
            400,
            """{"error":{"message":"Invalid value: temperature must be between 0 and 2","type":"invalid_request_error","code":"invalid_value"}}"""
        );

        Assert.IsAssignableFrom<ApiException>(error);
        Assert.Equal("Invalid value: temperature must be between 0 and 2", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Id + "extracts message and code from responses api error shape",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ExtractsMessageAndCodeFromResponsesApiErrorShape()
    {
        var error = Vercel.AI.Xai.XaiError.FailedResponseHandler(
            400,
            """{"code":"Client specified an invalid argument","error":"Invalid request content: Each message must have at least one content element."}"""
        );

        Assert.IsAssignableFrom<ApiException>(error);
        Assert.Equal(
            "Client specified an invalid argument: Invalid request content: Each message must have at least one content element.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "extracts message from text to speech error shape",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ExtractsMessageFromTextToSpeechErrorShape()
    {
        var error = Vercel.AI.Xai.XaiError.FailedResponseHandler(
            400,
            """{"error":"speed must be between 0.7 and 1.5"}"""
        );

        Assert.IsAssignableFrom<ApiException>(error);
        Assert.Equal("speed must be between 0.7 and 1.5", error.Message);
    }
}
