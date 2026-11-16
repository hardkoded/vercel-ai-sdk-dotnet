// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Gateway;

/// <summary>The models the Gateway offers. Maps to <c>GatewayFetchMetadataResponse</c>.</summary>
public sealed class GatewayFetchMetadataResponse
{
    /// <summary>Gets or sets the models.</summary>
    public IReadOnlyList<GatewayLanguageModelEntry> Models { get; set; } = [];
}

/// <summary>The Gateway credit balance. Maps to <c>GatewayCreditsResponse</c>.</summary>
public sealed class GatewayCreditsResponse
{
    /// <summary>Gets or sets the remaining gateway credit balance available for API usage.</summary>
    public string Balance { get; set; } = string.Empty;

    /// <summary>Gets or sets the total amount of gateway credits that have been consumed.</summary>
    public string TotalUsed { get; set; } = string.Empty;
}

/// <summary>Fetches the available models (<c>GET /config</c>) and the credits (<c>GET /v1/credits</c>) from the Gateway.</summary>
public sealed class GatewayFetchMetadata
{
    private readonly GatewayConfig _config;

    /// <summary>Initializes a new instance of the <see cref="GatewayFetchMetadata"/> class.</summary>
    public GatewayFetchMetadata(GatewayConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>Gets the models the Gateway offers.</summary>
    public async Task<GatewayFetchMetadataResponse> GetAvailableModelsAsync(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var http = _config.Http
                ?? throw new InvalidOperationException("GatewayConfig.Http is required.");
            using var document = await http.SendJsonAsync(
                    HttpMethod.Get,
                    new Uri(_config.BaseUrl + "/config"),
                    null,
                    await _config.ResolveHeadersAsync(cancellationToken).ConfigureAwait(false),
                    cancellationToken
                )
                .ConfigureAwait(false);
            return ParseAvailableModels(document.RootElement);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw GatewayErrors.AsGatewayError(exception);
        }
    }

    /// <summary>Gets the credit balance of the account.</summary>
    public async Task<GatewayCreditsResponse> GetCreditsAsync(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var baseUrl = new Uri(_config.BaseUrl);
            var headers = await _config
                .ResolveHeadersAsync(cancellationToken)
                .ConfigureAwait(false);

            // The credits endpoint selects the team from the `teamId` / `slug`
            // query parameter rather than the team header, so forward it there.
            // Without it, tokens that can access multiple teams (e.g. Vercel access
            // tokens) cannot be scoped to a team and the request is rejected.
            var url = baseUrl.GetLeftPart(UriPartial.Authority) + "/v1/credits";
            var teamIdOrSlug = GetTeamIdOrSlug(headers);
            if (teamIdOrSlug != null)
            {
                url +=
                    (
                        teamIdOrSlug.StartsWith("team_", StringComparison.Ordinal)
                            ? "?teamId="
                            : "?slug="
                    ) + Uri.EscapeDataString(teamIdOrSlug);
            }

            var http = _config.Http
                ?? throw new InvalidOperationException("GatewayConfig.Http is required.");
            using var document = await http.SendJsonAsync(
                    HttpMethod.Get,
                    new Uri(url),
                    null,
                    headers,
                    cancellationToken
                )
                .ConfigureAwait(false);
            return ParseCredits(document.RootElement);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw GatewayErrors.AsGatewayError(exception);
        }
    }

    private static string? GetTeamIdOrSlug(IReadOnlyDictionary<string, string?>? headers)
    {
        if (headers == null)
        {
            return null;
        }

        foreach (var pair in headers)
        {
            if (
                string.Equals(
                    pair.Key,
                    GatewayErrors.TeamHeader,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                var value = pair.Value?.Trim();
                return string.IsNullOrEmpty(value) ? null : value;
            }
        }

        return null;
    }

    private static GatewayCreditsResponse ParseCredits(JsonElement root)
    {
        return new GatewayCreditsResponse
        {
            Balance = RequiredString(root, "balance"),
            TotalUsed = RequiredString(root, "total_used"),
        };
    }

    private static GatewayFetchMetadataResponse ParseAvailableModels(JsonElement root)
    {
        if (
            root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("models", out var models)
            || models.ValueKind != JsonValueKind.Array
        )
        {
            throw new FormatException(
                "Type validation failed: expected an object with a models array."
            );
        }

        var entries = new List<GatewayLanguageModelEntry>();
        foreach (var model in models.EnumerateArray())
        {
            var entry = ParseEntry(model);

            // Models of a type this SDK does not know are dropped.
            if (
                entry.ModelType == null
                || GatewayLanguageModelEntry.KnownModelTypes.Contains(entry.ModelType)
            )
            {
                entries.Add(entry);
            }
        }

        return new GatewayFetchMetadataResponse { Models = entries };
    }

    private static GatewayLanguageModelEntry ParseEntry(JsonElement model)
    {
        if (model.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("Type validation failed: a model must be an object.");
        }

        var specification = RequiredObject(model, "specification");
        var specificationVersion = RequiredString(specification, "specificationVersion");
        if (specificationVersion != "v4")
        {
            throw new FormatException("Type validation failed: specificationVersion must be v4.");
        }

        return new GatewayLanguageModelEntry
        {
            Id = RequiredString(model, "id"),
            Name = RequiredString(model, "name"),
            Description = OptionalString(model, "description"),
            Pricing = ParsePricing(model),
            Specification = new GatewayLanguageModelSpecification
            {
                SpecificationVersion = specificationVersion,
                Provider = RequiredString(specification, "provider"),
                ModelId = RequiredString(specification, "modelId"),
            },
            ModelType = OptionalString(model, "modelType"),
        };
    }

    private static GatewayModelPricing? ParsePricing(JsonElement model)
    {
        if (
            !model.TryGetProperty("pricing", out var pricing)
            || pricing.ValueKind == JsonValueKind.Null
        )
        {
            return null;
        }

        if (pricing.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("Type validation failed: invalid pricing.");
        }

        // The cache prices are only kept when they are not empty, under the SDK names.
        var cacheRead = OptionalString(pricing, "input_cache_read");
        var cacheWrite = OptionalString(pricing, "input_cache_write");
        return new GatewayModelPricing
        {
            Input = RequiredString(pricing, "input"),
            Output = RequiredString(pricing, "output"),
            CachedInputTokens = string.IsNullOrEmpty(cacheRead) ? null : cacheRead,
            CacheCreationInputTokens = string.IsNullOrEmpty(cacheWrite) ? null : cacheWrite,
        };
    }

    private static JsonElement RequiredObject(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("Type validation failed: invalid or missing " + name + ".");
        }

        return value;
    }

    private static string RequiredString(JsonElement parent, string name)
    {
        if (
            parent.ValueKind != JsonValueKind.Object
            || !parent.TryGetProperty(name, out var value)
            || value.ValueKind != JsonValueKind.String
        )
        {
            throw new FormatException("Type validation failed: invalid or missing " + name + ".");
        }

        return value.GetString()!;
    }

    private static string? OptionalString(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            throw new FormatException("Type validation failed: invalid " + name + ".");
        }

        return value.GetString();
    }
}
