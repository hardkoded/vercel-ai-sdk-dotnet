// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>
/// Shared helpers of the tool loop agent tests that run a tool call. Maps to the sandbox, <c>mockSandboxSessionFileStubs</c>, and <c>testTool</c> setup
/// of <c>tool-loop-agent.test.ts</c>.
/// </summary>
internal static class ToolLoopAgentToolCalls
{
    public const string TestToolSchema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}";

    private static readonly LanguageModelUsage s_usage = new(3, 10, 13);

    public static LanguageModelGenerateResult GenerateToolCall(
        string input = "{ \"value\": \"test\" }"
    ) =>
        new(
            new GeneratedContent[] { new GeneratedToolCall("call-1", "testTool", input) },
            FinishReason.ToolCalls,
            s_usage,
            null
        );

    public static LanguageModelGenerateResult GenerateDone() =>
        SteeringTestSupport.TextResponse("done");

    public static IReadOnlyList<LanguageModelStreamPart> StreamToolCall(
        string input = "{ \"value\": \"test\" }"
    ) =>
        new LanguageModelStreamPart[]
        {
            new StreamStartStreamPart(Array.Empty<CallWarning>()),
            new ToolCallStreamPart("call-1", "testTool", input),
            new FinishStreamPart(FinishReason.ToolCalls, s_usage, null),
        };

    public static IReadOnlyList<LanguageModelStreamPart> StreamDone() =>
        SteeringTestSupport.TextStream("done");

    public static SteeringTestModel GenerateModel(
        out Func<int> callCount,
        string input = "{ \"value\": \"test\" }"
    )
    {
        var calls = 0;
        callCount = () => calls;
        return SteeringTestModel.Generate(
            _ =>
            {
                calls++;
                return GenerateToolCall(input);
            },
            _ =>
            {
                calls++;
                return GenerateDone();
            }
        );
    }

    public static SteeringTestModel StreamModel(
        out Func<int> callCount,
        string input = "{ \"value\": \"test\" }"
    )
    {
        var calls = 0;
        callCount = () => calls;
        return SteeringTestModel.Stream(
            () =>
            {
                calls++;
                return StreamToolCall(input);
            },
            () =>
            {
                calls++;
                return StreamDone();
            }
        );
    }

    public static Tool TestTool(Func<string, ToolExecutionOptions, string> execute)
    {
        var tool = Tool.Function("testTool", null, TestToolSchema, null);
        tool.ExecuteWithOptions = (input, options) =>
            Task.FromResult(
                JsonSerializer.Serialize(execute(input.GetProperty("value").GetString()!, options))
            );
        return tool;
    }

    /// <summary>
    /// Maps to the <c>toMatchInlineSnapshot</c> of the tool execution event tests: the call id, the messages of the step, the tool call
    /// and the tool context. The tool call is <c>{ "value": value }</c>.
    /// </summary>
    public static void AssertToolExecutionEvent(
        ToolExecutionStartEvent e,
        string callId,
        string value
    )
    {
        Assert.Equal(callId, e.CallId);
        var message = Assert.IsType<UserModelMessage>(Assert.Single(e.Messages));
        Assert.Equal("test", Assert.IsType<TextContentPart>(Assert.Single(message.Content)).Text);
        Assert.Equal("call-1", e.ToolCall.ToolCallId);
        Assert.Equal("testTool", e.ToolCall.ToolName);
        Assert.Equal(
            JsonSerializer.Serialize(new { value }),
            JsonSerializer.Serialize(
                JsonSerializer.Deserialize<JsonElement>(e.ToolCall.ArgumentsJson)
            )
        );
        Assert.False(e.ToolCall.ProviderExecuted);
        Assert.Null(e.ToolCall.ProviderMetadata);
        Assert.Null(e.ToolContext);
    }

    /// <summary>Maps to the <c>toolOutput</c> of the <c>onToolExecutionEnd</c> snapshots, with the output <c>"{value}-result"</c>.</summary>
    public static void AssertToolOutput(ExecutedTool output, string value)
    {
        Assert.Equal("call-1", output.ToolCallId);
        Assert.Equal("testTool", output.ToolName);
        Assert.False(output.Dynamic);
        Assert.False(output.IsError);
        Assert.Equal(
            JsonSerializer.Serialize(new { value }),
            JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(output.InputJson!))
        );
        Assert.Equal(JsonSerializer.Serialize(value + "-result"), output.OutputJson);
    }

    public static void AssertApprovalRequestOnly(IReadOnlyList<ModelMessage> messages)
    {
        var assistant = Assert.IsType<AssistantModelMessage>(Assert.Single(messages));
        Assert.Collection(
            assistant.Parts,
            part =>
            {
                var call = Assert.IsType<AssistantToolCallPart>(part);
                Assert.Equal("call-1", call.ToolCall.ToolCallId);
                Assert.Equal("testTool", call.ToolCall.ToolName);
            },
            part =>
                Assert.Equal(
                    "call-1",
                    Assert.IsType<AssistantToolApprovalRequestPart>(part).ToolCallId
                )
        );
    }
}

/// <summary>Maps to <c>mockSandboxSessionFileStubs</c> and the sandbox object of <c>tool-loop-agent.test.ts</c>.</summary>
internal sealed class TestSandboxSession : ISandboxSession
{
    public string Description => "test sandbox";

    public Task<Stream?> ReadFileAsync(SandboxReadFileOptions options) =>
        Task.FromResult<Stream?>(null);

    public Task<byte[]?> ReadBinaryFileAsync(SandboxReadBinaryFileOptions options) =>
        Task.FromResult<byte[]?>(null);

    public Task<string?> ReadTextFileAsync(SandboxReadTextFileOptions options) =>
        Task.FromResult<string?>(null);

    public Task WriteFileAsync(SandboxWriteFileOptions options) => Task.CompletedTask;

    public Task WriteBinaryFileAsync(SandboxWriteBinaryFileOptions options) => Task.CompletedTask;

    public Task WriteTextFileAsync(SandboxWriteTextFileOptions options) => Task.CompletedTask;

    public Task<SandboxFileStat?> StatAsync(SandboxStatOptions options) =>
        Task.FromResult<SandboxFileStat?>(null);

    public Task<IReadOnlyList<SandboxDirectoryEntry>?> ListDirectoryAsync(
        SandboxListDirectoryOptions options
    ) => Task.FromResult<IReadOnlyList<SandboxDirectoryEntry>?>(null);

    public Task<bool> DeleteFileAsync(SandboxDeleteFileOptions options) => Task.FromResult(false);

    public Task<ISandboxProcess> SpawnAsync(SandboxSpawnOptions options) =>
        throw new NotSupportedException();

    public Task<SandboxRunResult> RunAsync(SandboxRunOptions options) =>
        Task.FromResult(new SandboxRunResult { ExitCode = 0, Stdout = "ok" });
}
