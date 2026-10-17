// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using BedrockProvider = Vercel.AI.AmazonBedrock.AmazonBedrockAnthropicProvider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockAnthropicProvider;

public sealed class AmazonBedrockAnthropicProviderTests
{
    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should build correct URL for non-streaming requests", Coverage = UpstreamCoverage.Covered)]
    public void Builds_the_non_streaming_url()
    {
        var url = AmazonBedrockAnthropicRequests.BuildUrl("https://bedrock-runtime.us-east-1.amazonaws.com", "anthropic.claude-3-sonnet-20240229-v1:0", false);
        Assert.Equal("https://bedrock-runtime.us-east-1.amazonaws.com/model/anthropic.claude-3-sonnet-20240229-v1%3A0/invoke", url);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should build correct URL for streaming requests", Coverage = UpstreamCoverage.Covered)]
    public void Builds_the_streaming_url()
    {
        var url = AmazonBedrockAnthropicRequests.BuildUrl("https://bedrock-runtime.us-east-1.amazonaws.com", "anthropic.claude-3-sonnet-20240229-v1:0", true);
        Assert.Equal("https://bedrock-runtime.us-east-1.amazonaws.com/model/anthropic.claude-3-sonnet-20240229-v1%3A0/invoke-with-response-stream", url);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should transform request body to add anthropic_version and remove model", Coverage = UpstreamCoverage.Covered)]
    public void Adds_anthropic_version_and_removes_model()
    {
        var transformed = AmazonBedrockAnthropicRequests.Transform(Object("{\"model\":\"test-model-id\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"max_tokens\":1024}"));
        Assert.Null(transformed["model"]);
        Assert.Equal("bedrock-2023-05-31", transformed["anthropic_version"]!.GetValue<string>());
        Assert.Equal(1024, transformed["max_tokens"]!.GetValue<int>());
        Assert.Equal("Hello", transformed["messages"]![0]!["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should strip stream parameter from request body", Coverage = UpstreamCoverage.Covered)]
    public void Strips_the_stream_parameter()
    {
        var transformed = AmazonBedrockAnthropicRequests.Transform(Object("{\"model\":\"test-model-id\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"max_tokens\":1024,\"stream\":true}"));
        Assert.Null(transformed["stream"]);
        Assert.Equal("bedrock-2023-05-31", transformed["anthropic_version"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should strip disable_parallel_tool_use from tool_choice", Coverage = UpstreamCoverage.Covered)]
    public void Strips_disable_parallel_tool_use()
    {
        var transformed = AmazonBedrockAnthropicRequests.Transform(Object("{\"tool_choice\":{\"type\":\"auto\",\"disable_parallel_tool_use\":true}}"));
        Assert.Equal("auto", transformed["tool_choice"]!["type"]!.GetValue<string>());
        Assert.Null(transformed["tool_choice"]!["disable_parallel_tool_use"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should preserve tool_choice name when present", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_the_tool_choice_name()
    {
        var transformed = AmazonBedrockAnthropicRequests.Transform(Object("{\"tool_choice\":{\"type\":\"tool\",\"name\":\"my_tool\",\"disable_parallel_tool_use\":true}}"));
        Assert.Equal("tool", transformed["tool_choice"]!["type"]!.GetValue<string>());
        Assert.Equal("my_tool", transformed["tool_choice"]!["name"]!.GetValue<string>());
        Assert.Null(transformed["tool_choice"]!["disable_parallel_tool_use"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::resolves the Bedrock Runtime endpoint for an AWS ISO region", Coverage = UpstreamCoverage.Covered)]
    public void Resolves_the_iso_runtime_endpoint()
    {
        var provider = BedrockProvider.Create(new AmazonBedrockAnthropicOptions
        {
            Region = "us-iso-east-1",
            AccessKeyId = "test-key",
            SecretAccessKey = "test-secret",
        });
        Assert.Equal("https://bedrock-runtime.us-iso-east-1.c2s.ic.gov", provider.ResolveBaseUrl());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should have correct specificationVersion", Coverage = UpstreamCoverage.Covered)]
    public void Exposes_specification_version_v4()
    {
        var provider = BedrockProvider.Create(new AmazonBedrockAnthropicOptions
        {
            Region = "us-east-1",
            AccessKeyId = "test-key",
            SecretAccessKey = "test-secret",
        });
        Assert.Equal("v4", provider.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should provide languageModel as alias", Coverage = UpstreamCoverage.Covered)]
    public void Provides_a_language_model()
    {
        var provider = BedrockProvider.Create(new AmazonBedrockAnthropicOptions
        {
            Region = "us-east-1",
            AccessKeyId = "test-key",
            SecretAccessKey = "test-secret",
        });
        var model = provider.LanguageModel("test-model-id");
        Assert.Equal("bedrock.anthropic.messages", model.Provider);
        Assert.Equal("V4", model.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should throw NoSuchModelError for embeddingModel", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_embedding_models()
    {
        var provider = BedrockProvider.Create(new AmazonBedrockAnthropicOptions { Region = "us-east-1", AccessKeyId = "test-key", SecretAccessKey = "test-secret" });
        Assert.Throws<NoSuchModelError>(() => provider.EmbeddingModel("invalid-model-id"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should throw NoSuchModelError for imageModel", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_image_models()
    {
        var provider = BedrockProvider.Create(new AmazonBedrockAnthropicOptions { Region = "us-east-1", AccessKeyId = "test-key", SecretAccessKey = "test-secret" });
        Assert.Throws<NoSuchModelError>(() => provider.ImageModel("invalid-model-id"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should include anthropicTools", Coverage = UpstreamCoverage.Covered)]
    public void Includes_the_anthropic_tools()
    {
        Assert.Same(AnthropicProvider.Tools, BedrockProvider.Tools);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should throw an error when using new keyword", Coverage = UpstreamCoverage.Covered)]
    public void Throws_an_error_when_using_new_keyword()
    {
        var provider = BedrockProvider.Create(new AmazonBedrockAnthropicOptions { Region = "us-east-1", AccessKeyId = "test-key", SecretAccessKey = "test-secret" });
        var exception = Assert.Throws<InvalidOperationException>(() => provider.New("test-model-id"));
        Assert.Equal("The Bedrock Anthropic model function cannot be called with the new keyword.", exception.Message);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should create a language model with default settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Creates_a_language_model_with_default_settings()
    {
        var provider = BedrockProvider.Create(new AmazonBedrockAnthropicOptions { Region = "us-east-1", AccessKeyId = "test-key", SecretAccessKey = "test-secret" });
        const string modelId = "anthropic.claude-3-5-sonnet-20241022-v2:0";
        var model = Assert.IsType<AmazonBedrockAnthropicLanguageModel>(provider.LanguageModel(modelId));
        Assert.Equal(modelId, model.ModelId);

        var inner = model.CreateInnerProvider();
        Assert.Contains("bedrock-runtime", inner.Options.BaseUrl);
        Assert.Equal("bedrock.anthropic.messages", inner.Options.Name);
        Assert.Empty(inner.SupportedUrls);
        Assert.True(inner.SupportsNativeStructuredOutput);
        Assert.True(inner.SupportsStrictTools);
        Assert.EndsWith("/model/anthropic.claude-3-5-sonnet-20241022-v2%3A0/invoke-with-response-stream", inner.RequestUri(modelId, true).AbsoluteUri);
        Assert.Equal("bedrock-2023-05-31", inner.TransformRequestBody(new JsonObject(), new List<string>())["anthropic_version"]!.GetValue<string>());
        var headers = await inner.CreateHeadersAsync(new List<string>(), null, null, CancellationToken.None);
        Assert.Contains("ai-sdk-amazon-bedrock/", headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should pass custom baseURL without loading a region", Coverage = UpstreamCoverage.Covered)]
    public void Passes_a_custom_base_url_without_loading_a_region()
    {
        // An invalid region would throw if the region were used to build the host.
        var provider = BedrockProvider.Create(new AmazonBedrockAnthropicOptions { ApiKey = "test-api-key", Region = "not a region", BaseUrl = "https://custom-bedrock.amazonaws.com" });
        var model = Assert.IsType<AmazonBedrockAnthropicLanguageModel>(provider.LanguageModel("test-model-id"));
        Assert.Equal("https://custom-bedrock.amazonaws.com", model.CreateInnerProvider().Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::prefers the service-specific endpoint over the global endpoint without loading a region", Coverage = UpstreamCoverage.Covered)]
    public void Prefers_the_service_specific_endpoint_over_the_global_endpoint()
    {
        var previousService = Environment.GetEnvironmentVariable("AWS_ENDPOINT_URL_BEDROCK_RUNTIME");
        var previousGlobal = Environment.GetEnvironmentVariable("AWS_ENDPOINT_URL");
        Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_BEDROCK_RUNTIME", "https://runtime.example.com/");
        Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL", "https://global.example.com");
        try
        {
            // An invalid region would throw if the region were used to build the host.
            var provider = BedrockProvider.Create(new AmazonBedrockAnthropicOptions { ApiKey = "test-api-key", Region = "not a region" });
            var model = Assert.IsType<AmazonBedrockAnthropicLanguageModel>(provider.LanguageModel("test-model-id"));
            Assert.Equal("https://runtime.example.com", model.CreateInnerProvider().Options.BaseUrl);
        }
        finally
        {
            Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_BEDROCK_RUNTIME", previousService);
            Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL", previousGlobal);
        }
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should map old tool versions to Bedrock-supported versions", Coverage = UpstreamCoverage.Covered)]
    public void Should_map_old_tool_versions_to_Bedrock_supported_versions()
    {
        var transformed = Transform(Object("{\"model\":\"test-model-id\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"max_tokens\":1024,\"tools\":[{\"type\":\"bash_20241022\",\"name\":\"bash\"},{\"type\":\"text_editor_20241022\",\"name\":\"str_replace_editor\"},{\"type\":\"computer_20241022\",\"name\":\"computer\"}]}"), Array.Empty<string>());

        Assert.Equal(
            "[{\"type\":\"bash_20250124\",\"name\":\"bash\"},{\"type\":\"text_editor_20250728\",\"name\":\"str_replace_based_edit_tool\"},{\"type\":\"computer_20250124\",\"name\":\"computer\"}]",
            transformed["tools"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should add anthropic_beta when computer use tools are detected", Coverage = UpstreamCoverage.Covered)]
    public void Should_add_anthropic_beta_when_computer_use_tools_are_detected()
    {
        var transformed = Transform(Object("{\"model\":\"test-model-id\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"max_tokens\":1024,\"tools\":[{\"type\":\"bash_20250124\",\"name\":\"bash\"}]}"), Array.Empty<string>());

        Assert.Contains("computer-use-2025-01-24", transformed["anthropic_beta"]!.AsArray().Select(beta => beta!.GetValue<string>()));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should include betas passed to transformRequestBody in anthropic_beta body field", Coverage = UpstreamCoverage.Covered)]
    public void Should_include_betas_passed_to_transformRequestBody_in_anthropic_beta_body_field()
    {
        var transformed = Transform(Object("{\"model\":\"test-model-id\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"max_tokens\":1024}"), Array.Empty<string>());

        Assert.Equal("bedrock-2023-05-31", transformed["anthropic_version"]!.GetValue<string>());
        Assert.False(transformed.ContainsKey("anthropic_beta"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should not add anthropic_beta when no computer use tools are present", Coverage = UpstreamCoverage.Covered)]
    public void Should_not_add_anthropic_beta_when_no_computer_use_tools_are_present()
    {
        var transformed = Transform(Object("{\"model\":\"test-model-id\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"max_tokens\":1024,\"tools\":[{\"type\":\"function\",\"name\":\"get_weather\",\"input_schema\":{\"type\":\"object\",\"properties\":{}}}]}"), Array.Empty<string>());

        Assert.Null(transformed["anthropic_beta"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should rename thinking block_binding prefix_mismatch_behavior to mismatch_behavior", Coverage = UpstreamCoverage.Covered)]
    public void Should_rename_thinking_block_binding_prefix_mismatch_behavior_to_mismatch_behavior()
    {
        var transformed = Transform(Object("{\"model\":\"test-model-id\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"max_tokens\":1024,\"thinking\":{\"type\":\"adaptive\",\"display\":\"summarized\",\"block_binding\":{\"prefix_mismatch_behavior\":\"drop_block\"}}}"), new[] { "thinking-binding-controls-2026-08-01" });

        Assert.Equal(
            "{\"type\":\"adaptive\",\"display\":\"summarized\",\"block_binding\":{\"mismatch_behavior\":\"drop_block\"}}",
            transformed["thinking"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should leave thinking unchanged when it has no block_binding", Coverage = UpstreamCoverage.Covered)]
    public void Should_leave_thinking_unchanged_when_it_has_no_block_binding()
    {
        var transformed = Transform(Object("{\"model\":\"test-model-id\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"max_tokens\":1024,\"thinking\":{\"type\":\"enabled\",\"budget_tokens\":2000}}"), Array.Empty<string>());

        Assert.Equal("{\"type\":\"enabled\",\"budget_tokens\":2000}", transformed["thinking"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should translate eager_input_streaming on tools into the fine-grained-tool-streaming beta", Coverage = UpstreamCoverage.Covered)]
    public void Should_translate_eager_input_streaming_on_tools_into_the_fine_grained_tool_streaming_beta()
    {
        var transformed = Transform(Object("{\"model\":\"test-model-id\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"max_tokens\":1024,\"tools\":[{\"name\":\"get_weather\",\"input_schema\":{\"type\":\"object\",\"properties\":{}},\"eager_input_streaming\":true},{\"name\":\"get_time\",\"input_schema\":{\"type\":\"object\",\"properties\":{}}}]}"), Array.Empty<string>());

        Assert.Equal(
            "[{\"name\":\"get_weather\",\"input_schema\":{\"type\":\"object\",\"properties\":{}}},{\"name\":\"get_time\",\"input_schema\":{\"type\":\"object\",\"properties\":{}}}]",
            transformed["tools"]!.ToJsonString());
        Assert.Equal("[\"fine-grained-tool-streaming-2025-05-14\"]", transformed["anthropic_beta"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should not add the fine-grained-tool-streaming beta when no tool has eager_input_streaming", Coverage = UpstreamCoverage.Covered)]
    public void Should_not_add_the_fine_grained_tool_streaming_beta_when_no_tool_has_eager_input_streaming()
    {
        var transformed = Transform(Object("{\"model\":\"test-model-id\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"max_tokens\":1024,\"tools\":[{\"name\":\"get_weather\",\"input_schema\":{\"type\":\"object\",\"properties\":{}}}]}"), Array.Empty<string>());

        Assert.Null(transformed["anthropic_beta"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should strip eager_input_streaming from tools that also get version remapped", Coverage = UpstreamCoverage.Covered)]
    public void Should_strip_eager_input_streaming_from_tools_that_also_get_version_remapped()
    {
        var transformed = Transform(Object("{\"model\":\"test-model-id\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"max_tokens\":1024,\"tools\":[{\"name\":\"get_weather\",\"input_schema\":{\"type\":\"object\",\"properties\":{}},\"eager_input_streaming\":true},{\"type\":\"bash_20241022\",\"name\":\"bash\"}]}"), Array.Empty<string>());

        Assert.Equal(
            "[{\"name\":\"get_weather\",\"input_schema\":{\"type\":\"object\",\"properties\":{}}},{\"type\":\"bash_20250124\",\"name\":\"bash\"}]",
            transformed["tools"]!.ToJsonString());
        var betas = transformed["anthropic_beta"]!.AsArray().Select(beta => beta!.GetValue<string>()).ToArray();
        Assert.Contains("fine-grained-tool-streaming-2025-05-14", betas);
        Assert.Contains("computer-use-2025-01-24", betas);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should add tool-search-tool beta when tool search tools are present", Coverage = UpstreamCoverage.Covered)]
    public void Should_add_tool_search_tool_beta_when_tool_search_tools_are_present()
    {
        var transformed = Transform(Object("{\"model\":\"test-model-id\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello\"}],\"max_tokens\":1024,\"tools\":[{\"type\":\"tool_search_tool_regex_20251119\",\"name\":\"tool_search\"},{\"type\":\"tool_search_tool_bm25_20251119\",\"name\":\"tool_search_bm25\"}]}"), Array.Empty<string>());

        Assert.Contains("tool-search-tool-2025-10-19", transformed["anthropic_beta"]!.AsArray().Select(beta => beta!.GetValue<string>()));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should handle models with us. prefix for inference profiles", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_models_with_us_prefix_for_inference_profiles()
    {
        var url = AmazonBedrockAnthropicRequests.BuildUrl("https://bedrock-runtime.us-east-1.amazonaws.com", "us.anthropic.claude-3-5-sonnet-20240620-v1:0", false);

        Assert.Equal("https://bedrock-runtime.us-east-1.amazonaws.com/model/us.anthropic.claude-3-5-sonnet-20240620-v1%3A0/invoke", url);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should not support URL sources to force base64 conversion", Coverage = UpstreamCoverage.Covered)]
    public void Should_not_support_URL_sources_to_force_base64_conversion()
    {
        var provider = new AmazonBedrockAnthropicInnerProvider(new HttpClient(), "https://bedrock-runtime.us-east-1.amazonaws.com", "test-model-id", new Dictionary<string, string?>());

        Assert.Empty(provider.SupportedUrls);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-provider.test.ts::amazon-bedrock-anthropic-provider::should pass custom headers wrapped in a function with user-agent suffix", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_custom_headers_wrapped_in_a_function_with_user_agent_suffix()
    {
        var handler = new UpstreamRecordingHandler("{\"content\":[],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}");
        var options = new AmazonBedrockAnthropicOptions { Region = "us-east-1", AccessKeyId = "test-key", SecretAccessKey = "test-secret" };
        options.Headers["Custom-Header"] = "custom-value";
        var provider = BedrockProvider.Create(options, handler);

        await provider.LanguageModel("test-model-id").DoGenerateAsync(
            new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("Hello") } },
            CancellationToken.None);

        Assert.Equal("custom-value", handler.Headers["Custom-Header"]);
        Assert.Contains("ai-sdk-amazon-bedrock/", handler.Headers["User-Agent"]);
    }

    private static JsonObject Transform(JsonObject body, IReadOnlyList<string> betas)
    {
        return AmazonBedrockAnthropicRequests.Transform(body, betas);
    }

    private static JsonObject Object(string json)
    {
        return (JsonObject)JsonNode.Parse(json)!;
    }
}
