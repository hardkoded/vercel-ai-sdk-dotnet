// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doStream</c> &gt; <c>request</c>.</summary>
public sealed class DoStreamRequestTests
{
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::";
    private const string Suite = Chat + "doStream > request::";
    private const string Body =
        "{\"messages\":[{\"content\":\"you are a friendly bot!\",\"role\":\"system\"},{\"content\":\"Hello\",\"role\":\"user\"}],\"model\":\"command-r-plus\",\"stream\":true}";

    private static LanguageModelCallOptions Options()
    {
        var options = TestPrompt();
        options.IncludeRawChunks = false;
        return options;
    }

    [Fact]
    [UpstreamTest(
        Suite + "should pass the messages and the model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_messages_and_the_model()
    {
        var handler = ChunksFixture("cohere-text");

        await Model(handler).DoStreamAsync(Options(), CancellationToken.None);

        JsonAssert.Equal(Json(handler.Calls[0].Text), Body);
    }

    [Fact]
    [UpstreamTest(Suite + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers()
    {
        var handler = ChunksFixture("cohere-text");
        var options = Options();
        options.Headers = RequestHeaders();

        await ModelWithHeaders(handler).DoStreamAsync(options, CancellationToken.None);

        AssertTestHeaders(handler.Calls[0]);
    }

    [Fact]
    [UpstreamTest(Suite + "should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_request_body()
    {
        var result = await Model(ChunksFixture("cohere-text"))
            .DoStreamAsync(Options(), CancellationToken.None);

        JsonAssert.Equal(result.Request!.Body!.Value, Body);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should expose the raw response headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_the_raw_response_headers()
    {
        var result = await Model(ChunksFixture("cohere-text", ("test-header", "test-value")))
            .DoStreamAsync(Options(), CancellationToken.None);

        Assert.Equal("test-value", result.Response!.Headers["test-header"]);
        Assert.StartsWith("text/event-stream", result.Response.Headers["content-type"]);
    }
}
