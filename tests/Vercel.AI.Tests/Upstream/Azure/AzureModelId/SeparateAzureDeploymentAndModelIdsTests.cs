// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.AmazonBedrock;

namespace Vercel.AI.Tests.Upstream.Azure.AzureModelId;

/// <summary>Port of <c>azure-model-id.test.ts</c> &gt; <c>separate Azure deployment and model IDs</c>.</summary>
public sealed class SeparateAzureDeploymentAndModelIdsTests
{
    private const string DeploymentId = "production-chat";
    private const string ModelId = "gpt-5";
    private const string ResponseModelId = "gpt-5-2025-08-07";

    private static AzureOpenAIProvider CreateProvider(HttpMessageHandler handler)
    {
        return AzureOpenAIProvider.Create(new AzureOpenAIOptions { ResourceName = "test-resource", ApiKey = "test-api-key" }, handler);
    }

    private static ILanguageModel CreateModel(AzureOpenAIProvider provider, string api, AzureOpenAILanguageModelSettings? settings)
    {
        return api switch
        {
            "responses" => provider.ResponsesModel(DeploymentId, settings),
            "chat" => provider.ChatModel(DeploymentId, settings),
            _ => provider.CompletionModel(DeploymentId, settings),
        };
    }

    private static LanguageModelCallOptions CallOptions()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage("Be helpful."),
                new UserModelMessage("Hello"),
            },
            Temperature = 0.5,
        };
    }

    [Theory]
    [InlineData("responses")]
    [InlineData("chat")]
    [InlineData("completion")]
    public void ReportsTheSuppliedModelIdAndFallsBackToTheDeploymentId(string factory)
    {
        var provider = CreateProvider(new UpstreamRecordingHandler("{}"));

        Assert.Equal(ModelId, CreateModel(provider, factory, new AzureOpenAILanguageModelSettings { ModelId = ModelId }).ModelId);
        Assert.Equal(DeploymentId, CreateModel(provider, factory, new AzureOpenAILanguageModelSettings()).ModelId);
        Assert.Equal(DeploymentId, CreateModel(provider, factory, new AzureOpenAILanguageModelSettings { ModelId = null }).ModelId);
        Assert.Equal(DeploymentId, CreateModel(provider, factory, null).ModelId);
    }

    [Theory]
    [InlineData("responses", "generate")]
    [InlineData("responses", "stream")]
    [InlineData("chat", "generate")]
    [InlineData("chat", "stream")]
    [InlineData("completion", "generate")]
    [InlineData("completion", "stream")]
    public async Task RoutesByDeploymentAndInfersCapabilitiesFromTheModelId(string api, string operation)
    {
        var handler = new UpstreamRecordingHandler(operation == "generate" ? GenerateBody(api) : StreamBody(api));
        var model = CreateModel(CreateProvider(handler), api, new AzureOpenAILanguageModelSettings { ModelId = ModelId });

        if (operation == "generate")
        {
            var result = await model.DoGenerateAsync(CallOptions(), CancellationToken.None);
            Assert.Equal(ResponseModelId, result.ResponseModelId);
        }
        else
        {
            var result = await model.DoStreamAsync(CallOptions(), CancellationToken.None);
            var parts = new List<LanguageModelStreamPart>();
            await foreach (var part in result.Stream)
            {
                parts.Add(part);
            }

            Assert.DoesNotContain(parts, part => part is ErrorStreamPart);
            Assert.Contains(parts, part => part is ResponseMetadataStreamPart metadata && metadata.ModelId == ResponseModelId);
        }

        var request = JsonNode.Parse(handler.Body)!.AsObject();
        Assert.Equal(DeploymentId, request["model"]!.GetValue<string>());
        if (api == "chat" || api == "completion")
        {
            Assert.Contains("/deployments/" + DeploymentId + "/", handler.Uri, StringComparison.Ordinal);
        }

        if (api == "completion")
        {
            Assert.Equal(0.5, request["temperature"]!.GetValue<double>());
        }
        else if (api == "responses")
        {
            Assert.Null(request["temperature"]);
            Assert.Equal("developer", request["input"]![0]!["role"]!.GetValue<string>());
        }
    }

    private static string Response(string api)
    {
        return api == "responses"
            ? "{\"id\":\"resp-test\",\"created_at\":1741257730,\"model\":\"" + ResponseModelId + "\",\"output\":[],\"status\":\"completed\",\"usage\":{\"input_tokens\":4,\"output_tokens\":0,\"total_tokens\":4}}"
            : "{\"id\":\"chatcmpl-test\",\"created\":1741257730,\"model\":\"" + ResponseModelId + "\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"Hello\"},\"text\":\"Hello\",\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":4,\"completion_tokens\":1,\"total_tokens\":5}}";
    }

    private static string GenerateBody(string api) => Response(api);

    private static string StreamBody(string api)
    {
        var chunks = api == "responses"
            ? new[]
            {
                "{\"type\":\"response.created\",\"response\":" + Response(api) + "}",
                "{\"type\":\"response.completed\",\"response\":" + Response(api) + "}",
            }
            : new[]
            {
                "{\"id\":\"chatcmpl-test\",\"created\":1741257730,\"model\":\"" + ResponseModelId + "\",\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"Hello\"},\"text\":\"Hello\",\"finish_reason\":\"stop\"}]}",
            };
        return string.Concat(chunks.Select(chunk => "data: " + chunk + "\n\n")) + "data: [DONE]\n\n";
    }
}
