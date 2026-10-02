// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Azure provider, chat, DeepSeek, completion, embedding, and image calls.</summary>
public sealed class AzureOpenAIProviderParityTests
{
    private const string ResponsesDefault = "packages/azure/src/azure-openai-provider.test.ts::responses (default language model) > doGenerate::";

    private const string Chat = "packages/azure/src/azure-openai-provider.test.ts::chat > doGenerate::";

    private const string Deepseek = "packages/azure/src/azure-openai-provider.test.ts::deepseek::";

    private const string Completion = "packages/azure/src/azure-openai-provider.test.ts::completion > doGenerate::";

    private const string Embedding = "packages/azure/src/azure-openai-provider.test.ts::embedding";

    private const string Image = "packages/azure/src/azure-openai-provider.test.ts::image > doGenerate::";

    [Fact]
    [UpstreamTest(ResponsesDefault + "should set the correct modified api version", Coverage = UpstreamCoverage.Covered)]
    public async Task Responses_use_the_configured_api_version()
    {
        var handler = AzureParity.Handler(AzureParity.ResponsesOk);
        var provider = AzureParity.Provider(handler, options => options.ApiVersion = "2025-04-01-preview");
        await provider.Responses("test-deployment").DoGenerateAsync(AzureParity.Hello(), CancellationToken.None);

        var uri = handler.Uri!.ToString();
        Assert.Contains("api-version=2025-04-01-preview", uri, StringComparison.Ordinal);
        Assert.Contains("/openai/deployments/test-deployment/responses?", uri, StringComparison.Ordinal);
        Assert.StartsWith("https://test-resource.openai.azure.com/", uri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(ResponsesDefault + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Responses_pass_provider_and_request_headers()
    {
        var handler = AzureParity.Handler(AzureParity.ResponsesOk);
        var provider = HeaderProvider(handler);
        var call = AzureParity.Hello();
        call.Headers = RequestHeaders();
        await provider.Responses("test-deployment").DoGenerateAsync(call, CancellationToken.None);

        AssertApiKeyHeaders(handler);
    }

    [Fact]
    [UpstreamTest(ResponsesDefault + "should use tokenProvider for Microsoft Entra ID auth", Coverage = UpstreamCoverage.Covered)]
    public async Task Responses_use_a_bearer_token_instead_of_api_key()
    {
        var handler = AzureParity.Handler(AzureParity.ResponsesOk);
        var provider = AzureParity.Provider(handler, options =>
        {
            options.ApiKey = null;
            options.TokenProvider = _ => Task.FromResult("test-azure-ad-token");
        });
        await provider.Responses("test-deployment").DoGenerateAsync(AzureParity.Hello(), CancellationToken.None);

        Assert.Equal("Bearer test-azure-ad-token", handler.RequestHeaders["Authorization"]);
        Assert.False(handler.RequestHeaders.ContainsKey("api-key"));
        Assert.Contains("application/json", handler.RequestHeaders["Content-Type"], StringComparison.Ordinal);
        Assert.Contains("ai-sdk/azure/0.0.0-test", handler.RequestHeaders["User-Agent"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(ResponsesDefault + "should call tokenProvider for every request", Coverage = UpstreamCoverage.Covered)]
    public async Task Responses_call_the_token_provider_for_every_request()
    {
        var handler = AzureParity.Handler(AzureParity.ResponsesOk);
        var count = 0;
        var provider = AzureParity.Provider(handler, options =>
        {
            options.ApiKey = null;
            options.TokenProvider = _ =>
            {
                count++;
                return Task.FromResult("token-" + count.ToString());
            };
        });
        var model = provider.Responses("test-deployment");
        await model.DoGenerateAsync(AzureParity.Hello(), CancellationToken.None);
        Assert.Equal("Bearer token-1", handler.RequestHeaders["Authorization"]);
        await model.DoGenerateAsync(AzureParity.Hello(), CancellationToken.None);

        Assert.Equal(2, count);
        Assert.Equal("Bearer token-2", handler.RequestHeaders["Authorization"]);
    }

    [Fact]
    [UpstreamTest(ResponsesDefault + "should reject explicit apiKey with tokenProvider", Coverage = UpstreamCoverage.Covered)]
    public void Responses_reject_an_api_key_and_a_token_provider()
    {
        var handler = AzureParity.Handler(AzureParity.ResponsesOk);
        var exception = Assert.Throws<ArgumentException>(() => AzureParity.Provider(handler, options =>
        {
            options.TokenProvider = _ => Task.FromResult("test-azure-ad-token");
        }));

        Assert.Equal(
            "Both apiKey and tokenProvider were provided. Please use only one authentication method.",
            exception.Message);
    }

    [Fact]
    [UpstreamTest(ResponsesDefault + "should include explicit message item types for Foundry project endpoints", Coverage = UpstreamCoverage.Covered)]
    public async Task Foundry_project_endpoints_send_explicit_message_types()
    {
        var handler = AzureParity.Handler(AzureParity.ResponsesOk);
        var provider = AzureParity.Provider(handler, options =>
        {
            options.BaseUrl = "https://test-resource.services.ai.azure.com/api/projects/test-project/openai/v1";
        });
        var call = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage("You are concise."),
                new UserModelMessage("Say hi."),
                new AssistantModelMessage("Hi.", null, null),
                new UserModelMessage("Say it again."),
            },
        };
        await provider.Responses("test-deployment").DoGenerateAsync(call, CancellationToken.None);

        var input = JsonNode.Parse(handler.Body)!["input"];
        ParityAssert.JsonEqual(
            input,
            """
            [
              {"type":"message","role":"system","content":"You are concise."},
              {"type":"message","role":"user","content":[{"type":"input_text","text":"Say hi."}]},
              {"type":"message","role":"assistant","content":"Hi."},
              {"type":"message","role":"user","content":[{"type":"input_text","text":"Say it again."}]}
            ]
            """);
    }

    [Fact]
    [UpstreamTest(Chat + "should set the correct modified api version", Coverage = UpstreamCoverage.Covered)]
    public async Task Chat_uses_the_configured_api_version()
    {
        var handler = AzureParity.Handler(AzureParity.Ok);
        var provider = AzureParity.Provider(handler, options => options.ApiVersion = "2025-04-01-preview");
        await provider.Chat("test-deployment").DoGenerateAsync(AzureParity.Hello(), CancellationToken.None);

        var uri = handler.Uri!.ToString();
        Assert.Contains("api-version=2025-04-01-preview", uri, StringComparison.Ordinal);
        Assert.Contains("/openai/deployments/test-deployment/chat/completions?", uri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Chat + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Chat_passes_provider_and_request_headers()
    {
        var handler = AzureParity.Handler(AzureParity.Ok);
        var provider = HeaderProvider(handler);
        var call = AzureParity.Hello();
        call.Headers = RequestHeaders();
        await provider.Chat("test-deployment").DoGenerateAsync(call, CancellationToken.None);

        AssertApiKeyHeaders(handler);
    }

    [Fact]
    [UpstreamTest(Deepseek + "should map top-level reasoning to Azure DeepSeek reasoning effort", Coverage = UpstreamCoverage.Covered)]
    public async Task Deepseek_maps_top_level_reasoning_effort()
    {
        var handler = AzureParity.Handler(
            """
            {"choices":[{"message":{"role":"assistant","reasoning_content":"We need to invent a holiday.","content":"Absolutely!"},"finish_reason":"stop"}]}
            """);
        var provider = AzureParity.Provider(handler);
        var call = AzureParity.Hello();
        call.Reasoning = "high";
        var result = await provider.Deepseek("deepseek-v4-pro").DoGenerateAsync(call, CancellationToken.None);

        AzureParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "deepseek-v4-pro",
              "messages": [{"role":"user","content":"Hello"}],
              "reasoning_effort": "high"
            }
            """);
        Assert.IsType<GeneratedReasoning>(result.Content[0]);
        Assert.Equal("We need to invent a holiday.", ((GeneratedReasoning)result.Content[0]).Text);
        Assert.Equal("Absolutely!", ((GeneratedText)result.Content[1]).Text);
        Assert.False(JsonNode.Parse(handler.Body)!.AsObject().ContainsKey("stream"));
    }

    [Fact]
    [UpstreamTest(Deepseek + "should parse providerOptions from the azure namespace", Coverage = UpstreamCoverage.Covered)]
    public async Task Deepseek_prefers_azure_reasoning_effort()
    {
        var handler = AzureParity.Handler(AzureParity.Ok);
        var provider = AzureParity.Provider(handler);
        var call = AzureParity.Hello();
        call.Reasoning = "high";
        call.ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement>
        {
            ["azure"] = AzureParity.Element("{\"reasoningEffort\":\"max\"}"),
        };
        await provider.Deepseek("deepseek-v4-flash").DoGenerateAsync(call, CancellationToken.None);

        AzureParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "deepseek-v4-flash",
              "messages": [{"role":"user","content":"Hello"}],
              "reasoning_effort": "max"
            }
            """);
    }

    [Fact]
    [UpstreamTest(Deepseek + "should send a json_schema response format for structured output", Coverage = UpstreamCoverage.Covered)]
    public async Task Deepseek_sends_a_strict_json_schema()
    {
        var handler = AzureParity.Handler(AzureParity.Ok);
        var provider = AzureParity.Provider(handler);
        var call = AzureParity.Hello();
        call.Reasoning = "high";
        call.JsonSchema = AzureParity.Element(
            """
            {"type":"object","properties":{"sentiment":{"type":"string"}},"required":["sentiment"],"additionalProperties":false}
            """);
        await provider.Deepseek("deepseek-v4-flash").DoGenerateAsync(call, CancellationToken.None);

        AzureParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "deepseek-v4-flash",
              "messages": [{"role":"user","content":"Hello"}],
              "reasoning_effort": "high",
              "response_format": {
                "type": "json_schema",
                "json_schema": {
                  "name": "response",
                  "strict": true,
                  "schema": {
                    "type": "object",
                    "properties": {"sentiment": {"type": "string"}},
                    "required": ["sentiment"],
                    "additionalProperties": false
                  }
                }
              }
            }
            """);
    }

    [Fact]
    [UpstreamTest(Deepseek + "should stream reasoning content", Coverage = UpstreamCoverage.Covered)]
    public async Task Deepseek_streams_reasoning_before_text()
    {
        var handler = AzureParity.Handler(
            AzureParity.Ok,
            AzureParity.Sse(
                """{"id":"7334c29da064437e9d158710cdefbae6","model":"deepseek-v4-pro","created":0,"object":"chat.completion.chunk","choices":[{"index":0,"delta":{"role":"assistant","reasoning_content":"We"}}]}""",
                """{"choices":[{"index":0,"delta":{"reasoning_content":" need to"}}]}""",
                """{"choices":[{"index":0,"delta":{"content":"Absolutely!"},"finish_reason":"stop"}]}""",
                """{"object":"chat.completion.chunk","choices":[],"usage":{"prompt_tokens":19,"completion_tokens":1720,"total_tokens":1739,"prompt_tokens_details":null,"reasoning_tokens":0}}"""));
        var provider = AzureParity.Provider(handler);
        var call = AzureParity.Hello();
        call.Reasoning = "high";
        var model = (AzureChatLanguageModel)provider.Deepseek("deepseek-v4-pro");
        var parts = await AzureParity.Collect((await model.OpenStreamAsync(call, CancellationToken.None)).Parts);

        AzureParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "deepseek-v4-pro",
              "messages": [{"role":"user","content":"Hello"}],
              "reasoning_effort": "high",
              "stream": true,
              "stream_options": {"include_usage": true}
            }
            """);
        Assert.IsType<StreamStartStreamPart>(parts[0]);
        Assert.Empty(((StreamStartStreamPart)parts[0]).Warnings);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal("7334c29da064437e9d158710cdefbae6", metadata.Id);
        Assert.Equal("deepseek-v4-pro", metadata.ModelId);
        Assert.Equal(DateTimeOffset.UnixEpoch, metadata.Timestamp);
        Assert.Equal("reasoning-0", Assert.IsType<ReasoningStartStreamPart>(parts[2]).Id);
        Assert.Equal("We", Assert.IsType<ReasoningDeltaStreamPart>(parts[3]).Delta);
        Assert.Equal(" need to", Assert.IsType<ReasoningDeltaStreamPart>(parts[4]).Delta);
        var textStart = parts.FindIndex(part => part is TextStartStreamPart);
        var reasoningEnd = parts.FindIndex(part => part is ReasoningEndStreamPart);
        Assert.True(textStart >= 0 && textStart < reasoningEnd);
        Assert.Equal("txt-0", ((TextStartStreamPart)parts[textStart]).Id);
        Assert.Equal("reasoning-0", ((ReasoningEndStreamPart)parts[reasoningEnd]).Id);
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("stop", finish.RawFinishReason);
        Assert.Equal(19, finish.Usage.InputTokens);
        Assert.Equal(1720, finish.Usage.OutputTokens);
        Assert.Equal(1739, finish.Usage.TotalTokens);
        Assert.Equal(0, finish.Usage.CacheReadTokens);
        Assert.Equal(0, finish.Usage.ReasoningTokens);
        Assert.Equal(1720, finish.Usage.TextTokens);
        Assert.Equal(JsonValueKind.Null, finish.Usage.Raw!.Value.GetProperty("prompt_tokens_details").ValueKind);
        Assert.Equal(0, finish.Usage.Raw.Value.GetProperty("reasoning_tokens").GetInt32());
        var azure = finish.ProviderMetadata!.Value.GetProperty("azure");
        Assert.Equal(0, azure.GetProperty("choiceIndex").GetInt32());
        Assert.Equal("assistant", azure.GetProperty("messageRole").GetString());
        Assert.Equal("chat.completion.chunk", azure.GetProperty("responseObject").GetString());
    }

    [Fact]
    [UpstreamTest(Completion + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Completion_passes_provider_and_request_headers()
    {
        var handler = AzureParity.Handler("{\"choices\":[{\"text\":\"Hello World!\",\"finish_reason\":\"stop\"}]}");
        var provider = HeaderProvider(handler);
        var call = AzureParity.Hello();
        call.Headers = RequestHeaders();
        await provider.Completion("test-deployment").DoGenerateAsync(call, CancellationToken.None);

        AssertApiKeyHeaders(handler);
        Assert.Contains("/openai/deployments/test-deployment/completions?", handler.Uri!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Embedding + "::should expose the aggregate token limit", Coverage = UpstreamCoverage.Covered)]
    public void Embedding_exposes_the_aggregate_byte_limit()
    {
        var handler = AzureParity.Handler("{}");
        var model = AzureParity.Provider(handler).Embedding("my-embedding");

        Assert.Equal(300000, model.MaxInputBytesPerCall);
    }

    [Fact]
    [UpstreamTest(Embedding + " > doEmbed::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Embedding_passes_provider_and_request_headers()
    {
        var handler = AzureParity.Handler(
            """
            {"data":[{"embedding":[0.1,0.2]}],"usage":{"prompt_tokens":8,"total_tokens":8}}
            """);
        var provider = HeaderProvider(handler);
        await provider.Embedding("my-embedding").EmbedAsync(
            new[] { "sunny day at the beach", "rainy day in the city" },
            RequestHeaders(),
            CancellationToken.None);

        AssertApiKeyHeaders(handler);
        Assert.Contains("/openai/deployments/my-embedding/embeddings?", handler.Uri!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Image + "should set the correct modified api version", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_uses_the_configured_api_version()
    {
        var handler = AzureParity.Handler("{\"data\":[{\"b64_json\":\"base64-image-1\"}]}");
        var provider = AzureParity.Provider(handler, options => options.ApiVersion = "2025-04-01-preview");
        await provider.ImageModel("dalle-deployment").DoGenerateAsync(new ImageCallOptions("A cute baby sea otter") { Size = "1024x1024" }, CancellationToken.None);

        var uri = handler.Uri!.ToString();
        Assert.Contains("api-version=2025-04-01-preview", uri, StringComparison.Ordinal);
        Assert.Contains("/openai/deployments/dalle-deployment/images/generations?", uri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Image + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_passes_provider_and_request_headers()
    {
        var handler = AzureParity.Handler("{\"data\":[{\"b64_json\":\"base64-image-1\"}]}");
        var provider = HeaderProvider(handler);
        await provider.Image("dalle-deployment").GenerateAsync(
            new AzureImageCallOptions("A cute baby sea otter")
            {
                Size = "1024x1024",
                Headers = RequestHeaders(),
            },
            CancellationToken.None);

        AssertApiKeyHeaders(handler);
    }

    [Fact]
    [UpstreamTest(Image + "should extract the generated images", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_returns_the_b64_json_strings()
    {
        var handler = AzureParity.Handler(
            """
            {"data":[{"b64_json":"base64-image-1","revised_prompt":"A charming visual illustration"},{"b64_json":"base64-image-2"}]}
            """);
        var provider = AzureParity.Provider(handler);
        var result = await provider.Image("dalle-deployment").GenerateAsync(
            new AzureImageCallOptions("A cute baby sea otter") { Size = "1024x1024" },
            CancellationToken.None);

        Assert.Equal(new[] { "base64-image-1", "base64-image-2" }, result.Base64Images);
    }

    [Fact]
    [UpstreamTest(Image + "should send the correct request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_sends_the_generation_body()
    {
        var handler = AzureParity.Handler("{\"data\":[]}");
        var provider = AzureParity.Provider(handler);
        await provider.Image("dalle-deployment").GenerateAsync(
            new AzureImageCallOptions("A cute baby sea otter")
            {
                Count = 2,
                Size = "1024x1024",
                ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement>
                {
                    ["openai"] = AzureParity.Element("{\"style\":\"natural\"}"),
                },
            },
            CancellationToken.None);

        AzureParity.JsonEqual(
            handler.Body,
            """
            {
              "model": "dalle-deployment",
              "prompt": "A cute baby sea otter",
              "n": 2,
              "response_format": "b64_json",
              "size": "1024x1024",
              "style": "natural"
            }
            """);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::image > imageModel method::should create the same model as image method", Coverage = UpstreamCoverage.Covered)]
    public void Image_and_image_model_identify_the_same_deployment()
    {
        var handler = AzureParity.Handler("{}");
        var provider = AzureParity.Provider(handler);
        var image = provider.Image("dalle-deployment");
        var imageModel = provider.ImageModel("dalle-deployment");

        Assert.Equal(image.Provider, imageModel.Provider);
        Assert.Equal(image.ModelId, imageModel.ModelId);
        Assert.Equal("azure", image.Provider);
        Assert.Equal("dalle-deployment", image.ModelId);
        Assert.IsType<AzureImageModel>(imageModel);
    }

    private static AzureOpenAIProvider HeaderProvider(CaptureHandler handler)
    {
        return AzureParity.Provider(handler, options =>
        {
            options.Headers = new Dictionary<string, string?>
            {
                ["Custom-Provider-Header"] = "provider-header-value",
            };
        });
    }

    private static Dictionary<string, string?> RequestHeaders()
    {
        return new Dictionary<string, string?>
        {
            ["Custom-Request-Header"] = "request-header-value",
        };
    }

    private static void AssertApiKeyHeaders(CaptureHandler handler)
    {
        Assert.Equal("test-api-key", handler.RequestHeaders["api-key"]);
        Assert.Contains("application/json", handler.RequestHeaders["Content-Type"], StringComparison.Ordinal);
        Assert.Equal("provider-header-value", handler.RequestHeaders["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", handler.RequestHeaders["Custom-Request-Header"]);
        Assert.Contains("ai-sdk/azure/0.0.0-test", handler.RequestHeaders["User-Agent"], StringComparison.Ordinal);
    }
}
