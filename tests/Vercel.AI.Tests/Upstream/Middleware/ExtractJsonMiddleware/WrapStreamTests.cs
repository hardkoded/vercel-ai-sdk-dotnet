// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Json = global::Vercel.AI.ExtractJsonMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.ExtractJsonMiddleware;

/// <summary>Port of <c>extract-json-middleware.test.ts</c> &gt; <c>extractJsonMiddleware &gt; wrapStream</c>.</summary>
public sealed class WrapStreamTests
{
    private const string Id =
        "packages/ai/src/middleware/extract-json-middleware.test.ts::extractJsonMiddleware > wrapStream::";

    private static async IAsyncEnumerable<LanguageModelStreamPart> Stream(
        params LanguageModelStreamPart[] parts
    )
    {
        foreach (var part in parts)
        {
            yield return part;
            await Task.Yield();
        }
    }

    private static async Task<List<LanguageModelStreamPart>> RunParts(
        LanguageModelStreamPart[] input,
        Func<string, string>? transform = null
    )
    {
        var wrapped = await new Json(transform).WrapStreamAsync(
            new LanguageModelWrapContext(
                new LanguageModelCallOptions(),
                new TestLanguageModel(),
                _ => throw new NotSupportedException(),
                _ => Task.FromResult(new LanguageModelStreamResult(Stream(input)))
            ),
            CancellationToken.None
        );
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in wrapped)
        {
            parts.Add(part);
        }

