// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Vercel.AI.Gateway;

/// <summary>The primary aggregation dimension of a spend report.</summary>
public enum GatewaySpendReportGroupBy
{
    /// <summary>Group by day (<c>day</c>).</summary>
    Day,

    /// <summary>Group by user (<c>user</c>).</summary>
    User,

    /// <summary>Group by model (<c>model</c>).</summary>
    Model,

    /// <summary>Group by tag (<c>tag</c>).</summary>
    Tag,

    /// <summary>Group by provider (<c>provider</c>).</summary>
    Provider,

    /// <summary>Group by credential type (<c>credential_type</c>).</summary>
    CredentialType,
}

/// <summary>The time granularity of a spend report grouped by day.</summary>
public enum GatewaySpendReportDatePart
{
    /// <summary>One row per day (<c>day</c>).</summary>
    Day,

    /// <summary>One row per hour (<c>hour</c>).</summary>
    Hour,
}

/// <summary>The kind of credentials a request used.</summary>
public enum GatewayCredentialType
{
    /// <summary>Bring your own key (<c>byok</c>).</summary>
    Byok,

    /// <summary>System credentials (<c>system</c>).</summary>
    System,
}

/// <summary>Parameters of <see cref="GatewaySpendReport.GetSpendReportAsync"/>. Maps to <c>GatewaySpendReportParams</c>.</summary>
public sealed class GatewaySpendReportParams
{
    /// <summary>Gets or sets the start date in YYYY-MM-DD format (inclusive).</summary>
    public string StartDate { get; set; } = string.Empty;

    /// <summary>Gets or sets the end date in YYYY-MM-DD format (inclusive).</summary>
    public string EndDate { get; set; } = string.Empty;

    /// <summary>Gets or sets the primary aggregation dimension. The Gateway defaults to <c>day</c>.</summary>
    public GatewaySpendReportGroupBy? GroupBy { get; set; }

    /// <summary>Gets or sets the time granularity when <see cref="GroupBy"/> is <c>day</c>.</summary>
    public GatewaySpendReportDatePart? DatePart { get; set; }

    /// <summary>Gets or sets the user to filter the spend to.</summary>
    public string? UserId { get; set; }

    /// <summary>Gets or sets the model to filter the spend to (for example <c>anthropic/claude-sonnet-4.5</c>).</summary>
    public string? Model { get; set; }

    /// <summary>Gets or sets the provider to filter the spend to (for example <c>anthropic</c>).</summary>
    public string? Provider { get; set; }

    /// <summary>Gets or sets the credential type to filter the spend to.</summary>
    public GatewayCredentialType? CredentialType { get; set; }

    /// <summary>Gets or sets the tags to filter the spend to.</summary>
    public IReadOnlyList<string>? Tags { get; set; }
}

/// <summary>A row of a spend report. Maps to <c>GatewaySpendReportRow</c>.</summary>
public sealed class GatewaySpendReportRow
{
    /// <summary>Gets or sets the date string (present when grouped by day).</summary>
    public string? Day { get; set; }

    /// <summary>Gets or sets the hour timestamp (present when grouped by day with an hour date part).</summary>
    public string? Hour { get; set; }

    /// <summary>Gets or sets the user identifier (present when grouped by user).</summary>
    public string? User { get; set; }

    /// <summary>Gets or sets the model identifier (present when grouped by model).</summary>
    public string? Model { get; set; }

    /// <summary>Gets or sets the tag value (present when grouped by tag).</summary>
    public string? Tag { get; set; }

    /// <summary>Gets or sets the provider name (present when grouped by provider).</summary>
    public string? Provider { get; set; }

    /// <summary>Gets or sets the credential type (present when grouped by credential type).</summary>
    public GatewayCredentialType? CredentialType { get; set; }

    /// <summary>Gets or sets the total cost in USD.</summary>
    public double TotalCost { get; set; }

    /// <summary>Gets or sets the market cost in USD.</summary>
    public double? MarketCost { get; set; }

    /// <summary>Gets or sets the number of input tokens.</summary>
    public double? InputTokens { get; set; }

    /// <summary>Gets or sets the number of output tokens.</summary>
    public double? OutputTokens { get; set; }

    /// <summary>Gets or sets the number of cached input tokens.</summary>
    public double? CachedInputTokens { get; set; }

    /// <summary>Gets or sets the number of cache creation input tokens.</summary>
    public double? CacheCreationInputTokens { get; set; }

    /// <summary>Gets or sets the number of reasoning tokens.</summary>
    public double? ReasoningTokens { get; set; }

    /// <summary>Gets or sets the number of requests.</summary>
    public double? RequestCount { get; set; }
}

/// <summary>A spend report. Maps to <c>GatewaySpendReportResponse</c>.</summary>
public sealed class GatewaySpendReportResponse
{
    /// <summary>Gets or sets the rows.</summary>
    public IReadOnlyList<GatewaySpendReportRow> Results { get; set; } = [];
}

/// <summary>Fetches a spend report (<c>GET /v1/report</c>) from the Gateway.</summary>
public sealed class GatewaySpendReport
{
    private readonly GatewayConfig _config;

