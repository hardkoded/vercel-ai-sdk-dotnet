// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using DecisionModel = Vercel.AI.Operations.DecisionLanguageModel;

namespace Vercel.AI.Tests.DecisionLanguageModel;

/// <summary>Port of <c>decision-language-model.test.ts</c>.</summary>
public sealed class DecisionLanguageModelTests
{
    private static Dictionary<string, EvaluationQuestion> Questions()
    {
        return new Dictionary<string, EvaluationQuestion>
        {
            ["category"] = new EvaluationQuestion(
                "choice",
                "Pick the category.",
                new Dictionary<string, object?> { ["Needs Review"] = "Review it.", ["Needs review"] = "Review it too.", ["other"] = null }),
            ["severity"] = new EvaluationQuestion(
                "score",
                "Rate the severity.",
                new List<object?> { "low", new Dictionary<string, object?> { ["meaning"] = "medium" }, null }),
        };
    }

    private static (DecisionModel Model, TestLanguageModel Language) Setup()
    {
        var language = new TestLanguageModel("test-model")
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("{\"q1\":1.25,\"q0\":\"c1\"}") },
                FinishReason.Stop,
                new LanguageModelUsage(20, 12, 32),
                "end",
                new[] { new CallWarning("other", "test warning") }),
        };
        return (new DecisionModel(language, "test.decision"), language);
    }

    private static EvaluationModelCall Call(IReadOnlyList<object?> state)
    {
        return new EvaluationModelCall(state, Questions(), default, new Dictionary<string, string>(), CancellationToken.None);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/decision-language-model.test.ts::sends images as files and preserves state part order in the prompt", Coverage = UpstreamCoverage.Partial, Note = "The C# file content part has no provider options, so the image's detail option is not asserted.")]
    public async Task Sends_images_as_files_and_preserves_state_part_order_in_the_prompt()
    {
        var (model, language) = Setup();
        var image = new Dictionary<string, object?>
        {
            ["type"] = "file",
            ["mediaType"] = "image/png",
            ["data"] = new Dictionary<string, object?> { ["type"] = "data", ["data"] = new byte[] { 1, 2 } },
        };

        await model.DoDecideAsync(Call(new List<object?>
        {
            new Dictionary<string, object?> { ["type"] = "text", ["text"] = "Inspect." },
            image,
            new Dictionary<string, object?> { ["type"] = "json", ["value"] = new List<object?> { 1, null } },
        }));

        var user = Assert.IsType<UserModelMessage>(language.Calls[0].Prompt[1]);
        Assert.Equal(5, user.Content.Count);
        Assert.Contains("\"questions\"", Assert.IsType<TextContentPart>(user.Content[0]).Text);
        Assert.Equal("Shared state:", Assert.IsType<TextContentPart>(user.Content[1]).Text);
        Assert.Equal("Inspect.", Assert.IsType<TextContentPart>(user.Content[2]).Text);
        var file = Assert.IsType<FileContentPart>(user.Content[3]);
        Assert.Equal("image/png", file.MediaType);
        Assert.Equal(new byte[] { 1, 2 }, file.Data);
        Assert.Equal("[1,null]", Assert.IsType<TextContentPart>(user.Content[4]).Text);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/decision-language-model.test.ts::rejects unsupported media before generating a decision", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_media_before_generating_a_decision()
    {
        var (model, language) = Setup();

        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => model.DoDecideAsync(Call(new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["type"] = "file",
                ["mediaType"] = "audio/wav",
                ["data"] = new Dictionary<string, object?> { ["type"] = "data", ["data"] = "AAAA" },
            },
        })));

        Assert.Empty(language.Calls);
    }
}
