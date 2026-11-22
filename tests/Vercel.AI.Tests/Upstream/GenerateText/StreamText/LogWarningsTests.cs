// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; logWarnings</c>.</summary>
[Collection("LogWarnings")]
public sealed class LogWarningsTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > logWarnings::";
    private const string Schema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    private static LanguageModelStreamPart[] Stream(
        CallWarning[] warnings,
        LanguageModelStreamPart content,
        FinishReason finishReason
    ) =>
        [
            new StreamStartStreamPart(warnings),
            content,
            new FinishStreamPart(finishReason, new LanguageModelUsage(3, 10, 13), "stop"),
        ];

    private static async Task<List<LogWarningsOptions>> Spy(StreamTextOptions options)
    {
        var calls = new List<LogWarningsOptions>();
        var previous = LogWarnings.Observer;
        LogWarnings.Observer = calls.Add;
        try
        {
            var result = new AiClient(
                GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
            ).StreamTextAsync(options);

            // Consume the stream to trigger the warning logging
            await result.FinishReason;
        }
        finally
        {
            LogWarnings.Observer = previous;
        }

        return calls;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call logWarnings with warnings from a single step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_logWarnings_with_warnings_from_a_single_step()
    {
        CallWarning[] expected =
        [
            new("other", "Setting is not supported"),
            new("unsupported", "temperature", "Temperature parameter not supported"),
        ];

        var calls = await Spy(
            new StreamTextOptions
            {
                Model = new TestLanguageModel
                {
                    StreamParts = Stream(
                        expected,
                        new TextDeltaStreamPart("1", "Hello"),
                        FinishReason.Stop
                    ),
                },
                Prompt = "prompt",
            }
        );

        var call = Assert.Single(calls);
        Assert.Equal("test", call.Provider);
        Assert.Equal("test", call.Model);
        Assert.Equal(2, call.Warnings.Count);
        Assert.Equal(
            "Setting is not supported",
            Assert.IsType<OtherWarning>(call.Warnings[0]).Message
        );
        var unsupported = Assert.IsType<UnsupportedWarning>(call.Warnings[1]);
        Assert.Equal("temperature", unsupported.Feature);
        Assert.Equal("Temperature parameter not supported", unsupported.Details);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call logWarnings once for each step with warnings from that step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_logWarnings_once_for_each_step_with_warnings_from_that_step()
    {
        var model = new StepModel();

        var calls = await Spy(
            new StreamTextOptions
            {
                Model = model,
                Prompt = "prompt",
                Tools =
                [
                    Tool.Function("testTool", null, Schema, (_, _) =>
                        Task.FromResult("\"result\"")
                    ),
                ],
                StopWhen = StopWhen.IsStepCount(3),
            }
        );

        Assert.Equal(2, calls.Count);
        Assert.Equal(
            "Warning from step 1",
            Assert.IsType<OtherWarning>(Assert.Single(calls[0].Warnings)).Message
        );
        Assert.Equal(
            "Warning from step 2",
            Assert.IsType<OtherWarning>(Assert.Single(calls[1].Warnings)).Message
        );
        Assert.All(calls, call =>
        {
            Assert.Equal("test", call.Provider);
            Assert.Equal("test", call.Model);
        });
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call logWarnings with empty array when no warnings are present",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_logWarnings_with_empty_array_when_no_warnings_are_present()
    {
        var calls = await Spy(
            new StreamTextOptions
            {
                Model = new TestLanguageModel
                {
                    StreamParts = Stream(
                        [],
                        new TextDeltaStreamPart("1", "Hello"),
                        FinishReason.Stop
                    ),
                },
                Prompt = "prompt",
            }
        );

        var call = Assert.Single(calls);
        Assert.Empty(call.Warnings);
        Assert.Equal("test", call.Provider);
        Assert.Equal("test", call.Model);
    }

    // Streams a tool call with the first warning, then a final text with the second warning.
    private sealed class StepModel : ILanguageModel
    {
        private int _callCount;

        public string SpecificationVersion => "V4";

        public string Provider => "test";

        public string ModelId => "test";

        public IReadOnlyDictionary<string, string> SupportedUrls { get; } =
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
            var parts = Interlocked.Increment(ref _callCount) switch
            {
                1 => Stream(
                    [new("other", "Warning from step 1")],
                    new ToolCallStreamPart("call-1", "testTool", "{ \"value\": \"test\" }"),
                    FinishReason.ToolCalls
                ),
                2 => Stream(
                    [new("other", "Warning from step 2")],
                    new TextDeltaStreamPart("1", "Final response"),
                    FinishReason.Stop
                ),
                _ => throw new InvalidOperationException("Unexpected call"),
            };
            return Task.FromResult(new LanguageModelStreamResult(ToAsync(parts), null, null));
        }

        private static async IAsyncEnumerable<LanguageModelStreamPart> ToAsync(
            LanguageModelStreamPart[] parts
        )
        {
            foreach (var part in parts)
            {
                yield return part;
                await Task.Yield();
            }
        }
    }
}
