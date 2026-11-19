// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using BatchApi = Vercel.AI.Operations.Batch;

namespace Vercel.AI.Tests.Upstream.BatchTypes;

/// <summary>
/// Port of <c>batch-types.test-d.ts</c>. The type tests become compile-time checks: an assignment that does not
/// compile stands for a failed <c>expectTypeOf</c>.
/// </summary>
public sealed class BatchTypesTests
{
    private const string Commit = "3bea041f28e65f371e640d2c70c302776a46387f";
    private const string Prefix = "packages/ai/src/batch/batch-types.test-d.ts::";

    private sealed record TextOutput(string Text);

    [Fact]
    [UpstreamTest(
        Prefix + "infers the parsed output of text batch results from the output specification",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Infers_the_parsed_output_of_text_batch_results_from_the_output_specification()
    {
        var output = OutputSpec.Object<TextOutput>(
            "{\"type\":\"object\",\"properties\":{\"text\":{\"type\":\"string\"}},\"required\":[\"text\"]}"
        );
        var request = Request("{\"text\":\"hello\"}");

        var count = 0;
        await foreach (var item in BatchApi.GetBatchResults(request, output))
        {
            count++;
            // The item is a BatchItemResult<TextOutput>: Output is TextOutput?, OutputError is BatchError?.
            BatchItemResult<TextOutput> typed = item;
            TextOutput? parsed = typed.Output;
            BatchError? error = typed.OutputError;

            Assert.Null(error);
            Assert.Equal(new TextOutput("hello"), parsed);
            Assert.NotNull(((BatchItemResult)typed).Output);
        }

        Assert.Equal(1, count);

        // Without an output specification the item has no typed output: Output stays the raw JSON value.
        await foreach (var item in BatchApi.GetBatchResults(request))
        {
            Assert.IsNotType<BatchItemResult<TextOutput>>(item);
            Assert.Null(item.Output);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "accepts an output specification when starting a batch",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Accepts_an_output_specification_when_starting_a_batch()
    {
        // StartBatchOptions.Output is an OutputSpec?, and a typed specification is one.
        var options = new BatchRequestOptions { Output = OutputSpec.Object<TextOutput>("{}") };

        Assert.IsAssignableFrom<OutputSpec<TextOutput>>(options.Output);
        Assert.Null(new BatchRequestOptions().Output);
    }

    private static Type PropertyType<T>(string name) =>
        typeof(T).GetProperty(name)?.PropertyType
        ?? throw new InvalidOperationException($"{typeof(T).Name}.{name} does not exist.");

    [Fact]
    [UpstreamTest(
        Prefix + "uses a modality-independent batch reference",
        Coverage = UpstreamCoverage.Partial,
        Commit = Commit
    )]
    public void Uses_a_modality_independent_batch_reference()
    {
        // GetBatchStatus takes the same BatchReference as every other batch operation. The C# start result is
        // not a BatchReference subtype: it repeats Version, Id and Provider (see the follow-up issue).
        Assert.Equal(typeof(BatchReference), PropertyType<BatchRequestOptions>("Batch"));
        foreach (var name in new[] { "Version", "Id", "Provider" })
        {
            Assert.Equal(
                typeof(BatchReference).GetProperty(name)!.PropertyType,
                typeof(BatchStartResult).GetProperty(name)!.PropertyType
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps batch start non-retrying",
        Coverage = UpstreamCoverage.Partial,
        Commit = Commit
    )]
    public void Keeps_batch_start_non_retrying()
    {
        // Covered: webhookUrl is an optional string and the timeout is optional.
        // Not covered: C# shares BatchRequestOptions between operations, so MaxRetries exists on the type.
        Assert.Equal(typeof(string), PropertyType<BatchRequestOptions>("WebhookUrl"));
        Assert.Equal(typeof(int?), PropertyType<BatchRequestOptions>("TimeoutMs"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps text-specific options on text batch requests",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Keeps_text_specific_options_on_text_batch_requests()
    {
        Assert.Equal(typeof(string), PropertyType<BatchRequest>("Type"));
        Assert.Equal(typeof(string), PropertyType<BatchRequest>("Model"));
        Assert.Equal(typeof(IReadOnlyList<BatchTool>), PropertyType<BatchRequest>("Tools"));
        Assert.Null(typeof(BatchRequest).GetProperty("StopWhen"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "accepts shared definition-only tools when starting and reading a batch",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Accepts_shared_definition_only_tools_when_starting_and_reading_a_batch()
    {
        var tools = new[]
        {
            new BatchTool(
                "weather",
                System
                    .Text.Json.JsonDocument.Parse(
                        "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}}}"
                    )
                    .RootElement,
                execute: (_, _) => Task.FromResult<object?>(new { temperature = 20 })
            ),
        };

        // The same tools go on a request and on the options used to read results.
        var request = new BatchRequest("request-1", "text", "model") { Tools = tools };
        var options = new BatchRequestOptions { Tools = tools };
        Assert.Same(tools[0], request.Tools![0]);
        Assert.Same(tools[0], options.Tools![0]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "uses serializable response timestamps",
        Coverage = UpstreamCoverage.Partial,
        Commit = Commit
    )]
    public void Uses_serializable_response_timestamps()
    {
        // Upstream types the timestamp as string. The C# result uses DateTime?, which serializes to ISO 8601.
        Assert.Equal(typeof(DateTime?), PropertyType<BatchTextGeneration>("Timestamp"));
        Assert.Equal(typeof(DateTime?), PropertyType<BatchItemResult>("Timestamp"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "exposes Core content in successful batch results",
        Coverage = UpstreamCoverage.Partial,
        Commit = Commit
    )]
    public void Exposes_Core_content_in_successful_batch_results()
    {
        // The C# item exposes BatchContentPart, not the generate-text ContentPart.
        Assert.Equal(
            typeof(IReadOnlyList<BatchContentPart>),
            PropertyType<BatchTextGeneration>("Content")
        );
        Assert.Equal(
            typeof(IReadOnlyList<BatchContentPart>),
            PropertyType<BatchItemResult>("Content")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "flattens successful Core items while reusing provider status and errors",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Flattens_successful_Core_items_while_reusing_provider_status_and_errors()
    {
        Assert.Equal(typeof(string), PropertyType<BatchItemResult>("Text"));
        Assert.Equal(typeof(string), PropertyType<BatchItemResult>("Id"));
        Assert.Null(typeof(BatchItemResult).GetProperty("Result"));
        Assert.Equal(typeof(BatchError), PropertyType<BatchItemResult>("OutputError"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "defines batch support as a standalone provider service",
        Coverage = UpstreamCoverage.Partial,
        Commit = Commit
    )]
    public void Defines_batch_support_as_a_standalone_provider_service()
    {
        // A batch API is a standalone service: a language model is not a batch provider.
        Assert.False(typeof(IBatchProvider).IsAssignableFrom(typeof(ILanguageModel)));
        Assert.Equal(
            typeof(IBatchApi),
            typeof(IBatchProvider).GetMethod("ExperimentalBatch")!.ReturnType
        );
        Assert.Equal(
            typeof(BatchOperationCall),
            typeof(IBatchApi).GetMethod("DoGetResultsAsync")!.GetParameters()[0].ParameterType
        );
        Assert.Equal(
            typeof(Task<IAsyncEnumerable<BatchItem>>),
            typeof(IBatchApi).GetMethod("DoGetResultsAsync")!.ReturnType
        );
        Assert.Equal(
            typeof(Task<BatchListResult>),
            typeof(IBatchApi).GetMethod("DoListAsync")!.ReturnType
        );
        Assert.Equal(typeof(string), PropertyType<BatchStatus>("Status"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "allows the default provider to be omitted",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Allows_the_default_provider_to_be_omitted()
    {
        // Provider is optional on the options of every batch operation.
        var options = new BatchRequestOptions { Batch = new BatchReference(2, "batch", "test") };
        Assert.Null(options.Provider);
        Assert.NotNull(typeof(BatchApi).GetMethod("GetBatchStatusAsync"));
        Assert.NotNull(typeof(BatchApi).GetMethod("CancelBatchAsync"));
        Assert.NotNull(typeof(BatchApi).GetMethod("ListBatchesAsync"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "exports the experimental batch functions with the public result types",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Exports_the_experimental_batch_functions_with_the_public_result_types()
    {
        Assert.Equal(typeof(Task<BatchStartResult>), BatchApi_Return("StartBatchAsync"));
        Assert.Equal(typeof(Task<BatchStatus>), BatchApi_Return("GetBatchStatusAsync"));
        Assert.Equal(typeof(Task<BatchCancelResult>), BatchApi_Return("CancelBatchAsync"));
        Assert.Equal(typeof(Task<BatchListResult>), BatchApi_Return("ListBatchesAsync"));
        Assert.Equal(
            typeof(IAsyncEnumerable<BatchItemResult>),
            typeof(BatchApi)
                .GetMethods()
                .Single(m => m.Name == "GetBatchResults" && !m.IsGenericMethodDefinition)
                .ReturnType
        );
    }

    private static Type BatchApi_Return(string name) =>
        typeof(BatchApi).GetMethod(name)!.ReturnType;

    private static BatchRequestOptions Request(string text)
    {
        return new BatchRequestOptions
        {
            Provider = ApiOrProvider<IBatchApi, IBatchProvider>.FromApi(
                new BatchTests.BatchFake
                {
                    Items =
                    [
                        new BatchItem("text", "ok", "succeeded")
                        {
                            Text = new BatchTextGeneration
                            {
                                Content = [new BatchContentPart("text") { Text = text }],
                                FinishReason = "stop",
                                RawFinishReason = "stop",
                            },
                        },
                    ],
                }
            ),
            Batch = new BatchReference(2, "batch-1", "test"),
            MaxRetries = 0,
        };
    }
}
