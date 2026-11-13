// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleLanguageModelTests;

/// <summary>Port of <c>google-language-model.test.ts</c> &gt; <c>doStream</c> &gt; <c>streaming-tool-call-sparse-array-arguments</c>.</summary>
public sealed class DoStreamStreamingToolCallSparseArrayArgumentsTests
{
    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream > streaming-tool-call-sparse-array-arguments::should skip streamed array indexes that leave a gap", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_skip_streamed_array_indexes_that_leave_a_gap()
    {
        var chunks = new[]
        {
            FunctionCallChunk("{\"name\":\"t\",\"willContinue\":true}"),
            FunctionCallChunk("{\"partialArgs\":[{\"jsonPath\":\"$.items[0]\",\"stringValue\":\"a\"}],\"willContinue\":true}"),
            FunctionCallChunk("{\"partialArgs\":[{\"jsonPath\":\"$.items[20000000]\",\"stringValue\":\"b\"}],\"willContinue\":true}"),
            FunctionCallChunk("{\"partialArgs\":[{\"jsonPath\":\"$.items[1]\",\"stringValue\":\"c\"}]}"),
            "{\"candidates\":[{\"content\":{\"role\":\"model\",\"parts\":[{\"text\":\"\"}]},\"finishReason\":\"STOP\"}]}",
        };
        var handler = new RecordingHandler
        {
            ResponseText = string.Concat(chunks.Select(chunk => "data: " + chunk + "\n\n")),
            MediaType = "text/event-stream",
        };
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "test-project", Region = "us-central1", ApiKey = "test-api-key", GenerateId = () => "test-id" }, handler);

        var options = GoogleUpstream.Hello();
        options.Tools = new[] { GoogleUpstream.Function("t", "{\"type\":\"object\",\"properties\":{}}", null) };
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"vertex\":{\"streamFunctionCallArguments\":true}}");
        var events = await GoogleUpstream.Collect(await provider.LanguageModel("gemini-pro").DoStreamAsync(options, CancellationToken.None));

        var input = string.Concat(events.OfType<ToolInputDeltaStreamPart>().Select(part => part.Delta));
        var toolCalls = events.OfType<ToolCallStreamPart>().ToList();

        var expectedInput = "{\"items\":[\"a\",\"c\"]}";
        Assert.Equal(new[] { expectedInput.Length }, toolCalls.Select(call => call.ArgumentsJson.Length));
        Assert.Equal(new[] { expectedInput }, toolCalls.Select(call => call.ArgumentsJson));
        Assert.Equal(expectedInput, input);
    }

    private static string FunctionCallChunk(string functionCall)
    {
        return "{\"candidates\":[{\"content\":{\"role\":\"model\",\"parts\":[{\"functionCall\":" + functionCall + "}]}}]}";
    }
}
