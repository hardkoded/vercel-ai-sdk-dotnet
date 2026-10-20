// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.ResultToolCallsTests;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; result.toolResults</c>.</summary>
public sealed class ResultToolResultsTests
{
    private const string Prefix = "packages/ai/src/generate-text/generate-text.test.ts::generateText > result.toolResults::";

    private const string ValueSchema = "{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"additionalProperties\":false,\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"type\":\"object\"}";

    [Fact]
    [UpstreamTest(Prefix + "should contain tool results", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_contain_tool_results()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => ToolCallResponse("{ \"value\": \"value\" }"),
        };

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Tools = new[]
            {
                Tool.Function("tool1", null, ValueSchema, (input, _) =>
                {
                    Assert.Equal("{\"value\":\"value\"}", Compact(input));
                    return Task.FromResult("\"result1\"");
                }),
            },
            Prompt = "test-input",
        });

        var call = model.Calls[0];
        var tool = Assert.Single(call.Tools!);
        Assert.Equal("tool1", tool.Name);
        Assert.Equal(ValueSchema, tool.InputSchema.GetRawText());
        Assert.Equal("auto", call.ToolChoice!.Type);
        var user = Assert.IsType<UserModelMessage>(Assert.Single(call.Prompt));
        Assert.Equal("test-input", Assert.IsType<TextContentPart>(Assert.Single(user.Content)).Text);

        var toolResult = Assert.Single(result.ToolResults);
        Assert.False(toolResult.Dynamic);
        Assert.Equal("{\"value\":\"value\"}", Compact(toolResult.InputJson!));
        Assert.Equal("\"result1\"", toolResult.OutputJson);
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("tool1", toolResult.ToolName);
        Assert.False(toolResult.IsError);
    }
}
