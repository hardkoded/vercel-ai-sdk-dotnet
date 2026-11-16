// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Helpers of the <c>azure-openai-provider.test.ts</c> &gt; <c>responses</c> &gt; <c>doGenerate</c> tool tests.</summary>
internal static class AzureResponsesToolSupport
{
    internal const string Url = "https://test-resource.openai.azure.com/openai/v1/responses";

    internal static string Fixture(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));

    internal static JsonElement Json(string json) => OpenAIUpstream.Json(json);

    internal static AzureOpenAIProvider Provider(OpenAICapture capture) =>
        AzureOpenAIProvider.Create(new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" }, capture);

    internal static LanguageModelCallOptions Call(IReadOnlyList<LanguageModelTool>? tools = null, IReadOnlyList<ModelMessage>? extra = null, string? azureOptions = null)
    {
        var prompt = new List<ModelMessage> { new UserModelMessage("Hello") };
        if (extra != null)
        {
            prompt.AddRange(extra);
        }

        return new LanguageModelCallOptions
        {
            Prompt = prompt,
            Tools = tools,
            ProviderOptions = azureOptions == null ? null : new Dictionary<string, JsonElement> { ["azure"] = Json(azureOptions) },
        };
    }

    internal static async Task<LanguageModelGenerateResult> Generate(OpenAICapture capture, LanguageModelCallOptions options) =>
        await Provider(capture).ResponsesModel("test-deployment").DoGenerateAsync(options, CancellationToken.None);

    internal static async Task<List<LanguageModelStreamPart>> Stream(OpenAICapture capture, LanguageModelCallOptions options) =>
        await OpenAIUpstream.Read(await Provider(capture).ResponsesModel("test-deployment").DoStreamAsync(options, CancellationToken.None));

    internal static JsonNode RequestBody(OpenAICapture capture) => JsonNode.Parse(capture.Body)!;

    internal static void AssertTools(OpenAICapture capture, string expected) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), RequestBody(capture)["tools"]), RequestBody(capture)["tools"]!.ToJsonString());

    internal static string Sse(IEnumerable<JsonNode> events) =>
        string.Concat(events.Select(@event => "data: " + @event.ToJsonString() + "\n\n"));
}
