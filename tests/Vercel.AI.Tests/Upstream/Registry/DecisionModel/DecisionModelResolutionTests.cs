// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using DecideOperation = Vercel.AI.Operations.Decide;

namespace Vercel.AI.Tests.Upstream.Registry.DecisionModel;

/// <summary>Port of <c>decision-model.test.ts</c> <c>describe('decision model resolution')</c>.</summary>
[Collection("DownloadFetch")]
public sealed class DecisionModelResolutionTests
{
    private const string Prefix = "packages/ai/src/registry/decision-model.test.ts::decision model resolution::";

    private readonly DecisionMockModel _model = new DecisionMockModel();

    [Fact]
    [UpstreamTest(Prefix + "returns model instances unchanged", Coverage = UpstreamCoverage.Covered)]
    public void Returns_model_instances_unchanged()
    {
        Assert.Same(_model, DecisionModels.Validate(_model));
    }

    [Fact]
    [UpstreamTest(Prefix + "validates versions returned by model instances, aliases, registries, and defaults", Coverage = UpstreamCoverage.Partial, Note = "Checks model instances, aliases, and registries. The port has no default provider for decision models. Issue #389.")]
    public void Validates_versions_returned_by_model_instances_aliases_registries_and_defaults()
    {
        var invalid = new DecisionMockModel { SpecificationVersion = "v3" };
        var provider = new CustomProvider().AddDecisionModel("invalid", invalid);
        var registry = new ProviderRegistry();
        registry.Register("provider", provider);
        Assert.Throws<UnsupportedModelVersionException>(() => DecisionModels.Validate(invalid));
        Assert.Throws<UnsupportedModelVersionException>(() => provider.DecisionModel("invalid"));
        Assert.Throws<UnsupportedModelVersionException>(() => registry.DecisionModel("provider:invalid"));
    }

    [Fact]
    [UpstreamTest(Prefix + "decides with %s model resolution and forwards options in one call", Coverage = UpstreamCoverage.Partial, Note = "Checks the instance case. String model ids are not ported. Issue #389.")]
    public async Task Decides_with_instance_model_resolution_and_forwards_options_in_one_call()
    {
        var model = new DecisionMockModel
        {
            OnDecide = _ => new DecisionModelResult(
                new Dictionary<string, DecisionAnswer> { ["route"] = new DecisionAnswer("choice", choice: "billing") },
                usage: new OperationUsage(10, 2)),
        };
        var provider = new CustomProvider().AddDecisionModel("route", model);
        var registry = new ProviderRegistry();
        registry.Register("provider", provider);
        var questions = new Dictionary<string, DecisionQuestion>
        {
            ["route"] = new DecisionQuestion("choice", "Team?", new Dictionary<string, object?> { ["billing"] = "Charges", ["support"] = "Other" }),
        };
        using var source = new CancellationTokenSource();
        var result = await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = registry.DecisionModel("provider:route"),
            State = new Dictionary<string, object?> { ["message"] = "Charged twice" },
            Questions = questions,
            Headers = new Dictionary<string, string> { ["x-test"] = "header" },
            ProviderOptions = OperationJson.Parse("{\"test\":{\"flag\":true}}"),
            CancellationToken = source.Token,
        });
        Assert.Equal("billing", result.Answers["route"].Choice);
        Assert.Equal((int?)12, result.Usage.TotalTokens);
        var call = Assert.Single(model.Calls);
        Assert.Same(questions, call.Questions);
        Assert.Equal(source.Token, call.CancellationToken);
        Assert.Equal("header", call.Headers["x-test"]);
        Assert.Equal("{\"test\":{\"flag\":true}}", call.ProviderOptions.GetRawText());
    }
}
