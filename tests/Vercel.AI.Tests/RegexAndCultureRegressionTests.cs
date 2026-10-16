// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.Anthropic;
using Vercel.AI.Google;
using Vercel.AI.Mcp;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Xai;

namespace Vercel.AI.Tests;

/// <summary>
/// Regression tests for regexes that must match like JS: <c>$</c> rejects a trailing newline, and <c>\d</c> and <c>\w</c> are ASCII.
/// Upstream has no <c>it</c> for these cases, so none of them carries an upstream id.
/// </summary>
public sealed class RegexAndCultureRegressionTests
{
    private const string ArabicIndicDigits = "١٢";

    [Theory]
    [InlineData("abc\n")]
    [InlineData("a١")]
    [InlineData("é")]
    public void Anthropic_batch_request_id_rejects_trailing_newline_and_non_ascii(string id)
    {
        Assert.Throws<AiSdkException>(() => AnthropicBatch.ValidateRequestId(id));
    }

    [Fact]
    public void Anthropic_batch_request_id_accepts_a_plain_id()
    {
        AnthropicBatch.ValidateRequestId("req_1-A");
    }

    [Theory]
    [InlineData("https://youtu.be/abc\n")]
    [InlineData("https://www.youtube.com/watch?v=abc\n")]
    [InlineData("https://youtu.be/abé")]
    [InlineData("https://youtu.be/" + ArabicIndicDigits)]
    public void Google_file_urls_reject_trailing_newline_and_non_ascii(string url)
    {
        Assert.False(GoogleFileUrls.IsSupported(url));
    }

    [Theory]
    [InlineData("https://youtu.be/abc_-1")]
    [InlineData("https://www.youtube.com/watch?v=abc_-1&t=10")]
    public void Google_file_urls_accept_youtube_urls(string url)
    {
        Assert.True(GoogleFileUrls.IsSupported(url));
    }

    [Theory]
    [InlineData("X\n")]
    [InlineData("Xé")]
    public void Mcp_header_name_rejects_trailing_newline_and_non_ascii(string name)
    {
        var schema = "{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"string\",\"x-mcp-header\":" + JsonSerializer.Serialize(name) + "}}}";
        using var document = JsonDocument.Parse(schema);
        Assert.False(McpToolHeaders.GetBindings(document.RootElement).Success);
    }

    [Theory]
    [InlineData("grok-4.20-reasoning\n", true)]
    [InlineData("grok-4.20-non-reasoning\n", true)]
    [InlineData("grok-4.20-٠١٢٣-reasoning", true)]
    [InlineData("grok-4.20-0309-reasoning", false)]
    public void Xai_reasoning_effort_matches_only_exact_ascii_ids(string modelId, bool supported)
    {
        Assert.Equal(supported, XaiReasoningEffort.Supports(modelId));
    }

    [Theory]
    [InlineData("o3\n")]
    [InlineData("o٣")]
    [InlineData("o99999999999")]
    public void OpenAI_capabilities_treat_malformed_o_series_ids_as_non_reasoning(string modelId)
    {
        Assert.False(OpenAILanguageModelCapabilities.ForModel(modelId).IsReasoningModel);
    }

    [Theory]
    [InlineData("gpt-5\n")]
    [InlineData("gpt-5.1\n")]
    [InlineData("gpt-٥")]
    [InlineData("gpt-99999999999")]
    [InlineData("gpt-5.99999999999")]
    public void OpenAI_capabilities_treat_malformed_gpt_ids_as_non_reasoning(string modelId)
    {
        Assert.False(OpenAILanguageModelCapabilities.ForModel(modelId).IsReasoningModel);
    }

    [Fact]
    public void OpenAI_capabilities_still_classify_valid_ids()
    {
        Assert.True(OpenAILanguageModelCapabilities.ForModel("o3-mini").IsReasoningModel);
        Assert.True(OpenAILanguageModelCapabilities.ForModel("gpt-5.1").IsReasoningModel);
    }

    [Theory]
    [InlineData("gemini-3-pro\n")]
    [InlineData("gemini-٣-pro")]
    [InlineData("gemini-99999999999-pro")]
    [InlineData("gemini-99999999999.1-flash")]
    [InlineData("gemini-3.99999999999-flash")]
    [InlineData("gemini-3.٧-flash")]
    public void Google_thinking_minimum_level_ignores_malformed_versions(string modelId)
    {
        Assert.Equal("minimal", MinimumLevel(modelId));
    }

    [Fact]
    public void Google_thinking_minimum_level_still_reads_valid_versions()
    {
        Assert.Equal("low", MinimumLevel("gemini-3-pro"));
        Assert.Equal("low", MinimumLevel("gemini-3.7-flash"));
        Assert.Equal("minimal", MinimumLevel("gemini-3.5-flash"));
    }

