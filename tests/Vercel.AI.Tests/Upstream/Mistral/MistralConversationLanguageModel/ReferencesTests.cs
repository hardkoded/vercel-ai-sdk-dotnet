// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Mistral.MistralConversationLanguageModel.MistralConversationFixtures;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralConversationLanguageModel;

/// <summary>Port of <c>mistral-conversation-language-model.test.ts</c> &gt; <c>Mistral web search</c> &gt; <c>%s references</c>.</summary>
public sealed class ReferencesTests
{
    private const string Prefix = "packages/mistral/src/mistral-conversation-language-model.test.ts::Mistral web search > %s references::";

    public static TheoryData<string, string, string, string> Cases()
    {
        var data = new TheoryData<string, string, string, string>();
        foreach (var method in new[] { "doGenerate", "doStream" })
        {
            data.Add(method, "missing title", "{\"type\":\"tool_reference\",\"tool\":\"web_search\",\"url\":\"https://example.com/results\"}", "https://example.com/results");
            data.Add(method, "missing tool", "{\"type\":\"tool_reference\",\"title\":\"Results\",\"url\":\"https://example.com/results\"}", "Results");
            data.Add(method, "missing title and tool", "{\"type\":\"tool_reference\",\"url\":\"https://example.com/results\"}", "https://example.com/results");
            data.Add(method, "null title and tool", "{\"type\":\"tool_reference\",\"title\":null,\"tool\":null,\"url\":\"https://example.com/results\"}", "https://example.com/results");
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(Prefix + "handles $description without losing the answer", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_description_without_losing_the_answer(string method, string description, string chunk, string title)
    {
        Assert.NotEmpty(description);
        if (method == "doGenerate")
        {
            var capture = Capture("[{\"type\":\"message.output\",\"content\":[" + chunk + ",{\"type\":\"text\",\"text\":\"Answer\"}]}]");

            var result = await Model(capture).DoGenerateAsync(new LanguageModelCallOptions { Prompt = PromptMessages, Tools = Tools }, CancellationToken.None);

            Assert.Contains(result.Content, content => content is GeneratedText { Text: "Answer" });
            Assert.Equal(FinishReason.Stop, result.FinishReason);
            Assert.Contains(result.Content, content => content is GeneratedSource source && source.Url == "https://example.com/results" && source.Title == title);
        }
        else
        {
            var capture = StreamCapture(
                "{\"type\":\"message.output.delta\",\"id\":\"message-1\",\"content\":" + chunk + "}",
                "{\"type\":\"message.output.delta\",\"id\":\"message-1\",\"content\":\"Answer\"}",
                "{\"type\":\"conversation.response.done\",\"usage\":" + UsageJson + "}");

            var result = await Model(capture).DoStreamAsync(new LanguageModelCallOptions { Prompt = PromptMessages, Tools = Tools }, CancellationToken.None);
            var parts = await Read(result);

            Assert.Contains(parts.OfType<TextDeltaStreamPart>(), part => part.Id == "message-1-0" && part.Delta == "Answer");
            Assert.Equal(FinishReason.Stop, ((FinishStreamPart)parts[^1]).FinishReason);
            Assert.DoesNotContain(parts, part => part is ErrorStreamPart);
            Assert.Contains(parts.OfType<SourceStreamPart>(), source => source.Url == "https://example.com/results" && source.Title == title);
        }
    }
}
