// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

internal static class ToolExecutionCallbackSupport
{
    internal const string ValueSchema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    internal static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }

    internal static Tool Tool1(Func<JsonElement, CancellationToken, Task<string>>? execute = null, bool withExecute = true)
    {
        return Tool.Function(
            "tool1",
            null,
            ValueSchema,
            withExecute ? execute ?? ((input, _) => Task.FromResult(input.GetProperty("value").GetString() + "-result")) : null);
    }

    // A model that makes the given tool calls on every call, with a tool-calls finish reason.
    internal static TestLanguageModel GenerateModel(params (string Id, string Input)[] calls)
    {
        return new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                calls.Select(call => (GeneratedContent)new GeneratedToolCall(call.Id, "tool1", call.Input)).ToArray(),
                FinishReason.ToolCalls,
                new LanguageModelUsage(3, 10, 13)),
        };
    }

    // A model that makes the tool calls on its first call and answers with text on every later one.
    internal static TestLanguageModel TwoStepGenerateModel()
    {
        var calls = 0;
        return new TestLanguageModel
        {
            OnGenerate = _ => ++calls == 1
                ? new LanguageModelGenerateResult(
                    new GeneratedContent[] { new GeneratedToolCall("call-1", "tool1", "{ \"value\": \"a\" }") },
                    FinishReason.ToolCalls,
                    new LanguageModelUsage(3, 10, 13))
                : new LanguageModelGenerateResult(
                    new GeneratedContent[] { new GeneratedText("done") },
                    FinishReason.Stop,
                    new LanguageModelUsage(3, 10, 13)),
        };
    }

    internal static async IAsyncEnumerable<LanguageModelStreamPart> ToolCallStream(params (string Id, string Input)[] calls)
    {
        yield return new StreamStartStreamPart(null);
        foreach (var call in calls)
        {
            yield return new ToolCallStreamPart(call.Id, "tool1", call.Input);
        }

        yield return new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13));
        await Task.CompletedTask;
    }

    internal static async IAsyncEnumerable<LanguageModelStreamPart> TextStream()
    {
        yield return new StreamStartStreamPart(null);
        yield return new TextStartStreamPart("1");
        yield return new TextDeltaStreamPart("1", "done");
        yield return new TextEndStreamPart("1");
        yield return new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13));
        await Task.CompletedTask;
    }

    internal static ILanguageModel StreamModel(params (string Id, string Input)[] calls)
    {
        return new Vercel.AI.Tests.StreamText.StreamTextAbortSupport.ScriptedStreamModel(() => ToolCallStream(calls));
    }

    internal static ILanguageModel TwoStepStreamModel()
    {
        var steps = 0;
        return new Vercel.AI.Tests.StreamText.StreamTextAbortSupport.ScriptedStreamModel(
            () => ++steps == 1 ? ToolCallStream(("call-1", "{ \"value\": \"a\" }")) : TextStream());
    }

    internal static async Task ConsumeAsync(StreamTextResult result)
    {
        await foreach (var _ in result.Stream())
        {
        }
    }
}
