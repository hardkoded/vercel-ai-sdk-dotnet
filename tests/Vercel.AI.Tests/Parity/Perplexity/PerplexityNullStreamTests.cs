// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAICompatible;
using Vercel.AI.Perplexity;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class PerplexityNullStreamTests
{
    [Fact]
    public async Task Stream_finishes_when_agent_events_contain_null_fields()
    {
        var sse = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "perplexity-agent-null-events.sse"));
        var handler = new PerplexityScriptedHandler
        {
            ResponseBody = sse,
            MediaType = "text/event-stream",
        };
        var provider = PerplexityProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, handler);
        var parts = await Read(provider.LanguageModel("low").DoStreamAsync(Prompt(), CancellationToken.None));

        Assert.DoesNotContain(parts, part => part is ErrorStreamPart);
        Assert.Equal("Fetched content from 0 URLs", string.Concat(parts.OfType<ReasoningDeltaStreamPart>().Select(part => part.Delta)));
        Assert.DoesNotContain(parts.OfType<ReasoningDeltaStreamPart>(), part => part.Delta.Contains("null"));
        var text = Assert.Single(parts.OfType<TextDeltaStreamPart>());
        Assert.Equal("msg-1", text.Id);
        Assert.Equal("Hello", text.Delta);
        Assert.Equal("msg-1", Assert.Single(parts.OfType<TextStartStreamPart>()).Id);
        Assert.Equal("msg-1", Assert.Single(parts.OfType<TextEndStreamPart>()).Id);
        Assert.DoesNotContain(parts.OfType<TextStartStreamPart>(), part => part.Id.Contains("null"));
        Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Empty(parts.OfType<SourceStreamPart>());
    }

    private static LanguageModelCallOptions Prompt()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
        };
    }

    private static async Task<List<LanguageModelStreamPart>> Read(IAsyncEnumerable<LanguageModelStreamPart> stream)
    {
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in stream)
        {
            parts.Add(part);
        }

        return parts;
    }
}
