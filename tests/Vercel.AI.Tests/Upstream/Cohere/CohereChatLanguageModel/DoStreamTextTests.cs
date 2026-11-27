// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doStream</c> &gt; <c>text</c>.</summary>
public sealed class DoStreamTextTests
{
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::";
    private const string Suite = Chat + "doStream > text::";

    private static LanguageModelCallOptions Options(bool raw) =>
        new() { Prompt = TestPrompt().Prompt, IncludeRawChunks = raw };

    private static async Task<FinishStreamPart> Finish(string usage)
    {
        var parts = await Stream(
            Model(
                ChunkLines(
                    "{\"type\":\"message-end\",\"delta\":{\"finish_reason\":\"COMPLETE\",\"usage\":"
                        + usage
                        + "}}"
                )
            ),
            Options(false)
        );
        return Assert.IsType<FinishStreamPart>(parts[^1]);
    }

    [Fact]
    [UpstreamTest(Suite + "should stream text deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_text_deltas()
    {
        var parts = await Stream(Model(ChunksFixture("cohere-text")), Options(false));

        Assert.Equal(12, parts.Count);
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings);
        Assert.Equal(
            "321d178c-2c12-44d3-ae42-2f5510f6b1cc",
            Assert.IsType<ResponseMetadataStreamPart>(parts[1]).Id
        );
        Assert.Equal("0", Assert.IsType<TextStartStreamPart>(parts[2]).Id);
        Assert.Equal(
            "The capital of France is Paris.",
            string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta))
        );
        Assert.All(parts.OfType<TextDeltaStreamPart>(), part => Assert.Equal("0", part.Id));
        Assert.Equal(7, parts.OfType<TextDeltaStreamPart>().Count());
        Assert.Equal("0", Assert.IsType<TextEndStreamPart>(parts[10]).Id);
        var finish = Assert.IsType<FinishStreamPart>(parts[11]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("COMPLETE", finish.RawFinishReason);
        Assert.Equal(507, finish.Usage.InputTokens);
        Assert.Equal(59, finish.Usage.NoCacheInputTokens);
        Assert.Equal(448, finish.Usage.CacheReadTokens);
        Assert.Equal(10, finish.Usage.OutputTokens);
        Assert.Equal(10, finish.Usage.TextTokens);
        JsonAssert.Equal(
            finish.Usage.Raw!.Value,
            "{\"billed_units\":{\"input_tokens\":12,\"output_tokens\":7},\"cached_tokens\":448,\"tokens\":{\"input_tokens\":507,\"output_tokens\":10}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should include raw chunks when includeRawChunks is enabled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_raw_chunks_when_includeRawChunks_is_enabled()
    {
        var parts = await Stream(Model(ChunksFixture("cohere-text")), Options(true));

        var expected = File.ReadAllLines(
                Path.Combine(AppContext.BaseDirectory, "Fixtures", "cohere-text.chunks.txt")
            )
            .Where(line => line.Trim().Length > 0)
            .ToList();
        var raw = parts.OfType<RawStreamPart>().ToList();
        Assert.Equal(expected.Count, raw.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            JsonAssert.Equal(Json(raw[i].RawJson), expected[i]);
        }
    }

    [Fact]
    [UpstreamTest(
        Suite + "should not include raw chunks when includeRawChunks is false",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_include_raw_chunks_when_includeRawChunks_is_false()
    {
        var parts = await Stream(Model(ChunksFixture("cohere-text")), Options(false));

        Assert.Empty(parts.OfType<RawStreamPart>());
    }

    [Fact]
    [UpstreamTest(
        Suite + "should preserve extra top-level and nested fields in raw usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_extra_top_level_and_nested_fields_in_raw_usage()
    {
        const string rawUsage =
            "{\"billed_units\":{\"input_tokens\":12,\"output_tokens\":7,\"billing_tier\":\"standard\"},\"tokens\":{\"input_tokens\":507,\"output_tokens\":10,\"tokenizer\":\"command\"},\"cached_tokens\":448,\"provider_usage_id\":\"usage-123\"}";

        var finish = await Finish(rawUsage);

        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("COMPLETE", finish.RawFinishReason);
        Assert.Equal(507, finish.Usage.InputTokens);
        Assert.Equal(59, finish.Usage.NoCacheInputTokens);
        Assert.Equal(448, finish.Usage.CacheReadTokens);
        Assert.Null(finish.Usage.CacheWriteTokens);
        Assert.Equal(10, finish.Usage.OutputTokens);
        Assert.Equal(10, finish.Usage.TextTokens);
        Assert.Null(finish.Usage.ReasoningTokens);
        JsonAssert.Equal(finish.Usage.Raw!.Value, rawUsage);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should finish successfully when billed units are absent",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_finish_successfully_when_billed_units_are_absent()
    {
        const string rawUsage = "{\"tokens\":{\"input_tokens\":507,\"output_tokens\":10}}";

        var finish = await Finish(rawUsage);

        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("COMPLETE", finish.RawFinishReason);
        Assert.Equal(507, finish.Usage.InputTokens);
        Assert.Equal(507, finish.Usage.NoCacheInputTokens);
        Assert.Null(finish.Usage.CacheReadTokens);
        Assert.Null(finish.Usage.CacheWriteTokens);
        Assert.Equal(10, finish.Usage.OutputTokens);
        Assert.Equal(10, finish.Usage.TextTokens);
        Assert.Null(finish.Usage.ReasoningTokens);
        JsonAssert.Equal(finish.Usage.Raw!.Value, rawUsage);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should return undefined token counts when message-end has no tokens",
        Coverage = UpstreamCoverage.Partial,
        Note = "NoCacheInputTokens and TextTokens stay null because Cohere reports no cache-write or reasoning counts. Upstream reports them equal to the totals.",
        Commit = "3aad18a4178eada4ef31b4c169933a2dfe0f792f"
    )]
    public async Task Should_return_undefined_token_counts_when_message_end_has_no_tokens()
    {
        const string rawUsage =
            "{\"billed_units\":{\"input_tokens\":88,\"output_tokens\":13},\"cached_tokens\":128}";
        var handler = CohereTestSupport.ChunkLines(
            "{\"type\":\"message-end\",\"delta\":{\"finish_reason\":\"COMPLETE\",\"usage\":"
                + rawUsage
                + "}}"
        );

        var parts = await CohereTestSupport.Stream(
            CohereTestSupport.Model(handler),
            new LanguageModelCallOptions
            {
                Prompt = CohereTestSupport.TestPrompt().Prompt,
                IncludeRawChunks = false,
            }
        );

        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("COMPLETE", finish.RawFinishReason);
        Assert.Null(finish.Usage.InputTokens);
        Assert.Null(finish.Usage.OutputTokens);
        Assert.Null(finish.Usage.NoCacheInputTokens);
        Assert.Equal(128, finish.Usage.CacheReadTokens);
        Assert.Null(finish.Usage.CacheWriteTokens);
        Assert.Null(finish.Usage.ReasoningTokens);
        JsonAssert.Equal(finish.Usage.Raw!.Value, rawUsage);
    }

    [Theory]
    [InlineData(
        "billed input tokens",
        "{\"billed_units\":{\"input_tokens\":\"12\",\"output_tokens\":7},\"tokens\":{\"input_tokens\":507,\"output_tokens\":10},\"cached_tokens\":448}"
    )]
    [InlineData(
        "cached tokens",
        "{\"billed_units\":{\"input_tokens\":12,\"output_tokens\":7},\"tokens\":{\"input_tokens\":507,\"output_tokens\":10},\"cached_tokens\":\"448\"}"
    )]
    [UpstreamTest(Suite + "should validate streamed $field", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_validate_streamed_field(string field, string usage)
    {
        Assert.NotEmpty(field);

        var parts = await Stream(
            Model(
                ChunkLines(
                    "{\"type\":\"message-end\",\"delta\":{\"finish_reason\":\"COMPLETE\",\"usage\":"
                        + usage
                        + "}}"
                )
            ),
            Options(false)
        );

        Assert.Equal(3, parts.Count);
        Assert.IsType<StreamStartStreamPart>(parts[0]);
        Assert.IsType<ErrorStreamPart>(parts[1]);
        var finish = Assert.IsType<FinishStreamPart>(parts[2]);
        Assert.Equal(FinishReason.Error, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
        Assert.Null(finish.Usage.InputTokens);
        Assert.Null(finish.Usage.NoCacheInputTokens);
        Assert.Null(finish.Usage.CacheReadTokens);
        Assert.Null(finish.Usage.CacheWriteTokens);
        Assert.Null(finish.Usage.OutputTokens);
        Assert.Null(finish.Usage.TextTokens);
        Assert.Null(finish.Usage.ReasoningTokens);
    }
}
