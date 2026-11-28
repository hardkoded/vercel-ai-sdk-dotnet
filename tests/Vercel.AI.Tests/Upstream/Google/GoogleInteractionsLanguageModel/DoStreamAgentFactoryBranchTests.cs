// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doStream</c> &gt; <c>agent factory branch</c>.
/// </summary>
public sealed class DoStreamAgentFactoryBranchTests
{
    private const string Prefix =
        "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doStream > agent factory branch::";
    private const string AgentName = "deep-research-pro-preview-12-2025";

    private static SequencedHandler Basic()
    {
        return new SequencedHandler().Json(GoogleInteractionsStreamSupport.JsonFixture("basic"));
    }

    private static ILanguageModel Agent(SequencedHandler handler)
    {
        return GoogleProvider
            .Create(
                new GoogleOptions { ApiKey = "test-api-key", GenerateId = () => "test-id" },
                handler
            )
            .Interactions(new GoogleInteractionsAgent(AgentName));
    }

    private static LanguageModelCallOptions Options()
    {
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        options.IncludeRawChunks = false;
        options.ProviderOptions = GoogleUpstream.ProviderOptions(
            "{\"google\":{\"background\":true}}"
        );
        return options;
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "puts `agent` (not `model`) and `background:true` (not `stream:true`) in the request body when streaming with background:true",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Puts_agent_not_model_and_background_true_not_stream_true_in_the_request_body_when_streaming_with_background_true()
    {
        var handler = Basic();

        await GoogleInteractionsStreamSupport.Stream(Agent(handler), Options());

        var body = JsonNode.Parse(handler.Bodies[0])!.AsObject();
        Assert.Equal(AgentName, (string?)body["agent"]);
        Assert.Null(body["model"]);
        Assert.True((bool?)body["background"]);
        Assert.Null(body["stream"]);
        Assert.Null(body["generation_config"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "emits warning and drops generation-config fields on streaming agent calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_warning_and_drops_generation_config_fields_on_streaming_agent_calls()
    {
        var options = Options();
        options.Temperature = 0.5;

        var parts = await GoogleInteractionsStreamSupport.Stream(Agent(Basic()), options);

        var streamStart = Assert.IsType<StreamStartStreamPart>(
            parts.First(part => part is StreamStartStreamPart)
        );
        Assert.Contains(streamStart.Warnings, warning =>
            warning.Type == "other" && warning.Message.Contains("temperature")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "synthesizes text-start, text-delta, text-end, finish parts from a terminal-status agent response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Synthesizes_text_start_text_delta_text_end_finish_parts_from_a_terminal_status_agent_response()
    {
        var parts = await GoogleInteractionsStreamSupport.Stream(Agent(Basic()), Options());

        var types = parts.Select(part => part.Type).ToList();
        Assert.Contains("stream-start", types);
        Assert.Contains("text-start", types);
        Assert.Contains("text-delta", types);
        Assert.Contains("text-end", types);
        Assert.Contains("finish", types);
        var finish = Assert.IsType<FinishStreamPart>(parts.First(part => part is FinishStreamPart));
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
    }
}
