// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Tests.StreamText;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>
/// Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; result.request</c>. Not ported: <c>should accept deprecated experimental_include as an alias for include</c>
/// and <c>should prefer include over deprecated experimental_include</c>, because the port has no deprecated <c>experimental_include</c> alias.
/// </summary>
public sealed class ResultRequestTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.request::";

    [Fact]
    [UpstreamTest(Prefix + "should exclude request body and request messages by default", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_exclude_request_body_and_request_messages_by_default()
    {
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = HelloModel(),
            Prompt = "test-input",
        });

        var request = await result.Request;
        Assert.Null(request.Body);
        Assert.Null(request.Messages);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include request body when retention.requestBody is true", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_request_body_when_retention_requestBody_is_true()
    {
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = HelloModel(),
            Prompt = "test-input",
            Include = new StreamTextInclude { RequestBody = true },
        });

        var request = await result.Request;
        Assert.Equal("test body", request.Body!.Value.GetString());
        Assert.Null(request.Messages);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include request messages when retention.requestMessages is true", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_request_messages_when_retention_requestMessages_is_true()
    {
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = HelloModel(),
            Prompt = "test-input",
            Include = new StreamTextInclude { RequestBody = true, RequestMessages = true },
        });

        var request = await result.Request;
        Assert.Equal("test body", request.Body!.Value.GetString());
        AssertUserMessage("test-input", Assert.Single(request.Messages!));
        AssertUserMessage("test-input", Assert.Single((await result.Steps)[0].Request.Messages!));
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve with messages from after prepareStep", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_resolve_with_messages_from_after_prepareStep()
    {
        var preparedMessages = new ModelMessage[] { new UserModelMessage("prepared prompt") };

        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = HelloModel(request: false),
            Prompt = "test-input",
            PrepareStep = _ => new PrepareStepUpdate { Messages = preparedMessages },
            Include = new StreamTextInclude { RequestMessages = true },
        });

        Assert.Equal(preparedMessages, (await result.Request).Messages);
        Assert.Equal(preparedMessages, (await result.Steps)[0].Request.Messages);
    }

    [Fact]
    [UpstreamTest(Prefix + "should carry messages from prepareStep forward to the next step", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_carry_messages_from_prepareStep_forward_to_the_next_step()
    {
        var preparedMessages = new ModelMessage[] { new UserModelMessage("prepared prompt") };
        var prepareStepMessages = new List<ModelMessage[]>();
        var responseCount = 0;

        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = new StreamTextAbortSupport.ScriptedStreamModel(() => responseCount++ == 0 ? ToolCallStep() : DoneStep()),
            Tools = new[]
            {
                Tool.Function("tool1", null, "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}", (input, _) =>
                    Task.FromResult(JsonSerializer.Serialize(input.GetProperty("value").GetString() + "-result"))),
            },
            Prompt = "prompt",
            StopWhen = StopWhen.IsStepCount(3),
            PrepareStep = context =>
            {
                prepareStepMessages.Add(context.Messages.ToArray());
                return context.StepNumber == 0 ? new PrepareStepUpdate { Messages = preparedMessages } : null;
            },
            Include = new StreamTextInclude { RequestMessages = true },
            OnError = (_, _) => Task.CompletedTask,
        });

        await foreach (var _ in result.Stream())
        {
        }

        var steps = await result.Steps;
        var expected = preparedMessages.Concat(steps[0].Response.Messages).ToArray();
        Assert.Equal(expected, prepareStepMessages[1]);
        Assert.Equal(expected, steps[1].Request.Messages);
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> ToolCallStep()
    {
        yield return new StreamStartStreamPart(null);
        yield return new ToolCallStreamPart("call-1", "tool1", "{ \"value\": \"test\" }");
        yield return new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13));
        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> DoneStep()
    {
        yield return new StreamStartStreamPart(null);
        yield return new TextStartStreamPart("1");
        yield return new TextDeltaStreamPart("1", "Done.");
        yield return new TextEndStreamPart("1");
        yield return new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop");
        await Task.CompletedTask;
    }

    private static TestLanguageModel HelloModel(bool request = true)
    {
        return new TestLanguageModel("mock-model-id")
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ResponseMetadataStreamPart("id-0", "mock-model-id", DateTimeOffset.FromUnixTimeMilliseconds(0)),
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "Hello"),
                new TextEndStreamPart("1"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
            StreamRequest = request ? LanguageModelStreamRequest.FromJson("\"test body\"") : null,
        };
    }

    private static void AssertUserMessage(string text, ModelMessage message)
    {
        var user = Assert.IsType<UserModelMessage>(message);
        Assert.Equal(text, Assert.IsType<TextContentPart>(Assert.Single(user.Content)).Text);
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
