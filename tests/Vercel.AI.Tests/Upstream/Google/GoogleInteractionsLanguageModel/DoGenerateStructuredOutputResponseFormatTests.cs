// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt; <c>structured output (responseFormat)</c>.
/// Not ported: <c>serializes video response format controls and returns inline video data</c> (the port does not read
/// <c>providerOptions.google.responseFormat</c> or parse video outputs) and <c>returns the JSON-shaped text content from the parsed response</c>
/// (the port does not return thought steps as reasoning or stamp the interaction id on content parts).
/// </summary>
public sealed class DoGenerateStructuredOutputResponseFormatTests
{
    private const string Prefix = "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doGenerate > structured output (responseFormat)::";

    private const string Schema = "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\",\"description\":\"Full name of the person.\"},\"age\":{\"type\":\"number\",\"description\":\"Age of the person in years.\"}},\"required\":[\"name\",\"age\"],\"additionalProperties\":false,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}";

    private static LanguageModelCallOptions Options(bool json, bool schema)
    {
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        options.JsonResponse = json;
        if (schema)
        {
            options.JsonSchema = GoogleUpstream.Element(Schema);
        }

        return options;
    }

    private static SequencedHandler Handler()
    {
        return new SequencedHandler().Json(GoogleInteractionsStreamSupport.JsonFixture("structured-output"));
    }

    [Fact]
    [UpstreamTest(Prefix + "emits response_format on the request body when responseFormat is json with a schema", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_response_format_on_the_request_body_when_responseFormat_is_json_with_a_schema()
    {
        var handler = Handler();

        await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options(true, true), CancellationToken.None);

        GoogleUpstream.JsonEqual(
            JsonNode.Parse(handler.Bodies[0]),
            "{\"input\":[{\"content\":[{\"text\":\"Hello, how are you?\",\"type\":\"text\"}],\"type\":\"user_input\"}],"
            + "\"model\":\"gemini-2.5-flash\","
            + "\"response_format\":[{\"mime_type\":\"application/json\",\"schema\":" + Schema + ",\"type\":\"text\"}]}");
    }

    [Fact]
    [UpstreamTest(Prefix + "emits a single text response_format entry when responseFormat is json without a schema", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_a_single_text_response_format_entry_when_responseFormat_is_json_without_a_schema()
    {
        var handler = Handler();

        await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options(true, false), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        // mime_type lives inside the polymorphic response_format entry; no top-level response_mime_type field on the wire.
        Assert.False(body.ContainsKey("response_mime_type"));
        GoogleUpstream.JsonEqual(body["response_format"], "[{\"type\":\"text\",\"mime_type\":\"application/json\"}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "omits response_format when responseFormat is text", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_response_format_when_responseFormat_is_text()
    {
        var handler = Handler();

        await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options(false, false), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.False(body.ContainsKey("response_mime_type"));
        Assert.False(body.ContainsKey("response_format"));
    }

    [Fact]
    [UpstreamTest(Prefix + "warns and drops responseFormat when an agent is set", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_and_drops_responseFormat_when_an_agent_is_set()
    {
        var handler = Handler();
        var model = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key", GenerateId = () => "test-id" }, handler)
            .Interactions(new GoogleInteractionsAgent("deep-research-pro-preview-12-2025"));

        await model.DoGenerateAsync(Options(true, true), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.False(body.ContainsKey("response_mime_type"));
        Assert.False(body.ContainsKey("response_format"));
    }
}
