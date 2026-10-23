// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.AmazonBedrock;

namespace Vercel.AI.Tests.Upstream.Azure.AzureModelId;

/// <summary>Port of <c>azure-model-id.test.ts</c> &gt; <c>separate Azure deployment and model IDs</c> &gt; <c>deployment-based URLs</c>.</summary>
public sealed class DeploymentBasedUrlsTests
{
    private const string BaseUrl = "https://test-resource.openai.azure.com/openai";
    private const string DeploymentId = "production-chat";
    private const string ModelId = "gpt-5";
    private const string ResponseModelId = "gpt-5-2025-08-07";

    private static ILanguageModel CreateModel(AzureOpenAIProvider provider, string api)
    {
        var settings = new AzureOpenAILanguageModelSettings { ModelId = ModelId };
        return api switch
        {
            "responses" => provider.ResponsesModel(DeploymentId, settings),
            "chat" => provider.ChatModel(DeploymentId, settings),
            _ => provider.CompletionModel(DeploymentId, settings),
        };
    }

    private static string Path(string api)
    {
        return api switch
        {
            "responses" => "/responses",
            "chat" => "/chat/completions",
            _ => "/completions",
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
    [InlineData(false, "responses", "generate")]
    [InlineData(false, "responses", "stream")]
    [InlineData(false, "chat", "generate")]
    [InlineData(false, "chat", "stream")]
    [InlineData(false, "completion", "generate")]
    [InlineData(false, "completion", "stream")]
    [InlineData(true, "responses", "generate")]
    [InlineData(true, "responses", "stream")]
    [InlineData(true, "chat", "generate")]
    [InlineData(true, "chat", "stream")]
    [InlineData(true, "completion", "generate")]
    [InlineData(true, "completion", "stream")]
    [UpstreamTest("packages/azure/src/azure-model-id.test.ts::separate Azure deployment and model IDs > deployment-based URLs: %s > %s::%s routes by deployment and infers capabilities from the model ID", Coverage = UpstreamCoverage.Covered)]
    public async Task RoutesByDeploymentAndInfersCapabilitiesFromTheModelId(bool useDeploymentBasedUrls, string api, string operation)
    {
        var handler = new UpstreamRecordingHandler(operation == "generate" ? Response(api) : StreamBody(api));
        var provider = AzureOpenAIProvider.Create(
            new AzureOpenAIOptions { ResourceName = "test-resource", ApiKey = "test-api-key", UseDeploymentBasedUrls = useDeploymentBasedUrls },
            handler);
        var model = CreateModel(provider, api);
        var url = useDeploymentBasedUrls
            ? BaseUrl + "/deployments/" + DeploymentId + Path(api)
            : BaseUrl + "/v1" + Path(api);

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

        Assert.Equal(url + "?api-version=v1", handler.Uri);
        var request = JsonNode.Parse(handler.Body)!.AsObject();
        Assert.Equal(DeploymentId, request["model"]!.GetValue<string>());
        if (api == "completion")
        {
            Assert.Equal(0.5, request["temperature"]!.GetValue<double>());
        }
        else
        {
            Assert.Null(request["temperature"]);
            var first = (api == "responses" ? request["input"] : request["messages"])![0]!;
            Assert.Equal("developer", first["role"]!.GetValue<string>());
        }
    }

    private static string Response(string api)
    {
        return api == "responses"
            ? "{\"id\":\"resp-test\",\"created_at\":1741257730,\"model\":\"" + ResponseModelId + "\",\"output\":[],\"status\":\"completed\",\"usage\":{\"input_tokens\":4,\"output_tokens\":0,\"total_tokens\":4}}"
            : "{\"id\":\"chatcmpl-test\",\"created\":1741257730,\"model\":\"" + ResponseModelId + "\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"Hello\"},\"text\":\"Hello\",\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":4,\"completion_tokens\":1,\"total_tokens\":5}}";
    }

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
