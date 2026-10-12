// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate &gt; request parameters</c>.</summary>
public sealed class DoGenerateRequestParametersTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate > request parameters::";

    [Fact]
    [UpstreamTest(Prefix + "should send correct request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_request_body()
    {
        var capture = JsonFixture("lmstudio-basic.1");
        var options = UpstreamChat.Prompt();
        options.MaxOutputTokens = 100;
        options.Temperature = 0.5;
        options.TopP = 0.9;
        options.PresencePenalty = 0.1;
        options.FrequencyPenalty = 0.2;
        options.JsonSchema = UpstreamChat.Json("""{"type":"object","properties":{"status":{"type":"string"}},"required":["status"]}""");
        options.JsonSchemaName = "response";
        options.JsonSchemaDescription = "Example response schema";

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(UpstreamChat.Body(capture), """
            {
              "frequency_penalty":0.2,
              "input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user","type":"message"}],
              "max_output_tokens":100,
              "model":"gemma-7b-it",
              "presence_penalty":0.1,
              "temperature":0.5,
              "text":{"format":{"description":"Example response schema","name":"response","schema":{"properties":{"status":{"type":"string"}},"required":["status"],"type":"object"},"strict":true,"type":"json_schema"}},
              "top_p":0.9
            }
            """);
    }
}
