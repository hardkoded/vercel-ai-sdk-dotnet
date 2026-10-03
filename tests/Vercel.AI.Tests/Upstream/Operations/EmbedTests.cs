// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests;

/// <summary>Upstream parity for <c>embed</c> and <c>embedMany</c>.</summary>
[Collection("WarningLog")]
public sealed class EmbedTests
{
    private const string EmbedFile = "packages/ai/src/embed/embed.test.ts";
    private const string Many = "packages/ai/src/embed/embed-many.test.ts";

    [Fact]
    [UpstreamTest(EmbedFile + "::result.embedding::should generate embedding", Coverage = UpstreamCoverage.Covered)]
    public async Task Generates_one_embedding()
    {
        var model = new EmbedFake { Respond = _ => Vector(new[] { 0.1, 0.2, 0.3 }, 5) };
        var result = await Embed.EmbedAsync(new EmbedRequest { Model = model, Value = "sunny day at the beach" });
        Assert.Equal(new[] { 0.1, 0.2, 0.3 }, result.Embedding);
        Assert.Equal(new[] { "sunny day at the beach" }, model.Calls[0].Values);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::result.embedding::should reject when the model returns no embeddings", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_when_the_model_returns_no_embeddings()
    {
        var model = new EmbedFake { Respond = _ => new EmbeddingModelResponse(Array.Empty<double[]>(), 0) };
        var error = await Assert.ThrowsAsync<InvalidResponseDataException>(() => Embed.EmbedAsync(new EmbedRequest { Model = model, Value = "value", MaxRetries = 0 }));
        Assert.Equal("No embedding generated.", error.Message);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::result.response::should include response in the result", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_the_provider_response()
    {
        var response = new ProviderResponse(new Dictionary<string, string> { ["x-request-id"] = "id" }, OperationJson.Parse("{\"raw\":true}"), "response-id", modelId: "response-model");
        var model = new EmbedFake { Respond = _ => new EmbeddingModelResponse(new[] { new[] { 1.0 } }, 1, response: response) };
        var result = await Embed.EmbedAsync(new EmbedRequest { Model = model, Value = "value" });
        Assert.Equal("response-id", result.Response!.Id);
        Assert.Equal("id", result.Response.Headers!["x-request-id"]);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::result.value::should include value in the result", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_the_value()
    {
        var result = await Embed.EmbedAsync(new EmbedRequest { Model = new EmbedFake(), Value = "value" });
        Assert.Equal("value", result.Value);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::result.usage::should include usage in the result", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_usage_and_nan_when_omitted()
    {
        var reported = await Embed.EmbedAsync(new EmbedRequest { Model = new EmbedFake { Respond = _ => Vector(new[] { 1.0 }, 11) }, Value = "value" });
        Assert.Equal((double?)11, reported.Usage.Tokens);
        var omitted = await Embed.EmbedAsync(new EmbedRequest { Model = new EmbedFake { Respond = _ => new EmbeddingModelResponse(new[] { new[] { 1.0 } }, usageOmitted: true) }, Value = "value" });
        Assert.True(double.IsNaN(omitted.Usage.Tokens!.Value));
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::result.providerMetadata::should include provider metadata when returned by the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_provider_metadata()
    {
        var metadata = OperationJson.Parse("{\"provider\":{\"key\":\"value\"}}");
        var result = await Embed.EmbedAsync(new EmbedRequest { Model = new EmbedFake { Respond = _ => new EmbeddingModelResponse(new[] { new[] { 1.0 } }, 1, providerMetadata: metadata) }, Value = "value" });
        Assert.Equal("value", result.ProviderMetadata!.Value.GetProperty("provider").GetProperty("key").GetString());
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.headers::should set headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Sets_headers_and_the_user_agent()
    {
        var model = new EmbedFake();
        await Embed.EmbedAsync(new EmbedRequest { Model = model, Value = "value", Headers = new Dictionary<string, string> { ["Custom-Header"] = "custom-value" } });
        Assert.Equal("custom-value", model.Calls[0].Headers["custom-header"]);
        Assert.Equal("ai/0.0.0-test", model.Calls[0].Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.providerOptions::should pass provider options to model", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_provider_options()
    {
        var model = new EmbedFake();
        var options = OperationJson.Parse("{\"aProvider\":{\"someKey\":\"someValue\"}}");
        await Embed.EmbedAsync(new EmbedRequest { Model = model, Value = "value", ProviderOptions = options });
        Assert.Equal("someValue", model.Calls[0].ProviderOptions!.Value.GetProperty("aProvider").GetProperty("someKey").GetString());
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::result.warnings::should include warnings in the result", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_warnings()
    {
        var warning = OperationWarning.Other("embedding note");
        var result = await Embed.EmbedAsync(new EmbedRequest { Model = new EmbedFake { Respond = _ => new EmbeddingModelResponse(new[] { new[] { 1.0 } }, 1, new[] { warning }) }, Value = "value" });
        Assert.Equal("embedding note", result.Warnings[0].Message);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::result.warnings::should default missing v2 provider warnings to an empty array", Coverage = UpstreamCoverage.Covered)]
    public async Task Defaults_missing_warnings_to_an_empty_list()
    {
        var result = await Embed.EmbedAsync(new EmbedRequest { Model = new EmbedFake { SpecificationVersion = "v2" }, Value = "value" });
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::logWarnings::should call logWarnings with the correct warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Logs_embedding_warnings()
    {
        var seen = new List<WarningLogContext>();
        WarningLog.Observer = seen.Add;
        try
        {
            var warning = OperationWarning.Other("note");
            await Embed.EmbedAsync(new EmbedRequest { Model = new EmbedFake { Provider = "embed-provider", ModelId = "embed-model", Respond = _ => new EmbeddingModelResponse(new[] { new[] { 1.0 } }, 1, new[] { warning }) }, Value = "value" });
            Assert.Equal("note", seen[0].Warnings[0].Message);
            Assert.Equal("embed-provider", seen[0].Provider);
            Assert.Equal("embed-model", seen[0].Model);
        }
        finally
        {
            WarningLog.Observer = null;
        }
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onStart::should send correct event information", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_start_event()
    {
        EmbedStartEvent? seen = null;
        await Embed.EmbedAsync(new EmbedRequest
        {
            Model = new EmbedFake(),
            Value = "value",
            GenerateCallId = () => "call-1",
            MaxRetries = 0,
            OnStart = item => { seen = item; return Task.CompletedTask; },
        });
        Assert.Equal("call-1", seen!.CallId);
        Assert.Equal("ai.embed", seen.OperationId);
        Assert.Equal("value", seen.Value);
        Assert.Equal(0, seen.MaxRetries);
        Assert.Equal("test-provider", seen.Provider);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onStart::should include telemetry fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Filters_runtime_context_for_telemetry()
    {
        EmbedStartEvent? user = null;
        EmbedStartEvent? telemetry = null;
        await Embed.EmbedAsync(new EmbedRequest
        {
            Model = new EmbedFake(),
            Value = "value",
            RuntimeContext = new Dictionary<string, object?> { ["requestId"] = "r", ["secret"] = "s" },
            OnStart = item => { user = item; return Task.CompletedTask; },
            Telemetry = new EmbedTelemetry { OnStart = item => { telemetry = item; return Task.CompletedTask; } },
        });
        Assert.Equal("s", user!.RuntimeContext["secret"]);
        Assert.Empty(telemetry!.RuntimeContext);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onStart::should accept deprecated experimental_telemetry as an alias for telemetry", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_the_experimental_telemetry_alias()
    {
        var called = false;
        await Embed.EmbedAsync(new EmbedRequest
        {
            Model = new EmbedFake(),
            Value = "value",
            ExperimentalTelemetry = new EmbedTelemetry { OnStart = _ => { called = true; return Task.CompletedTask; } },
        });
        Assert.True(called);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onStart::should include model information", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_model_information_on_start()
    {
        EmbedStartEvent? seen = null;
        await Embed.EmbedAsync(new EmbedRequest { Model = new EmbedFake { Provider = "p", ModelId = "m" }, Value = "value", OnStart = item => { seen = item; return Task.CompletedTask; } });
        Assert.Equal("p", seen!.Provider);
        Assert.Equal("m", seen.ModelId);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onStart::should be called before doEmbed", Coverage = UpstreamCoverage.Covered)]
    public async Task Calls_on_start_before_the_model()
    {
        var order = new List<string>();
        var model = new EmbedFake { Before = () => order.Add("model") };
        await Embed.EmbedAsync(new EmbedRequest { Model = model, Value = "value", OnStart = _ => { order.Add("start"); return Task.CompletedTask; } });
        Assert.Equal(new[] { "start", "model" }, order);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onStart::should not break embedding when callback throws", Coverage = UpstreamCoverage.Covered)]
    public async Task Continues_when_on_start_throws()
    {
        var result = await Embed.EmbedAsync(new EmbedRequest { Model = new EmbedFake(), Value = "value", OnStart = _ => throw new InvalidOperationException("callback") });
        Assert.NotEmpty(result.Embedding);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onStart::should include providerOptions and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_options_and_headers_on_start()
    {
        EmbedStartEvent? seen = null;
        var options = OperationJson.Parse("{\"p\":{\"k\":1}}");
        await Embed.EmbedAsync(new EmbedRequest
        {
            Model = new EmbedFake(),
            Value = "value",
            ProviderOptions = options,
            Headers = new Dictionary<string, string> { ["X-Test"] = "1" },
            OnStart = item => { seen = item; return Task.CompletedTask; },
        });
        Assert.Equal(1, seen!.ProviderOptions!.Value.GetProperty("p").GetProperty("k").GetInt32());
        Assert.Equal("1", seen.Headers["x-test"]);
        Assert.Contains("ai/", seen.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onEnd::should send correct event information", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_end_event()
    {
        EmbedEndEvent? seen = null;
        await Embed.EmbedAsync(new EmbedRequest { Model = new EmbedFake(), Value = "value", GenerateCallId = () => "call-1", OnEnd = item => { seen = item; return Task.CompletedTask; } });
        Assert.Equal("call-1", seen!.CallId);
        Assert.Equal("ai.embed", seen.OperationId);
        Assert.Equal("value", seen.Value);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onEnd::should include embedding and usage in event", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_embedding_and_usage_on_end()
    {
        EmbedEndEvent? seen = null;
        await Embed.EmbedAsync(new EmbedRequest { Model = new EmbedFake { Respond = _ => Vector(new[] { 0.5 }, 7) }, Value = "value", OnEnd = item => { seen = item; return Task.CompletedTask; } });
        Assert.Equal(new[] { 0.5 }, (double[])seen!.Embedding);
        Assert.Equal((double?)7, seen.Usage.Tokens);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onEnd::should include model information", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_model_information_on_end()
    {
        EmbedEndEvent? seen = null;
        await Embed.EmbedAsync(new EmbedRequest { Model = new EmbedFake { ModelId = "m" }, Value = "value", OnEnd = item => { seen = item; return Task.CompletedTask; } });
        Assert.Equal("m", seen!.ModelId);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onEnd::should include warnings and providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_warnings_and_metadata_on_end()
    {
        EmbedEndEvent? seen = null;
        var metadata = OperationJson.Parse("{\"p\":{\"a\":true}}");
        await Embed.EmbedAsync(new EmbedRequest
        {
            Model = new EmbedFake { Respond = _ => new EmbeddingModelResponse(new[] { new[] { 1.0 } }, 1, new[] { OperationWarning.Other("w") }, metadata) },
            Value = "value",
            OnEnd = item => { seen = item; return Task.CompletedTask; },
        });
        Assert.Equal("w", seen!.Warnings[0].Message);
        Assert.True(seen.ProviderMetadata!.Value.GetProperty("p").GetProperty("a").GetBoolean());
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onEnd::should include response data", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_response_data_on_end()
    {
        EmbedEndEvent? seen = null;
        var response = new ProviderResponse(id: "rid");
        await Embed.EmbedAsync(new EmbedRequest { Model = new EmbedFake { Respond = _ => new EmbeddingModelResponse(new[] { new[] { 1.0 } }, 1, response: response) }, Value = "value", OnEnd = item => { seen = item; return Task.CompletedTask; } });
        Assert.Equal("rid", ((ProviderResponse)seen!.Response!).Id);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onEnd::should be called after doEmbed", Coverage = UpstreamCoverage.Covered)]
    public async Task Calls_on_end_after_the_model()
    {
        var order = new List<string>();
        await Embed.EmbedAsync(new EmbedRequest { Model = new EmbedFake { Before = () => order.Add("model") }, Value = "value", OnEnd = _ => { order.Add("end"); return Task.CompletedTask; } });
        Assert.Equal(new[] { "model", "end" }, order);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onEnd::should not break embedding when callback throws", Coverage = UpstreamCoverage.Covered)]
    public async Task Continues_when_on_end_throws()
    {
        var result = await Embed.EmbedAsync(new EmbedRequest { Model = new EmbedFake(), Value = "value", OnEnd = _ => throw new InvalidOperationException("callback") });
        Assert.NotEmpty(result.Embedding);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onStart and onEnd together::should have consistent callId across both events", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_one_call_id_for_start_and_end()
    {
        string? start = null;
        string? end = null;
        await Embed.EmbedAsync(new EmbedRequest
        {
            Model = new EmbedFake(),
            Value = "value",
            GenerateCallId = () => "same",
            OnStart = item => { start = item.CallId; return Task.CompletedTask; },
            OnEnd = item => { end = item.CallId; return Task.CompletedTask; },
        });
        Assert.Equal("same", start);
        Assert.Equal(start, end);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onStart and onEnd together::should call onStart before doEmbed and onEnd after", Coverage = UpstreamCoverage.Covered)]
    public async Task Orders_start_model_and_end()
    {
        var order = new List<string>();
        await Embed.EmbedAsync(new EmbedRequest
        {
            Model = new EmbedFake { Before = () => order.Add("model") },
            Value = "value",
            OnStart = _ => { order.Add("start"); return Task.CompletedTask; },
            OnEnd = _ => { order.Add("end"); return Task.CompletedTask; },
        });
        Assert.Equal(new[] { "start", "model", "end" }, order);
    }

    [Fact]
    [UpstreamTest(EmbedFile + "::options.onStart and onEnd together::should still call onEnd when onStart throws", Coverage = UpstreamCoverage.Covered)]
    public async Task Still_calls_on_end_when_on_start_throws()
    {
        var ended = false;
        await Embed.EmbedAsync(new EmbedRequest
        {
            Model = new EmbedFake(),
            Value = "value",
            OnStart = _ => throw new InvalidOperationException("start"),
            OnEnd = _ => { ended = true; return Task.CompletedTask; },
        });
        Assert.True(ended);
    }

    [Fact]
    [UpstreamTest(Many + "::error handling::should reject an embedding count mismatch in a single call", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_embedding_count_mismatch_in_a_single_call()
    {
        var model = new EmbedFake { Respond = _ => new EmbeddingModelResponse(new[] { new[] { 1.0 } }, 1) };
        var error = await Assert.ThrowsAsync<InvalidResponseDataException>(() => EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "a", "b" }, MaxRetries = 0 }));
        Assert.Equal("Expected 2 embeddings, but received 1.", error.Message);
    }

    [Fact]
    [UpstreamTest(Many + "::error handling::should reject an embedding count mismatch in each chunk", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_embedding_count_mismatch_in_each_chunk()
    {
        var model = new EmbedFake { MaxEmbeddingsPerCall = 1, Respond = _ => new EmbeddingModelResponse(Array.Empty<double[]>(), 1) };
        var error = await Assert.ThrowsAsync<InvalidResponseDataException>(() => EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "a", "b" }, MaxRetries = 0 }));
        Assert.Contains("Expected 1 embeddings, but received 0.", error.Message);
    }

    [Fact]
    [UpstreamTest(Many + "::model.supportsParallelCalls::should not parallelize when false", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_parallelize_when_the_model_forbids_it()
    {
        var gate = new TaskCompletionSource<bool>();
        var active = 0;
        var max = 0;
        var model = new EmbedFake
        {
            MaxEmbeddingsPerCall = 1,
            SupportsParallelCalls = false,
            RespondAsync = async call =>
            {
                var current = Interlocked.Increment(ref active);
                max = Math.Max(max, current);
                if (current == 1)
                {
                    await gate.Task;
                }

                Interlocked.Decrement(ref active);
                return Vector(new[] { 1d }, 1);
            },
        };
        var pending = EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "a", "b" }, MaxParallelCalls = 8, MaxRetries = 0 });
        await Task.Delay(30);
        Assert.Single(model.Calls);
        gate.SetResult(true);
        var result = await pending;
        Assert.Equal(1, max);
        Assert.Equal(2, result.Embeddings.Count);
    }

    [Fact]
    [UpstreamTest(Many + "::model.supportsParallelCalls::should parallelize when true", Coverage = UpstreamCoverage.Covered)]
    public async Task Parallelizes_when_the_model_allows_it()
    {
        var both = new TaskCompletionSource<bool>();
        var active = 0;
        var model = new EmbedFake
        {
            MaxEmbeddingsPerCall = 1,
            SupportsParallelCalls = true,
            RespondAsync = async call =>
            {
                if (Interlocked.Increment(ref active) == 2)
                {
                    both.TrySetResult(true);
                }

                await both.Task;
                return Vector(new[] { 1.0 }, 1);
            },
        };
        var result = await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "a", "b" }, MaxRetries = 0 });
        Assert.True(both.Task.IsCompleted);
        Assert.Equal(2, result.Embeddings.Count);
    }

    [Fact]
    [UpstreamTest(Many + "::model.supportsParallelCalls::should support maxParallelCalls", Coverage = UpstreamCoverage.Covered)]
    public async Task Limits_parallel_calls()
    {
        var release = new TaskCompletionSource<bool>();
        var active = 0;
        var max = 0;
        var model = new EmbedFake
        {
            MaxEmbeddingsPerCall = 1,
            RespondAsync = async _ =>
            {
                var current = Interlocked.Increment(ref active);
                lock (release)
                {
                    max = Math.Max(max, current);
                }

                if (current == 2)
                {
                    release.TrySetResult(true);
                }

                await release.Task;
                Interlocked.Decrement(ref active);
                return Vector(new[] { 1.0 }, 1);
            },
        };
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "a", "b", "c" }, MaxParallelCalls = 2, MaxRetries = 0 });
        Assert.Equal(2, max);
    }

    [Fact]
    [UpstreamTest(Many + "::model.supportsParallelCalls::should throw InvalidArgumentError when maxParallelCalls is %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_a_non_positive_parallel_limit()
    {
        foreach (var limit in new[] { 0, -1 })
        {
            var model = new EmbedFake { MaxEmbeddingsPerCall = 1 };
            var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "a", "b" }, MaxParallelCalls = limit }));
            Assert.Equal("chunkSize", error.Parameter);
            Assert.Equal("chunkSize must be greater than 0", error.Message.Split(':')[1].Trim());
            Assert.Empty(model.Calls);
        }
    }

    [Fact]
    [UpstreamTest(Many + "::result.embedding::should generate embeddings", Coverage = UpstreamCoverage.Covered)]
    public async Task Generates_embeddings_for_every_value()
    {
        var result = await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake(), Values = new[] { "a", "b" } });
        Assert.Equal(2, result.Embeddings.Count);
        Assert.Equal(new[] { "a", "b" }, result.Values);
    }

    [Fact]
    [UpstreamTest(Many + "::result.embedding::should generate embeddings when several calls are required", Coverage = UpstreamCoverage.Covered)]
    public async Task Splits_embeddings_across_calls()
    {
        var model = new EmbedFake { MaxEmbeddingsPerCall = 2, Respond = call => new EmbeddingModelResponse(call.Values.Select(value => new[] { (double)value.Length }).ToArray(), 1) };
        var result = await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "a", "bb", "ccc" }, MaxRetries = 0 });
        Assert.Equal(2, model.Calls.Count);
        Assert.Equal(new[] { 1.0, 2, 3 }, result.Embeddings.Select(item => item[0]));
    }

    [Fact]
    [UpstreamTest(Many + "::result.embedding::should split calls when the UTF-8 input byte budget is exceeded", Coverage = UpstreamCoverage.Covered)]
    public async Task Splits_when_the_utf8_budget_is_exceeded()
    {
        var model = new EmbedFake { MaxEmbeddingsPerCall = 10, MaxInputBytesPerCall = 4 };
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "ab", "cd", "ef" }, MaxRetries = 0 });
        Assert.Equal(new[] { "ab", "cd" }, model.Calls[0].Values);
        Assert.Equal(new[] { "ef" }, model.Calls[1].Values);
    }

    [Fact]
    [UpstreamTest(Many + "::result.embedding::should split by input bytes without an embedding count limit", Coverage = UpstreamCoverage.Covered)]
    public async Task Splits_by_bytes_without_a_count_limit()
    {
        var model = new EmbedFake { MaxInputBytesPerCall = 3 };
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "aa", "bb" }, MaxRetries = 0 });
        Assert.Equal(2, model.Calls.Count);
    }

    [Fact]
    [UpstreamTest(Many + "::result.embedding::should combine embedding count and input byte limits in one pass", Coverage = UpstreamCoverage.Covered)]
    public async Task Combines_count_and_byte_limits()
    {
        var model = new EmbedFake { MaxEmbeddingsPerCall = 2, MaxInputBytesPerCall = 3 };
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "a", "b", "c" }, MaxRetries = 0 });
        Assert.Equal(new[] { "a", "b" }, model.Calls[0].Values);
        Assert.Equal(new[] { "c" }, model.Calls[1].Values);
    }

    [Fact]
    [UpstreamTest(Many + "::result.embedding::should treat an infinite input byte budget as unlimited", Coverage = UpstreamCoverage.Covered)]
    public async Task Treats_an_infinite_byte_budget_as_unlimited()
    {
        var model = new EmbedFake { MaxInputBytesPerCall = double.PositiveInfinity };
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "a", "b" }, MaxRetries = 0 });
        Assert.Single(model.Calls);
    }

