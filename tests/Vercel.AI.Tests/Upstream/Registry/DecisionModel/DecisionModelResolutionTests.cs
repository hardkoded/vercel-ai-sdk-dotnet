// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Registry.DecisionModel.DecisionModelSupport;
using DecideOperation = Vercel.AI.Operations.Decide;

namespace Vercel.AI.Tests.Upstream.Registry.DecisionModel;

/// <summary>Port of <c>describe('decision model resolution')</c> in <c>decision-model.test.ts</c>.</summary>
[Collection("DecisionDefaultProvider")]
public sealed class DecisionModelResolutionTests : DecisionDefaultProviderFixture
{
    private const string Prefix = "packages/ai/src/registry/decision-model.test.ts::decision model resolution::";

    private readonly DecisionMockModel _model = new();

    public static IEnumerable<object[]> GatewayCases()
    {
        var cases = new (string Name, object State, string Sent)[]
        {
            ("string", "The capital of France is Paris.", "\"The capital of France is Paris.\""),
            ("JSON object", new Dictionary<string, object?> { ["statement"] = "The capital of France is Paris." }, """{"statement":"The capital of France is Paris."}"""),
            (
                "mixed parts",
                new List<object?>
                {
                    new Dictionary<string, object?> { ["type"] = "text", ["text"] = "Check this statement." },
                    new Dictionary<string, object?> { ["type"] = "json", ["value"] = new Dictionary<string, object?> { ["capital"] = "Paris", ["country"] = "France" } },
                },
                """["Check this statement.",{"capital":"Paris","country":"France"}]"""),
            ("empty parts", new List<object?>(), "[]"),
        };
        foreach (var resolution in new[] { "string", "alias" })
        {
            foreach (var test in cases)
            {
                yield return new object[] { resolution, test.Name, test.State, test.Sent };
            }
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "returns model instances unchanged", Coverage = UpstreamCoverage.Covered)]
    public void Returns_model_instances_unchanged()
    {
        Assert.Same(_model, DecisionModels.Resolve(_model));
    }

