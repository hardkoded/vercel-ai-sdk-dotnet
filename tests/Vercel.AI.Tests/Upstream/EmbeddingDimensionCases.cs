// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Tests.Upstream;

/// <summary>The four dimension cases shared by every provider <c>doEmbed</c> mapping test.</summary>
internal static class EmbeddingDimensionCases
{
    /// <summary>Dimensions, provider override, expected body value. Null means unset or omitted.</summary>
    public static TheoryData<int?, int?, int?> Cases => new()
    {
        { null, null, null },
        { 256, null, 256 },
        { null, 512, 512 },
        { 256, 512, 512 },
    };

    /// <summary>Builds <c>{ provider: { name: value } }</c>. An unset value leaves the bag empty.</summary>
    public static Dictionary<string, JsonElement> Options(string provider, string name, int? value)
    {
        var bag = new JsonObject();
        if (value != null)
        {
            bag[name] = value.Value;
        }

        return new Dictionary<string, JsonElement> { [provider] = JsonSerializer.Deserialize<JsonElement>(bag.ToJsonString()) };
    }

    /// <summary>Asserts that <paramref name="node"/> holds <paramref name="expected"/>, or is absent when it is null.</summary>
    public static void AssertDimension(JsonNode? node, int? expected)
    {
        if (expected == null)
        {
            Assert.Null(node);
        }
        else
        {
            Assert.Equal(expected.Value, node!.GetValue<int>());
        }
    }
}

/// <summary>Records request bodies and answers every call with one JSON response.</summary>
internal sealed class EmbeddingCapture : HttpMessageHandler
{
    public EmbeddingCapture(string response)
    {
        Response = response;
    }

    public string Response { get; }

    public List<string> Bodies { get; } = new();

    public List<string> Uris { get; } = new();

    public JsonNode Body => JsonNode.Parse(Bodies[0])!;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Uris.Add(request.RequestUri?.AbsoluteUri ?? string.Empty);
        Bodies.Add(request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false));
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Response, Encoding.UTF8, "application/json") };
    }
}
