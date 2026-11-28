// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt; <c>basic text</c>.
/// </summary>
public sealed class DoGenerateBasicTextTests
{
    private const string Prefix = DoGenerateSupport.Id + "basic text::";

    private const string InteractionId =
        "v1_ChdTbXNIYXFyUEV0ZUttdGtQNXVqVHdRRRIXU21zSGFxclBFdGVLbXRrUDV1alR3UUU";

    private const string ExpectedBody =
        "{\"input\":[{\"content\":[{\"text\":\"Hello, how are you?\",\"type\":\"text\"}],\"type\":\"user_input\"}],\"model\":\"gemini-2.5-flash\"}";

    private static SequencedHandler Basic()
    {
        return new SequencedHandler().Json(GoogleInteractionsStreamSupport.JsonFixture("basic"));
    }

    private static LanguageModelCallOptions Options()
    {
        return GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "returns content, finishReason, usage, warnings, providerMetadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Returns_content_finishReason_usage_warnings_providerMetadata()
    {
        var result = await GoogleInteractionsStreamSupport
            .Model(Basic())
            .DoGenerateAsync(Options(), CancellationToken.None);

        GoogleUpstream.JsonEqual(
            DoGenerateSupport.Content(result.Content),
            "[{\"providerMetadata\":{\"google\":{\"interactionId\":\""
                + InteractionId
                + "\",\"signature\":\"CqwBAQw51sfgKVnBHSz5praTe+uG0Anr7XQqpCF63u254O4l2U4+GL3n7WRshuMFMfTty31n/76lM81JlqplsBd+YnEEPdyYqh4RpVrMnvUgDP7rkuWFPutrEgLUU/r3LuD3z1dc3qiMjtw3r3RkXtdHNF2Om28zmRoMT0/u8yNkPJA7S2IEfzasBN5yobkpwgbPAQ73PDJZy8n0qjNQmSG/OCMCUDBZMH3C9A1rEg==\"}},\"text\":\"\",\"type\":\"reasoning\"},"
                + "{\"providerMetadata\":{\"google\":{\"interactionId\":\""
                + InteractionId
                + "\"}},\"text\":\"Hello! I'm doing well, thank you for asking.\\n\\nHow are you today?\",\"type\":\"text\"}]"
        );
        GoogleUpstream.JsonEqual(
            DoGenerateSupport.FinishReason(result),
            "{\"raw\":\"completed\",\"unified\":\"stop\"}"
        );
        GoogleUpstream.JsonEqual(
            JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText()),
            "{\"google\":{\"interactionId\":\""
                + InteractionId
                + "\",\"serviceTier\":\"standard\"}}"
        );
        Assert.Equal(7, result.Usage.InputTokens);
        Assert.Equal(0, result.Usage.CacheReadTokens);
        Assert.Null(result.Usage.CacheWriteTokens);
        Assert.Equal(7, result.Usage.NoCacheInputTokens);
        Assert.Equal(51, result.Usage.OutputTokens);
        Assert.Equal(32, result.Usage.ReasoningTokens);
        Assert.Equal(19, result.Usage.TextTokens);
        GoogleUpstream.JsonEqual(
            result.Usage.Raw!.Value,
            "{\"input_tokens_by_modality\":[{\"modality\":\"text\",\"tokens\":7}],\"total_cached_tokens\":0,\"total_input_tokens\":7,\"total_output_tokens\":19,\"total_thought_tokens\":32,\"total_tokens\":58,\"total_tool_use_tokens\":0}"
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "sends the expected request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_expected_request_body()
    {
        var handler = Basic();

        await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(Options(), CancellationToken.None);

        GoogleUpstream.JsonEqual(DoGenerateSupport.Body(handler), ExpectedBody);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not send an Api-Revision header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Does_not_send_an_Api_Revision_header()
    {
        var handler = Basic();

        await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(Options(), CancellationToken.None);

        Assert.DoesNotContain("api-revision", handler.RequestHeaders[0].Keys);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "exposes the request body via result.request.body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Exposes_the_request_body_via_result_request_body()
    {
        var result = await GoogleInteractionsStreamSupport
            .Model(Basic())
            .DoGenerateAsync(Options(), CancellationToken.None);

        GoogleUpstream.JsonEqual(result.Request!.Body!.Value, ExpectedBody);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "exposes the raw response body via result.response.body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Exposes_the_raw_response_body_via_result_response_body()
    {
        var result = await GoogleInteractionsStreamSupport
            .Model(Basic())
            .DoGenerateAsync(Options(), CancellationToken.None);

        using var body = JsonDocument.Parse(result.RawResponse!);
        Assert.Equal(JsonValueKind.Object, body.RootElement.ValueKind);
        Assert.Matches("^v1_", body.RootElement.GetProperty("id").GetString());
    }
}
