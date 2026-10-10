// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class GoogleCoreUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/google/src/get-model-path.test.ts::should pass through model path for models/*", Coverage = UpstreamCoverage.Covered)]
    public void Passes_through_models_prefix()
    {
        Assert.Equal("models/some-model", GoogleModelPath.Get("models/some-model"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/get-model-path.test.ts::should pass through model path for tunedModels/*", Coverage = UpstreamCoverage.Covered)]
    public void Passes_through_tuned_models_prefix()
    {
        Assert.Equal("tunedModels/some-model", GoogleModelPath.Get("tunedModels/some-model"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/get-model-path.test.ts::should add model path prefix to models without slash", Coverage = UpstreamCoverage.Covered)]
    public void Prefixes_models_without_a_slash()
    {
        Assert.Equal("models/some-model", GoogleModelPath.Get("some-model"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-google-usage.test.ts::convertGoogleUsage::includes tool-use prompt tokens in input usage", Coverage = UpstreamCoverage.Covered)]
    public void Includes_tool_use_prompt_tokens_in_input_usage()
    {
        var usage = GoogleUsage.Convert(GoogleUpstream.Element("{\"promptTokenCount\":55,\"toolUsePromptTokenCount\":89,\"candidatesTokenCount\":51,\"thoughtsTokenCount\":56,\"totalTokenCount\":251}"));
        Assert.NotNull(usage);
        Assert.Equal(144, usage!.InputTotal);
        Assert.Equal(144, usage.NoCache);
        Assert.Equal(0, usage.CacheRead);
        Assert.Null(usage.CacheWrite);
        Assert.Equal(107, usage.OutputTotal);
        Assert.Equal(51, usage.Text);
        Assert.Equal(56, usage.Reasoning);
        GoogleUpstream.JsonEqual(usage.Raw!.Value, "{\"promptTokenCount\":55,\"toolUsePromptTokenCount\":89,\"candidatesTokenCount\":51,\"thoughtsTokenCount\":56,\"totalTokenCount\":251}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-google-usage.test.ts::convertGoogleUsage::includes tool-use prompt tokens when calculating uncached input", Coverage = UpstreamCoverage.Covered)]
    public void Subtracts_cached_tokens_after_adding_tool_use_tokens()
    {
        var usage = GoogleUsage.Convert(GoogleUpstream.Element("{\"promptTokenCount\":55,\"toolUsePromptTokenCount\":89,\"cachedContentTokenCount\":100}"));
        Assert.NotNull(usage);
        Assert.Equal(144, usage!.InputTotal);
        Assert.Equal(44, usage.NoCache);
        Assert.Equal(100, usage.CacheRead);
        Assert.Null(usage.CacheWrite);
    }

    [Fact]
    [UpstreamTest("packages/google/src/sanitize-response-json-schema.test.ts::replaces const with enum while preserving JSON Schema", Coverage = UpstreamCoverage.Covered)]
    public void Replaces_const_with_enum_without_mutating_the_input()
    {
        const string schema = "{\"type\":\"object\",\"properties\":{\"response\":{\"oneOf\":[{\"type\":\"object\",\"properties\":{\"type\":{\"type\":\"string\",\"const\":\"fruit\"}},\"required\":[\"type\"],\"additionalProperties\":false}]}},\"required\":[\"response\"],\"additionalProperties\":false,\"$defs\":{\"label\":{\"type\":\"string\",\"const\":\"produce\"}}}";
        var input = JsonNode.Parse(schema)!;
        var sanitized = GoogleJsonSchema.Sanitize(input);
        GoogleUpstream.JsonEqual(sanitized, "{\"type\":\"object\",\"properties\":{\"response\":{\"oneOf\":[{\"type\":\"object\",\"properties\":{\"type\":{\"type\":\"string\",\"enum\":[\"fruit\"]}},\"required\":[\"type\"],\"additionalProperties\":false}]}},\"required\":[\"response\"],\"additionalProperties\":false,\"$defs\":{\"label\":{\"type\":\"string\",\"enum\":[\"produce\"]}}}");
        GoogleUpstream.JsonEqual(input, schema);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-supported-file-url.test.ts::should return true for valid Google generative language file URLs", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_generative_language_file_urls()
    {
        Assert.True(GoogleFileUrls.IsSupported("https://generativelanguage.googleapis.com/v1beta/files/00000000-00000000-00000000-00000000"));
        Assert.True(GoogleFileUrls.IsSupported("https://generativelanguage.googleapis.com/v1beta/files/test123"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-supported-file-url.test.ts::should return true for valid YouTube URLs", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_public_youtube_watch_urls()
    {
        Assert.True(GoogleFileUrls.IsSupported("https://www.youtube.com/watch?v=dQw4w9WgXcQ"));
        Assert.True(GoogleFileUrls.IsSupported("https://youtube.com/watch?v=dQw4w9WgXcQ"));
        Assert.True(GoogleFileUrls.IsSupported("https://youtu.be/dQw4w9WgXcQ"));
        Assert.True(GoogleFileUrls.IsSupported("https://www.youtube.com/watch?v=dQw4w9WgXcQ&feature=youtu.be"));
        Assert.True(GoogleFileUrls.IsSupported("https://youtu.be/dQw4w9WgXcQ?t=42"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-supported-file-url.test.ts::should return false for invalid YouTube URLs", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_youtube_urls_that_are_not_watch_links()
    {
        Assert.False(GoogleFileUrls.IsSupported("https://youtube.com/channel/UCdQw4w9WgXcQ"));
        Assert.False(GoogleFileUrls.IsSupported("https://youtube.com/playlist?list=PLdQw4w9WgXcQ"));
        Assert.False(GoogleFileUrls.IsSupported("https://m.youtube.com/watch?v=dQw4w9WgXcQ"));
        Assert.False(GoogleFileUrls.IsSupported("http://youtube.com/watch?v=dQw4w9WgXcQ"));
        Assert.False(GoogleFileUrls.IsSupported("https://vimeo.com/123456789"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-supported-file-url.test.ts::should return false for non-Google generative language file URLs", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_urls_outside_the_files_endpoint()
    {
        Assert.False(GoogleFileUrls.IsSupported("https://example.com"));
        Assert.False(GoogleFileUrls.IsSupported("https://example.com/foo/bar"));
        Assert.False(GoogleFileUrls.IsSupported("https://generativelanguage.googleapis.com"));
        Assert.False(GoogleFileUrls.IsSupported("https://generativelanguage.googleapis.com/v1/other"));
        Assert.False(GoogleFileUrls.IsSupported("http://generativelanguage.googleapis.com/v1beta/files/test"));
        Assert.False(GoogleFileUrls.IsSupported("https://api.googleapis.com/v1beta/files/test"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-model-capabilities.test.ts::getGoogleModelCapabilities::classifies $modelId without falling back to legacy behavior", Coverage = UpstreamCoverage.Covered)]
    public void Classifies_known_and_future_gemini_model_ids()
    {
        AssertCapabilities("gemini-pro", false, false, false);
        AssertCapabilities("gemini-pro-vision", false, false, false);
        AssertCapabilities("gemini-1.5-flash", false, false, false);
        AssertCapabilities("gemini-robotics-er-1.5-preview", false, false, false);
        AssertCapabilities("gemini-2.0-flash", true, false, false);
        AssertCapabilities("gemini-2.5-flash", true, true, false);
        AssertCapabilities("gemini-3.1-pro-preview", true, true, true);
        AssertCapabilities("gemini-99-pro-preview", true, true, true);
        AssertCapabilities("gemini-ultra-latest", true, true, true);
        AssertCapabilities("nano-banana-pro-preview", true, false, false);
        AssertCapabilities("eu.gemini-2.5-flash", true, true, false);
        AssertCapabilities("au.gemini-3.5-flash", true, true, true);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > flat paths::should accumulate a simple string arg with willContinue", Coverage = UpstreamCoverage.Covered)]
    public void Accumulates_a_continued_string()
    {
        var accumulator = new GoogleJsonAccumulator();
        var update = accumulator.Process(new[] { Arg("$.location", "Boston", willContinue: true) });
        AssertUpdate(update, "{\"location\":\"Boston\"}", "{\"location\":\"Boston");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > flat paths::should continue a string arg across multiple chunks", Coverage = UpstreamCoverage.Covered)]
    public void Continues_a_string_across_chunks()
    {
        var accumulator = new GoogleJsonAccumulator();
        accumulator.Process(new[] { Arg("$.location", "Boston", willContinue: true) });
        var update = accumulator.Process(new[] { Arg("$.location", ", MA") });
        AssertUpdate(update, "{\"location\":\"Boston, MA\"}", ", MA");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > flat paths::should accumulate a complete string arg (no willContinue)", Coverage = UpstreamCoverage.Covered)]
    public void Accumulates_a_complete_string()
    {
        var update = new GoogleJsonAccumulator().Process(new[] { Arg("$.location", "Boston") });
        AssertUpdate(update, "{\"location\":\"Boston\"}", "{\"location\":\"Boston\"");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > flat paths::should accumulate a number arg", Coverage = UpstreamCoverage.Covered)]
    public void Accumulates_a_number()
    {
        var update = new GoogleJsonAccumulator().Process(new[] { Arg("$.brightness", number: 50) });
        AssertUpdate(update, "{\"brightness\":50}", "{\"brightness\":50");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > flat paths::should accumulate a boolean arg", Coverage = UpstreamCoverage.Covered)]
    public void Accumulates_a_boolean()
    {
        var update = new GoogleJsonAccumulator().Process(new[] { Arg("$.enabled", flag: true) });
        AssertUpdate(update, "{\"enabled\":true}", "{\"enabled\":true");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > flat paths::should accumulate a null arg", Coverage = UpstreamCoverage.Covered)]
    public void Accumulates_null()
    {
        var update = new GoogleJsonAccumulator().Process(new[] { Arg("$.nickname", hasNull: true) });
        AssertUpdate(update, "{\"nickname\":null}", "{\"nickname\":null");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > flat paths::should accumulate multiple args with commas between them", Coverage = UpstreamCoverage.Covered)]
    public void Separates_flat_args_with_commas()
    {
        var accumulator = new GoogleJsonAccumulator();
        AssertUpdate(accumulator.Process(new[] { Arg("$.brightness", number: 50) }), "{\"brightness\":50}", "{\"brightness\":50");
        AssertUpdate(accumulator.Process(new[] { Arg("$.enabled", flag: true) }), "{\"brightness\":50,\"enabled\":true}", ",\"enabled\":true");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > flat paths::should accumulate multiple args in a single call", Coverage = UpstreamCoverage.Covered)]
    public void Accumulates_several_args_in_one_call()
    {
        var update = new GoogleJsonAccumulator().Process(new[] { Arg("$.brightness", number: 50), Arg("$.enabled", flag: false), Arg("$.nickname", hasNull: true) });
        AssertUpdate(update, "{\"brightness\":50,\"enabled\":false,\"nickname\":null}", "{\"brightness\":50,\"enabled\":false,\"nickname\":null");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > flat paths::should escape special characters in continued strings", Coverage = UpstreamCoverage.Covered)]
    public void Escapes_continued_string_fragments()
    {
        var accumulator = new GoogleJsonAccumulator();
        accumulator.Process(new[] { Arg("$.query", "Boston \"Lo", willContinue: true) });
        var update = accumulator.Process(new[] { Arg("$.query", "gan\"") });
        AssertUpdate(update, "{\"query\":\"Boston \\\"Logan\\\"\"}", "gan\\\"");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > flat paths::should skip args with empty jsonPath after stripping $. prefix", Coverage = UpstreamCoverage.Covered)]
    public void Skips_an_empty_json_path()
    {
        var update = new GoogleJsonAccumulator().Process(new[] { Arg("$.", "ignored") });
        AssertUpdate(update, "{}", "");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > flat paths::should skip args with no resolvable value", Coverage = UpstreamCoverage.Covered)]
    public void Skips_args_without_a_value()
    {
        var update = new GoogleJsonAccumulator().Process(new[] { Arg("$.something") });
        AssertUpdate(update, "{}", "");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > flat paths::should return empty textDelta for empty partialArgs array", Coverage = UpstreamCoverage.Covered)]
    public void Returns_an_empty_delta_for_no_args()
    {
        var update = new GoogleJsonAccumulator().Process(Array.Empty<GooglePartialArgument>());
        AssertUpdate(update, "{}", "");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > prototype pollution protection::should not pollute Object.prototype through __proto__ path segments", Coverage = UpstreamCoverage.Covered)]
    public void Stores_proto_as_an_ordinary_property()
    {
        var update = new GoogleJsonAccumulator().Process(new[] { Arg("$.__proto__.pollutedByGoogleAccumulatorTest", "true") });
        GoogleUpstream.JsonEqual(update.Current, "{\"__proto__\":{\"pollutedByGoogleAccumulatorTest\":\"true\"}}");
        Assert.Equal("true", update.Current["__proto__"]!["pollutedByGoogleAccumulatorTest"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > prototype pollution protection::should not pollute Object.prototype through constructor.prototype path segments", Coverage = UpstreamCoverage.Covered)]
    public void Stores_constructor_prototype_as_data()
    {
        var update = new GoogleJsonAccumulator().Process(new[] { Arg("$.constructor.prototype.pollutedByGoogleConstructorTest", "true") });
        GoogleUpstream.JsonEqual(update.Current, "{\"constructor\":{\"prototype\":{\"pollutedByGoogleConstructorTest\":\"true\"}}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > nested paths::should build nested object from dotted jsonPath", Coverage = UpstreamCoverage.Covered)]
    public void Builds_a_nested_object()
    {
        var update = new GoogleJsonAccumulator().Process(new[] { Arg("$.recipe.name", "Lasagna") });
        AssertUpdate(update, "{\"recipe\":{\"name\":\"Lasagna\"}}", "{\"recipe\":{\"name\":\"Lasagna\"");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > nested paths::should build nested object with array from indexed jsonPath", Coverage = UpstreamCoverage.Covered)]
    public void Builds_a_nested_array_object()
    {
        var accumulator = new GoogleJsonAccumulator();
        AssertUpdate(accumulator.Process(new[] { Arg("$.recipe.ingredients[0].amount", "16 oz") }), "{\"recipe\":{\"ingredients\":[{\"amount\":\"16 oz\"}]}}", "{\"recipe\":{\"ingredients\":[{\"amount\":\"16 oz\"");
        AssertUpdate(accumulator.Process(new[] { Arg("$.recipe.ingredients[0].name", "Lasagna noodles") }), "{\"recipe\":{\"ingredients\":[{\"amount\":\"16 oz\",\"name\":\"Lasagna noodles\"}]}}", ",\"name\":\"Lasagna noodles\"");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > nested paths::should accumulate multiple array elements across chunks", Coverage = UpstreamCoverage.Covered)]
    public void Accumulates_array_elements_and_keeps_the_concatenation_invariant()
    {
        var accumulator = new GoogleJsonAccumulator();
        var deltas = new List<string>();
        deltas.Add(accumulator.Process(new[] { Arg("$.recipe.ingredients[0].amount", "16 oz") }).TextDelta);
        deltas.Add(accumulator.Process(new[] { Arg("$.recipe.ingredients[0].name", "Noodles") }).TextDelta);
        var third = accumulator.Process(new[] { Arg("$.recipe.ingredients[1].amount", "1 lb") });
        deltas.Add(third.TextDelta);
        Assert.Equal("},{\"amount\":\"1 lb\"", third.TextDelta);
        var fourth = accumulator.Process(new[] { Arg("$.recipe.ingredients[1].name", "Beef") });
        deltas.Add(fourth.TextDelta);
        Assert.Equal(",\"name\":\"Beef\"", fourth.TextDelta);
        GoogleUpstream.JsonEqual(fourth.Current, "{\"recipe\":{\"ingredients\":[{\"amount\":\"16 oz\",\"name\":\"Noodles\"},{\"amount\":\"1 lb\",\"name\":\"Beef\"}]}}");
        var final = accumulator.Finalize();
        deltas.Add(final.ClosingDelta);
        Assert.Equal(final.FinalJson, string.Concat(deltas));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > nested paths::should handle string continuation on nested paths", Coverage = UpstreamCoverage.Covered)]
    public void Continues_a_string_inside_an_array()
    {
        var accumulator = new GoogleJsonAccumulator();
        var start = accumulator.Process(new[] { Arg("$.recipe.steps[0]", "Preheat oven", willContinue: true) });
        Assert.Equal("{\"recipe\":{\"steps\":[\"Preheat oven", start.TextDelta);
        var update = accumulator.Process(new[] { Arg("$.recipe.steps[0]", " to 375\u00b0F.") });
        AssertUpdate(update, "{\"recipe\":{\"steps\":[\"Preheat oven to 375\u00b0F.\"]}}", " to 375\u00b0F.");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > nested paths::should handle mixed nested and flat paths", Coverage = UpstreamCoverage.Covered)]
    public void Mixes_flat_and_nested_paths()
    {
        var accumulator = new GoogleJsonAccumulator();
        Assert.Equal("{\"location\":\"Boston\"", accumulator.Process(new[] { Arg("$.location", "Boston") }).TextDelta);
        var details = accumulator.Process(new[] { Arg("$.details.zip", "02101") });
        AssertUpdate(details, "{\"location\":\"Boston\",\"details\":{\"zip\":\"02101\"}}", ",\"details\":{\"zip\":\"02101\"");
        var final = accumulator.Finalize();
        Assert.Equal("}}", final.ClosingDelta);
        Assert.Equal("{\"location\":\"Boston\",\"details\":{\"zip\":\"02101\"}}", final.FinalJson);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > nested paths::should handle array elements that are direct string values", Coverage = UpstreamCoverage.Covered)]
    public void Streams_array_string_elements()
    {
        var accumulator = new GoogleJsonAccumulator();
        Assert.Equal("{\"steps\":[\"Step one\"", accumulator.Process(new[] { Arg("$.steps[0]", "Step one") }).TextDelta);
        var second = accumulator.Process(new[] { Arg("$.steps[1]", "Step two") });
        AssertUpdate(second, "{\"steps\":[\"Step one\",\"Step two\"]}", ",\"Step two\"");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > nested paths::should handle deeply nested paths", Coverage = UpstreamCoverage.Covered)]
    public void Streams_a_deeply_nested_path()
    {
        var accumulator = new GoogleJsonAccumulator();
        var update = accumulator.Process(new[] { Arg("$.a.b.c.d", "deep") });
        AssertUpdate(update, "{\"a\":{\"b\":{\"c\":{\"d\":\"deep\"}}}}", "{\"a\":{\"b\":{\"c\":{\"d\":\"deep\"");
        var final = accumulator.Finalize();
        Assert.Equal("}}}}", final.ClosingDelta);
        Assert.Equal("{\"a\":{\"b\":{\"c\":{\"d\":\"deep\"}}}}", final.FinalJson);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > finalize::should produce closing delta for a continued string", Coverage = UpstreamCoverage.Covered)]
    public void Closes_a_continued_string()
    {
        var accumulator = new GoogleJsonAccumulator();
        accumulator.Process(new[] { Arg("$.location", "Boston", willContinue: true) });
        var final = accumulator.Finalize();
        Assert.Equal("\"}", final.ClosingDelta);
        Assert.Equal("{\"location\":\"Boston\"}", final.FinalJson);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > finalize::should produce closing delta for a complete string", Coverage = UpstreamCoverage.Covered)]
    public void Closes_a_complete_string()
    {
        var accumulator = new GoogleJsonAccumulator();
        accumulator.Process(new[] { Arg("$.location", "Boston") });
        var final = accumulator.Finalize();
        Assert.Equal("}", final.ClosingDelta);
        Assert.Equal("{\"location\":\"Boston\"}", final.FinalJson);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > finalize::should produce closing delta for multiple args", Coverage = UpstreamCoverage.Covered)]
    public void Closes_multiple_args()
    {
        var accumulator = new GoogleJsonAccumulator();
        accumulator.Process(new[] { Arg("$.brightness", number: 50), Arg("$.enabled", flag: true) });
        var final = accumulator.Finalize();
        Assert.Equal("}", final.ClosingDelta);
        Assert.Equal("{\"brightness\":50,\"enabled\":true}", final.FinalJson);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > finalize::should produce closing delta for continued string with continuation", Coverage = UpstreamCoverage.Covered)]
    public void Closes_a_continued_string_after_more_text()
    {
        var accumulator = new GoogleJsonAccumulator();
        accumulator.Process(new[] { Arg("$.location", "Boston", willContinue: true) });
        accumulator.Process(new[] { Arg("$.location", ", MA") });
        var final = accumulator.Finalize();
        Assert.Equal("\"}", final.ClosingDelta);
        Assert.Equal("{\"location\":\"Boston, MA\"}", final.FinalJson);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > finalize::should handle empty accumulator", Coverage = UpstreamCoverage.Covered)]
    public void Finalizes_an_empty_object()
    {
        var final = new GoogleJsonAccumulator().Finalize();
        Assert.Equal("{}", final.ClosingDelta);
        Assert.Equal("{}", final.FinalJson);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > finalize::should finalize nested structure to proper JSON", Coverage = UpstreamCoverage.Covered)]
    public void Finalizes_nested_json()
    {
        var accumulator = new GoogleJsonAccumulator();
        accumulator.Process(new[] { Arg("$.recipe.ingredients[0].name", "Noodles") });
        accumulator.Process(new[] { Arg("$.recipe.name", "Lasagna") });
        var parsed = JsonNode.Parse(accumulator.Finalize().FinalJson);
        GoogleUpstream.JsonEqual(parsed, "{\"recipe\":{\"ingredients\":[{\"name\":\"Noodles\"}],\"name\":\"Lasagna\"}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > finalize::should finalize nested arrays with string continuation", Coverage = UpstreamCoverage.Covered)]
    public void Finalizes_continued_array_strings()
    {
        var accumulator = new GoogleJsonAccumulator();
        accumulator.Process(new[] { Arg("$.recipe.steps[0]", "Preheat", willContinue: true) });
        accumulator.Process(new[] { Arg("$.recipe.steps[0]", " oven.") });
        accumulator.Process(new[] { Arg("$.recipe.steps[1]", "Cook.") });
        var parsed = JsonNode.Parse(accumulator.Finalize().FinalJson);
        GoogleUpstream.JsonEqual(parsed, "{\"recipe\":{\"steps\":[\"Preheat oven.\",\"Cook.\"]}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > concatenation invariant::flat args: concatenated deltas + closingDelta === JSON.stringify", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_the_flat_concatenation_invariant()
    {
        var accumulator = new GoogleJsonAccumulator();
        var deltas = new List<string>
        {
            accumulator.Process(new[] { Arg("$.brightness", number: 50) }).TextDelta,
            accumulator.Process(new[] { Arg("$.enabled", flag: true) }).TextDelta,
            accumulator.Process(new[] { Arg("$.name", "test") }).TextDelta,
        };
        var final = accumulator.Finalize();
        deltas.Add(final.ClosingDelta);
        Assert.Equal(final.FinalJson, string.Concat(deltas));
        GoogleUpstream.JsonEqual(JsonNode.Parse(final.FinalJson), "{\"brightness\":50,\"enabled\":true,\"name\":\"test\"}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > concatenation invariant::nested args: concatenated deltas + closingDelta === JSON.stringify", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_the_nested_concatenation_invariant()
    {
        var accumulator = new GoogleJsonAccumulator();
        var deltas = new List<string>
        {
            accumulator.Process(new[] { Arg("$.recipe.ingredients[0].amount", "16 oz") }).TextDelta,
            accumulator.Process(new[] { Arg("$.recipe.ingredients[0].name", "Noodles") }).TextDelta,
            accumulator.Process(new[] { Arg("$.recipe.ingredients[1].amount", "1 lb") }).TextDelta,
            accumulator.Process(new[] { Arg("$.recipe.ingredients[1].name", "Beef") }).TextDelta,
            accumulator.Process(new[] { Arg("$.recipe.name", "Lasagna") }).TextDelta,
            accumulator.Process(new[] { Arg("$.recipe.steps[0]", "Preheat", willContinue: true) }).TextDelta,
            accumulator.Process(new[] { Arg("$.recipe.steps[0]", " oven.") }).TextDelta,
            accumulator.Process(new[] { Arg("$.recipe.steps[1]", "Cook.") }).TextDelta,
        };
        var final = accumulator.Finalize();
        deltas.Add(final.ClosingDelta);
        Assert.Equal(final.FinalJson, string.Concat(deltas));
        GoogleUpstream.JsonEqual(JsonNode.Parse(final.FinalJson), "{\"recipe\":{\"ingredients\":[{\"amount\":\"16 oz\",\"name\":\"Noodles\"},{\"amount\":\"1 lb\",\"name\":\"Beef\"}],\"name\":\"Lasagna\",\"steps\":[\"Preheat oven.\",\"Cook.\"]}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > concatenation invariant::willContinue strings: concatenated deltas + closingDelta === JSON.stringify", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_the_continued_string_concatenation_invariant()
    {
        var accumulator = new GoogleJsonAccumulator();
        var deltas = new List<string>
        {
            accumulator.Process(new[] { Arg("$.location", "Bos", willContinue: true) }).TextDelta,
            accumulator.Process(new[] { Arg("$.location", "ton") }).TextDelta,
            accumulator.Process(new[] { Arg("$.count", number: 42) }).TextDelta,
        };
        var final = accumulator.Finalize();
        deltas.Add(final.ClosingDelta);
        Assert.Equal(final.FinalJson, string.Concat(deltas));
    }

    private static void AssertCapabilities(string modelId, bool tools, bool fileSearch, bool gemini3)
    {
        var capabilities = GoogleModelCapability.Get(modelId);
        Assert.Equal(tools, capabilities.SupportsGemini2Tools);
        Assert.Equal(fileSearch, capabilities.SupportsFileSearch);
        Assert.Equal(gemini3, capabilities.UsesGemini3Features);
    }

    private static GooglePartialArgument Arg(string path, string? text = null, bool? willContinue = null, double? number = null, bool? flag = null, bool hasNull = false)
    {
        var argument = new GooglePartialArgument { JsonPath = path, HasNull = hasNull, WillContinue = willContinue, NumberValue = number, BoolValue = flag };
        if (text != null)
        {
            argument.HasString = true;
            argument.StringValue = text;
        }

        return argument;
    }

    private static void AssertUpdate(GooglePartialArgumentsUpdate update, string json, string delta)
    {
        GoogleUpstream.JsonEqual(update.Current, json);
        Assert.Equal(delta, update.TextDelta);
    }
}
