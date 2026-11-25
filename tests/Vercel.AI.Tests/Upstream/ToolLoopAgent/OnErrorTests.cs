// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>onError</c>.</summary>
public sealed class OnErrorTests
{
    private const string Prefix =
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onError::";

    private const string Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71";

    private static async IAsyncEnumerable<LanguageModelStreamPart> StartOnly()
    {
        yield return new StreamStartStreamPart(null);
        await Task.CompletedTask;
    }

    private static Agent AgentOf(
        ILanguageModel model,
        List<Exception> errors,
        int? maxRetries = 0
    ) =>
        new(
            new AgentOptions
            {
                Model = model,
                MaxRetries = maxRetries,
                OnError = (e, _) =>
                {
                    errors.Add(e);
                    return Task.CompletedTask;
                },
            },
            ToolLoopAgentInstructions.Client()
        );

    [Fact]
    [UpstreamTest(
        Prefix + "should call onError when a stream step fails",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldCallOnErrorWhenAStreamStepFails()
    {
        var errors = new List<Exception>();
        var agent = AgentOf(
            new Vercel.AI.Tests.StreamText.StreamTextAbortSupport.ScriptedStreamModel(StartOnly),
            errors,
            maxRetries: null
        );

        var result = agent.Stream("Hello, world!");
        await foreach (var _ in result.Stream()) { }
        await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Text);

        Assert.IsType<NoOutputGeneratedException>(Assert.Single(errors));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call onError and rethrow when generate fails",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldCallOnErrorAndRethrowWhenGenerateFails()
    {
        var errors = new List<Exception>();
        var providerError = new InvalidOperationException("provider failure");
        var agent = AgentOf(new StepErrorModel(doGenerate: (_, _) => throw providerError), errors);

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            agent.GenerateAsync("Hello, world!")
        );

        Assert.Same(providerError, thrown);
        Assert.Same(providerError, Assert.Single(errors));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should rethrow the generate error when onError throws",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldRethrowTheGenerateErrorWhenOnErrorThrows()
    {
        var providerError = new InvalidOperationException("provider failure");
        var agent = new Agent(
            new AgentOptions
            {
                Model = new StepErrorModel(doGenerate: (_, _) => throw providerError),
                MaxRetries = 0,
                OnError = (_, _) => throw new InvalidOperationException("callback failure"),
            },
            ToolLoopAgentInstructions.Client()
        );

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            agent.GenerateAsync("Hello, world!")
        );

        Assert.Same(providerError, thrown);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not call onError for an abort in generate()",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldNotCallOnErrorForAnAbortInGenerate()
    {
        var errors = new List<Exception>();
        using var cancellation = new CancellationTokenSource();
        var agent = AgentOf(
            new StepErrorModel(doGenerate: (_, token) =>
            {
                cancellation.Cancel();
                return StepErrorModel.HangUntilAborted<LanguageModelGenerateResult>(token);
            }),
            errors
        );

        await Assert.ThrowsAnyAsync<Exception>(() =>
            agent.GenerateAsync("Hello, world!", cancellation.Token)
        );

        Assert.Empty(errors);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not call onError for a timeout in generate()",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldNotCallOnErrorForATimeoutInGenerate()
    {
        var errors = new List<Exception>();
        var agent = AgentOf(
            new StepErrorModel(doGenerate: (_, token) =>
                StepErrorModel.HangUntilAborted<LanguageModelGenerateResult>(token)
            ),
            errors
        );

        await Assert.ThrowsAnyAsync<Exception>(() =>
            agent.GenerateAsync("Hello, world!", timeout: new TimeoutConfiguration { TotalMs = 10 })
        );

        Assert.Empty(errors);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not call onError when a provider wraps a timeout error",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldNotCallOnErrorWhenAProviderWrapsATimeoutError()
    {
        var errors = new List<Exception>();
        var agent = AgentOf(
            new StepErrorModel(doGenerate: (_, _) =>
                throw new InvalidOperationException(
                    "Gateway request failed",
                    new TimeoutError("timed out")
                )
            ),
            errors
        );

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            agent.GenerateAsync("Hello, world!")
        );

        Assert.Equal("Gateway request failed", thrown.Message);
        Assert.Empty(errors);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not call onError when a provider wraps an abort in stream()",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldNotCallOnErrorWhenAProviderWrapsAnAbortInStream()
    {
        var errors = new List<Exception>();
        var abortController = new AbortController();
        var agent = AgentOf(
            new StepErrorModel(doStream: (_, _) =>
            {
                abortController.Abort();
                throw new InvalidOperationException(
                    "Gateway request failed",
                    new OperationCanceledException("aborted")
                );
            }),
            errors
        );

        var result = agent.Stream("Hello, world!", abortSignal: abortController.Signal);
        await foreach (var _ in result.Stream()) { }

        Assert.Empty(errors);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not call onError when a provider wraps a timeout in stream()",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldNotCallOnErrorWhenAProviderWrapsATimeoutInStream()
    {
        var errors = new List<Exception>();
        var agent = AgentOf(
            new StepErrorModel(doStream: async (_, token) =>
            {
                try
                {
                    return await StepErrorModel.HangUntilAborted<LanguageModelStreamResult>(token);
                }
                catch (OperationCanceledException exception)
                {
                    throw new InvalidOperationException("Gateway request failed", exception);
                }
            }),
            errors
        );

        var result = agent.Stream(
            "Hello, world!",
            timeout: new TimeoutConfiguration { TotalMs = 10 }
        );
        await foreach (var _ in result.Stream()) { }

        Assert.Empty(errors);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not call onError when generate() is aborted with a custom reason",
        Coverage = UpstreamCoverage.Partial,
        Note = "Agent.GenerateAsync takes a CancellationToken, which carries no abort reason, so the test does not check that the error carries the custom reason.",
        Commit = Commit
    )]
    public async Task ShouldNotCallOnErrorWhenGenerateIsAbortedWithACustomReason()
    {
        var errors = new List<Exception>();
        using var cancellation = new CancellationTokenSource();
        var agent = AgentOf(
            new StepErrorModel(doGenerate: (_, _) =>
            {
                cancellation.Cancel();
                throw new OperationCanceledException(cancellation.Token);
            }),
            errors
        );

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            agent.GenerateAsync("Hello, world!", cancellation.Token)
        );

        Assert.Empty(errors);
    }
}
