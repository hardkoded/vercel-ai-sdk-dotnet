// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; result.steps</c>.</summary>
public sealed class ResultStepsTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.steps::";

    [Fact]
    [UpstreamTest(Prefix + "should add reasoning-files from the model response to the step result", Coverage = UpstreamCoverage.Covered, Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Should_add_reasoning_files_from_the_model_response_to_the_step_result()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ReasoningFileStreamPart(Encoding.UTF8.GetBytes("reasoning-file-data-1"), "image/png"),
                new ReasoningStartStreamPart("1"),
                new ReasoningDeltaStreamPart("1", "Some reasoning text."),
                new ReasoningEndStreamPart("1"),
                new ReasoningFileStreamPart(Encoding.UTF8.GetBytes("reasoning-file-data-2"), "image/jpeg", JsonDocument.Parse("{\"testProvider\":{\"signature\":\"rf-sig-1\"}}").RootElement.Clone()),
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "Hello!"),
                new TextEndStreamPart("1"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };
        var result = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "prompt" });

        var step = Assert.Single(await result.Steps);

        Assert.Equal(4, step.Content.Count);
        var firstFile = Assert.IsType<GeneratedReasoningFile>(step.Content[0]);
        Assert.Equal("reasoning-file-data-1", Encoding.UTF8.GetString(firstFile.Data));
        Assert.Equal("image/png", firstFile.MediaType);
        Assert.Null(firstFile.ProviderMetadata);
        Assert.Equal("Some reasoning text.", Assert.IsType<GeneratedReasoning>(step.Content[1]).Text);
        var secondFile = Assert.IsType<GeneratedReasoningFile>(step.Content[2]);
        Assert.Equal("reasoning-file-data-2", Encoding.UTF8.GetString(secondFile.Data));
        Assert.Equal("image/jpeg", secondFile.MediaType);
        Assert.Equal("rf-sig-1", secondFile.ProviderMetadata!.Value.GetProperty("testProvider").GetProperty("signature").GetString());
        Assert.Equal("Hello!", Assert.IsType<GeneratedText>(step.Content[3]).Text);

        var message = Assert.IsType<AssistantModelMessage>(Assert.Single(step.Response.Messages));
        Assert.Equal(4, message.Parts.Count);
        var firstPart = Assert.IsType<AssistantReasoningFilePart>(message.Parts[0]);
        Assert.Equal("image/png", firstPart.MediaType);
        Assert.Null(firstPart.ProviderOptions);
        Assert.IsType<AssistantReasoningPart>(message.Parts[1]);
        var secondPart = Assert.IsType<AssistantReasoningFilePart>(message.Parts[2]);
        Assert.Equal("image/jpeg", secondPart.MediaType);
        Assert.Equal("rf-sig-1", secondPart.ProviderOptions!["testProvider"].GetProperty("signature").GetString());
        Assert.IsType<AssistantTextPart>(message.Parts[3]);
    }
}
