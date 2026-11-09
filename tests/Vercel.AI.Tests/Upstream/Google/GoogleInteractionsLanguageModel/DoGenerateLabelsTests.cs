// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt; <c>labels</c>.
/// </summary>
public sealed class DoGenerateLabelsTests
{
    private const string Prefix = "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doGenerate > labels::";

    private static LanguageModelCallOptions Options(string providerOptions)
    {
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        options.ProviderOptions = GoogleUpstream.ProviderOptions(providerOptions);
        return options;
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"application\":\"example\",\"safety_identifier\":\"opaque_user_123\"}")]
    [UpstreamTest(Prefix + "forwards labels %j as top-level request metadata", Coverage = UpstreamCoverage.Covered, Commit = "a210a29f9b1be06d87e9e880840419defecbee2b")]
    public async Task Forwards_labels_as_top_level_request_metadata(string labels)
    {
        var handler = new SequencedHandler().Json(GoogleInteractionsStreamSupport.JsonFixture("basic"));

        await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options("{\"google\":{\"labels\":" + labels + ",\"store\":false}}"), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(labels), body["labels"]));
        Assert.False((bool)body["store"]!);
    }

    [Theory]
    [InlineData("undefined")]
    [InlineData("null")]
    [UpstreamTest(Prefix + "omits labels when set to %j", Coverage = UpstreamCoverage.Covered, Commit = "a210a29f9b1be06d87e9e880840419defecbee2b")]
    public async Task Omits_labels_when_set_to(string labels)
    {
        var handler = new SequencedHandler().Json(GoogleInteractionsStreamSupport.JsonFixture("basic"));
        var providerOptions = labels == "null" ? "{\"google\":{\"labels\":null}}" : "{\"google\":{}}";

        await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options(providerOptions), CancellationToken.None);

        Assert.False(JsonNode.Parse(handler.Bodies[0])!.AsObject().ContainsKey("labels"));
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects labels with non-string values before sending a request", Coverage = UpstreamCoverage.Covered, Commit = "a210a29f9b1be06d87e9e880840419defecbee2b")]
    public async Task Rejects_labels_with_non_string_values_before_sending_a_request()
    {
        var handler = new SequencedHandler().Json(GoogleInteractionsStreamSupport.JsonFixture("basic"));

        await Assert.ThrowsAnyAsync<Exception>(() => GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options("{\"google\":{\"labels\":{\"invalid\":42}}}"), CancellationToken.None));

        Assert.Empty(handler.Bodies);
    }
}
