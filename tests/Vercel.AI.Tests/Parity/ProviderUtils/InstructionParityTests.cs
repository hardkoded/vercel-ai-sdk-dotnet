// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

public sealed class InstructionParityTests
{
    private const string BasicSchemaJson = "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"age\":{\"type\":\"number\"}},\"required\":[\"name\",\"age\"]}";

    private const string SchemaSuffix = "You MUST answer with a JSON object that matches the JSON schema above.";

    private const string Inject = "packages/provider-utils/src/inject-json-instruction.test.ts::";

    private const string Reasoning = "packages/provider-utils/src/map-reasoning-to-provider.test.ts::";

    private const string Serialize = "packages/provider-utils/src/serialize-model-options.test.ts::serializeModelOptions::";

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstruction::should handle basic case with prompt and schema", Coverage = UpstreamCoverage.Covered)]
    public void Injects_prompt_and_schema()
    {
        var result = JsonInstructions.InjectJsonInstruction(new JsonInstruction
        {
            HasPrompt = true,
            Prompt = "Generate a person",
            HasSchema = true,
            Schema = BasicSchema(),
        });
        Assert.Equal(
            "Generate a person\n\nJSON schema:\n" + BasicSchemaJson + "\n" + SchemaSuffix,
            result);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstruction::should handle only prompt, no schema", Coverage = UpstreamCoverage.Covered)]
    public void Injects_prompt_without_a_schema()
    {
        var result = JsonInstructions.InjectJsonInstruction(new JsonInstruction
        {
            HasPrompt = true,
            Prompt = "Generate a person",
        });
        Assert.Equal("Generate a person\n\nYou MUST answer with JSON.", result);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstruction::should handle only schema, no prompt", Coverage = UpstreamCoverage.Covered)]
    public void Injects_schema_without_a_prompt()
    {
        var result = JsonInstructions.InjectJsonInstruction(new JsonInstruction
        {
            HasSchema = true,
            Schema = BasicSchema(),
        });
        Assert.Equal("JSON schema:\n" + BasicSchemaJson + "\n" + SchemaSuffix, result);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstruction::should handle no prompt, no schema", Coverage = UpstreamCoverage.Covered)]
    public void Injects_the_generic_suffix_when_nothing_is_passed()
    {
        Assert.Equal("You MUST answer with JSON.", JsonInstructions.InjectJsonInstruction(new JsonInstruction()));
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstruction::should handle custom schemaPrefix and schemaSuffix", Coverage = UpstreamCoverage.Covered)]
    public void Injects_custom_prefix_and_suffix()
    {
        var result = JsonInstructions.InjectJsonInstruction(new JsonInstruction
        {
            HasPrompt = true,
            Prompt = "Generate a person",
            HasSchema = true,
            Schema = BasicSchema(),
            HasSchemaPrefix = true,
            SchemaPrefix = "Custom prefix:",
            HasSchemaSuffix = true,
            SchemaSuffix = "Custom suffix",
        });
        Assert.Equal(
            "Generate a person\n\nCustom prefix:\n" + BasicSchemaJson + "\nCustom suffix",
            result);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstruction::should handle empty string prompt", Coverage = UpstreamCoverage.Covered)]
    public void Empty_prompt_is_omitted()
    {
        var result = JsonInstructions.InjectJsonInstruction(new JsonInstruction
        {
            HasPrompt = true,
            Prompt = string.Empty,
            HasSchema = true,
            Schema = BasicSchema(),
        });
        Assert.Equal("JSON schema:\n" + BasicSchemaJson + "\n" + SchemaSuffix, result);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstruction::should handle empty object schema", Coverage = UpstreamCoverage.Covered)]
    public void Empty_object_schema_is_stringified()
    {
        var result = JsonInstructions.InjectJsonInstruction(new JsonInstruction
        {
            HasPrompt = true,
            Prompt = "Generate something",
            HasSchema = true,
            Schema = new JsonObject(),
        });
        Assert.Equal(
            "Generate something\n\nJSON schema:\n{}\n" + SchemaSuffix,
            result);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstruction::should handle complex nested schema", Coverage = UpstreamCoverage.Covered)]
    public void Nested_schema_keeps_json_stringify_shape()
    {
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["person"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["name"] = new JsonObject { ["type"] = "string" },
                        ["age"] = new JsonObject { ["type"] = "number" },
                        ["address"] = new JsonObject
                        {
                            ["type"] = "object",
                            ["properties"] = new JsonObject
                            {
                                ["street"] = new JsonObject { ["type"] = "string" },
                                ["city"] = new JsonObject { ["type"] = "string" },
                            },
                        },
                    },
                },
            },
        };
        var result = JsonInstructions.InjectJsonInstruction(new JsonInstruction
        {
            HasPrompt = true,
            Prompt = "Generate a complex person",
            HasSchema = true,
            Schema = schema,
        });
        Assert.Equal(
            "Generate a complex person\n\nJSON schema:\n" +
            "{\"type\":\"object\",\"properties\":{\"person\":{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"age\":{\"type\":\"number\"},\"address\":{\"type\":\"object\",\"properties\":{\"street\":{\"type\":\"string\"},\"city\":{\"type\":\"string\"}}}}}}}\n" +
            SchemaSuffix,
            result);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstruction::should handle schema with special characters", Coverage = UpstreamCoverage.Covered)]
    public void Schema_special_characters_are_not_escaped()
    {
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["special@property"] = new JsonObject { ["type"] = "string" },
                ["emoji😊"] = new JsonObject { ["type"] = "string" },
            },
        };
        var result = JsonInstructions.InjectJsonInstruction(new JsonInstruction
        {
            HasSchema = true,
            Schema = schema,
        });
        Assert.Equal(
            "JSON schema:\n{\"type\":\"object\",\"properties\":{\"special@property\":{\"type\":\"string\"},\"emoji😊\":{\"type\":\"string\"}}}\n" + SchemaSuffix,
            result);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstruction::should handle very long prompt and schema", Coverage = UpstreamCoverage.Covered)]
    public void Long_prompt_and_schema_are_joined_with_the_default_instruction()
    {
        var prompt = new string('A', 1000);
        var properties = new JsonObject();
        for (var i = 0; i < 100; i++)
        {
            properties["prop" + i.ToString(System.Globalization.CultureInfo.InvariantCulture)] = new JsonObject { ["type"] = "string" };
        }

        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = properties,
        };
        var json = schema.ToJsonString(new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
        var result = JsonInstructions.InjectJsonInstruction(new JsonInstruction
        {
            HasPrompt = true,
            Prompt = prompt,
            HasSchema = true,
            Schema = schema,
        });
        Assert.Equal(prompt + "\n\nJSON schema:\n" + json + "\n" + SchemaSuffix, result);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstruction::should handle null values for optional parameters", Coverage = UpstreamCoverage.Covered)]
    public void Explicit_null_prompt_schema_and_affixes_produce_an_empty_string()
    {
        var result = JsonInstructions.InjectJsonInstruction(new JsonInstruction
        {
            HasPrompt = true,
            HasSchema = true,
            HasSchemaPrefix = true,
            HasSchemaSuffix = true,
        });
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstruction::should handle undefined values for optional parameters", Coverage = UpstreamCoverage.Covered)]
    public void Omitted_optional_parameters_use_the_generic_suffix()
    {
        Assert.Equal("You MUST answer with JSON.", JsonInstructions.InjectJsonInstruction(new JsonInstruction()));
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstructionIntoMessages::should handle basic case with prompt and schema", Coverage = UpstreamCoverage.Covered)]
    public void System_message_receives_the_schema_instruction()
    {
        var result = JsonInstructions.InjectJsonInstructionIntoMessages(
            new[] { new InstructionMessage("system", "Generate a person") },
            SchemaInstruction());
        Assert.Single(result);
        Assert.Equal("system", result[0].Role);
        Assert.Equal(
            "Generate a person\n\nJSON schema:\n" + BasicSchemaJson + "\n" + SchemaSuffix,
            result[0].Content);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstructionIntoMessages::should not mutate the input messages array", Coverage = UpstreamCoverage.Covered)]
    public void Input_messages_are_not_mutated()
    {
        var original = new InstructionMessage("system", "Generate a person");
        var messages = new List<InstructionMessage> { original };
        JsonInstructions.InjectJsonInstructionIntoMessages(messages, SchemaInstruction());
        Assert.Same(original, messages[0]);
        Assert.Equal("Generate a person", original.Content);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstructionIntoMessages::should handle empty messages array", Coverage = UpstreamCoverage.Covered)]
    public void Empty_messages_gain_a_system_instruction()
    {
        var result = JsonInstructions.InjectJsonInstructionIntoMessages(Array.Empty<InstructionMessage>(), SchemaInstruction());
        Assert.Single(result);
        Assert.Equal("system", result[0].Role);
        Assert.Equal("JSON schema:\n" + BasicSchemaJson + "\n" + SchemaSuffix, result[0].Content);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstructionIntoMessages::should handle messages without initial system message", Coverage = UpstreamCoverage.Covered)]
    public void Missing_system_message_is_prepended_and_other_messages_are_shared()
    {
        var user = new InstructionMessage("user", Text("Hello"));
        var assistant = new InstructionMessage("assistant", Text("Hi there"));
        var result = JsonInstructions.InjectJsonInstructionIntoMessages(new[] { user, assistant }, SchemaInstruction());
        Assert.Equal(3, result.Count);
        Assert.Equal("system", result[0].Role);
        Assert.Equal("JSON schema:\n" + BasicSchemaJson + "\n" + SchemaSuffix, result[0].Content);
        Assert.Same(user, result[1]);
        Assert.Same(assistant, result[2]);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstructionIntoMessages::should handle system message with empty content", Coverage = UpstreamCoverage.Covered)]
    public void Empty_system_content_is_replaced_by_the_schema_instruction()
    {
        var user = new InstructionMessage("user", Text("Generate data"));
        var result = JsonInstructions.InjectJsonInstructionIntoMessages(
            new[] { new InstructionMessage("system", string.Empty), user },
            SchemaInstruction());
        Assert.Equal(2, result.Count);
        Assert.Equal("JSON schema:\n" + BasicSchemaJson + "\n" + SchemaSuffix, result[0].Content);
        Assert.Same(user, result[1]);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstructionIntoMessages::should preserve all non-system messages", Coverage = UpstreamCoverage.Covered)]
    public void Non_system_messages_keep_their_references()
    {
        var user = new InstructionMessage("user", Text("Hello"));
        var assistant = new InstructionMessage("assistant", Text("Hi"));
        var followUp = new InstructionMessage("user", Text("Generate person"));
        var result = JsonInstructions.InjectJsonInstructionIntoMessages(
            new[]
            {
                new InstructionMessage("system", "You are helpful"),
                user,
                assistant,
                followUp,
            },
            SchemaInstruction());
        Assert.Equal(4, result.Count);
        Assert.Equal(
            "You are helpful\n\nJSON schema:\n" + BasicSchemaJson + "\n" + SchemaSuffix,
            result[0].Content);
        Assert.Same(user, result[1]);
        Assert.Same(assistant, result[2]);
        Assert.Same(followUp, result[3]);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstructionIntoMessages::should handle case with no schema", Coverage = UpstreamCoverage.Covered)]
    public void Messages_without_a_schema_get_the_generic_suffix()
    {
        var result = JsonInstructions.InjectJsonInstructionIntoMessages(
            new[] { new InstructionMessage("system", "Generate data") },
            new JsonInstruction());
        Assert.Equal("Generate data\n\nYou MUST answer with JSON.", result[0].Content);
    }

    [Fact]
    [UpstreamTest(Inject + "injectJsonInstructionIntoMessages::should handle custom schema prefix and suffix", Coverage = UpstreamCoverage.Covered)]
    public void Messages_use_custom_schema_prefix_and_suffix()
    {
        var result = JsonInstructions.InjectJsonInstructionIntoMessages(
            new[] { new InstructionMessage("system", "Generate data") },
            new JsonInstruction
            {
                HasSchema = true,
                Schema = BasicSchema(),
                HasSchemaPrefix = true,
                SchemaPrefix = "Custom schema:",
                HasSchemaSuffix = true,
                SchemaSuffix = "Follow this format exactly.",
            });
        Assert.Equal(
            "Generate data\n\nCustom schema:\n" + BasicSchemaJson + "\nFollow this format exactly.",
            result[0].Content);
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderEffort::returns mapped value with no warning for direct match", Coverage = UpstreamCoverage.Covered)]
    public void Direct_effort_match_has_no_warning()
    {
        var warnings = new List<SdkWarning>();
        Assert.Equal("medium", ReasoningMap.MapReasoningToProviderEffort("medium", EffortMap(), warnings));
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderEffort::returns mapped value with compatibility warning for renamed match", Coverage = UpstreamCoverage.Covered)]
    public void Renamed_effort_adds_a_compatibility_warning()
    {
        var warnings = new List<SdkWarning>();
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
        var warnings = new List<SdkWarning>();
        Assert.Equal("max", ReasoningMap.MapReasoningToProviderEffort("xhigh", EffortMap(), warnings));
        var warning = Assert.IsType<CompatibilityWarning>(Assert.Single(warnings));
        Assert.Equal("reasoning", warning.Feature);
        Assert.Equal("reasoning \"xhigh\" is not directly supported by this model. mapped to effort \"max\".", warning.Details);
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderEffort::returns undefined with unsupported warning for key missing from effortMap", Coverage = UpstreamCoverage.Covered)]
    public void Missing_effort_returns_null_and_an_unsupported_warning()
    {
        var warnings = new List<SdkWarning>();
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
        foreach (var value in new[] { "minimal", "low", "medium", "high", "xhigh" })
        {
            Assert.True(ReasoningMap.IsCustomReasoning(value));
        }
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderBudget::returns correct budget for known key", Coverage = UpstreamCoverage.Covered)]
    public void Medium_budget_is_thirty_percent()
    {
        var warnings = new List<SdkWarning>();
        Assert.Equal(19200, ReasoningMap.MapReasoningToProviderBudget("medium", 64000, 64000, warnings));
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderBudget::caps result at maxReasoningBudget", Coverage = UpstreamCoverage.Covered)]
    public void Budget_is_capped_at_the_provider_maximum()
    {
        var warnings = new List<SdkWarning>();
        Assert.Equal(50000, ReasoningMap.MapReasoningToProviderBudget("xhigh", 64000, 50000, warnings));
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderBudget::floors result at default minReasoningBudget of 1024", Coverage = UpstreamCoverage.Covered)]
    public void Budget_is_floored_at_1024()
    {
        var warnings = new List<SdkWarning>();
        Assert.Equal(1024, ReasoningMap.MapReasoningToProviderBudget("minimal", 10000, 10000, warnings));
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderBudget::respects custom minReasoningBudget", Coverage = UpstreamCoverage.Covered)]
    public void Custom_minimum_budget_is_used()
    {
        var warnings = new List<SdkWarning>();
        Assert.Equal(512, ReasoningMap.MapReasoningToProviderBudget("minimal", 10000, 10000, warnings, minReasoningBudget: 512));
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Reasoning + "mapReasoningToProviderBudget::respects custom budgetPercentages", Coverage = UpstreamCoverage.Covered)]
    public void Custom_budget_percentage_is_used()
    {
        var warnings = new List<SdkWarning>();
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
        var warnings = new List<SdkWarning>();
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

    private static JsonObject BasicSchema()
    {
        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject { ["type"] = "string" },
                ["age"] = new JsonObject { ["type"] = "number" },
            },
            ["required"] = new JsonArray { "name", "age" },
        };
    }

    private static JsonInstruction SchemaInstruction()
    {
        return new JsonInstruction
        {
            HasSchema = true,
            Schema = BasicSchema(),
        };
    }

    private static JsonArray Text(string text)
    {
        return new JsonArray
        {
            new JsonObject
            {
                ["type"] = "text",
                ["text"] = text,
            },
        };
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
