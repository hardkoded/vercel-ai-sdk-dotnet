// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AspNetCore;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.CreateAgentUIStream;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.PipeAgentUIStreamToResponse;

/// <summary>Port of <c>pipe-agent-ui-stream-to-response.test.ts</c> &gt; <c>pipeAgentUIStreamToResponse</c>.</summary>
public sealed class PipeAgentUIStreamToResponseTests
{
    private const string Prefix =
        "packages/ai/src/agent/pipe-agent-ui-stream-to-response.test.ts::pipeAgentUIStreamToResponse::";

    [Fact]
    [UpstreamTest(
        Prefix + "calls onUIMessageStepEnd with the accumulated response message",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Calls_onUIMessageStepEnd_with_the_accumulated_response_message()
    {
        var agent = new Agent(
            new AgentOptions
            {
                Model = new TestLanguageModel
                {
                    StreamParts =
                    [
                        new StreamStartStreamPart([]),
                        new ResponseMetadataStreamPart(
                            "response-1",
                            "mock-model-id",
                            DateTimeOffset.UnixEpoch
                        ),
                        new TextStartStreamPart("text-1"),
                        new TextDeltaStreamPart("text-1", "Hello!"),
                        new TextEndStreamPart("text-1"),
                        new FinishStreamPart(
                            FinishReason.Stop,
                            new LanguageModelUsage(1, 1, 2),
                            "stop"
                        ),
                    ],
                },
            },
            CreateAgentUIStreamSupport.Client()
        );
        var response = new ServerResponse();
        var calls = new List<UIMessageStreamOnStepEndEvent>();

        await UIMessageStreamConverter.PipeAgentUIStreamToResponse(
            new PipeAgentUIStreamToResponseOptions
            {
                Response = response,
                Agent = agent,
                UIMessages =
                [
                    new UIMessage(UIMessageRole.User, [new TextUIPart("Say hello.")])
                    {
                        Id = "user-1",
                    },
                ],
                UIMessageStreamOptions = new UIMessageStreamOptions
                {
                    GenerateMessageId = () => "assistant-1",
                },
                OnUIMessageStepEnd = e =>
                {
                    calls.Add(e);
                    return Task.CompletedTask;
                },
            }
        );
        await response.WaitForEndAsync();

        var call = Assert.Single(calls);
        Assert.Equal("assistant-1", call.ResponseMessage.Id);
        Assert.Equal(UIMessageRole.Assistant, call.ResponseMessage.Role);
        Assert.Collection(
            call.ResponseMessage.Parts,
            part => Assert.IsType<StepStartUIPart>(part),
            part => Assert.Equal("Hello!", Assert.IsType<TextUIPart>(part).Text)
        );
    }
}
