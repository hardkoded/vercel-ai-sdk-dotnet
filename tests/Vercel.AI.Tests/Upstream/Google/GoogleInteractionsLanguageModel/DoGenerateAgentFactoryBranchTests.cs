// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt; <c>agent factory branch</c>.
/// The nested <c>environment</c> describe is <see cref="DoGenerateAgentFactoryBranchEnvironmentTests"/>.
/// </summary>
public sealed class DoGenerateAgentFactoryBranchTests
{
    private const string Prefix = "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doGenerate > agent factory branch::";
    private const string AgentName = "deep-research-pro-preview-12-2025";

    private static ILanguageModel Agent(SequencedHandler handler)
    {
        return GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key", GenerateId = () => "test-id" }, handler).Interactions(new GoogleInteractionsAgent(AgentName));
    }

    private static SequencedHandler Basic()
    {
        return new SequencedHandler().Json(GoogleInteractionsStreamSupport.JsonFixture("basic"));
    }

    private static LanguageModelCallOptions Options(string? providerOptions = null)
    {
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        if (providerOptions != null)
        {
            options.ProviderOptions = GoogleUpstream.ProviderOptions(providerOptions);
        }

        return options;
    }

    [Fact]
    [UpstreamTest(Prefix + "puts `agent` (not `model`) in the request body when constructed via the agent factory branch", Coverage = UpstreamCoverage.Covered)]
    public async Task Puts_agent_not_model_in_the_request_body_when_constructed_via_the_agent_factory_branch()
    {
        var handler = Basic();

        await Agent(handler).DoGenerateAsync(Options(), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.Equal(AgentName, (string?)body["agent"]);
        Assert.False(body.ContainsKey("model"));
    }

    [Fact]
    [UpstreamTest(Prefix + "puts `model` (not `agent`) in the request body when constructed via a model id", Coverage = UpstreamCoverage.Covered)]
    public async Task Puts_model_not_agent_in_the_request_body_when_constructed_via_a_model_id()
    {
        var handler = Basic();

        await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options(), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.Equal("gemini-2.5-flash", (string?)body["model"]);
        Assert.False(body.ContainsKey("agent"));
    }

    [Fact]
    [UpstreamTest(Prefix + "omits generation_config from the request body when an agent is set", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_generation_config_from_the_request_body_when_an_agent_is_set()
    {
        var handler = Basic();

        await Agent(handler).DoGenerateAsync(Options(), CancellationToken.None);

        Assert.False(JsonNode.Parse(handler.Bodies[0])!.AsObject().ContainsKey("generation_config"));
    }

    [Fact]
    [UpstreamTest(Prefix + "emits agent_config with type \"deep-research\" and the renamed snake_case fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_agent_config_with_type_deep_research_and_the_renamed_snake_case_fields()
    {
        var handler = Basic();

        await Agent(handler).DoGenerateAsync(Options("{\"google\":{\"agentConfig\":{\"type\":\"deep-research\",\"thinkingSummaries\":\"auto\",\"visualization\":\"auto\",\"collaborativePlanning\":true}}}"), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"deep-research\",\"thinking_summaries\":\"auto\",\"visualization\":\"auto\",\"collaborative_planning\":true}"), body["agent_config"]));
    }

    [Fact]
    [UpstreamTest(Prefix + "emits agent_config with type \"dynamic\" verbatim", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_agent_config_with_type_dynamic_verbatim()
    {
        var handler = Basic();

        await Agent(handler).DoGenerateAsync(Options("{\"google\":{\"agentConfig\":{\"type\":\"dynamic\"}}}"), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"dynamic\"}"), body["agent_config"]));
    }

    [Fact]
    [UpstreamTest(Prefix + "emits file_search tools when an agent is set", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_file_search_tools_when_an_agent_is_set()
    {
        var handler = Basic();
        var options = Options("{\"google\":{\"background\":true}}");
        options.Tools = [LanguageModelTool.ProviderDefined("google.file_search", "file_search", JsonDocument.Parse("{\"fileSearchStoreNames\":[\"fileSearchStores/x\"]}").RootElement.Clone())];

        var result = await Agent(handler).DoGenerateAsync(options, CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("[{\"type\":\"file_search\",\"file_search_store_names\":[\"fileSearchStores/x\"]}]"), body["tools"]));
        Assert.True((bool?)body["background"]);
        Assert.DoesNotContain(result.Warnings, warning => warning.Type == "other" && warning.Message.Contains("tools"));
    }

    [Fact]
    [UpstreamTest(Prefix + "emits a warning listing every dropped generation-config field when an agent is set", Coverage = UpstreamCoverage.Covered, Commit = "8e60cb4ba35426e7f4a21e341a5e16f37a478a5b")]
    public async Task Emits_a_warning_listing_every_dropped_generation_config_field_when_an_agent_is_set()
    {
        var handler = Basic();
        var options = Options("{\"google\":{\"thinkingLevel\":\"high\"}}");
        options.Temperature = 0.5;
        options.TopP = 0.9;
        options.TopK = 10;
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.5;
        options.Reasoning = "high";

        var result = await Agent(handler).DoGenerateAsync(options, CancellationToken.None);

        Assert.False(JsonNode.Parse(handler.Bodies[0])!.AsObject().ContainsKey("generation_config"));
        Assert.Contains(result.Warnings, warning => warning.Type == "other"
            && new[] { "temperature", "topP", "topK", "frequencyPenalty", "presencePenalty", "reasoning", "thinkingLevel" }.All(warning.Message.Contains));
    }

    [Fact]
    [UpstreamTest(Prefix + "does not emit a generation-config warning when no generation-config fields are set on an agent call", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_emit_a_generation_config_warning_when_no_generation_config_fields_are_set_on_an_agent_call()
    {
        var handler = Basic();

        var result = await Agent(handler).DoGenerateAsync(Options(), CancellationToken.None);

        Assert.DoesNotContain(result.Warnings, warning => warning.Type == "other" && warning.Message.Contains("use providerOptions.google.agentConfig instead"));
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves previousInteractionId on agent calls (multi-turn agent flow)", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_previousInteractionId_on_agent_calls_multi_turn_agent_flow()
    {
        var handler = Basic();

        await Agent(handler).DoGenerateAsync(Options("{\"google\":{\"previousInteractionId\":\"v1_prior-agent-turn\"}}"), CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.Equal("v1_prior-agent-turn", (string?)body["previous_interaction_id"]);
        Assert.Equal(AgentName, (string?)body["agent"]);
        Assert.False(body.ContainsKey("model"));
    }

    [Fact]
    [UpstreamTest(Prefix + "passes background through from providerOptions.google.background", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_background_through_from_providerOptions_google_background()
    {
        var handler = Basic();

        await Agent(handler).DoGenerateAsync(Options("{\"google\":{\"background\":true}}"), CancellationToken.None);

        Assert.True((bool?)JsonNode.Parse(handler.Bodies[0])!["background"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "omits background by default (no auto-injection on agent calls)", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_background_by_default_no_auto_injection_on_agent_calls()
    {
        var handler = new SequencedHandler().Json(GoogleInteractionsStreamSupport.JsonFixture("basic")).Json(GoogleInteractionsStreamSupport.JsonFixture("basic"));

        await Agent(handler).DoGenerateAsync(Options(), CancellationToken.None);
        await GoogleInteractionsStreamSupport.Model(handler).DoGenerateAsync(Options(), CancellationToken.None);

        Assert.False(JsonNode.Parse(handler.Bodies[0])!.AsObject().ContainsKey("background"));
        Assert.False(JsonNode.Parse(handler.Bodies[1])!.AsObject().ContainsKey("background"));
    }
}
