// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Captures one Gemini HTTP call and returns a scripted body.</summary>
internal sealed class GoogleCapture : HttpMessageHandler
{
    /// <summary>Last request URL.</summary>
    public Uri? Url { get; private set; }

    /// <summary>Last JSON request body.</summary>
    public string? Body { get; private set; }

    /// <summary>Last request.</summary>
    public HttpRequestMessage? Request { get; private set; }

    /// <summary>JSON response. Ignored when <see cref="Sse"/> is set.</summary>
    public string ResponseJson { get; set; } = "{\"candidates\":[{\"content\":{\"role\":\"model\",\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\"}]}";

    /// <summary>SSE payload, when the call is a stream.</summary>
    public string? Sse { get; set; }

    /// <summary>HTTP status.</summary>
    public int Status { get; set; } = 200;

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Url = request.RequestUri;
        Request = request;
        Body = request.Content == null ? null : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        var response = new HttpResponseMessage((HttpStatusCode)Status);
        if (Sse != null)
        {
            response.Content = new StringContent(Sse, Encoding.UTF8, "text/event-stream");
        }
        else
        {
            response.Content = new StringContent(ResponseJson, Encoding.UTF8, "application/json");
        }

        return response;
    }
}

/// <summary>Shared assertions for Google parity tests.</summary>
internal static class GoogleParity
{
    /// <summary>Parses JSON.</summary>
    public static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    /// <summary>Compares a JSON node with an expected document.</summary>
    public static void Equal(JsonNode? actual, string expected)
    {
        var parsed = JsonNode.Parse(expected);
        Assert.True(JsonNode.DeepEquals(actual, parsed), "Expected " + expected + " but was " + (actual == null ? "null" : actual.ToJsonString()));
    }

    /// <summary>Asserts contents and an optional system instruction.</summary>
    public static void Prompt(GooglePromptConversion conversion, string contents, string? systemInstruction = null)
    {
        Equal(conversion.Contents, contents);
        if (systemInstruction == null)
        {
            Assert.Null(conversion.SystemInstruction);
        }
        else
        {
            Equal(conversion.SystemInstruction, systemInstruction);
        }
    }

    /// <summary>Parses the captured request body.</summary>
    public static JsonNode Request(GoogleCapture capture)
    {
        Assert.False(string.IsNullOrEmpty(capture.Body));
        return JsonNode.Parse(capture.Body!)!;
    }

    /// <summary>Creates a Google provider bound to <paramref name="capture"/>.</summary>
    public static GoogleProvider Google(GoogleCapture capture)
    {
        return GoogleProvider.Create(
            new GoogleOptions
            {
                ApiKey = "test-key",
                GenerateId = () => "id-1",
            },
            capture);
    }

    /// <summary>Creates a Vertex provider bound to <paramref name="capture"/>.</summary>
    public static GoogleVertexProvider Vertex(GoogleCapture capture)
    {
        return GoogleVertexProvider.Create(
            new VertexOptions
            {
                ApiKey = "token",
                Project = "proj",
                Region = "us-central1",
                GenerateId = () => "id-1",
            },
            capture);
    }

    /// <summary>Posts one generateContent call.</summary>
    public static Task<LanguageModelGenerateResult> Generate(GoogleCapture capture, string modelId, LanguageModelCallOptions options, bool vertex = false)
    {
        ILanguageModel model = vertex
            ? Vertex(capture).LanguageModel(modelId)
            : Google(capture).LanguageModel(modelId);
        return model.DoGenerateAsync(options, CancellationToken.None);
    }

    /// <summary>Reads a generateContent stream.</summary>
    public static async Task<List<LanguageModelStreamPart>> Stream(GoogleCapture capture, string modelId, LanguageModelCallOptions options, bool vertex = false)
    {
        ILanguageModel model = vertex
            ? Vertex(capture).LanguageModel(modelId)
            : Google(capture).LanguageModel(modelId);
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in model.DoStreamAsync(options, CancellationToken.None))
        {
            parts.Add(part);
        }

        return parts;
    }

    /// <summary>A user prompt.</summary>
    public static LanguageModelCallOptions Prompt(string text = "hi")
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage(text) },
        };
    }

    /// <summary>Google provider options.</summary>
    public static Dictionary<string, JsonElement> GoogleOptions(string json)
    {
        return new Dictionary<string, JsonElement> { ["google"] = Json(json) };
    }

    /// <summary>A function tool.</summary>
    public static GoogleToolSpec Function(string name, string? description, string schema, bool? strict = null)
    {
        return new GoogleToolSpec(name, description, Json(schema), strict);
    }

    /// <summary>A provider tool.</summary>
    public static GoogleToolSpec ProviderTool(string id, string? name, string args = "{}")
    {
        return new GoogleToolSpec(id, name, Json(args));
    }

    /// <summary>Header value, or null.</summary>
    public static string? Header(GoogleCapture capture, string name)
    {
        Assert.NotNull(capture.Request);
        if (capture.Request!.Headers.TryGetValues(name, out var values))
        {
            return string.Join(",", values);
        }

        if (capture.Request.Content != null && capture.Request.Content.Headers.TryGetValues(name, out var content))
        {
            return string.Join(",", content);
        }

        return null;
    }
}
