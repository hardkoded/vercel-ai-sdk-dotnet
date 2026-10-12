// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.OpenResponses;

/// <summary>Open Responses settings. The default origin is the AI Gateway Open Responses route.</summary>
public sealed class OpenResponsesOptions
{
    /// <summary>API origin. Requests are posted to <c>{BaseUrl}/responses</c>.</summary>
    public string BaseUrl { get; set; } = "https://ai-gateway.vercel.sh/v1";

    /// <summary>Explicit key.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Environment variable.</summary>
    public string ApiKeyEnvironmentVariable { get; set; } = "AI_GATEWAY_API_KEY";

    /// <summary>Key of this provider in provider options and provider metadata.</summary>
    public string Name { get; set; } = OpenResponsesProvider.ProviderName;

    /// <summary>Extra headers sent with every request.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Whether to serialize assistant history using the strict OpenAI Responses input schemas. Assistant messages without an item id
    /// are sent as easy input messages, while messages with an item id are sent as complete output items.
    /// </summary>
    public bool StrictResponseInput { get; set; }

    /// <summary>Codecs for Open Responses extension tools, items, and streaming events. Experimental.</summary>
    public IReadOnlyList<OpenResponsesExtension>? Extensions { get; set; }
}

/// <summary>Open Responses provider.</summary>
public sealed class OpenResponsesProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderName = "open-responses";

    /// <summary>Creates a provider.</summary>
    public OpenResponsesProvider(HttpClient httpClient, OpenResponsesOptions? options = null)
        : base(ProviderName)
    {
        Options = options ?? new OpenResponsesOptions();
        ExtensionRegistry = OpenResponsesExtensions.CreateRegistry(Options.Extensions);
        Http = new ProviderHttp(httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
    }

    /// <summary>Options.</summary>
    public OpenResponsesOptions Options { get; }

    /// <summary>The registered extensions, indexed.</summary>
    public OpenResponsesExtensionRegistry ExtensionRegistry { get; }

    /// <summary>HTTP helper.</summary>
    public ProviderHttp Http { get; }

    /// <summary>Creates a provider.</summary>
    public static OpenResponsesProvider Create(OpenResponsesOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new OpenResponsesProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId) => new OpenResponsesLanguageModel(this, modelId);
}

/// <summary>Registers Open Responses.</summary>
public static class OpenResponsesServiceCollectionExtensions
{
    /// <summary>Adds <see cref="OpenResponsesProvider"/>.</summary>
    public static IServiceCollection AddOpenResponses(this IServiceCollection services, Action<OpenResponsesOptions>? configure = null)
    {
        services.AddHttpClient(OpenResponsesProvider.ProviderName);
        services.AddSingleton(sp =>
        {
            var options = new OpenResponsesOptions();
            configure?.Invoke(options);
            return new OpenResponsesProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(OpenResponsesProvider.ProviderName), options);
        });
        return services;
    }
}
