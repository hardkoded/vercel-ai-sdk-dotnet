// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Perplexity;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.PerplexityLanguageModelTests;

/// <summary>Port of <c>perplexity-language-model.test.ts</c> &gt; <c>doStream</c>.</summary>
public sealed class DoStreamTests
{
    [Theory]
    [InlineData("response.completed")]
    [InlineData("response.incomplete")]
    [UpstreamTest("packages/perplexity/src/perplexity-language-model.test.ts::doStream::preserves an explicit %s event without response details", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_an_explicit_event_without_response_details(string type)
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = UpstreamChat.Sse("{\"type\":\"" + type + "\"}"),
            MediaType = "text/event-stream",
        };

        var parts = await Stream(capture);

        Assert.DoesNotContain(parts, part => part is ErrorStreamPart);
        var finish = Assert.IsType<FinishStreamPart>(Assert.Single(parts, part => part is FinishStreamPart));
        Assert.Equal(FinishReason.Other, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest("packages/perplexity/src/perplexity-language-model.test.ts::doStream::handles a truncated stream (with error: %s)", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_a_truncated_stream(bool withError)
    {
        var events = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", "perplexity-agent-web-search.sse"))
            .Where(line => line.StartsWith("data: ", StringComparison.Ordinal))
            .Select(line => line.Substring("data: ".Length))
            .ToList();
        var firstText = events.FindIndex(line => JsonNode.Parse(line)!["type"]!.GetValue<string>() == "response.output_text.delta");
        Assert.True(firstText >= 0);

        var body = string.Concat(events.Take(firstText + 1).Select(line => "data: " + line + "\n\n"))
            + (withError ? "data: {invalid-json}\n\n" : string.Empty);
        var capture = new UpstreamCapture { ResponseBody = body, MediaType = "text/event-stream" };

        var parts = await Stream(capture);

        Assert.Contains(parts, part => part is TextDeltaStreamPart);
        Assert.Equal(withError ? 1 : 0, parts.Count(part => part is ErrorStreamPart));
        var finish = Assert.IsType<FinishStreamPart>(Assert.Single(parts, part => part is FinishStreamPart));
        Assert.Equal(withError ? FinishReason.Error : FinishReason.Interrupted, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
    }

    private static async Task<List<LanguageModelStreamPart>> Stream(UpstreamCapture capture)
    {
        var model = PerplexityProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).LanguageModel("low");
        return await UpstreamChat.Read(await model.DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
    }
}
