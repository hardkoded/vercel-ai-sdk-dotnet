// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.EmptyTextProviderMetadata;

/// <summary>Port of <c>empty-text-provider-metadata.test.ts</c> &gt; <c>public empty text response metadata preservation</c>.</summary>
public sealed class PublicEmptyTextResponseMetadataPreservationTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/empty-text-provider-metadata.test.ts::public empty text response metadata preservation::";

    public static IEnumerable<object?[]> Cases()
    {
        foreach (var api in new[] { "streamText", "generateText" })
        {
            yield return [api, "undefined metadata", null, false];
            yield return [api, "null metadata from a JavaScript provider", "null", false];
            yield return [api, "an empty metadata object", "{}", true];
            yield return
            [
                api,
                "opaque generic provider metadata",
                "{\"custom\":{\"opaque\":\"preserve-exactly/+=\"}}",
                true,
            ];
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(
        Prefix + "${api}: $name",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fd7e8a75d719314f79528f0c9fda734bb83e68f5"
    )]
    public async Task Preserves_empty_text_only_with_provider_metadata(
        string api,
        string name,
        string? metadataJson,
        bool retain
    )
    {
        Assert.NotEmpty(name);
        JsonElement? metadata = metadataJson is null
            ? null
            : JsonDocument.Parse(metadataJson).RootElement.Clone();
        var usage = new LanguageModelUsage(1, 1, 2);
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[]
                {
                    new GeneratedText("A"),
                    new GeneratedText("", null, metadata),
                    new GeneratedText("B"),
                },
                FinishReason.Stop,
                usage,
                "STOP"
            ),
            StreamParts =
            [
                new TextStartStreamPart("a"),
                new TextDeltaStreamPart("a", "A"),
                new TextEndStreamPart("a"),
                new TextStartStreamPart("empty", metadata),
                new TextDeltaStreamPart("empty", "", metadata),
                new TextEndStreamPart("empty"),
                new TextStartStreamPart("b"),
                new TextDeltaStreamPart("b", "B"),
                new TextEndStreamPart("b"),
                new FinishStreamPart(FinishReason.Stop, usage, "STOP"),
            ],
        };
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

        StepResult step;
        if (api == "streamText")
        {
            var result = client.StreamTextAsync(
                new StreamTextOptions { Model = model, Prompt = "Metadata preservation" }
            );
            await foreach (var part in result.Stream())
            {
                Assert.IsNotType<ErrorPart>(part);
            }

            step = (await result.Steps).Last();
        }
        else
        {
            step = (
                await client.GenerateTextAsync(
                    new GenerateTextOptions { Model = model, Prompt = "Metadata preservation" }
                )
            ).FinalStep;
        }

        var message = Assert.IsType<AssistantModelMessage>(Assert.Single(step.Response.Messages));
        var texts = message.Parts.Cast<AssistantTextPart>().ToList();
        Assert.Equal(retain ? ["A", "", "B"] : ["A", "B"], texts.Select(part => part.Text));
        Assert.All(texts.Where(part => part.Text != "A" && part.Text != "B"), part =>
            Assert.NotNull(part.ProviderOptions)
        );
        if (retain)
        {
            Assert.Contains(step.Content.OfType<GeneratedText>(), text =>
                text.Text.Length == 0 && text.ProviderMetadata is not null
            );
        }
    }
}
