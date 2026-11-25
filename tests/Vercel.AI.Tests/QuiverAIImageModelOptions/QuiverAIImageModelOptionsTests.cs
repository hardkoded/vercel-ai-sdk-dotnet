// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.QuiverAI;
using Vercel.AI.Tests.Upstream;
using ImageOptions = Vercel.AI.QuiverAI.QuiverAIImageModelOptions;

namespace Vercel.AI.Tests.QuiverAIImageModelOptions;

public sealed class QuiverAIImageModelOptionsTests
{
    private const string Commit = "4fb2fd377d2e0f626c947c4d8fff8a59628bbace";
    private const string Prefix =
        "packages/quiverai/src/quiverai-image-model-options.test-d.ts::QuiverAIImageModelOptions::";

    [Fact]
    [UpstreamTest(
        Prefix + "supports SVG animation as an image operation",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void SupportsSvgAnimationAsAnImageOperation()
    {
        var options = new ImageOptions { Operation = QuiverAIImageOperation.Animate };

        Assert.Equal("animate", Quiver(options)["operation"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "supports SVG editing with references and settings",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void SupportsSvgEditingWithReferencesAndSettings()
    {
        var options = new ImageOptions
        {
            Operation = QuiverAIImageOperation.Edit,
            ReferenceImages =
            [
                QuiverAIImageReference.Prepare(new Uri("https://example.com/reference.png")),
                QuiverAIImageReference.Prepare(PngBytes),
            ],
            MaxReviewSteps = 2,
            ReasoningEffort = QuiverAIReasoningEffort.High,
            MaxOutputTokens = 4096,
            OrchestratorMaxOutputTokens = 2048,
            ShallowMaxOutputTokens = 1024,
            Temperature = 0.3,
        };

        var json = Quiver(options);
        Assert.Equal("edit", json["operation"]!.GetValue<string>());
        Assert.Equal("high", json["reasoningEffort"]!.GetValue<string>());
        Assert.Equal(2, json["maxReviewSteps"]!.GetValue<int>());
        Assert.Equal(4096, json["maxOutputTokens"]!.GetValue<int>());
        Assert.Equal(2048, json["orchestratorMaxOutputTokens"]!.GetValue<int>());
        Assert.Equal(1024, json["shallowMaxOutputTokens"]!.GetValue<int>());
        Assert.Equal(0.3, json["temperature"]!.GetValue<double>());
        var references = json["referenceImages"]!.AsArray();
        Assert.Equal(2, references.Count);
        Assert.Equal(
            "https://example.com/reference.png",
            references[0]!["url"]!.GetValue<string>()
        );
        Assert.Equal(
            Convert.ToBase64String(PngBytes),
            references[1]!["base64"]!.GetValue<string>()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "supports all image operations",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void SupportsAllImageOperations()
    {
        Assert.Equal(
            ["generate", "vectorize", "animate", "edit"],
            Enum.GetValues<QuiverAIImageOperation>()
                .Select(operation =>
                    Quiver(new ImageOptions { Operation = operation })[
                        "operation"
                    ]!.GetValue<string>()
                )
        );
    }

    private static readonly byte[] PngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private static JsonNode Quiver(ImageOptions options) =>
        JsonNode.Parse(options.ToProviderOptions()["quiverai"].GetRawText())!;
}
