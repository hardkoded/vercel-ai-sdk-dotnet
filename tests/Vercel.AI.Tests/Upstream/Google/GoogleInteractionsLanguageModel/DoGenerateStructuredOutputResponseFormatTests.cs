// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt; <c>structured output (responseFormat)</c>.
/// </summary>
public sealed class DoGenerateStructuredOutputResponseFormatTests
{
    private const string Prefix =
        "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doGenerate > structured output (responseFormat)::";

    private const string Schema =
        "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\",\"description\":\"Full name of the person.\"},\"age\":{\"type\":\"number\",\"description\":\"Age of the person in years.\"}},\"required\":[\"name\",\"age\"],\"additionalProperties\":false,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}";

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
        return new SequencedHandler().Json(
            GoogleInteractionsStreamSupport.JsonFixture("structured-output")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "emits response_format on the request body when responseFormat is json with a schema",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_response_format_on_the_request_body_when_responseFormat_is_json_with_a_schema()
    {
        var handler = Handler();

        await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(Options(true, true), CancellationToken.None);

        GoogleUpstream.JsonEqual(
            JsonNode.Parse(handler.Bodies[0]),
            "{\"input\":[{\"content\":[{\"text\":\"Hello, how are you?\",\"type\":\"text\"}],\"type\":\"user_input\"}],"
                + "\"model\":\"gemini-2.5-flash\","
                + "\"response_format\":[{\"mime_type\":\"application/json\",\"schema\":"
                + Schema
                + ",\"type\":\"text\"}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "emits a single text response_format entry when responseFormat is json without a schema",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_a_single_text_response_format_entry_when_responseFormat_is_json_without_a_schema()
    {
        var handler = Handler();

        await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(Options(true, false), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        // mime_type lives inside the polymorphic response_format entry; no top-level response_mime_type field on the wire.
        Assert.False(body.ContainsKey("response_mime_type"));
        GoogleUpstream.JsonEqual(
            body["response_format"],
            "[{\"type\":\"text\",\"mime_type\":\"application/json\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "omits response_format when responseFormat is text",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Omits_response_format_when_responseFormat_is_text()
    {
        var handler = Handler();

        await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(Options(false, false), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.False(body.ContainsKey("response_mime_type"));
        Assert.False(body.ContainsKey("response_format"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "serializes video response format controls and returns inline video data",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Serializes_video_response_format_controls_and_returns_inline_video_data()
    {
        var handler = new SequencedHandler().Json(
            "{\"id\":\"v1_video\",\"status\":\"completed\",\"model\":\"gemini-omni-flash-preview\",\"steps\":[{\"type\":\"model_output\",\"content\":[{\"type\":\"video\",\"mime_type\":\"video/mp4\",\"data\":\"AAAAIGZ0eXBpc29t\"}]}]}"
        );
        var options = Options(false, false);
        options.ProviderOptions = GoogleUpstream.ProviderOptions(
            "{\"google\":{\"responseModalities\":[\"video\"],\"responseFormat\":[{\"type\":\"video\",\"aspectRatio\":\"16:9\",\"resolution\":\"360p\",\"duration\":\"4s\",\"delivery\":\"uri\",\"gcsUri\":\"gs://video-output/clip.mp4\"}]}}"
        );

        var result = await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(options, CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        GoogleUpstream.JsonEqual(body["response_modalities"], "[\"video\"]");
        GoogleUpstream.JsonEqual(
            body["response_format"],
            "[{\"type\":\"video\",\"aspect_ratio\":\"16:9\",\"resolution\":\"360p\",\"duration\":\"4s\",\"delivery\":\"uri\",\"gcs_uri\":\"gs://video-output/clip.mp4\"}]"
        );
        Assert.Contains(result.Content.OfType<GeneratedFile>(), file =>
            file.MediaType == "video/mp4"
            && System.Convert.ToBase64String(file.Data) == "AAAAIGZ0eXBpc29t"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "returns the JSON-shaped text content from the parsed response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Returns_the_JSON_shaped_text_content_from_the_parsed_response()
    {
        var result = await GoogleInteractionsStreamSupport
            .Model(Handler())
            .DoGenerateAsync(Options(true, true), CancellationToken.None);

        GoogleUpstream.JsonEqual(
            DoGenerateSupport.Content(result.Content),
            "[{\"providerMetadata\":{\"google\":{\"interactionId\":\"v1_ChdUV3NIYW9LTk9OYXlxdHNQb2RpcC1RRRIXVFdzSGFvS05PTmF5cXRzUG9kaXAtUUU\",\"signature\":\"CqACAQw51sc7wSan2PLLtHG+z0j4AgWR8MPPe76QUDZpwhGeQ1AGXBWGqSm7nhjwbkyXhJ7JlfZ/R3iLZAmqxGH9mLrdkzQooTu5YKptE0+D1jb+PLv7PM/pkCeI8E2IUDtamWiQP2/eG3Rgouxd7+kNxcaERwGTLFsGMdZttew0aP8+xd0hKPAZVkkqKtjVVNYQaSVsnXpTxZrE+spikYx1T/lB9tl6tYkJrB/SmkGO/tsTqDVsaejBXheSM+pu/Nt7s9gAhiNxJnoTvFENezmmiGwiho1lkkPrK8u5uODO5MldRXefJnv8mkZAw/3l6Cxd63hKPGP5za6sjFr/0Scsv0LcrOU65e+YEqZOkp7OXcCoqTCZZITx89JjkOA/pN1T\"}},\"text\":\"\",\"type\":\"reasoning\"},{\"providerMetadata\":{\"google\":{\"interactionId\":\"v1_ChdUV3NIYW9LTk9OYXlxdHNQb2RpcC1RRRIXVFdzSGFvS05PTmF5cXRzUG9kaXAtUUU\"}},\"text\":\"{\\\"name\\\":\\\"John Doe\\\",\\\"age\\\":30}\",\"type\":\"text\"}]"
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "warns and drops responseFormat when an agent is set",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Warns_and_drops_responseFormat_when_an_agent_is_set()
    {
        var handler = Handler();
        var model = GoogleProvider
            .Create(
                new GoogleOptions { ApiKey = "test-api-key", GenerateId = () => "test-id" },
                handler
            )
            .Interactions(new GoogleInteractionsAgent("deep-research-pro-preview-12-2025"));

        await model.DoGenerateAsync(Options(true, true), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.False(body.ContainsKey("response_mime_type"));
        Assert.False(body.ContainsKey("response_format"));
    }
}
