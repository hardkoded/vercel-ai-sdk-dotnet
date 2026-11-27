// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexProviderBase;

/// <summary>Port of <c>google-vertex-provider-base.test.ts</c> &gt; <c>google-vertex-provider-base</c>.</summary>
public sealed class GoogleVertexProviderBaseTests
{
    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use correct URL for global region",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Uses_the_global_Vertex_host()
    {
        Assert.Equal(
            "https://aiplatform.googleapis.com/v1beta1/projects/test-project/locations/global/publishers/google",
            Upstream("test-project", "global").Options.BaseUrl
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use correct URL for global region with embedding model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Uses_the_global_host_for_an_embedding_model()
    {
        var provider = Upstream("test-project", "global");
        Assert.Equal(
            "google.vertex.embedding",
            provider.EmbeddingModel("text-embedding-005").Provider
        );
        Assert.Equal(
            "https://aiplatform.googleapis.com/v1beta1/projects/test-project/locations/global/publishers/google",
            provider.Options.BaseUrl
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use correct URL for global region with image model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Uses_the_global_host_for_an_image_model()
    {
        var provider = Upstream("test-project", "global");
        Assert.Equal("google.vertex.image", provider.ImageModel("gemini-2.5-flash-image").Provider);
        Assert.Equal(
            "https://aiplatform.googleapis.com/v1beta1/projects/test-project/locations/global/publishers/google",
            provider.Options.BaseUrl
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use region-prefixed URL for non-global regions",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Prefixes_the_host_with_a_regional_location()
    {
        Assert.Equal(
            "https://us-central1-aiplatform.googleapis.com/v1beta1/projects/test-project/locations/us-central1/publishers/google",
            Upstream("test-project", "us-central1").Options.BaseUrl
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use multi-region REP URL for us location",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Uses_the_us_multi_region_host()
    {
        Assert.Equal(
            "https://aiplatform.us.rep.googleapis.com/v1beta1/projects/test-project/locations/us/publishers/google",
            Upstream("test-project", "us").Options.BaseUrl
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should configure the tool result download size limit",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2ebc78b65435926718879f85a1a85687b053c542"
    )]
    public void Configures_the_tool_result_download_size_limit()
    {
        var provider = GoogleVertexProvider.Create(
            new VertexOptions
            {
                Project = "test-project",
                Region = "test-location",
                UpstreamRoutes = true,
                ToolResultDownloads = new ToolResultDownloads { MaxBytes = 20 * 1024 * 1024 },
            },
            new RecordingHandler()
        );

        var settings = provider.ToolResultDownloadSettings!;
        Assert.Equal(20 * 1024 * 1024, settings.MaxBytes);
        Assert.True(settings.SupportsGoogleCloudStorageUrls);
        Assert.Null(settings.Fetch);
        Assert.Equal(
            7L * 1024 * 1024,
            Upstream("test-project", "us").ToolResultDownloadSettings!.MaxBytes
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use multi-region REP URL for eu location",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Uses_the_eu_multi_region_host()
    {
        Assert.Equal(
            "https://aiplatform.eu.rep.googleapis.com/v1beta1/projects/test-project/locations/eu/publishers/google",
            Upstream("test-project", "eu").Options.BaseUrl
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use express mode base URL when apiKey is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Uses_the_express_mode_base_url_for_an_api_key()
    {
        var provider = GoogleVertexProvider.Create(
            new VertexOptions
            {
                Project = "test-project",
                Region = "us-central1",
                ApiKey = "test-api-key",
                UpstreamRoutes = true,
            },
            new RecordingHandler()
        );
        Assert.Equal(GoogleVertexEndpoints.ExpressModeBaseUrl, provider.Options.BaseUrl);
        Assert.False(provider.Options.UseBearerToken);
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use custom baseURL when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Trims_a_custom_Vertex_base_url()
    {
        var provider = GoogleVertexProvider.Create(
            new VertexOptions
            {
                Project = "p",
                Region = "us-central1",
                BaseUrl = "https://custom.example.com/",
                ApiKey = "k",
            },
            new RecordingHandler()
        );
        Assert.Equal("https://custom.example.com", provider.Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should leave base model ids and the publishers/google base URL unchanged",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Keeps_publishers_google_for_a_publisher_model()
    {
        Assert.EndsWith(
            "/publishers/google",
            Upstream("test-project", "test-location").Options.BaseUrl
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use a tuned model endpoints/ id with no publishers/google suffix",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Strips_publishers_google_from_a_tuned_model_request()
    {
        var handler = new RecordingHandler();
        var previous = Environment.GetEnvironmentVariable("GOOGLE_VERTEX_API_KEY");
        Environment.SetEnvironmentVariable("GOOGLE_VERTEX_API_KEY", "test-api-key");
        try
        {
            var provider = Upstream("test-project", "test-location", handler);
            await provider
                .LanguageModel("endpoints/1234567890")
                .DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None)
                .ConfigureAwait(false);
            Assert.Contains(
                "/locations/test-location/endpoints/1234567890:generateContent",
                handler.Uris[0]
            );
            Assert.DoesNotContain("publishers/google", handler.Uris[0]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GOOGLE_VERTEX_API_KEY", previous);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should respect a custom baseURL for tuned model endpoints/ ids",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Keeps_a_custom_base_url_for_a_tuned_model()
    {
        var handler = new RecordingHandler();
        var provider = GoogleVertexProvider.Create(
            new VertexOptions
            {
                Project = "p",
                Region = "us-central1",
                BaseUrl = "https://custom-endpoint.example.com",
                ApiKey = "k",
            },
            handler
        );
        await provider
            .LanguageModel("endpoints/1234567890")
            .DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None)
            .ConfigureAwait(false);
        Assert.StartsWith(
            "https://custom-endpoint.example.com/endpoints/1234567890:generateContent",
            handler.Uris[0]
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should reject Express Mode for tuned models",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Rejects_express_mode_for_tuned_models()
    {
        var provider = GoogleVertexProvider.Create(
            new VertexOptions
            {
                Project = "p",
                Region = "us-central1",
                ApiKey = "test-api-key",
                UpstreamRoutes = true,
            },
            new RecordingHandler()
        );
        var error = Assert.Throws<InvalidOperationException>(() =>
            provider.LanguageModel("endpoints/1234567890")
        );
        Assert.Equal(
            "Google Vertex tuned models do not support Express Mode API keys. Use standard Google Cloud credentials instead.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should throw for interactions models when an Express Mode API key is set",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Rejects_express_mode_for_interactions()
    {
        var provider = GoogleVertexProvider.Create(
            new VertexOptions
            {
                Project = "p",
                Region = "us-central1",
                ApiKey = "test-api-key",
                UpstreamRoutes = true,
            },
            new RecordingHandler()
        );
        var error = Assert.Throws<InvalidOperationException>(() =>
            provider.Interactions("gemini-omni-flash-preview")
        );
        Assert.Contains("do not support Express Mode API keys", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should create an interactions model targeting the location-scoped interactions resource",
        Coverage = UpstreamCoverage.Covered
    )]
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
            await model
                .DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None)
                .ConfigureAwait(false);
            Assert.Equal(
                "https://test-location-aiplatform.googleapis.com/v1beta1/projects/test-project/locations/test-location/interactions",
                handler.Uris[0]
            );
        }
        finally
        {
            Environment.SetEnvironmentVariable("GOOGLE_VERTEX_API_KEY", previous);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should create an embedding model with correct settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Creates_a_Vertex_embedding_model()
    {
        var provider = Upstream("test-project", "test-location");
        Assert.Equal(
            "google.vertex.embedding",
            provider.EmbeddingModel("test-embedding-model").Provider
        );
        Assert.Equal(
            "https://test-location-aiplatform.googleapis.com/v1beta1/projects/test-project/locations/test-location/publishers/google",
            provider.Options.BaseUrl
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should create a speech model with correct settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Creates_a_Vertex_Gemini_speech_model()
    {
        var provider = Upstream("test-project", "test-location");
        var model = Assert.IsType<Vercel.AI.Google.GoogleSpeechModel>(
            provider.SpeechModel("gemini-2.5-flash-tts")
        );
        Assert.Equal("google.vertex.speech", model.Provider);
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should create a speech model via speechModel()",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Creates_a_Vertex_speech_model_from_SpeechModel()
    {
        Assert.Equal(
            "google.vertex.speech",
            Upstream("test-project", "test-location").SpeechModel("gemini-2.5-pro-tts").Provider
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should route chirp speech models to the Cloud Text-to-Speech model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Routes_Chirp_models_to_Cloud_Text_to_Speech()
    {
        var model = Assert.IsType<GoogleVertexCloudSpeechModel>(
            Upstream("test-project", "test-location").SpeechModel("chirp-3-hd")
        );
        Assert.Equal("google.vertex.speech", model.Provider);
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should throw for chirp speech models when an Express Mode API key is set",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Rejects_express_mode_for_Chirp_speech()
    {
        var provider = GoogleVertexProvider.Create(
            new VertexOptions
            {
                Project = "p",
                Region = "us-central1",
                ApiKey = "test-api-key",
                UpstreamRoutes = true,
            },
            new RecordingHandler()
        );
        var error = Assert.Throws<InvalidOperationException>(() =>
            provider.SpeechModel("chirp-3-hd")
        );
        Assert.Equal(
            "Google Vertex Chirp speech models do not support Express Mode API keys. Use standard Google Cloud credentials instead.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should keep routing gemini speech models to the Gemini speech model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Keeps_Gemini_speech_on_the_Gemini_speech_model()
    {
        Assert.IsType<Vercel.AI.Google.GoogleSpeechModel>(
            Upstream("test-project", "test-location").SpeechModel("gemini-2.5-flash-tts")
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should create a transcription model with correct settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Creates_a_Vertex_speech_transcription_model()
    {
        var model = Assert.IsType<GoogleVertexSpeechTranscriptionModel>(
            Upstream("test-project", "us-central1").TranscriptionModel("chirp_2")
        );
        Assert.Equal("google.vertex.transcription", model.Provider);
        Assert.Equal("us-central1", Upstream("test-project", "us-central1").Vertex.Region);
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should create a transcription model via transcriptionModel()",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Creates_a_Vertex_transcription_model_from_TranscriptionModel()
    {
        Assert.Equal(
            "google.vertex.transcription",
            Upstream("test-project", "us-central1").TranscriptionModel("chirp_3").Provider
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should reject Express Mode for transcription models",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Rejects_express_mode_for_transcription()
    {
        var provider = GoogleVertexProvider.Create(
            new VertexOptions
            {
                Project = "p",
                Region = "us-central1",
                ApiKey = "test-api-key",
                UpstreamRoutes = true,
            },
            new RecordingHandler()
        );
        var error = Assert.Throws<InvalidOperationException>(() =>
            provider.TranscriptionModel("chirp_3")
        );
        Assert.Equal(
            "Google Vertex transcription models do not support Express Mode API keys. Use standard Google Cloud credentials instead.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should create an image model with default settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Creates_a_Vertex_image_model()
    {
        var model = Assert.IsType<Vercel.AI.Google.GoogleVertexImageModel>(
            Upstream("test-project", "test-location").ImageModel("gemini-2.5-flash-image")
        );
        Assert.Equal("google.vertex.image", model.Provider);
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should create a language model with default settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_create_a_language_model_with_default_settings()
    {
        var provider = Upstream("test-project", "test-location");

        var model = Assert.IsType<GoogleLanguageModel>(provider.LanguageModel("test-model-id"));

        Assert.Equal("test-model-id", model.ModelId);
        Assert.Equal("google.vertex.chat", provider.ModelProvider);
        Assert.Equal(
            "https://test-location-aiplatform.googleapis.com/v1beta1/projects/test-project/locations/test-location/publishers/google",
            provider.Options.BaseUrl
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should throw an error when using new keyword",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_an_error_when_using_new_keyword()
    {
        var error = Assert.Throws<InvalidOperationException>(() =>
            Upstream("test-project", "global").New("test-model-id")
        );

        Assert.Equal(
            "The Google Vertex AI model function cannot be called with the new keyword.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should pass custom headers to the model constructor",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_custom_headers_to_the_model_constructor()
    {
        var handler = new RecordingHandler();
        var provider = GoogleVertexProvider.Create(
            new VertexOptions
            {
                Project = "test-project",
                Region = "test-location",
                UpstreamRoutes = true,
                GenerateAuthToken = _ => Task.FromResult("token"),
                Headers = { ["Custom-Header"] = "custom-value" },
            },
            handler
        );

        await provider
            .LanguageModel("test-model-id")
            .DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None);

        Assert.Equal("custom-value", handler.RequestHeaders["Custom-Header"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should pass custom generateId function to the model constructor",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_custom_generateId_function_to_the_model_constructor()
    {
        var handler = new RecordingHandler
        {
            ResponseText =
                "{\"candidates\":[{\"content\":{\"parts\":[{\"functionCall\":{\"name\":\"weather\",\"args\":{}}}],\"role\":\"model\"},\"finishReason\":\"STOP\"}]}",
        };
        var provider = GoogleVertexProvider.Create(
            new VertexOptions
            {
                Project = "test-project",
                Region = "test-location",
                UpstreamRoutes = true,
                GenerateAuthToken = _ => Task.FromResult("token"),
                GenerateId = () => "custom-id",
            },
            handler
        );

        var result = await provider
            .LanguageModel("test-model-id")
            .DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None);

        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("custom-id", call.ToolCallId);
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use languageModel method to create a model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_languageModel_method_to_create_a_model()
    {
        var model = Assert.IsType<GoogleLanguageModel>(
            Upstream("test-project", "test-location").LanguageModel("test-model-id")
        );

        Assert.Equal("test-model-id", model.ModelId);
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should add API key as query parameter via custom fetch",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_add_API_key_as_query_parameter_via_custom_fetch()
    {
        var handler = new RecordingHandler();
        var provider = GoogleVertexProvider.Create(
            new VertexOptions { ApiKey = "test-api-key", UpstreamRoutes = true },
            handler
        );

        await provider
            .LanguageModel("gemini-2.5-pro")
            .DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None);

        Assert.StartsWith(
            "https://aiplatform.googleapis.com/v1/publishers/google/models/gemini-2.5-pro:generateContent",
            handler.Uris[0]
        );
        Assert.Equal("test-api-key", handler.RequestHeaders["x-goog-api-key"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should create a video model with default settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_create_a_video_model_with_default_settings()
    {
        var provider = Upstream("test-project", "test-location");

        var model = provider.VideoModel("veo-2.0-generate-001");

        Assert.Equal("google.vertex.video", model.Provider);
        Assert.Equal(
            "https://test-location-aiplatform.googleapis.com/v1beta1/projects/test-project/locations/test-location/publishers/google",
            provider.Options.BaseUrl
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use correct URL for global region with video model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_correct_URL_for_global_region_with_video_model()
    {
        var provider = Upstream("test-project", "global");

        var model = provider.VideoModel("veo-3.0-generate-001");

        Assert.Equal("google.vertex.video", model.Provider);
        Assert.Equal(
            "https://aiplatform.googleapis.com/v1beta1/projects/test-project/locations/global/publishers/google",
            provider.Options.BaseUrl
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google-vertex/src/google-vertex-provider-base.test.ts::google-vertex-provider-base::should use custom baseURL for video model when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_custom_baseURL_for_video_model_when_provided()
    {
        var provider = GoogleVertexProvider.Create(
            new VertexOptions
            {
                Project = "test-project",
                Region = "test-location",
                UpstreamRoutes = true,
                BaseUrl = "https://custom-endpoint.example.com",
            },
            new RecordingHandler()
        );

        var model = provider.VideoModel("veo-2.0-generate-001");

        Assert.Equal("google.vertex.video", model.Provider);
        Assert.Equal("https://custom-endpoint.example.com", provider.Options.BaseUrl);
    }

    private static GoogleVertexProvider Upstream(
        string project,
        string location,
        RecordingHandler? handler = null
    )
    {
        return GoogleVertexProvider.Create(
            new VertexOptions
            {
                Project = project,
                Region = location,
                UpstreamRoutes = true,
            },
            handler ?? new RecordingHandler()
        );
    }
}
