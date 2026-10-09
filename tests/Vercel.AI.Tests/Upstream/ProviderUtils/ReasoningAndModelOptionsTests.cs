// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

public sealed class ReasoningAndModelOptionsTests
{
    private const string Reasoning = "packages/provider-utils/src/map-reasoning-to-provider.test.ts::";

    private const string Serialize = "packages/provider-utils/src/serialize-model-options.test.ts::serializeModelOptions::";

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderEffort::returns mapped value with no warning for direct match", Coverage = UpstreamCoverage.Covered)]
    public void Direct_effort_match_has_no_warning()
    {
        var warnings = new List<ModelWarning>();
        Assert.Equal("medium", ReasoningMap.MapReasoningToProviderEffort("medium", EffortMap(), warnings));
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderEffort::returns mapped value with compatibility warning for renamed match", Coverage = UpstreamCoverage.Covered)]
    public void Renamed_effort_adds_a_compatibility_warning()
    {
        var warnings = new List<ModelWarning>();
        Assert.Equal("low", ReasoningMap.MapReasoningToProviderEffort("minimal", EffortMap(), warnings));
        var warning = Assert.IsType<CompatibilityWarning>(Assert.Single(warnings));
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal("reasoning", warning.Feature);
        Assert.Equal("reasoning \"minimal\" is not directly supported by this model. mapped to effort \"low\".", warning.Details);
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderEffort::returns mapped value with compatibility warning for \"xhigh\"", Coverage = UpstreamCoverage.Covered)]
    public void Xhigh_effort_maps_to_max_with_a_compatibility_warning()
    {
        var warnings = new List<ModelWarning>();
        Assert.Equal("max", ReasoningMap.MapReasoningToProviderEffort("xhigh", EffortMap(), warnings));
        var warning = Assert.IsType<CompatibilityWarning>(Assert.Single(warnings));
        Assert.Equal("reasoning", warning.Feature);
        Assert.Equal("reasoning \"xhigh\" is not directly supported by this model. mapped to effort \"max\".", warning.Details);
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderEffort::returns undefined with unsupported warning for key missing from effortMap", Coverage = UpstreamCoverage.Covered)]
    public void Missing_effort_returns_null_and_an_unsupported_warning()
    {
        var warnings = new List<ModelWarning>();
        var result = ReasoningMap.MapReasoningToProviderEffort(
            "high",
            new Dictionary<string, string> { ["medium"] = "medium" },
            warnings);
        Assert.Null(result);
        var warning = Assert.IsType<UnsupportedWarning>(Assert.Single(warnings));
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("reasoning", warning.Feature);
        Assert.Equal("reasoning \"high\" is not supported by this model.", warning.Details);
    }

    [Fact]
    [UpstreamTest(Reasoning + "isCustomReasoning::returns false for undefined", Coverage = UpstreamCoverage.Covered)]
    public void Omitted_reasoning_is_not_custom()
    {
        Assert.False(ReasoningMap.IsCustomReasoning(null));
    }

    [Fact]
    [UpstreamTest(Reasoning + "isCustomReasoning::returns false for provider-default", Coverage = UpstreamCoverage.Covered)]
    public void Provider_default_reasoning_is_not_custom()
    {
        Assert.False(ReasoningMap.IsCustomReasoning("provider-default"));
    }

    [Fact]
    [UpstreamTest(Reasoning + "isCustomReasoning::returns true for none", Coverage = UpstreamCoverage.Covered)]
    public void None_reasoning_is_custom()
    {
        Assert.True(ReasoningMap.IsCustomReasoning("none"));
    }

    [Fact]
    [UpstreamTest(Reasoning + "isCustomReasoning::returns true for all reasoning levels", Coverage = UpstreamCoverage.Covered)]
    public void Named_reasoning_levels_are_custom()
    {
        foreach (var value in new[] { "minimal", "low", "medium", "high", "xhigh", "max" })
        {
            Assert.True(ReasoningMap.IsCustomReasoning(value));
        }
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderBudget::returns correct budget for known key", Coverage = UpstreamCoverage.Covered)]
    public void Medium_budget_is_thirty_percent()
    {
        var warnings = new List<ModelWarning>();
        Assert.Equal(19200, ReasoningMap.MapReasoningToProviderBudget("medium", 64000, 64000, warnings));
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderBudget::caps result at maxReasoningBudget", Coverage = UpstreamCoverage.Covered)]
    public void Budget_is_capped_at_the_provider_maximum()
    {
        var warnings = new List<ModelWarning>();
        Assert.Equal(50000, ReasoningMap.MapReasoningToProviderBudget("xhigh", 64000, 50000, warnings));
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderBudget::floors result at default minReasoningBudget of 1024", Coverage = UpstreamCoverage.Covered)]
    public void Budget_is_floored_at_1024()
    {
        var warnings = new List<ModelWarning>();
        Assert.Equal(1024, ReasoningMap.MapReasoningToProviderBudget("minimal", 10000, 10000, warnings));
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderBudget::respects custom minReasoningBudget", Coverage = UpstreamCoverage.Covered)]
    public void Custom_minimum_budget_is_used()
    {
        var warnings = new List<ModelWarning>();
        Assert.Equal(512, ReasoningMap.MapReasoningToProviderBudget("minimal", 10000, 10000, warnings, minReasoningBudget: 512));
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderBudget::respects custom budgetPercentages", Coverage = UpstreamCoverage.Covered)]
    public void Custom_budget_percentage_is_used()
    {
        var warnings = new List<ModelWarning>();
        Assert.Equal(
            5000,
            ReasoningMap.MapReasoningToProviderBudget(
                "medium",
                10000,
                10000,
                warnings,
                budgetPercentages: new Dictionary<string, double> { ["medium"] = 0.5 }));
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderBudget::returns undefined with unsupported warning for key missing from custom budgetPercentages", Coverage = UpstreamCoverage.Covered)]
    public void Missing_custom_budget_key_returns_null()
    {
        var warnings = new List<ModelWarning>();
        var result = ReasoningMap.MapReasoningToProviderBudget(
            "high",
            64000,
            64000,
            warnings,
            budgetPercentages: new Dictionary<string, double> { ["medium"] = 0.5 });
        Assert.Null(result);
        var warning = Assert.IsType<UnsupportedWarning>(Assert.Single(warnings));
        Assert.Equal("reasoning \"high\" is not supported by this model.", warning.Details);
    }

    [Fact]
    [UpstreamTest(Serialize + "returns modelId and serializable config", Coverage = UpstreamCoverage.Covered)]
    public void Serializes_model_id_and_drops_non_json_config()
    {
        var result = ModelOptionsSerialization.SerializeModelOptions(
            "claude-sonnet-4-20250514",
            new Dictionary<string, object?>
            {
                ["provider"] = "anthropic.messages",
                ["baseURL"] = "https://api.anthropic.com/v1",
                ["headers"] = new Func<Dictionary<string, string>>(() => new Dictionary<string, string> { ["x-api-key"] = "sk-test" }),
                ["fetch"] = JsUndefined.Value,
                ["generateId"] = new Func<string>(() => "id"),
                ["supportedUrls"] = new Func<Dictionary<string, object?>>(() => new Dictionary<string, object?>()),
                ["supportsNativeStructuredOutput"] = true,
                ["supportsStrictTools"] = false,
            });
        Assert.Equal("claude-sonnet-4-20250514", result.ModelId);
        Assert.Equal(
            new[] { "baseURL", "headers", "provider", "supportsNativeStructuredOutput", "supportsStrictTools" },
            result.Config.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray());
        Assert.Equal("anthropic.messages", result.Config["provider"]);
        Assert.Equal("https://api.anthropic.com/v1", result.Config["baseURL"]);
        Assert.Equal(true, result.Config["supportsNativeStructuredOutput"]);
        Assert.Equal(false, result.Config["supportsStrictTools"]);
        var headers = Assert.IsType<Dictionary<string, object?>>(result.Config["headers"]);
        Assert.Equal("sk-test", headers["x-api-key"]);
    }

    [Fact]
    [UpstreamTest(Serialize + "resolves headers functions but filters out other functions", Coverage = UpstreamCoverage.Covered)]
    public void Header_functions_are_invoked_and_other_functions_are_dropped()
    {
        var result = ModelOptionsSerialization.SerializeModelOptions(
            "gpt-4",
            new Dictionary<string, object?>
            {
                ["provider"] = "openai",
                ["headers"] = new Func<Dictionary<string, string>>(() => new Dictionary<string, string> { ["authorization"] = "Bearer sk-test" }),
                ["url"] = new Func<string>(() => "https://api.openai.com/v1/chat/completions"),
            });
        Assert.Equal(new[] { "headers", "provider" }, result.Config.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray());
        var headers = Assert.IsType<Dictionary<string, object?>>(result.Config["headers"]);
        Assert.Equal("Bearer sk-test", headers["authorization"]);
    }

    [Fact]
    [UpstreamTest(Serialize + "filters out objects containing functions", Coverage = UpstreamCoverage.Covered)]
    public void Objects_that_contain_functions_are_dropped()
    {
        var result = ModelOptionsSerialization.SerializeModelOptions(
            "model",
            new Dictionary<string, object?>
            {
                ["provider"] = "openai-compatible",
                ["errorStructure"] = new Dictionary<string, object?>
                {
                    ["errorSchema"] = new Dictionary<string, object?>(),
                    ["errorToMessage"] = new Func<string>(() => "error"),
                },
                ["metadataExtractor"] = new Dictionary<string, object?>
                {
                    ["extractMetadata"] = new Func<Task>(() => Task.CompletedTask),
                    ["createStreamExtractor"] = new Func<object?>(() => new Dictionary<string, object?>()),
                },
            });
        Assert.Equal(new[] { "provider" }, result.Config.Keys.ToArray());
        Assert.Equal("openai-compatible", result.Config["provider"]);
    }

    [Fact]
    [UpstreamTest(Serialize + "keeps arrays of primitives", Coverage = UpstreamCoverage.Covered)]
    public void Primitive_arrays_are_kept()
    {
        var tags = new[] { "a", "b" };
        var result = ModelOptionsSerialization.SerializeModelOptions(
            "model",
            new Dictionary<string, object?>
            {
                ["provider"] = "test",
                ["tags"] = tags,
                ["fn"] = new Action(() => { }),
            });
        Assert.Same(tags, result.Config["tags"]);
        Assert.False(result.Config.ContainsKey("fn"));
    }

    [Fact]
    [UpstreamTest(Serialize + "throws when headers resolve asynchronously", Coverage = UpstreamCoverage.Covered)]
    public void Async_headers_throw_serialization_error()
    {
        var error = Assert.Throws<SerializationError>(() => ModelOptionsSerialization.SerializeModelOptions(
            "model",
            new Dictionary<string, object?>
            {
                ["provider"] = "test",
                ["headers"] = new Func<Task<Dictionary<string, string>>>(async () =>
                {
                    await Task.Yield();
                    return new Dictionary<string, string> { ["x-api-key"] = "sk-test" };
                }),
            }));
        Assert.Equal("Cannot serialize asynchronous model options.", error.Message);
    }

    [Fact]
    [UpstreamTest(Serialize + "throws when headers is a function returning a Promise", Coverage = UpstreamCoverage.Covered)]
    public void Headers_that_return_a_task_throw()
    {
        var error = Assert.Throws<SerializationError>(() => ModelOptionsSerialization.SerializeModelOptions(
            "model",
            new Dictionary<string, object?>
            {
                ["provider"] = "test",
                ["headers"] = new Func<Task<Dictionary<string, string>>>(() => Task.FromResult(new Dictionary<string, string> { ["x-api-key"] = "sk-test" })),
            }));
        Assert.Equal("Cannot serialize asynchronous model options.", error.Message);
    }

    [Fact]
    [UpstreamTest(Serialize + "marks serialization errors for cross-realm detection", Coverage = UpstreamCoverage.Covered)]
    public void Serialization_errors_are_detectable()
    {
        object? serializationError = null;
        try
        {
            ModelOptionsSerialization.SerializeModelOptions(
                "model",
                new Dictionary<string, object?>
                {
                    ["headers"] = new Func<Task<Dictionary<string, string>>>(() => Task.FromResult(new Dictionary<string, string>())),
                });
        }
        catch (Exception error)
        {
            serializationError = error;
        }

        Assert.True(SerializationError.IsInstance(serializationError));
    }

    [Fact]
    [UpstreamTest(Serialize + "filters out class instances", Coverage = UpstreamCoverage.Covered)]
    public void Class_instances_are_dropped()
    {
        var result = ModelOptionsSerialization.SerializeModelOptions(
            "model",
            new Dictionary<string, object?>
            {
                ["provider"] = "test",
                ["date"] = DateTime.UtcNow,
                ["regex"] = new Regex("test"),
            });
        Assert.Equal(new[] { "provider" }, result.Config.Keys.ToArray());
        Assert.Equal("test", result.Config["provider"]);
    }

    private static Dictionary<string, string> EffortMap()
    {
        return new Dictionary<string, string>
        {
            ["minimal"] = "low",
            ["low"] = "low",
            ["medium"] = "medium",
            ["high"] = "high",
            ["xhigh"] = "max",
        };
    }
}