    [Theory]
    [InlineData("claude-3\n", false)]
    [InlineData("claude-instant\n", false)]
    [InlineData("claude-3", true)]
    [InlineData("claude-instant", true)]
    public void Anthropic_legacy_model_ids_must_end_at_the_end_of_the_string(string modelId, bool legacy)
    {
        // Legacy ids get a 4096 token limit; an unrecognized claude id gets 128000.
        Assert.Equal(legacy ? 4096 : 128000, AnthropicModelCapabilities.Get(modelId).MaxOutputTokens);
    }

    [Theory]
    [InlineData("&#12\n;")]
    [InlineData("&#" + ArabicIndicDigits + ";")]
    [InlineData("&#x1\n;")]
    [InlineData("&#x١;")]
    public void QuiverAI_rejects_malformed_character_references(string value)
    {
        Assert.False(HasValidXmlReferences(value));
    }

    [Theory]
    [InlineData("&#12;")]
    [InlineData("&#12345678901;")]
    [InlineData("&#x1F600;")]
    public void QuiverAI_accepts_valid_character_references(string value)
    {
        Assert.True(HasValidXmlReferences(value));
    }

    [Fact]
    public void QuiverAI_image_reference_scheme_is_ascii_only()
    {
        Assert.Throws<InvalidArgumentException>(() => QuiverAI.QuiverAIImageReference.Prepare("a١://example.com/x.png"));
    }

    [Fact]
    public void QuiverAI_self_closing_svg_check_stops_at_the_match_timeout()
    {
        // Quadratic input for the self-closing check: many "<svg" openers and no ">".
        var data = System.Text.Encoding.UTF8.GetBytes("<svg>" + string.Concat(Enumerable.Repeat("<svg ", 20000)));
        var watch = System.Diagnostics.Stopwatch.StartNew();
        Assert.Throws<InvalidArgumentException>(() => QuiverAI.QuiverAIImageReference.Prepare(data));
        Assert.True(watch.Elapsed < TimeSpan.FromSeconds(30), watch.Elapsed.ToString());
    }

    [Fact]
    public void Url_support_treats_a_pattern_timeout_as_no_match()
    {
        var slow = new Regex("^(a+)+$", RegexOptions.None, TimeSpan.FromMilliseconds(20));
        var supported = new Dictionary<string, IReadOnlyList<Regex>> { ["*"] = new[] { slow } };
        Assert.False(UrlSupport.IsUrlSupported("image/png", new string('a', 40) + "!", supported));
    }

    [Fact]
    public void Retry_after_ms_uses_invariant_culture()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        // U+2212 is the minus sign of the sv-SE culture. It must not parse as a negative number.
        response.Headers.TryAddWithoutValidation("retry-after-ms", "−5");
        Assert.Null(WithCulture("sv-SE", () => ReadRetryAfter(response)));

        using var valid = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        valid.Headers.TryAddWithoutValidation("retry-after-ms", "1500");
        Assert.Equal(TimeSpan.FromMilliseconds(1500), ReadRetryAfter(valid));
    }

    [Fact]
    public void Retry_after_ms_with_ten_digits_is_ignored()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.TryAddWithoutValidation("retry-after-ms", "99999999999");
        Assert.Null(ReadRetryAfter(response));
    }

    [Theory]
    [InlineData("−5x10")]
    [InlineData("10x−5")]
    [InlineData("99999999999x10")]
    [InlineData("10x99999999999")]
    public void TogetherAI_size_split_rejects_malformed_numbers(string size)
    {
        Assert.Null(WithCulture("sv-SE", () => SplitSize(size)));
    }

    [Fact]
    public void TogetherAI_size_split_reads_a_valid_size()
    {
        Assert.Equal((1024, 768), SplitSize("1024x768"));
    }

    private static string MinimumLevel(string modelId)
    {
        var method = typeof(GoogleThinking).GetMethod("MinimumLevel", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (string)method.Invoke(null, new object[] { modelId })!;
    }

    private static bool HasValidXmlReferences(string value)
    {
        var method = typeof(QuiverAI.QuiverAIImageModel).GetMethod("HasValidXmlReferences", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (bool)method.Invoke(null, new object[] { value })!;
    }

    private static TimeSpan? ReadRetryAfter(HttpResponseMessage response)
    {
        var method = typeof(ProviderHttp).GetMethod("ReadRetryAfter", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (TimeSpan?)method.Invoke(null, new object[] { response });
    }

    private static (int Width, int Height)? SplitSize(string size)
    {
        var method = typeof(TogetherAI.TogetherAIImageModel).GetMethod("SplitSize", BindingFlags.NonPublic | BindingFlags.Static)!;
        return ((int Width, int Height)?)method.Invoke(null, new object[] { size });
    }

    private static T WithCulture<T>(string name, Func<T> action)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(name);
            return action();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
