// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Vertex, Anthropic, MaaS, and xAI request targets.</summary>
public sealed class GoogleVertexUpstreamTests
{
    private const string AnthropicTests = "packages/google-vertex/src/anthropic/google-vertex-anthropic-provider.test.ts::google-vertex-anthropic-provider::";
    private const string AnthropicNodeTests = "packages/google-vertex/src/anthropic/google-vertex-anthropic-provider-node.test.ts::google-vertex-anthropic-provider-node::";
    private const string AnthropicResponse = "{\"type\":\"message\",\"id\":\"msg_1\",\"content\":[{\"type\":\"text\",\"text\":\"ok\"}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}";

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use correct URL for global region", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_global_Vertex_host()
    {
        Assert.Equal("https://aiplatform.googleapis.com/v1beta1/projects/test-project/locations/global/publishers/google", Upstream("test-project", "global").Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use correct URL for global region with embedding model", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_global_host_for_an_embedding_model()
    {
        var provider = Upstream("test-project", "global");
        Assert.Equal("google.vertex.embedding", provider.EmbeddingModel("text-embedding-005").Provider);
        Assert.Equal("https://aiplatform.googleapis.com/v1beta1/projects/test-project/locations/global/publishers/google", provider.Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use correct URL for global region with image model", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_global_host_for_an_image_model()
    {
        var provider = Upstream("test-project", "global");
        Assert.Equal("google.vertex.image", provider.ImageModel("gemini-2.5-flash-image").Provider);
        Assert.Equal("https://aiplatform.googleapis.com/v1beta1/projects/test-project/locations/global/publishers/google", provider.Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use region-prefixed URL for non-global regions", Coverage = UpstreamCoverage.Covered)]
    public void Prefixes_the_host_with_a_regional_location()
    {
        Assert.Equal("https://us-central1-aiplatform.googleapis.com/v1beta1/projects/test-project/locations/us-central1/publishers/google", Upstream("test-project", "us-central1").Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use multi-region REP URL for us location", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_us_multi_region_host()
    {
        Assert.Equal("https://aiplatform.us.rep.googleapis.com/v1beta1/projects/test-project/locations/us/publishers/google", Upstream("test-project", "us").Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use multi-region REP URL for eu location", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_eu_multi_region_host()
    {
        Assert.Equal("https://aiplatform.eu.rep.googleapis.com/v1beta1/projects/test-project/locations/eu/publishers/google", Upstream("test-project", "eu").Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use express mode base URL when apiKey is provided", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_express_mode_base_url_for_an_api_key()
    {
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "test-project", Region = "us-central1", ApiKey = "test-api-key", UpstreamRoutes = true }, new RecordingHandler());
        Assert.Equal(GoogleVertexEndpoints.ExpressModeBaseUrl, provider.Options.BaseUrl);
        Assert.False(provider.Options.UseBearerToken);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use custom baseURL when provided", Coverage = UpstreamCoverage.Covered)]
    public void Trims_a_custom_Vertex_base_url()
    {
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "p", Region = "us-central1", BaseUrl = "https://custom.example.com/", ApiKey = "k" }, new RecordingHandler());
        Assert.Equal("https://custom.example.com", provider.Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should leave base model ids and the publishers/google base URL unchanged", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_publishers_google_for_a_publisher_model()
    {
        Assert.EndsWith("/publishers/google", Upstream("test-project", "test-location").Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use a tuned model endpoints/ id with no publishers/google suffix", Coverage = UpstreamCoverage.Covered)]
    public async Task Strips_publishers_google_from_a_tuned_model_request()
    {
        var handler = new RecordingHandler();
        var previous = Environment.GetEnvironmentVariable("GOOGLE_VERTEX_API_KEY");
        Environment.SetEnvironmentVariable("GOOGLE_VERTEX_API_KEY", "test-api-key");
        try
        {
            var provider = Upstream("test-project", "test-location", handler);
            await provider.LanguageModel("endpoints/1234567890").DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None).ConfigureAwait(false);
            Assert.Contains("/locations/test-location/endpoints/1234567890:generateContent", handler.Uris[0]);
            Assert.DoesNotContain("publishers/google", handler.Uris[0]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GOOGLE_VERTEX_API_KEY", previous);
        }
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should respect a custom baseURL for tuned model endpoints/ ids", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_a_custom_base_url_for_a_tuned_model()
    {
        var handler = new RecordingHandler();
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "p", Region = "us-central1", BaseUrl = "https://custom-endpoint.example.com", ApiKey = "k" }, handler);
        await provider.LanguageModel("endpoints/1234567890").DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None).ConfigureAwait(false);
        Assert.StartsWith("https://custom-endpoint.example.com/endpoints/1234567890:generateContent", handler.Uris[0]);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should reject Express Mode for tuned models", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_express_mode_for_tuned_models()
    {
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "p", Region = "us-central1", ApiKey = "test-api-key", UpstreamRoutes = true }, new RecordingHandler());
        var error = Assert.Throws<InvalidOperationException>(() => provider.LanguageModel("endpoints/1234567890"));
        Assert.Equal("Google Vertex tuned models do not support Express Mode API keys. Use standard Google Cloud credentials instead.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should throw for interactions models when an Express Mode API key is set", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_express_mode_for_interactions()
    {
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "p", Region = "us-central1", ApiKey = "test-api-key", UpstreamRoutes = true }, new RecordingHandler());
        var error = Assert.Throws<InvalidOperationException>(() => provider.Interactions("gemini-omni-flash-preview"));
        Assert.Contains("do not support Express Mode API keys", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should create an interactions model targeting the location-scoped interactions resource", Coverage = UpstreamCoverage.Covered)]
    public async Task Targets_the_location_scoped_interactions_resource()
    {
        var handler = new RecordingHandler { ResponseText = "{\"status\":\"completed\"}" };
        var previous = Environment.GetEnvironmentVariable("GOOGLE_VERTEX_API_KEY");
        Environment.SetEnvironmentVariable("GOOGLE_VERTEX_API_KEY", "test-api-key");
        try
        {
            var provider = Upstream("test-project", "test-location", handler);
            var model = provider.Interactions("gemini-omni-flash-preview");
            Assert.Equal("google.vertex.interactions", model.Provider);
            await model.DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None).ConfigureAwait(false);
            Assert.Equal("https://test-location-aiplatform.googleapis.com/v1beta1/projects/test-project/locations/test-location/interactions", handler.Uris[0]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GOOGLE_VERTEX_API_KEY", previous);
        }
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should create an embedding model with correct settings", Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_Vertex_embedding_model()
    {
        var provider = Upstream("test-project", "test-location");
        Assert.Equal("google.vertex.embedding", provider.EmbeddingModel("test-embedding-model").Provider);
        Assert.Equal("https://test-location-aiplatform.googleapis.com/v1beta1/projects/test-project/locations/test-location/publishers/google", provider.Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should create a speech model with correct settings", Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_Vertex_Gemini_speech_model()
    {
        var provider = Upstream("test-project", "test-location");
        var model = Assert.IsType<GoogleSpeechModel>(provider.SpeechModel("gemini-2.5-flash-tts"));
        Assert.Equal("google.vertex.speech", model.Provider);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should create a speech model via speechModel()", Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_Vertex_speech_model_from_SpeechModel()
    {
        Assert.Equal("google.vertex.speech", Upstream("test-project", "test-location").SpeechModel("gemini-2.5-pro-tts").Provider);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should route chirp speech models to the Cloud Text-to-Speech model", Coverage = UpstreamCoverage.Covered)]
    public void Routes_Chirp_models_to_Cloud_Text_to_Speech()
    {
        var model = Assert.IsType<GoogleVertexCloudSpeechModel>(Upstream("test-project", "test-location").SpeechModel("chirp-3-hd"));
        Assert.Equal("google.vertex.speech", model.Provider);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should throw for chirp speech models when an Express Mode API key is set", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_express_mode_for_Chirp_speech()
    {
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "p", Region = "us-central1", ApiKey = "test-api-key", UpstreamRoutes = true }, new RecordingHandler());
        var error = Assert.Throws<InvalidOperationException>(() => provider.SpeechModel("chirp-3-hd"));
        Assert.Equal("Google Vertex Chirp speech models do not support Express Mode API keys. Use standard Google Cloud credentials instead.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should keep routing gemini speech models to the Gemini speech model", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_Gemini_speech_on_the_Gemini_speech_model()
    {
        Assert.IsType<GoogleSpeechModel>(Upstream("test-project", "test-location").SpeechModel("gemini-2.5-flash-tts"));
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should create a transcription model with correct settings", Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_Vertex_speech_transcription_model()
    {
        var model = Assert.IsType<GoogleVertexSpeechTranscriptionModel>(Upstream("test-project", "us-central1").TranscriptionModel("chirp_2"));
        Assert.Equal("google.vertex.transcription", model.Provider);
        Assert.Equal("us-central1", Upstream("test-project", "us-central1").Vertex.Region);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should create a transcription model via transcriptionModel()", Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_Vertex_transcription_model_from_TranscriptionModel()
    {
        Assert.Equal("google.vertex.transcription", Upstream("test-project", "us-central1").TranscriptionModel("chirp_3").Provider);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should reject Express Mode for transcription models", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_express_mode_for_transcription()
    {
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "p", Region = "us-central1", ApiKey = "test-api-key", UpstreamRoutes = true }, new RecordingHandler());
        var error = Assert.Throws<InvalidOperationException>(() => provider.TranscriptionModel("chirp_3"));
        Assert.Equal("Google Vertex transcription models do not support Express Mode API keys. Use standard Google Cloud credentials instead.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should create an image model with default settings", Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_Vertex_image_model()
    {
        var model = Assert.IsType<GoogleImageModel>(Upstream("test-project", "test-location").ImageModel("gemini-2.5-flash-image"));
        Assert.Equal("google.vertex.image", model.Provider);
    }

    [Fact]
    [UpstreamTest(AnthropicTests + "should create a language model with default settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_Anthropic_requests_to_rawPredict_without_native_structured_output_or_strict_tools()
    {
        var handler = new RecordingHandler { ResponseText = AnthropicResponse };
        var provider = GoogleVertexAnthropicProvider.Create(new GoogleVertexAnthropicOptions { Project = "test-project", Location = "test-location" }, handler);
        var model = Assert.IsType<AnthropicLanguageModel>(provider.LanguageModel("claude-sonnet-4-5@20250929"));
        Assert.Equal("googleVertex.anthropic.messages", model.Provider);

        var options = GoogleUpstream.Hello();
        options.JsonSchema = GoogleUpstream.Element("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}}");
        options.Tools = new[] { GoogleUpstream.Function("lookup", "{\"type\":\"object\"}", strict: true) };
        await model.DoGenerateAsync(options, CancellationToken.None).ConfigureAwait(false);

        Assert.Equal("https://test-location-aiplatform.googleapis.com/v1/projects/test-project/locations/test-location/publishers/anthropic/models/claude-sonnet-4-5@20250929:rawPredict", handler.Uris[0]);
        Assert.Equal(
            "https://test-location-aiplatform.googleapis.com/v1/projects/test-project/locations/test-location/publishers/anthropic/models/claude-sonnet-4-5@20250929:streamRawPredict",
            provider.PredictUrl("claude-sonnet-4-5@20250929", streaming: true));
        var body = JsonNode.Parse(handler.Body)!.AsObject();
        Assert.False(body.ContainsKey("model"));
        Assert.Equal("vertex-2023-10-16", body["anthropic_version"]!.GetValue<string>());
        Assert.False(body.ContainsKey("output_config"));
        var tools = body["tools"]!.AsArray();
        Assert.Contains(tools, tool => tool!["name"]!.GetValue<string>() == "json");
        Assert.All(tools, tool => Assert.False(tool!.AsObject().ContainsKey("strict")));
    }

    [Fact]
    [UpstreamTest(AnthropicTests + "should throw an error when using new keyword", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_the_new_keyword_for_Anthropic_models()
    {
        var error = Assert.Throws<InvalidOperationException>(() => AnthropicOnVertex("test-project", "global").New("test-model-id"));
        Assert.Equal("The Anthropic model function cannot be called with the new keyword.", error.Message);
    }

    [Fact]
    [UpstreamTest(AnthropicTests + "should use correct URL for global location", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_global_Anthropic_publisher_url()
    {
        Assert.Equal("https://aiplatform.googleapis.com/v1/projects/test-project/locations/global/publishers/anthropic/models", AnthropicOnVertex("test-project", "global").BaseUrl);
    }

    [Fact]
    [UpstreamTest(AnthropicTests + "should use region-prefixed URL for non-global locations", Coverage = UpstreamCoverage.Covered)]
    public void Prefixes_the_Anthropic_host_with_the_region()
    {
        Assert.Equal("https://us-central1-aiplatform.googleapis.com/v1/projects/test-project/locations/us-central1/publishers/anthropic/models", AnthropicOnVertex("test-project", "us-central1").BaseUrl);
    }

    [Fact]
    [UpstreamTest(AnthropicTests + "should use multi-region URL for eu location", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_eu_Anthropic_host()
    {
        Assert.Equal("https://aiplatform.eu.rep.googleapis.com/v1/projects/test-project/locations/eu/publishers/anthropic/models", AnthropicOnVertex("test-project", "eu").BaseUrl);
    }

    [Fact]
    [UpstreamTest(AnthropicTests + "should use multi-region URL for us location", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_us_Anthropic_host()
    {
        Assert.Equal("https://aiplatform.us.rep.googleapis.com/v1/projects/test-project/locations/us/publishers/anthropic/models", AnthropicOnVertex("test-project", "us").BaseUrl);
    }

    [Fact]
    [UpstreamTest(AnthropicTests + "should pass baseURL to the model when created", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_a_custom_Anthropic_base_url()
    {
        var handler = new RecordingHandler { ResponseText = AnthropicResponse };
        var provider = GoogleVertexAnthropicProvider.Create(new GoogleVertexAnthropicOptions { Project = "p", Location = "global", BaseUrl = "https://custom-url.com" }, handler);
        await provider.LanguageModel("test-model-id").DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("https://custom-url.com/test-model-id:rawPredict", handler.Uris[0]);
    }

    [Fact]
    [UpstreamTest(AnthropicTests + "should throw NoSuchModelError for textEmbeddingModel", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_Anthropic_embedding_models()
    {
        var error = Assert.Throws<AiSdkException>(() => AnthropicOnVertex("p", "global").EmbeddingModel("text"));
        Assert.Contains("google.vertex.anthropic", error.Message);
    }

    [Fact]
    [UpstreamTest(AnthropicTests + "should include googleVertexAnthropicTools (subset of anthropicTools)", Coverage = UpstreamCoverage.Covered)]
    public void Lists_the_Anthropic_tools_that_Vertex_accepts()
    {
        Assert.Equal(
            new[]
            {
                "anthropic.bash_20241022",
                "anthropic.bash_20250124",
                "anthropic.text_editor_20241022",
                "anthropic.text_editor_20250124",
                "anthropic.text_editor_20250429",
                "anthropic.text_editor_20250728",
                "anthropic.computer_20241022",
                "anthropic.web_search_20250305",
                "anthropic.tool_search_regex_20251119",
                "anthropic.tool_search_bm25_20251119",
            },
            GoogleVertexAnthropicProvider.Tools);
        Assert.DoesNotContain("anthropic.code_execution_20250825", GoogleVertexAnthropicProvider.Tools);
        Assert.DoesNotContain("anthropic.code_execution_20260120", GoogleVertexAnthropicProvider.Tools);
    }

    [Fact]
    [UpstreamTest(AnthropicTests + "should pass custom headers to the model constructor", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_custom_Anthropic_headers()
    {
        var handler = new RecordingHandler { ResponseText = AnthropicResponse };
        var options = new GoogleVertexAnthropicOptions { Project = "p", Location = "global" };
        options.Headers["Custom-Header"] = "custom-value";
        await GoogleVertexAnthropicProvider.Create(options, handler).LanguageModel("test-model-id").DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("custom-value", handler.RequestHeaders["Custom-Header"]);
        Assert.False(handler.RequestHeaders.ContainsKey("Authorization"));
        Assert.False(handler.RequestHeaders.ContainsKey("x-api-key"));
    }

    [Fact]
    [UpstreamTest(AnthropicTests + "should create a Google Vertex Anthropic provider instance with custom settings", Coverage = UpstreamCoverage.Covered)]
    public void Creates_an_Anthropic_provider_with_custom_settings()
    {
        var options = new GoogleVertexAnthropicOptions { Project = "custom-project", Location = "custom-location", BaseUrl = "https://custom.base.url" };
        options.Headers["Custom-Header"] = "value";
        var provider = GoogleVertexAnthropicProvider.Create(options);
        Assert.Equal("custom-project", provider.Project);
        Assert.Equal("custom-location", provider.Location);
        Assert.Equal("https://custom.base.url", provider.BaseUrl);
        Assert.Equal("value", provider.Options.Headers["Custom-Header"]);
        Assert.NotNull(provider.LanguageModel("test-model-id"));
    }

    [Fact]
    [UpstreamTest(AnthropicTests + "should not support URL sources to force base64 conversion", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_no_Anthropic_URL_sources()
    {
        var model = Assert.IsType<AnthropicLanguageModel>(AnthropicOnVertex("p", "global").LanguageModel("test-model-id"));
        Assert.Empty(model.SupportedUrls);
        Assert.False(model.SupportsUrl("image/*", "https://example.com/image.png"));
    }

    [Fact]
    [UpstreamTest(AnthropicTests + "should support combining tools with structured outputs (inherited from Anthropic)", Coverage = UpstreamCoverage.Covered)]
    public void Creates_Anthropic_Messages_models()
    {
        var model = Assert.IsType<AnthropicLanguageModel>(AnthropicOnVertex("test-project", "us-east5").LanguageModel("claude-3-5-sonnet-v2@20241022"));
        Assert.Equal("claude-3-5-sonnet-v2@20241022", model.ModelId);
        Assert.Equal("googleVertex.anthropic.messages", model.Provider);
    }

    [Fact]
    [UpstreamTest(AnthropicNodeTests + "uses custom generateAuthToken when provided and skips the default", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_generated_Anthropic_bearer_token()
    {
        var handler = new RecordingHandler { ResponseText = AnthropicResponse };
        var calls = 0;
        await AnthropicWithToken(handler, _ =>
        {
            calls++;
            return Task.FromResult("custom-token");
        }).ConfigureAwait(false);
        Assert.Equal("Bearer custom-token", handler.RequestHeaders["Authorization"]);
        Assert.Equal(1, calls);
    }

    [Fact]
    [UpstreamTest(AnthropicNodeTests + "merges custom generateAuthToken with user-provided headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Merges_the_generated_Anthropic_token_with_custom_headers()
    {
        var handler = new RecordingHandler { ResponseText = AnthropicResponse };
        await AnthropicWithToken(handler, _ => Task.FromResult("custom-token"), new Dictionary<string, string?> { ["Custom-Header"] = "custom-value" }).ConfigureAwait(false);
        Assert.Equal("Bearer custom-token", handler.RequestHeaders["Authorization"]);
        Assert.Equal("custom-value", handler.RequestHeaders["Custom-Header"]);
    }

    [Fact]
    [UpstreamTest(AnthropicNodeTests + "invokes custom generateAuthToken on each headers resolution", Coverage = UpstreamCoverage.Covered)]
    public async Task Generates_an_Anthropic_token_for_each_request()
    {
        var handler = new RecordingHandler { ResponseText = AnthropicResponse };
        var calls = 0;
        var options = new GoogleVertexAnthropicOptions { Project = "test-project", Location = "global", GenerateAuthToken = _ => Task.FromResult("token-" + ++calls) };
        var model = GoogleVertexAnthropicProvider.Create(options, handler).LanguageModel("test-model-id");
        await model.DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("Bearer token-1", handler.RequestHeaders["Authorization"]);
        await model.DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("Bearer token-2", handler.RequestHeaders["Authorization"]);
        Assert.Equal(2, calls);
    }

    [Fact]
    [UpstreamTest(AnthropicNodeTests + "propagates errors thrown from custom generateAuthToken", Coverage = UpstreamCoverage.Covered)]
    public async Task Propagates_Anthropic_token_errors()
    {
        var handler = new RecordingHandler { ResponseText = AnthropicResponse };
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => AnthropicWithToken(handler, _ => throw new InvalidOperationException("token mint failed"))).ConfigureAwait(false);
        Assert.Equal("token mint failed", error.Message);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest(AnthropicNodeTests + "user-provided Authorization in headers overrides the generated token", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_a_caller_supplied_Anthropic_authorization_header()
    {
        var handler = new RecordingHandler { ResponseText = AnthropicResponse };
        await AnthropicWithToken(handler, _ => Task.FromResult("custom-token"), new Dictionary<string, string?> { ["Authorization"] = "Bearer user-override" }).ConfigureAwait(false);
        Assert.Equal("Bearer user-override", handler.RequestHeaders["Authorization"]);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/maas/google-vertex-maas-provider.test.ts::google-vertex-maas-provider::should not call createOpenAICompatible at provider creation time", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_create_the_MaaS_client_until_a_model_is_requested()
    {
        var provider = new GoogleVertexMaasProvider("test-project", "global");
        Assert.False(provider.ClientCreated);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/maas/google-vertex-maas-provider.test.ts::google-vertex-maas-provider::should create a provider with correct base URL for global location", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_global_MaaS_url()
    {
        Assert.Equal("https://aiplatform.googleapis.com/v1/projects/test-project/locations/global/endpoints/openapi", new GoogleVertexMaasProvider("test-project", "global").BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/maas/google-vertex-maas-provider.test.ts::google-vertex-maas-provider::should create a provider with correct base URL for regional location", Coverage = UpstreamCoverage.Covered)]
    public void Uses_a_regional_MaaS_url()
    {
        Assert.Equal("https://us-central1-aiplatform.googleapis.com/v1/projects/test-project/locations/us-central1/endpoints/openapi", new GoogleVertexMaasProvider("test-project", "us-central1").BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/maas/google-vertex-maas-provider.test.ts::google-vertex-maas-provider::should create a provider with correct base URL for multi-region location", Coverage = UpstreamCoverage.Covered)]
    public void Uses_a_multi_region_MaaS_url()
    {
        Assert.Equal("https://aiplatform.us.rep.googleapis.com/v1/projects/test-project/locations/us/endpoints/openapi", new GoogleVertexMaasProvider("test-project", "us").BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/maas/google-vertex-maas-provider.test.ts::google-vertex-maas-provider::should default to global location when not specified", Coverage = UpstreamCoverage.Covered)]
    public void Defaults_MaaS_to_the_global_location()
    {
        var provider = new GoogleVertexMaasProvider("test-project", null);
        Assert.Equal("global", provider.Location);
        Assert.Contains("/locations/global/", provider.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/maas/google-vertex-maas-provider.test.ts::google-vertex-maas-provider::should use custom baseURL when provided", Coverage = UpstreamCoverage.Covered)]
    public void Uses_a_custom_MaaS_base_url()
    {
        Assert.Equal("https://example.com/maas", new GoogleVertexMaasProvider("p", "global", "https://example.com/maas/").BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/maas/google-vertex-maas-provider.test.ts::google-vertex-maas-provider::should construct correct URL with trailing slash removed from baseURL", Coverage = UpstreamCoverage.Covered)]
    public void Trims_a_trailing_slash_from_the_MaaS_base_url()
    {
        Assert.Equal("https://example.com/maas", GoogleVertexEndpoints.MaasBaseUrl("p", "global", "https://example.com/maas/"));
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/maas/google-vertex-maas-provider.test.ts::google-vertex-maas-provider::should construct correct URL when baseURL is empty string", Coverage = UpstreamCoverage.Covered)]
    public void Rebuilds_the_MaaS_url_when_the_base_url_is_empty()
    {
        Assert.Equal("https://aiplatform.googleapis.com/v1/projects/test-project/locations/global/endpoints/openapi", GoogleVertexEndpoints.MaasBaseUrl("test-project", "global", ""));
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/maas/google-vertex-maas-provider.test.ts::google-vertex-maas-provider::should cache the provider after first access", Coverage = UpstreamCoverage.Covered)]
    public void Caches_the_MaaS_client_after_the_first_model()
    {
        var provider = new GoogleVertexMaasProvider("test-project", "global");
        provider.LanguageModel("meta/llama-3.1-8b-instruct-maas");
        Assert.True(provider.ClientCreated);
        provider.LanguageModel("meta/llama-3.1-8b-instruct-maas");
        Assert.True(provider.ClientCreated);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/maas/google-vertex-maas-provider.test.ts::google-vertex-maas-provider::should default Llama 4 max tokens without overriding explicit or other model settings", Coverage = UpstreamCoverage.Covered)]
    public void Defaults_Llama_4_max_tokens_only_when_unset()
    {
        var llama = GoogleVertexMaasProvider.TransformBody("meta/llama-4-scout-17b-16e-instruct-maas", new JsonObject { ["model"] = "meta/llama-4-scout-17b-16e-instruct-maas" });
        Assert.Equal(8192, llama["max_tokens"]!.GetValue<int>());
        var explicitTokens = GoogleVertexMaasProvider.TransformBody("meta/llama-4-scout-17b-16e-instruct-maas", new JsonObject { ["max_tokens"] = 10 });
        Assert.Equal(10, explicitTokens["max_tokens"]!.GetValue<int>());
        var other = GoogleVertexMaasProvider.TransformBody("meta/llama-3.1-8b-instruct-maas", new JsonObject());
        Assert.False(other.ContainsKey("max_tokens"));
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/xai/google-vertex-xai-provider.test.ts::google-vertex-xai-provider::should not call createOpenAICompatible at provider creation time", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_create_the_xAI_client_until_a_model_is_requested()
    {
        Assert.False(new GoogleVertexXaiProvider("test-project", "us-central1").ClientCreated);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/xai/google-vertex-xai-provider.test.ts::google-vertex-xai-provider::should create a provider with correct base URL for global location", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_global_host_for_xAI()
    {
        Assert.Equal("https://aiplatform.googleapis.com/v1/projects/test-project/locations/global/endpoints/openapi", new GoogleVertexXaiProvider("test-project", "global").BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/xai/google-vertex-xai-provider.test.ts::google-vertex-xai-provider::should construct the default base URL when baseURL is an empty string", Coverage = UpstreamCoverage.Covered)]
    public void Rebuilds_the_xAI_url_when_the_base_url_is_empty()
    {
        Assert.Equal("https://aiplatform.googleapis.com/v1/projects/test-project/locations/us-central1/endpoints/openapi", GoogleVertexEndpoints.XaiBaseUrl("test-project", "us-central1", ""));
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/xai/google-vertex-xai-provider.test.ts::google-vertex-xai-provider::should create a provider with correct base URL for regional location", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_the_global_xAI_host_for_a_regional_location()
    {
        Assert.Equal("https://aiplatform.googleapis.com/v1/projects/test-project/locations/us-central1/endpoints/openapi", new GoogleVertexXaiProvider("test-project", "us-central1").BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/xai/google-vertex-xai-provider.test.ts::google-vertex-xai-provider::should strip reasoning_effort from request bodies", Coverage = UpstreamCoverage.Covered)]
    public void Strips_reasoning_effort_from_xAI_requests()
    {
        var body = GoogleVertexXaiProvider.TransformBody(new JsonObject { ["model"] = "grok", ["reasoning_effort"] = "high", ["temperature"] = 0.2 });
        Assert.False(body.ContainsKey("reasoning_effort"));
        Assert.Equal(0.2, body["temperature"]!.GetValue<double>());
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/xai/google-vertex-xai-provider.test.ts::google-vertex-xai-provider::should count Grok reasoning tokens separately from completion tokens", Coverage = UpstreamCoverage.Covered)]
    public void Adds_Grok_reasoning_tokens_to_the_output_total()
    {
        var usage = GoogleVertexXaiProvider.ConvertUsage(10, 4, 2, 6);
        Assert.Equal(10, usage.InputTotal);
        Assert.Equal(8, usage.NoCache);
        Assert.Equal(2, usage.CacheRead);
        Assert.Equal(10, usage.OutputTotal);
        Assert.Equal(4, usage.Text);
        Assert.Equal(6, usage.Reasoning);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/xai/google-vertex-xai-provider.test.ts::google-vertex-xai-provider::should cache the OpenAI-compatible provider after first access", Coverage = UpstreamCoverage.Covered)]
    public void Caches_the_xAI_client_after_the_first_model()
    {
        var provider = new GoogleVertexXaiProvider("test-project", "global");
        provider.LanguageModel("grok");
        Assert.True(provider.ClientCreated);
    }


    [Fact]
    public void Rejects_locations_that_would_rewrite_the_host()
    {
        Assert.Throws<ArgumentException>(() => GoogleVertexEndpoints.RequireLabel("us.central1", "location"));
        Assert.Throws<ArgumentException>(() => GoogleVertexEndpoints.RequireLabel("bad\nlabel", "location"));
        var handler = new RecordingHandler();
        Assert.Throws<ArgumentException>(() => GoogleVertexProvider.Create(new VertexOptions { Project = "p", Region = "us.central1" }, handler));
        Assert.Equal(0, handler.Calls);
    }

    private static GoogleVertexAnthropicProvider AnthropicOnVertex(string project, string location)
    {
        return GoogleVertexAnthropicProvider.Create(new GoogleVertexAnthropicOptions { Project = project, Location = location }, new RecordingHandler());
    }

    private static Task<LanguageModelGenerateResult> AnthropicWithToken(RecordingHandler handler, Func<CancellationToken, Task<string>> token, IReadOnlyDictionary<string, string?>? headers = null)
    {
        var options = new GoogleVertexAnthropicOptions { Project = "test-project", Location = "global", GenerateAuthToken = token };
        foreach (var pair in headers ?? new Dictionary<string, string?>())
        {
            options.Headers[pair.Key] = pair.Value;
        }

        return GoogleVertexAnthropicProvider.Create(options, handler).LanguageModel("test-model-id").DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None);
    }

    private static GoogleVertexProvider Upstream(string project, string location, RecordingHandler? handler = null)
    {
        return GoogleVertexProvider.Create(new VertexOptions
        {
            Project = project,
            Region = location,
            UpstreamRoutes = true,
        }, handler ?? new RecordingHandler());
    }

}
