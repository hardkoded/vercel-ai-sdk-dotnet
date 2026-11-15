// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Provider;
using AzureProvider = Vercel.AI.Azure.AzureOpenAIProvider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>
/// Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses (default language model)</c> &gt; <c>doGenerate</c>, the
/// Foundry project test. The other tests of this describe are ported in <c>AzureOpenAIParityTests</c>.
/// </summary>
public sealed class ResponsesDefaultLanguageModelDoGenerateTests
{
    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::responses (default language model) > doGenerate::should include explicit message item types for Foundry project endpoints", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Should_include_explicit_message_item_types_for_Foundry_project_endpoints()
    {
        var handler = new UpstreamRecordingHandler("{\"id\":\"resp_1\",\"output\":[],\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}");
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions
            {
                BaseUrl = "https://test-resource.services.ai.azure.com/api/projects/test-project/openai/v1",
                ApiKey = "test-api-key",
            },
            handler);

        await provider.LanguageModel("test-deployment").DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new SystemModelMessage("You are concise."),
                    new UserModelMessage("Say hi."),
                    new AssistantModelMessage("Hi.", null, null),
                    new UserModelMessage("Say it again."),
                },
            },
            CancellationToken.None);

        Assert.Equal("https://test-resource.services.ai.azure.com/api/projects/test-project/openai/v1/responses", handler.Uri);
        var expected = JsonNode.Parse(
            "[{\"type\":\"message\",\"role\":\"system\",\"content\":\"You are concise.\"},"
            + "{\"type\":\"message\",\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Say hi.\"}]},"
            + "{\"type\":\"message\",\"role\":\"assistant\",\"content\":\"Hi.\"},"
            + "{\"type\":\"message\",\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Say it again.\"}]}]")!;
        Assert.True(JsonNode.DeepEquals(expected, JsonNode.Parse(handler.Body)!["input"]));
    }
}
