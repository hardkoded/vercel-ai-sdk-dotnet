// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIBatch.OpenAIBatchTestSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIBatch;

public sealed class OpenAIBatchWebSearchStatusWithActionTests
{
    [Theory]
    [InlineData(true, "failed")]
    [InlineData(true, "incomplete")]
    [InlineData(false, "failed")]
    [InlineData(false, "incomplete")]
    [UpstreamTest("packages/openai/src/openai-batch.test.ts::OpenAI batch service > batch result lifecycle > web search status with action: %s::preserves %s web search tool errors in successful batch responses", Coverage = UpstreamCoverage.Covered)]
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
}
