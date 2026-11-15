// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-vertex-interactions-language-model.test.ts</c> &gt; <c>Vertex Interactions labels (%s)</c>,
/// run once for <c>generate</c> and once for <c>stream</c>.
/// </summary>
public sealed class GoogleVertexInteractionsLanguageModelTests
{
    private const string Prefix = "packages/google-vertex/src/google-vertex-interactions-language-model.test.ts::Vertex Interactions labels (";

    private const string Response = "{\"id\":\"interaction_test\",\"object\":\"interaction\",\"status\":\"completed\",\"model\":\"gemini-2.5-flash\",\"steps\":[{\"type\":\"model_output\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]}]}";

    private static ILanguageModel Model(SequencedHandler handler)
    {
        var provider = GoogleVertexProvider.Create(new VertexOptions
        {
            Project = "test-project",
            Region = "global",
            UpstreamRoutes = true,
            GenerateAuthToken = _ => Task.FromResult("test-token"),
        }, handler);
        return provider.Interactions("gemini-2.5-flash");
    }

    private static async Task<JsonObject> Send(string mode, string? labels)
    {
        var handler = mode == "generate"
            ? new SequencedHandler().Json(Response)
            : new SequencedHandler().Sse(
                "data: {\"event_type\":\"interaction.created\",\"interaction\":{\"id\":\"interaction_test\",\"status\":\"in_progress\",\"model\":\"gemini-2.5-flash\"}}\n\n" +
                "data: {\"event_type\":\"interaction.completed\",\"interaction\":" + Response + "}\n\n");
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello"));
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{" + (labels == null ? string.Empty : "\"labels\":" + labels + ",") + "\"store\":false}}");
        var model = Model(handler);
        if (mode == "generate")
        {
            await model.DoGenerateAsync(options, CancellationToken.None);
        }
        else
        {
            await GoogleInteractionsStreamSupport.Stream(model, options);
        }

        return JsonNode.Parse(handler.Bodies[0])!.AsObject();
    }

    private static void Check(JsonObject body, string? labels)
    {
        Assert.False((bool)body["store"]!);
        if (labels is null or "null")
        {
            Assert.False(body.ContainsKey("labels"));
        }
        else
        {
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(labels), body["labels"]));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"application\":\"example\",\"safety_identifier\":\"opaque_user_123\"}")]
    [UpstreamTest(Prefix + "generate)::preserves $name labels and other options", Coverage = UpstreamCoverage.Covered, Commit = "a210a29f9b1be06d87e9e880840419defecbee2b")]
    public async Task Generate_preserves_labels_and_other_options(string? labels)
    {
        Check(await Send("generate", labels), labels);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"application\":\"example\",\"safety_identifier\":\"opaque_user_123\"}")]
    [UpstreamTest(Prefix + "stream)::preserves $name labels and other options", Coverage = UpstreamCoverage.Covered, Commit = "a210a29f9b1be06d87e9e880840419defecbee2b")]
    public async Task Stream_preserves_labels_and_other_options(string? labels)
    {
        Check(await Send("stream", labels), labels);
    }
}
