// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Azure;

/// <summary>One request the Azure transcription tests recorded. Maps to the <c>fetch</c> mock calls.</summary>
internal sealed class RecordedAzureRequest
{
    public string Url { get; init; } = string.Empty;

    public Dictionary<string, string> Headers { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, RecordedPart> Parts { get; init; } = new(StringComparer.Ordinal);
}

internal sealed class RecordedPart
{
    public string? FileName { get; init; }

    public string? ContentType { get; init; }

    public byte[] Bytes { get; init; } = Array.Empty<byte>();

    public string Text => Encoding.UTF8.GetString(Bytes);
}

/// <summary>Records requests and answers like the <c>fetch</c> mock of <c>azure-transcription-model.test.ts</c>.</summary>
internal sealed class AzureTranscriptionHandler : HttpMessageHandler
{
    public AzureTranscriptionHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        Responder = responder;
    }

    public Func<HttpRequestMessage, HttpResponseMessage> Responder { get; set; }

    public List<RecordedAzureRequest> Requests { get; } = new();

    public RecordedAzureRequest Last => Requests[^1];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var record = new RecordedAzureRequest { Url = request.RequestUri!.AbsoluteUri };
        foreach (var header in request.Headers)
        {
            record.Headers[header.Key] = string.Join(",", header.Value);
        }

        if (request.Content is MultipartFormDataContent form)
        {
            foreach (var part in form)
            {
                var name = part.Headers.ContentDisposition!.Name!.Trim('"');
                record.Parts[name] = new RecordedPart
                {
                    FileName = part.Headers.ContentDisposition.FileName?.Trim('"'),
                    ContentType = part.Headers.ContentType?.MediaType,
                    Bytes = await part.ReadAsByteArrayAsync(cancellationToken),
                };
            }
        }

        lock (Requests)
        {
            Requests.Add(record);
        }

        return Responder(request);
    }
}

internal sealed class AzureTranscriptionHarness
{
    public AzureTranscriptionHarness(AzureOpenAIProvider provider, AzureTranscriptionHandler handler)
    {
        Provider = provider;
        Handler = handler;
    }

    public AzureOpenAIProvider Provider { get; }

    public AzureTranscriptionHandler Handler { get; }

    public RecordedAzureRequest Request => Handler.Last;

    public JsonNode Definition => JsonNode.Parse(Request.Parts["definition"].Text)!;

    public ITranscriptionCaller Transcription(string modelId) => Provider.Transcription(modelId);
}

internal static class AzureTranscriptionTestSupport
{
    public static readonly byte[] Input = { 1, 2, 3 };

    public static readonly JsonObject Response = (JsonObject)JsonNode.Parse(
        """
        {
          "durationMilliseconds": 2000,
          "combinedPhrases": [{ "text": "Hello world." }],
          "phrases": [
            {
              "text": "Hello world.",
              "offsetMilliseconds": 40,
              "durationMilliseconds": 1500,
              "locale": "en-US",
              "speaker": 1,
              "confidence": 0.9,
              "words": [{ "text": "Hello", "offsetMilliseconds": 40, "durationMilliseconds": 400 }]
            }
          ]
        }
        """)!;

    public static AzureTranscriptionHarness Setup(Action<AzureOpenAIOptions>? configure = null, JsonNode? body = null, bool defaultResource = true)
    {
        var handler = new AzureTranscriptionHandler(request =>
        {
            var openai = request.RequestUri!.AbsoluteUri.Contains("/audio/transcriptions", StringComparison.Ordinal);
            return Json(openai ? "{\"text\":\"OpenAI\"}" : (body ?? Response).ToJsonString());
        });
        var options = new AzureOpenAIOptions { ApiKey = "test-key" };
        if (defaultResource)
        {
            options.ResourceName = "test-resource";
        }

        configure?.Invoke(options);
        return new AzureTranscriptionHarness(AzureOpenAIProvider.Create(options, handler), handler);
    }

    public static HttpResponseMessage Json(string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
    }

    public static TranscriptionModelCall Call(
        string? providerOptions = null,
        byte[]? audio = null,
        string[]? timestampGranularities = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        return new TranscriptionModelCall(
            audio ?? Input,
            "audio/wav",
            JsonSerializer.Deserialize<JsonElement>(providerOptions ?? "{}"),
            headers ?? new Dictionary<string, string>(),
            cancellationToken)
        {
            TimestampGranularities = timestampGranularities,
        };
    }

    public static string Fixture(string name)
    {
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name + ".json"));
    }

    public static IReadOnlyList<(string Type, string? Feature, string? Details)> Warnings(TranscriptionModelResult result)
    {
        return result.Warnings.Select(warning => (warning.Type, warning.Feature, warning.Details)).ToList();
    }

    public static void AssertJson(string expected, JsonNode? actual)
    {
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual?.ToJsonString());
    }
}
