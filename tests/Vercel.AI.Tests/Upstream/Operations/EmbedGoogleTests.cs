// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests;

/// <summary>Upstream parity for Google multimodal <c>embedMany</c>.</summary>
public sealed class EmbedGoogleTests
{
    private const string Prefix = "packages/ai/src/embed/embed-many-google.test.ts::embedMany with Google multimodal content::";

    [Fact]
    [UpstreamTest(Prefix + "preserves content alignment through wrapped models with maxParallelCalls %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_content_alignment_through_wrapped_models()
    {
        foreach (var parallel in new int?[] { 1, 2, null })
        {
            var handler = new RecordingHandler();
            var model = EmbedMany.Wrap(new GoogleMultimodalEmbedding("gemini-embedding-2", new HttpClient(handler), "https://generativelanguage.googleapis.com/v1beta"), null);
            var values = Enumerable.Repeat(string.Empty, 201).ToArray();
            var result = await EmbedMany.EmbedManyAsync(new EmbedManyRequest
            {
                Model = model,
                Values = values,
                ProviderOptions = OperationJson.Parse(ContentJson(201, index => index % 3 == 0 ? "null" : "[{\"text\":\"context " + index + "\"}]")),
                MaxParallelCalls = parallel,
                MaxRetries = 0,
            });
            Assert.Equal(201, result.Embeddings.Count);
            for (var index = 0; index < 201; index++)
            {
                Assert.Equal(index % 3 == 0 ? -1 : index, result.Embeddings[index][0]);
            }

            Assert.Equal(new[] { 100, 100, 1 }, handler.Bodies.Select(BatchLength));
            Assert.Equal(3, result.Responses.Count);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "allows middleware to normalize provider options for %s values", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_middleware_to_normalize_provider_options()
    {
        foreach (var count in new[] { 1, 101 })
        {
            var handler = new RecordingHandler();
            var inner = new GoogleMultimodalEmbedding("gemini-embedding-2", new HttpClient(handler), "https://generativelanguage.googleapis.com/v1beta");
            var model = EmbedMany.Wrap(inner, (call, _, _) =>
            {
                var json = call.ProviderOptions!.Value.GetRawText().Replace("\"outputDimensionality\":\"128\"", "\"outputDimensionality\":128");
                return Task.FromResult(new EmbeddingModelCall(call.Values, call.Headers, OperationJson.Parse(json), call.CancellationToken));
            });
            await EmbedMany.EmbedManyAsync(new EmbedManyRequest
            {
                Model = model,
                Values = Enumerable.Repeat("document", count).ToArray(),
                ProviderOptions = OperationJson.Parse("{\"google\":{\"outputDimensionality\":\"128\",\"content\":" + ContentArray(count, _ => "[{\"text\":\"context\"}]") + "}}"),
                MaxRetries = 0,
            });
            Assert.Equal(count, handler.Bodies.Sum(BatchLength));
            Assert.All(handler.Bodies.SelectMany(Dimensionality), value => Assert.Equal(128, value));
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "retries a failed batch with the same content without repeating successful batches", Coverage = UpstreamCoverage.Covered)]
    public async Task Retries_a_failed_batch_without_repeating_successful_batches()
    {
        var handler = new RecordingHandler { FailAt = 1 };
        var model = new GoogleMultimodalEmbedding("gemini-embedding-2", new HttpClient(handler), "https://generativelanguage.googleapis.com/v1beta");
        var result = await EmbedMany.EmbedManyAsync(new EmbedManyRequest
        {
            Model = model,
            Values = Enumerable.Repeat(string.Empty, 201).ToArray(),
            ProviderOptions = OperationJson.Parse(ContentJson(201, index => "[{\"text\":\"context " + index + "\"}]")),
            MaxParallelCalls = 1,
            MaxRetries = 1,
        });
        Assert.Equal(201, result.Embeddings.Count);
        Assert.Equal(0, result.Embeddings[0][0]);
        Assert.Equal(200, result.Embeddings[200][0]);
        Assert.Equal(4, handler.Bodies.Count);
        Assert.Equal(handler.Bodies[1], handler.Bodies[2]);
        Assert.Equal(new[] { 100, 100, 100, 1 }, handler.Bodies.Select(BatchLength));
        Assert.Equal(3, result.Responses.Count);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects mismatched content before making any requests", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_mismatched_content_before_any_request()
    {
        var handler = new RecordingHandler();
        var model = new GoogleMultimodalEmbedding("gemini-embedding-2", new HttpClient(handler), "https://generativelanguage.googleapis.com/v1beta");
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => EmbedMany.EmbedManyAsync(new EmbedManyRequest
        {
            Model = model,
            Values = Enumerable.Repeat("document", 101).ToArray(),
            ProviderOptions = OperationJson.Parse(ContentJson(100, _ => "null")),
            MaxRetries = 0,
        }));
        Assert.Contains("The number of multimodal content entries (100) must match the number of values (101).", error.Message);
        Assert.Empty(handler.Bodies);
    }

    [Fact]
    [UpstreamTest(Prefix + "validates malformed content when its batch is processed", Coverage = UpstreamCoverage.Covered)]
    public async Task Validates_malformed_content_when_its_batch_is_processed()
    {
        var handler = new RecordingHandler();
        var model = new GoogleMultimodalEmbedding("gemini-embedding-2", new HttpClient(handler), "https://generativelanguage.googleapis.com/v1beta");
        var error = await Assert.ThrowsAnyAsync<Exception>(() => EmbedMany.EmbedManyAsync(new EmbedManyRequest
        {
            Model = model,
            Values = Enumerable.Repeat("document", 101).ToArray(),
            ProviderOptions = OperationJson.Parse(ContentJson(101, index => index == 100 ? "[{\"text\":123}]" : "null")),
            MaxParallelCalls = 1,
            MaxRetries = 0,
        }));
        Assert.Contains("invalid google provider options", error.Message);
        Assert.Equal(new[] { 100 }, handler.Bodies.Select(BatchLength));
    }

    private static string ContentJson(int count, Func<int, string> entry)
    {
        return "{\"google\":{\"content\":" + ContentArray(count, entry) + "}}";
    }

    private static string ContentArray(int count, Func<int, string> entry)
    {
        return "[" + string.Join(",", Enumerable.Range(0, count).Select(entry)) + "]";
    }

    private static int BatchLength(string body)
    {
        using var document = JsonDocument.Parse(body);
        return document.RootElement.TryGetProperty("requests", out var requests) ? requests.GetArrayLength() : 1;
    }

    private static IEnumerable<int> Dimensionality(string body)
    {
        using var document = JsonDocument.Parse(body);
        if (document.RootElement.TryGetProperty("requests", out var requests))
        {
            foreach (var request in requests.EnumerateArray())
            {
                yield return request.GetProperty("outputDimensionality").GetInt32();
            }
        }
        else
        {
            yield return document.RootElement.GetProperty("outputDimensionality").GetInt32();
        }
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<string> Bodies { get; } = new List<string>();

        public int FailAt { get; set; } = -1;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content == null ? "{}" : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            var index = Bodies.Count;
            Bodies.Add(body);
            if (index == FailAt)
            {
                var failed = new HttpResponseMessage((HttpStatusCode)429) { Content = new StringContent("{}", Encoding.UTF8, "application/json") };
                failed.Headers.TryAddWithoutValidation("retry-after-ms", "0");
                return failed;
            }

            using var document = JsonDocument.Parse(body);
            var single = !document.RootElement.TryGetProperty("requests", out _);
            var embeddings = new List<string>();
            if (single)
            {
                embeddings.Add(Vector(document.RootElement));
            }
            else
            {
                foreach (var item in document.RootElement.GetProperty("requests").EnumerateArray())
                {
                    embeddings.Add(Vector(item));
                }
            }

            var payload = single
                ? "{\"embedding\":" + embeddings[0] + "}"
                : "{\"embeddings\":[" + string.Join(",", embeddings) + "]}";
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(payload, Encoding.UTF8, "application/json") };
        }

        private static string Vector(JsonElement request)
        {
            var value = -1;
            foreach (var part in request.GetProperty("content").GetProperty("parts").EnumerateArray())
            {
                if (part.TryGetProperty("text", out var text) && text.GetString() is string raw && raw.StartsWith("context ", StringComparison.Ordinal))
                {
                    value = int.Parse(raw.Substring("context ".Length), System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            return "{\"values\":[" + value.ToString(System.Globalization.CultureInfo.InvariantCulture) + "]}";
        }
    }
}
