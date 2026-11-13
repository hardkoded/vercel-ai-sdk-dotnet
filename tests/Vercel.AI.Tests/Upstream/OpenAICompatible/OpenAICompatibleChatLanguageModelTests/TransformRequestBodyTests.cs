// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAICompatibleChatLanguageModelTests;

/// <summary>Port of <c>openai-compatible-chat-language-model.test.ts</c> &gt; <c>transformRequestBody</c>.</summary>
public sealed class TransformRequestBodyTests
{
    private const string UpstreamCommit = "c9c6402cbdfce3f82b36b4073236043b1946dd06";
    private const string Prefix = "packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::transformRequestBody::";
    private const string StreamBody = "{\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Hello\"},\"finish_reason\":\"stop\"}]}";

    [Fact]
    [UpstreamTest(Prefix + "should transform request body in doGenerate when transformRequestBody is provided", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_transform_request_body_in_doGenerate_when_transformRequestBody_is_provided()
    {
        var capture = new UpstreamCapture();
        var calls = new List<JsonObject>();
        var model = UpstreamChat.Model(capture, "test-provider", "grok-3", options => options.TransformRequestBody = (body, _) =>
        {
            calls.Add((JsonObject)body.DeepClone());
            body["custom_field"] = "added-by-transform";
            return body;
        });

        await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        var call = Assert.Single(calls);
        Assert.Equal("grok-3", call["model"]!.GetValue<string>());
        Assert.Equal("Hello", call["messages"]![0]!["content"]!.GetValue<string>());
        Assert.Equal("added-by-transform", UpstreamChat.Body(capture)["custom_field"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "should transform request body in doStream when transformRequestBody is provided", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_transform_request_body_in_doStream_when_transformRequestBody_is_provided()
    {
        var capture = new UpstreamCapture { ResponseBody = UpstreamChat.Sse(StreamBody), MediaType = "text/event-stream" };
        var calls = new List<JsonObject>();
        var model = UpstreamChat.Model(capture, "test-provider", "grok-3", options => options.TransformRequestBody = (body, _) =>
        {
            calls.Add((JsonObject)body.DeepClone());
            body["custom_field"] = "added-by-transform";
            return body;
        });

        await UpstreamChat.Read(await model.DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));

        var call = Assert.Single(calls);
        Assert.Equal("grok-3", call["model"]!.GetValue<string>());
        Assert.Equal("Hello", call["messages"]![0]!["content"]!.GetValue<string>());
        Assert.True(call["stream"]!.GetValue<bool>());
        Assert.Equal("added-by-transform", UpstreamChat.Body(capture)["custom_field"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "should work without transformRequestBody", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_work_without_transformRequestBody()
    {
        var capture = new UpstreamCapture();
        var model = UpstreamChat.Model(capture, "test-provider", "grok-3");

        await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        var body = UpstreamChat.Body(capture);
        Assert.Equal("grok-3", body["model"]!.GetValue<string>());
        Assert.False(body.ContainsKey("custom_field"));
    }
}
