// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.Telemetry;
using Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; telemetry model-call context</c>.</summary>
[Collection("TelemetryRegistry")]
public sealed class TelemetryModelCallContextTests : IDisposable
{
    private const string Prefix =
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > telemetry model-call context::";

    public TelemetryModelCallContextTests()
    {
        TelemetryRegistry.Integrations = null;
    }

    public void Dispose()
    {
        TelemetryRegistry.Integrations = null;
    }

    [Fact]
    [UpstreamTest(Prefix + "should execute doGenerate inside executeLanguageModelCall context")]
    public async Task Should_execute_doGenerate_inside_executeLanguageModelCall_context()
    {
        string? activeContext = null;
        string? capturedContext = null;
        var model = new TestLanguageModel
        {
            OnGenerate = _ =>
            {
                capturedContext = activeContext;
                return new LanguageModelGenerateResult(
                    [new GeneratedText("done")],
                    FinishReason.Stop,
                    new LanguageModelUsage(3, 10, 13)
                );
            },
        };

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = model,
                    Prompt = "prompt",
                    GenerateCallId = () => "test-telemetry-call-id",
                    Telemetry = new TelemetryOptions
                    {
                        IsEnabled = true,
                        Integrations =
                        [
                            new TestTelemetry
                            {
                                ExecuteLanguageModelCall = new ContextWrapper(callId =>
                                    activeContext = callId
                                ),
                            },
                        ],
                    },
                }
            );

        Assert.Equal("test-telemetry-call-id", capturedContext);
    }
}

/// <summary>An <c>executeLanguageModelCall</c> wrapper that exposes the call id while the model call runs.</summary>
internal sealed class ContextWrapper(Action<string?> setContext) : IExecuteLanguageModelCall
{
    public async Task<T> ExecuteAsync<T>(
        ExecuteLanguageModelCallOptions<T> options,
        CancellationToken cancellationToken
    )
    {
        setContext(options.CallId);
        try
        {
            return await options.Execute();
        }
        finally
        {
            setContext(null);
        }
    }
}
