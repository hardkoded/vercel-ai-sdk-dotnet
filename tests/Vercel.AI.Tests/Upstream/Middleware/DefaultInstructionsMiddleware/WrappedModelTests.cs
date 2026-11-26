// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.Middleware.DefaultInstructionsMiddleware.DefaultInstructionsTestSupport;
using Instructions = global::Vercel.AI.DefaultInstructionsMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.DefaultInstructionsMiddleware;

/// <summary>Port of <c>default-instructions-middleware.test.ts</c> &gt; <c>defaultInstructionsMiddleware &gt; wrapped model</c>.</summary>
public sealed class WrappedModelTests
{
    private const string Id =
        "packages/ai/src/middleware/default-instructions-middleware.test.ts::defaultInstructionsMiddleware > wrapped model::";

    private static AiClient Client() =>
        new(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

    private static LanguageModelUsage Usage => new(1, 1, 2);

    private sealed class AddInstructionsMiddleware : LanguageModelMiddleware
    {
        public override Task<LanguageModelCallOptions> TransformParamsAsync(
            LanguageModelCallOptions options,
            LanguageModelCallType type,
            ILanguageModel model,
            CancellationToken cancellationToken
        )
        {
            var copy = new LanguageModelCallOptions
            {
                Prompt =
                [
                    new SystemModelMessage("Earlier middleware instructions"),
                    .. options.Prompt,
                ],
            };
            return Task.FromResult(copy);
        }
    }

    [Fact]
    [UpstreamTest(
        Id + "should apply defaults to generate and stream calls without mutating the input",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_apply_defaults_to_generate_and_stream_calls_without_mutating_the_input()
    {
        var model = new TestLanguageModel();
        var wrappedModel = model.WrapLanguageModel(new Instructions("Default instructions"));
        var parameters = BaseParams();

        await wrappedModel.DoGenerateAsync(parameters, CancellationToken.None);
        await wrappedModel.DoStreamAsync(parameters, CancellationToken.None);

        Assert.Equal(2, model.Calls.Count);
        foreach (var call in model.Calls)
        {
            Assert.Equal(2, call.Prompt.Count);
            AssertSystem(call.Prompt[0], "Default instructions");
            AssertUser(call.Prompt[1], "Hello, world!");
        }

        AssertUser(Assert.Single(parameters.Prompt), "Hello, world!");
    }

    [Fact]
    [UpstreamTest(
        Id + "should respect system messages added by an earlier middleware",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_respect_system_messages_added_by_an_earlier_middleware()
    {
        var model = new TestLanguageModel();
        var wrappedModel = model.WrapLanguageModel(
            new AddInstructionsMiddleware(),
            new Instructions("Default instructions")
        );

        await wrappedModel.DoGenerateAsync(BaseParams(), CancellationToken.None);

        var prompt = Assert.Single(model.Calls).Prompt;
        Assert.Equal(2, prompt.Count);
        AssertSystem(prompt[0], "Earlier middleware instructions");
        AssertUser(prompt[1], "Hello, world!");
    }

    [Fact]
    [UpstreamTest(
        Id + "should apply defaults once on repeated calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_apply_defaults_once_on_repeated_calls()
    {
        var model = new TestLanguageModel();
        var wrappedModel = model.WrapLanguageModel(new Instructions("Default instructions"));
        var parameters = BaseParams();

        await wrappedModel.DoGenerateAsync(parameters, CancellationToken.None);
        await wrappedModel.DoGenerateAsync(parameters, CancellationToken.None);

        Assert.Equal(2, model.Calls.Count);
        foreach (var call in model.Calls)
        {
            Assert.Equal(2, call.Prompt.Count);
            AssertSystem(call.Prompt[0], "Default instructions");
            AssertUser(call.Prompt[1], "Hello, world!");
        }
    }

    [Fact]
    [UpstreamTest(
        Id + "should apply defaults once to every step of a multi-step generation",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_apply_defaults_once_to_every_step_of_a_multi_step_generation()
    {
        var prompts = new List<IReadOnlyList<ModelMessage>>();
        var callCount = 0;
        var model = new TestLanguageModel
        {
            OnGenerate = options =>
            {
                prompts.Add(options.Prompt);
                return callCount++ == 0
                    ? new LanguageModelGenerateResult(
                        [new GeneratedToolCall("call-1", "weather", "{}")],
                        FinishReason.ToolCalls,
                        Usage
                    )
                    : new LanguageModelGenerateResult(
                        [new GeneratedText("Done.")],
                        FinishReason.Stop,
                        Usage
                    );
            },
        };

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = model.WrapLanguageModel(new Instructions("Default instructions")),
                    Tools =
                    [
                        Tool.Function("weather", null, "{\"type\":\"object\"}", (_, _) =>
                            Task.FromResult("\"sunny\"")
                        ),
                    ],
                    Prompt = "What is the weather?",
                    StopWhen = StopWhen.IsStepCount(2),
                }
            );

        Assert.Equal(2, prompts.Count);
        foreach (var prompt in prompts)
        {
            var system = Assert.Single(prompt.OfType<SystemModelMessage>());
            Assert.Equal("Default instructions", system.Content);
            Assert.Null(system.ProviderOptions);
        }
    }

    [Fact]
    [UpstreamTest(
        Id + "should let trusted system messages in message histories override defaults",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_let_trusted_system_messages_in_message_histories_override_defaults()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                [new GeneratedText("Done.")],
                FinishReason.Stop,
                Usage
            ),
        };

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = model.WrapLanguageModel(new Instructions("Default instructions")),
                    Messages =
                    [
                        new SystemModelMessage("Conversation instructions"),
                        new UserModelMessage("Hello"),
                    ],
                    AllowSystemInMessages = true,
                }
            );

        var prompt = Assert.Single(model.Calls).Prompt;
        Assert.Equal(2, prompt.Count);
        AssertSystem(prompt[0], "Conversation instructions");
        AssertUser(prompt[1], "Hello");
        Assert.Null(((UserModelMessage)prompt[1]).ProviderOptions);
    }
}
