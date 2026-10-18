// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>A test server for <c>https://api.test.com/language-model</c>. It records each request and answers with a canned response.</summary>
internal sealed class GatewayLanguageModelServer : HttpMessageHandler
{
    public string ResponseBody { get; set; } = string.Empty;

    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

    public string ContentType { get; set; } = "text/event-stream";

    /// <summary>When set, the request throws this exception, as a failing <c>fetch</c> does.</summary>
    public Exception? Failure { get; set; }

    /// <summary>Runs while the request is handled, before the response is returned.</summary>
    public Action? BeforeRespond { get; set; }

    public int Calls { get; private set; }

    public string RequestBody { get; private set; } = string.Empty;

    public Dictionary<string, string> RequestHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

    public CancellationToken RequestToken { get; private set; }

    public JsonNode RequestJson => JsonNode.Parse(RequestBody)!;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        RequestToken = cancellationToken;
        RequestBody = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        foreach (var header in request.Headers)
        {
            RequestHeaders[header.Key] = string.Join(",", header.Value);
        }

        BeforeRespond?.Invoke();
        cancellationToken.ThrowIfCancellationRequested();
        if (Failure != null)
        {
            throw Failure;
        }

        var content = new StringContent(ResponseBody, Encoding.UTF8);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse(ContentType);
        return new HttpResponseMessage(Status) { Content = content };
    }
}

/// <summary>Helpers shared by the <c>gateway-language-model.test.ts</c> doStream ports.</summary>
internal static class GatewayLanguageModelSupport
{
    internal static readonly LanguageModelCallOptions TestPrompt = new() { Prompt = new ModelMessage[] { new UserModelMessage("Hello") } };

    internal static ILanguageModel CreateTestModel(HttpMessageHandler handler, IReadOnlyDictionary<string, string>? o11yHeaders = null)
        => GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com", O11yHeaders = o11yHeaders ?? new Dictionary<string, string>() }, handler).LanguageModel("test-model");

    /// <summary>Builds the server response of upstream <c>prepareStreamResponse</c>.</summary>
    internal static GatewayLanguageModelServer PrepareStreamResponse(params string[] content)
    {
        var body = new StringBuilder();
        foreach (var text in content)
        {
            body.Append("data: {\"type\":\"text-delta\",\"textDelta\":\"").Append(text).Append("\"}\n\n");
        }

        body.Append("data: {\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":20}}\n\n");
        return new GatewayLanguageModelServer { ResponseBody = body.ToString() };
    }

    internal static GatewayLanguageModelServer StreamChunks(params string[] chunks)
        => new() { ResponseBody = string.Concat(chunks.Select(chunk => "data: " + chunk + "\n\n")) };

    internal static async Task<List<LanguageModelStreamPart>> ReadAll(LanguageModelStreamResult result)
    {
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in result)
        {
            parts.Add(part);
        }

        return parts;
    }
}