        return parts;
    }

    private static Task<List<LanguageModelStreamPart>> Run(params string[] deltas) =>
        Run(deltas, null);

    private static Task<List<LanguageModelStreamPart>> Run(
        string[] deltas,
        Func<string, string>? transform
    )
    {
        var input = new List<LanguageModelStreamPart> { new TextStartStreamPart("1") };
        input.AddRange(deltas.Select(d => new TextDeltaStreamPart("1", d)));
        input.Add(new TextEndStreamPart("1"));
        input.Add(
            new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(5, 10, 15), "stop")
        );
        return RunParts(input.ToArray(), transform);
    }

    private static string Text(IEnumerable<LanguageModelStreamPart> parts) =>
        string.Concat(parts.OfType<TextDeltaStreamPart>().Select(p => p.Delta));

    [Fact]
    [UpstreamTest(
        Id + "should not read Object.prototype for missing text part ids",
        Coverage = UpstreamCoverage.Partial,
        Note = "JavaScript Object.prototype pollution has no .NET equivalent. Checks that a delta whose id has no text-start passes through untouched."
    )]
    public async Task Should_not_read_Objectprototype_for_missing_text_part_ids()
    {
        var parts = await RunParts(
            new LanguageModelStreamPart[]
            {
                new TextDeltaStreamPart("__proto__", "{\"value\": \"test\"}"),
            }
        );

        Assert.Equal("{\"value\": \"test\"}", Text(parts));
    }

    [Fact]
    [UpstreamTest(
        Id + "should strip markdown json fence from streamed text",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_strip_markdown_json_fence_from_streamed_text()
    {
        var parts = await Run("```json\n", "{\"value\": \"test\"}", "\n```");
        Assert.Equal("{\"value\": \"test\"}", Text(parts));
    }

    [Fact]
    [UpstreamTest(
        Id + "should strip markdown fence without json tag from streamed text",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_strip_markdown_fence_without_json_tag_from_streamed_text()
    {
        var parts = await Run("```\n", "{\"value\": \"test\"}", "\n```");
        Assert.Equal("{\"value\": \"test\"}", Text(parts));
    }

    [Fact]
    [UpstreamTest(
        Id + "should leave text without fences unchanged in stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_leave_text_without_fences_unchanged_in_stream()
    {
        var parts = await Run("{\"value\": \"test\"}");
        Assert.Equal("{\"value\": \"test\"}", Text(parts));
    }

    [Fact]
    [UpstreamTest(
        Id + "should preserve leading space in final streamed suffix without fences",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_leading_space_in_final_streamed_suffix_without_fences()
    {
        var middleware = new Json();
        var wrapped = await middleware.WrapStreamAsync(
            new LanguageModelWrapContext(
                new LanguageModelCallOptions(),
                new TestLanguageModel(),
                _ => throw new NotSupportedException(),
                _ =>
                    Task.FromResult(
                        new LanguageModelStreamResult(
                            Stream(
                                new TextStartStreamPart("t"),
                                new TextDeltaStreamPart("t", "there"),
                                new TextDeltaStreamPart("t", " altogether?"),
                                new TextEndStreamPart("t")
                            )
                        )
                    )
            ),
            CancellationToken.None
        );

        var chunks = new List<LanguageModelStreamPart>();
        await foreach (var chunk in wrapped)
        {
            chunks.Add(chunk);
        }

        Assert.Equal(
            "there altogether?",
            string.Concat(chunks.OfType<TextDeltaStreamPart>().Select(c => c.Delta))
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle fence split across multiple deltas",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_fence_split_across_multiple_deltas()
    {
        var parts = await Run("`", "``", "json\n", "{\"value\": \"test\"}", "\n`", "``");
        Assert.Equal("{\"value\": \"test\"}", Text(parts));
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle content that starts with backtick but is not a fence",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_content_that_starts_with_backtick_but_is_not_a_fence()
    {
        var parts = await Run("`code`");
        Assert.Equal("`code`", Text(parts));
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass through non-text chunks unchanged",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_through_nontext_chunks_unchanged()
    {
        var parts = await RunParts(
            new LanguageModelStreamPart[]
            {
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "```json\n"),
                new TextDeltaStreamPart("1", "{\"value\": \"test\"}"),
                new TextDeltaStreamPart("1", "\n```"),
                new TextEndStreamPart("1"),
                new ToolInputStartStreamPart("tool-1", "testTool"),
                new ToolInputDeltaStreamPart("tool-1", "{\"arg\": \"value\"}"),
                new ToolInputEndStreamPart("tool-1"),
            }
        );

        Assert.NotEmpty(parts.OfType<ToolInputStartStreamPart>());
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle multiple text blocks with different IDs",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_multiple_text_blocks_with_different_IDs()
    {
        var parts = await RunParts(
            new LanguageModelStreamPart[]
            {
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "```json\n"),
                new TextDeltaStreamPart("1", "{\"first\": true}"),
                new TextDeltaStreamPart("1", "\n```"),
                new TextEndStreamPart("1"),
                new TextStartStreamPart("2"),
                new TextDeltaStreamPart("2", "```json\n"),
                new TextDeltaStreamPart("2", "{\"second\": true}"),
                new TextDeltaStreamPart("2", "\n```"),
                new TextEndStreamPart("2"),
            }
        );

        var allText = string.Concat(parts.OfType<TextDeltaStreamPart>().Select(d => d.Delta));
        Assert.Contains("{\"first\": true}", allText);
        Assert.Contains("{\"second\": true}", allText);
        Assert.DoesNotContain("```", allText);
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle text-delta without prior text-start",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_textdelta_without_prior_textstart()
    {
        var parts = await RunParts(
            new LanguageModelStreamPart[] { new TextDeltaStreamPart("unknown", "some text") }
        );

        Assert.True(parts.OfType<TextDeltaStreamPart>().Count() >= 1);
    }

    [Fact]
    [UpstreamTest(
        Id + "should emit text-start when stream ends while still in prefix phase",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_emit_textstart_when_stream_ends_while_still_in_prefix_phase()
    {
        var parts = await RunParts(
            new LanguageModelStreamPart[]
            {
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "``"),
                new TextEndStreamPart("1"),
            }
        );

        Assert.Single(parts.OfType<TextStartStreamPart>());
        Assert.Single(parts.OfType<TextEndStreamPart>());
    }

    [Fact]
    [UpstreamTest(
        Id + "should apply custom transform to streamed content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_apply_custom_transform_to_streamed_content()
    {
        var parts = await Run(new[] { "PREFIX", "{\"value\": \"test\"}", "SUFFIX" }, t =>
            t.Replace("PREFIX", "").Replace("SUFFIX", "")
        );
        Assert.Equal("{\"value\": \"test\"}", Text(parts));
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle large content exceeding suffix buffer",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_large_content_exceeding_suffix_buffer()
    {
        var largeJson =
            "{\"data\":\""
            + new string('x', 100)
            + "\",\"nested\":{\"values\":[0,1,2,3,4,5,6,7,8,9]}}";
        var parts = await Run("```json\n", largeJson, "\n```");

        Assert.Equal(largeJson, Text(parts));
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle content arriving character by character",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_content_arriving_character_by_character()
    {
        var chars = ("```json\n" + "{\"value\": \"test\"}" + "\n```")
            .Select(c => c.ToString())
            .ToArray();
        var parts = await Run(chars);

        Assert.Equal("{\"value\": \"test\"}", Text(parts));
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle fence with extra whitespace",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_fence_with_extra_whitespace()
    {
        var parts = await Run("```json  \n", "{\"value\": \"test\"}", "\n```  ");
        Assert.Equal("{\"value\": \"test\"}", Text(parts));
    }

    [Fact]
    [UpstreamTest(
        Id + "should strip a closing fence followed by long trailing whitespace",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_strip_a_closing_fence_followed_by_long_trailing_whitespace()
    {
        var json = "{\"value\": \"test\"}";
        var fencedJson = "```json\n" + json + "\n```";

        foreach (var trailingWhitespace in new[] { new string(' ', 20), new string('\n', 20) })
        {
            var text = fencedJson + trailingWhitespace;
            var chunkLayouts = new[]
            {
                new[] { text },
                text.Select(c => c.ToString()).ToArray(),
                new[] { "```json\n" + json + "\n", "```", trailingWhitespace },
            };

            foreach (var chunks in chunkLayouts)
            {
                Assert.Equal(json, Text(await Run(chunks)));
            }
        }
    }

    [Fact]
    [UpstreamTest(
        Id + "should efficiently stream whitespace-heavy content followed by non-whitespace",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_efficiently_stream_whitespaceheavy_content_followed_by_nonwhitespace()
    {
        var text = new string(' ', 10_000) + "x";
        var parts = await Run(text);

        Assert.Equal(text, Text(parts));
    }

    [Fact]
    [UpstreamTest(
        Id + "should verify stream output matches expected structure",
        Coverage = UpstreamCoverage.Covered,
        Note = "The start part is emitted by StreamText, not the middleware, so the check runs on the model stream."
    )]
    public async Task Should_verify_stream_output_matches_expected_structure()
    {
        var parts = await Run("```json\n", "{\"value\": \"test\"}", "\n```");

        Assert.NotEmpty(parts.OfType<TextStartStreamPart>());
        Assert.NotEmpty(parts.OfType<TextEndStreamPart>());
        Assert.NotEmpty(parts.OfType<FinishStreamPart>());
        Assert.Equal("{\"value\": \"test\"}", Text(parts));
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle empty content between fences",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_empty_content_between_fences()
    {
        var parts = await Run("```json\n```");
        Assert.Equal("", Text(parts));
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle content starting without backtick quickly switching to streaming",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_content_starting_without_backtick_quickly_switching_to_streaming()
    {
        var parts = await Run("{", "\"value\": \"test\"", "}");
        Assert.Equal("{\"value\": \"test\"}", Text(parts));
    }
}
