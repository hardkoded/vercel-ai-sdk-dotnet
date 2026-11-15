// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicAwsProvider;

/// <summary>Port of <c>anthropic-aws-provider.test.ts</c> &gt; <c>createAnthropicAws</c>.</summary>
public sealed class CreateAnthropicAwsTests
{
    [Theory]
    [InlineData("create")]
    [InlineData("continue")]
    [UpstreamTest("packages/anthropic-aws/src/anthropic-aws-provider.test.ts::createAnthropicAws::supports thread %s requests", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public async Task Supports_thread_requests(string type)
    {
        var handler = AnthropicParity.Ok();
        var model = AnthropicParity.Aws(handler, new AnthropicOptions { Region = "us-east-1", WorkspaceId = "wrkspc_test", ApiKey = "test-api-key" })
            .LanguageModel("claude-sonnet-4-6");
        var thread = type == "create"
            ? "{\"type\":\"create\"}"
            : "{\"type\":\"continue\",\"previousMessageId\":\"msg_previous\"}";

        await model.DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[] { new SystemModelMessage("Be concise."), new UserModelMessage("Hello") },
                ProviderOptions = new Dictionary<string, JsonElement> { ["anthropic"] = AnthropicParity.Json("{\"thread\":" + thread + "}") },
            },
            CancellationToken.None);

        Assert.Equal("https://aws-external-anthropic.us-east-1.api.aws/v1/messages", handler.Uri);
        using var body = JsonDocument.Parse(handler.Body);
        JsonAssert.Equal(
            body.RootElement.GetProperty("thread"),
            type == "create" ? "{\"type\":\"create\"}" : "{\"type\":\"continue\",\"previous_message_id\":\"msg_previous\"}");
        Assert.Contains("message-threads-2026-08-12", handler.Headers["anthropic-beta"]);
        Assert.Equal("wrkspc_test", handler.Headers["anthropic-workspace-id"]);
        if (type == "continue")
        {
            Assert.False(body.RootElement.TryGetProperty("system", out _));
        }
    }
}
