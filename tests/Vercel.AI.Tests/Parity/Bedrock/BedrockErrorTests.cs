// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AmazonBedrock;

namespace Vercel.AI.Tests;

/// <summary>Bedrock error-body formatting.</summary>
public sealed class BedrockErrorTests
{
    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-error.test.ts::amazonBedrockFailedResponseHandler::preserves the provider message when the error type is omitted", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_the_message_when_type_is_omitted()
    {
        Assert.Equal("boom", AmazonBedrockErrors.Format("{\"message\":\"boom\"}"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-error.test.ts::amazonBedrockFailedResponseHandler::prefixes the provider message when the error type is present", Coverage = UpstreamCoverage.Covered)]
    public void Prefixes_the_message_with_the_error_type()
    {
        Assert.Equal("ValidationException: boom", AmazonBedrockErrors.Format("{\"type\":\"ValidationException\",\"message\":\"boom\"}"));
    }
}
