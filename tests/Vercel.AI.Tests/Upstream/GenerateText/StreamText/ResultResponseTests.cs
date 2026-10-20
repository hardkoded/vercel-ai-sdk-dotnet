// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

public sealed class ResultResponseTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/stream-text.test.ts::streamText > result.response::should resolve with response information", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_resolve_with_response_information()
    {
        var model = new StepModel(StepModel.HelloStream())
        {
            Response = new LanguageModelStreamResponse(new Dictionary<string, string> { ["call"] = "2" }),
        };
        var result = StepModel.Stream(model, new StreamTextOptions { Prompt = "prompt" });

        var response = await result.Response;
        Assert.Equal(new Dictionary<string, string> { ["call"] = "2" }, response.Headers);
        Assert.Equal("id-0", response.Id);
        var message = Assert.IsType<AssistantModelMessage>(Assert.Single(response.Messages));
        Assert.Equal("Hello", message.Text);
        Assert.Equal("mock-model-id", response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(0), response.Timestamp);
    }
}