    /// <summary>Initializes a new instance of the <see cref="GatewaySpendReport"/> class.</summary>
    public GatewaySpendReport(GatewayConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>Gets the spend report of the account.</summary>
    public async Task<GatewaySpendReportResponse> GetSpendReportAsync(GatewaySpendReportParams parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var baseUrl = new Uri(_config.BaseUrl);
            var query = new StringBuilder();
            Add(query, "start_date", parameters.StartDate);
            Add(query, "end_date", parameters.EndDate);
            if (parameters.GroupBy is { } groupBy)
            {
                Add(query, "group_by", GroupByValue(groupBy));
            }

            if (parameters.DatePart is { } datePart)
            {
                Add(query, "date_part", datePart == GatewaySpendReportDatePart.Hour ? "hour" : "day");
            }

            if (!string.IsNullOrEmpty(parameters.UserId))
            {
                Add(query, "user_id", parameters.UserId!);
            }

            if (!string.IsNullOrEmpty(parameters.Model))
            {
                Add(query, "model", parameters.Model!);
            }

            if (!string.IsNullOrEmpty(parameters.Provider))
            {
                Add(query, "provider", parameters.Provider!);
            }

            if (parameters.CredentialType is { } credentialType)
            {
                Add(query, "credential_type", CredentialTypeValue(credentialType));
            }

            if (parameters.Tags is { Count: > 0 })
            {
                Add(query, "tags", string.Join(",", parameters.Tags));
            }

            var http = _config.Http ?? throw new InvalidOperationException("GatewayConfig.Http is required.");
            using var document = await http.SendJsonAsync(
                HttpMethod.Get,
                new Uri(baseUrl.GetLeftPart(UriPartial.Authority) + "/v1/report?" + query),
                null,
                _config.Headers?.Invoke(),
                cancellationToken).ConfigureAwait(false);
            return Parse(document.RootElement);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw GatewayErrors.AsGatewayError(exception);
        }
    }

    private static void Add(StringBuilder query, string name, string value)
    {
        if (query.Length > 0)
        {
            query.Append('&');
        }

        query.Append(name).Append('=').Append(Uri.EscapeDataString(value));
    }

    private static string GroupByValue(GatewaySpendReportGroupBy groupBy) => groupBy switch
    {
        GatewaySpendReportGroupBy.Day => "day",
        GatewaySpendReportGroupBy.User => "user",
        GatewaySpendReportGroupBy.Model => "model",
        GatewaySpendReportGroupBy.Tag => "tag",
        GatewaySpendReportGroupBy.Provider => "provider",
        GatewaySpendReportGroupBy.CredentialType => "credential_type",
        _ => throw new ArgumentOutOfRangeException(nameof(groupBy)),
    };

    private static string CredentialTypeValue(GatewayCredentialType credentialType) => credentialType switch
    {
        GatewayCredentialType.Byok => "byok",
        GatewayCredentialType.System => "system",
        _ => throw new ArgumentOutOfRangeException(nameof(credentialType)),
    };

    private static GatewaySpendReportResponse Parse(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
        {
            throw new FormatException("Type validation failed: expected an object with a results array.");
        }

        var rows = new List<GatewaySpendReportRow>();
        foreach (var row in results.EnumerateArray())
        {
            if (row.ValueKind != JsonValueKind.Object)
            {
                throw new FormatException("Type validation failed: a result must be an object.");
            }

            rows.Add(new GatewaySpendReportRow
            {
                Day = OptionalString(row, "day"),
                Hour = OptionalString(row, "hour"),
                User = OptionalString(row, "user"),
                Model = OptionalString(row, "model"),
                Tag = OptionalString(row, "tag"),
                Provider = OptionalString(row, "provider"),
                CredentialType = ParseCredentialType(OptionalString(row, "credential_type")),
                TotalCost = RequiredNumber(row, "total_cost"),
                MarketCost = OptionalNumber(row, "market_cost"),
                InputTokens = OptionalNumber(row, "input_tokens"),
                OutputTokens = OptionalNumber(row, "output_tokens"),
                CachedInputTokens = OptionalNumber(row, "cached_input_tokens"),
                CacheCreationInputTokens = OptionalNumber(row, "cache_creation_input_tokens"),
                ReasoningTokens = OptionalNumber(row, "reasoning_tokens"),
                RequestCount = OptionalNumber(row, "request_count"),
            });
        }

        return new GatewaySpendReportResponse { Results = rows };
    }

    private static GatewayCredentialType? ParseCredentialType(string? value) => value switch
    {
        null => null,
        "byok" => GatewayCredentialType.Byok,
        "system" => GatewayCredentialType.System,
        _ => throw new FormatException(string.Format(CultureInfo.InvariantCulture, "Type validation failed: invalid credential_type '{0}'.", value)),
    };

    private static string? OptionalString(JsonElement row, string name)
    {
        if (!row.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            throw new FormatException("Type validation failed: invalid " + name + ".");
        }

        return value.GetString();
    }

    private static double RequiredNumber(JsonElement row, string name)
    {
        return OptionalNumber(row, name) ?? throw new FormatException("Type validation failed: invalid or missing " + name + ".");
    }

    private static double? OptionalNumber(JsonElement row, string name)
    {
        if (!row.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.Number)
        {
            throw new FormatException("Type validation failed: invalid " + name + ".");
        }

        return value.GetDouble();
    }
}
