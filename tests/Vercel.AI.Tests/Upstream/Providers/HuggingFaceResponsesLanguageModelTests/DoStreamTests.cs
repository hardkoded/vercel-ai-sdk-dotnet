// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.HuggingFace;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.HuggingFaceResponsesLanguageModelTests;

/// <summary>Port of <c>huggingface-responses-language-model.test.ts</c> &gt; <c>HuggingFaceResponsesLanguageModel &gt; doStream</c>.</summary>
public sealed class DoStreamTests
{
    [Fact]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > doStream::should forward tool choice none while preserving tool definitions", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_tool_choice_none_while_preserving_tool_definitions()
    {
        var handler = new SseStreamHandler(new[]
        {
            "data:{\"type\":\"response.completed\",\"response\":{\"id\":\"resp_test\",\"status\":\"completed\",\"incomplete_details\":null,\"usage\":null},\"sequence_number\":1}\n\n",
        });
        var model = HuggingFaceProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, handler).ResponsesModel("test-model");
        var options = UpstreamChat.Prompt();
        options.Tools = new[] { new LanguageModelTool("test", null, UpstreamChat.Json("{\"type\":\"object\"}")) };
        options.ToolChoice = ToolChoice.None;

        await UpstreamChat.Read(await model.DoStreamAsync(options, CancellationToken.None));

        var body = JsonNode.Parse(handler.Body)!;
        Assert.Equal("none", body["tool_choice"]!.GetValue<string>());
        var tool = Assert.Single(body["tools"]!.AsArray());
        Assert.Equal("function", tool!["type"]!.GetValue<string>());
        Assert.Equal("test", tool["name"]!.GetValue<string>());
        Assert.Equal("object", tool["parameters"]!["type"]!.GetValue<string>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest("packages/huggingface/src/responses/huggingface-responses-language-model.test.ts::HuggingFaceResponsesLanguageModel > doStream::handles a truncated stream (with error: %s)", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_a_truncated_stream_with_error(bool withError)
    {
        var handler = new SseStreamHandler(
            new[]
            {
                "data: {\"type\":\"response.created\",\"response\":{\"id\":\"r1\",\"created_at\":1,\"model\":\"test-model\",\"status\":\"in_progress\"}}\n\n",
                "data: {\"type\":\"response.output_text.delta\",\"item_id\":\"msg1\",\"output_index\":0,\"content_index\":0,\"delta\":\"Partial answer\",\"sequence_number\":1}\n\n",
            }.Concat(withError ? new[] { "data: {invalid-json}\n\n" } : Array.Empty<string>()));
        var model = HuggingFaceProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, handler).ResponsesModel("test-model");

        var parts = await UpstreamChat.Read(await model.DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));

        Assert.Contains(parts.OfType<TextDeltaStreamPart>(), part => part.Delta == "Partial answer");
        Assert.Equal(withError ? 1 : 0, parts.OfType<ErrorStreamPart>().Count());
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(withError ? FinishReason.Error : FinishReason.Interrupted, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
    }
}
