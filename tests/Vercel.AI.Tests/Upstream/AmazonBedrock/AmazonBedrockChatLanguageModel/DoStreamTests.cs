// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModel;

public sealed class DoStreamTests
{
    private const string TextResponse = "{\"output\":{\"message\":{\"content\":[{\"text\":\"Hello\"}],\"role\":\"assistant\"}},\"stopReason\":\"stop_sequence\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1,\"totalTokens\":2}}";

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doStream::should properly combine headers from all sources",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_properly_combine_headers_from_all_sources()
    {
        var handler = new UpstreamRecordingHandler(TextResponse);
        await HeadersTestHelper.Stream(
            handler,
            new Dictionary<string, string?> { ["model-header"] = "model-value", ["shared-header"] = "model-shared" },
            new Dictionary<string, string?> { ["options-header"] = "options-value", ["shared-header"] = "options-shared" });

        Assert.Equal("options-value", handler.Headers["options-header"]);
        Assert.Equal("model-value", handler.Headers["model-header"]);
        Assert.Equal("options-shared", handler.Headers["shared-header"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doStream::should work with partial headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_work_with_partial_headers()
    {
        var handler = new UpstreamRecordingHandler(TextResponse);
        await HeadersTestHelper.Stream(
            handler,
            new Dictionary<string, string?> { ["model-header"] = "model-value" },
            new Dictionary<string, string?> { ["options-header"] = "options-value" });

        Assert.Equal("options-value", handler.Headers["options-header"]);
        Assert.Equal("model-value", handler.Headers["model-header"]);
    }
}
