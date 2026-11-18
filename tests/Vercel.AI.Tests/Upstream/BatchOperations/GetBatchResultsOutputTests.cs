// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Tests;
using BatchApi = Vercel.AI.Operations.Batch;

namespace Vercel.AI.Tests.Upstream.BatchOperations;

/// <summary>Upstream parity for <c>describe('output')</c> inside <c>getBatchResults</c> in <c>batch.test.ts</c>.</summary>
public sealed class GetBatchResultsOutputTests
{
    private const string Prefix = "packages/ai/src/batch/batch.test.ts::";

    private const string OutputSchema =
        "{\"type\":\"object\",\"properties\":{\"text\":{\"type\":\"string\"}},\"required\":[\"text\"]}";

    [Fact]
    [UpstreamTest(
        Prefix + "getBatchResults > output::parses and validates the text of successful items",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public async Task Parses_and_validates_the_text_of_successful_items()
    {
        var results = await CollectOutput(
            new[] { OutputItem("ok", "{\"text\":\"hello\"}") },
            OutputSpec.Object(OutputSchema)
        );

        Assert.Equal("ok", results[0].Id);
        Assert.Equal("succeeded", results[0].Status);
        Assert.Equal("{\"text\":\"hello\"}", results[0].Text);
        Assert.Equal("hello", results[0].Output!.Value.GetProperty("text").GetString());
        Assert.Null(results[0].OutputError);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "getBatchResults > output::reports parse failures per item without dropping text and usage",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public async Task Reports_parse_failures_per_item_without_dropping_text_and_usage()
    {
        var results = await CollectOutput(
            new[]
            {
                OutputItem("not-json", "oops"),
                OutputItem("wrong-shape", "{\"text\":1}"),
                OutputItem("ok", "{\"text\":\"fine\"}"),
            },
            OutputSpec.Object(OutputSchema)
        );

        Assert.Equal("not-json", results[0].Id);
        Assert.Equal("succeeded", results[0].Status);
        Assert.Equal("oops", results[0].Text);
        Assert.Equal(3, results[0].Usage!.InputTokens);
        Assert.Equal("AI_NoObjectGeneratedError", results[0].OutputError!.Type);
        Assert.Equal(
            "No object generated: could not parse the response.",
            results[0].OutputError!.Message
        );
        Assert.Null(results[0].Output);
        Assert.Equal("wrong-shape", results[1].Id);
        Assert.Equal("AI_NoObjectGeneratedError", results[1].OutputError!.Type);
        Assert.Equal(
            "No object generated: response did not match schema.",
            results[1].OutputError!.Message
        );
        Assert.Equal("ok", results[2].Id);
        Assert.Equal("fine", results[2].Output!.Value.GetProperty("text").GetString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "getBatchResults > output::prefers structuredOutputText for JSON output",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public async Task Prefers_structuredOutputText_for_JSON_output()
    {
        var item = OutputItem("structured", "draft {\"text\":\"final\"}");
        item.Text!.StructuredOutputText = "{\"text\":\"final\"}";
        var results = await CollectOutput(new[] { item }, OutputSpec.Object(OutputSchema));

        Assert.Equal("final", results[0].Output!.Value.GetProperty("text").GetString());
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "getBatchResults > output::reports an output error when the item did not finish with a response",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public async Task Reports_an_output_error_when_the_item_did_not_finish_with_a_response()
    {
        var item = new BatchItem("text", "tool-call", "succeeded")
        {
            Text = new BatchTextGeneration
            {
                Content = new[]
                {
                    new BatchContentPart("tool-call")
                    {
                        ToolCallId = "call-1",
                        ToolName = "weather",
                        Text = "{}",
                    },
                },
                FinishReason = "tool-calls",
                RawFinishReason = "tool_use",
                Usage = new BatchModelUsage(3, 2, 1, outputTotal: 5, text: 4, reasoning: 1),
            },
        };
        var results = await CollectOutput(new[] { item }, OutputSpec.Object(OutputSchema));

        Assert.Equal("AI_NoOutputGeneratedError", results[0].OutputError!.Type);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "getBatchResults > output::does not parse failed items or add fields without output",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public async Task Does_not_parse_failed_items_or_add_fields_without_output()
    {
        var items = new[]
        {
            new BatchItem("text", "failed", "failed") { ErrorMessage = "boom" },
            OutputItem("plain", "{\"text\":\"x\"}"),
        };

        var withOutput = await CollectOutput(items, OutputSpec.Object(OutputSchema));
        Assert.Equal("failed", withOutput[0].Status);
        Assert.Equal("boom", withOutput[0].ErrorMessage);
        Assert.Null(withOutput[0].Output);
        Assert.Null(withOutput[0].OutputError);

        var withoutOutput = await CollectOutput(items, null);
        Assert.Null(withoutOutput[1].Output);
        Assert.Null(withoutOutput[1].OutputError);
    }

    private static BatchItem OutputItem(string id, string text)
    {
        return new BatchItem("text", id, "succeeded")
        {
            Text = new BatchTextGeneration
            {
                Content = new[] { new BatchContentPart("text") { Text = text } },
                FinishReason = "stop",
                RawFinishReason = "stop",
                Usage = new BatchModelUsage(3, 2, 1, outputTotal: 5, text: 4, reasoning: 1),
            },
        };
    }

    private static Task<List<BatchItemResult>> CollectOutput(
        IReadOnlyList<BatchItem> items,
        OutputSpec? output
    )
    {
        return ReadAll(
            BatchApi.GetBatchResults(
                new BatchRequestOptions
                {
                    Provider = ApiOrProvider<IBatchApi, IBatchProvider>.FromApi(
                        new BatchTests.BatchFake { Items = items }
                    ),
                    Batch = new BatchReference(2, "batch-1", "test"),
                    MaxRetries = 0,
                    Output = output,
                }
            )
        );
    }

    private static async Task<List<BatchItemResult>> ReadAll(
        IAsyncEnumerable<BatchItemResult> stream
    )
    {
        var results = new List<BatchItemResult>();
        await foreach (var item in stream)
        {
            results.Add(item);
        }

        return results;
    }
}
