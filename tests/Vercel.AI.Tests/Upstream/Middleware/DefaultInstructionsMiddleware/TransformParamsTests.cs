// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.Middleware.DefaultInstructionsMiddleware.DefaultInstructionsTestSupport;
using Instructions = global::Vercel.AI.DefaultInstructionsMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.DefaultInstructionsMiddleware;

/// <summary>Port of <c>default-instructions-middleware.test.ts</c> &gt; <c>defaultInstructionsMiddleware &gt; transformParams</c>.</summary>
public sealed class TransformParamsTests
{
    private const string Id =
        "packages/ai/src/middleware/default-instructions-middleware.test.ts::defaultInstructionsMiddleware > transformParams::";

    private static readonly TestLanguageModel MockModel = new();

    private static Task<LanguageModelCallOptions> Transform(
        Instructions middleware,
        LanguageModelCallOptions options
    ) =>
        middleware.TransformParamsAsync(
            options,
            LanguageModelCallType.Generate,
            MockModel,
            CancellationToken.None
        );

    [Fact]
    [UpstreamTest(
        Id + "should prepend string instructions when the prompt has no system message",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prepend_string_instructions_when_the_prompt_has_no_system_message()
    {
        var middleware = new Instructions("You are a helpful assistant.");

        var result = await Transform(middleware, BaseParams());

        Assert.Equal(2, result.Prompt.Count);
        AssertSystem(result.Prompt[0], "You are a helpful assistant.");
        AssertUser(result.Prompt[1], "Hello, world!");
    }

    [Fact]
    [UpstreamTest(
        Id + "should preserve provider options on default instructions",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_provider_options_on_default_instructions()
    {
        var providerOptions = new Dictionary<string, JsonElement>
        {
            ["anthropic"] = JsonSerializer.SerializeToElement(
                new { cacheControl = new { type = "ephemeral" } }
            ),
        };
        var middleware = new Instructions([
            new SystemModelMessage("You are a helpful assistant.", providerOptions),
        ]);

        var result = await Transform(middleware, BaseParams());

        var system = Assert.IsType<SystemModelMessage>(result.Prompt[0]);
        Assert.Equal("You are a helpful assistant.", system.Content);
        Assert.NotNull(system.ProviderOptions);
        var options = Assert.Single(system.ProviderOptions!);
        Assert.Equal("anthropic", options.Key);
        Assert.Equal(
            "ephemeral",
            options.Value.GetProperty("cacheControl").GetProperty("type").GetString()
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should prepend multiple default instruction messages in order",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prepend_multiple_default_instruction_messages_in_order()
    {
        var middleware = new Instructions([
            new SystemModelMessage("You are a helpful assistant."),
            new SystemModelMessage("Answer concisely."),
        ]);

        var result = await Transform(middleware, BaseParams());

        Assert.Equal(3, result.Prompt.Count);
        AssertSystem(result.Prompt[0], "You are a helpful assistant.");
        AssertSystem(result.Prompt[1], "Answer concisely.");
        AssertUser(result.Prompt[2], "Hello, world!");
    }

    [Fact]
    [UpstreamTest(
        Id + "should let call-level instructions take precedence",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_let_call_level_instructions_take_precedence()
    {
        var middleware = new Instructions("Default instructions");
        var parameters = BaseParams();
        parameters.Prompt =
        [
            new SystemModelMessage(
                "Call-level instructions",
                new Dictionary<string, JsonElement>
                {
                    ["openai"] = JsonSerializer.SerializeToElement(
                        new { promptCacheKey = "call-level" }
                    ),
                }
            ),
            .. BaseParams().Prompt,
        ];
        var prompt = parameters.Prompt;

        var result = await Transform(middleware, parameters);

        Assert.Same(parameters, result);
        Assert.Same(prompt, result.Prompt);
        Assert.Equal(2, result.Prompt.Count);
        AssertSystem(result.Prompt[0], "Call-level instructions");
    }

    [Fact]
    [UpstreamTest(
        Id + "should not add defaults when a system message appears later in the prompt",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_add_defaults_when_a_system_message_appears_later_in_the_prompt()
    {
        var middleware = new Instructions("Default instructions");
        var parameters = new LanguageModelCallOptions
        {
            Prompt =
            [
                .. BaseParams().Prompt,
                new SystemModelMessage("Trusted conversation instructions"),
            ],
        };

        var result = await Transform(middleware, parameters);

        Assert.Same(parameters, result);
        Assert.Equal(2, result.Prompt.Count);
    }

    [Fact]
    [UpstreamTest(
        Id + "should preserve other call parameters",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_other_call_parameters()
    {
        var middleware = new Instructions("Default instructions");
        var providerOptions = new Dictionary<string, JsonElement>
        {
            ["openai"] = JsonSerializer.SerializeToElement(new { store = false }),
        };
        var parameters = BaseParams();
        parameters.Temperature = 0.5;
        parameters.ProviderOptions = providerOptions;

        var result = await Transform(middleware, parameters);

        Assert.Equal(0.5, result.Temperature);
        Assert.Same(providerOptions, result.ProviderOptions);
        Assert.Equal(2, result.Prompt.Count);
        AssertSystem(result.Prompt[0], "Default instructions");
        AssertUser(result.Prompt[1], "Hello, world!");
    }

    [Fact]
    [UpstreamTest(
        Id + "should leave the parameters unchanged for an empty instructions array",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_leave_the_parameters_unchanged_for_an_empty_instructions_array()
    {
        var middleware = new Instructions(Array.Empty<SystemModelMessage>());
        var parameters = BaseParams();

        var result = await Transform(middleware, parameters);

        Assert.Same(parameters, result);
    }
}
