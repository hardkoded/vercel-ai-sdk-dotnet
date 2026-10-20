// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

public sealed class ResultRequestAndResponseTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > result.request::should exclude request body and request messages by default", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_exclude_request_body_and_request_messages_by_default()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("Hello, world!") };

        var result = await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "prompt" });

        Assert.Null(result.Request.Body);
        Assert.Null(result.Request.Messages);
        Assert.Same(result.Steps[0].Request, result.Request);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > result.response::should contain response body and headers",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts id, timestamp, model id, headers and messages. The port has no generateText include.responseBody, so the response body is not asserted.")]
    public async Task Should_contain_response_body_and_headers()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("Hello, world!") },
                FinishReason.Stop,
                new LanguageModelUsage(3, 10, 13),
                "stop",
                responseId: "test-id-from-model",
                responseModelId: "test-response-model-id",
                responseTimestamp: DateTimeOffset.FromUnixTimeMilliseconds(10000),
                responseHeaders: new Dictionary<string, string> { ["custom-response-header"] = "response-header-value" }),
        };

        var result = await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "prompt" });

        Assert.Same(result.Steps[0].Response, result.Response);
        Assert.Equal("test-id-from-model", result.Response.Id);
        Assert.Equal("test-response-model-id", result.Response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(10000), result.Response.Timestamp);
        Assert.Equal(new Dictionary<string, string> { ["custom-response-header"] = "response-header-value" }, result.Response.Headers);
        var message = Assert.IsType<AssistantModelMessage>(Assert.Single(result.Response.Messages));
        Assert.Equal("Hello, world!", message.Text);
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
