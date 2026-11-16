// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; steering messages</c>.</summary>
[Collection("LogWarnings")]
public sealed class SteeringMessagesTests : WarningsOff
{
    private const string Suite = Prefix + "convertToModelMessages > steering messages::";
    private const string Commit = "b8c01eb49d82fd32efe8a2a1e4cd02fbb2b7ae97";

    [UpstreamTest(
        Suite
            + "should add steering messages as user messages between the steps of an assistant message",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    [Fact]
    public void Should_add_steering_messages_as_user_messages_between_the_steps_of_an_assistant_message()
    {
        var result = Convert(
            new[]
            {
                User(new TextUIPart("What is the weather in Paris?")),
                Assistant(
                    new StepStartUIPart(),
                    new ToolUIPart("weather", "call-1", ToolUIPartState.OutputAvailable)
                    {
                        Input = Json("""{"city":"Paris"}"""),
                        Output = Json("\"sunny\""),
                    },
                    new SteeringMessageUIPart(
                        "steer-1",
                        new UIMessagePart[]
                        {
                            new TextUIPart("I meant Paris, Texas."),
                            new FileUIPart("image/png", "https://example.com/map.png"),
                            new FileUIPart("application/pdf", string.Empty)
                            {
                                ProviderReference = new Dictionary<string, string>
                                {
                                    ["openai"] = "file-1",
                                },
                            },
                        }
                    ),
                    new StepStartUIPart(),
                    new TextUIPart("It is hot in Texas.") { State = "done" }
                ),
            }
        );

        AssertEqual(
            """
            [
              { "content": [{ "text": "What is the weather in Paris?", "type": "text" }], "role": "user" },
              {
                "content": [{ "input": { "city": "Paris" }, "toolCallId": "call-1", "toolName": "weather", "type": "tool-call" }],
                "role": "assistant"
              },
              {
                "content": [{ "output": { "type": "text", "value": "sunny" }, "toolCallId": "call-1", "toolName": "weather", "type": "tool-result" }],
                "role": "tool"
              },
              {
                "content": [
                  { "text": "I meant Paris, Texas.", "type": "text" },
                  { "data": { "type": "url", "url": "https://example.com/map.png" }, "mediaType": "image/png", "type": "file" },
                  { "data": { "reference": { "openai": "file-1" }, "type": "reference" }, "mediaType": "application/pdf", "type": "file" }
                ],
                "role": "user"
              },
              { "content": [{ "text": "It is hot in Texas.", "type": "text" }], "role": "assistant" }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should add a steering message that was received before the first step",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    [Fact]
    public void Should_add_a_steering_message_that_was_received_before_the_first_step()
    {
        var result = Convert(
            new[]
            {
                Assistant(
                    new SteeringMessageUIPart(
                        "steer-1",
                        new UIMessagePart[] { new TextUIPart("Use Celsius.") }
                    ),
                    new StepStartUIPart(),
                    new TextUIPart("It is 20 degrees.") { State = "done" }
                ),
            }
        );

        Assert.Equal(["user", "assistant"], result.Select(message => message.Role));
    }
}
