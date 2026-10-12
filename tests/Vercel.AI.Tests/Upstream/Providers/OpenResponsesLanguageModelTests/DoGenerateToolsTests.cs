// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>
/// Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate &gt; tools</c>.
/// The call options have no provider-defined tools, so the cases about them are not ported (issue #365).
/// </summary>
public sealed class DoGenerateToolsTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate > tools::";

    [Fact]
    [UpstreamTest(Prefix + "should send correct request body with tools", Coverage = UpstreamCoverage.Partial, Note = "The call has the two function tools only. The two provider-defined tools cannot be passed (issue #365); upstream leaves them out of the body too.")]
    public async Task Should_send_correct_request_body_with_tools()
    {
        var capture = JsonFixture("lmstudio-basic.1");
        var options = UpstreamChat.Prompt();
        options.Tools = new[]
        {
            Tool("get_weather", "Get the current weather for a location", """{"type":"object","properties":{"location":{"type":"string","description":"The city and state"}},"required":["location"]}"""),
            Tool("search", "Search for information", """{"type":"object","properties":{"query":{"type":"string"}},"required":["query"]}""", strict: true),
        };

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(UpstreamChat.Body(capture), """
            {
              "input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user","type":"message"}],
              "model":"gemma-7b-it",
              "tools":[
                {"description":"Get the current weather for a location","name":"get_weather","parameters":{"properties":{"location":{"description":"The city and state","type":"string"}},"required":["location"],"type":"object"},"type":"function"},
                {"description":"Search for information","name":"search","parameters":{"properties":{"query":{"type":"string"}},"required":["query"],"type":"object"},"strict":true,"type":"function"}
              ]
            }
            """);
    }
}
