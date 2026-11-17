// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; options.output &gt; array output</c>. Only the tests below are ported.</summary>
public sealed class OptionsOutputArrayOutputTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.output > array output::";

    [Fact]
    [UpstreamTest(Prefix + "should stream array elements after an earlier tool step emits text")]
    public async Task Should_stream_array_elements_after_an_earlier_tool_step_emits_text()
    {
        var responseCount = 0;
        var model = new ScriptedStreamModel(_ =>
            responseCount++ switch
            {
                0 => Parts(
                    new TextStartStreamPart("answer"),
                    new TextDeltaStreamPart("answer", "Checking the value."),
                    new TextEndStreamPart("answer"),
                    new ToolCallStreamPart("call-1", "lookup", "{}"),
                    new FinishStreamPart(
                        FinishReason.ToolCalls,
                        new LanguageModelUsage(3, 10, 13),
                        "tool-calls"
                    )
                ),
                1 => Parts(
                    new TextStartStreamPart("answer"),
                    new TextDeltaStreamPart("answer", "{\"elements\":[{\"value\":\"done\"}]}"),
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
                Output = OutputSpec.Array(OptionsOutputSupport.ValueSchema),
                Prompt = "Look up the value and return it.",
                StopWhen = StopWhen.IsStepCount(2),
            }
        );

        var partials = OptionsOutputSupport.Collect(result);
        var elements = OptionsOutputSupport.CollectElements(result);
        var output = result.Output;

        Assert.Equal(new[] { "[{\"value\":\"done\"}]" }, await partials);
        Assert.Equal(new[] { "{\"value\":\"done\"}" }, await elements);
        Assert.Equal("[{\"value\":\"done\"}]", (await output)!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(Prefix + "should error elementStream when the model exceeds maxItems")]
    public async Task Should_error_elementStream_when_the_model_exceeds_maxItems()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text(
                "{\"elements\":[",
                "\"element 1\",",
                "\"element 2\",",
                "\"element 3\"]}"
            ),
            OutputSpec.Array("{\"type\":\"string\"}", maxItems: 2)
        );

        var elements = new List<string>();
        Exception? streamError = null;
        try
        {
            await foreach (var element in result.ElementStream())
            {
                elements.Add(element.GetString()!);
            }
        }
        catch (Exception error)
        {
            streamError = error;
        }

        Assert.Equal(new[] { "element 1", "element 2" }, elements);
        var validation = Assert.IsType<Vercel.AI.ProviderUtils.TypeValidationException>(
            streamError
        );
        Assert.Contains("elements array must contain at most 2 items", validation.Message);
        var failure = await Assert.ThrowsAsync<Vercel.AI.GenerateObject.NoObjectGeneratedException>(
            async () =>
                await result.Output
        );
        Assert.IsType<Vercel.AI.ProviderUtils.TypeValidationException>(failure.InnerException);
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
