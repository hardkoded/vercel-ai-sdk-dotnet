// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vercel.AI.Groq;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Upstream.Groq.GroqTranscriptionModel;

/// <summary>Shared fixtures for the Groq transcription tests. Mirrors the top of <c>groq-transcription-model.test.ts</c>.</summary>
internal static class GroqTranscriptionTestSupport
{
    public const string Url = "https://api.groq.com/openai/v1/audio/transcriptions";

    public static readonly byte[] AudioData = { 1, 2, 3, 4 };

    public const string TextResponseBody = " Hello from the Versal AISDK.";

    public static string JsonBody(string? segments = null, string? words = null)
    {
        return "{\"task\":\"transcribe\",\"language\":\"English\",\"duration\":2.5,\"text\":\"Hello world!\","
            + "\"segments\":" + (segments ?? "[{\"id\":0,\"seek\":0,\"start\":0,\"end\":2.48,\"text\":\"Hello world!\",\"tokens\":[50365,2425,490,264],\"temperature\":0,\"avg_logprob\":-0.29010406,\"compression_ratio\":0.7777778,\"no_speech_prob\":0.032802984}]")
            + (words == null ? string.Empty : ",\"words\":" + words)
            + ",\"x_groq\":{\"id\":\"req_01jrh9nn61f24rydqq1r4b3yg5\"}}";
    }

    public static GroqProvider CreateProvider(ParityHandler handler, Action<OpenAICompatibleOptions>? configure = null)
    {
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        configure?.Invoke(options);
        return GroqProvider.Create(options, handler);
    }

    public static HttpResponseMessage Error(HttpStatusCode status, string message)
    {
        var content = new StringContent("{\"error\":{\"message\":\"" + message + "\",\"type\":\"invalid_request_error\"}}", Encoding.UTF8, "application/json");
        return new HttpResponseMessage(status) { Content = content };
    }

    /// <summary>Reads the multipart fields of a recorded request, keeping repeated names in order.</summary>
    public static List<KeyValuePair<string, string>> Multipart(ParityCall call)
    {
        var fields = new List<KeyValuePair<string, string>>();
        var pattern = new Regex("Content-Disposition: form-data; name=\"?(?<name>[^\";\\r\\n]+)\"?[^\\r\\n]*\\r\\n(?:[^\\r\\n]+\\r\\n)*\\r\\n(?<value>.*?)\\r\\n--", RegexOptions.Singleline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2));
        foreach (Match match in pattern.Matches(call.Text))
        {
            fields.Add(new KeyValuePair<string, string>(match.Groups["name"].Value, match.Groups["value"].Value));
        }

        return fields;
    }

    public static string? Field(ParityCall call, string name)
    {
        foreach (var field in Multipart(call))
        {
            if (field.Key == name)
            {
                return field.Value;
            }
        }

        return null;
    }
}
