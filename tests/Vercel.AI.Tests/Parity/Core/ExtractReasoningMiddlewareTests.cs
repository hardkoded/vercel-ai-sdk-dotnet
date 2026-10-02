// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class ExtractReasoningMiddlewareTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/extract-reasoning-middleware.test.ts::extractReasoningMiddleware > wrapGenerate::should extract reasoning from <think> tags when there is no text",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Wrap_generate_keeps_reasoning_when_the_answer_is_empty()
    {
        var result = await Generate("<think>analyzing the request\n</think>");
        Assert.Equal(new[] { "reasoning:analyzing the request\n", "text:" }, Describe(result));
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal(5, result.Usage.InputTokens);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/extract-reasoning-middleware.test.ts::extractReasoningMiddleware > wrapGenerate::should extract reasoning from multiple <think> tags",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Wrap_generate_joins_multiple_think_tags()
    {
        var result = await Generate(
            "<think>analyzing the request</think>Here is the response<think>thinking about the response</think>more");
        Assert.Equal(
            new[]
            {
                "reasoning:analyzing the request\nthinking about the response",
                "text:Here is the response\nmore",
            },
            Describe(result));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/extract-reasoning-middleware.test.ts::extractReasoningMiddleware > wrapGenerate::should prepend <think> tag IFF startWithReasoning is true",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Wrap_generate_prepends_the_opening_tag_only_when_requested()
    {
        const string text = "analyzing the request</think>Here is the response";
        var started = await Generate(text, new ExtractReasoningMiddleware("think", "\n", true));
        var plain = await Generate(text, new ExtractReasoningMiddleware("think", "\n", false));
        Assert.Equal(
            new[] { "reasoning:analyzing the request", "text:Here is the response" },
            Describe(started));
        Assert.Equal(new[] { "text:" + text }, Describe(plain));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/extract-reasoning-middleware.test.ts::extractReasoningMiddleware > wrapGenerate::should preserve reasoning property even when rest contains other properties",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Wrap_generate_keeps_usage_finish_and_metadata()
    {
        using var metadata = JsonDocument.Parse("{\"kept\":true}");
        var usage = new LanguageModelUsage(5, 10, 15, 0, 0, 3);
        var model = new ScriptedLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("<think>analyzing the request</think>Here is the response") },
                FinishReason.Stop,
                usage,
                "stop",
                new[] { new CallWarning("other", "kept") },
                "resp-1",
                metadata.RootElement.Clone()),
        }.WrapLanguageModel(new ExtractReasoningMiddleware("think", "\n", false));

        var result = await model.DoGenerateAsync(new LanguageModelCallOptions(), CancellationToken.None);
        Assert.Equal(
            new[] { "reasoning:analyzing the request", "text:Here is the response" },
            Describe(result));
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("stop", result.RawFinishReason);
        Assert.Equal(5, result.Usage.InputTokens);
        Assert.Equal(10, result.Usage.OutputTokens);
        Assert.Equal(15, result.Usage.TotalTokens);
        Assert.Equal(0, result.Usage.CacheReadTokens);
        Assert.Equal(0, result.Usage.CacheWriteTokens);
        Assert.Equal(3, result.Usage.ReasoningTokens);
        Assert.Equal("resp-1", result.ResponseId);
        Assert.Equal("kept", Assert.Single(result.Warnings).Message);
        Assert.True(result.ProviderMetadata.HasValue);
        Assert.True(result.ProviderMetadata!.Value.GetProperty("kept").GetBoolean());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/extract-reasoning-middleware.test.ts::extractReasoningMiddleware > wrapStream::should extract reasoning from split <think> tags",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Wrap_stream_splits_a_think_tag_across_deltas()
    {
        var parts = await Stream(
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", "<think>"),
            new TextDeltaStreamPart("1", "ana"),
            new TextDeltaStreamPart("1", "lyzing the request"),
            new TextDeltaStreamPart("1", "</think>"),
            new TextDeltaStreamPart("1", "Here"),
            new TextDeltaStreamPart("1", " is the response"),
            new TextEndStreamPart("1"));
        Assert.Equal(
            new[]
            {
                "reasoning-start:reasoning-0",
                "reasoning-delta:reasoning-0:ana",
                "reasoning-delta:reasoning-0:lyzing the request",
                "reasoning-end:reasoning-0",
                "text-start:1",
                "text-delta:1:Here",
                "text-delta:1: is the response",
                "text-end:1",
            },
            Describe(parts));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/extract-reasoning-middleware.test.ts::extractReasoningMiddleware > wrapStream::should extract reasoning from single chunk with multiple <think> tags",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Wrap_stream_splits_multiple_think_tags_in_one_chunk()
    {
        var parts = await Stream(
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart(
                "1",
                "<think>analyzing the request</think>Here is the response<think>thinking about the response</think>more"),
            new TextEndStreamPart("1"));
        Assert.Equal(
            new[]
            {
                "reasoning-start:reasoning-0",
                "reasoning-delta:reasoning-0:analyzing the request",
                "reasoning-end:reasoning-0",
                "text-start:1",
                "text-delta:1:Here is the response",
                "reasoning-start:reasoning-1",
                "reasoning-delta:reasoning-1:\nthinking about the response",
                "reasoning-end:reasoning-1",
                "text-delta:1:\nmore",
                "text-end:1",
            },
            Describe(parts));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/extract-reasoning-middleware.test.ts::extractReasoningMiddleware > wrapStream::should extract reasoning from <think> when there is no text",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Wrap_stream_emits_reasoning_and_an_empty_text_block()
    {
        var parts = await Stream(
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", "<think>"),
            new TextDeltaStreamPart("1", "ana"),
            new TextDeltaStreamPart("1", "lyzing the request\n"),
            new TextDeltaStreamPart("1", "</think>"),
            new TextEndStreamPart("1"));
        Assert.Equal(
            new[]
            {
                "reasoning-start:reasoning-0",
                "reasoning-delta:reasoning-0:ana",
                "reasoning-delta:reasoning-0:lyzing the request\n",
                "reasoning-end:reasoning-0",
                "text-start:1",
                "text-end:1",
            },
            Describe(parts));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/extract-reasoning-middleware.test.ts::extractReasoningMiddleware > wrapStream::should prepend <think> tag if startWithReasoning is true",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Wrap_stream_treats_the_prefix_as_reasoning_only_when_requested()
    {
        var chunks = new LanguageModelStreamPart[]
        {
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", "ana"),
            new TextDeltaStreamPart("1", "lyzing the request\n"),
            new TextDeltaStreamPart("1", "</think>"),
            new TextDeltaStreamPart("1", "this is the response"),
            new TextEndStreamPart("1"),
        };
        var started = await Stream(new ExtractReasoningMiddleware("think", "\n", true), chunks);
        var plain = await Stream(new ExtractReasoningMiddleware("think", "\n", false), chunks);
        Assert.Equal(
            new[]
            {
                "reasoning-start:reasoning-0",
                "reasoning-delta:reasoning-0:ana",
                "reasoning-delta:reasoning-0:lyzing the request\n",
                "reasoning-end:reasoning-0",
                "text-start:1",
                "text-delta:1:this is the response",
                "text-end:1",
            },
            Describe(started));
        Assert.Equal(
            new[]
            {
                "text-start:1",
                "text-delta:1:ana",
                "text-delta:1:lyzing the request\n",
                "text-delta:1:</think>",
                "text-delta:1:this is the response",
                "text-end:1",
            },
            Describe(plain));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/extract-reasoning-middleware.test.ts::extractReasoningMiddleware > wrapStream::should keep original text when <think> tag is not present",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Wrap_stream_publishes_text_without_a_think_tag()
    {
        var parts = await Stream(
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", "this is the response"),
            new TextEndStreamPart("1"));
        Assert.Equal(
            new[]
            {
                "text-start:1",
                "text-delta:1:this is the response",
                "text-end:1",
            },
            Describe(parts));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/extract-reasoning-middleware.test.ts::extractReasoningMiddleware > wrapStream::should preserve overlapping text parts",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Wrap_stream_keeps_overlapping_text_in_first_seen_order()
    {
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = Wrapped(new LanguageModelStreamPart[]
            {
                new TextStartStreamPart("a"),
                new TextStartStreamPart("b"),
                new TextDeltaStreamPart("a", "Alpha."),
                new TextDeltaStreamPart("b", "Beta."),
                new TextEndStreamPart("a"),
                new TextEndStreamPart("b"),
                new FinishStreamPart(FinishReason.Stop, Usage(), "stop"),
            }),
            Prompt = "Hello, how can I help?",
        });
        var errors = new List<ErrorPart>();
        await foreach (var part in result.Stream())
        {
            if (part is ErrorPart error)
            {
                errors.Add(error);
            }
        }

        Assert.Empty(errors);
        Assert.Equal("Alpha.Beta.", await result.Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/extract-reasoning-middleware.test.ts::extractReasoningMiddleware > wrapStream::should preserve reasoning from overlapping text parts",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Wrap_stream_groups_overlapping_reasoning_by_block()
    {
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = Wrapped(new LanguageModelStreamPart[]
            {
                new TextStartStreamPart("a"),
                new TextStartStreamPart("b"),
                new TextDeltaStreamPart("a", "<think>A"),
                new TextDeltaStreamPart("b", "<think>B"),
                new TextDeltaStreamPart("a", "1</think>Alpha."),
                new TextDeltaStreamPart("b", "2</think>Beta."),
                new TextEndStreamPart("a"),
                new TextEndStreamPart("b"),
                new FinishStreamPart(FinishReason.Stop, Usage(), "stop"),
            }).WrapLanguageModel(new ExtractReasoningMiddleware("think", "\n", false)),
            Prompt = "Hello, how can I help?",
        });
        var errors = new List<ErrorPart>();
        await foreach (var part in result.Stream())
        {
            if (part is ErrorPart error)
            {
                errors.Add(error);
            }
        }

        Assert.Empty(errors);
        Assert.Equal("A1B2", await result.ReasoningText);
        Assert.Equal("Alpha.Beta.", await result.Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/middleware/extract-reasoning-middleware.test.ts::extractReasoningMiddleware > wrapStream::should handle empty <think></think> tags without crashing",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Wrap_stream_emits_empty_reasoning_around_the_answer()
    {
        var parts = await Stream(
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", "<think></think>"),
            new TextDeltaStreamPart("1", " This is the answer."),
            new TextEndStreamPart("1"));
        var described = Describe(parts);
        var start = Array.IndexOf(described, "reasoning-start:reasoning-0");
        var end = Array.IndexOf(described, "reasoning-end:reasoning-0");
        Assert.True(start >= 0);
        Assert.True(end > start);
        Assert.Contains("text-delta:1: This is the answer.", described);
    }

    private static async Task<LanguageModelGenerateResult> Generate(string text)
    {
        return await Generate(text, new ExtractReasoningMiddleware("think", "\n", false));
    }

    private static async Task<LanguageModelGenerateResult> Generate(string text, ExtractReasoningMiddleware middleware)
    {
        var model = new ScriptedLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText(text) },
                FinishReason.Stop,
                Usage(),
                "stop"),
        }.WrapLanguageModel(middleware);
        return await model.DoGenerateAsync(new LanguageModelCallOptions(), CancellationToken.None);
    }

    private static Task<List<LanguageModelStreamPart>> Stream(params LanguageModelStreamPart[] parts)
    {
        return Stream(new ExtractReasoningMiddleware("think", "\n", false), parts);
    }

    private static async Task<List<LanguageModelStreamPart>> Stream(
        ExtractReasoningMiddleware middleware,
        IReadOnlyList<LanguageModelStreamPart> parts)
    {
        var model = Wrapped(parts).WrapLanguageModel(middleware);
        var emitted = new List<LanguageModelStreamPart>();
        await foreach (var part in model.DoStreamAsync(new LanguageModelCallOptions(), CancellationToken.None))
        {
            emitted.Add(part);
        }

        return emitted;
    }

    private static ScriptedLanguageModel Wrapped(IReadOnlyList<LanguageModelStreamPart> parts)
    {
        return new ScriptedLanguageModel
        {
            OnStream = _ => parts,
        };
    }

    private static LanguageModelUsage Usage()
    {
        return new LanguageModelUsage(5, 10, 15, 0, 0, 3);
    }

    private static string[] Describe(LanguageModelGenerateResult result)
    {
        var described = new List<string>();
        foreach (var part in result.Content)
        {
            if (part is GeneratedReasoning reasoning)
            {
                described.Add("reasoning:" + reasoning.Text);
            }
            else if (part is GeneratedText text)
            {
                described.Add("text:" + text.Text);
            }
            else
            {
                described.Add(part.Type);
            }
        }

        return described.ToArray();
    }

    private static string[] Describe(IReadOnlyList<LanguageModelStreamPart> parts)
    {
        var described = new List<string>();
        foreach (var part in parts)
        {
            switch (part)
            {
                case TextStartStreamPart start:
                    described.Add("text-start:" + start.Id);
                    break;
                case TextEndStreamPart end:
                    described.Add("text-end:" + end.Id);
                    break;
                case TextDeltaStreamPart delta:
                    described.Add("text-delta:" + delta.Id + ":" + delta.Delta);
                    break;
                case ReasoningStartStreamPart reasoningStart:
                    described.Add("reasoning-start:" + reasoningStart.Id);
                    break;
                case ReasoningEndStreamPart reasoningEnd:
                    described.Add("reasoning-end:" + reasoningEnd.Id);
                    break;
                case ReasoningDeltaStreamPart reasoning:
                    described.Add("reasoning-delta:" + reasoning.Id + ":" + reasoning.Delta);
                    break;
            }
        }

        return described.ToArray();
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
