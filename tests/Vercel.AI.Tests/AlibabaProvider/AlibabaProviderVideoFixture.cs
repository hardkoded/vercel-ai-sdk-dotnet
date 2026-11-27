// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Tests.AlibabaVideoModel;
using AlibabaProviderType = Vercel.AI.Alibaba.AlibabaProvider;

namespace Vercel.AI.Tests.AlibabaProvider;

/// <summary>
/// Helpers for the <c>video</c> and <c>videoModel</c> describes of <c>alibaba-provider.test.ts</c>. Upstream mocks the
/// <c>AlibabaVideoModel</c> constructor and reads its config. Here the provider runs against a test server, and the
/// tests read the config through the request it sends: the base URL from the request URL, <c>fetch</c> from the
/// handler that received it, and <c>headers</c> from its headers.
/// </summary>
internal static class AlibabaProviderVideoFixture
{
    public static AlibabaProviderType CreateAlibaba(
        AlibabaVideoServer server,
        string? videoBaseUrl = null,
        IReadOnlyDictionary<string, string>? headers = null
    )
    {
        var options = new OpenAICompatibleOptions
        {
            ApiKey = "test-key",
            VideoBaseUrl = videoBaseUrl,
        };
        foreach (var header in headers ?? new Dictionary<string, string>())
        {
            options.Headers[header.Key] = header.Value;
        }

        return AlibabaProviderType.Create(options, server);
    }

    /// <summary>Starts a task, so the model sends one request with its config.</summary>
    public static Task<VideoStartResult> StartAsync(object model)
    {
        return ((IVideoCaller)model).DoStartAsync(
            new VideoModelCall(
                "prompt",
                1,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                JsonDocument.Parse("{}").RootElement.Clone(),
                new Dictionary<string, string>(),
                CancellationToken.None
            ),
            CancellationToken.None
        );
    }

    /// <summary>The create-task URL under <paramref name="baseUrl"/>.</summary>
    public static string CreateUrl(string baseUrl)
    {
        return baseUrl + "/api/v1/services/aigc/video-generation/video-synthesis";
    }
}
