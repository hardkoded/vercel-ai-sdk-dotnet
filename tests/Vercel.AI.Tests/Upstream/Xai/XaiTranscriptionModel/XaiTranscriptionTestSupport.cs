// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.ProviderUtils;
using Vercel.AI.Xai;

namespace Vercel.AI.Tests.Upstream.Xai.XaiTranscriptionModel;

/// <summary>Helpers of <c>xai-transcription-model.test.ts</c>.</summary>
internal static class XaiTranscriptionTestSupport
{
    public const string Url = "https://api.x.ai/v1/stt";

    public static readonly byte[] AudioData = { 1, 2, 3, 4 };

    private static readonly Regex FieldName = new Regex(
        "^Content-Disposition: form-data; name=\"?([^\";\\r\\n]+)\"?",
        RegexOptions.Multiline | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1)
    );

    /// <summary>Maps to <c>prepareJsonResponse</c>.</summary>
    public static UpstreamRecordingHandler PrepareJsonResponse(
        params (string Name, string Value)[] headers
    )
    {
        var handler = new UpstreamRecordingHandler(
            """
            {"text":"Hello from the AI SDK!","language":"en","duration":2.5,"words":[{"text":"Hello","start":0,"end":1},{"text":"from the AI SDK!","start":1,"end":2.5}]}
            """
        );
        foreach (var (name, value) in headers)
        {
            handler.ResponseHeaders[name] = value;
        }

        return handler;
    }

    public static Vercel.AI.Xai.XaiProvider CreateProvider(
        HttpMessageHandler? handler = null,
        Action<XaiOptions>? configure = null
    )
    {
        var options = new XaiOptions { ApiKey = "test-api-key" };
        configure?.Invoke(options);
        return Vercel.AI.Xai.XaiProvider.Create(options, handler);
    }

    public static Vercel.AI.Xai.XaiTranscriptionModel Model(
        HttpMessageHandler? handler = null,
        string modelId = "",
        Action<XaiOptions>? configure = null,
        Func<DateTimeOffset>? clock = null
    )
    {
        return new Vercel.AI.Xai.XaiTranscriptionModel(
            CreateProvider(handler, configure),
            modelId,
            clock
        );
    }

    public static Vercel.AI.Xai.XaiTranscriptionModel StreamingModel(
        string modelId = "",
        Action<XaiOptions>? configure = null,
        Func<DateTimeOffset>? clock = null
    )
    {
        return Model(
            null,
            modelId,
            options =>
            {
                options.WebSocket = MockWebSocket.Factory;
                configure?.Invoke(options);
            },
            clock
        );
    }

    public static Task<TranscriptionModelResult> DoGenerate(
        ITranscriptionCaller model,
        string mediaType = "audio/wav",
        string providerOptions = "{}",
        IReadOnlyList<string>? timestampGranularities = null,
        IReadOnlyDictionary<string, string>? headers = null
    )
    {
        using var options = JsonDocument.Parse(providerOptions);
        return model.DoGenerateAsync(
            new TranscriptionModelCall(
                AudioData,
                mediaType,
                options.RootElement.Clone(),
                headers ?? new Dictionary<string, string>(),
                CancellationToken.None
            )
            {
                TimestampGranularities = timestampGranularities,
            },
            CancellationToken.None
        );
    }

    /// <summary>The multipart field names of a recorded request body, in order.</summary>
    public static List<string> FieldNames(string body)
    {
        return FieldName.Matches(body).Select(match => match.Groups[1].Value).ToList();
    }

    public static AudioChunkStream Audio(params byte[][] chunks)
    {
        var stream = new AudioChunkStream();
        foreach (var chunk in chunks.Length == 0 ? new[] { new byte[] { 1, 2, 3 } } : chunks)
        {
            stream.Write(chunk);
        }

        stream.Complete();
        return stream;
    }

    public static Task<TranscriptionStreamStart?> DoStream(
        ITranscriptionCaller model,
        AudioChunkStream? audio = null,
        AudioFormat? format = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string?>? headers = null
    )
    {
        using var options = JsonDocument.Parse(providerOptions);
        return model.DoStreamAsync(
            new TranscriptionStreamCall(
                audio ?? Audio(),
                format ?? new AudioFormat("audio/pcm", 16000),
                options.RootElement.Clone(),
                headers ?? new Dictionary<string, string?>(),
                null,
                CancellationToken.None
            ),
            CancellationToken.None
        );
    }

    public static async Task<List<ModelStreamPart>> Collect(TranscriptionStreamStart start)
    {
        var parts = new List<ModelStreamPart>();
        await foreach (var part in start.Stream.ReadAsync())
        {
            parts.Add(part);
        }

        return parts;
    }

    public static IReadOnlyList<string> Values(Uri url, string name)
    {
        return url
            .Query.TrimStart('?')
            .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(pair => pair.Split(new[] { '=' }, 2))
            .Where(pair => Uri.UnescapeDataString(pair[0].Replace('+', ' ')) == name)
            .Select(pair => Uri.UnescapeDataString(pair[1].Replace('+', ' ')))
            .ToList();
    }

    public static bool HasParameter(Uri url, string name) => Values(url, name).Count > 0;

    public static JsonObject Json(string text) => (JsonObject)JsonNode.Parse(text)!;
}
