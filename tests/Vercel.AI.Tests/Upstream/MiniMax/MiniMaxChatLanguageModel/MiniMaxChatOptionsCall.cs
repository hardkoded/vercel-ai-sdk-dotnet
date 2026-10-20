// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.MiniMaxChatLanguageModel;

/// <summary>The <c>call</c> helper of <c>describe.each(['generate', 'stream'])('MiniMax chat options (%s)')</c>.</summary>
internal static class MiniMaxChatOptionsCall
{
    public static IEnumerable<object?[]> Modes(params object?[][] rows)
    {
        foreach (var mode in new[] { "generate", "stream" })
        {
            if (rows.Length == 0)
            {
                yield return new object?[] { mode };
            }

            foreach (var row in rows)
            {
                yield return new object?[] { mode }.Concat(row).ToArray();
            }
        }
    }

    public static async Task<(JsonNode Body, IReadOnlyList<CallWarning> Warnings, MiniMaxServer Server)> Call(
        string mode,
        LanguageModelCallOptions? options = null,
        string modelId = "minimax-m3.1-flash")
    {
        var server = new MiniMaxServer();
        var model = MiniMaxParity.Provider(server).Chat(modelId);
        options ??= new LanguageModelCallOptions();
        if (options.Prompt.Count == 0)
        {
            options.Prompt = new ModelMessage[] { new UserModelMessage("Hello") };
        }

        options.MaxOutputTokens ??= 1000;
        IReadOnlyList<CallWarning> warnings;
        if (mode == "generate")
        {
            server.SetJson("{\"type\":\"message\",\"content\":[{\"type\":\"text\",\"text\":\"Hello!\"}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":10,\"output_tokens\":3}}");
            warnings = (await model.DoGenerateAsync(options, CancellationToken.None)).Warnings;
        }
        else
        {
            server.SetStream(
                "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg_sampling\",\"model\":\"minimax-m3.1-flash\",\"usage\":{\"input_tokens\":10,\"output_tokens\":0}}}\n\n",
                "data: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":0}}\n\n",
                "data: {\"type\":\"message_stop\"}\n\n");
            var parts = await MiniMaxParity.Collect(await model.DoStreamAsync(options, CancellationToken.None));
            Assert.DoesNotContain(parts, part => part is ErrorStreamPart);
            warnings = Assert.Single(parts.OfType<StreamStartStreamPart>()).Warnings;
        }

        return (server.Calls[0].Body, warnings, server);
    }

    public static LanguageModelCallOptions WithText(string text, string? options = null)
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[] { Text(text, options) }),
            },
        };
    }

    public static TextContentPart Text(string text, string? providerOptions = null)
    {
        return new TextContentPart(text) { ProviderOptions = providerOptions == null ? null : MiniMaxParity.Options(providerOptions) };
    }
}
