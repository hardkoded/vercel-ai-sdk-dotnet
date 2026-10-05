// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Vertex host and URL construction. Locations are single DNS labels so they cannot rewrite the host.</summary>
public static class GoogleVertexEndpoints
{
    /// <summary>Express Mode origin used when an API key is supplied.</summary>
    public const string ExpressModeBaseUrl = "https://aiplatform.googleapis.com/v1/publishers/google";

    /// <summary>
    /// Returns the Vertex AI host for <paramref name="location"/>.
    /// <c>global</c> uses the unprefixed host. <c>eu</c> and <c>us</c> use the multi-region REP host.
    /// </summary>
    public static string Host(string location)
    {
        var label = RequireLabel(location, "location");
        if (label == "global")
        {
            return "aiplatform.googleapis.com";
        }

        if (label == "eu" || label == "us")
        {
            return "aiplatform." + label + ".rep.googleapis.com";
        }

        return label + "-aiplatform.googleapis.com";
    }

    /// <summary>Speech-to-Text host. <c>global</c> is <c>speech.googleapis.com</c>.</summary>
    public static string SpeechHost(string location)
    {
        var label = RequireLabel(location, "location");
        return label == "global" ? "speech.googleapis.com" : label + "-speech.googleapis.com";
    }

    /// <summary>Publisher base URL for Gemini on Vertex. Tuned <c>endpoints/</c> ids omit <c>/publishers/google</c>.</summary>
    public static string GeminiBaseUrl(string project, string location, string? baseUrl, bool expressMode, bool endpointModel)
    {
        if (expressMode)
        {
            if (endpointModel)
            {
                throw new InvalidOperationException("Google Vertex tuned models do not support Express Mode API keys. Use standard Google Cloud credentials instead.");
            }

            return Trim(baseUrl) ?? ExpressModeBaseUrl;
        }

        var custom = Trim(baseUrl);
        if (custom != null)
        {
            return custom;
        }

        var label = RequireLabel(location, "location");
        var suffix = endpointModel ? string.Empty : "/publishers/google";
        return "https://" + Host(label) + "/v1beta1/projects/" + project + "/locations/" + label + suffix;
    }

    /// <summary>Anthropic on Vertex publisher URL.</summary>
    public static string AnthropicBaseUrl(string? project, string? location, string? baseUrl)
    {
        var custom = Trim(baseUrl);
        if (custom != null)
        {
            return custom;
        }

        var label = RequireLabel(location ?? string.Empty, "location");
        return "https://" + Host(label) + "/v1/projects/" + (project ?? string.Empty) + "/locations/" + label + "/publishers/anthropic/models";
    }

    /// <summary>OpenAI-compatible MaaS URL. An empty base URL is rebuilt from the project and location.</summary>
    public static string MaasBaseUrl(string project, string? location, string? baseUrl)
    {
        var custom = Trim(baseUrl);
        if (custom != null)
        {
            return custom;
        }

        var label = string.IsNullOrEmpty(location) ? "global" : RequireLabel(location!, "location");
        return "https://" + Host(label) + "/v1/projects/" + project + "/locations/" + label + "/endpoints/openapi";
    }

    /// <summary>xAI on Vertex always uses the global AI Platform host.</summary>
    public static string XaiBaseUrl(string project, string? location, string? baseUrl)
    {
        var custom = Trim(baseUrl);
        if (custom != null)
        {
            return custom;
        }

        var label = string.IsNullOrEmpty(location) ? "global" : RequireLabel(location!, "location");
        return "https://aiplatform.googleapis.com/v1/projects/" + project + "/locations/" + label + "/endpoints/openapi";
    }

    /// <summary>Cloud Speech-to-Text recognize URL.</summary>
    public static string RecognizeUrl(string project, string location)
    {
        var label = RequireLabel(location, "location");
        return "https://" + SpeechHost(label) + "/v2/projects/" + project + "/locations/" + label + "/recognizers/_:recognize";
    }

    /// <summary>True when the model id addresses a deployed endpoint.</summary>
    public static bool IsEndpointModel(string modelId)
    {
        return modelId != null && modelId.StartsWith("endpoints/", StringComparison.Ordinal);
    }

    /// <summary>Rejects a location that is not one DNS label.</summary>
    public static string RequireLabel(string location, string name)
    {
        if (!HostnameParts.IsValidHostnamePart(location))
        {
            throw new ArgumentException("A Vertex location must be a single DNS label.", name);
        }

        return location;
    }

    private static string? Trim(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        return baseUrl!.TrimEnd('/');
    }
}

/// <summary>Header construction for Vertex. Live Application Default Credentials are not contacted.</summary>
public static class GoogleVertexAuth
{
    /// <summary>Bearer header for an access token that the caller already obtained.</summary>
    public static IReadOnlyDictionary<string, string?> Bearer(string accessToken, IReadOnlyDictionary<string, string?>? headers = null)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (headers != null)
        {
            foreach (var pair in headers)
            {
                result[pair.Key] = pair.Value;
            }
        }

        if (!result.ContainsKey("Authorization"))
        {
            result["Authorization"] = "Bearer " + accessToken;
        }

        return result;
    }

    /// <summary>Express Mode sends the API key and does not add a bearer token.</summary>
    public static IReadOnlyDictionary<string, string?> Express(string apiKey, IReadOnlyDictionary<string, string?>? headers = null)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (headers != null)
        {
            foreach (var pair in headers)
            {
                result[pair.Key] = pair.Value;
            }
        }

        result["x-goog-api-key"] = apiKey;
        return result;
    }
}

