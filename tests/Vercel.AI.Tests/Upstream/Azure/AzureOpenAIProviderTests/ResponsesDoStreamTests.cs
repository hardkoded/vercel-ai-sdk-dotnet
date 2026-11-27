// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Azure;
using Vercel.AI.Provider;
using Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;
using AzureProvider = Vercel.AI.Azure.AzureOpenAIProvider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses</c> &gt; <c>doStream</c>.</summary>
public sealed class ResponsesDoStreamTests
{
    private const string Id =
        "packages/azure/src/azure-openai-provider.test.ts::responses > doStream::";

    private const string Calculator =
        "{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"number\"},\"b\":{\"type\":\"number\"},\"op\":{\"type\":\"string\"}},\"required\":[\"a\",\"b\"],\"additionalProperties\":false}";

    [Fact]
    [UpstreamTest(Id + "should extract response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_response_headers()
    {
        var handler = new SseStreamHandler(
            SseStreamHandler.FixtureChunks("azure-text.1", done: true),
            new Dictionary<string, string> { ["test-header"] = "test-value" }
        );
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" },
            handler
        );

        var result = await provider
            .ResponsesModel("test-deployment")
            .DoStreamAsync(
                new LanguageModelCallOptions
                {
                    Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
                },
                CancellationToken.None
            );

        SseStreamHandler.AssertDefaultHeaders(result.Response!.Headers);
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle file_citation annotations without optional fields in streaming",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_file_citation_annotations_without_optional_fields_in_streaming()
    {
        var parts = await Stream(ChunksFixture("azure-file-citation-annotations"), Call());

        OpenAIResponsesSupport.AssertStreamSnapshot(
            parts,
            "azure-file-citation-annotations",
            SequentialIds(),
            includeCitations: true
        );
    }

    [Fact]
    [UpstreamTest(Id + "should send code interpreter calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_code_interpreter_calls()
    {
        var parts = await Stream(
            ChunksFixture("azure-code-interpreter-tool.1"),
            Call(ProviderTool("openai.code_interpreter", "code_interpreter"))
        );

        OpenAIResponsesSupport.AssertStreamSnapshot(
            parts,
            "azure-code-interpreter-tool.1",
            SequentialIds(),
            includeCitations: true
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should stream with reasoning encrypted content include reasoning-delta part",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_with_reasoning_encrypted_content_include_reasoning_delta_part()
    {
        var parts = await Stream(
            ChunksFixture("azure-reasoning-encrypted-content.1"),
            Call(
                new[] { new LanguageModelTool("calculator", null, Json(Calculator)) },
                azureOptions: "{\"reasoningEffort\":\"high\",\"maxCompletionTokens\":32000,\"store\":false,\"include\":[\"reasoning.encrypted_content\"],\"reasoningSummary\":\"auto\",\"forceReasoning\":true}",
                optionsKey: "openai"
            )
        );

        OpenAIResponsesSupport.AssertStreamSnapshot(
            parts,
            "azure-reasoning-encrypted-content.1",
            SequentialIds(),
            includeCitations: true
        );
    }
}
