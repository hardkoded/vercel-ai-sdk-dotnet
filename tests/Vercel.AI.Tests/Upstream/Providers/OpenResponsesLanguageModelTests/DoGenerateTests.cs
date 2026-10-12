// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenResponses;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate::";

    [Fact]
    [UpstreamTest(Prefix + "should send schema-less JSON as a JSON object format", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_schema_less_JSON_as_a_JSON_object_format()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.JsonResponse = true;

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("{\"format\":{\"type\":\"json_object\"}}", UpstreamChat.Body(capture)["text"]!.ToJsonString());
    }

    private static OpenResponsesLanguageModel Model(UpstreamCapture capture)
    {
        return (OpenResponsesLanguageModel)OpenResponsesProvider.Create(new OpenResponsesOptions { ApiKey = "test-key", BaseUrl = "https://example.test/v1" }, capture).LanguageModel("gemma-7b-it");
    }
}
