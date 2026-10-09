// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.E2e.Anthropic;

[Trait("Category", AnthropicFeatureSuite.Category)]
public sealed class ObjectGenerationTests
{
    private static async Task<GenerateTextResult> Generate(string modelId, string schema, string prompt)
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        return await AnthropicFeatureSuite.Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = AnthropicFeatureSuite.Chat(modelId),
            Output = OutputSpec.Object(schema),
            Prompt = prompt,
        });
    }

    private const string Str = "{\"type\":\"string\"}";

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_generate_basic_blog_metadata(string modelId)
    {
        var result = await Generate(modelId, "{\"type\":\"object\",\"properties\":{\"title\":" + Str + ",\"tags\":{\"type\":\"array\",\"items\":" + Str + "}},\"required\":[\"title\",\"tags\"]}", "Generate metadata for a blog post about TypeScript.");

        Assert.False(string.IsNullOrEmpty(result.Output!.Value.GetProperty("title").GetString()));
        Assert.Equal(JsonValueKind.Array, result.Output.Value.GetProperty("tags").ValueKind);
        Assert.True(result.Usage.TotalTokens > 0);
    }

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_generate_a_simple_object(string modelId)
    {
        var result = await Generate(modelId, "{\"type\":\"object\",\"properties\":{\"name\":" + Str + ",\"age\":{\"type\":\"number\"}},\"required\":[\"name\",\"age\"]}", "Generate details for a person.");

        Assert.False(string.IsNullOrEmpty(result.Output!.Value.GetProperty("name").GetString()));
        Assert.Equal(JsonValueKind.Number, result.Output.Value.GetProperty("age").ValueKind);
        Assert.True(result.Usage.TotalTokens > 0);
    }

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_generate_multiple_simple_items(string modelId)
    {
        var result = await Generate(modelId, "{\"type\":\"object\",\"properties\":{\"items\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"name\":" + Str + ",\"quantity\":{\"type\":\"number\"}},\"required\":[\"name\",\"quantity\"]},\"minItems\":3,\"maxItems\":3}},\"required\":[\"items\"]}", "Generate a shopping list with 3 items.");

        var items = result.Output!.Value.GetProperty("items");
        Assert.Equal(3, items.GetArrayLength());
        Assert.False(string.IsNullOrEmpty(items[0].GetProperty("name").GetString()));
        Assert.Equal(JsonValueKind.Number, items[0].GetProperty("quantity").ValueKind);
    }

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_generate_nested_objects(string modelId)
    {
        var result = await Generate(modelId, "{\"type\":\"object\",\"properties\":{\"user\":{\"type\":\"object\",\"properties\":{\"name\":" + Str + ",\"contact\":{\"type\":\"object\",\"properties\":{\"email\":" + Str + ",\"phone\":" + Str + "},\"required\":[\"email\",\"phone\"]}},\"required\":[\"name\",\"contact\"]},\"preferences\":{\"type\":\"object\",\"properties\":{\"theme\":{\"type\":\"string\",\"enum\":[\"light\",\"dark\"]},\"notifications\":{\"type\":\"boolean\"}},\"required\":[\"theme\",\"notifications\"]}},\"required\":[\"user\",\"preferences\"]}", "Generate a user profile with contact details and preferences.");

        var output = result.Output!.Value;
        Assert.Equal(JsonValueKind.String, output.GetProperty("user").GetProperty("name").ValueKind);
        Assert.Equal(JsonValueKind.String, output.GetProperty("user").GetProperty("contact").GetProperty("email").ValueKind);
        Assert.Equal(JsonValueKind.String, output.GetProperty("user").GetProperty("contact").GetProperty("phone").ValueKind);
        Assert.Contains(output.GetProperty("preferences").GetProperty("theme").GetString(), new[] { "light", "dark" });
        Assert.True(output.GetProperty("preferences").GetProperty("notifications").ValueKind is JsonValueKind.True or JsonValueKind.False);
    }

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_generate_arrays_of_objects(string modelId)
    {
        var result = await Generate(modelId, "{\"type\":\"object\",\"properties\":{\"posts\":{\"type\":\"array\",\"minItems\":2,\"items\":{\"type\":\"object\",\"properties\":{\"title\":" + Str + ",\"comments\":{\"type\":\"array\",\"minItems\":1,\"items\":{\"type\":\"object\",\"properties\":{\"author\":" + Str + ",\"text\":" + Str + "},\"required\":[\"author\",\"text\"]}}},\"required\":[\"title\",\"comments\"]}}},\"required\":[\"posts\"]}", "Generate a blog with multiple posts and comments.");

        var posts = result.Output!.Value.GetProperty("posts");
        Assert.True(posts.GetArrayLength() >= 2);
        Assert.True(posts[0].GetProperty("comments").GetArrayLength() >= 1);
    }

    private static string Routine(string specific) =>
        "{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"name\":" + Str + ",\"category\":" + Str + ",\"usage_instructions\":" + Str + ",\"" + specific + "\":" + Str + "},\"required\":[\"name\",\"category\",\"usage_instructions\",\"" + specific + "\"]}}";

    private static string SkincareSchema() =>
        "{\"type\":\"object\",\"properties\":{\"morning_routine\":" + Routine("morning_specific_instructions") + ",\"evening_routine\":" + Routine("evening_specific_instructions") + ",\"notes\":" + Str + "},\"required\":[\"morning_routine\",\"evening_routine\",\"notes\"]}";

    private static void AssertSkincare(GenerateTextResult result)
    {
        var output = result.Output!.Value;
        Assert.True(output.GetProperty("morning_routine").GetArrayLength() > 0);
        Assert.True(output.GetProperty("evening_routine").GetArrayLength() > 0);
        Assert.False(string.IsNullOrEmpty(output.GetProperty("morning_routine")[0].GetProperty("morning_specific_instructions").GetString()));
        Assert.False(string.IsNullOrEmpty(output.GetProperty("evening_routine")[0].GetProperty("evening_specific_instructions").GetString()));
    }

    // JSON Schema has no extend(); the cross-referenced zod schema serializes to the same shape as the flat one.
    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_handle_cross_referenced_schemas(string modelId)
    {
        AssertSkincare(await Generate(modelId, SkincareSchema(), "Generate a skincare routine with morning and evening products."));
    }

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_handle_equivalent_flat_schemas(string modelId)
    {
        AssertSkincare(await Generate(modelId, SkincareSchema(), "Generate a skincare routine with morning and evening products."));
    }
}