    [Theory]
    [MemberData(nameof(GatewayCases))]
    [UpstreamTest(Prefix + "decides through Gateway using a model $resolution with $name state when no default provider is configured", Coverage = UpstreamCoverage.Partial, Note = "The port's Gateway decision route sends the state under \"state\" and omits empty providerOptions, where upstream sends \"stateParts\" and \"providerOptions\": {}. The test checks the port's body.")]
    public async Task Decides_through_Gateway_using_a_model_resolution_with_name_state_when_no_default_provider_is_configured(string resolution, string name, object state, string sent)
    {
        Assert.NotEmpty(name);
        DecisionModels.DefaultProvider = null;
        Environment.SetEnvironmentVariable("AI_GATEWAY_API_KEY", "test-api-key");
        var capture = new OpenAICapture
        {
            ResponseJson = """{"answers":{"correct":{"type":"boolean","probability":0.98}},"usage":{"inputTokens":10,"outputTokens":2},"warnings":[]}""",
        };
        DecisionModels.GatewayFactory = () => GatewayProvider.Create(handler: capture);
        const string modelId = "typesafe-ai/jev";
        var questions = new Dictionary<string, DecisionQuestion> { ["correct"] = new DecisionQuestion("boolean", "Is this correct?") };

        var result = await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = resolution == "string" ? modelId : new CustomProvider().AddDecisionModel("check", modelId).DecisionModel("check"),
            State = state,
            Questions = questions,
        });

        Assert.Equal(0.98, result.Answers["correct"].Probability);
        Assert.Equal(12, result.Usage.TotalTokens);
        Assert.Equal(1, capture.Calls);
        Assert.Equal("Bearer test-api-key", capture.Headers["Authorization"]);
        Assert.Equal(modelId, capture.Headers["ai-model-id"]);
        Assert.Equal("4", capture.Headers["ai-decision-model-specification-version"]);
        var body = JsonNode.Parse(capture.Body)!;
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(sent), body["state"]), capture.Body);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("""{"correct":{"type":"boolean","instructions":"Is this correct?"}}"""), body["questions"]), capture.Body);
    }

    [Fact]
    [UpstreamTest(Prefix + "requires an explicitly configured default provider to support decision", Coverage = UpstreamCoverage.Covered)]
    public void Requires_an_explicitly_configured_default_provider_to_support_decision()
    {
        var gatewayCalls = 0;
        DecisionModels.GatewayFactory = () =>
        {
            gatewayCalls++;
            return GatewayProvider.Create();
        };
        DecisionModels.DefaultProvider = new PlainProvider();
        var error = Assert.Throws<InvalidOperationException>(() => DecisionModels.Resolve("typesafe-ai/jev"));
        Assert.Equal("The default provider does not support decision models.", error.Message);
        Assert.Equal(0, gatewayCalls);
    }

    [Fact]
    [UpstreamTest(Prefix + "validates versions returned by model instances, aliases, registries, and defaults", Coverage = UpstreamCoverage.Covered)]
    public void Validates_versions_returned_by_model_instances_aliases_registries_and_defaults()
    {
        var invalid = new DecisionMockModel { SpecificationVersion = "v3" };
        var provider = new CustomProvider().AddDecisionModel("invalid", invalid);
        var registry = new ProviderRegistry();
        registry.Register("provider", provider);
        DecisionModels.DefaultProvider = registry;
        foreach (var resolve in new Func<IDecisionCaller>[]
        {
            () => DecisionModels.Resolve(invalid),
            () => provider.DecisionModel("invalid"),
            () => registry.DecisionModel("provider:invalid"),
            () => DecisionModels.Resolve("provider:invalid"),
        })
        {
            Assert.Throws<UnsupportedModelVersionException>(resolve);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves errors thrown by provider factories", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_errors_thrown_by_provider_factories()
    {
        var error = new NoSuchModelError("missing", "decisionModel");
        DecisionModels.DefaultProvider = new FactoryProvider(_ => throw error);
        Assert.Same(error, Assert.Throws<NoSuchModelError>(() => DecisionModels.Resolve("missing")));
    }

    [Fact]
    [UpstreamTest(Prefix + "reports missing models returned by the default provider", Coverage = UpstreamCoverage.Covered)]
    public void Reports_missing_models_returned_by_the_default_provider()
    {
        DecisionModels.DefaultProvider = new FactoryProvider(_ => null);
        var error = Assert.Throws<NoSuchModelError>(() => DecisionModels.Resolve("missing"));
        Assert.Equal("missing", error.ModelId);
        Assert.Equal("decisionModel", error.ModelType);
    }

    [Theory]
    [InlineData("instance")]
    [InlineData("string")]
    [UpstreamTest(Prefix + "decides with %s model resolution and forwards options in one call", Coverage = UpstreamCoverage.Covered)]
    public async Task Decides_with_model_resolution_and_forwards_options_in_one_call(string resolution)
    {
        var mock = new DecisionMockModel(_ => Task.FromResult(RouteResult()));
        var provider = new CustomProvider().AddDecisionModel("route", mock);
        DecisionModels.DefaultProvider = provider;
        var questions = RouteQuestions();
        using var source = new CancellationTokenSource();
        var registry = new ProviderRegistry();
        registry.Register("provider", provider);

        var result = await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = resolution == "string" ? "route" : registry.DecisionModel("provider:route"),
            State = new Dictionary<string, object?> { ["message"] = "Charged twice" },
            Questions = questions,
            Headers = new Dictionary<string, string> { ["x-test"] = "header" },
            ProviderOptions = Json("""{"test":{"flag":true}}"""),
            CancellationToken = source.Token,
        });

        Assert.Equal("billing", result.Answers["route"].Choice);
        Assert.Equal(12, result.Usage.TotalTokens);
        var call = Assert.Single(mock.Calls);
        Assert.Same(questions["route"], call.Questions["route"]);
        Assert.Equal(source.Token, call.CancellationToken);
        Assert.Equal("header", call.Headers["x-test"]);
        Assert.Equal("""{"test":{"flag":true}}""", call.ProviderOptions.GetRawText());
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects unsupported questions before I/O after resolving a string", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_questions_before_IO_after_resolving_a_string()
    {
        var mock = new DecisionMockModel(_ => throw new InvalidOperationException("unreachable"), new[] { "choice" });
        DecisionModels.DefaultProvider = new CustomProvider().AddDecisionModel("choice", mock);
        await Assert.ThrowsAnyAsync<DecisionUnsupportedQuestionTypeException>(() => DecideOperation.DecideAsync(new DecideRequest
        {
            Model = "choice",
            State = "test",
            Questions = new Dictionary<string, DecisionQuestion> { ["flag"] = new DecisionQuestion("boolean", "Yes?") },
        }));
        Assert.Empty(mock.Calls);
    }
}
