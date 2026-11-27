// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; code execution file uploads</c>.</summary>
public sealed class DoStreamCodeExecutionFileUploadsTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream > code execution file uploads::";

    [Fact]
    [UpstreamTest(
        Prefix + "should stream container upload code execution results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_container_upload_code_execution_results()
    {
        var handler = new CaptureHandler
        {
            ContentType = "text/event-stream",
            ResponseBody = string.Concat(
                SseStreamHandler.FixtureChunks("anthropic-code-execution-file-upload.1", done: true)
            ),
        };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(
                    new UserContentPart[]
                    {
                        new TextContentPart("Analyze this CSV data."),
                        FileContentPart.FromProviderReference(
                            "text/csv",
                            new Dictionary<string, string> { ["anthropic"] = "file-csv-12345" },
                            null,
                            new Dictionary<string, JsonElement>
                            {
                                ["anthropic"] = AnthropicParity.Json("{\"containerUpload\":true}"),
                            }
                        ),
                    }
                ),
            },
            Tools = new[]
            {
                LanguageModelTool.ProviderDefined(
                    "anthropic.code_execution_20250825",
                    "code_execution",
                    AnthropicParity.Json("{}")
                ),
            },
        };

        var parts = await DoStreamSupport.Collect(
            AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-api-key" }, handler),
            "claude-3-haiku-20240307",
            options
        );

        DoStreamSupport.AssertSnapshot(
            JsonNode.Parse(handler.Body),
            "container-upload-request-body"
        );
        DoStreamSupport.AssertSnapshot(
            DoStreamSupport.RequestHeaders(handler),
            "container-upload-request-headers"
        );

        var toolCalls = parts.OfType<ToolCallStreamPart>().ToList();
        var toolResults = parts.OfType<ToolResultStreamPart>().ToList();
        var first = Assert.Single(toolCalls, call =>
            call.ToolCallId == "srvtoolu_01UAM7DM8XEfNwyddFNKpVp2"
        );
        Assert.Equal("code_execution", first.ToolName);
        Assert.Contains("$INPUT_DIR/sample.csv", first.ArgumentsJson);
        Assert.True(first.ProviderExecuted);
        var firstResult = Assert.Single(toolResults, result =>
            result.ToolCallId == "srvtoolu_01UAM7DM8XEfNwyddFNKpVp2"
        );
        Assert.Equal("code_execution", firstResult.ToolName);
        Assert.Equal(
            "text_editor_code_execution_view_result",
            firstResult.Result.GetProperty("type").GetString()
        );
        Assert.Contains(
            "month,revenue,expenses,profit",
            firstResult.Result.GetProperty("content").GetString()
        );
        var second = Assert.Single(toolCalls, call =>
            call.ToolCallId == "srvtoolu_01P2RuXQdkVngtqpdr2dQhv2"
        );
        Assert.Equal("code_execution", second.ToolName);
        Assert.Contains("python /tmp/analyze_data.py", second.ArgumentsJson);
        Assert.True(second.ProviderExecuted);
        var secondResult = Assert.Single(toolResults, result =>
            result.ToolCallId == "srvtoolu_01P2RuXQdkVngtqpdr2dQhv2"
        );
        Assert.Equal("code_execution", secondResult.ToolName);
        Assert.Equal(
            "bash_code_execution_result",
            secondResult.Result.GetProperty("type").GetString()
        );
        Assert.Contains(
            "Total Profit: $35,400.00",
            secondResult.Result.GetProperty("stdout").GetString()
        );
        var finish = DoStreamSupport.Finish(parts);
        Assert.Equal(
            "container_011CbJQ7DqpL337rdwQ76jnu",
            finish
                .ProviderMetadata!.Value.GetProperty("anthropic")
                .GetProperty("container")
                .GetProperty("id")
                .GetString()
        );
    }
}
