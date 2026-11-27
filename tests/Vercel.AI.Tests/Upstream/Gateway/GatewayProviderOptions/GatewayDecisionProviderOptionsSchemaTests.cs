// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderOptionsTests;

/// <summary>Port of <c>gateway-provider-options.test.ts</c> &gt; <c>gatewayDecisionProviderOptionsSchema</c>.</summary>
public sealed class GatewayDecisionProviderOptionsSchemaTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-provider-options.test.ts::gatewayDecisionProviderOptionsSchema::";
    private const string ConditionalFallback =
        "{\"model\":\"openai/gpt-5.6-sol\",\"when\":{\"question\":\"intent\",\"confidenceBelow\":0.6}}";

    private static GatewayDecisionProviderOptionsResult SafeValidateTypes(string value)
    {
        return GatewayDecisionProviderOptions.SafeValidateTypes(
            JsonDocument.Parse(value).RootElement
        );
    }

    private static GatewayDecisionProviderOptionsResult ValidateWhen(string when)
    {
        return SafeValidateTypes(
            "{\"models\":[{\"model\":\"openai/gpt-5.6-sol\",\"when\":" + when + "}]}"
        );
    }

    private static string Conditions(int count)
    {
        return "["
            + string.Join(
                ",",
                Enumerable
                    .Range(0, count)
                    .Select(index =>
                        "{\"question\":\"question" + index + "\",\"confidenceBelow\":0.6}"
                    )
            )
            + "]";
    }

    [Theory]
    [InlineData("{\"question\":\"intent\",\"confidenceBelow\":0.6}")]
    [InlineData("{\"question\":\"refunded\",\"probabilityBetween\":[0.4,0.6]}")]
    [InlineData(
        "{\"any\":[{\"question\":\"intent\",\"confidenceBelow\":0.6},{\"question\":\"refunded\",\"probabilityBetween\":[0.4,0.6]}]}"
    )]
    [InlineData(
        "{\"all\":[{\"question\":\"intent\",\"confidenceBelow\":0.6},{\"question\":\"severity\",\"confidenceBelow\":0.7}]}"
    )]
    [InlineData(
        "{\"atLeast\":{\"count\":2,\"conditions\":[{\"question\":\"intent\",\"confidenceBelow\":0.6},{\"question\":\"refunded\",\"probabilityBetween\":[0.4,0.6]},{\"question\":\"severity\",\"confidenceBelow\":0.7}]}}"
    )]
    [InlineData("{\"confidenceBelow\":0.6}")]
    [InlineData("{\"probabilityBetween\":[0.4,0.6]}")]
    [InlineData(
        "{\"any\":[{\"confidenceBelow\":0.6},{\"question\":\"intent\",\"confidenceBelow\":0.8},{\"probabilityBetween\":[0.4,0.6]}]}"
    )]
    [UpstreamTest(Prefix + "accepts condition %#", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_condition(string when)
    {
        Assert.True(ValidateWhen(when).Success);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps existing string fallback lists and service-owned options valid",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Keeps_existing_string_fallback_lists_and_service_owned_options_valid()
    {
        var value = JsonDocument
            .Parse(
                "{\"models\":[\"openai/gpt-5.6-sol\",\"anthropic/claude-sonnet-5\"],\"serviceOwnedOption\":true}"
            )
            .RootElement;

        var result = GatewayDecisionProviderOptions.SafeValidateTypes(value);

        Assert.True(result.Success);
        Assert.Equal(value.GetRawText(), result.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "accepts one conditional entry followed by string error fallbacks",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Accepts_one_conditional_entry_followed_by_string_error_fallbacks()
    {
        var result = SafeValidateTypes(
            "{\"models\":[" + ConditionalFallback + ",\"anthropic/claude-sonnet-5\"]}"
        );

        Assert.True(result.Success);
    }

    // The upstream cases with Number.NaN and Number.POSITIVE_INFINITY cannot be written in JSON, so a JsonElement
    // never carries them.
    [Theory]
    [InlineData("{\"question\":\"\",\"confidenceBelow\":0.5}")]
    [InlineData("{\"question\":\"intent\",\"confidenceBelow\":-0.1}")]
    [InlineData("{\"question\":\"intent\",\"confidenceBelow\":1.1}")]
    [InlineData("{\"question\":\"refunded\",\"probabilityBetween\":[0.7,0.3]}")]
    [InlineData("{\"question\":\"refunded\",\"probabilityBetween\":[0,1,2]}")]
    [InlineData("{\"any\":[]}")]
    [InlineData("{\"all\":[]}")]
    [InlineData(
        "{\"atLeast\":{\"count\":0,\"conditions\":[{\"question\":\"intent\",\"confidenceBelow\":0.5}]}}"
    )]
    [InlineData(
        "{\"atLeast\":{\"count\":1.5,\"conditions\":[{\"question\":\"intent\",\"confidenceBelow\":0.5}]}}"
    )]
    [InlineData(
        "{\"atLeast\":{\"count\":2,\"conditions\":[{\"question\":\"intent\",\"confidenceBelow\":0.5}]}}"
    )]
    [InlineData(
        "{\"question\":\"intent\",\"confidenceBelow\":0.5,\"probabilityBetween\":[0.4,0.6]}"
    )]
    [InlineData(
        "{\"question\":\"intent\",\"confidenceBelow\":0.5,\"any\":[{\"question\":\"severity\",\"confidenceBelow\":0.7}]}"
    )]
    [InlineData("{\"question\":\"intent\",\"confidenceBelow\":0.5,\"extra\":true}")]
    [InlineData("{}")]
    [InlineData("{\"question\":\"intent\"}")]
    [InlineData("{\"confidenceBelow\":0.5,\"probabilityBetween\":[0.4,0.6]}")]
    [InlineData("{\"confidenceBelow\":0.5,\"extra\":true}")]
    [UpstreamTest(
        Prefix + "rejects condition %#",
        Coverage = UpstreamCoverage.Partial,
        Note = "The upstream cases with Number.NaN and Number.POSITIVE_INFINITY are not ported: JSON has no such numbers."
    )]
    public void Rejects_condition(string when)
    {
        Assert.False(ValidateWhen(when).Success);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "enforces the maximum condition depth",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Enforces_the_maximum_condition_depth()
    {
        var condition = "{\"question\":\"intent\",\"confidenceBelow\":0.5}";
        for (
            var depth = 1;
            depth < GatewayDecisionProviderOptions.DecisionFallbackMaxConditionDepth;
            depth++
        )
        {
            condition = "{\"any\":[" + condition + "]}";
        }

        var validResult = ValidateWhen(condition);
        var invalidResult = ValidateWhen("{\"any\":[" + condition + "]}");

        Assert.True(validResult.Success);
        Assert.False(invalidResult.Success);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "accepts five condition levels and rejects six",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Accepts_five_condition_levels_and_rejects_six()
    {
        const string direct = "{\"question\":\"intent\",\"confidenceBelow\":0.5}";
        var fiveLevels =
            "{\"any\":[{\"all\":[{\"atLeast\":{\"count\":1,\"conditions\":[{\"any\":["
            + direct
            + "]}]}}]}]}";
        var sixLevels = "{\"any\":[" + fiveLevels + "]}";

        var validResult = ValidateWhen(fiveLevels);
        var invalidResult = ValidateWhen(sixLevels);

        Assert.True(validResult.Success);
        Assert.False(invalidResult.Success);
        Assert.Contains(
            "conditions can be nested at most "
                + GatewayDecisionProviderOptions.DecisionFallbackMaxConditionDepth
                + " levels deep",
            invalidResult.Error
        );
    }

    [Theory]
    [InlineData(
        "{\"models\":[{\"model\":\"\",\"when\":{\"question\":\"intent\",\"confidenceBelow\":0.6}}]}"
    )]
    [InlineData("{\"models\":[" + ConditionalFallback + "," + ConditionalFallback + "]}")]
    [InlineData("{\"models\":[\"anthropic/claude-sonnet-5\"," + ConditionalFallback + "]}")]
    [UpstreamTest(Prefix + "rejects invalid models list %#", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_invalid_models_list(string value)
    {
        Assert.False(SafeValidateTypes(value).Success);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "applies the shared condition bounds",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Applies_the_shared_condition_bounds()
    {
        Assert.Equal(5, GatewayDecisionProviderOptions.DecisionFallbackMaxConditionDepth);
        Assert.Equal(20, GatewayDecisionProviderOptions.DecisionFallbackMaxConditionsPerList);
        Assert.Equal(256, GatewayDecisionProviderOptions.DecisionFallbackMaxQuestionLength);
    }

    [Theory]
    [InlineData(256, true)]
    [InlineData(257, false)]
    [UpstreamTest(
        Prefix + "bounds question length at $length",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Bounds_question_length(int length, bool success)
    {
        var result = ValidateWhen(
            "{\"question\":\"" + new string('q', length) + "\",\"confidenceBelow\":0.6}"
        );

        Assert.Equal(success, result.Success);
    }

    [Theory]
    [InlineData(1000, true)]
    [InlineData(0, false)]
    [UpstreamTest(
        Prefix + "accepts any non-empty conditional model length ($length)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Accepts_any_non_empty_conditional_model_length(int length, bool success)
    {
        var result = SafeValidateTypes(
            "{\"models\":[{\"model\":\""
                + new string('m', length)
                + "\",\"when\":{\"question\":\"intent\",\"confidenceBelow\":0.6}}]}"
        );

        Assert.Equal(success, result.Success);
    }

    [Theory]
    [InlineData("any")]
    [InlineData("all")]
    [InlineData("atLeast")]
    [UpstreamTest(Prefix + "bounds $name condition lists", Coverage = UpstreamCoverage.Covered)]
    public void Bounds_name_condition_lists(string name)
    {
        string When(int count) =>
            name == "atLeast"
                ? "{\"atLeast\":{\"count\":1,\"conditions\":" + Conditions(count) + "}}"
                : "{\"" + name + "\":" + Conditions(count) + "}";

        Assert.True(
            ValidateWhen(
                When(GatewayDecisionProviderOptions.DecisionFallbackMaxConditionsPerList)
            ).Success
        );
        Assert.False(
            ValidateWhen(
                When(GatewayDecisionProviderOptions.DecisionFallbackMaxConditionsPerList + 1)
            ).Success
        );
    }
}
