// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleLanguageModelTests;

/// <summary>Port of <c>google-language-model.test.ts</c> &gt; <c>doStream</c> &gt; <c>streaming-tool-call-empty-container-arguments</c>.</summary>
public sealed class DoStreamStreamingToolCallEmptyContainerArgumentsTests
{
    [Fact]
    [UpstreamTest(
        "packages/google/src/google-language-model.test.ts::doStream > streaming-tool-call-empty-container-arguments::should keep empty arrays and objects in streamed function call arguments",
        Coverage = UpstreamCoverage.Covered,
        Commit = "bbe2bd1163bd251ced45a72610af71a597eca4bf"
    )]
    public async Task Should_keep_empty_arrays_and_objects_in_streamed_function_call_arguments()
    {
        var chunks = new[]
        {
            FunctionCallChunk("{\"name\":\"t\",\"willContinue\":true}"),
            PartialArgsChunk("{\"jsonPath\":\"$.id\",\"stringValue\":\"x\"}", true),
            PartialArgsChunk("{\"jsonPath\":\"$.emptyArr[]\"}", true),
            PartialArgsChunk("{\"jsonPath\":\"$.emptyObj\"}", true),
            PartialArgsChunk("{\"jsonPath\":\"$.nested.tags[]\"}", true),
            PartialArgsChunk("{\"jsonPath\":\"$.zero\",\"numberValue\":0}", false),
            "{\"candidates\":[{\"content\":{\"role\":\"model\",\"parts\":[{\"text\":\"\"}]},\"finishReason\":\"STOP\"}]}",
        };
        var handler = new RecordingHandler
        {
            ResponseText = string.Concat(chunks.Select(chunk => "data: " + chunk + "\n\n")),
            MediaType = "text/event-stream",
        };
        var provider = GoogleVertexProvider.Create(
            new VertexOptions
            {
                Project = "test-project",
                Region = "us-central1",
                ApiKey = "test-api-key",
                GenerateId = () => "test-id",
            },
            handler
        );

        var options = GoogleUpstream.Hello();
        options.Tools = new[]
        {
            GoogleUpstream.Function("t", "{\"type\":\"object\",\"properties\":{}}", null),
        };
        options.ProviderOptions = GoogleUpstream.ProviderOptions(
            "{\"vertex\":{\"streamFunctionCallArguments\":true}}"
        );
        var events = await GoogleUpstream.Collect(
            await provider
                .LanguageModel("gemini-pro")
                .DoStreamAsync(options, CancellationToken.None)
        );

        var input = string.Concat(
            events.OfType<ToolInputDeltaStreamPart>().Select(part => part.Delta)
        );
        var toolCalls = events.OfType<ToolCallStreamPart>().ToList();

        var expectedInput =
            "{\"id\":\"x\",\"emptyArr\":[],\"emptyObj\":{},\"nested\":{\"tags\":[]},\"zero\":0}";
        Assert.Equal(
            new[] { expectedInput.Length },
            toolCalls.Select(call => call.ArgumentsJson.Length)
        );
        Assert.Equal(new[] { expectedInput }, toolCalls.Select(call => call.ArgumentsJson));
        Assert.Equal(expectedInput, input);
    }

    private static string PartialArgsChunk(string arg, bool willContinue)
    {
        return FunctionCallChunk(
            "{\"partialArgs\":["
                + arg
                + "]"
                + (willContinue ? ",\"willContinue\":true" : string.Empty)
                + "}"
        );
    }

    private static string FunctionCallChunk(string functionCall)
    {
        return "{\"candidates\":[{\"content\":{\"role\":\"model\",\"parts\":[{\"functionCall\":"
            + functionCall
            + "}]}}]}";
    }
}
