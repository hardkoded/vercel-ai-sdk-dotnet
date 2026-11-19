// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
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

        await foreach (var item in BatchApi.GetBatchResults(request, output))
        {
            // The item is a BatchItemResult<TextOutput>: Output is TextOutput?, OutputError is BatchError?.
            BatchItemResult<TextOutput> typed = item;
            TextOutput? parsed = typed.Output;
            BatchError? error = typed.OutputError;

            Assert.Null(error);
            Assert.Equal(new TextOutput("hello"), parsed);
        }

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
