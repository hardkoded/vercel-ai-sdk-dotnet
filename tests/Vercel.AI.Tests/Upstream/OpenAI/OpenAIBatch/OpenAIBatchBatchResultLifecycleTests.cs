// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using System.Text.Json.Nodes;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIBatch.OpenAIBatchTestSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIBatch;

public sealed class OpenAIBatchBatchResultLifecycleTests
{
    [Fact]
    [UpstreamTest("packages/openai/src/openai-batch.test.ts::OpenAI batch service > batch result lifecycle::fails an invalid item and continues with later results", Coverage = UpstreamCoverage.Covered)]
    public async Task Fails_an_invalid_item_and_continues_with_later_results()
    {
        var handler = new RoutedHandler()
            .Add("https://api.openai.com/v1/batches/batch_123", "{\"id\":\"batch_123\",\"status\":\"completed\",\"output_file_id\":\"file-output\"}")
            .Add("https://api.openai.com/v1/files/file-output/content",
                "{\"custom_id\":\"invalid\",\"response\":{\"status_code\":200,\"body\":{\"output\":42}}}\n"
                + "{\"custom_id\":\"valid\",\"response\":{\"status_code\":200,\"body\":{\"id\":\"resp_123\",\"output\":[{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Paris\"}]}]}}}\n");
        var batch = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-api-key" }, handler).ExperimentalBatch()!;

        var results = await batch.DoGetResultsAsync(new BatchOperationCall("batch_123", null, new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("invalid", results[0].Id);
        Assert.Equal("failed", results[0].Status);
        Assert.Equal("OpenAI returned an invalid Responses batch result.", results[0].ErrorMessage);
        Assert.Equal("invalid_response", results[0].ErrorCode);
        Assert.Equal("valid", results[1].Id);
        Assert.Equal("succeeded", results[1].Status);
        Assert.Equal("Paris", results[1].Text!.Content[0].Text);
    }

    [Theory]
    [InlineData(true, "failed")]
    [InlineData(true, "incomplete")]
    [InlineData(false, "failed")]
    [InlineData(false, "incomplete")]
    public async Task Preserves_web_search_tool_errors_in_successful_batch_responses(bool withAction, string status)
    {
        var webSearch = new JsonObject { ["type"] = "web_search_call", ["id"] = "ws_test", ["status"] = status };
        if (withAction)
        {
            webSearch["action"] = new JsonObject { ["type"] = "search", ["query"] = "AI SDK" };
        }

        var handler = new RoutedHandler()
            .Add(BatchUrl, BatchResponse(outputFileId: "file-output"))
            .Add(OutputUrl, ResultLine("web-search", WithOutput(ResponsesResultBody(string.Empty), webSearch)));

        var item = Assert.Single(await GetResultsAsync(handler));

        Assert.Equal("web-search", item.Id);
        Assert.Equal("succeeded", item.Status);
        var content = item.Text!.Content;
        Assert.Equal(2, content.Count);
        Assert.Equal("tool-call", content[0].Type);
        Assert.Equal("ws_test", content[0].ToolCallId);
        Assert.Equal("web_search", content[0].ToolName);
        Assert.Equal("{}", content[0].Text);
        Assert.True(content[0].ProviderExecuted);
        Assert.Equal("tool-result", content[1].Type);
        Assert.Equal("ws_test", content[1].ToolCallId);
        Assert.Equal("web_search", content[1].ToolName);
        Assert.True(content[1].IsError);
        Assert.Equal(status, content[1].Output!.Value.GetProperty("status").GetString());
    }

    [Fact]
    [UpstreamTest("packages/openai/src/openai-batch.test.ts::OpenAI batch service > batch result lifecycle::preserves tool calls and fails unsupported items without stopping later results", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_tool_calls_and_fails_unsupported_items_without_stopping_later_results()
    {
        var functionCall = new JsonObject { ["type"] = "function_call", ["id"] = "function-call", ["call_id"] = "call-123", ["name"] = "get_weather", ["arguments"] = "{\"city\":\"Paris\"}", ["namespace"] = null, ["caller"] = null };
        var customToolCall = new JsonObject { ["type"] = "custom_tool_call", ["id"] = "custom-tool-call", ["call_id"] = "call-123", ["name"] = "shell", ["input"] = "echo Paris" };
        var webSearch = new JsonObject { ["type"] = "web_search_call", ["id"] = "web-search", ["status"] = "completed", ["action"] = new JsonObject { ["type"] = "search", ["query"] = "weather in Paris" } };
        var fileSearch = new JsonObject
        {
            ["type"] = "file_search_call",
            ["id"] = "file-search",
            ["queries"] = new JsonArray("weather in Paris"),
            ["results"] = new JsonArray(new JsonObject { ["attributes"] = new JsonObject { ["country"] = "France" }, ["file_id"] = "file_123", ["filename"] = "weather.md", ["score"] = 0.9, ["text"] = "Paris is sunny." }),
        };
        var image = new JsonObject { ["type"] = "image_generation_call", ["id"] = "image-123", ["result"] = "aW1hZ2U=" };
        var handler = new RoutedHandler()
            .Add(BatchUrl, BatchResponse(outputFileId: "file-output"))
            .Add(OutputUrl,
                ResultLine("function-call", WithOutput(ResponsesResultBody(string.Empty), functionCall)) + "\n"
                + ResultLine("custom-tool-call", WithOutput(ResponsesResultBody(string.Empty), customToolCall)) + "\n"
                + ResultLine("web-search", WithOutput(ResponsesResultBody(string.Empty), webSearch)) + "\n"
                + ResultLine("file-search", WithOutput(ResponsesResultBody(string.Empty), fileSearch)) + "\n"
                + ResultLine("image", WithOutput(ResponsesResultBody(string.Empty), image)) + "\n"
                + ResultLine("valid", ResponsesResultBody("Paris")));

        var results = await GetResultsAsync(handler);

        Assert.Equal(6, results.Count);

        Assert.Equal("function-call", results[0].Id);
        Assert.Equal("succeeded", results[0].Status);
        var call = Assert.Single(results[0].Text!.Content);
        Assert.Equal("tool-call", call.Type);
        Assert.Equal("call-123", call.ToolCallId);
        Assert.Equal("get_weather", call.ToolName);
        Assert.Equal("{\"city\":\"Paris\"}", call.Text);
        Assert.Equal("function-call", call.ProviderMetadata!.Value.GetProperty("openai").GetProperty("itemId").GetString());

        Assert.Equal("custom-tool-call", results[1].Id);
        Assert.Equal("succeeded", results[1].Status);
        call = Assert.Single(results[1].Text!.Content);
        Assert.Equal("tool-call", call.Type);
        Assert.Equal("call-123", call.ToolCallId);
        Assert.Equal("shell", call.ToolName);
        Assert.Equal("\"echo Paris\"", call.Text);
        Assert.Equal("custom-tool-call", call.ProviderMetadata!.Value.GetProperty("openai").GetProperty("itemId").GetString());

        Assert.Equal("web-search", results[2].Id);
        Assert.Equal("succeeded", results[2].Status);
        var parts = results[2].Text!.Content;
        Assert.Equal(2, parts.Count);
        Assert.Equal("tool-call", parts[0].Type);
        Assert.Equal("web-search", parts[0].ToolCallId);
        Assert.Equal("web_search", parts[0].ToolName);
        Assert.Equal("{}", parts[0].Text);
        Assert.True(parts[0].ProviderExecuted);
        Assert.True(parts[0].Dynamic);
        Assert.Equal("tool-result", parts[1].Type);
        Assert.Equal("web-search", parts[1].ToolCallId);
        Assert.Equal("web_search", parts[1].ToolName);
        Assert.Equal("{\"action\":{\"type\":\"search\",\"query\":\"weather in Paris\"}}", parts[1].Output!.Value.GetRawText());
        Assert.True(parts[1].Dynamic);

        Assert.Equal("file-search", results[3].Id);
        Assert.Equal("succeeded", results[3].Status);
        parts = results[3].Text!.Content;
        Assert.Equal(2, parts.Count);
        Assert.Equal("tool-call", parts[0].Type);
        Assert.Equal("file-search", parts[0].ToolCallId);
        Assert.Equal("file_search", parts[0].ToolName);
        Assert.Equal("{}", parts[0].Text);
        Assert.True(parts[0].ProviderExecuted);
        Assert.True(parts[0].Dynamic);
        Assert.Equal("tool-result", parts[1].Type);
        Assert.Equal("file-search", parts[1].ToolCallId);
        Assert.Equal("file_search", parts[1].ToolName);
        Assert.Equal("{\"queries\":[\"weather in Paris\"],\"results\":[{\"attributes\":{\"country\":\"France\"},\"fileId\":\"file_123\",\"filename\":\"weather.md\",\"score\":0.9,\"text\":\"Paris is sunny.\"}]}", parts[1].Output!.Value.GetRawText());
        Assert.True(parts[1].Dynamic);

        Assert.Equal("image", results[4].Id);
        Assert.Equal("failed", results[4].Status);
        Assert.Equal("OpenAI returned an unsupported \"image_generation_call\" output item in an AI SDK text batch.", results[4].ErrorMessage);
        Assert.Equal("unsupported_content", results[4].ErrorCode);

        Assert.Equal("valid", results[5].Id);
        Assert.Equal("succeeded", results[5].Status);
        Assert.Equal("Paris", Assert.Single(results[5].Text!.Content).Text);
    }
}
