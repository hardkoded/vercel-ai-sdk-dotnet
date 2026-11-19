// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; mixed citation types &gt; compaction</c>.</summary>
public sealed class MixedCitationTypesCompactionTests
{
    private const string Prefix =
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > mixed citation types > compaction::";

    private const string ContextManagementOptions =
        """{"store":false,"contextManagement":[{"type":"compaction","compactThreshold":50000}]}""";

    [Fact]
    [UpstreamTest(
        Prefix + "should append an explicit compaction trigger to streaming input",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_append_an_explicit_compaction_trigger_to_streaming_input()
    {
        var options = OpenAIUpstream.Hello();
        options.ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(
            """{"compactionTrigger":true}"""
        );

        var (_, capture) = await Run(options);

        var body = JsonNode.Parse(capture.Body)!;
        Assert.True(body["stream"]!.GetValue<bool>());
        var expected = JsonNode.Parse(
            """[{"role":"user","content":[{"type":"input_text","text":"Hello"}]},{"type":"compaction_trigger"}]"""
        );
        Assert.True(JsonNode.DeepEquals(expected, body["input"]), body.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should stream compaction output item from real fixture",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_compaction_output_item_from_real_fixture()
    {
        var (parts, _) = await Run(WithContextManagement());

        OpenAIResponsesSupport.AssertStreamSnapshot(parts, "openai-compaction.1");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send context_management in streaming request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_context_management_in_streaming_request_body()
    {
        var (_, capture) = await Run(WithContextManagement());

        var body = JsonNode.Parse(capture.Body)!;
        Assert.Equal("gpt-5.2", body["model"]!.GetValue<string>());
        Assert.False(body["store"]!.GetValue<bool>());
        Assert.True(body["stream"]!.GetValue<bool>());
        var expected = JsonNode.Parse(
            """[{"type":"compaction","compact_threshold":50000}]"""
        );
        Assert.True(
            JsonNode.DeepEquals(expected, body["context_management"]),
            body.ToJsonString()
        );
    }

    private static LanguageModelCallOptions WithContextManagement()
    {
        var options = OpenAIUpstream.Hello();
        options.ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(ContextManagementOptions);
        return options;
    }

    private static async Task<(
        List<LanguageModelStreamPart> Parts,
        OpenAICapture Capture
    )> Run(LanguageModelCallOptions options)
    {
        var capture = new OpenAICapture
        {
            ServerSentEvents = string.Concat(
                SseStreamHandler.FixtureChunks("openai-compaction.1", done: false)
            ),
        };
        var result = await OpenAIUpstream
            .Provider(capture)
            .ResponsesModel("gpt-5.2")
            .DoStreamAsync(options, CancellationToken.None);
        return (await OpenAIUpstream.Read(result.Stream), capture);
    }
}
