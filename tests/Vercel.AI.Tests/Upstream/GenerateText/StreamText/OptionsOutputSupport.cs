// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

internal static class OptionsOutputSupport
{
    internal static StreamTextResult Stream(
        IReadOnlyList<LanguageModelStreamPart> parts,
        OutputSpec? output = null
    )
    {
        return Stream(new TestLanguageModel { StreamParts = parts }, output);
    }

    internal static StreamTextResult Stream(
        ILanguageModel model,
        OutputSpec? output = null,
        Func<GenerateTextResult, CancellationToken, Task>? onEnd = null,
        Func<string>? generateCallId = null
    )
    {
        return new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).StreamTextAsync(
            new StreamTextOptions
            {
                Model = model,
                Prompt = "prompt",
                Output = output,
                OnEnd = onEnd,
                GenerateCallId = generateCallId,
                NowMs = generateCallId == null ? null : () => 0,
                StopWhen = StopWhen.IsStepCount(2),
            }
        );
    }

    internal static LanguageModelStreamPart[] Text(params string[] deltas)
    {
        var parts = new List<LanguageModelStreamPart> { new TextStartStreamPart("1") };
        parts.AddRange(deltas.Select(delta => new TextDeltaStreamPart("1", delta)));
        parts.Add(new TextEndStreamPart("1"));
        parts.Add(
            new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop")
        );
        return parts.ToArray();
    }

    internal static async Task<List<string>> Collect(StreamTextResult result)
    {
        var values = new List<string>();
        await foreach (var value in result.PartialOutputStream())
        {
            values.Add(value.GetRawText());
        }

        return values;
    }

    internal static async Task<List<string>> CollectElements(StreamTextResult result)
    {
        var values = new List<string>();
        await foreach (var value in result.ElementStream())
        {
            values.Add(value.GetRawText());
        }

        return values;
    }

    internal static LanguageModelStreamPart[] TextWithoutEnd(params string[] deltas)
    {
        var parts = new List<LanguageModelStreamPart> { new TextStartStreamPart("1") };
        parts.AddRange(deltas.Select(delta => new TextDeltaStreamPart("1", delta)));
        parts.Add(
            new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop")
        );
        return parts.ToArray();
    }

    internal const string ContentSchema =
        "{\"type\":\"object\",\"properties\":{\"content\":{\"type\":\"string\"}},\"required\":[\"content\"],\"additionalProperties\":false}";

    internal const string ValueSchema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}";
}
