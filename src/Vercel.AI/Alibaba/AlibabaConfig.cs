// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Alibaba;

/// <summary>Configuration a provider passes to an Alibaba model. Maps to <c>AlibabaConfig</c>.</summary>
public sealed class AlibabaConfig
{
    /// <summary>Creates a configuration.</summary>
    public AlibabaConfig(
        string provider,
        string baseUrl,
        Func<CancellationToken, Task<Dictionary<string, string?>>> headers,
        HttpClient httpClient
    )
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        Headers = headers ?? throw new ArgumentNullException(nameof(headers));
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Provider id, such as <c>alibaba.embedding</c>.</summary>
    public string Provider { get; }

    /// <summary>API origin the model appends its route to.</summary>
    public string BaseUrl { get; }

    /// <summary>Resolves the request headers on every call (bearer token, custom headers and user agent).</summary>
    public Func<CancellationToken, Task<Dictionary<string, string?>>> Headers { get; }

    /// <summary>The client every request goes through (upstream <c>fetch</c>).</summary>
    public HttpClient HttpClient { get; }
}
