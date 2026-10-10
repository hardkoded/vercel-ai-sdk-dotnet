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
        Http = new ProviderHttp(httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
    }

    /// <summary>Options.</summary>
    public OpenResponsesOptions Options { get; }

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

/// <summary>Posts to <c>/responses</c>.</summary>
public sealed class OpenResponsesLanguageModel : ILanguageModel
{
    private readonly OpenResponsesProvider _provider;

    /// <summary>Creates a model.</summary>
    public OpenResponsesLanguageModel(OpenResponsesProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => OpenResponsesProvider.ProviderName;

    /// <inheritdoc />
    public string ModelId { get; }

    private static readonly Dictionary<string, string> ReasoningEffortMap = new Dictionary<string, string>
    {
        { "minimal", "low" },
        { "low", "low" },
        { "medium", "medium" },
        { "high", "high" },
        { "xhigh", "xhigh" },
        { "max", "max" },
    };

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var input = new JsonArray();
        foreach (var message in options.Prompt)
        {
            if (message is UserModelMessage user)
            {
                var text = string.Empty;
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart textPart)
                    {
                        text += textPart.Text;
                    }
                }

                input.Add(new JsonObject { ["role"] = "user", ["content"] = text });
            }
            else if (message is SystemModelMessage system)
            {
                input.Add(new JsonObject { ["role"] = "system", ["content"] = system.Content });
            }
        }

        var warnings = new List<CallWarning>();
        string? providerEffort = null;
        string? providerSummary = null;
        if (options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue(OpenResponsesProvider.ProviderName, out var configured)
            && configured.ValueKind == JsonValueKind.Object)
        {
            if (configured.TryGetProperty("reasoningEffort", out var effortValue) && effortValue.ValueKind == JsonValueKind.String)
            {
                providerEffort = effortValue.GetString();
            }

            if (configured.TryGetProperty("reasoningSummary", out var summaryValue) && summaryValue.ValueKind == JsonValueKind.String)
            {
                providerSummary = summaryValue.GetString();
            }
        }

        var effort = providerEffort;
        if (effort == null && ReasoningMap.IsCustomReasoning(options.Reasoning))
        {
            if (options.Reasoning == "none")
            {
                effort = "none";
            }
            else
            {
                var mappingWarnings = new List<ModelWarning>();
                effort = ReasoningMap.MapReasoningToProviderEffort(options.Reasoning!, ReasoningEffortMap, mappingWarnings);
                foreach (var warning in mappingWarnings)
                {
                    warnings.Add(new CallWarning(warning.Type, (warning as CompatibilityWarning)?.Details ?? (warning as UnsupportedWarning)?.Details ?? string.Empty));
                }
            }
        }

        var body = new JsonObject { ["model"] = ModelId, ["input"] = input };
        if (effort != null || providerSummary != null)
        {
            var reasoning = new JsonObject();
            if (effort != null)
            {
                reasoning["effort"] = effort;
            }

            if (providerSummary != null)
            {
                reasoning["summary"] = providerSummary;
            }

            body["reasoning"] = reasoning;
        }

        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "responses"),
            body.ToJsonString(),
            new Dictionary<string, string?> { ["Authorization"] = "Bearer " + ApiKeys.Require(_provider.Options.ApiKey, _provider.Options.ApiKeyEnvironmentVariable) },
            cancellationToken).ConfigureAwait(false);
        var outputText = document.RootElement.TryGetProperty("output_text", out var output) ? output.GetString() ?? string.Empty : string.Empty;
        return new LanguageModelGenerateResult(new GeneratedContent[] { new GeneratedText(outputText) }, FinishReason.Stop, LanguageModelUsage.Empty, warnings: warnings);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = await DoGenerateAsync(options, cancellationToken).ConfigureAwait(false);
        yield return new TextDeltaStreamPart("text", result.Text);
        yield return new FinishStreamPart(result.FinishReason, result.Usage);
    }
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
