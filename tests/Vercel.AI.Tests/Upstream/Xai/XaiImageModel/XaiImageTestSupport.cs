// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Xai;
using Model = Vercel.AI.Xai.XaiImageModel;

namespace Vercel.AI.Tests.Upstream.Xai.XaiImageModel;

/// <summary>Shared fixtures for the xAI image tests. Mirrors the top of <c>xai-image-model.test.ts</c>.</summary>
internal static class XaiImageTestSupport
{
    public const string Commit = "f630e0d735688a2e50c8d904436cb0f3ea25f33c";
    public const string Prompt = "A cute baby sea otter";
    public const string BaseUrl = "https://api.example.com";
    public const string GenerationsUrl = BaseUrl + "/images/generations";
    public const string EditsUrl = BaseUrl + "/images/edits";
    public const string DefaultResponse = "{\"data\":[{\"b64_json\":\"dGVzdA==\"}]}";

    /// <summary>Port of <c>createTestServer</c>: both endpoints answer <see cref="DefaultResponse"/>.</summary>
    public static ParityHandler Server(string response = DefaultResponse)
    {
        return new ParityHandler(_ => ParityHandler.Json(response));
    }

    /// <summary>Port of <c>createModel</c>.</summary>
    public static Model CreateModel(
        ParityHandler handler,
        IReadOnlyDictionary<string, string>? headers = null,
        Func<DateTime>? currentDate = null
    )
    {
        var options = new OpenAICompatibleOptions { BaseUrl = BaseUrl, ApiKey = "test-key" };
        foreach (var header in headers ?? new Dictionary<string, string>())
        {
            options.Headers[header.Key] = header.Value;
        }

        return new Model(
            Vercel.AI.Xai.XaiProvider.Create(options, handler),
            "grok-imagine-image",
            currentDate
        );
    }

    /// <summary>The <c>doGenerate</c> argument of the upstream tests, with overrides.</summary>
    public static ImageModelCall Call(
        string? prompt = Prompt,
        IReadOnlyList<ImageModelFile>? files = null,
        ImageModelFile? mask = null,
        string? size = null,
        string? aspectRatio = null,
        int? seed = null,
        string? xai = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default
    )
    {
        return new ImageModelCall(
            prompt,
            files,
            mask,
            1,
            size,
            aspectRatio,
            seed,
            JsonDocument.Parse(xai == null ? "{}" : "{\"xai\":" + xai + "}").RootElement.Clone(),
            headers ?? new Dictionary<string, string>(),
            cancellationToken
        );
    }

    public static JsonNode Body(ParityCall call) => JsonNode.Parse(call.Text)!;

    /// <summary><c>toStrictEqual</c> for JSON.</summary>
    public static void AssertJson(string expected, JsonNode? actual)
    {
        var expectedNode = JsonNode.Parse(expected);
        Assert.True(
            JsonNode.DeepEquals(expectedNode, actual),
            "Expected " + expectedNode?.ToJsonString() + " but was " + actual?.ToJsonString()
        );
    }

    public static HttpResponseMessage Error(HttpStatusCode status, string body)
    {
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
    }

    public static ImageModelFile PngFile() =>
        ImageModelFile.FromFile([137, 80, 78, 71], "image/png");
}
