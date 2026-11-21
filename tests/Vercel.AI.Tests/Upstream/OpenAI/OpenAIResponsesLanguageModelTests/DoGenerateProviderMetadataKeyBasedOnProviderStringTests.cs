// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Azure;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; providerMetadata key based on provider string</c>.</summary>
public sealed class DoGenerateProviderMetadataKeyBasedOnProviderStringTests
{
    private const string Prefix =
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > providerMetadata key based on provider string::";

    [Fact]
    [UpstreamTest(
        Prefix + "should use \"azure\" as providerMetadata key when provider includes \"azure\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_azure_as_providerMetadata_key_when_provider_includes_azure()
    {
        var result = await Generate(
            AzureModel,
            "resp_provider_metadata_azure",
            Message("msg_azure_text", "Hello from Azure!")
        );

        var metadata = result.ProviderMetadata!.Value;
        Assert.True(metadata.TryGetProperty("azure", out var azure));
        Assert.False(metadata.TryGetProperty("openai", out _));
        Assert.Equal("resp_provider_metadata_azure", azure.GetProperty("responseId").GetString());
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should use \"openai\" as providerMetadata key when provider does not include \"azure\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_openai_as_providerMetadata_key_when_provider_does_not_include_azure()
    {
        var result = await Generate(
            OpenAIModel,
            "resp_provider_metadata_openai",
            Message("msg_openai_text", "Hello from OpenAI!")
        );

        var metadata = result.ProviderMetadata!.Value;
        Assert.True(metadata.TryGetProperty("openai", out var openai));
        Assert.False(metadata.TryGetProperty("azure", out _));
        Assert.Equal("resp_provider_metadata_openai", openai.GetProperty("responseId").GetString());
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should use \"azure\" as providerMetadata key in tool call content when provider includes \"azure\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_azure_as_providerMetadata_key_in_tool_call_content_when_provider_includes_azure()
    {
        var result = await Generate(
            AzureModel,
            "resp_provider_metadata_tool_call",
            """{"id":"fc_azure","type":"function_call","status":"completed","call_id":"call_azure","name":"weather","arguments":"{\"location\":\"Seattle\"}"}""",
            OpenAIResponsesSupport.TestTools
        );

        var toolCall = result.Content.OfType<GeneratedToolCall>().First();
        var metadata = toolCall.ProviderMetadata!.Value;
        Assert.True(metadata.TryGetProperty("azure", out _));
        Assert.False(metadata.TryGetProperty("openai", out _));
    }

    private static string Message(string id, string text) =>
        JsonSerializer.Serialize(
            new
            {
                id,
                type = "message",
                status = "completed",
                role = "assistant",
                content = new[]
                {
                    new
                    {
                        type = "output_text",
                        text,
                        annotations = Array.Empty<object>(),
                    },
                },
            }
        );

    private static ILanguageModel AzureModel(OpenAICapture capture) =>
        AzureOpenAIProvider
            .Create(
                new AzureOpenAIOptions { ResourceName = "test-resource", ApiKey = "test-api-key" },
                capture
            )
            .ResponsesModel("gpt-4o");

    private static ILanguageModel OpenAIModel(OpenAICapture capture) =>
        OpenAIUpstream.Provider(capture).ResponsesModel("gpt-4o");

    private static async Task<LanguageModelGenerateResult> Generate(
        Func<OpenAICapture, ILanguageModel> model,
        string id,
        string outputItem,
        IReadOnlyList<LanguageModelTool>? tools = null
    )
    {
        var capture = new OpenAICapture
        {
            ResponseJson = $$$"""
                {"id":"{{{id}}}","object":"response","created_at":1234567890,"status":"completed","error":null,"incomplete_details":null,"input":[],"instructions":null,"max_output_tokens":null,"model":"gpt-4o","output":[{{{outputItem}}}],"parallel_tool_calls":true,"previous_response_id":null,"reasoning":{"effort":null,"summary":null},"store":true,"temperature":0,"text":{"format":{"type":"text"}},"tool_choice":"auto","tools":[],"top_p":1,"truncation":"disabled","usage":{"input_tokens":10,"input_tokens_details":{"cached_tokens":0},"output_tokens":5,"output_tokens_details":{"reasoning_tokens":0},"total_tokens":15},"user":null,"metadata":{}}
                """,
        };
        var options = OpenAIUpstream.Hello();
        options.Tools = tools;
        return await model(capture).DoGenerateAsync(options, CancellationToken.None);
    }
}
