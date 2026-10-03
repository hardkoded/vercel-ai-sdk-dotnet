// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests;

/// <summary>Upstream parity for <c>rerank</c>.</summary>
public sealed class RerankTests
{
    private const string Prefix = "packages/ai/src/rerank/rerank.test.ts::rerank > ";

    [Fact]
    [UpstreamTest(Prefix + "error handling::should reject invalid provider ranking index %s", Coverage = UpstreamCoverage.Partial, Note = "1.5 is a JavaScript number. RerankModelRank.Index is an int, so 3, -1, and 5 are the representable rejections.")]
    public async Task Rejects_an_invalid_ranking_index()
    {
        foreach (var index in new[] { 3, -1, 5 })
        {
            var model = new RerankFake { Ranking = new[] { new RerankModelRank(index, 0.9) } };
            var ended = false;
            var error = await Assert.ThrowsAsync<InvalidResponseDataException>(() => Rerank.RerankAsync(new RerankRequest
            {
                Model = model,
                Documents = new object?[] { "a", "b", "c" },
                Query = "q",
                OnEnd = _ => { ended = true; return Task.CompletedTask; },
            }));
            Assert.Equal("Invalid ranking index " + index + ". Expected an integer between 0 and 2.", error.Message);
            Assert.Equal(1, model.Calls);
            Assert.False(ended);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "rerank with string documents::should call the model with the correct options", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_string_documents_as_text()
    {
        var model = new RerankFake();
        await Rerank.RerankAsync(Request(model, new object?[] { "sunny", "rainy", "cloudy" }));
        Assert.Equal("text", model.Last!.Documents.Type);
        Assert.Equal("beach", model.Last.Query);
        Assert.Equal((int?)2, model.Last.TopN);
        Assert.Equal((object?)"sunny", model.Last.Documents.Values[0]);
        Assert.False(model.Last.Headers!.ContainsKey("user-agent"));
    }

    [Fact]
    [UpstreamTest(Prefix + "rerank with string documents::should return the correct original documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_original_string_documents()
    {
        var result = await Rerank.RerankAsync(Request(new RerankFake(), new object?[] { "sunny", "rainy", "cloudy" }));
        Assert.Equal(new object?[] { "sunny", "rainy", "cloudy" }, result.OriginalDocuments);
    }

    [Fact]
    [UpstreamTest(Prefix + "rerank with string documents::should return the correct reranked documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_string_documents_in_ranking_order()
    {
        var result = await Rerank.RerankAsync(Request(new RerankFake(), new object?[] { "sunny", "rainy", "cloudy" }));
        Assert.Equal(new object?[] { "cloudy", "sunny", "rainy" }, result.RerankedDocuments);
    }

    [Fact]
    [UpstreamTest(Prefix + "rerank with string documents::should return the correct ranking", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_string_ranking()
    {
        var result = await Rerank.RerankAsync(Request(new RerankFake(), new object?[] { "sunny", "rainy", "cloudy" }));
        Assert.Equal(new[] { 2, 0, 1 }, result.Ranking.Select(item => item.OriginalIndex));
        Assert.Equal(new[] { 0.9, 0.8, 0.7 }, result.Ranking.Select(item => item.Score));
    }

    [Fact]
    [UpstreamTest(Prefix + "rerank with string documents::should return the correct provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_string_document_provider_metadata()
    {
        var result = await Rerank.RerankAsync(Request(new RerankFake(), new object?[] { "sunny", "rainy", "cloudy" }));
        Assert.Equal("someResponseValue", result.ProviderMetadata!.Value.GetProperty("aProvider").GetProperty("someResponseKey").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "rerank with string documents::should return the correct response", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_string_document_response()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = await Rerank.RerankAsync(Request(new RerankFake(), new object?[] { "sunny", "rainy", "cloudy" }, now));
        Assert.Equal("mock-response-id", result.Response.Id);
        Assert.Equal("mock-response-model-id", result.Response.ModelId);
        Assert.Equal("application/json", result.Response.Headers!["content-type"]);
        Assert.Equal(now, result.Response.Timestamp);
    }

    [Fact]
    [UpstreamTest(Prefix + "rerank with object documents::should call the model with the correct options", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_object_documents()
    {
        var model = new RerankFake();
        var documents = Objects();
        await Rerank.RerankAsync(Request(model, documents));
        Assert.Equal("object", model.Last!.Documents.Type);
        Assert.Same(documents[0], model.Last.Documents.Values[0]);
    }

    [Fact]
    [UpstreamTest(Prefix + "rerank with object documents::should return the correct original documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_original_object_documents()
    {
        var documents = Objects();
        var result = await Rerank.RerankAsync(Request(new RerankFake(), documents));
        Assert.Same(documents[1], result.OriginalDocuments[1]);
    }

    [Fact]
    [UpstreamTest(Prefix + "rerank with object documents::should return the correct reranked documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_object_documents_in_ranking_order()
    {
        var documents = Objects();
        var result = await Rerank.RerankAsync(Request(new RerankFake(), documents));
        Assert.Same(documents[2], result.RerankedDocuments[0]);
    }

    [Fact]
    [UpstreamTest(Prefix + "rerank with object documents::should return the correct ranking", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_object_ranking()
    {
        var result = await Rerank.RerankAsync(Request(new RerankFake(), Objects()));
        Assert.Equal(0.8, result.Ranking[1].Score);
        Assert.Same(result.OriginalDocuments[0], result.Ranking[1].Document);
    }

    [Fact]
    [UpstreamTest(Prefix + "rerank with object documents::should return the correct provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_object_document_provider_metadata()
    {
        var result = await Rerank.RerankAsync(Request(new RerankFake(), Objects()));
        Assert.Equal(JsonValueKind.Object, result.ProviderMetadata!.Value.ValueKind);
    }

    [Fact]
    [UpstreamTest(Prefix + "rerank with object documents::should return the correct response", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_object_document_response()
    {
        var result = await Rerank.RerankAsync(Request(new RerankFake(), Objects()));
        Assert.Equal(123, result.Response.Body!.Value.GetProperty("id").GetInt32());
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onStart::should send correct event information", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_start_event()
    {
        RerankStartEvent? seen = null;
        await Rerank.RerankAsync(Request(new RerankFake(), new object?[] { "a" }, onStart: item => { seen = item; return Task.CompletedTask; }, callId: "rank"));
        Assert.Equal("rank", seen!.CallId);
        Assert.Equal("ai.rerank", seen.OperationId);
        Assert.Equal("beach", seen.Query);
        Assert.Equal(2, seen.MaxRetries);
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onStart::should include telemetry fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Filters_rerank_telemetry_context()
    {
        RerankStartEvent? telemetry = null;
        await Rerank.RerankAsync(new RerankRequest
        {
            Model = new RerankFake(),
            Documents = new object?[] { "a" },
            Query = "q",
            RuntimeContext = new Dictionary<string, object?> { ["secret"] = "s" },
            Telemetry = new RerankTelemetry { OnStart = item => { telemetry = item; return Task.CompletedTask; } },
        });
        Assert.Empty(telemetry!.RuntimeContext);
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onStart::should accept deprecated experimental_telemetry as an alias for telemetry", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_experimental_rerank_telemetry()
    {
        var called = false;
        await Rerank.RerankAsync(new RerankRequest { Model = new RerankFake(), Documents = new object?[] { "a" }, Query = "q", ExperimentalTelemetry = new RerankTelemetry { OnStart = _ => { called = true; return Task.CompletedTask; } } });
        Assert.True(called);
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onStart::should include model information", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_rerank_model_information()
    {
        RerankStartEvent? seen = null;
        await Rerank.RerankAsync(new RerankRequest { Model = new RerankFake { ModelId = "rank-model" }, Documents = new object?[] { "a" }, Query = "q", OnStart = item => { seen = item; return Task.CompletedTask; } });
        Assert.Equal("rank-model", seen!.ModelId);
        Assert.Equal("test-provider", seen.Provider);
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onStart::should be called before doRerank", Coverage = UpstreamCoverage.Covered)]
    public async Task Calls_on_start_before_rerank()
    {
        var order = new List<string>();
        var model = new RerankFake { Before = () => order.Add("model") };
        await Rerank.RerankAsync(new RerankRequest { Model = model, Documents = new object?[] { "a" }, Query = "q", OnStart = _ => { order.Add("start"); return Task.CompletedTask; } });
        Assert.Equal(new[] { "start", "model" }, order);
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onStart::should not break reranking when callback throws", Coverage = UpstreamCoverage.Covered)]
    public async Task Continues_when_rerank_start_throws()
    {
        var result = await Rerank.RerankAsync(new RerankRequest { Model = new RerankFake(), Documents = new object?[] { "a" }, Query = "q", OnStart = _ => throw new InvalidOperationException("start") });
        Assert.Single(result.Ranking);
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onStart::should include providerOptions, headers, documents, and query", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_rerank_inputs_on_start()
    {
        RerankStartEvent? seen = null;
        await Rerank.RerankAsync(new RerankRequest
        {
            Model = new RerankFake(),
            Documents = new object?[] { "doc" },
            Query = "find",
            TopN = 1,
            Headers = new Dictionary<string, string> { ["X-Test"] = "1" },
            ProviderOptions = OperationJson.Parse("{\"p\":true}"),
            OnStart = item => { seen = item; return Task.CompletedTask; },
        });
        Assert.Equal("doc", seen!.Documents[0]);
        Assert.Equal("find", seen.Query);
        Assert.Equal((int?)1, seen.TopN);
        Assert.Equal("1", seen.Headers!["X-Test"]);
        Assert.True(seen.ProviderOptions!.Value.GetProperty("p").GetBoolean());
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onEnd::should send correct event information", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_end_event()
    {
        RerankEndEvent? seen = null;
        await Rerank.RerankAsync(Request(new RerankFake(), new object?[] { "a" }, onEnd: item => { seen = item; return Task.CompletedTask; }, callId: "end"));
        Assert.Equal("end", seen!.CallId);
        Assert.Equal("ai.rerank", seen.OperationId);
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onEnd::should include ranking and documents in event", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_ranking_and_documents_on_end()
    {
        RerankEndEvent? seen = null;
        await Rerank.RerankAsync(Request(new RerankFake(), new object?[] { "sunny", "rainy", "cloudy" }, onEnd: item => { seen = item; return Task.CompletedTask; }));
        Assert.Equal("cloudy", seen!.Ranking[0].Document);
        Assert.Equal("sunny", seen.Documents[0]);
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onEnd::should isolate the result from ranking mutations in onEnd", Coverage = UpstreamCoverage.Covered)]
    public async Task Isolates_the_result_from_ranking_mutations()
    {
        var result = await Rerank.RerankAsync(Request(new RerankFake(), new object?[] { "sunny", "rainy", "cloudy" }, onEnd: item =>
        {
            ((List<RankedDocument>)item.Ranking).Clear();
            return Task.CompletedTask;
        }));
        Assert.Equal(3, result.Ranking.Count);
        Assert.Equal((object?)"cloudy", result.RerankedDocuments[0]);
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onEnd::should include model information", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_model_information_on_end()
    {
        RerankEndEvent? seen = null;
        await Rerank.RerankAsync(new RerankRequest { Model = new RerankFake { Provider = "rp" }, Documents = new object?[] { "a" }, Query = "q", OnEnd = item => { seen = item; return Task.CompletedTask; } });
        Assert.Equal("rp", seen!.Provider);
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onEnd::should include warnings and providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_warnings_and_metadata_on_end()
    {
        RerankEndEvent? seen = null;
        var model = new RerankFake { Warnings = new[] { OperationWarning.Other("careful") } };
        await Rerank.RerankAsync(Request(model, new object?[] { "a" }, onEnd: item => { seen = item; return Task.CompletedTask; }));
        Assert.Equal("careful", seen!.Warnings[0].Message);
        Assert.Equal("someResponseValue", seen.ProviderMetadata!.Value.GetProperty("aProvider").GetProperty("someResponseKey").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onEnd::should include response data", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_response_data_on_end()
    {
        RerankEndEvent? seen = null;
        await Rerank.RerankAsync(Request(new RerankFake(), new object?[] { "a" }, onEnd: item => { seen = item; return Task.CompletedTask; }));
        Assert.Equal("mock-response-id", seen!.Response.Id);
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onEnd::should be called after doRerank", Coverage = UpstreamCoverage.Covered)]
    public async Task Calls_on_end_after_the_model()
    {
        var order = new List<string>();
        await Rerank.RerankAsync(new RerankRequest { Model = new RerankFake { Before = () => order.Add("model") }, Documents = new object?[] { "a" }, Query = "q", OnEnd = _ => { order.Add("end"); return Task.CompletedTask; } });
        Assert.Equal(new[] { "model", "end" }, order);
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onEnd::should not break reranking when callback throws", Coverage = UpstreamCoverage.Covered)]
    public async Task Continues_when_rerank_end_throws()
    {
        var result = await Rerank.RerankAsync(new RerankRequest { Model = new RerankFake(), Documents = new object?[] { "a" }, Query = "q", OnEnd = _ => throw new InvalidOperationException("end") });
        Assert.Single(result.Ranking);
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onStart and onEnd together::should have consistent callId across both events", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_one_rerank_call_id()
    {
        string? start = null;
        string? end = null;
        await Rerank.RerankAsync(new RerankRequest
        {
            Model = new RerankFake(),
            Documents = new object?[] { "a" },
            Query = "q",
            GenerateCallId = () => "same",
            OnStart = item => { start = item.CallId; return Task.CompletedTask; },
            OnEnd = item => { end = item.CallId; return Task.CompletedTask; },
        });
        Assert.Equal("same", start);
        Assert.Equal(start, end);
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onStart and onEnd together::should call onStart before doRerank and onEnd after", Coverage = UpstreamCoverage.Covered)]
    public async Task Orders_rerank_callbacks()
    {
        var order = new List<string>();
        await Rerank.RerankAsync(new RerankRequest
        {
            Model = new RerankFake { Before = () => order.Add("model") },
            Documents = new object?[] { "a" },
            Query = "q",
            OnStart = _ => { order.Add("start"); return Task.CompletedTask; },
            OnEnd = _ => { order.Add("end"); return Task.CompletedTask; },
        });
        Assert.Equal(new[] { "start", "model", "end" }, order);
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onStart and onEnd together::should still call onEnd when onStart throws", Coverage = UpstreamCoverage.Covered)]
    public async Task Still_ends_rerank_when_start_throws()
    {
        var ended = false;
        await Rerank.RerankAsync(new RerankRequest { Model = new RerankFake(), Documents = new object?[] { "a" }, Query = "q", OnStart = _ => throw new InvalidOperationException("start"), OnEnd = _ => { ended = true; return Task.CompletedTask; } });
        Assert.True(ended);
    }

    [Fact]
    [UpstreamTest(Prefix + "options.onStart and onEnd together::should fire callbacks for empty documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Fires_callbacks_for_empty_documents()
    {
        var model = new RerankFake();
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var started = false;
        var ended = false;
        var result = await Rerank.RerankAsync(new RerankRequest
        {
            Model = model,
            Documents = Array.Empty<object?>(),
            Query = "q",
            Now = () => now,
            OnStart = _ => { started = true; return Task.CompletedTask; },
            OnEnd = _ => { ended = true; return Task.CompletedTask; },
        });
        Assert.True(started);
        Assert.True(ended);
        Assert.Equal(0, model.Calls);
        Assert.Empty(result.Ranking);
        Assert.Equal(now, result.Response.Timestamp);
        Assert.Equal("test-model", result.Response.ModelId);
    }

    private static object?[] Objects()
    {
        return new object?[]
        {
            new Dictionary<string, object?> { ["text"] = "sunny" },
            new Dictionary<string, object?> { ["text"] = "rainy" },
            new Dictionary<string, object?> { ["text"] = "cloudy" },
        };
    }

    private static RerankRequest Request(RerankFake model, object?[] documents, DateTime? now = null, Func<RerankStartEvent, Task>? onStart = null, Func<RerankEndEvent, Task>? onEnd = null, string? callId = null)
    {
        return new RerankRequest
        {
            Model = model,
            Documents = documents,
            Query = "beach",
            TopN = 2,
            Headers = new Dictionary<string, string> { ["Custom"] = "value" },
            Now = () => now ?? new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            GenerateCallId = callId == null ? null : () => callId,
            OnStart = onStart,
            OnEnd = onEnd,
        };
    }

    private sealed class RerankFake : IRerankCaller
    {
        public string Provider { get; set; } = "test-provider";

        public string ModelId { get; set; } = "test-model";

        public int Calls { get; private set; }

        public RerankModelCall? Last { get; private set; }

        public IReadOnlyList<RerankModelRank>? Ranking { get; set; }

        public IReadOnlyList<OperationWarning>? Warnings { get; set; }

        public Action? Before { get; set; }

        public Task<RerankModelResponse> DoRerankAsync(RerankModelCall call, CancellationToken cancellationToken)
        {
            Calls++;
            Last = call;
            Before?.Invoke();
            var ranking = Ranking ?? new[]
            {
                new RerankModelRank(Math.Min(2, call.Documents.Values.Count - 1), 0.9),
                new RerankModelRank(0, 0.8),
                new RerankModelRank(Math.Min(1, call.Documents.Values.Count - 1), 0.7),
            };
            if (call.Documents.Values.Count == 1)
            {
                ranking = new[] { new RerankModelRank(0, 0.9) };
            }

            return Task.FromResult(new RerankModelResponse(
                ranking,
                Warnings,
                OperationJson.Parse("{\"aProvider\":{\"someResponseKey\":\"someResponseValue\"}}"),
                new ProviderResponse(new Dictionary<string, string> { ["content-type"] = "application/json" }, OperationJson.Parse("{\"id\":123}"), "mock-response-id", modelId: "mock-response-model-id")));
        }
    }
}
