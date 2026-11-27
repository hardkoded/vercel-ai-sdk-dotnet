// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>null tool call arguments</c>.</summary>
public sealed class DoGenerateNullToolCallArgumentsTests
{
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::";

    [Fact]
    [UpstreamTest(
        Chat
            + "doGenerate > null tool call arguments::should handle string \"null\" tool call arguments",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_string_null_tool_call_arguments()
    {
        var options = UserPrompt("What is the current time?");
        options.Tools = new[]
        {
            FunctionTool(
                "currentTime",
                "{\"type\":\"object\",\"properties\":{},\"required\":[],\"additionalProperties\":false,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}"
            ),
        };

        var result = await Model(JsonFixture("cohere-null-args"))
            .DoGenerateAsync(options, CancellationToken.None);

        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("currentTime_tf4dywn8wgnk", call.ToolCallId);
        Assert.Equal("currentTime", call.ToolName);
        Assert.Equal("{}", call.ArgumentsJson);
    }
}
