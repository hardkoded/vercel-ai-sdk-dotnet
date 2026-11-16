// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.AmazonBedrock;
using AzureProvider = Vercel.AI.Azure.AzureOpenAIProvider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>deepseek</c>.</summary>
public sealed class DeepSeekTests
{
    private const string Url =
        "https://test-resource.openai.azure.com/openai/v1/chat/completions?api-version=v1";

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::deepseek::should map top-level reasoning to Azure DeepSeek reasoning effort",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_top_level_reasoning_to_Azure_DeepSeek_reasoning_effort()
    {
        var handler = new UpstreamRecordingHandler(Fixture("azure-deepseek-reasoning.1.json"));
        var provider = Provider(handler);

        var result = await provider
            .DeepSeek("deepseek-v4-pro")
            .DoGenerateAsync(
                new LanguageModelCallOptions { Prompt = Prompt(), Reasoning = "high" },
                CancellationToken.None
            );

        Assert.Equal(Url, handler.Uri);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(Body("deepseek-v4-pro", "high")),
                JsonNode.Parse(handler.Body)
            )
        );
        Assert.Equal(["reasoning", "text"], result.Content.Select(part => part.Type).ToArray());
        var expected = JsonNode.Parse(Fixture("azure-deepseek-reasoning.1.json"))!["choices"]![0]![
            "message"
        ]!["reasoning_content"]!.GetValue<string>();
        Assert.Equal(expected, Assert.IsAssignableFrom<GeneratedReasoning>(result.Content[0]).Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::deepseek::should parse providerOptions from the azure namespace",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_parse_providerOptions_from_the_azure_namespace()
    {
        var handler = new UpstreamRecordingHandler(Fixture("azure-deepseek-reasoning.1.json"));
        var provider = Provider(handler);

        await provider
            .DeepSeek("deepseek-v4-flash")
            .DoGenerateAsync(
                new LanguageModelCallOptions
                {
                    Prompt = Prompt(),
                    Reasoning = "high",
                    ProviderOptions = new Dictionary<string, JsonElement>
                    {
                        ["azure"] = Parse("{\"reasoningEffort\":\"max\"}"),
                    },
                },
                CancellationToken.None
            );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(Body("deepseek-v4-flash", "max")),
                JsonNode.Parse(handler.Body)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::deepseek::should send a json_schema response format for structured output",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_a_json_schema_response_format_for_structured_output()
    {
        var handler = new UpstreamRecordingHandler(Fixture("azure-deepseek-reasoning.1.json"));
        var provider = Provider(handler);

        await provider
            .DeepSeek("deepseek-v4-flash")
            .DoGenerateAsync(
                new LanguageModelCallOptions
                {
                    Prompt = Prompt(),
                    Reasoning = "high",
                    JsonSchema = Parse(
                        "{\"type\":\"object\",\"properties\":{\"sentiment\":{\"type\":\"string\"}},\"required\":[\"sentiment\"],\"additionalProperties\":false}"
                    ),
                },
                CancellationToken.None
            );

        var expected = JsonNode.Parse(Body("deepseek-v4-flash", "high"))!.AsObject();
        expected["response_format"] = JsonNode.Parse(
            "{\"type\":\"json_schema\",\"json_schema\":{\"name\":\"response\",\"schema\":{\"type\":\"object\",\"properties\":{\"sentiment\":{\"type\":\"string\"}},\"required\":[\"sentiment\"],\"additionalProperties\":false},\"strict\":true}}"
        );
        Assert.True(JsonNode.DeepEquals(expected, JsonNode.Parse(handler.Body)));
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::deepseek::should stream reasoning content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_reasoning_content()
    {
        var handler = new SseStreamHandler(
            SseStreamHandler.FixtureChunks("azure-deepseek-reasoning.1", done: true)
        );
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" },
            handler
        );

        var result = await provider
            .DeepSeek("deepseek-v4-pro")
            .DoStreamAsync(
                new LanguageModelCallOptions { Prompt = Prompt(), Reasoning = "high" },
                CancellationToken.None
            );

        var expected = JsonNode.Parse(Body("deepseek-v4-pro", "high"))!.AsObject();
        expected["stream"] = true;
        expected["stream_options"] = JsonNode.Parse("{\"include_usage\":true}");
        Assert.True(JsonNode.DeepEquals(expected, JsonNode.Parse(handler.Body)), handler.Body);

        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in result)
        {
            parts.Add(part);
        }

        Assert.IsType<StreamStartStreamPart>(parts[0]);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal("7334c29da064437e9d158710cdefbae6", metadata.Id);
        Assert.Equal("deepseek-v4-pro", metadata.ModelId);
        Assert.Equal("reasoning-0", Assert.IsType<ReasoningStartStreamPart>(parts[2]).Id);
        Assert.Equal("We", Assert.IsType<ReasoningDeltaStreamPart>(parts[3]).Delta);
        Assert.Equal(" need to", Assert.IsType<ReasoningDeltaStreamPart>(parts[4]).Delta);
        Assert.Equal("txt-0", Assert.Single(parts.OfType<TextStartStreamPart>()).Id);
        Assert.Equal("reasoning-0", Assert.Single(parts.OfType<ReasoningEndStreamPart>()).Id);

        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal("stop", finish.RawFinishReason);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal(19, finish.Usage.InputTokens);
        Assert.Equal(1720, finish.Usage.OutputTokens);
        Assert.Equal(0, finish.Usage.CacheReadTokens);
        Assert.True(
            finish.ProviderMetadata!.Value.TryGetProperty("azure", out var azure),
            finish.ProviderMetadata.ToString()
        );
        Assert.Equal("assistant", azure.GetProperty("messageRole").GetString());
        Assert.Equal(0, azure.GetProperty("choiceIndex").GetInt32());
        Assert.Equal("chat.completion.chunk", azure.GetProperty("responseObject").GetString());
    }

    private static AzureProvider Provider(HttpMessageHandler handler)
    {
        return AzureProvider.Create(
            new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" },
            handler
        );
    }

    private static string Fixture(string name)
    {
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));
    }

    private static string Body(string model, string effort)
    {
        return new JsonObject
        {
            ["model"] = model,
            ["messages"] = new JsonArray(
                new JsonObject { ["role"] = "user", ["content"] = "Hello" }
            ),
            ["reasoning_effort"] = effort,
        }.ToJsonString();
    }

    private static ModelMessage[] Prompt()
    {
        return [new UserModelMessage("Hello")];
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
