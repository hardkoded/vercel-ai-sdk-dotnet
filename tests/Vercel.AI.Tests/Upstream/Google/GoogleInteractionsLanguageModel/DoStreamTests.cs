// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doStream</c>
/// (the tests outside a nested <c>describe</c>; only the labels tests are ported here).
/// </summary>
public sealed class DoStreamTests
{
    private const string Prefix = "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doStream::";

    private static LanguageModelCallOptions Options(string providerOptions)
    {
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        options.ProviderOptions = GoogleUpstream.ProviderOptions(providerOptions);
        return options;
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"application\":\"example\",\"safety_identifier\":\"opaque_user_123\"}")]
    [UpstreamTest(Prefix + "forwards labels %j on streaming requests", Coverage = UpstreamCoverage.Covered, Commit = "a210a29f9b1be06d87e9e880840419defecbee2b")]
    public async Task Forwards_labels_on_streaming_requests(string labels)
    {
        var handler = GoogleInteractionsStreamSupport.FixtureHandler("basic");

        await GoogleInteractionsStreamSupport.Stream(GoogleInteractionsStreamSupport.Model(handler), Options("{\"google\":{\"labels\":" + labels + ",\"store\":false}}"));

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(labels), body["labels"]));
        Assert.False((bool)body["store"]!);
        Assert.True((bool)body["stream"]!);
    }

    [Theory]
    [InlineData("undefined")]
    [InlineData("null")]
    [UpstreamTest(Prefix + "omits labels set to %j on streaming requests", Coverage = UpstreamCoverage.Covered, Commit = "a210a29f9b1be06d87e9e880840419defecbee2b")]
    public async Task Omits_labels_set_to_on_streaming_requests(string labels)
    {
        var handler = GoogleInteractionsStreamSupport.FixtureHandler("basic");
        var providerOptions = labels == "null" ? "{\"google\":{\"labels\":null}}" : "{\"google\":{}}";

        await GoogleInteractionsStreamSupport.Stream(GoogleInteractionsStreamSupport.Model(handler), Options(providerOptions));

        Assert.False(JsonNode.Parse(handler.Bodies[0])!.AsObject().ContainsKey("labels"));
    }
}
