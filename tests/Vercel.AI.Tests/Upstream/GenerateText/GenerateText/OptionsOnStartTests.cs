// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText</c> &gt; <c>options.onStart</c>.</summary>
public sealed class OptionsOnStartTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.onStart::";

    [Fact]
    [UpstreamTest(
        Prefix + "should send correct information with text prompt",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_correct_information_with_text_prompt()
    {
        GenerateTextStartEvent? startEvent = null;

        await ToolExecutionCallbackSupport
            .Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello, World!"),
                    },
                    Prompt = "test-input",
                    Telemetry = new TelemetryOptions { FunctionId = "test-function" },
                    GenerateCallId = () => "test-telemetry-call-id",
                    OnStart = (@event, _) =>
                    {
                        startEvent = @event;
                        return Task.CompletedTask;
                    },
                }
            );

        Assert.NotNull(startEvent);
        Assert.Equal("test-telemetry-call-id", startEvent.CallId);
        Assert.Equal("ai.generateText", startEvent.OperationId);
        Assert.Equal("test", startEvent.Provider);
        Assert.Equal("test", startEvent.ModelId);
        Assert.Null(startEvent.Instructions);
        var message = Assert.IsType<UserModelMessage>(Assert.Single(startEvent.Messages));
        Assert.Equal(
            "test-input",
            Assert.IsType<TextContentPart>(Assert.Single(message.Content)).Text
        );
        Assert.Equal(2, startEvent.MaxRetries);
        Assert.Null(startEvent.ActiveTools);
        Assert.Null(startEvent.ToolOrder);
        Assert.Null(startEvent.Tools);
        Assert.Null(startEvent.ToolChoice);
        Assert.Null(startEvent.Output);
        Assert.Null(startEvent.ProviderOptions);
        Assert.Null(startEvent.Timeout);
        var headers = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string?>>(
            startEvent.Headers
        );
        var userAgent = Assert.Single(headers);
        Assert.Equal("user-agent", userAgent.Key);
        Assert.Equal(AiSdkVersion.UserAgent, userAgent.Value);
        Assert.Empty(startEvent.RuntimeContext!);
        Assert.Empty(startEvent.ToolsContext!);
        Assert.Null(typeof(GenerateTextStartEvent).GetProperty("FunctionId"));
        Assert.Null(typeof(GenerateTextStartEvent).GetProperty("IsEnabled"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass runtimeContext", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_runtimeContext()
    {
        GenerateTextStartEvent? startEvent = null;

        await ToolExecutionCallbackSupport
            .Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello!"),
                    },
                    Prompt = "test-input",
                    RuntimeContext = new Dictionary<string, object?>
                    {
                        ["userId"] = "test-user",
                        ["sessionId"] = "123",
                    },
                    OnStart = (@event, _) =>
                    {
                        startEvent = @event;
                        return Task.CompletedTask;
                    },
                }
            );

        Assert.Equal("test-user", startEvent!.RuntimeContext!["userId"]);
        Assert.Equal("123", startEvent.RuntimeContext["sessionId"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept deprecated experimental_telemetry as an alias for telemetry",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_accept_deprecated_experimental_telemetry_as_an_alias_for_telemetry()
    {
        GenerateTextStartEvent? startEvent = null;

        await ToolExecutionCallbackSupport
            .Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello!"),
                    },
                    Prompt = "test-input",
#pragma warning disable CS0618 // the deprecated alias is what this test covers
                    ExperimentalTelemetry = new TelemetryOptions
                    {
                        IsEnabled = true,
                        FunctionId = "deprecated-fn",
                    },
#pragma warning restore CS0618
                    OnStart = (@event, _) =>
                    {
                        startEvent = @event;
                        return Task.CompletedTask;
                    },
                }
            );

        Assert.NotNull(startEvent);
        Assert.Null(typeof(GenerateTextStartEvent).GetProperty("IsEnabled"));
        Assert.Null(typeof(GenerateTextStartEvent).GetProperty("FunctionId"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send correct information with system and messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_correct_information_with_system_and_messages()
    {
        GenerateTextStartEvent? startEvent = null;

        await ToolExecutionCallbackSupport
            .Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello!"),
                    },
                    Instructions = Instructions.From("you are a helpful assistant"),
                    Messages = [new UserModelMessage("test-message")],
                    MaxOutputTokens = 100,
                    Temperature = 0.5,
                    ToolOrder = ["b", "a"],
                    OnStart = (@event, _) =>
                    {
                        startEvent = @event;
                        return Task.CompletedTask;
                    },
                }
            );

        Assert.Equal("test", startEvent!.Provider);
        Assert.Equal("test", startEvent.ModelId);
        Assert.NotNull(startEvent.Instructions);
        var message = Assert.IsType<UserModelMessage>(Assert.Single(startEvent.Messages));
        Assert.Equal(
            "test-message",
            Assert.IsType<TextContentPart>(Assert.Single(message.Content)).Text
        );
        Assert.Equal(100, startEvent.MaxOutputTokens);
        Assert.Equal(0.5, startEvent.Temperature);
        Assert.Equal(2, startEvent.MaxRetries);
        Assert.Equal(new[] { "b", "a" }, startEvent.ToolOrder);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should be called before doGenerate",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_called_before_doGenerate()
    {
        var callOrder = new List<string>();

        await ToolExecutionCallbackSupport
            .Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ =>
                        {
                            callOrder.Add("doGenerate");
                            return TestLanguageModel.Text("Hello!");
                        },
                    },
                    Prompt = "test-input",
                    OnStart = (_, _) =>
                    {
                        callOrder.Add("onStart");
                        return Task.CompletedTask;
                    },
                }
            );

        Assert.Equal(new[] { "onStart", "doGenerate" }, callOrder);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not break generation when callback throws",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_break_generation_when_callback_throws()
    {
        var result = await ToolExecutionCallbackSupport
            .Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello, World!"),
                    },
                    Prompt = "test-input",
                    OnStart = (_, _) => throw new InvalidOperationException("callback error"),
                }
            );

        Assert.Equal("Hello, World!", result.Text);
    }
}
