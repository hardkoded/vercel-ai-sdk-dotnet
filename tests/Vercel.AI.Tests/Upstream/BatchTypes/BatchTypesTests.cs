// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;
using System.Text.Json;
using Vercel.AI.Gateway;
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
        Prefix + "exposes typed Gateway async-job metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Exposes_typed_Gateway_async_job_metadata()
    {
        Assert.Equal(typeof(string), PropertyType<GatewayAsyncJobMetadata>("WebhookSigningSecret"));
        Assert.Equal(
            typeof(GatewayAsyncJobMetadata),
            PropertyType<GatewayProviderMetadata>("AsyncJob")
        );

        var providerMetadata = JsonDocument
            .Parse(
                "{\"gateway\":{\"asyncJob\":{\"jobId\":\"job_123\",\"status\":\"queued\",\"webhookSigningSecret\":\"whsec_123\"},\"extra\":1}}"
            )
            .RootElement;
        var gateway = GatewayProviderMetadata.From(providerMetadata);

        Assert.Equal("job_123", gateway!.AsyncJob!.JobId);
        Assert.Equal("queued", gateway.AsyncJob.Status);
        Assert.Equal("whsec_123", gateway.AsyncJob.WebhookSigningSecret);
        Assert.True(gateway.AdditionalProperties.ContainsKey("extra"));
        Assert.Null(GatewayProviderMetadata.From(JsonDocument.Parse("{}").RootElement));
    }

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
        var options = new StartBatchOptions { Output = OutputSpec.Object<TextOutput>("{}") };

        Assert.IsAssignableFrom<OutputSpec<TextOutput>>(options.Output);
        Assert.Null(new StartBatchOptions().Output);
    }

    private static Type PropertyType<T>(string name) =>
        typeof(T).GetProperty(name)?.PropertyType
        ?? throw new InvalidOperationException($"{typeof(T).Name}.{name} does not exist.");

    [Fact]
    [UpstreamTest(
        Prefix + "uses a modality-independent batch reference",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Uses_a_modality_independent_batch_reference()
    {
        // Batch (a start result or a listed batch) is a BatchReference, and status takes a BatchReference.
        Assert.True(typeof(BatchReference).IsAssignableFrom(typeof(BatchStartResult)));
        Assert.True(typeof(BatchReference).IsAssignableFrom(typeof(ListedBatch)));
        Assert.Equal(typeof(BatchReference), PropertyType<BatchRequestOptions>("Batch"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps batch start non-retrying",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Keeps_batch_start_non_retrying()
    {
        Assert.Null(typeof(StartBatchOptions).GetProperty("MaxRetries"));
        Assert.Equal(typeof(int?), PropertyType<StartBatchOptions>("TimeoutMs"));
        Assert.Equal(typeof(string), PropertyType<StartBatchOptions>("WebhookUrl"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps text-specific options on text batch requests",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Keeps_text_specific_options_on_text_batch_requests()
    {
        var request = new TextBatchRequest("request-1", "model-a");
        Assert.Equal("text", request.Type);
        Assert.Equal(typeof(string), PropertyType<TextBatchRequest>("Model"));
        Assert.Equal(typeof(IReadOnlyList<BatchTool>), PropertyType<TextBatchRequest>("Tools"));
        Assert.Null(typeof(TextBatchRequest).GetProperty("StopWhen"));
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
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Uses_serializable_response_timestamps()
    {
        // Like upstream, the result timestamp is a string (ISO 8601), so the result serializes.
        Assert.Equal(typeof(string), PropertyType<BatchItemResult>("Timestamp"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "exposes Core content in successful batch results",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Exposes_Core_content_in_successful_batch_results()
    {
        // The provider result carries provider content parts; the Core item exposes the generate-text content parts.
        Assert.Equal(
            typeof(IReadOnlyList<BatchContentPart>),
            PropertyType<BatchTextGeneration>("Content")
        );
        Assert.Equal(
            typeof(IReadOnlyList<GeneratedContent>),
            PropertyType<BatchItemResult>("Content")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "exposes modality-specific call options to batch providers",
        Coverage = UpstreamCoverage.Partial,
        Commit = Commit
    )]
    public async Task Exposes_modality_specific_call_options_to_batch_providers()
    {
        // BatchV4Request has exactly id, type, modelId, and options.
        Assert.Equal(
            ["Id", "ModelId", "Options", "Type"],
            typeof(NormalizedBatchRequest).GetProperties().Select(p => p.Name).Order().ToArray()
        );
        Assert.Equal("text", new TextBatchRequest("a", "model-a").Type);
        Assert.Equal("model-a", new TextBatchRequest("a", "model-a").Model);
        Assert.Equal("image", new ImageBatchRequest("a", "image-a").Type);

        var fake = new BatchTests.BatchFake { Items = [] };
        await BatchApi.StartBatchAsync(
            new StartBatchOptions
            {
                Provider = ApiOrProvider<IBatchApi, IBatchProvider>.FromApi(fake),
                Requests =
                [
                    new TextBatchRequest("text-1", "model-a")
                    {
                        Prompt = "hello",
                        MaxOutputTokens = 1,
                        Temperature = 1,
                        StopSequences = ["x"],
                        TopP = 1,
                        TopK = 1,
                        PresencePenalty = 1,
                        FrequencyPenalty = 1,
                        Seed = 1,
                        Reasoning = "low",
                        ProviderOptions = JsonDocument.Parse("{}").RootElement,
                    },
                    new ImageBatchRequest("image-1", "image-a")
                    {
                        Prompt = "hello",
                        N = 1,
                        Size = "1x1",
                        AspectRatio = "1:1",
                        Seed = 1,
                    },
                ],
                Output = OutputSpec.Object<TextOutput>("{}"),
            }
        );

        var started = fake.Last!.Requests!;
        var text = started.Single(r => r.Type == "text");
        var image = started.Single(r => r.Type == "image");
        Assert.Equal("model-a", text.ModelId);
        var textKeys = text.Options.EnumerateObject().Select(p => p.Name).ToHashSet();
        var imageKeys = image.Options.EnumerateObject().Select(p => p.Name).ToHashSet();
        string[] allowedText =
        [
            "prompt",
            "maxOutputTokens",
            "temperature",
            "stopSequences",
            "topP",
            "topK",
            "presencePenalty",
            "frequencyPenalty",
            "seed",
            "reasoning",
            "responseFormat",
            "toolChoice",
            "tools",
            "providerOptions",
        ];
        string[] allowedImage =
        [
            "prompt",
            "n",
            "size",
            "aspectRatio",
            "seed",
            "files",
            "mask",
            "providerOptions",
        ];
        Assert.True(textKeys.IsSubsetOf(allowedText));
        Assert.Contains("responseFormat", textKeys);
        Assert.DoesNotContain("includeRawChunks", textKeys);
        Assert.DoesNotContain("abortSignal", textKeys);
        Assert.DoesNotContain("headers", textKeys);
        Assert.True(imageKeys.IsSubsetOf(allowedImage));
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

        // BatchV4<{ text: 'model-a' | 'model-b'; image: 'image-model' }>: the batch API declares its model ids.
        Assert.Equal(
            typeof(BatchModelIds),
            typeof(IBatchApi).GetProperty("ModelIds")!.PropertyType
        );
        Assert.Equal(
            typeof(IReadOnlyList<string>),
            typeof(BatchModelIds).GetProperty("Text")!.PropertyType
        );
        Assert.Equal(
            typeof(IReadOnlyList<string>),
            typeof(BatchModelIds).GetProperty("Image")!.PropertyType
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "infers per-request model IDs from the provider batch service",
        Coverage = UpstreamCoverage.Partial,
        Commit = Commit
    )]
    public async Task Infers_per_request_model_IDs_from_the_provider_batch_service()
    {
        // C# has no string-literal unions: the check happens when the batch starts, not at compile time.
        var fake = new BatchTests.BatchFake
        {
            Items = [],
            ModelIds = new BatchModelIds { Text = ["model-a", "model-b"], Image = ["image-model"] },
        };
        var provider = ApiOrProvider<IBatchApi, IBatchProvider>.FromApi(fake);

        await BatchApi.StartBatchAsync(
            new StartBatchOptions
            {
                Provider = provider,
                Requests = [new TextBatchRequest("request-1", "model-b") { Prompt = "hello" }],
            }
        );
        await BatchApi.StartBatchAsync(
            new StartBatchOptions
            {
                Provider = provider,
                Requests = [new ImageBatchRequest("request-1", "image-model") { Prompt = "hello" }],
            }
        );

        // request model ID is checked against provider.ExperimentalBatch().ModelIds
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            BatchApi.StartBatchAsync(
                new StartBatchOptions
                {
                    Provider = provider,
                    Requests =
                    [
                        new TextBatchRequest("request-1", "image-model") { Prompt = "hello" },
                    ],
                }
            )
        );
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
