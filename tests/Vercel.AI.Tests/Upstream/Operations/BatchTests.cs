// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests;

/// <summary>Upstream parity for batch operations.</summary>
[Collection("WarningLog")]
public sealed class BatchTests
{
    private const string Prefix = "packages/ai/src/batch/batch.test.ts::";

    [Fact]
    [UpstreamTest(Prefix + "cancelBatch::requests cancellation and returns provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Requests_cancellation_and_returns_metadata()
    {
        var api = new BatchFake();
        var result = await Batch.CancelBatchAsync(new BatchRequestOptions { Provider = api, Batch = new BatchReference(2, "batch-1", "test") });
        Assert.Equal("batch-1", api.Last!.BatchId);
        Assert.Equal("cancelling", result.Status);
        Assert.Equal("test", result.ProviderMetadata!.Value.GetProperty("provider").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "cancelBatch::throws when cancellation is unsupported", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_cancellation_is_unsupported()
    {
        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => Batch.CancelBatchAsync(new BatchRequestOptions { Provider = new BatchFake { CanCancel = false }, Batch = new BatchReference(2, "batch-1", "test") }));
        Assert.Equal("batch cancellation", error.Functionality);
        Assert.Equal("The provider does not support batch cancellation.", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "listBatches::preserves the batch API as the method receiver", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_the_batch_api_as_the_receiver()
    {
        var api = new BatchFake();
        await Batch.ListBatchesAsync(new BatchRequestOptions { Provider = api });
        Assert.Same(api, api.Last!.Receiver);
    }

    [Fact]
    [UpstreamTest(Prefix + "listBatches::returns normalized batch references and the next cursor", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_normalized_batch_references()
    {
        var api = new BatchFake();
        var page = await Batch.ListBatchesAsync(new BatchRequestOptions { Provider = api, Limit = 2, Cursor = "start" });
        Assert.Equal((int?)2, api.Last!.Limit);
        Assert.Equal("start", api.Last.Cursor);
        Assert.Equal(2, page.Batches[0].Version);
        Assert.Equal("test", page.Batches[0].Provider);
        Assert.Equal("batch-1", page.Batches[0].Id);
        Assert.Equal("next", page.NextCursor);
        Assert.Contains("ai/0.0.0-test", api.Last.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "listBatches::throws when listing is unsupported", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_listing_is_unsupported()
    {
        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => Batch.ListBatchesAsync(new BatchRequestOptions { Provider = new BatchFake { CanList = false } }));
        Assert.Equal("batch listing", error.Functionality);
        Assert.Equal("The provider does not support listing batches.", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "startBatch::rejects unsupported request types before starting a batch", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_request_types_before_starting()
    {
        var api = new BatchFake();
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => Batch.StartBatchAsync(new BatchRequestOptions { Provider = api, Requests = new[] { new BatchRequest("a", "audio", "model") } }));
        Assert.Equal("requests", error.Parameter);
        Assert.Contains("Unsupported batch request type \"audio\".", error.Message);
        Assert.Null(api.Last);
    }

    [Fact]
    [UpstreamTest(Prefix + "startBatch::normalizes image requests before starting a batch", Coverage = UpstreamCoverage.Covered)]
    public async Task Normalizes_image_requests()
    {
        var api = new BatchFake();
        var request = new BatchRequest("image-1", "image", "image-model") { Prompt = "a cat", N = 2, Size = "1024x1024", AspectRatio = "1:1", Seed = 4 };
        await Batch.StartBatchAsync(new BatchRequestOptions { Provider = api, Requests = new[] { request } });
        var options = api.Last!.Requests![0].Options;
        Assert.Equal("image", api.Last.Requests[0].Type);
        Assert.Equal("a cat", options.GetProperty("prompt").GetString());
        Assert.Equal(2, options.GetProperty("n").GetInt32());
        Assert.Equal("1024x1024", options.GetProperty("size").GetString());
        Assert.Equal(JsonValueKind.Null, options.GetProperty("files").ValueKind);
    }

    [Fact]
    [UpstreamTest(Prefix + "startBatch::uses the global default provider when provider is omitted", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_global_default_provider()
    {
        var api = new BatchFake();
        var previous = BatchModels.Default;
        BatchModels.Default = () => new BatchProvider(api);
        try
        {
            var started = await Batch.StartBatchAsync(new BatchRequestOptions { Requests = new[] { new BatchRequest("a", "text", "language") { Prompt = "hi" } } });
            Assert.Same(api, api.Last!.Receiver);
            Assert.Equal("test", started.Provider);
            Assert.Equal(2, started.Version);
        }
        finally
        {
            BatchModels.Default = previous;
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "startBatch::resolves the batch service from a provider", Coverage = UpstreamCoverage.Covered)]
    public async Task Resolves_the_batch_service_from_a_provider()
    {
        var api = new BatchFake();
        await Batch.StartBatchAsync(new BatchRequestOptions { Provider = new BatchProvider(api), Requests = new[] { new BatchRequest("a", "text", "language") { Prompt = "hi" } } });
        Assert.Same(api, api.Last!.Receiver);
    }

    [Fact]
    [UpstreamTest(Prefix + "startBatch::normalizes requests and returns the acknowledged batch", Coverage = UpstreamCoverage.Covered)]
    public async Task Normalizes_text_requests_and_returns_the_batch()
    {
        var api = new BatchFake();
        var request = new BatchRequest("text-1", "text", "language-model")
        {
            Prompt = "Hello",
            MaxOutputTokens = 16,
            Temperature = 0.2,
            TopP = null,
            Seed = 3,
        };
        var started = await Batch.StartBatchAsync(new BatchRequestOptions { Provider = api, Requests = new[] { request }, Headers = new Dictionary<string, string> { ["X-Test"] = "1" } });
        var options = api.Last!.Requests![0].Options;
        Assert.Equal("user", options.GetProperty("prompt")[0].GetProperty("role").GetString());
        Assert.Equal("Hello", options.GetProperty("prompt")[0].GetProperty("content")[0].GetProperty("text").GetString());
        Assert.Equal(16, options.GetProperty("maxOutputTokens").GetInt32());
        Assert.Equal(JsonValueKind.Null, options.GetProperty("topP").ValueKind);
        Assert.Equal("batch-1", started.Id);
        Assert.Equal(2, started.Version);
        Assert.Equal("test", started.Provider);
        Assert.Equal("1", api.Last.Headers["x-test"]);
        Assert.Contains("ai/", api.Last.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "startBatch::rejects empty and duplicate request IDs", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_empty_and_duplicate_request_ids()
    {
        var empty = await Assert.ThrowsAsync<InvalidArgumentException>(() => Batch.StartBatchAsync(new BatchRequestOptions { Provider = new BatchFake(), Requests = Array.Empty<BatchRequest>() }));
        Assert.Contains("requests must not be empty", empty.Message);
        var blank = await Assert.ThrowsAsync<InvalidArgumentException>(() => Batch.StartBatchAsync(new BatchRequestOptions { Provider = new BatchFake(), Requests = new[] { new BatchRequest("  ", "text", "model") } }));
        Assert.Contains("request IDs must not be empty", blank.Message);
        var duplicate = await Assert.ThrowsAsync<InvalidArgumentException>(() => Batch.StartBatchAsync(new BatchRequestOptions { Provider = new BatchFake(), Requests = new[] { new BatchRequest("a", "text", "model"), new BatchRequest("a", "text", "model") } }));
        Assert.Contains("duplicate ID \"a\"", duplicate.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "startBatch::rejects providers without batch support", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_providers_without_batch_support()
    {
        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => Batch.StartBatchAsync(new BatchRequestOptions { Provider = new BatchProvider(null), Requests = new[] { new BatchRequest("a", "text", "model") } }));
        Assert.Contains("experimental_batch()", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "startBatch::forwards the webhook URL to the batch service", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_the_webhook_url()
    {
        var api = new BatchFake();
        await Batch.StartBatchAsync(new BatchRequestOptions { Provider = api, WebhookUrl = "https://example.com/hook", Requests = new[] { new BatchRequest("a", "text", "model") { Prompt = "hi" } } });
        Assert.Equal("https://example.com/hook", api.Last!.WebhookUrl);
    }

    [Fact]
    [UpstreamTest(Prefix + "startBatch::logs request warnings with the request model", Coverage = UpstreamCoverage.Covered)]
    public async Task Logs_request_warnings_with_the_request_model()
    {
        var seen = new List<WarningLogContext>();
        WarningLog.Observer = seen.Add;
        try
        {
            var api = new BatchFake { StartWarnings = new[] { new BatchRequestWarning("a", OperationWarning.Other("careful")) } };
            await Batch.StartBatchAsync(new BatchRequestOptions { Provider = api, Requests = new[] { new BatchRequest("a", "text", "language-model") { Prompt = "hi" } } });
            Assert.Equal("careful", seen[0].Warnings[0].Message);
            Assert.Equal("test", seen[0].Provider);
            Assert.Equal("language-model", seen[0].Model);
        }
        finally
        {
            WarningLog.Observer = null;
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "startBatch::forwards definition-only tools without executing them", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_tools_without_executing_them()
    {
        var executed = 0;
        var tool = new BatchTool("weather", OperationJson.Parse("{\"type\":\"object\"}"), "Forecast", (_, _) => { executed++; return Task.FromResult<object?>("nope"); });
        var api = new BatchFake();
        await Batch.StartBatchAsync(new BatchRequestOptions { Provider = api, Requests = new[] { new BatchRequest("a", "text", "model") { Prompt = "hi", Tools = new[] { tool }, ToolChoice = "required" } } });
        var tools = api.Last!.Requests![0].Options.GetProperty("tools");
        Assert.Equal("function", tools[0].GetProperty("type").GetString());
        Assert.Equal("weather", tools[0].GetProperty("name").GetString());
        Assert.Equal("Forecast", tools[0].GetProperty("description").GetString());
        Assert.Equal("required", api.Last.Requests[0].Options.GetProperty("toolChoice").GetProperty("type").GetString());
        Assert.Equal(0, executed);
    }

    [Fact]
    [UpstreamTest(Prefix + "startBatch::rejects incompatible definitions for the same tool name", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_incompatible_tool_definitions()
    {
        var first = new BatchRequest("a", "text", "model") { Prompt = "hi", Tools = new[] { new BatchTool("weather", OperationJson.Parse("{\"type\":\"object\"}"), "One") } };
        var second = new BatchRequest("b", "text", "model") { Prompt = "hi", Tools = new[] { new BatchTool("weather", OperationJson.Parse("{\"type\":\"object\"}"), "Two") } };
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => Batch.StartBatchAsync(new BatchRequestOptions { Provider = new BatchFake(), Requests = new[] { first, second } }));
        Assert.Contains("tool \"weather\" must have the same definition in every batch request", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "getBatchStatus::returns the latest status without the batch reference", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_latest_status()
    {
        var api = new BatchFake();
        var status = await Batch.GetBatchStatusAsync(new BatchRequestOptions { Provider = api, Batch = new BatchReference(2, "batch-1", "test") });
        Assert.Equal("batch-1", api.Last!.BatchId);
        Assert.Equal("in_progress", status.Status);
        Assert.Equal(3, status.RequestCounts!.Total);
        Assert.IsNotType<BatchStartResult>(status);
    }

    [Fact]
    [UpstreamTest(Prefix + "getBatchStatus::rejects an incompatible provider", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_incompatible_provider()
    {
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => Batch.GetBatchStatusAsync(new BatchRequestOptions { Provider = new BatchFake(), Batch = new BatchReference(2, "batch-1", "other") }));
        Assert.Contains("provider test is not compatible with batch provider other", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "getBatchStatus::rejects a version 1 batch reference", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_a_version_1_reference()
    {
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => Batch.GetBatchStatusAsync(new BatchRequestOptions { Provider = new BatchFake(), Batch = new BatchReference(1, "batch-1", "test") }));
        Assert.Contains("batch must be a supported batch reference", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "getBatchResults::opens eagerly and streams normalized item results", Coverage = UpstreamCoverage.Covered)]
    public async Task Opens_results_eagerly_and_streams_them()
    {
        var api = new BatchFake();
        var stream = Batch.GetBatchResults(new BatchRequestOptions { Provider = api, Batch = new BatchReference(2, "batch-1", "test") });
        await Task.Delay(30);
        Assert.Equal(1, api.ResultOpens);
        var results = await Read(stream);
        Assert.Equal(2, results.Count);
        Assert.Equal("Hello", results[0].Text);
        Assert.Equal((int?)8, results[0].Usage!.TotalTokens);
        Assert.Equal("failed", results[1].Status);
        Assert.Equal("bad", results[1].ErrorMessage);
    }

    [Fact]
    [UpstreamTest(Prefix + "getBatchResults::normalizes provider-executed tool content and preserves usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Normalizes_provider_executed_tools_and_usage()
    {
        var api = new BatchFake { Items = new[] { ToolItem(providerExecuted: true) } };
        var executed = 0;
        var results = await Read(Batch.GetBatchResults(new BatchRequestOptions
        {
            Provider = api,
            Batch = new BatchReference(2, "batch-1", "test"),
            Tools = new[] { new BatchTool("weather", OperationJson.Parse("{}"), execute: (_, _) => { executed++; return Task.FromResult<object?>("local"); }) },
        }));
        var call = results[0].Content![0];
        Assert.Equal("tool-call", call.Type);
        Assert.Equal("Paris", call.Input!.Value.GetProperty("city").GetString());
        Assert.True(call.ProviderExecuted);
        var usage = results[0].Usage!;
        Assert.Equal((int?)3, usage.InputTokens);
        Assert.Equal((int?)2, usage.NoCacheTokens);
        Assert.Equal((int?)1, usage.CacheReadTokens);
        Assert.Equal((int?)5, usage.OutputTokens);
        Assert.Equal((int?)4, usage.TextTokens);
        Assert.Equal((int?)1, usage.ReasoningTokens);
        Assert.Equal((int?)8, usage.TotalTokens);
        Assert.Equal(0, executed);
    }

    [Fact]
    [UpstreamTest(Prefix + "getBatchResults::normalizes successful image results", Coverage = UpstreamCoverage.Covered)]
    public async Task Normalizes_successful_image_results()
    {
        var item = new BatchItem("image", "image-1", "succeeded")
        {
            Image = new BatchImageGeneration
            {
                Images = new object?[] { "aGVsbG8=" },
                Warnings = new[] { OperationWarning.Other("image") },
                ProviderMetadata = OperationJson.Parse("{\"openai\":{\"images\":[{\"revised\":\"cat\"}]}}"),
                Usage = new OperationUsage(4, 0, 4),
            },
        };
        var results = await Read(Batch.GetBatchResults(new BatchRequestOptions { Provider = new BatchFake { Items = new[] { item } }, Batch = new BatchReference(2, "batch-1", "test") }));
        var image = results[0].Images![0];
        Assert.Equal("hello", Encoding.UTF8.GetString(image.Data));
        Assert.Equal("cat", image.ProviderMetadata!.Value.GetProperty("openai").GetProperty("revised").GetString());
        Assert.Equal((int?)4, results[0].ImageUsage!.InputTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "getBatchResults::normalizes client tool calls with their definitions without executing them", Coverage = UpstreamCoverage.Covered)]
    public async Task Copies_pending_tool_input_without_executing()
    {
        var executed = 0;
        var results = await Read(Batch.GetBatchResults(new BatchRequestOptions
        {
            Provider = new BatchFake { Items = new[] { ToolItem(providerExecuted: false, includeResult: true) } },
            Batch = new BatchReference(2, "batch-1", "test"),
            Tools = new[] { new BatchTool("weather", OperationJson.Parse("{}"), execute: (_, _) => { executed++; return Task.FromResult<object?>("local"); }) },
        }));
        Assert.Equal("Paris", results[0].Content![1].Input!.Value.GetProperty("city").GetString());
        Assert.Equal(0, executed);
    }

    [Fact]
    [UpstreamTest(Prefix + "batch generated file downloads::downloads %s content", Coverage = UpstreamCoverage.Covered)]
    public async Task Downloads_generated_file_content()
    {
        foreach (var type in new[] { "file", "reasoning-file" })
        {
            var results = await Read(Batch.GetBatchResults(new BatchRequestOptions
            {
                Provider = new BatchFake { Items = new[] { FileItem(type) } },
                Batch = new BatchReference(2, "batch-1", "test"),
                Download = (_, _) => Task.FromResult(new DownloadedMedia(Encoding.UTF8.GetBytes("Hello World"), "text/plain")),
            }));
            var file = results[0].Content![0];
            Assert.Equal(type, file.Type);
            Assert.Equal("Hello World", Encoding.UTF8.GetString(file.FileBytes!));
            Assert.Equal("SGVsbG8gV29ybGQ=", Convert.ToBase64String(file.FileBytes!));
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "batch generated file downloads::cancels in-flight downloads on %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_in_flight_downloads()
    {
        foreach (var mode in new[] { "abort", "timeout" })
        {
            using var source = new CancellationTokenSource();
            var started = new TaskCompletionSource<bool>();
            CancellationToken seen = default;
            var stream = Batch.GetBatchResults(new BatchRequestOptions
            {
                Provider = new BatchFake { Items = new[] { FileItem("file") } },
                Batch = new BatchReference(2, "batch-1", "test"),
                CancellationToken = source.Token,
                TimeoutMs = mode == "timeout" ? 80 : null,
                Download = async (_, token) =>
                {
                    seen = token;
                    started.TrySetResult(true);
                    await Task.Delay(Timeout.Infinite, token);
                    return new DownloadedMedia(Array.Empty<byte>(), null);
                },
            });
            var reading = Read(stream);
            await started.Task;
            if (mode == "abort")
            {
                source.Cancel();
            }

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => reading);
            Assert.True(seen.IsCancellationRequested);
        }
    }

    private static async Task<List<BatchItemResult>> Read(IAsyncEnumerable<BatchItemResult> stream)
    {
        var results = new List<BatchItemResult>();
        await foreach (var item in stream)
        {
            results.Add(item);
        }

        return results;
    }

    private static BatchItem ToolItem(bool providerExecuted, bool includeResult = false)
    {
        var content = new List<BatchContentPart>
        {
            new BatchContentPart("tool-call") { ToolCallId = "call-1", ToolName = "weather", Text = "{\"city\":\"Paris\"}", ProviderExecuted = providerExecuted },
        };
        if (includeResult)
        {
            content.Add(new BatchContentPart("tool-result") { ToolCallId = "call-1", ToolName = "weather", Output = OperationJson.Parse("{\"ok\":true}") });
        }

        return new BatchItem("text", "request", "succeeded")
        {
            Text = new BatchTextGeneration
            {
                Content = content,
                FinishReason = "tool-calls",
                Usage = new BatchModelUsage(3, 2, 1, outputTotal: 5, text: 4, reasoning: 1),
            },
        };
    }

    private static BatchItem FileItem(string type)
    {
        return new BatchItem("text", "request", "succeeded")
        {
            Text = new BatchTextGeneration
            {
                Content = new[] { new BatchContentPart(type) { MediaType = "text/plain", FileUrl = "https://example.com/batch.txt" } },
                FinishReason = "stop",
                Usage = new BatchModelUsage(3, 2, 1, outputTotal: 5, text: 4, reasoning: 1),
            },
        };
    }

    private sealed class BatchProvider : IBatchProvider
    {
        private readonly IBatchApi? _api;

        public BatchProvider(IBatchApi? api)
        {
            _api = api;
        }

        public IBatchApi? ExperimentalBatch()
        {
            return _api;
        }
    }

    private sealed class BatchFake : IBatchApi
    {
        public string Provider => "test";

        public bool CanCancel { get; set; } = true;

        public bool CanList { get; set; } = true;

        public BatchOperationCall? Last { get; private set; }

        public int ResultOpens { get; private set; }

        public IReadOnlyList<BatchRequestWarning> StartWarnings { get; set; } = Array.Empty<BatchRequestWarning>();

        public IReadOnlyList<BatchItem> Items { get; set; } = new[]
        {
            new BatchItem("text", "request", "succeeded")
            {
                Text = new BatchTextGeneration
                {
                    Content = new[] { new BatchContentPart("text") { Text = "Hello" } },
                    FinishReason = "stop",
                    RawFinishReason = "stop",
                    Usage = new BatchModelUsage(3, 2, 1, outputTotal: 5, text: 4, reasoning: 1),
                },
            },
            new BatchItem("text", "failed", "failed") { ErrorMessage = "bad", ErrorCode = "bad" },
        };

        public Task<BatchStartResult> DoStartAsync(BatchOperationCall call, CancellationToken cancellationToken)
        {
            Last = call;
            return Task.FromResult(new BatchStartResult { Id = "batch-1", Status = "submitted", Warnings = StartWarnings, ProviderMetadata = OperationJson.Parse("{\"provider\":\"test\"}") });
        }

        public Task<BatchStatus> DoGetStatusAsync(BatchOperationCall call, CancellationToken cancellationToken)
        {
            Last = call;
            return Task.FromResult(new BatchStatus { Status = "in_progress", RawStatus = "in_progress", RequestCounts = new BatchRequestCounts(3, 1, 2, 0) });
        }

        public Task<IReadOnlyList<BatchItem>> DoGetResultsAsync(BatchOperationCall call, CancellationToken cancellationToken)
        {
            Last = call;
            ResultOpens++;
            return Task.FromResult(Items);
        }

        public Task<BatchCancelResult> DoCancelAsync(BatchOperationCall call, CancellationToken cancellationToken)
        {
            Last = call;
            return Task.FromResult(new BatchCancelResult { Status = "cancelling", ProviderMetadata = OperationJson.Parse("{\"provider\":\"test\"}") });
        }

        public Task<BatchListResult> DoListAsync(BatchOperationCall call, CancellationToken cancellationToken)
        {
            Last = call;
            return Task.FromResult(new BatchListResult(new[] { new ListedBatch { Id = "batch-1", Status = "in_progress" } }, "next"));
        }
    }
}
