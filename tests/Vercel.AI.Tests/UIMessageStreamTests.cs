// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AspNetCore;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests;

public sealed class UIMessageStreamTests
{
    [Fact]
    public async Task Writes_text_tool_and_finish_chunks()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new TextDeltaStreamPart("text", "Hi"),
                new ToolCallStreamPart("call_1", "lookup", "{\"q\":1}"),
                new SourceStreamPart("src_1", "https://example.test/doc", "Doc"),
                new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(1, 1, 2), "tool_calls"),
            },
        };
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
        var stream = client.StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "hi",
            Tools = new[] { Tool.Function("lookup", "find", "{\"type\":\"object\"}", (_, _) => Task.FromResult("{\"n\":1}")) },
        });
        using var buffer = new MemoryStream();
        await UIMessageStreamResult.WriteAsync(stream, buffer);
        var text = System.Text.Encoding.UTF8.GetString(buffer.ToArray());
        Assert.Contains("\"type\":\"start\"", text);
        Assert.Contains("\"type\":\"text-start\"", text);
        Assert.Contains("\"type\":\"text-delta\"", text);
        Assert.Contains("\"delta\":\"Hi\"", text);
        Assert.Contains("\"type\":\"text-end\"", text);
        Assert.Contains("\"type\":\"tool-input-available\"", text);
        Assert.Contains("\"type\":\"tool-output-available\"", text);
        Assert.Contains("\"type\":\"source-url\"", text);
        Assert.Contains("\"type\":\"finish-step\"", text);
        Assert.Contains("\"type\":\"finish\"", text);
        Assert.Contains("data: [DONE]", text);
    }
}
