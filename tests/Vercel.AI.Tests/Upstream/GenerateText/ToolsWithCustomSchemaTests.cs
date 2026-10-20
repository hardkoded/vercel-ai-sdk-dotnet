// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.ResultToolCallsTests;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; tools with custom schema</c>.</summary>
public sealed class ToolsWithCustomSchemaTests
{
    private const string Prefix = "packages/ai/src/generate-text/generate-text.test.ts::generateText > tools with custom schema::";

    private const string ValueSchema = "{\"additionalProperties\":false,\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"type\":\"object\"}";
    private const string SomethingElseSchema = "{\"additionalProperties\":false,\"properties\":{\"somethingElse\":{\"type\":\"string\"}},\"required\":[\"somethingElse\"],\"type\":\"object\"}";

    [Fact]
    [UpstreamTest(Prefix + "should contain tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_contain_tool_calls()
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
                Tool.Function("tool1", null, ValueSchema, null),

                // 2nd tool to show typing:
                Tool.Function("tool2", null, SomethingElseSchema, null),
            },
            ToolChoice = ToolChoice.Required,
            Prompt = "test-input",
        });

        var call = Assert.Single(model.Calls);
        Assert.Equal(new[] { "tool1", "tool2" }, call.Tools!.Select(tool => tool.Name));
        Assert.Equal(ValueSchema, call.Tools![0].InputSchema.GetRawText());
        Assert.Equal(SomethingElseSchema, call.Tools![1].InputSchema.GetRawText());
        Assert.Equal("required", call.ToolChoice!.Type);
        var user = Assert.IsType<UserModelMessage>(Assert.Single(call.Prompt));
        Assert.Equal("test-input", Assert.IsType<TextContentPart>(Assert.Single(user.Content)).Text);

        var toolCall = Assert.Single(result.ToolCalls);
        Assert.False(toolCall.Dynamic);
        Assert.Equal("{ \"value\": \"value\" }", toolCall.ArgumentsJson);
        Assert.False(toolCall.ProviderExecuted);
        Assert.Null(toolCall.ProviderMetadata);
        Assert.Null(toolCall.Title);
        Assert.Equal("call-1", toolCall.ToolCallId);
        Assert.Equal("tool1", toolCall.ToolName);
    }
}