/// <summary>Vertex settings. The default route stays compatible with the regional <c>v1</c> publisher URL.</summary>
public sealed class VertexOptions : GoogleOptions
{
    /// <summary>
    /// Creates Vertex options. An empty <see cref="GoogleOptions.BaseUrl"/> is replaced with the regional publisher host.
    /// </summary>
    public VertexOptions()
    {
        UseBearerToken = true;
        ApiKeyEnvironmentVariable = "GOOGLE_VERTEX_API_KEY";
        BaseUrl = string.Empty;
    }

    /// <summary>GCP project id.</summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>Vertex location. One DNS label.</summary>
    public string Region { get; set; } = "us-central1";

    /// <summary>
    /// When true, URLs follow the upstream host rules (<c>global</c>, <c>eu</c>, <c>us</c>, Express Mode, <c>v1beta1</c>).
    /// The default keeps the regional <c>v1</c> publisher URL.
    /// </summary>
    public bool UpstreamRoutes { get; set; }
}

/// <summary>Google Vertex AI provider.</summary>
public sealed class GoogleVertexProvider : GoogleProvider
{
    /// <summary>Creates a Vertex provider.</summary>
    public GoogleVertexProvider(HttpClient httpClient, VertexOptions? options = null)
        : base(httpClient, Prepare(options), "google.vertex.chat")
    {
        Vertex = (VertexOptions)Options;
    }

    /// <summary>Vertex options.</summary>
    public VertexOptions Vertex { get; }

    /// <summary>Creates a provider.</summary>
    public static GoogleVertexProvider Create(VertexOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new GoogleVertexProvider(client, options);
    }

    /// <summary>Tuned <c>endpoints/</c> models reject Express Mode and omit <c>/publishers/google</c>.</summary>
    public override ILanguageModel LanguageModel(string modelId)
    {
        if (Vertex.UpstreamRoutes && !string.IsNullOrEmpty(Options.ApiKey) && GoogleVertexEndpoints.IsEndpointModel(modelId))
        {
            throw new InvalidOperationException("Google Vertex tuned models do not support Express Mode API keys. Use standard Google Cloud credentials instead.");
        }

        return base.LanguageModel(modelId);
    }

    /// <summary>Interactions use the location resource and reject Express Mode.</summary>
    public override ILanguageModel Interactions(string modelId)
    {
        if (Vertex.UpstreamRoutes && !string.IsNullOrEmpty(Options.ApiKey))
        {
            throw new InvalidOperationException("Google Vertex interactions models do not support Express Mode API keys. Use standard Google Cloud credentials instead.");
        }

        return base.Interactions(modelId);
    }

    /// <summary>Chirp speech models use Cloud Text-to-Speech. Gemini speech models use generateContent.</summary>
    public override ISpeechModel SpeechModel(string modelId)
    {
        if (modelId != null && modelId.StartsWith("chirp", StringComparison.Ordinal))
        {
            if (Vertex.UpstreamRoutes && !string.IsNullOrEmpty(Options.ApiKey))
            {
                throw new InvalidOperationException("Google Vertex Chirp speech models do not support Express Mode API keys. Use standard Google Cloud credentials instead.");
            }

            return new GoogleVertexCloudSpeechModel(this, modelId);
        }

        return new GoogleSpeechModel(this, modelId!);
    }

    /// <summary>Gemini transcription ids use generateContent. Other ids use Cloud Speech-to-Text.</summary>
    public override ITranscriptionModel TranscriptionModel(string modelId)
    {
        if (Vertex.UpstreamRoutes && !string.IsNullOrEmpty(Options.ApiKey))
        {
            throw new InvalidOperationException("Google Vertex transcription models do not support Express Mode API keys. Use standard Google Cloud credentials instead.");
        }

        if (modelId != null && modelId.StartsWith("gemini", StringComparison.Ordinal))
        {
            return new GoogleVertexGeminiTranscriptionModel(this, modelId);
        }

        return new GoogleVertexSpeechTranscriptionModel(this, modelId!);
    }

    private static VertexOptions Prepare(VertexOptions? options)
    {
        options ??= new VertexOptions();
        options.UseBearerToken = true;
        if (string.IsNullOrEmpty(options.Project))
        {
            options.Project = Environment.GetEnvironmentVariable("GOOGLE_VERTEX_PROJECT") ?? string.Empty;
        }

        if (!string.IsNullOrEmpty(options.BaseUrl))
        {
            options.BaseUrl = options.BaseUrl.TrimEnd('/');
            return options;
        }

        if (options.Region != null && !HostnameParts.IsValidHostnamePart(options.Region))
        {
            throw new ArgumentException("A Vertex location must be a single DNS label.", nameof(VertexOptions.Region));
        }

        if (string.IsNullOrEmpty(options.Region))
        {
            options.Region = Environment.GetEnvironmentVariable("GOOGLE_VERTEX_LOCATION") ?? "us-central1";
        }

        if (options.UpstreamRoutes)
        {
            var express = !string.IsNullOrEmpty(options.ApiKey);
            options.BaseUrl = GoogleVertexEndpoints.GeminiBaseUrl(options.Project, options.Region, null, express, false);
            if (express)
            {
                options.UseBearerToken = false;
            }

            return options;
        }

        if (!HostnameParts.IsValidHostnamePart(options.Region))
        {
            throw new ArgumentException("A Vertex location must be a single DNS label.", nameof(VertexOptions.Region));
        }

        options.BaseUrl = "https://" + options.Region + "-aiplatform.googleapis.com/v1/projects/" + options.Project
            + "/locations/" + options.Region + "/publishers/google";
        return options;
    }
}
