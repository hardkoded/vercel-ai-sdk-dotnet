// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText</c> &gt; <c>options.onStepError</c>.</summary>
public sealed class OptionsOnStepErrorTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.onStepError::";

    [Fact]
    [UpstreamTest(
        Prefix + "should be called when the provider call fails",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_called_when_the_provider_call_fails()
    {
        var stepStarts = new List<GenerateTextStepStartEvent>();
        var stepErrors = new List<GenerateTextStepErrorEvent>();
        var stepEnds = 0;
        var providerError = new InvalidOperationException("provider failure");

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Client()
                .GenerateTextAsync(
                    new GenerateTextOptions
                    {
                        Model = new StepErrorModel(doGenerate: (_, _) => throw providerError),
                        Prompt = "test-input",
                        MaxRetries = 0,
                        OnStepStart = (e, _) =>
                        {
                            stepStarts.Add(e);
                            return Task.CompletedTask;
                        },
                        OnStepError = (e, _) =>
                        {
                            stepErrors.Add(e);
                            return Task.CompletedTask;
                        },
                        OnStepEnd = (_, _) =>
                        {
                            stepEnds++;
                            return Task.CompletedTask;
                        },
                    }
                )
        );

        Assert.Same(providerError, thrown);
        Assert.Equal(0, stepEnds);
        var stepError = Assert.Single(stepErrors);
        Assert.Same(providerError, stepError.Error);
        var stepStart = Assert.Single(stepStarts);
        Assert.Equal(stepStart.StepNumber, stepError.StepNumber);
        Assert.Equal(stepStart.CallId, stepError.CallId);
        Assert.Equal(stepStart.Messages, stepError.Messages);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should report the step that failed in a multi-step run",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_report_the_step_that_failed_in_a_multi_step_run()
    {
        var stepErrors = new List<GenerateTextStepErrorEvent>();
        var stepEnds = 0;
        var providerError = new InvalidOperationException("provider failure");
        var responseCount = 0;

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Client()
                .GenerateTextAsync(
                    new GenerateTextOptions
                    {
                        Model = new StepErrorModel(
                            doGenerate: (_, _) =>
                                responseCount++ > 0
                                    ? throw providerError
                                    : Task.FromResult(
                                        GenerateModel(("call-1", "{ \"value\": \"value\" }"))
                                            .OnGenerate!(null!)
                                    )
                        ),
                        Tools = new[] { Tool1() },
                        Prompt = "test-input",
                        MaxRetries = 0,
                        StopWhen = StopWhen.IsStepCount(3),
                        OnStepError = (e, _) =>
                        {
                            stepErrors.Add(e);
                            return Task.CompletedTask;
                        },
                        OnStepEnd = (_, _) =>
                        {
                            stepEnds++;
                            return Task.CompletedTask;
                        },
                    }
                )
        );

        Assert.Same(providerError, thrown);
        Assert.Equal(1, stepEnds);
        var stepError = Assert.Single(stepErrors);
        Assert.Equal(1, stepError.StepNumber);
        Assert.Same(providerError, stepError.Error);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not be called when the step completes",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_be_called_when_the_step_completes()
    {
        var stepErrors = 0;

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new Vercel.AI.Testing.TestLanguageModel
                    {
                        OnGenerate = _ => Vercel.AI.Testing.TestLanguageModel.Text("Hello!"),
                    },
                    Prompt = "test-input",
                    OnStepError = (_, _) =>
                    {
                        stepErrors++;
                        return Task.CompletedTask;
                    },
                }
            );

        Assert.Equal(0, stepErrors);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should be called with the same wrapped gateway error that is thrown",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_called_with_the_same_wrapped_gateway_error_that_is_thrown()
    {
        var stepErrors = new List<GenerateTextStepErrorEvent>();

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            Client()
                .GenerateTextAsync(
                    new GenerateTextOptions
                    {
                        Model = new StepErrorModel(
                            doGenerate: (_, _) => throw new GatewayAuthenticationError()
                        ),
                        Prompt = "test-input",
                        MaxRetries = 0,
                        OnStepError = (e, _) =>
                        {
                            stepErrors.Add(e);
                            return Task.CompletedTask;
                        },
                    }
                )
        );

        Assert.Equal("GatewayAuthenticationError", ((GatewayAuthenticationError)error).ErrorName);
        Assert.Same(error, Assert.Single(stepErrors).Error);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should be called with the abort error when the call is aborted",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_called_with_the_abort_error_when_the_call_is_aborted()
    {
        var stepErrors = new List<GenerateTextStepErrorEvent>();
        var stepEnds = 0;
        using var cancellation = new CancellationTokenSource();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            Client()
                .GenerateTextAsync(
                    new GenerateTextOptions
                    {
                        Model = new StepErrorModel(
                            doGenerate: (_, token) =>
                            {
                                cancellation.Cancel();
                                return StepErrorModel.HangUntilAborted<LanguageModelGenerateResult>(
                                    token
                                );
                            }
                        ),
                        Prompt = "test-input",
                        MaxRetries = 0,
                        OnStepEnd = (_, _) =>
                        {
                            stepEnds++;
                            return Task.CompletedTask;
                        },
                        OnStepError = (e, _) =>
                        {
                            stepErrors.Add(e);
                            return Task.CompletedTask;
                        },
                    },
                    cancellation.Token
                )
        );

        Assert.Equal(0, stepEnds);
        Assert.IsAssignableFrom<OperationCanceledException>(Assert.Single(stepErrors).Error);
    }

    [Theory]
    [InlineData("total")]
    [InlineData("step")]
    [UpstreamTest(
        Prefix + "should be called with the timeout error when the %s timeout is reached",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_called_with_the_timeout_error_when_the_timeout_is_reached(
        string kind
    )
    {
        var stepErrors = new List<GenerateTextStepErrorEvent>();
        var timeout =
            kind == "total"
                ? new TimeoutConfiguration { TotalMs = 10 }
                : new TimeoutConfiguration { StepMs = 10 };

        var thrown = await Assert.ThrowsAnyAsync<Exception>(() =>
            Client()
                .GenerateTextAsync(
                    new GenerateTextOptions
                    {
                        Model = new StepErrorModel(
                            doGenerate: (_, token) =>
                                StepErrorModel.HangUntilAborted<LanguageModelGenerateResult>(token)
                        ),
                        Prompt = "test-input",
                        MaxRetries = 0,
                        Timeout = timeout,
                        OnStepError = (e, _) =>
                        {
                            stepErrors.Add(e);
                            return Task.CompletedTask;
                        },
                    }
                )
        );

        var stepError = Assert.Single(stepErrors);
        Assert.Equal(0, stepError.StepNumber);
        Assert.Same(thrown, stepError.Error);
        Assert.IsType<TimeoutError>(stepError.Error);
    }
}