    [Fact]
    [UpstreamTest(Many + "::result.embedding::should send a value larger than the input byte budget by itself", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_an_oversized_value_by_itself()
    {
        var model = new EmbedFake { MaxInputBytesPerCall = 2 };
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "abcd", "e" }, MaxRetries = 0 });
        Assert.Equal(new[] { "abcd" }, model.Calls[0].Values);
        Assert.Equal(new[] { "e" }, model.Calls[1].Values);
    }

    [Fact]
    [UpstreamTest(Many + "::result.responses::should include responses in the result", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_one_response_per_call()
    {
        var model = new EmbedFake { MaxEmbeddingsPerCall = 1, Respond = call => new EmbeddingModelResponse(new[] { new[] { 1.0 } }, 1, response: new ProviderResponse(id: call.Values[0])) };
        var result = await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "a", "b" }, MaxRetries = 0 });
        Assert.Equal(new[] { "a", "b" }, result.Responses.Select(item => item!.Id));
    }

    [Fact]
    [UpstreamTest(Many + "::result.values::should include values in the result", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_the_submitted_values()
    {
        var result = await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake(), Values = new[] { "a", "b" } });
        Assert.Equal(new[] { "a", "b" }, result.Values);
    }

    [Fact]
    [UpstreamTest(Many + "::result.usage::should include usage in the result", Coverage = UpstreamCoverage.Covered)]
    public async Task Sums_usage_across_chunks()
    {
        var model = new EmbedFake { MaxEmbeddingsPerCall = 1, Respond = _ => Vector(new[] { 1.0 }, 4) };
        var result = await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "a", "b" }, MaxRetries = 0 });
        Assert.Equal((double?)8, result.Usage.Tokens);
        var omitted = new EmbedFake { Respond = _ => new EmbeddingModelResponse(new[] { new[] { 1.0 }, new[] { 2.0 } }, usageOmitted: true) };
        var missing = await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = omitted, Values = new[] { "a", "b" } });
        Assert.True(double.IsNaN(missing.Usage.Tokens!.Value));
    }

    [Fact]
    [UpstreamTest(Many + "::options.headers::should set headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Sets_headers_for_every_chunk()
    {
        var model = new EmbedFake();
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "a" }, Headers = new Dictionary<string, string> { ["X-Test"] = "1" } });
        Assert.Equal("1", model.Calls[0].Headers["x-test"]);
        Assert.Equal("ai/0.0.0-test", model.Calls[0].Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Many + "::options.providerOptions::should pass provider options to model", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_provider_options_to_embed_many()
    {
        var model = new EmbedFake();
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "a" }, ProviderOptions = OperationJson.Parse("{\"p\":{\"k\":true}}") });
        Assert.True(model.Calls[0].ProviderOptions!.Value.GetProperty("p").GetProperty("k").GetBoolean());
    }

    [Fact]
    [UpstreamTest(Many + "::options.providerOptions::should align provider options across batches with limits %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Aligns_provider_options_across_batches()
    {
        var cases = new (int? Parallel, double? Bytes)[] { (1, null), (2, null), (null, null), (2, 3) };
        foreach (var item in cases)
        {
            var slices = new List<(int Start, int End)>();
            var model = new EmbedFake
            {
                MaxEmbeddingsPerCall = item.Bytes == null ? 1 : 10,
                MaxInputBytesPerCall = item.Bytes,
                Transform = (_, _, start, end) =>
                {
                    slices.Add((start, end));
                    return OperationJson.Parse("{\"slice\":" + start + "}");
                },
            };
            var values = item.Bytes == null ? new[] { "a", "b" } : new[] { "a", "bb", "c" };
            await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = values, MaxParallelCalls = item.Parallel, ProviderOptions = OperationJson.Parse("{}"), MaxRetries = 0 });
            Assert.Equal(0, slices[0].Start);
            Assert.Equal(slices.Select(slice => slice.Start).Distinct().Count(), slices.Count);
        }
    }

    [Fact]
    [UpstreamTest(Many + "::result.providerMetadata::should include provider metadata when returned by the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Merges_provider_metadata_across_chunks()
    {
        var turn = 0;
        var model = new EmbedFake
        {
            MaxEmbeddingsPerCall = 1,
            Respond = _ =>
            {
                var index = turn++;
                return new EmbeddingModelResponse(new[] { new[] { 1.0 } }, 1, providerMetadata: OperationJson.Parse("{\"p\":{\"n\":" + index + "}}"));
            },
        };
        var result = await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "a", "b" }, MaxParallelCalls = 1, MaxRetries = 0 });
        Assert.Equal(1, result.ProviderMetadata!.Value.GetProperty("p").GetProperty("n").GetInt32());
    }

    [Fact]
    [UpstreamTest(Many + "::result.warnings::should include warnings in the result (single call path)", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_warnings_from_a_single_call()
    {
        var result = await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake { Respond = _ => new EmbeddingModelResponse(new[] { new[] { 1.0 } }, 1, new[] { OperationWarning.Other("once") }) }, Values = new[] { "a" } });
        Assert.Equal("once", result.Warnings[0].Message);
    }

    [Fact]
    [UpstreamTest(Many + "::result.warnings::should default missing v2 provider warnings to an empty array in the single call path", Coverage = UpstreamCoverage.Covered)]
    public async Task Defaults_single_call_warnings()
    {
        var result = await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake { SpecificationVersion = "v2" }, Values = new[] { "a" } });
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Many + "::result.warnings::should aggregate warnings from multiple calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Aggregates_warnings_from_multiple_calls()
    {
        var turn = 0;
        var model = new EmbedFake
        {
            MaxEmbeddingsPerCall = 1,
            Respond = _ => new EmbeddingModelResponse(new[] { new[] { 1.0 } }, 1, new[] { OperationWarning.Other("w" + turn++) }),
        };
        var result = await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "a", "b" }, MaxParallelCalls = 1, MaxRetries = 0 });
        Assert.Equal(new[] { "w0", "w1" }, result.Warnings.Select(item => item.Message));
    }

    [Fact]
    [UpstreamTest(Many + "::result.warnings::should default missing v2 provider warnings to an empty array in the chunked path", Coverage = UpstreamCoverage.Covered)]
    public async Task Defaults_chunked_warnings()
    {
        var result = await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake { SpecificationVersion = "v2", MaxEmbeddingsPerCall = 1 }, Values = new[] { "a", "b" }, MaxRetries = 0 });
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Many + "::logWarnings::should call logWarnings with the correct warnings (single call path)", Coverage = UpstreamCoverage.Covered)]
    public async Task Logs_single_call_warnings()
    {
        var seen = new List<WarningLogContext>();
        WarningLog.Observer = seen.Add;
        try
        {
            await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake { Respond = _ => new EmbeddingModelResponse(new[] { new[] { 1.0 } }, 1, new[] { OperationWarning.Other("logged") }) }, Values = new[] { "a" } });
            Assert.Equal("logged", seen[0].Warnings[0].Message);
        }
        finally
        {
            WarningLog.Observer = null;
        }
    }

    [Fact]
    [UpstreamTest(Many + "::logWarnings::should call logWarnings with aggregated warnings from multiple calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Logs_aggregated_warnings()
    {
        var seen = new List<WarningLogContext>();
        WarningLog.Observer = seen.Add;
        try
        {
            var turn = 0;
            var model = new EmbedFake { MaxEmbeddingsPerCall = 1, Respond = _ => new EmbeddingModelResponse(new[] { new[] { 1.0 } }, 1, new[] { OperationWarning.Other("n" + turn++) }) };
            await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "a", "b" }, MaxParallelCalls = 1, MaxRetries = 0 });
            Assert.Equal(2, seen[0].Warnings.Count);
        }
        finally
        {
            WarningLog.Observer = null;
        }
    }

    [Fact]
    [UpstreamTest(Many + "::options.onStart::should send correct event information", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_embed_many_start_information()
    {
        EmbedStartEvent? seen = null;
        var values = new[] { "a", "b" };
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake(), Values = values, GenerateCallId = () => "many", OnStart = item => { seen = item; return Task.CompletedTask; } });
        Assert.Equal("many", seen!.CallId);
        Assert.Equal("ai.embedMany", seen.OperationId);
        Assert.Same(values, seen.Value);
    }

    [Fact]
    [UpstreamTest(Many + "::options.onStart::should include telemetry fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Filters_embed_many_telemetry_context()
    {
        EmbedStartEvent? telemetry = null;
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest
        {
            Model = new EmbedFake(),
            Values = new[] { "a" },
            RuntimeContext = new Dictionary<string, object?> { ["secret"] = "s" },
            Telemetry = new EmbedTelemetry { OnStart = item => { telemetry = item; return Task.CompletedTask; } },
        });
        Assert.Empty(telemetry!.RuntimeContext);
    }

    [Fact]
    [UpstreamTest(Many + "::options.onStart::should accept deprecated experimental_telemetry as an alias for telemetry", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_experimental_telemetry_for_embed_many()
    {
        var called = false;
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake(), Values = new[] { "a" }, ExperimentalTelemetry = new EmbedTelemetry { OnStart = _ => { called = true; return Task.CompletedTask; } } });
        Assert.True(called);
    }

    [Fact]
    [UpstreamTest(Many + "::options.onStart::should include model information", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_embed_many_model_information()
    {
        EmbedStartEvent? seen = null;
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake { ModelId = "many-model" }, Values = new[] { "a" }, OnStart = item => { seen = item; return Task.CompletedTask; } });
        Assert.Equal("many-model", seen!.ModelId);
    }

    [Fact]
    [UpstreamTest(Many + "::options.onStart::should be called before doEmbed", Coverage = UpstreamCoverage.Covered)]
    public async Task Calls_embed_many_start_before_the_model()
    {
        var order = new List<string>();
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake { Before = () => order.Add("model") }, Values = new[] { "a" }, OnStart = _ => { order.Add("start"); return Task.CompletedTask; } });
        Assert.Equal(new[] { "start", "model" }, order);
    }

    [Fact]
    [UpstreamTest(Many + "::options.onStart::should not break embedding when callback throws", Coverage = UpstreamCoverage.Covered)]
    public async Task Continues_embed_many_when_start_throws()
    {
        var result = await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake(), Values = new[] { "a" }, OnStart = _ => throw new InvalidOperationException("start") });
        Assert.Single(result.Embeddings);
    }

    [Fact]
    [UpstreamTest(Many + "::options.onStart::should include providerOptions and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_embed_many_options_on_start()
    {
        EmbedStartEvent? seen = null;
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest
        {
            Model = new EmbedFake(),
            Values = new[] { "a" },
            Headers = new Dictionary<string, string> { ["X"] = "1" },
            ProviderOptions = OperationJson.Parse("{\"k\":true}"),
            OnStart = item => { seen = item; return Task.CompletedTask; },
        });
        Assert.Equal("1", seen!.Headers["x"]);
        Assert.True(seen.ProviderOptions!.Value.GetProperty("k").GetBoolean());
    }

    [Fact]
    [UpstreamTest(Many + "::options.onEnd::should send correct event information (single call path)", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_embed_many_end_for_one_call()
    {
        EmbedEndEvent? seen = null;
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake(), Values = new[] { "a" }, GenerateCallId = () => "end", OnEnd = item => { seen = item; return Task.CompletedTask; } });
        Assert.Equal("end", seen!.CallId);
        Assert.Equal("ai.embedMany", seen.OperationId);
    }

    [Fact]
    [UpstreamTest(Many + "::options.onEnd::should send correct event information (chunked path)", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_embed_many_end_for_chunks()
    {
        EmbedEndEvent? seen = null;
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake { MaxEmbeddingsPerCall = 1 }, Values = new[] { "a", "b" }, MaxRetries = 0, OnEnd = item => { seen = item; return Task.CompletedTask; } });
        Assert.Equal(2, ((IReadOnlyList<double[]>)seen!.Embedding).Count);
        Assert.Equal(2, ((IReadOnlyList<ProviderResponse?>)seen.Response!).Count);
    }

    [Fact]
    [UpstreamTest(Many + "::options.onEnd::should include embeddings and usage in event", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_embeddings_and_usage_on_end()
    {
        EmbedEndEvent? seen = null;
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake { Respond = _ => Vector(new[] { 3.0 }, 9) }, Values = new[] { "a" }, OnEnd = item => { seen = item; return Task.CompletedTask; } });
        Assert.Equal((double?)9, seen!.Usage.Tokens);
        Assert.Equal(3, ((IReadOnlyList<double[]>)seen.Embedding)[0][0]);
    }

    [Fact]
    [UpstreamTest(Many + "::options.onEnd::should include model information", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_embed_many_model_on_end()
    {
        EmbedEndEvent? seen = null;
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake { Provider = "prov" }, Values = new[] { "a" }, OnEnd = item => { seen = item; return Task.CompletedTask; } });
        Assert.Equal("prov", seen!.Provider);
    }

    [Fact]
    [UpstreamTest(Many + "::options.onEnd::should include responses data", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_responses_on_end()
    {
        EmbedEndEvent? seen = null;
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake { Respond = _ => new EmbeddingModelResponse(new[] { new[] { 1.0 } }, 1, response: new ProviderResponse(id: "body")) }, Values = new[] { "a" }, OnEnd = item => { seen = item; return Task.CompletedTask; } });
        Assert.Equal("body", ((IReadOnlyList<ProviderResponse?>)seen!.Response!)[0]!.Id);
    }

    [Fact]
    [UpstreamTest(Many + "::options.onEnd::should be called after doEmbed", Coverage = UpstreamCoverage.Covered)]
    public async Task Calls_embed_many_end_after_the_model()
    {
        var order = new List<string>();
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake { Before = () => order.Add("model") }, Values = new[] { "a" }, OnEnd = _ => { order.Add("end"); return Task.CompletedTask; } });
        Assert.Equal(new[] { "model", "end" }, order);
    }

    [Fact]
    [UpstreamTest(Many + "::options.onEnd::should not break embedding when callback throws", Coverage = UpstreamCoverage.Covered)]
    public async Task Continues_embed_many_when_end_throws()
    {
        var result = await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake(), Values = new[] { "a" }, OnEnd = _ => throw new InvalidOperationException("end") });
        Assert.Single(result.Embeddings);
    }

    [Fact]
    [UpstreamTest(Many + "::options.onStart and onEnd together::should have consistent callId across both events", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_one_embed_many_call_id()
    {
        string? start = null;
        string? end = null;
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest
        {
            Model = new EmbedFake(),
            Values = new[] { "a" },
            GenerateCallId = () => "shared",
            OnStart = item => { start = item.CallId; return Task.CompletedTask; },
            OnEnd = item => { end = item.CallId; return Task.CompletedTask; },
        });
        Assert.Equal("shared", end);
        Assert.Equal(start, end);
    }

    [Fact]
    [UpstreamTest(Many + "::options.onStart and onEnd together::should call onStart before doEmbed and onEnd after", Coverage = UpstreamCoverage.Covered)]
    public async Task Orders_embed_many_callbacks()
    {
        var order = new List<string>();
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest
        {
            Model = new EmbedFake { Before = () => order.Add("model") },
            Values = new[] { "a" },
            OnStart = _ => { order.Add("start"); return Task.CompletedTask; },
            OnEnd = _ => { order.Add("end"); return Task.CompletedTask; },
        });
        Assert.Equal(new[] { "start", "model", "end" }, order);
    }

    [Fact]
    [UpstreamTest(Many + "::options.onStart and onEnd together::should still call onEnd when onStart throws", Coverage = UpstreamCoverage.Covered)]
    public async Task Still_ends_embed_many_when_start_throws()
    {
        var ended = false;
        await EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = new EmbedFake(), Values = new[] { "a" }, OnStart = _ => throw new InvalidOperationException("start"), OnEnd = _ => { ended = true; return Task.CompletedTask; } });
        Assert.True(ended);
    }

    private static EmbeddingModelResponse Vector(double[] vector, double tokens)
    {
        return new EmbeddingModelResponse(new[] { vector }, tokens);
    }

    private sealed class EmbedFake : IEmbeddingCaller
    {
        public string Provider { get; set; } = "test-provider";

        public string ModelId { get; set; } = "test-model";

        public string SpecificationVersion { get; set; } = "v4";

        public int? MaxEmbeddingsPerCall { get; set; }

        public double? MaxInputBytesPerCall { get; set; }

        public bool SupportsParallelCalls { get; set; } = true;

        public Func<EmbeddingModelCall, EmbeddingModelResponse>? Respond { get; set; }

        public Func<EmbeddingModelCall, Task<EmbeddingModelResponse>>? RespondAsync { get; set; }

        public Func<JsonElement?, IReadOnlyList<string>, int, int, JsonElement?>? Transform { get; set; }

        public Action? Before { get; set; }

        public List<EmbeddingModelCall> Calls { get; } = new List<EmbeddingModelCall>();

        public Task<JsonElement?> TransformProviderOptionsAsync(JsonElement? providerOptions, IReadOnlyList<string> values, int startIndex, int endIndex, CancellationToken cancellationToken)
        {
            return Task.FromResult(Transform == null ? providerOptions : Transform(providerOptions, values, startIndex, endIndex));
        }

        public async Task<EmbeddingModelResponse> DoEmbedAsync(EmbeddingModelCall call, CancellationToken cancellationToken)
        {
            Calls.Add(call);
            Before?.Invoke();
            if (RespondAsync != null)
            {
                return await RespondAsync(call);
            }

            if (Respond != null)
            {
                return Respond(call);
            }

            return new EmbeddingModelResponse(call.Values.Select(_ => new[] { 0.1, 0.2 }).ToArray(), 10);
        }
    }
}
