// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Vertex, Anthropic, MaaS, and xAI request targets.</summary>
public sealed class GoogleVertexUpstreamTests
{
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

    private const string EdgeAuthTests = "packages/google-vertex/src/edge/google-vertex-auth-edge.test.ts::Google Vertex Edge Auth::";
    private const string EdgeProviderTests = "packages/google-vertex/src/edge/google-vertex-provider-edge.test.ts::google-vertex-provider-edge::";
    private const string TestClientEmail = "test@test.iam.gserviceaccount.com";
    private static readonly RSA TestKey = RSA.Create(2048);

    [Fact]
    [UpstreamTest(EdgeAuthTests + "should generate a valid JWT token", Coverage = UpstreamCoverage.Covered)]
    public async Task Signs_a_service_account_JWT_and_returns_the_access_token()
    {
        var handler = new TokenHandler();
        var token = await GoogleVertexServiceAccount.GenerateAuthTokenAsync(new HttpClient(handler), Credentials("test-key-id"), CancellationToken.None).ConfigureAwait(false);

        Assert.Equal("mock-auth-token", token);
        Assert.Equal(GoogleVertexServiceAccount.TokenUrl, handler.Uris[0]);
        Assert.Equal("urn:ietf:params:oauth:grant-type:jwt-bearer", handler.Form["grant_type"]);
        var parts = handler.Form["assertion"].Split('.');
        Assert.Equal(3, parts.Length);
        var header = JsonNode.Parse(FromBase64Url(parts[0]))!;
        Assert.Equal("RS256", header["alg"]!.GetValue<string>());
        Assert.Equal("JWT", header["typ"]!.GetValue<string>());
        Assert.Equal("test-key-id", header["kid"]!.GetValue<string>());
        var payload = JsonNode.Parse(FromBase64Url(parts[1]))!;
        Assert.Equal(TestClientEmail, payload["iss"]!.GetValue<string>());
        Assert.Equal("https://www.googleapis.com/auth/cloud-platform", payload["scope"]!.GetValue<string>());
        Assert.Equal("https://oauth2.googleapis.com/token", payload["aud"]!.GetValue<string>());
        Assert.Equal(3600, payload["exp"]!.GetValue<long>() - payload["iat"]!.GetValue<long>());
        Assert.True(TestKey.VerifyData(
            Encoding.ASCII.GetBytes(parts[0] + "." + parts[1]),
            FromBase64Url(parts[2]),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1));
    }

    [Fact]
    [UpstreamTest(EdgeAuthTests + "should throw error with invalid credentials", Coverage = UpstreamCoverage.Covered)]
    public async Task Fails_when_the_token_endpoint_rejects_the_credentials()
    {
        var handler = new TokenHandler { Status = HttpStatusCode.BadRequest, Reason = "Bad Request" };
        var error = await Assert.ThrowsAsync<AiSdkException>(() => GoogleVertexServiceAccount.GenerateAuthTokenAsync(new HttpClient(handler), Credentials("test-key-id"), CancellationToken.None)).ConfigureAwait(false);
        Assert.Equal("Token request failed: Bad Request", error.Message);
    }

