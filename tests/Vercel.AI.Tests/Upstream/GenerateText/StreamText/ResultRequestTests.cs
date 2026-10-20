// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

public sealed class ResultRequestTests
{
    private const string Suite = "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.request::";

    [Fact]
    [UpstreamTest(Suite + "should exclude request body and request messages by default", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_exclude_request_body_and_request_messages_by_default()
    {
        var result = Run(new StreamTextOptions { Prompt = "test-input" });

        var request = await result.Request;
        Assert.Null(request.Body);
        Assert.Null(request.Messages);
    }

    [Fact]
    [UpstreamTest(Suite + "should include request body when retention.requestBody is true", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_request_body_when_retention_requestBody_is_true()
    {
        var result = Run(new StreamTextOptions { Prompt = "test-input", Include = new StreamTextInclude { RequestBody = true } });

        var request = await result.Request;
        Assert.Equal("test body", request.Body!.Value.GetString());
        Assert.Null(request.Messages);
    }

    [Fact]
    [UpstreamTest(Suite + "should include request messages when retention.requestMessages is true", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_request_messages_when_retention_requestMessages_is_true()
    {
        var result = Run(new StreamTextOptions { Prompt = "test-input", Include = new StreamTextInclude { RequestBody = true, RequestMessages = true } });

        var request = await result.Request;
        Assert.Equal("test body", request.Body!.Value.GetString());
        AssertUserMessages(request.Messages!, "test-input");
        AssertUserMessages((await result.Steps)[0].Request.Messages!, "test-input");
    }

    [Fact]
    [UpstreamTest(Suite + "should resolve with messages from after prepareStep", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_resolve_with_messages_from_after_prepareStep()
    {
        var result = Run(new StreamTextOptions
        {
            Prompt = "test-input",
            PrepareStep = _ => new PrepareStepUpdate { Messages = new ModelMessage[] { new UserModelMessage("prepared prompt") } },
            Include = new StreamTextInclude { RequestMessages = true },
        });

        AssertUserMessages((await result.Request).Messages!, "prepared prompt");
        AssertUserMessages((await result.Steps)[0].Request.Messages!, "prepared prompt");
    }

    [Fact]
    [UpstreamTest(Suite + "should carry messages from prepareStep forward to the next step", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_carry_messages_from_prepareStep_forward_to_the_next_step()
    {
        var preparedMessages = new ModelMessage[] { new UserModelMessage("prepared prompt") };
        var prepareStepMessages = new List<IReadOnlyList<ModelMessage>>();
        var model = new StepModel(new IReadOnlyList<LanguageModelStreamPart>[]
        {
            new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(Array.Empty<CallWarning>()),
                new ToolCallStreamPart("call-1", "tool1", "{ \"value\": \"test\" }"),
                new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13), null),
            },
            new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(Array.Empty<CallWarning>()),
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "Done."),
                new TextEndStreamPart("1"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        });

        var result = StepModel.Stream(
            model,
            new StreamTextOptions
            {
                Prompt = "prompt",
                Tools = new[] { Tool.Function("tool1", null, "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}", (_, _) => Task.FromResult("test-result")) },
                StopWhen = StopWhen.IsStepCount(3),
                PrepareStep = context =>
                {
                    prepareStepMessages.Add(context.Messages.ToArray());
                    return context.StepNumber == 0 ? new PrepareStepUpdate { Messages = preparedMessages } : null;
                },
                Include = new StreamTextInclude { RequestMessages = true },
                OnError = (_, _) => Task.CompletedTask,
            });

        await result.Text;

        var steps = await result.Steps;
        var expected = preparedMessages.Concat(steps[0].Response.Messages).ToArray();
        Assert.Equal(expected, prepareStepMessages[1]);
        Assert.Equal(expected, steps[1].Request.Messages);
    }

    private static StreamTextResult Run(StreamTextOptions options)
    {
        var model = new StepModel(StepModel.HelloStream()) { Request = new LanguageModelStreamRequest(StepModel.Json("\"test body\"")) };
        return StepModel.Stream(model, options);
    }

    private static void AssertUserMessages(IReadOnlyList<ModelMessage> messages, string text)
    {
        var message = Assert.IsType<UserModelMessage>(Assert.Single(messages));
        Assert.Equal(text, Assert.IsType<TextContentPart>(Assert.Single(message.Content)).Text);
    }
}
