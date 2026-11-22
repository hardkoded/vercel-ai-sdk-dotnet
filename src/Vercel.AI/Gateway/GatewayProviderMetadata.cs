// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Gateway;

/// <summary>Metadata for an asynchronous AI Gateway job. Maps to <c>GatewayAsyncJobMetadata</c>.</summary>
public sealed class GatewayAsyncJobMetadata
{
    /// <summary>Gets or sets the job id.</summary>
    public string JobId { get; set; } = string.Empty;

    /// <summary>Gets or sets the job status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the secret for verifying customer webhook deliveries. This is sensitive, start-response-only
    /// metadata and must not be logged or forwarded.
    /// </summary>
    public string? WebhookSigningSecret { get; set; }

    /// <summary>Gets the other fields of the job metadata (the index signature of the upstream type).</summary>
    public IDictionary<string, JsonElement> AdditionalProperties { get; } =
        new Dictionary<string, JsonElement>();
}

/// <summary>
/// Shape of the inner <c>providerMetadata.gateway</c> object returned by AI Gateway operations. Maps to
/// <c>GatewayProviderMetadata</c>. Additional fields may be added without a package update.
/// </summary>
public sealed class GatewayProviderMetadata
{
    /// <summary>Gets or sets the asynchronous job metadata.</summary>
    public GatewayAsyncJobMetadata? AsyncJob { get; set; }

    /// <summary>Gets the other fields of the gateway metadata (the index signature of the upstream type).</summary>
    public IDictionary<string, JsonElement> AdditionalProperties { get; } =
        new Dictionary<string, JsonElement>();

    /// <summary>
    /// Reads the <c>gateway</c> entry of a provider metadata value, or returns <c>null</c> when it has none.
    /// </summary>
    public static GatewayProviderMetadata? From(JsonElement? providerMetadata)
    {
        if (
            providerMetadata is not { ValueKind: JsonValueKind.Object } root
            || !root.TryGetProperty("gateway", out var gateway)
            || gateway.ValueKind != JsonValueKind.Object
        )
        {
            return null;
        }

        var result = new GatewayProviderMetadata();
        foreach (var property in gateway.EnumerateObject())
        {
            if (
                property.Name == "asyncJob"
                && property.Value.ValueKind == JsonValueKind.Object
                && ReadAsyncJob(property.Value) is { } job
            )
            {
                result.AsyncJob = job;
            }
            else
            {
                result.AdditionalProperties[property.Name] = property.Value.Clone();
            }
        }

        return result;
    }

    private static GatewayAsyncJobMetadata? ReadAsyncJob(JsonElement element)
    {
        var job = new GatewayAsyncJobMetadata();
        foreach (var property in element.EnumerateObject())
        {
            var isString = property.Value.ValueKind == JsonValueKind.String;
            switch (property.Name)
            {
                case "jobId" when isString:
                    job.JobId = property.Value.GetString()!;
                    break;
                case "status" when isString:
                    job.Status = property.Value.GetString()!;
                    break;
                case "webhookSigningSecret" when isString:
                    job.WebhookSigningSecret = property.Value.GetString();
                    break;
                default:
                    job.AdditionalProperties[property.Name] = property.Value.Clone();
                    break;
            }
        }

        return job;
    }
}
