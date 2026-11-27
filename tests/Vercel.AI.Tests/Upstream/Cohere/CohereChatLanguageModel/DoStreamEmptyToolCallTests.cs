// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doStream</c> &gt; <c>empty tool call</c>.</summary>
public sealed class DoStreamEmptyToolCallTests
{
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::";

    [Fact]
    [UpstreamTest(
        Chat + "doStream > empty tool call::should handle empty tool call arguments",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_empty_tool_call_arguments()
    {
        var options = TestPrompt();
        options.IncludeRawChunks = false;
        options.Tools = new[]
        {
            FunctionTool(
                "test-tool",
                "{\"type\":\"object\",\"properties\":{},\"required\":[],\"additionalProperties\":false,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}"
            ),
        };

        var parts = await Stream(Model(ChunksFixture("cohere-empty-tool-call")), options);

        Assert.Equal(6, parts.Count);
        Assert.IsType<StreamStartStreamPart>(parts[0]);
        Assert.Equal(
            "66dec7d7-45e6-427c-8fd9-7d6375d12046",
            Assert.IsType<ResponseMetadataStreamPart>(parts[1]).Id
        );
        var start = Assert.IsType<ToolInputStartStreamPart>(parts[2]);
        Assert.Equal("currentTime_y46ar19t5gvw", start.Id);
        Assert.Equal("currentTime", start.ToolName);
        Assert.Equal(
            "currentTime_y46ar19t5gvw",
            Assert.IsType<ToolInputEndStreamPart>(parts[3]).Id
        );
        var call = Assert.IsType<ToolCallStreamPart>(parts[4]);
        Assert.Equal("currentTime_y46ar19t5gvw", call.ToolCallId);
        Assert.Equal("currentTime", call.ToolName);
        Assert.Equal("{}", call.ArgumentsJson);
        var finish = Assert.IsType<FinishStreamPart>(parts[5]);
        Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
        Assert.Equal("TOOL_CALL", finish.RawFinishReason);
        Assert.Equal(1445, finish.Usage.InputTokens);
        Assert.Equal(741, finish.Usage.NoCacheInputTokens);
        Assert.Equal(704, finish.Usage.CacheReadTokens);
        Assert.Equal(43, finish.Usage.OutputTokens);
    }
}
