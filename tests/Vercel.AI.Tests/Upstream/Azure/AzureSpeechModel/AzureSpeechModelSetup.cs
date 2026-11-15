// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Vercel.AI.Azure;
using Vercel.AI.OpenAI;

namespace Vercel.AI.Tests.Upstream.Azure.AzureSpeechModel;

/// <summary>Port of <c>setup</c>, <c>ssml</c> and the shared constants of <c>azure-speech-model.test.ts</c>.</summary>
internal sealed class AzureSpeechModelSetup
{
    public const string SpeechUrl = "https://test-resource.cognitiveservices.azure.com/tts/cognitiveservices/v1";

    public static readonly byte[] Audio = [0xff, 0xf3, 0x44, 0xc4];

    private AzureSpeechModelSetup(AzureOpenAIProvider provider, RecordingHandler handler)
    {
        Provider = provider;
        Handler = handler;
    }

    public AzureOpenAIProvider Provider { get; }

    public RecordingHandler Handler { get; }

    public (string Url, Dictionary<string, string> Headers, string Body) Request => (Handler.Url, Handler.Headers, Handler.Body);

    public static AzureSpeechModelSetup Create(Action<AzureOpenAIOptions>? configure = null, Func<HttpResponseMessage>? response = null)
    {
        var handler = new RecordingHandler(response ?? DefaultResponse);
        var options = new AzureOpenAIOptions { ResourceName = "test-resource", ApiKey = "test-key" };
        configure?.Invoke(options);
        return new AzureSpeechModelSetup(AzureOpenAIProvider.Create(options, handler), handler);
    }

    public static string Ssml(string voice, string content, string locale = "en-US")
    {
        return "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\" xml:lang=\""
            + locale + "\"><voice name=\"" + voice + "\">" + content + "</voice></speak>";
    }

    public static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public static OpenAISpeechCall Call(string text, string? providerOptions = null)
    {
        return new OpenAISpeechCall(text) { ProviderOptions = providerOptions == null ? null : Json(providerOptions) };
    }

    private static HttpResponseMessage DefaultResponse()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Audio) };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
        return response;
    }

    internal sealed class RecordingHandler(Func<HttpResponseMessage> response) : HttpMessageHandler
    {
        public int Calls { get; private set; }

        public string Url { get; private set; } = string.Empty;

        public string Body { get; private set; } = string.Empty;

        public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Url = request.RequestUri?.AbsoluteUri ?? string.Empty;
            Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            Headers.Clear();
            foreach (var header in request.Headers)
            {
                Headers[header.Key] = string.Join(",", header.Value);
            }

            if (request.Content != null)
            {
                foreach (var header in request.Content.Headers)
                {
                    Headers[header.Key] = string.Join(",", header.Value);
                }
            }

            if (request.Headers.Authorization != null)
            {
                Headers["Authorization"] = request.Headers.Authorization.ToString();
            }

            return response();
        }
    }
}
