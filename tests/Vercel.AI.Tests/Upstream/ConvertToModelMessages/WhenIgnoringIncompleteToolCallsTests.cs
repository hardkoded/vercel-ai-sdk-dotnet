// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; when ignoring incomplete tool calls</c>.</summary>
[Collection("LogWarnings")]
public sealed class WhenIgnoringIncompleteToolCallsTests : WarningsOff
{
    private const string Suite = Prefix + "convertToModelMessages > when ignoring incomplete tool calls::";

    [UpstreamTest(Suite + "should ignore preliminary tool outputs", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_ignore_preliminary_tool_outputs()
    {
        var toModelOutputCalls = 0;
        var streamingTool = new Tool("streamingTool", null, Json("{\"type\":\"object\",\"properties\":{\"task\":{\"type\":\"string\"}},\"required\":[\"task\"]}"), null)
        {
            ToModelOutput = call =>
            {
                toModelOutputCalls++;
                return new JsonToolModelOutput(Json((string)call.Output!));
            },
        };

        var result = Convert(
            new[]
            {
                Assistant(new ToolUIPart("streamingTool", "call-preliminary", ToolUIPartState.OutputAvailable)
                {
                    Input = Json("{\"task\":\"finish the work\"}"),
                    Output = Json("{\"complete\":false,\"progress\":\"half finished\"}"),
                    Preliminary = true,
                }),
                User(new TextUIPart("Continue.")),
            },
            new ConvertToModelMessagesOptions
            {
                IgnoreIncompleteToolCalls = true,
                Tools = new Dictionary<string, Tool> { ["streamingTool"] = streamingTool },
            });

        Assert.Equal(0, toModelOutputCalls);
        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [{ "type": "text", "text": "Continue." }]
              }
            ]
            """,
            result);
    }
}
