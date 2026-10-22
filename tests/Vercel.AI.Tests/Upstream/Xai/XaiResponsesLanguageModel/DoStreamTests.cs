// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doStream</c>.</summary>
public sealed class DoStreamTests
{
    private const string Prefix = TestFile + " > doStream::";

    [Theory]
    [InlineData("missing")]
    [InlineData("invalid-json")]
    [InlineData("provider-error")]
    [InlineData("empty-status")]
    [UpstreamTest(Prefix + "handles a stream ending with %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_a_stream_ending_with(string ending)
    {
        var withError = ending is "invalid-json" or "provider-error";
        var events = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", "xai-text-streaming.1.chunks.txt"))
            .Where(line => line.Trim().Length > 0)
            .ToList();
        var firstText = events.FindIndex(line => JsonNode.Parse(line)!["type"]!.GetValue<string>() == "response.output_text.delta");
        Assert.True(firstText >= 0);
        var chunks = events.Take(firstText + 1).ToList();
        if (ending == "invalid-json")
        {
            chunks.Add("{invalid-json}");
        }

        if (ending == "provider-error")
        {
            chunks.Add("""{"type":"error","message":"Stream failed","code":"server_error"}""");
        }

        if (ending == "empty-status")
        {
            var completed = JsonNode.Parse(events.First(line => JsonNode.Parse(line)!["type"]!.GetValue<string>() == "response.completed"))!.AsObject();
            completed["response"]!["status"] = "";
            chunks.Add(completed.ToJsonString());
        }

        var parts = await Collect(Model(StreamResponse(chunks.ToArray())), HelloPrompt());

        Assert.Contains(parts, part => part is TextDeltaStreamPart);
        Assert.Equal(withError ? 1 : 0, parts.OfType<ErrorStreamPart>().Count());
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(withError ? FinishReason.Error : ending == "empty-status" ? FinishReason.Other : FinishReason.Interrupted, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn about unsupported sampling settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_warn_about_unsupported_sampling_settings()
    {
        var capture = ChunksFixture("xai-text-streaming.1.chunks.txt");
        var options = HelloPrompt();
        options.TopK = 10;
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.5;

        var parts = await Collect(Model(capture), options);

        var start = First<StreamStartStreamPart>(parts);
        Assert.Equal(new[] { "frequencyPenalty", "presencePenalty" }, start.Warnings.Select(warning => warning.Feature));
        Assert.All(start.Warnings, warning => Assert.Equal("unsupported", warning.Type));
    }
}
