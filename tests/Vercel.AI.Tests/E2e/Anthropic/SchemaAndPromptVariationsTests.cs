// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.E2e.Anthropic;

[Trait("Category", AnthropicFeatureSuite.Category)]
public sealed class SchemaAndPromptVariationsTests
{
    private const string Str = "{\"type\":\"string\"}";

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_generate_with_field_descriptions(string modelId)
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        var result = await AnthropicFeatureSuite.Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = AnthropicFeatureSuite.Chat(modelId),
            Output = OutputSpec.Object("{\"type\":\"object\",\"properties\":{\"title\":{\"type\":\"string\",\"description\":\"A catchy title for the article\"},\"summary\":{\"type\":\"string\",\"description\":\"A 2-3 sentence overview of the main points\"},\"readingTime\":{\"type\":\"number\",\"description\":\"Estimated reading time in minutes\"},\"targetAudience\":{\"type\":\"array\",\"items\":" + Str + ",\"description\":\"The intended reader groups\"}},\"required\":[\"title\",\"summary\",\"readingTime\",\"targetAudience\"]}"),
            Prompt = "Generate metadata for a technical article.",
        });

        Assert.False(string.IsNullOrEmpty(result.Output!.Value.GetProperty("title").GetString()));
        Assert.True(result.Output.Value.GetProperty("summary").GetString()!.Length > 50);
    }

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_handle_detailed_system_prompts(string modelId)
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        var result = await AnthropicFeatureSuite.Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = AnthropicFeatureSuite.Chat(modelId),
            Output = OutputSpec.Object("{\"type\":\"object\",\"properties\":{\"recipe\":{\"type\":\"object\",\"properties\":{\"name\":" + Str + ",\"ingredients\":{\"type\":\"array\",\"items\":" + Str + "},\"steps\":{\"type\":\"array\",\"items\":" + Str + "}},\"required\":[\"name\",\"ingredients\",\"steps\"]}},\"required\":[\"recipe\"]}"),
            Messages = new ModelMessage[]
            {
                new SystemModelMessage("You are a professional chef. Always provide detailed, precise cooking instructions."),
                new UserModelMessage("Create a pasta recipe."),
            },
        });

        Assert.True(result.Output!.Value.GetProperty("recipe").GetProperty("steps").GetArrayLength() > 3);
        Assert.True(result.Output.Value.GetProperty("recipe").GetProperty("ingredients").GetArrayLength() > 3);
    }

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_generate_complex_objects_with_both_descriptions_and_system_context(string modelId)
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        var schema = "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\",\"description\":\"Product name, should be unique and memorable\"},\"price\":{\"type\":\"number\",\"description\":\"Price in USD, should be competitive for market\"},\"features\":{\"type\":\"array\",\"items\":" + Str + ",\"description\":\"Key selling points, 3-5 items\"},\"marketingPlan\":{\"type\":\"object\",\"description\":\"Marketing strategy details\",\"properties\":{\"targetMarket\":{\"type\":\"string\",\"description\":\"Primary customer demographic\"},\"channels\":{\"type\":\"array\",\"items\":" + Str + ",\"description\":\"Marketing channels to use\"},\"budget\":{\"type\":\"number\",\"description\":\"Proposed marketing budget in USD\"}},\"required\":[\"targetMarket\",\"channels\",\"budget\"]}},\"required\":[\"name\",\"price\",\"features\",\"marketingPlan\"]}";
        var result = await AnthropicFeatureSuite.Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = AnthropicFeatureSuite.Chat(modelId),
            Output = OutputSpec.Object(schema, "product"),
            Messages = new ModelMessage[]
            {
                new SystemModelMessage("You are a senior product manager with 15 years of experience in tech products."),
                new UserModelMessage("Create a product plan for a new smart home device."),
            },
        });

        var output = result.Output!.Value;
        Assert.True(output.GetProperty("features").GetArrayLength() >= 3);
        Assert.True(output.GetProperty("marketingPlan").GetProperty("budget").GetDouble() > 0);
        Assert.True(output.GetProperty("price").GetDouble() > 0);
    }
}
