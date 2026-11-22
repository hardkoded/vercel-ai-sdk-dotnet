// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.StreamText;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

internal static class TelemetryStreamSupport
{
    /// <summary>Maps to <c>createTestModel()</c> of <c>stream-text.test.ts</c>: it streams <c>Hello, world!</c>.</summary>
    internal static ILanguageModel CreateTestModel() =>
        new StreamTextAbortSupport.ScriptedStreamModel(HelloWorldStream);

    private static async IAsyncEnumerable<LanguageModelStreamPart> HelloWorldStream()
    {
        yield return new StreamStartStreamPart(null);
        yield return new TextStartStreamPart("1");
        yield return new TextDeltaStreamPart("1", "Hello, world!");
        yield return new TextEndStreamPart("1");
        yield return new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13));
        await Task.CompletedTask;
    }
}
