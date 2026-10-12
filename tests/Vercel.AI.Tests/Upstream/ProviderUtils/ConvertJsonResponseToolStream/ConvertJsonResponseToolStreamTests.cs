// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.ConvertJsonResponseToolStream;

/// <summary>Port of <c>convert-json-response-tool-stream.test.ts</c> &gt; <c>convertJsonResponseToolStream</c>.</summary>
public sealed class ConvertJsonResponseToolStreamTests
{
    private const string Date = "{\"date\":\"2031-06-17\"}";

    private static readonly JsonElement Metadata = JsonDocument.Parse("{\"google\":{\"thoughtSignature\":\"signature\"}}").RootElement.Clone();

    private static FinishStreamPart Finish(FinishReason reason = FinishReason.ToolCalls, string raw = "tool_calls")
    {
        return new FinishStreamPart(reason, LanguageModelUsage.Empty, raw);
    }

    private static List<LanguageModelStreamPart> ResponseParts()
    {
        return new List<LanguageModelStreamPart>
        {
            new ToolInputStartStreamPart("response", "json", Metadata),
            new ToolInputDeltaStreamPart("response", "{\"date\":"),
            new ToolInputDeltaStreamPart("response", "\"2031-06-17\"}"),
            new ToolInputEndStreamPart("response"),
            new ToolCallStreamPart("response", "json", Date, Metadata),
        };
    }

    private static async Task<List<LanguageModelStreamPart>> Convert(IEnumerable<LanguageModelStreamPart> parts)
    {
        async IAsyncEnumerable<LanguageModelStreamPart> Source()
        {
            foreach (var part in parts)
            {
                yield return part;
                await Task.Yield();
            }
        }

        var result = new List<LanguageModelStreamPart>();
        await foreach (var part in JsonResponseTools.ConvertJsonResponseToolStream(Source(), "json"))
        {
            result.Add(part);
        }

        return result;
    }

    private static string Raw(JsonElement? metadata)
    {
        return metadata?.GetRawText() ?? string.Empty;
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-json-response-tool-stream.test.ts::convertJsonResponseToolStream::converts streamed arguments and preserves metadata and usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Converts_streamed_arguments_and_preserves_metadata_and_usage()
    {
        var finish = Finish();
        var parts = await Convert(ResponseParts().Concat(new[] { finish }));

        Assert.Equal(5, parts.Count);
        var start = Assert.IsType<TextStartStreamPart>(parts[0]);
        Assert.Equal("response", start.Id);
        Assert.Equal(Metadata.GetRawText(), Raw(start.ProviderMetadata));
        Assert.Equal("{\"date\":", Assert.IsType<TextDeltaStreamPart>(parts[1]).Delta);
        Assert.Equal("\"2031-06-17\"}", Assert.IsType<TextDeltaStreamPart>(parts[2]).Delta);
        var end = Assert.IsType<TextEndStreamPart>(parts[3]);
        Assert.Equal("response", end.Id);
        Assert.Equal(Metadata.GetRawText(), Raw(end.ProviderMetadata));
        var result = Assert.IsType<FinishStreamPart>(parts[4]);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("tool_calls", result.RawFinishReason);
        Assert.Same(finish.Usage, result.Usage);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-json-response-tool-stream.test.ts::convertJsonResponseToolStream::uses complete input when the provider emits no argument deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_complete_input_when_the_provider_emits_no_argument_deltas()
    {
        var parts = await Convert(new LanguageModelStreamPart[]
        {
            new ToolInputStartStreamPart("response", "json"),
            new ToolInputDeltaStreamPart("response", string.Empty),
            new ToolInputEndStreamPart("response"),
            new ToolCallStreamPart("response", "json", "{}"),
        });

        Assert.Equal(4, parts.Count);
        Assert.Equal("response", Assert.IsType<TextStartStreamPart>(parts[0]).Id);
        Assert.Equal(string.Empty, Assert.IsType<TextDeltaStreamPart>(parts[1]).Delta);
        Assert.Equal("{}", Assert.IsType<TextDeltaStreamPart>(parts[2]).Delta);
        Assert.Equal("response", Assert.IsType<TextEndStreamPart>(parts[3]).Id);
    }

    [Theory]
    [InlineData("before")]
    [InlineData("after")]
    [UpstreamTest("packages/provider-utils/src/convert-json-response-tool-stream.test.ts::convertJsonResponseToolStream::preserves an application call %s the response call", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_an_application_call_around_the_response_call(string order)
    {
        var application = new LanguageModelStreamPart[]
        {
            new ToolInputStartStreamPart("application", "resolveDate"),
            new ToolInputDeltaStreamPart("application", "{}"),
            new ToolInputEndStreamPart("application"),
            new ToolCallStreamPart("application", "resolveDate", "{}"),
        };
        var finish = Finish();
        var input = order == "before"
            ? application.Concat(ResponseParts())
            : ResponseParts().Concat(application);

        var parts = await Convert(input.Concat(new[] { finish }));

        Assert.Equal(application, parts.Where(part => part.Type.StartsWith("tool-", StringComparison.Ordinal)).ToList());
        var result = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Equal(Date, string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta)));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-json-response-tool-stream.test.ts::convertJsonResponseToolStream::preserves an incomplete finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_an_incomplete_finish_reason()
    {
        var incomplete = Finish(FinishReason.Length, "MAX_TOKENS");

        var parts = await Convert(ResponseParts().Concat(new[] { incomplete }));

        Assert.Same(incomplete, parts[parts.Count - 1]);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-json-response-tool-stream.test.ts::convertJsonResponseToolStream::omits conversational text and passes through reasoning and errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_conversational_text_and_passes_through_reasoning_and_errors()
    {
        var reasoning = new ReasoningDeltaStreamPart("reasoning", "Thinking.");
        var error = new ErrorStreamPart("provider error");

        var parts = await Convert(new LanguageModelStreamPart[]
        {
            new TextStartStreamPart("conversation"),
            new TextDeltaStreamPart("conversation", "Here is the answer."),
            new TextEndStreamPart("conversation"),
            reasoning,
            error,
        });

        Assert.Equal(new LanguageModelStreamPart[] { reasoning, error }, parts);
    }
}
