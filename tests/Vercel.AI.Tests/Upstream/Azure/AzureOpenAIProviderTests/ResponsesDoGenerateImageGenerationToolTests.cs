// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses > doGenerate > image generation tool</c>.</summary>
public sealed class ResponsesDoGenerateImageGenerationToolTests
{
    private const string Id =
        "packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > image generation tool::";

    private static async Task<(OpenAICapture Capture, LanguageModelGenerateResult Result)> Run()
    {
        var capture = JsonFixture("azure-image-generation-tool.1");
        var result = await Generate(
            capture,
            Call(
                ProviderTool(
                    "openai.image_generation",
                    "image_generation",
                    "{\"outputFormat\":\"webp\",\"quality\":\"low\",\"size\":\"1024x1024\",\"partialImages\":2}"
                ),
                azureOptions: null
            )
        );
        return (capture, result);
    }

    [Fact]
    [UpstreamTest(
        Id + "should send request body with include and tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_request_body_with_include_and_tool()
    {
        var (capture, _) = await Run();

        var body = RequestBody(capture);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"input\":[{\"content\":[{\"text\":\"Hello\",\"type\":\"input_text\"}],\"role\":\"user\"}],\"model\":\"test-deployment\",\"tools\":[{\"output_format\":\"webp\",\"partial_images\":2,\"quality\":\"low\",\"size\":\"1024x1024\",\"type\":\"image_generation\"}]}"
                ),
                body
            ),
            body.ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should include generate image tool call and result in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_generate_image_tool_call_and_result_in_content()
    {
        var (_, result) = await Run();

        OpenAIResponsesSupport.AssertContentSnapshot(
            result.Content,
            "azure-image-generation-tool.1",
            includeCitations: true,
            SequentialIds()
        );
    }
}
