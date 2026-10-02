// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

public sealed class InjectJsonInstructionTests
{
    private const string Suffix = "You MUST answer with a JSON object that matches the JSON schema above.";
    private static readonly JsonElement Basic = Parse("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"age\":{\"type\":\"number\"}},\"required\":[\"name\",\"age\"]}");

    [Fact]
    [UpstreamTest("packages/ai/src/generate-object/inject-json-instruction.test.ts::should handle basic case with prompt and schema", Coverage = UpstreamCoverage.Covered)]
    public void Includes_prompt_and_schema()
    {
        var result = JsonInstructions.InjectJsonInstruction("Generate a person", Basic);
        Assert.Equal("Generate a person\n\nJSON schema:\n" + Compact(Basic) + "\n" + Suffix, result);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-object/inject-json-instruction.test.ts::should handle only prompt, no schema", Coverage = UpstreamCoverage.Covered)]
    public void Prompt_without_schema_asks_for_json()
    {
        Assert.Equal("Generate a person\n\nYou MUST answer with JSON.", JsonInstructions.InjectJsonInstruction("Generate a person"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-object/inject-json-instruction.test.ts::should handle only schema, no prompt", Coverage = UpstreamCoverage.Covered)]
    public void Schema_without_prompt()
    {
        Assert.Equal("JSON schema:\n" + Compact(Basic) + "\n" + Suffix, JsonInstructions.InjectJsonInstruction(schema: Basic));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-object/inject-json-instruction.test.ts::should handle no prompt, no schema", Coverage = UpstreamCoverage.Covered)]
    public void Neither_prompt_nor_schema()
    {
        Assert.Equal("You MUST answer with JSON.", JsonInstructions.InjectJsonInstruction());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-object/inject-json-instruction.test.ts::should handle custom schemaPrefix and schemaSuffix", Coverage = UpstreamCoverage.Covered)]
    public void Custom_prefix_and_suffix()
    {
        var result = JsonInstructions.InjectJsonInstruction("Generate a person", Basic, JsonInstructionText.From("Custom prefix:"), JsonInstructionText.From("Custom suffix"));
        Assert.Equal("Generate a person\n\nCustom prefix:\n" + Compact(Basic) + "\nCustom suffix", result);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-object/inject-json-instruction.test.ts::should handle empty string prompt", Coverage = UpstreamCoverage.Covered)]
    public void Empty_prompt_is_omitted()
    {
        Assert.Equal("JSON schema:\n" + Compact(Basic) + "\n" + Suffix, JsonInstructions.InjectJsonInstruction(string.Empty, Basic));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-object/inject-json-instruction.test.ts::should handle empty object schema", Coverage = UpstreamCoverage.Covered)]
    public void Empty_object_schema()
    {
        var schema = Parse("{}");
        Assert.Equal("Generate something\n\nJSON schema:\n{}\n" + Suffix, JsonInstructions.InjectJsonInstruction("Generate something", schema));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-object/inject-json-instruction.test.ts::should handle complex nested schema", Coverage = UpstreamCoverage.Covered)]
    public void Nested_schema()
    {
        var schema = Parse("{\"type\":\"object\",\"properties\":{\"person\":{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"age\":{\"type\":\"number\"},\"address\":{\"type\":\"object\",\"properties\":{\"street\":{\"type\":\"string\"},\"city\":{\"type\":\"string\"}}}}}}}");
        Assert.Equal("Generate a complex person\n\nJSON schema:\n" + Compact(schema) + "\n" + Suffix, JsonInstructions.InjectJsonInstruction("Generate a complex person", schema));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-object/inject-json-instruction.test.ts::should handle schema with special characters", Coverage = UpstreamCoverage.Covered)]
    public void Special_characters_stay_unescaped()
    {
        var schema = Parse("{\"type\":\"object\",\"properties\":{\"special@property\":{\"type\":\"string\"},\"emoji😊\":{\"type\":\"string\"}}}");
        Assert.Equal("JSON schema:\n{\"type\":\"object\",\"properties\":{\"special@property\":{\"type\":\"string\"},\"emoji😊\":{\"type\":\"string\"}}}\n" + Suffix, JsonInstructions.InjectJsonInstruction(schema: schema));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-object/inject-json-instruction.test.ts::should handle very long prompt and schema", Coverage = UpstreamCoverage.Covered)]
    public void Long_prompt_and_schema()
    {
        var prompt = new string('A', 1000);
        var properties = new StringBuilderJson();
        for (var i = 0; i < 100; i++)
        {
            if (i > 0)
            {
                properties.Append(',');
            }

            properties.Append("\"prop").Append(i.ToString()).Append("\":{\"type\":\"string\"}");
        }

        var schema = Parse("{\"type\":\"object\",\"properties\":{" + properties.Text + "}}");
        var result = JsonInstructions.InjectJsonInstruction(prompt, schema);
        Assert.Equal(prompt + "\n\nJSON schema:\n" + Compact(schema) + "\n" + Suffix, result);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-object/inject-json-instruction.test.ts::should handle null values for optional parameters", Coverage = UpstreamCoverage.Covered)]
    public void Explicit_nulls_produce_an_empty_string()
    {
        Assert.Equal(string.Empty, JsonInstructions.InjectJsonInstruction(null, null, JsonInstructionText.Null, JsonInstructionText.Null));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-object/inject-json-instruction.test.ts::should handle undefined values for optional parameters", Coverage = UpstreamCoverage.Covered)]
    public void Omitted_values_use_the_generic_suffix()
    {
        Assert.Equal("You MUST answer with JSON.", JsonInstructions.InjectJsonInstruction(null, null, default, default));
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static string Compact(JsonElement element)
    {
        return JsonSerializer.Serialize(element, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
    }

    private sealed class StringBuilderJson
    {
        private readonly System.Text.StringBuilder _builder = new();

        public string Text => _builder.ToString();

        public void Append(char value)
        {
            _builder.Append(value);
        }

        public void Append(string value)
        {
            _builder.Append(value);
        }
    }
}

public sealed class ArrayOutputStrategyTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/generate-object/output-strategy.test.ts::array output strategy::should preserve root-level %s when wrapping the element schema", Coverage = UpstreamCoverage.Covered)]
    public void Lifts_definitions_and_defs()
    {
        foreach (var keyword in new[] { "definitions", "$defs" })
        {
            var reference = keyword == "definitions" ? "#/definitions/Shared" : "#/$defs/Shared";
            using var document = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"shared\":{\"$ref\":\"" + reference + "\"}},\"required\":[\"shared\"],\"additionalProperties\":false,\"" + keyword + "\":{\"Shared\":{\"type\":\"string\"}}}");
            var result = OutputStrategies.WrapArrayOutputSchema(document.RootElement);
            Assert.Equal("string", result.GetProperty(keyword).GetProperty("Shared").GetProperty("type").GetString());
            var items = result.GetProperty("properties").GetProperty("elements").GetProperty("items");
            Assert.Equal(reference, items.GetProperty("properties").GetProperty("shared").GetProperty("$ref").GetString());
            Assert.False(items.TryGetProperty(keyword, out _));
        }
    }
}

public sealed class ValidateToolContextTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/validate-tool-context.test.ts::validateToolContext::returns the tool context as-is when no context schema is defined", Coverage = UpstreamCoverage.Covered)]
    public void Returns_the_same_context_without_a_schema()
    {
        var context = new Dictionary<string, object> { ["apiKey"] = 123 };
        Assert.Same(context, ToolContexts.ValidateToolContext("weather", context, null));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/validate-tool-context.test.ts::validateToolContext::returns the validated tool context when the context schema matches", Coverage = UpstreamCoverage.Covered)]
    public void Returns_context_when_the_schema_matches()
    {
        var context = new Dictionary<string, object> { ["apiKey"] = "secret" };
        var result = ToolContexts.ValidateToolContext("weather", context, value => value["apiKey"] is string);
        Assert.Equal("secret", result["apiKey"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/validate-tool-context.test.ts::validateToolContext::throws TypeValidationError when the context schema validation fails", Coverage = UpstreamCoverage.Covered)]
    public void Throws_when_the_schema_rejects_the_context()
    {
        var context = new Dictionary<string, object> { ["apiKey"] = 123 };
        var exception = Assert.Throws<TypeValidationException>(() => ToolContexts.ValidateToolContext("weather", context, value => value["apiKey"] is string));
        Assert.Same(context, exception.Value);
        Assert.Equal("tool context", exception.Context.Field);
        Assert.Equal("weather", exception.Context.EntityName);
    }
}

public sealed class GeneratedFileDownloadTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generated-file-download.test.ts::generated file downloads::rejects an unsafe file URL before fetching it", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_unsafe_file_url_before_fetching()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedFileUrl(new Uri("http://127.0.0.1/file"), "text/plain") },
                FinishReason.Stop,
                LanguageModelUsage.Empty,
                "stop"),
        };

        var exception = await Assert.ThrowsAsync<DownloadException>(() => Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "image" }));
        Assert.Equal("AI_DownloadError", exception.Name);
        Assert.Contains("127.0.0.1", exception.Message);
        Assert.Equal(0, model.Calls.Count == 0 ? 0 : 1);
        Assert.Single(model.Calls);
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
