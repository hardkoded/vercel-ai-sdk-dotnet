// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.Mistral;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralTranscriptionModel;

/// <summary>One multipart part of a recorded request.</summary>
internal sealed record MultipartPart(
    string Name,
    string? FileName,
    string? ContentType,
    int Size,
    string Value
);

/// <summary>Shared fixtures for the Mistral transcription tests. Mirrors the top of <c>mistral-transcription-model.test.ts</c>.</summary>
internal static class MistralTranscriptionTestSupport
{
    public const string ModelId = "voxtral-mini-latest";
    public const string Url = "https://api.mistral.ai/v1/audio/transcriptions";
    public const string Commit = "486e08f0446a5986c2f6fd5bcf8c9b00770e51f7";

    public static readonly byte[] AudioData = { 0, 1, 2, 3, 4 };

    public static readonly string TranscriptionResponse = File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "mistral-transcription.json")
    );

    public static ParityHandler JsonHandler(
        string? body = null,
        params (string Name, string Value)[] headers
    )
    {
        return new ParityHandler(_ => ParityHandler.Json(body ?? TranscriptionResponse, headers));
    }

    public static MistralProvider CreateProvider(
        ParityHandler handler,
        Action<OpenAICompatibleOptions>? configure = null
    )
    {
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        configure?.Invoke(options);
        return MistralProvider.Create(options, handler);
    }

    public static ITranscriptionCaller Transcription(
        MistralProvider provider,
        string modelId = ModelId
    ) => (ITranscriptionCaller)provider.TranscriptionModel(modelId);

    public static TranscriptionModelCall Call(
        string? providerOptions = null,
        string mediaType = "audio/wav",
        string? audioBase64 = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default
    )
    {
        var options = JsonDocument.Parse(providerOptions ?? "{}").RootElement;
        return audioBase64 is null
            ? new TranscriptionModelCall(
                AudioData,
                mediaType,
                options,
                headers ?? new Dictionary<string, string>(),
                cancellationToken
            )
            : new TranscriptionModelCall(
                audioBase64,
                mediaType,
                options,
                headers ?? new Dictionary<string, string>(),
                cancellationToken
            );
    }

    /// <summary>Reads the multipart parts of a recorded request, keeping repeated names in order.</summary>
    public static List<MultipartPart> Multipart(ParityCall call)
    {
        var contentType = call.Header("content-type") ?? string.Empty;
        var boundary = Regex
            .Match(
                contentType,
                "boundary=\"?(?<b>[^\";]+)\"?",
                RegexOptions.CultureInvariant,
                TimeSpan.FromSeconds(1)
            )
            .Groups["b"]
            .Value;
        // Latin-1 keeps every byte as one character, so a part's size is its text length.
        var text = Encoding.Latin1.GetString(call.Body);
        var parts = new List<MultipartPart>();
        foreach (var raw in text.Split(new[] { "--" + boundary }, StringSplitOptions.None))
        {
            var separator = raw.IndexOf("\r\n\r\n", StringComparison.Ordinal);
            if (separator < 0)
            {
                continue;
            }

            var head = raw.Substring(0, separator);
            var value = raw.Substring(separator + 4);
            if (value.EndsWith("\r\n", StringComparison.Ordinal))
            {
                value = value.Substring(0, value.Length - 2);
            }

            var name = Regex.Match(
                head,
                "[; ]name=\"?(?<n>[^\";\\r\\n]*)\"?",
                RegexOptions.CultureInvariant,
                TimeSpan.FromSeconds(1)
            );
            var file = Regex.Match(
                head,
                "[; ]filename=\"?(?<f>[^\";\\r\\n]*)\"?",
                RegexOptions.CultureInvariant,
                TimeSpan.FromSeconds(1)
            );
            var type = Regex.Match(
                head,
                "Content-Type: (?<t>[^\\r\\n]+)",
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
                TimeSpan.FromSeconds(1)
            );
            parts.Add(
                new MultipartPart(
                    name.Groups["n"].Value,
                    file.Success ? file.Groups["f"].Value : null,
                    type.Success ? type.Groups["t"].Value : null,
                    value.Length,
                    Encoding.UTF8.GetString(Encoding.Latin1.GetBytes(value))
                )
            );
        }

        return parts;
    }

    public static string[] Values(ParityCall call, string name) =>
        Multipart(call).Where(part => part.Name == name).Select(part => part.Value).ToArray();

    public static string? Field(ParityCall call, string name) =>
        Multipart(call).FirstOrDefault(part => part.Name == name)?.Value;
}