    [Fact]
    [UpstreamTest(EdgeAuthTests + "should load credentials from environment variables", Coverage = UpstreamCoverage.Covered)]
    public async Task Loads_service_account_credentials_from_the_environment()
    {
        var handler = new TokenHandler();
        await WithEnvironment(TestClientEmail, Pem(), "test-key-id", async () =>
        {
            Assert.Equal("mock-auth-token", await GoogleVertexServiceAccount.GenerateAuthTokenAsync(new HttpClient(handler), null, CancellationToken.None).ConfigureAwait(false));
        }).ConfigureAwait(false);
        Assert.Equal(TestClientEmail, AssertionPart(handler, 1)["iss"]!.GetValue<string>());
        Assert.Equal("test-key-id", AssertionPart(handler, 0)["kid"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(EdgeAuthTests + "should throw error when client email is missing", Coverage = UpstreamCoverage.Covered)]
    public async Task Requires_a_client_email()
    {
        await WithEnvironment(null, Pem(), "test-key-id", async () =>
        {
            var error = await Assert.ThrowsAsync<AiSdkException>(() => GoogleVertexServiceAccount.GenerateAuthTokenAsync(new HttpClient(new TokenHandler()), null, CancellationToken.None)).ConfigureAwait(false);
            Assert.Contains("Google client email setting is missing. Pass it using the 'clientEmail' parameter or the GOOGLE_CLIENT_EMAIL environment variable.", error.Message);
        }).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest(EdgeAuthTests + "should throw error when private key is missing", Coverage = UpstreamCoverage.Covered)]
    public async Task Requires_a_private_key()
    {
        await WithEnvironment(TestClientEmail, null, "test-key-id", async () =>
        {
            var error = await Assert.ThrowsAsync<AiSdkException>(() => GoogleVertexServiceAccount.GenerateAuthTokenAsync(new HttpClient(new TokenHandler()), null, CancellationToken.None)).ConfigureAwait(false);
            Assert.Contains("Google private key setting is missing. Pass it using the 'privateKey' parameter or the GOOGLE_PRIVATE_KEY environment variable.", error.Message);
        }).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest(EdgeAuthTests + "should work with or without private key ID", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_environment_credentials_with_or_without_a_key_id()
    {
        var withKeyId = new TokenHandler();
        await WithEnvironment(TestClientEmail, Pem(), "test-key-id", async () =>
        {
            Assert.Equal("mock-auth-token", await GoogleVertexServiceAccount.GenerateAuthTokenAsync(new HttpClient(withKeyId), null, CancellationToken.None).ConfigureAwait(false));
        }).ConfigureAwait(false);
        var withoutKeyId = new TokenHandler();
        await WithEnvironment(TestClientEmail, Pem(), null, async () =>
        {
            Assert.Equal("mock-auth-token", await GoogleVertexServiceAccount.GenerateAuthTokenAsync(new HttpClient(withoutKeyId), null, CancellationToken.None).ConfigureAwait(false));
        }).ConfigureAwait(false);
        Assert.Equal("test-key-id", AssertionPart(withKeyId, 0)["kid"]!.GetValue<string>());
        Assert.False(AssertionPart(withoutKeyId, 0).AsObject().ContainsKey("kid"));
    }

    [Fact]
    [UpstreamTest(EdgeAuthTests + "should handle newlines in private key from env vars", Coverage = UpstreamCoverage.Covered)]
    public async Task Reads_escaped_newlines_in_an_environment_private_key()
    {
        var handler = new TokenHandler();
        await WithEnvironment(TestClientEmail, Pem().Replace("\n", "\\n"), "test-key-id", async () =>
        {
            Assert.Equal("mock-auth-token", await GoogleVertexServiceAccount.GenerateAuthTokenAsync(new HttpClient(handler), null, CancellationToken.None).ConfigureAwait(false));
        }).ConfigureAwait(false);
        var parts = handler.Form["assertion"].Split('.');
        Assert.True(TestKey.VerifyData(Encoding.ASCII.GetBytes(parts[0] + "." + parts[1]), FromBase64Url(parts[2]), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
    }

    [Fact]
    [UpstreamTest(EdgeAuthTests + "should throw error on fetch failure", Coverage = UpstreamCoverage.Covered)]
    public async Task Propagates_a_token_request_network_error()
    {
        var handler = new TokenHandler { Error = new HttpRequestException("Network error") };
        var error = await Assert.ThrowsAsync<HttpRequestException>(() => GoogleVertexServiceAccount.GenerateAuthTokenAsync(new HttpClient(handler), Credentials("test-key-id"), CancellationToken.None)).ConfigureAwait(false);
        Assert.Equal("Network error", error.Message);
    }

    [Fact]
    [UpstreamTest(EdgeAuthTests + "should throw error when token request fails", Coverage = UpstreamCoverage.Covered)]
    public async Task Fails_when_the_token_request_is_unauthorized()
    {
        var handler = new TokenHandler { Status = HttpStatusCode.Unauthorized, Reason = "Unauthorized" };
        var error = await Assert.ThrowsAsync<AiSdkException>(() => GoogleVertexServiceAccount.GenerateAuthTokenAsync(new HttpClient(handler), Credentials("test-key-id"), CancellationToken.None)).ConfigureAwait(false);
        Assert.Equal("Token request failed: Unauthorized", error.Message);
    }

    [Fact]
    [UpstreamTest(EdgeAuthTests + "should work without privateKeyId", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_kid_without_a_private_key_id()
    {
        var handler = new TokenHandler();
        Assert.Equal("mock-auth-token", await GoogleVertexServiceAccount.GenerateAuthTokenAsync(new HttpClient(handler), Credentials(null), CancellationToken.None).ConfigureAwait(false));
        Assert.Equal(3, handler.Form["assertion"].Split('.').Length);
        Assert.False(AssertionPart(handler, 0).AsObject().ContainsKey("kid"));
    }

    [Fact]
    [UpstreamTest(EdgeAuthTests + "should include correct user-agent header", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_Vertex_user_agent_to_the_token_endpoint()
    {
        var handler = new TokenHandler();
        await GoogleVertexServiceAccount.GenerateAuthTokenAsync(new HttpClient(handler), Credentials("test-key-id"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("ai-sdk/google-vertex/0.0.0-test runtime/dotnet", handler.Headers[0]["User-Agent"]);
    }

    [Fact]
    [UpstreamTest(EdgeProviderTests + "default headers function should return auth token", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_a_service_account_token_from_the_environment_when_no_API_key_is_set()
    {
        var handler = new TokenHandler();
        await WithEnvironment(TestClientEmail, Pem(), null, () => Generate(new VertexOptions { Project = "test-project" }, handler)).ConfigureAwait(false);
        Assert.Equal("Bearer mock-auth-token", handler.Headers[1]["Authorization"]);
    }

    [Fact]
    [UpstreamTest(EdgeProviderTests + "should use custom headers in addition to auth token when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_custom_headers_with_the_service_account_token()
    {
        var handler = new TokenHandler();
        var options = new VertexOptions { Project = "test-project", GoogleCredentials = Credentials(null) };
        options.Headers["Custom-Header"] = "custom-value";
        await WithEnvironment(null, null, null, () => Generate(options, handler)).ConfigureAwait(false);
        Assert.Equal("Bearer mock-auth-token", handler.Headers[1]["Authorization"]);
        Assert.Equal("custom-value", handler.Headers[1]["Custom-Header"]);
    }

    [Fact]
    [UpstreamTest(EdgeProviderTests + "should use edge auth token generator", Coverage = UpstreamCoverage.Covered)]
    public async Task Requests_a_token_before_the_model_call()
    {
        var handler = new TokenHandler();
        await WithEnvironment(null, null, null, () => Generate(new VertexOptions { Project = "test-project", GoogleCredentials = Credentials(null) }, handler)).ConfigureAwait(false);
        Assert.Equal(GoogleVertexServiceAccount.TokenUrl, handler.Uris[0]);
        Assert.EndsWith(":generateContent", handler.Uris[1]);
    }

    [Fact]
    [UpstreamTest(EdgeProviderTests + "passes googleCredentials to generateAuthToken", Coverage = UpstreamCoverage.Covered)]
    public async Task Signs_the_token_with_the_configured_credentials()
    {
        var handler = new TokenHandler();
        var credentials = new GoogleCredentials { ClientEmail = "test@example.com", PrivateKey = Pem() };
        await WithEnvironment(TestClientEmail, null, null, () => Generate(new VertexOptions { Project = "test-project", GoogleCredentials = credentials }, handler)).ConfigureAwait(false);
        Assert.Equal("test@example.com", AssertionPart(handler, 1)["iss"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(EdgeProviderTests + "should pass options through to base provider when apiKey is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_API_key_without_requesting_a_token()
    {
        var handler = new TokenHandler();
        await WithEnvironment(TestClientEmail, Pem(), null, () => Generate(new VertexOptions { Project = "test-project", ApiKey = "test-api-key", UpstreamRoutes = true }, handler)).ConfigureAwait(false);
        Assert.Single(handler.Uris);
        Assert.DoesNotContain(GoogleVertexServiceAccount.TokenUrl, handler.Uris);
        Assert.Equal("test-api-key", handler.Headers[0]["x-goog-api-key"]);
        Assert.False(handler.Headers[0].ContainsKey("Authorization"));
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
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-provider.test.ts::google-vertex-anthropic-provider::should use correct URL for global location", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_global_Anthropic_publisher_url()
    {
        Assert.Equal("https://aiplatform.googleapis.com/v1/projects/test-project/locations/global/publishers/anthropic/models", new GoogleVertexAnthropicProvider("test-project", "global").BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-provider.test.ts::google-vertex-anthropic-provider::should use region-prefixed URL for non-global locations", Coverage = UpstreamCoverage.Covered)]
    public void Prefixes_the_Anthropic_host_with_the_region()
    {
        Assert.Equal("https://us-central1-aiplatform.googleapis.com/v1/projects/test-project/locations/us-central1/publishers/anthropic/models", new GoogleVertexAnthropicProvider("test-project", "us-central1").BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-provider.test.ts::google-vertex-anthropic-provider::should use multi-region URL for eu location", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_eu_Anthropic_host()
    {
        Assert.Equal("https://aiplatform.eu.rep.googleapis.com/v1/projects/test-project/locations/eu/publishers/anthropic/models", new GoogleVertexAnthropicProvider("test-project", "eu").BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-provider.test.ts::google-vertex-anthropic-provider::should use multi-region URL for us location", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_us_Anthropic_host()
    {
        Assert.Equal("https://aiplatform.us.rep.googleapis.com/v1/projects/test-project/locations/us/publishers/anthropic/models", new GoogleVertexAnthropicProvider("test-project", "us").BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-provider.test.ts::google-vertex-anthropic-provider::should pass baseURL to the model when created", Coverage = UpstreamCoverage.Covered)]
    public void Uses_a_custom_Anthropic_base_url()
    {
        Assert.Equal("https://example.com/anthropic", new GoogleVertexAnthropicProvider("p", "global", "https://example.com/anthropic").BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-provider.test.ts::google-vertex-anthropic-provider::should throw NoSuchModelError for textEmbeddingModel", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_Anthropic_embedding_models()
    {
        var error = Assert.Throws<AiSdkException>(() => new GoogleVertexAnthropicProvider("p", "global").EmbeddingModel("text"));
        Assert.Contains("google.vertex.anthropic", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/anthropic/google-vertex-anthropic-provider.test.ts::google-vertex-anthropic-provider::should pass custom headers to the model constructor", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_caller_supplied_Anthropic_authorization_header()
    {
        var headers = new GoogleVertexAnthropicProvider("p", "global", accessToken: "token").Headers(new Dictionary<string, string?> { ["Authorization"] = "Bearer caller", ["X-Custom"] = "1" });
        Assert.Equal("Bearer caller", headers["Authorization"]);
        Assert.Equal("1", headers["X-Custom"]);
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

    private static GoogleVertexProvider Upstream(string project, string location, RecordingHandler? handler = null)
    {
        return GoogleVertexProvider.Create(new VertexOptions
        {
            Project = project,
            Region = location,
            UpstreamRoutes = true,
        }, handler ?? new RecordingHandler());
    }

    private static string Pem()
    {
        return TestKey.ExportPkcs8PrivateKeyPem();
    }

    private static GoogleCredentials Credentials(string? privateKeyId)
    {
        return new GoogleCredentials { ClientEmail = TestClientEmail, PrivateKey = Pem(), PrivateKeyId = privateKeyId };
    }

    private static byte[] FromBase64Url(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(base64.PadRight(base64.Length + ((4 - (base64.Length % 4)) % 4), '='));
    }

    private static JsonNode AssertionPart(TokenHandler handler, int index)
    {
        return JsonNode.Parse(FromBase64Url(handler.Form["assertion"].Split('.')[index]))!;
    }

    private static Task Generate(VertexOptions options, TokenHandler handler)
    {
        return GoogleVertexProvider.Create(options, handler).LanguageModel("gemini-2.5-flash").DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None);
    }

    /// <summary>Sets the service account variables and clears <c>GOOGLE_VERTEX_API_KEY</c> while <paramref name="action"/> runs.</summary>
    private static async Task WithEnvironment(string? clientEmail, string? privateKey, string? privateKeyId, Func<Task> action)
    {
        var values = new Dictionary<string, string?>
        {
            ["GOOGLE_CLIENT_EMAIL"] = clientEmail,
            ["GOOGLE_PRIVATE_KEY"] = privateKey,
            ["GOOGLE_PRIVATE_KEY_ID"] = privateKeyId,
            ["GOOGLE_VERTEX_API_KEY"] = null,
        };
        var previous = new Dictionary<string, string?>();
        foreach (var pair in values)
        {
            previous[pair.Key] = Environment.GetEnvironmentVariable(pair.Key);
            Environment.SetEnvironmentVariable(pair.Key, pair.Value);
        }

        try
        {
            await action().ConfigureAwait(false);
        }
        finally
        {
            foreach (var pair in previous)
            {
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
            }
        }
    }

    /// <summary>Answers the OAuth token endpoint and Gemini calls, and records each request.</summary>
    private sealed class TokenHandler : HttpMessageHandler
    {
        public List<string> Uris { get; } = new();

        public List<Dictionary<string, string>> Headers { get; } = new();

        public Dictionary<string, string> Form { get; } = new();

        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        public string? Reason { get; set; }

        public Exception? Error { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri!.AbsoluteUri;
            Uris.Add(uri);
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in request.Headers)
            {
                headers[header.Key] = string.Join(" ", header.Value);
            }

            Headers.Add(headers);
            if (uri != GoogleVertexServiceAccount.TokenUrl)
            {
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new RecordingHandler().ResponseText, Encoding.UTF8, "application/json") };
            }

            if (Error != null)
            {
                throw Error;
            }

            foreach (var pair in (await request.Content!.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)).Split('&'))
            {
                var parts = pair.Split('=');
                Form[Uri.UnescapeDataString(parts[0])] = Uri.UnescapeDataString(parts[1].Replace('+', ' '));
            }

            return new HttpResponseMessage(Status)
            {
                ReasonPhrase = Reason,
                Content = new StringContent("{\"access_token\":\"mock-auth-token\"}", Encoding.UTF8, "application/json"),
            };
        }
    }
}
