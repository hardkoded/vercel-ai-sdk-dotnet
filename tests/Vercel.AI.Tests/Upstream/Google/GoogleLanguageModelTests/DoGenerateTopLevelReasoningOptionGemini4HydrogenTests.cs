// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleLanguageModelTests;

/// <summary>Port of <c>google-language-model.test.ts</c> &gt; <c>doGenerate &gt; top-level reasoning option &gt; Gemini 4 Hydrogen</c>.</summary>
public sealed class DoGenerateTopLevelReasoningOptionGemini4HydrogenTests
{
    private const string Prefix = "packages/google/src/google-language-model.test.ts::doGenerate > top-level reasoning option > Gemini 4 Hydrogen::";

    [Theory]
    [InlineData("low")]
    [InlineData("medium")]
    [InlineData("high")]
    [InlineData("xhigh")]
    [UpstreamTest(Prefix + "passes reasoning \"%s\" without a compatibility warning", Coverage = UpstreamCoverage.Covered)]
    public void Passes_reasoning_without_a_compatibility_warning(string reasoning)
    {
        var options = Hello();
        options.Reasoning = reasoning;

        var prepared = GoogleRequest.Prepare("gemini-4-hydrogen", "google.generative-ai", options, false);

        Assert.Equal(reasoning, (string?)prepared.Body["generationConfig"]!["thinkingConfig"]!["thinkingLevel"]);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "passes an explicit xhigh thinking level instead of portable reasoning", Coverage = UpstreamCoverage.Covered)]
    public void Passes_an_explicit_xhigh_thinking_level_instead_of_portable_reasoning()
    {
        var options = Hello();
        options.Reasoning = "low";
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"thinkingConfig\":{\"thinkingLevel\":\"xhigh\"}}}");

        var prepared = GoogleRequest.Prepare("gemini-4-hydrogen", "google.generative-ai", options, false);

        Assert.Equal("xhigh", (string?)prepared.Body["generationConfig"]!["thinkingConfig"]!["thinkingLevel"]);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "leaves the thinking level unset for the API default", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_the_thinking_level_unset_for_the_API_default()
    {
        var prepared = GoogleRequest.Prepare("gemini-4-hydrogen", "google.generative-ai", Hello(), false);

        Assert.Null(prepared.Body["generationConfig"]?["thinkingConfig"]);
    }

    private static LanguageModelCallOptions Hello()
    {
        return GoogleUpstream.Prompt(new UserModelMessage("Hello"));
    }
}
