// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

public sealed class ResultRequestAndResponseTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > result.request::should exclude request body and request messages by default",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_exclude_request_body_and_request_messages_by_default()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => TestLanguageModel.Text("Hello, world!"),
        };

        var result = await Client()
            .GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "prompt" });

        Assert.Null(result.Request.Body);
        Assert.Null(result.Request.Messages);
        Assert.Same(result.Steps[0].Request, result.Request);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > result.response::should contain response body and headers",
        Coverage = UpstreamCoverage.Covered
    )]
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
                responseHeaders: new Dictionary<string, string>
                {
                    ["custom-response-header"] = "response-header-value",
                },
                rawResponse: "\"test body\""
            ),
        };

        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = model,
                    Prompt = "prompt",
                    Include = new GenerateTextInclude { ResponseBody = true },
                }
            );

        Assert.Same(result.Steps[0].Response, result.Response);
        Assert.Equal("test body", result.Response.Body!.Value.GetString());
        Assert.Equal("test-id-from-model", result.Response.Id);
        Assert.Equal("test-response-model-id", result.Response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(10000), result.Response.Timestamp);
        Assert.Equal(
            new Dictionary<string, string> { ["custom-response-header"] = "response-header-value" },
            result.Response.Headers
        );
        var message = Assert.IsType<AssistantModelMessage>(Assert.Single(result.Response.Messages));
        Assert.Equal("Hello, world!", message.Text);
    }

    private const string RequestPrefix =
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > result.request::";

    [Fact]
    [UpstreamTest(
        RequestPrefix + "should include request body when retention.requestBody is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_request_body_when_retention_requestBody_is_true()
    {
        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = BodyModel(),
                    Prompt = "prompt",
                    Include = new GenerateTextInclude { RequestBody = true },
                }
            );

        Assert.Equal("test body", result.Request.Body!.Value.GetString());
        Assert.Null(result.Request.Messages);
    }

    [Fact]
    [UpstreamTest(
        RequestPrefix + "should accept deprecated experimental_include as an alias for include",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_accept_deprecated_experimental_include_as_an_alias_for_include()
    {
#pragma warning disable CS0618 // ExperimentalInclude is the deprecated alias of Include
        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = BodyModel(),
                    Prompt = "prompt",
                    ExperimentalInclude = new GenerateTextInclude { RequestBody = true },
                }
            );
#pragma warning restore CS0618

        Assert.Equal("test body", result.Request.Body!.Value.GetString());
        Assert.Null(result.Request.Messages);
    }

    [Fact]
    [UpstreamTest(
        RequestPrefix + "should prefer include over deprecated experimental_include",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_include_over_deprecated_experimental_include()
    {
#pragma warning disable CS0618 // ExperimentalInclude is the deprecated alias of Include
        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = BodyModel(),
                    Prompt = "prompt",
                    Include = new GenerateTextInclude { RequestBody = false },
                    ExperimentalInclude = new GenerateTextInclude { RequestBody = true },
                }
            );
#pragma warning restore CS0618

        Assert.Null(result.Request.Body);
        Assert.Null(result.Request.Messages);
    }

    [Fact]
    [UpstreamTest(
        RequestPrefix + "should include request messages when retention.requestMessages is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_request_messages_when_retention_requestMessages_is_true()
    {
        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = BodyModel(),
                    Prompt = "prompt",
                    Include = new GenerateTextInclude
                    {
                        RequestBody = true,
                        RequestMessages = true,
                    },
                }
            );

        Assert.Equal("test body", result.Request.Body!.Value.GetString());
        AssertUserMessage("prompt", Assert.Single(result.Request.Messages!));
        AssertUserMessage("prompt", Assert.Single(result.Steps[0].Request.Messages!));
    }

    [Fact]
    [UpstreamTest(
        RequestPrefix + "should contain messages from after prepareStep",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_contain_messages_from_after_prepareStep()
    {
        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = BodyModel(),
                    Prompt = "prompt",
                    PrepareStep = _ => new PrepareStepUpdate
                    {
                        Messages = new ModelMessage[] { new UserModelMessage("prepared prompt") },
                    },
                    Include = new GenerateTextInclude { RequestMessages = true },
                }
            );

        AssertUserMessage("prepared prompt", Assert.Single(result.Request.Messages!));
        AssertUserMessage("prepared prompt", Assert.Single(result.Steps[0].Request.Messages!));
    }

    [Fact]
    [UpstreamTest(
        RequestPrefix + "should carry messages from prepareStep forward to the next step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_carry_messages_from_prepareStep_forward_to_the_next_step()
    {
        ModelMessage[] preparedMessages = [new UserModelMessage("prepared prompt")];
        var prepareStepMessages = new List<IReadOnlyList<ModelMessage>>();
        var calls = 0;
        var model = new TestLanguageModel
        {
            OnGenerate = _ =>
                calls++ == 0
                    ? new LanguageModelGenerateResult(
                        new GeneratedContent[]
                        {
                            new GeneratedToolCall("call-1", "tool1", "{ \"value\": \"test\" }"),
                        },
                        FinishReason.ToolCalls,
                        new LanguageModelUsage(3, 10, 13),
                        "tool-calls"
                    )
                    : TestLanguageModel.Text("Done."),
        };

        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = model,
                    Prompt = "prompt",
                    Tools = new[]
                    {
                        Tool.Function(
                            "tool1",
                            null,
                            "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}",
                            (_, _) => Task.FromResult("test-result")
                        ),
                    },
                    StopWhen = StopWhen.IsStepCount(3),
                    PrepareStep = context =>
                    {
                        prepareStepMessages.Add([.. context.Messages]);
                        return context.StepNumber == 0
                            ? new PrepareStepUpdate { Messages = preparedMessages }
                            : null;
                    },
                    Include = new GenerateTextInclude { RequestMessages = true },
                }
            );

        var expected = preparedMessages.Concat(result.Steps[0].Response.Messages).ToArray();
        AssertSameMessages(expected, prepareStepMessages[1]);
        AssertSameMessages(expected, result.Steps[1].Request.Messages!);
    }

    private static void AssertSameMessages(
        IReadOnlyList<ModelMessage> expected,
        IReadOnlyList<ModelMessage> actual
    )
    {
        Assert.Equal(new[] { "user", "assistant", "tool" }, expected.Select(m => m.Role));
        Assert.Equal(expected.Select(m => m.Role), actual.Select(m => m.Role));
        AssertUserMessage("prepared prompt", actual[0]);
        Assert.IsType<AssistantModelMessage>(actual[1]);
        var tool = Assert.IsType<ToolModelMessage>(actual[2]);
        Assert.Equal(
            Assert.IsType<ToolModelMessage>(expected[2]).Results.Select(r => r.ToolCallId),
            tool.Results.Select(r => r.ToolCallId)
        );
    }

    private static void AssertUserMessage(string text, ModelMessage message)
    {
        var user = Assert.IsType<UserModelMessage>(message);
        Assert.Equal(text, Assert.IsType<TextContentPart>(Assert.Single(user.Content)).Text);
    }

    private static TestLanguageModel BodyModel()
    {
        return new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("Hello, world!") },
                FinishReason.Stop,
                new LanguageModelUsage(3, 10, 13),
                "stop",
                request: LanguageModelStreamRequest.FromJson("\"test body\"")
            ),
        };
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
