// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; options.output &gt; object output</c>. Only the tests below are ported.</summary>
public sealed class OptionsOutputObjectOutputTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.output > object output::";

    [Fact]
    [UpstreamTest(Prefix + "should send partial output stream")]
    public async Task Should_send_partial_output_stream()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text("{ ", "\"value\": ", "\"Hello, ", "world", "!\"", " }"),
            OutputSpec.Object(OptionsOutputSupport.ValueSchema)
        );

        Assert.Equal(
            new[]
            {
                "{}",
                "{\"value\":\"Hello, \"}",
                "{\"value\":\"Hello, world\"}",
                "{\"value\":\"Hello, world!\"}",
            },
            await OptionsOutputSupport.Collect(result)
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream structured output after an earlier tool step emits text")]
    public async Task Should_stream_structured_output_after_an_earlier_tool_step_emits_text()
    {
        var responseCount = 0;
        var model = new ScriptedStreamModel(_ =>
            responseCount++ switch
            {
                0 => Parts(
                    new TextStartStreamPart("intro"),
                    new TextDeltaStreamPart("intro", "Checking the value."),
                    new TextEndStreamPart("intro"),
                    new ToolCallStreamPart("call-1", "lookup", "{}"),
                    new FinishStreamPart(
                        FinishReason.ToolCalls,
                        new LanguageModelUsage(3, 10, 13),
                        "tool-calls"
                    )
                ),
                1 => Parts(
                    new TextStartStreamPart("answer"),
                    new TextDeltaStreamPart("answer", "{\"value\":\"done\"}"),
                    new TextEndStreamPart("answer"),
                    new FinishStreamPart(
                        FinishReason.Stop,
                        new LanguageModelUsage(3, 10, 13),
                        "stop"
                    )
                ),
                _ => throw new InvalidOperationException(
                    $"Unexpected response count: {responseCount}"
                ),
            }
        );
        var result = new AiClient(
            Vercel.AI.Gateway.GatewayProvider.Create(
                new Vercel.AI.Gateway.GatewayOptions { ApiKey = "test" }
            )
        ).StreamTextAsync(
            new StreamTextOptions
            {
                Model = model,
                Tools = new[]
                {
                    Tool.Function(
                        "lookup",
                        null,
                        "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}",
                        (_, _) => Task.FromResult("\"done\"")
                    ),
                },
                Output = OutputSpec.Object(OptionsOutputSupport.ValueSchema),
                Prompt = "Look up the value and return it.",
                StopWhen = StopWhen.IsStepCount(2),
            }
        );

        Assert.Equal(new[] { "{\"value\":\"done\"}" }, await OptionsOutputSupport.Collect(result));
        Assert.Equal("done", (await result.Output)!.Value.GetProperty("value").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should send partial output stream when last chunk contains content")]
    public async Task Should_send_partial_output_stream_when_last_chunk_contains_content()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text("{ ", "\"value\": ", "\"Hello, ", "world!\" }"),
            OutputSpec.Object(OptionsOutputSupport.ValueSchema)
        );

        Assert.Equal(
            new[] { "{}", "{\"value\":\"Hello, \"}", "{\"value\":\"Hello, world!\"}" },
            await OptionsOutputSupport.Collect(result)
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve text promise with the correct content")]
    public async Task Should_resolve_text_promise_with_the_correct_content()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text("{ ", "\"value\": ", "\"Hello, ", "world!\" ", "}"),
            OutputSpec.Object(OptionsOutputSupport.ValueSchema)
        );

        Assert.Equal("{ \"value\": \"Hello, world!\" }", await result.Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve output promise with the correct content")]
    public async Task Should_resolve_output_promise_with_the_correct_content()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text("{ ", "\"value\": ", "\"Hello, ", "world!\" ", "}"),
            OutputSpec.Object(OptionsOutputSupport.ValueSchema)
        );

        Assert.Equal(
            "Hello, world!",
            (await result.Output)!.Value.GetProperty("value").GetString()
        );
    }

    private static IAsyncEnumerable<LanguageModelStreamPart> Parts(
        params LanguageModelStreamPart[] parts
    )
    {
        return parts.ToAsyncEnumerable();
    }

    private sealed class ScriptedStreamModel(
        Func<LanguageModelCallOptions, IAsyncEnumerable<LanguageModelStreamPart>> next
    ) : ILanguageModel
    {
        public IReadOnlyDictionary<string, string> SupportedUrls { get; } =
            Vercel.AI.ProviderUtils.UrlSupport.None;

        public string SpecificationVersion => "V4";

        public string Provider => "test";

        public string ModelId => "test";

        public Task<LanguageModelGenerateResult> DoGenerateAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        )
        {
            throw new NotSupportedException();
        }

        public Task<LanguageModelStreamResult> DoStreamAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        )
        {
            return Task.FromResult(new LanguageModelStreamResult(next(options)));
        }
    }
}
