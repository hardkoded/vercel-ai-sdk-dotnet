// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Vercel.AI.GmiCloud;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Parity.Media;

namespace Vercel.AI.Tests;

public sealed class GmiCloudTests
{
    private const string MaxTokensMessage = "The request is invalid: Invalid max_tokens value, the valid range of max_tokens is [1, 393216]. Please check the request body, required fields, and request format.";

    private const string ThinkingMessage = "The request is invalid: Thinking mode does not support this tool_choice. Please check the request body, required fields, and request format.";

    private const string ImageMessage = "The request is invalid: Failed to deserialize the JSON body into the target type: messages[0]: unknown variant `image_url`, expected `text` at line 1 column 265. Please check the request body, required fields, and request format.";

    [Fact]
    [UpstreamTest(
        "packages/gmicloud/src/gmicloud-chat-language-model.test.ts::GmicloudChatLanguageModel error handling::surfaces the engine diagnostic from error.details on a 400",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Surfaces_the_engine_diagnostic_on_a_400()
    {
        var handler = new MediaHandler();
        handler.Text("https://api.gmi-serving.com/v1/chat/completions", Wrap("Backend request failed with status 400", MaxTokensMessage), HttpStatusCode.BadRequest, "application/json");
        var model = GmiCloudProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-key" }, handler)
            .LanguageModel("deepseek-ai/DeepSeek-V4-Flash-0731");

        var error = await Assert.ThrowsAsync<BadRequestException>(() => model.DoGenerateAsync(Prompt(), CancellationToken.None));

        Assert.Equal(400, error.StatusCode);
        Assert.Equal(MaxTokensMessage, error.Message);
        Assert.Contains("/chat/completions", handler.Calls[0].Url, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/gmicloud/src/gmicloud-error.test.ts::gmicloudErrorStructure::unwraps the engine diagnostic from error.details (max_tokens)",
        Coverage = UpstreamCoverage.Covered)]
    public void Unwraps_a_max_tokens_diagnostic()
    {
        Assert.Equal(MaxTokensMessage, Message(Wrap("Backend request failed with status 400", MaxTokensMessage)));
    }

    [Fact]
    [UpstreamTest(
        "packages/gmicloud/src/gmicloud-error.test.ts::gmicloudErrorStructure::unwraps the engine diagnostic from error.details (thinking tool_choice)",
        Coverage = UpstreamCoverage.Covered)]
    public void Unwraps_a_thinking_tool_choice_diagnostic()
    {
        Assert.Equal(ThinkingMessage, Message(Wrap("Backend request failed with status 400", ThinkingMessage)));
    }

    [Fact]
    [UpstreamTest(
        "packages/gmicloud/src/gmicloud-error.test.ts::gmicloudErrorStructure::unwraps the engine diagnostic from error.details (image input)",
        Coverage = UpstreamCoverage.Covered)]
    public void Unwraps_an_image_input_diagnostic()
    {
        Assert.Equal(ImageMessage, Message(Wrap("Backend request failed with status 400", ImageMessage)));
    }

    [Fact]
    [UpstreamTest(
        "packages/gmicloud/src/gmicloud-error.test.ts::gmicloudErrorStructure::falls back to the outer message when details is absent",
        Coverage = UpstreamCoverage.Covered)]
    public void Falls_back_when_details_is_absent()
    {
        Assert.True(GmiCloudErrors.TryParse("{\"error\":{\"message\":\"Backend request failed with status 400\"}}", out var error));
        Assert.Equal("Backend request failed with status 400", GmiCloudErrors.ToMessage(error!));
    }

    [Fact]
    [UpstreamTest(
        "packages/gmicloud/src/gmicloud-error.test.ts::gmicloudErrorStructure::falls back to the outer message when details is not JSON",
        Coverage = UpstreamCoverage.Covered)]
    public void Falls_back_when_details_is_not_json()
    {
        Assert.True(GmiCloudErrors.TryParse("{\"error\":{\"message\":\"banner\",\"details\":\"<html>nginx</html>\"}}", out var error));
        Assert.Equal("banner", GmiCloudErrors.ToMessage(error!));
    }

    [Fact]
    [UpstreamTest(
        "packages/gmicloud/src/gmicloud-error.test.ts::gmicloudErrorStructure::falls back to the outer message when details has no inner message",
        Coverage = UpstreamCoverage.Covered)]
    public void Falls_back_when_details_has_no_inner_message()
    {
        Assert.True(GmiCloudErrors.TryParse("{\"error\":{\"message\":\"banner\",\"details\":\"{\\\"error\\\":{\\\"code\\\":\\\"500\\\"}}\"}}", out var error));
        Assert.Equal("banner", GmiCloudErrors.ToMessage(error!));
    }

    [Fact]
    [UpstreamTest(
        "packages/gmicloud/src/gmicloud-error.test.ts::gmicloudErrorStructure::rejects GMI’s plain-text 404 body, deferring to status text",
        Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_plain_text_404_body()
    {
        Assert.False(GmiCloudErrors.TryParse("No matching target server found for model foo", out _));
    }

    [Fact]
    [UpstreamTest(
        "packages/gmicloud/src/gmicloud-provider.test.ts::createGmicloud::defaults to the GMI Cloud endpoint and GMI_CLOUD_APIKEY",
        Coverage = UpstreamCoverage.Covered)]
    public void Defaults_to_the_gmi_cloud_endpoint_and_api_key_variable()
    {
        Assert.Equal("GMI_CLOUD_APIKEY", GmiCloudProvider.ApiKeyVariable);
        var previous = Environment.GetEnvironmentVariable(GmiCloudProvider.ApiKeyVariable);
        Environment.SetEnvironmentVariable(GmiCloudProvider.ApiKeyVariable, "mock-api-key");
        try
        {
            var provider = GmiCloudProvider.Create();
            var model = Assert.IsType<GmiCloudChatLanguageModel>(provider.LanguageModel("deepseek-ai/DeepSeek-V4-Flash-0731"));
            var headers = provider.CreateHeaders();

            Assert.Equal("gmicloud.chat", model.Provider);
            Assert.Equal("https://api.gmi-serving.com/v1/chat/completions", provider.ChatUri(model.ModelId).AbsoluteUri);
            Assert.Equal("Bearer mock-api-key", headers["Authorization"]);
            Assert.Equal("ai-sdk/gmicloud/0.0.0-test", headers["User-Agent"]);
        }
        finally
        {
            Environment.SetEnvironmentVariable(GmiCloudProvider.ApiKeyVariable, previous);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/gmicloud/src/gmicloud-provider.test.ts::createGmicloud::attaches the gmicloud error structure and includeUsage",
        Coverage = UpstreamCoverage.Covered)]
    public void Attaches_the_error_structure_and_include_usage()
    {
        var provider = GmiCloudProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-key" });
        var model = Assert.IsType<GmiCloudChatLanguageModel>(provider.LanguageModel("deepseek-ai/DeepSeek-V4-Flash-0731"));

        Assert.True(model.IncludeUsage);
        Assert.True(GmiCloudErrors.IncludeUsage);
        Assert.Equal(MaxTokensMessage, Message(Wrap("Backend request failed with status 400", MaxTokensMessage)));
        Assert.Equal(MaxTokensMessage, MediaJson.Parse(GmiCloudErrors.Rewrite(Wrap("Backend request failed with status 400", MaxTokensMessage)))["error"]!["message"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/gmicloud/src/gmicloud-provider.test.ts::createGmicloud::respects a custom baseURL",
        Coverage = UpstreamCoverage.Covered)]
    public void Respects_a_custom_base_url()
    {
        var provider = GmiCloudProvider.Create(new OpenAICompatibleOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://example.com/gmi",
        });

        Assert.Equal("https://example.com/gmi/chat/completions", provider.ChatUri("model").AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(
        "packages/gmicloud/src/gmicloud-provider.test.ts::createGmicloud::throws NoSuchModelError for embedding and image models",
        Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_embedding_and_image_models()
    {
        var provider = GmiCloudProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-key" });
        var embedding = Assert.Throws<AiSdkException>(() => provider.EmbeddingModel("model"));
        var image = Assert.Throws<AiSdkException>(() => provider.ImageModel("model"));

        Assert.Contains("embeddingModel", embedding.Message, StringComparison.Ordinal);
        Assert.Contains("imageModel", image.Message, StringComparison.Ordinal);
    }

    private static LanguageModelCallOptions Prompt()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("hi") },
            MaxOutputTokens = 999999999,
        };
    }

    private static string Message(string json)
    {
        Assert.True(GmiCloudErrors.TryParse(json, out var error));
        return GmiCloudErrors.ToMessage(error!);
    }

    private static string Wrap(string outer, string inner)
    {
        var details = new JsonObject
        {
            ["error"] = new JsonObject { ["message"] = inner },
        };
        var body = new JsonObject
        {
            ["error"] = new JsonObject
            {
                ["message"] = outer,
                ["details"] = details.ToJsonString(),
            },
        };
        return body.ToJsonString();
    }
}
