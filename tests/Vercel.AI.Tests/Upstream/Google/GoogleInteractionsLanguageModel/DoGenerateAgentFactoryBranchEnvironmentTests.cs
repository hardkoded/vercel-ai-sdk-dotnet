// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt; <c>agent factory branch</c> &gt; <c>environment</c>.
/// </summary>
public sealed class DoGenerateAgentFactoryBranchEnvironmentTests
{
    private const string Prefix =
        "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doGenerate > agent factory branch > environment::";

    private static (SequencedHandler Handler, ILanguageModel Model) Agent()
    {
        var handler = new SequencedHandler().Json(
            GoogleInteractionsStreamSupport.JsonFixture("basic")
        );
        var model = GoogleProvider
            .Create(
                new GoogleOptions { ApiKey = "test-api-key", GenerateId = () => "test-id" },
                handler
            )
            .Interactions(new GoogleInteractionsAgent("deep-research-pro-preview-12-2025"));
        return (handler, model);
    }

    private static LanguageModelCallOptions Options(string environment)
    {
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        options.ProviderOptions = GoogleUpstream.ProviderOptions(
            "{\"google\":{\"environment\":" + environment + "}}"
        );
        return options;
    }

    private static async Task<JsonNode?> EnvironmentOf(string environment)
    {
        var (handler, model) = Agent();
        await model.DoGenerateAsync(Options(environment), CancellationToken.None);
        return JsonNode.Parse(handler.Bodies[0])!["environment"];
    }

    private static void AssertJson(string expected, JsonNode? actual)
    {
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual?.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes the literal \"remote\" string through verbatim",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Passes_the_literal_remote_string_through_verbatim()
    {
        Assert.Equal("remote", (string?)await EnvironmentOf("\"remote\""));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes an env_id string through verbatim",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Passes_an_env_id_string_through_verbatim()
    {
        Assert.Equal("env_abc123", (string?)await EnvironmentOf("\"env_abc123\""));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "serializes the object form with sources of all three types",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Serializes_the_object_form_with_sources_of_all_three_types()
    {
        AssertJson(
            "{\"type\":\"remote\",\"sources\":[{\"type\":\"inline\",\"content\":\"note contents\",\"target\":\"/data/note.txt\"},{\"type\":\"gcs\",\"source\":\"gs://example/path\",\"target\":\"/data/\"},{\"type\":\"repository\",\"source\":\"github://octocat/Hello-World\",\"target\":\"/repo/\"}]}",
            await EnvironmentOf(
                "{\"type\":\"remote\",\"sources\":[{\"type\":\"inline\",\"content\":\"note contents\",\"target\":\"/data/note.txt\"},{\"type\":\"gcs\",\"source\":\"gs://example/path\",\"target\":\"/data/\"},{\"type\":\"repository\",\"source\":\"github://octocat/Hello-World\",\"target\":\"/repo/\"}]}"
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "omits null target on non-inline sources",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Omits_null_target_on_non_inline_sources()
    {
        AssertJson(
            "{\"type\":\"remote\",\"sources\":[{\"type\":\"gcs\",\"source\":\"gs://example/path\"},{\"type\":\"repository\",\"source\":\"github://octocat/Hello-World\"}]}",
            await EnvironmentOf(
                "{\"type\":\"remote\",\"sources\":[{\"type\":\"gcs\",\"source\":\"gs://example/path\"},{\"type\":\"repository\",\"source\":\"github://octocat/Hello-World\"}]}"
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "serializes a network allowlist with header transforms",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Serializes_a_network_allowlist_with_header_transforms()
    {
        const string environment =
            "{\"type\":\"remote\",\"network\":{\"allowlist\":[{\"domain\":\"generativelanguage.googleapis.com\",\"transform\":[{\"x-goog-api-key\":\"AIza-redacted\"}]},{\"domain\":\"*\"}]}}";

        AssertJson(environment, await EnvironmentOf(environment));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "serializes network \"disabled\" verbatim",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Serializes_network_disabled_verbatim()
    {
        AssertJson(
            "{\"type\":\"remote\",\"network\":\"disabled\"}",
            await EnvironmentOf("{\"type\":\"remote\",\"network\":\"disabled\"}")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "emits a warning and drops environment on model-id calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_a_warning_and_drops_environment_on_model_id_calls()
    {
        var handler = new SequencedHandler().Json(
            GoogleInteractionsStreamSupport.JsonFixture("basic")
        );

        var result = await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(Options("\"remote\""), CancellationToken.None);

        Assert.False(JsonNode.Parse(handler.Bodies[0])!.AsObject().ContainsKey("environment"));
        Assert.Contains(result.Warnings, warning =>
            warning.Type == "other" && warning.Message.Contains("environment")
        );
    }
}
