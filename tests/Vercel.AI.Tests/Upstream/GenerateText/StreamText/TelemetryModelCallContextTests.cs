// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText.GenerateText;
using Vercel.AI.Tests.Upstream.Telemetry;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; telemetry model-call context</c>.</summary>
[Collection("TelemetryRegistry")]
public sealed class TelemetryModelCallContextTests : IDisposable
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > telemetry model-call context::";

    public TelemetryModelCallContextTests()
    {
        TelemetryRegistry.Integrations = null;
    }

    public void Dispose()
    {
        TelemetryRegistry.Integrations = null;
    }

    [Fact]
    [UpstreamTest(Prefix + "should execute doStream inside executeLanguageModelCall context")]
    public async Task Should_execute_doStream_inside_executeLanguageModelCall_context()
    {
        string? activeContext = null;
        string? capturedContext = null;
        var model = new CapturingStreamModel(() => capturedContext = activeContext);

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
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

        await ConsumeAsync(result);

        Assert.Equal("test-telemetry-call-id", capturedContext);
    }

    private sealed class CapturingStreamModel(Action capture) : ILanguageModel
    {
        public string SpecificationVersion => "V4";

        public string Provider => "test";

        public string ModelId => "test";

        public IReadOnlyDictionary<string, string> SupportedUrls =>
            Vercel.AI.ProviderUtils.UrlSupport.None;

        public Task<LanguageModelGenerateResult> DoGenerateAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();

        public Task<LanguageModelStreamResult> DoStreamAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        )
        {
            capture();
            return Task.FromResult(new LanguageModelStreamResult(Parts()));
        }

        private static async IAsyncEnumerable<LanguageModelStreamPart> Parts()
        {
            yield return new StreamStartStreamPart(null);
            yield return new TextStartStreamPart("1");
            yield return new TextDeltaStreamPart("1", "done");
            yield return new TextEndStreamPart("1");
            yield return new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13));
            await Task.CompletedTask;
        }
    }
}
