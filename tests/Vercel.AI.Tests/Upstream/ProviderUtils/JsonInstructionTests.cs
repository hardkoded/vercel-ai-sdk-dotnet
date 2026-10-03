// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

public sealed class JsonInstructionTests
{
    private const string SchemaSuffix = "You MUST answer with a JSON object that matches the JSON schema above.";
    private const string GenericSuffix = "You MUST answer with JSON.";

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstruction::should handle basic case with prompt and schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_joins_a_prompt_and_a_schema()
    {
        Assert.Equal(
            "Generate a person\n\nJSON schema:\n" + BasicJson() + "\n" + SchemaSuffix,
            JsonInstructions.Inject("Generate a person", BasicSchema()));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstruction::should handle only prompt, no schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_appends_a_generic_suffix_when_there_is_no_schema()
    {
        Assert.Equal("Generate a person\n\n" + GenericSuffix, JsonInstructions.Inject("Generate a person", null));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstruction::should handle only schema, no prompt",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_starts_with_the_schema_when_the_prompt_is_missing()
    {
        Assert.Equal("JSON schema:\n" + BasicJson() + "\n" + SchemaSuffix, JsonInstructions.Inject(null, BasicSchema()));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstruction::should handle no prompt, no schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_uses_the_generic_suffix_alone()
    {
        Assert.Equal(GenericSuffix, JsonInstructions.Inject(null, null));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstruction::should handle custom schemaPrefix and schemaSuffix",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_uses_a_custom_prefix_and_suffix()
    {
        var result = JsonInstructions.Inject("Generate a person", BasicSchema(), "Custom prefix:", "Custom suffix", true, true);
        Assert.Equal("Generate a person\n\nCustom prefix:\n" + BasicJson() + "\nCustom suffix", result);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstruction::should handle empty string prompt",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_omits_an_empty_prompt()
    {
        Assert.Equal("JSON schema:\n" + BasicJson() + "\n" + SchemaSuffix, JsonInstructions.Inject(string.Empty, BasicSchema()));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstruction::should handle empty object schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_prints_an_empty_object()
    {
        Assert.Equal("Generate something\n\nJSON schema:\n{}\n" + SchemaSuffix, JsonInstructions.Inject("Generate something", new JsonObject()));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstruction::should handle complex nested schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_prints_a_nested_object()
    {
        var address = JsonSchemas.Object(new[] { Pair("street", JsonSchemas.String()), Pair("city", JsonSchemas.String()) });
        var person = JsonSchemas.Object(new[]
        {
            Pair("name", JsonSchemas.String()),
            Pair("age", JsonSchemas.Number()),
            Pair("address", address),
        });
        var schema = JsonSchemas.Object(new[] { Pair("person", person) });
        var expected = "{\"type\":\"object\",\"properties\":{\"person\":{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"age\":{\"type\":\"number\"},\"address\":{\"type\":\"object\",\"properties\":{\"street\":{\"type\":\"string\"},\"city\":{\"type\":\"string\"}}}}}}}";
        Assert.Equal("Generate a complex person\n\nJSON schema:\n" + expected + "\n" + SchemaSuffix, JsonInstructions.Inject("Generate a complex person", schema));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstruction::should handle schema with special characters",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_keeps_special_characters_in_property_names()
    {
        var schema = JsonSchemas.Object(new[]
        {
            Pair("special@property", JsonSchemas.String()),
            Pair("emoji😊", JsonSchemas.String()),
        });
        var expected = "{\"type\":\"object\",\"properties\":{\"special@property\":{\"type\":\"string\"},\"emoji😊\":{\"type\":\"string\"}}}";
        Assert.Equal("JSON schema:\n" + expected + "\n" + SchemaSuffix, JsonInstructions.Inject(null, schema));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstruction::should handle very long prompt and schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_prints_a_long_prompt_and_schema()
    {
        var properties = new List<KeyValuePair<string, JsonNode>>();
        for (var index = 0; index < 100; index++)
        {
            properties.Add(Pair("prop" + index.ToString(), JsonSchemas.String()));
        }

        var schema = JsonSchemas.Object(properties);
        var prompt = new string('A', 1000);
        Assert.Equal(prompt + "\n\nJSON schema:\n" + JsonSchemas.Stringify(schema) + "\n" + SchemaSuffix, JsonInstructions.Inject(prompt, schema));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstruction::should handle null values for optional parameters",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_returns_empty_text_when_every_part_is_explicitly_null()
    {
        Assert.Equal(string.Empty, JsonInstructions.Inject(null, null, null, null, true, true));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstruction::should handle undefined values for optional parameters",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_uses_the_generic_suffix_when_optional_parts_are_omitted()
    {
        Assert.Equal(GenericSuffix, JsonInstructions.Inject(null, null, null, null));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstructionIntoMessages::should handle basic case with prompt and schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_into_messages_rewrites_the_system_message()
    {
        var result = JsonInstructions.InjectIntoMessages(new[] { System("Generate a person") }, BasicSchema());
        var message = Assert.Single(result);
        Assert.Equal("system", message.Role);
        Assert.Equal("Generate a person\n\nJSON schema:\n" + BasicJson() + "\n" + SchemaSuffix, (string)message.Content!);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstructionIntoMessages::should not mutate the input messages array",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_into_messages_leaves_the_input_unchanged()
    {
        var original = new[] { System("Generate a person") };
        JsonInstructions.InjectIntoMessages(original, BasicSchema());
        Assert.Equal("Generate a person", (string)original[0].Content!);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstructionIntoMessages::should handle empty messages array",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_into_messages_inserts_a_system_message()
    {
        var result = JsonInstructions.InjectIntoMessages(Array.Empty<JsonInstructions.PromptMessage>(), BasicSchema());
        var message = Assert.Single(result);
        Assert.Equal("JSON schema:\n" + BasicJson() + "\n" + SchemaSuffix, (string)message.Content!);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstructionIntoMessages::should handle messages without initial system message",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_into_messages_keeps_user_and_assistant_content()
    {
        var user = new JsonInstructions.PromptMessage("user", Text("Hello"));
        var assistant = new JsonInstructions.PromptMessage("assistant", Text("Hi there"));
        var result = JsonInstructions.InjectIntoMessages(new[] { user, assistant }, BasicSchema());
        Assert.Equal(3, result.Count);
        Assert.Equal("system", result[0].Role);
        Assert.Same(user, result[1]);
        Assert.Same(assistant, result[2]);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstructionIntoMessages::should handle system message with empty content",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_into_messages_replaces_empty_system_content()
    {
        var user = new JsonInstructions.PromptMessage("user", Text("Generate data"));
        var result = JsonInstructions.InjectIntoMessages(new[] { System(string.Empty), user }, BasicSchema());
        Assert.Equal("JSON schema:\n" + BasicJson() + "\n" + SchemaSuffix, (string)result[0].Content!);
        Assert.Same(user, result[1]);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstructionIntoMessages::should preserve all non-system messages",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_into_messages_preserves_later_messages()
    {
        var first = new JsonInstructions.PromptMessage("user", Text("Hello"));
        var second = new JsonInstructions.PromptMessage("assistant", Text("Hi"));
        var third = new JsonInstructions.PromptMessage("user", Text("Generate person"));
        var result = JsonInstructions.InjectIntoMessages(new[] { System("You are helpful"), first, second, third }, BasicSchema());
        Assert.Equal("You are helpful\n\nJSON schema:\n" + BasicJson() + "\n" + SchemaSuffix, (string)result[0].Content!);
        Assert.Same(first, result[1]);
        Assert.Same(second, result[2]);
        Assert.Same(third, result[3]);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstructionIntoMessages::should handle case with no schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_into_messages_uses_the_generic_suffix()
    {
        var result = JsonInstructions.InjectIntoMessages(new[] { System("Generate data") });
        Assert.Equal("Generate data\n\n" + GenericSuffix, (string)Assert.Single(result).Content!);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/inject-json-instruction.test.ts::injectJsonInstructionIntoMessages::should handle custom schema prefix and suffix",
        Coverage = UpstreamCoverage.Covered)]
    public void Inject_into_messages_uses_a_custom_prefix_and_suffix()
    {
        var result = JsonInstructions.InjectIntoMessages(
            new[] { System("Generate data") },
            BasicSchema(),
            "Custom schema:",
            "Follow this format exactly.",
            true,
            true);
        Assert.Equal("Generate data\n\nCustom schema:\n" + BasicJson() + "\nFollow this format exactly.", (string)Assert.Single(result).Content!);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::adds additionalProperties: false to objects recursively",
        Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_are_closed_on_nested_objects()
    {
        var schema = Apply("{\"type\":\"object\",\"properties\":{\"user\":{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}},\"age\":{\"type\":\"number\"}}}");
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"user\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"name\":{\"type\":\"string\"}}},\"age\":{\"type\":\"number\"}}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::adds additionalProperties: false to objects inside arrays",
        Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_are_closed_inside_array_items()
    {
        var schema = Apply("{\"type\":\"object\",\"properties\":{\"ingredients\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"amount\":{\"type\":\"string\"}},\"required\":[\"name\",\"amount\"]}}},\"required\":[\"ingredients\"]}");
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"ingredients\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"name\":{\"type\":\"string\"},\"amount\":{\"type\":\"string\"}},\"required\":[\"name\",\"amount\"]}}},\"required\":[\"ingredients\"]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::adds additionalProperties: false when type is a union that includes \"object\"",
        Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_are_closed_when_object_is_one_type()
    {
        var schema = Apply("{\"type\":\"object\",\"properties\":{\"response\":{\"type\":[\"object\",\"null\"],\"properties\":{\"name\":{\"type\":\"string\"}}}}}");
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"response\":{\"type\":[\"object\",\"null\"],\"additionalProperties\":false,\"properties\":{\"name\":{\"type\":\"string\"}}}}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::adds additionalProperties: false to objects inside anyOf",
        Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_are_closed_inside_any_of()
    {
        var schema = Apply("{\"type\":\"object\",\"properties\":{\"response\":{\"anyOf\":[{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}},{\"type\":\"object\",\"properties\":{\"amount\":{\"type\":\"string\"}}}]}}}");
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"response\":{\"anyOf\":[{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"name\":{\"type\":\"string\"}}},{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"amount\":{\"type\":\"string\"}}}]}}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::adds additionalProperties: false to objects inside allOf",
        Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_are_closed_inside_all_of()
    {
        var schema = Apply("{\"type\":\"object\",\"properties\":{\"response\":{\"allOf\":[{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}},{\"type\":\"object\",\"properties\":{\"age\":{\"type\":\"number\"}}}]}}}");
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"response\":{\"allOf\":[{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"name\":{\"type\":\"string\"}}},{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"age\":{\"type\":\"number\"}}}]}}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::adds additionalProperties: false to objects inside oneOf",
        Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_are_closed_inside_one_of()
    {
        var schema = Apply("{\"type\":\"object\",\"properties\":{\"response\":{\"oneOf\":[{\"type\":\"object\",\"properties\":{\"success\":{\"type\":\"boolean\"}}},{\"type\":\"object\",\"properties\":{\"error\":{\"type\":\"string\"}}}]}}}");
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"response\":{\"oneOf\":[{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"success\":{\"type\":\"boolean\"}}},{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"error\":{\"type\":\"string\"}}}]}}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::adds additionalProperties: false to object schemas inside definitions (refs)",
        Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_are_closed_inside_definitions()
    {
        var schema = Apply("{\"type\":\"object\",\"properties\":{\"node\":{\"$ref\":\"#/definitions/Node\"}},\"definitions\":{\"Node\":{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"},\"next\":{\"$ref\":\"#/definitions/Node\"}}}}}");
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"node\":{\"$ref\":\"#/definitions/Node\"}},\"definitions\":{\"Node\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"value\":{\"type\":\"string\"},\"next\":{\"$ref\":\"#/definitions/Node\"}}}}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::overwrites existing additionalProperties flags",
        Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_overwrite_true()
    {
        var schema = Apply("{\"type\":\"object\",\"additionalProperties\":true,\"properties\":{\"meta\":{\"type\":\"object\",\"additionalProperties\":true,\"properties\":{\"id\":{\"type\":\"string\"}}}}}");
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"meta\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"id\":{\"type\":\"string\"}}}}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::preserves schema-valued additionalProperties recursively",
        Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_keep_a_schema_and_close_it()
    {
        var schema = Apply("{\"type\":\"object\",\"additionalProperties\":{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}}");
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"additionalProperties\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"value\":{\"type\":\"string\"}}}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::leaves non-object schemas unchanged",
        Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_leave_a_string_schema_unchanged()
    {
        var schema = JsonNode.Parse("{\"type\":\"string\"}")!.AsObject();
        Assert.Same(schema, JsonSchemas.AddAdditionalProperties(schema));
        JsonAssert.Equal(schema, "{\"type\":\"string\"}");
    }

    private static string BasicJson()
    {
        return "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"age\":{\"type\":\"number\"}},\"required\":[\"name\",\"age\"]}";
    }

    private static JsonObject BasicSchema()
    {
        return JsonSchemas.Object(
            new[] { Pair("name", JsonSchemas.String()), Pair("age", JsonSchemas.Number()) },
            new[] { "name", "age" });
    }

    private static JsonInstructions.PromptMessage System(string content)
    {
        return new JsonInstructions.PromptMessage("system", content);
    }

    private static JsonArray Text(string text)
    {
        return new JsonArray { new JsonObject { ["type"] = "text", ["text"] = text } };
    }

    private static JsonObject Apply(string json)
    {
        return JsonSchemas.AddAdditionalProperties(JsonNode.Parse(json)!.AsObject());
    }

    private static KeyValuePair<string, JsonNode> Pair(string name, JsonNode schema)
    {
        return new KeyValuePair<string, JsonNode>(name, schema);
    }
}
