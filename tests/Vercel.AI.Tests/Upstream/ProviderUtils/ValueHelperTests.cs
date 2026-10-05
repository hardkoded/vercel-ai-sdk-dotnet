// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text.RegularExpressions;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

public sealed class ValueHelperTests
{
    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/as-array.test.ts::asArray::returns an empty array for undefined",
        Coverage = UpstreamCoverage.Covered)]
    public void As_array_returns_empty_for_null()
    {
        Assert.Empty(ProviderValues.AsArray<string>(null));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/as-array.test.ts::asArray::wraps a single value in an array",
        Coverage = UpstreamCoverage.Covered)]
    public void As_array_wraps_one_value()
    {
        Assert.Equal(new[] { "value" }, ProviderValues.AsArray<string>("value"));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/as-array.test.ts::asArray::returns an array value unchanged",
        Coverage = UpstreamCoverage.Covered)]
    public void As_array_returns_the_same_list()
    {
        var value = new[] { "a", "b" };
        Assert.Same(value, ProviderValues.AsArray<string>(value));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/filter-nullable.test.ts::filterNullable::removes null and undefined values from a value list",
        Coverage = UpstreamCoverage.Covered)]
    public void Filter_nullable_drops_null()
    {
        Assert.Equal(new object[] { 1, 2, 3 }, ProviderValues.FilterNullable(1, null, 2, null, 3));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/filter-nullable.test.ts::filterNullable::preserves other falsy values",
        Coverage = UpstreamCoverage.Covered)]
    public void Filter_nullable_keeps_false_zero_and_empty_text()
    {
        Assert.Equal(new object[] { 0, false, "" }, ProviderValues.FilterNullable(0, false, "", null));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/remove-undefined-entries.test.ts::should remove undefined entries from record",
        Coverage = UpstreamCoverage.Covered)]
    public void Remove_undefined_drops_null_entries()
    {
        var result = ProviderValues.RemoveUndefinedEntries(new Dictionary<string, object?>
        {
            ["a"] = 1,
            ["b"] = null,
            ["c"] = "test",
            ["d"] = null,
        });
        Assert.Equal(2, result.Count);
        Assert.Equal(1, (int)result["a"]);
        Assert.Equal("test", (string)result["c"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/remove-undefined-entries.test.ts::should handle empty object",
        Coverage = UpstreamCoverage.Covered)]
    public void Remove_undefined_keeps_an_empty_record_empty()
    {
        Assert.Empty(ProviderValues.RemoveUndefinedEntries(new Dictionary<string, object?>()));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/remove-undefined-entries.test.ts::should handle object with all undefined values",
        Coverage = UpstreamCoverage.Covered)]
    public void Remove_undefined_drops_a_record_of_nulls()
    {
        var result = ProviderValues.RemoveUndefinedEntries(new Dictionary<string, object?> { ["a"] = null, ["b"] = null });
        Assert.Empty(result);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/remove-undefined-entries.test.ts::should remove null values",
        Coverage = UpstreamCoverage.Covered)]
    public void Remove_undefined_drops_null_and_keeps_text()
    {
        var result = ProviderValues.RemoveUndefinedEntries(new Dictionary<string, object?>
        {
            ["a"] = null,
            ["b"] = null,
            ["c"] = "test",
        });
        Assert.Equal("test", (string)Assert.Single(result).Value);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/remove-undefined-entries.test.ts::should preserve falsy values except null and undefined",
        Coverage = UpstreamCoverage.Covered)]
    public void Remove_undefined_keeps_false_zero_and_empty_text()
    {
        var result = ProviderValues.RemoveUndefinedEntries(new Dictionary<string, object?>
        {
            ["a"] = false,
            ["b"] = 0,
            ["c"] = "",
            ["d"] = null,
            ["e"] = null,
        });
        Assert.False((bool)result["a"]);
        Assert.Equal(0, (int)result["b"]);
        Assert.Equal(string.Empty, (string)result["c"]);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-record.test.ts::isRecord::returns true for non-null, non-array objects",
        Coverage = UpstreamCoverage.Covered)]
    public void Is_record_accepts_reference_objects()
    {
        Assert.True(ProviderValues.IsRecord(new Dictionary<string, object?>()));
        Assert.True(ProviderValues.IsRecord(new Marker()));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-record.test.ts::isRecord::returns false for arrays, null, and primitive values",
        Coverage = UpstreamCoverage.Covered)]
    public void Is_record_rejects_lists_null_and_primitives()
    {
        Assert.False(ProviderValues.IsRecord(Array.Empty<string>()));
        Assert.False(ProviderValues.IsRecord(null));
        Assert.False(ProviderValues.IsRecord("value"));
        Assert.False(ProviderValues.IsRecord(42));
        Assert.False(ProviderValues.IsRecord(true));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-json-serializable.test.ts::isJSONSerializable::returns true for null and undefined",
        Coverage = UpstreamCoverage.Covered)]
    public void Serializable_accepts_null()
    {
        Assert.True(ProviderValues.IsJsonSerializable(null));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-json-serializable.test.ts::isJSONSerializable::returns true for primitive JSON-compatible values",
        Coverage = UpstreamCoverage.Covered)]
    public void Serializable_accepts_json_primitives()
    {
        Assert.True(ProviderValues.IsJsonSerializable("test"));
        Assert.True(ProviderValues.IsJsonSerializable(42));
        Assert.True(ProviderValues.IsJsonSerializable(true));
        Assert.True(ProviderValues.IsJsonSerializable(false));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-json-serializable.test.ts::isJSONSerializable::returns false for unsupported primitive values",
        Coverage = UpstreamCoverage.Covered)]
    public void Serializable_rejects_delegates_and_big_integers()
    {
        Assert.False(ProviderValues.IsJsonSerializable((Action)(() => { })));
        Assert.False(ProviderValues.IsJsonSerializable(new System.Numerics.BigInteger(1)));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-json-serializable.test.ts::isJSONSerializable::returns true for arrays when all values are serializable",
        Coverage = UpstreamCoverage.Covered)]
    public void Serializable_accepts_a_nested_list()
    {
        object?[] values = { "test", 42, true, null, new Dictionary<string, object?> { ["nested"] = new object?[] { "value" } } };
        Assert.True(ProviderValues.IsJsonSerializable(values));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-json-serializable.test.ts::isJSONSerializable::returns false for arrays containing non-serializable values",
        Coverage = UpstreamCoverage.Covered)]
    public void Serializable_rejects_a_list_that_contains_a_delegate()
    {
        Assert.False(ProviderValues.IsJsonSerializable(new object[] { "test", (Action)(() => { }) }));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-json-serializable.test.ts::isJSONSerializable::returns true for plain objects when all values are serializable",
        Coverage = UpstreamCoverage.Covered)]
    public void Serializable_accepts_a_string_keyed_dictionary()
    {
        var record = new Dictionary<string, object?>
        {
            ["string"] = "test",
            ["number"] = 42,
            ["boolean"] = true,
            ["nullValue"] = null,
            ["nested"] = new Dictionary<string, object?> { ["array"] = new object?[] { "value" } },
        };
        Assert.True(ProviderValues.IsJsonSerializable(record));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-json-serializable.test.ts::isJSONSerializable::returns false for plain objects containing non-serializable values",
        Coverage = UpstreamCoverage.Covered)]
    public void Serializable_rejects_a_nested_delegate()
    {
        var record = new Dictionary<string, object?>
        {
            ["nested"] = new Dictionary<string, object?> { ["callback"] = (Action)(() => { }) },
        };
        Assert.False(ProviderValues.IsJsonSerializable(record));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-json-serializable.test.ts::isJSONSerializable::returns false for non-plain objects",
        Coverage = UpstreamCoverage.Covered)]
    public void Serializable_rejects_dates_regexes_and_custom_types()
    {
        Assert.False(ProviderValues.IsJsonSerializable(DateTime.UtcNow));
        Assert.False(ProviderValues.IsJsonSerializable(new Regex("test")));
        Assert.False(ProviderValues.IsJsonSerializable(new Marker()));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/normalize-headers.test.ts::normalizeHeaders::returns empty object for undefined",
        Coverage = UpstreamCoverage.Covered)]
    public void Normalize_headers_returns_empty_for_null()
    {
        Assert.Empty(ProviderValues.NormalizeHeaders(null));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/normalize-headers.test.ts::normalizeHeaders::converts Headers instance to record",
        Coverage = UpstreamCoverage.Covered)]
    public void Normalize_headers_lowercases_names()
    {
        var headers = ProviderValues.NormalizeHeaders(new Dictionary<string, string?>
        {
            ["Authorization"] = "Bearer token",
            ["X-Feature"] = "beta",
        });
        Assert.Equal("Bearer token", headers["authorization"]);
        Assert.Equal("beta", headers["x-feature"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/normalize-headers.test.ts::normalizeHeaders::converts tuple array",
        Coverage = UpstreamCoverage.Covered)]
    public void Normalize_headers_reads_pairs_and_drops_null()
    {
        var headers = ProviderValues.NormalizeHeaders(new[]
        {
            new KeyValuePair<string, string?>("Authorization", "Bearer token"),
            new KeyValuePair<string, string?>("X-Feature", "beta"),
            new KeyValuePair<string, string?>("X-Ignore", null),
        });
        Assert.Equal(2, headers.Count);
        Assert.Equal("Bearer token", headers["authorization"]);
        Assert.Equal("beta", headers["x-feature"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/normalize-headers.test.ts::normalizeHeaders::converts plain record and filters nullish values",
        Coverage = UpstreamCoverage.Covered)]
    public void Normalize_headers_filters_null_values()
    {
        var headers = ProviderValues.NormalizeHeaders(new Dictionary<string, string?>
        {
            ["Authorization"] = "Bearer token",
            ["X-Feature"] = null,
            ["Content-Type"] = "application/json",
        });
        Assert.Equal("Bearer token", headers["authorization"]);
        Assert.Equal("application/json", headers["content-type"]);
        Assert.False(headers.ContainsKey("x-feature"));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/normalize-headers.test.ts::normalizeHeaders::handles empty Headers instance",
        Coverage = UpstreamCoverage.Covered)]
    public void Normalize_headers_handles_an_empty_sequence()
    {
        Assert.Empty(ProviderValues.NormalizeHeaders(Array.Empty<KeyValuePair<string, string?>>()));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/normalize-headers.test.ts::normalizeHeaders::converts uppercase keys to lowercase",
        Coverage = UpstreamCoverage.Covered)]
    public void Normalize_headers_lowercases_uppercase_names()
    {
        var headers = ProviderValues.NormalizeHeaders(new Dictionary<string, string?>
        {
            ["CONTENT-TYPE"] = "application/json",
            ["X-CUSTOM-HEADER"] = "test-value",
        });
        Assert.Equal("application/json", headers["content-type"]);
        Assert.Equal("test-value", headers["x-custom-header"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/with-user-agent-suffix.test.ts::withUserAgentSuffix::should create a new user-agent header when no existing user-agent exists",
        Coverage = UpstreamCoverage.Covered)]
    public void User_agent_suffix_creates_the_header()
    {
        var headers = ProviderValues.WithUserAgentSuffix(
            new Dictionary<string, string?>
            {
                ["content-type"] = "application/json",
                ["authorization"] = "Bearer token123",
            },
            "ai-sdk/0.0.0-test",
            "provider/test-openai");
        Assert.Equal("ai-sdk/0.0.0-test provider/test-openai", headers["user-agent"]);
        Assert.Equal("application/json", headers["content-type"]);
        Assert.Equal("Bearer token123", headers["authorization"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/with-user-agent-suffix.test.ts::withUserAgentSuffix::should append suffix parts to existing user-agent header",
        Coverage = UpstreamCoverage.Covered)]
    public void User_agent_suffix_appends_to_the_existing_value()
    {
        var headers = ProviderValues.WithUserAgentSuffix(
            new Dictionary<string, string?>
            {
                ["user-agent"] = "TestApp/0.0.0-test",
                ["accept"] = "application/json",
            },
            "ai-sdk/0.0.0-test",
            "provider/test-anthropic");
        Assert.Equal("TestApp/0.0.0-test ai-sdk/0.0.0-test provider/test-anthropic", headers["user-agent"]);
        Assert.Equal("application/json", headers["accept"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/with-user-agent-suffix.test.ts::withUserAgentSuffix::should automatically remove undefined entries from headers",
        Coverage = UpstreamCoverage.Covered)]
    public void User_agent_suffix_drops_null_headers()
    {
        var headers = ProviderValues.WithUserAgentSuffix(
            new Dictionary<string, string?>
            {
                ["content-type"] = "application/json",
                ["authorization"] = null,
                ["user-agent"] = "TestApp/0.0.0-test",
                ["accept"] = "application/json",
                ["cache-control"] = null,
            },
            "ai-sdk/0.0.0-test");
        Assert.Equal("TestApp/0.0.0-test ai-sdk/0.0.0-test", headers["user-agent"]);
        Assert.False(headers.ContainsKey("authorization"));
        Assert.False(headers.ContainsKey("cache-control"));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/with-user-agent-suffix.test.ts::withUserAgentSuffix::should preserve headers when given a Headers instance",
        Coverage = UpstreamCoverage.Covered)]
    public void User_agent_suffix_preserves_other_headers()
    {
        var headers = ProviderValues.WithUserAgentSuffix(
            new Dictionary<string, string?>
            {
                ["Authorization"] = "Bearer token123",
                ["X-Custom"] = "value",
            },
            "ai-sdk/0.0.0-test");
        Assert.Equal("Bearer token123", headers["authorization"]);
        Assert.Equal("value", headers["x-custom"]);
        Assert.Equal("ai-sdk/0.0.0-test", headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/with-user-agent-suffix.test.ts::withUserAgentSuffix::should handle array header entries",
        Coverage = UpstreamCoverage.Covered)]
    public void User_agent_suffix_reads_header_pairs()
    {
        var headers = ProviderValues.WithUserAgentSuffix(
            new[]
            {
                new KeyValuePair<string, string?>("Authorization", "Bearer token123"),
                new KeyValuePair<string, string?>("X-Feature", "alpha"),
            },
            "ai-sdk/0.0.0-test");
        Assert.Equal("Bearer token123", headers["authorization"]);
        Assert.Equal("alpha", headers["x-feature"]);
        Assert.Equal("ai-sdk/0.0.0-test", headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/without-trailing-slash.test.ts::withoutTrailingSlash::removes a trailing slash",
        Coverage = UpstreamCoverage.Covered)]
    public void Without_trailing_slash_removes_one_slash()
    {
        Assert.Equal("https://example.com", ProviderValues.WithoutTrailingSlash("https://example.com/"));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/without-trailing-slash.test.ts::withoutTrailingSlash::returns undefined when the URL is undefined",
        Coverage = UpstreamCoverage.Covered)]
    public void Without_trailing_slash_returns_null_for_null()
    {
        Assert.Null(ProviderValues.WithoutTrailingSlash(null));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/without-trailing-slash.test.ts::withoutTrailingSlash::preserves an empty string",
        Coverage = UpstreamCoverage.Covered)]
    public void Without_trailing_slash_keeps_an_empty_string()
    {
        Assert.Equal(string.Empty, ProviderValues.WithoutTrailingSlash(string.Empty));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/validate-base-url.test.ts::validateBaseURL::returns a valid baseURL",
        Coverage = UpstreamCoverage.Covered)]
    public void Validate_base_url_returns_the_url()
    {
        Assert.Equal("https://example.com/", ProviderValues.ValidateBaseUrl("https://example.com/"));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/validate-base-url.test.ts::validateBaseURL::returns undefined when the baseURL is undefined",
        Coverage = UpstreamCoverage.Covered)]
    public void Validate_base_url_returns_null_for_null()
    {
        Assert.Null(ProviderValues.ValidateBaseUrl(null));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/validate-base-url.test.ts::validateBaseURL::throws an InvalidArgumentError for an empty baseURL",
        Coverage = UpstreamCoverage.Covered)]
    public void Validate_base_url_rejects_empty_and_whitespace()
    {
        foreach (var value in new[] { string.Empty, "   " })
        {
            var error = Assert.Throws<ArgumentException>(() => ProviderValues.ValidateBaseUrl(value));
            Assert.Equal("baseURL", error.ParamName);
            Assert.Contains("baseURL must be a non-empty string.", error.Message);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/strip-file-extension.test.ts::stripFileExtension::should strip the extension from a filename",
        Coverage = UpstreamCoverage.Covered)]
    public void Strip_file_extension_removes_the_suffix()
    {
        Assert.Equal("report", ProviderValues.StripFileExtension("report.pdf"));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/strip-file-extension.test.ts::stripFileExtension::should return the input when there is no extension",
        Coverage = UpstreamCoverage.Covered)]
    public void Strip_file_extension_keeps_a_name_without_a_dot()
    {
        Assert.Equal("report", ProviderValues.StripFileExtension("report"));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/strip-file-extension.test.ts::stripFileExtension::should strip all extension segments for multi-dot filenames",
        Coverage = UpstreamCoverage.Covered)]
    public void Strip_file_extension_cuts_at_the_first_dot()
    {
        Assert.Equal("archive", ProviderValues.StripFileExtension("archive.tar.gz"));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/strip-file-extension.test.ts::stripFileExtension::should strip a trailing dot",
        Coverage = UpstreamCoverage.Covered)]
    public void Strip_file_extension_removes_a_trailing_dot()
    {
        Assert.Equal("report", ProviderValues.StripFileExtension("report."));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-same-origin.test.ts::isSameOrigin::returns true for identical origins (ignoring path/query)",
        Coverage = UpstreamCoverage.Covered)]
    public void Same_origin_ignores_path_and_query()
    {
        Assert.True(ProviderValues.IsSameOrigin("https://api.example.com/v1/file", "https://api.example.com"));
        Assert.True(ProviderValues.IsSameOrigin("https://api.example.com/a?x=1", "https://api.example.com/b"));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-same-origin.test.ts::isSameOrigin::returns false for a different host",
        Coverage = UpstreamCoverage.Covered)]
    public void Same_origin_rejects_a_different_host()
    {
        Assert.False(ProviderValues.IsSameOrigin("https://cdn.evil.com/file", "https://api.example.com"));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-same-origin.test.ts::isSameOrigin::returns false for a different scheme or port",
        Coverage = UpstreamCoverage.Covered)]
    public void Same_origin_rejects_a_different_scheme_or_port()
    {
        Assert.False(ProviderValues.IsSameOrigin("http://api.example.com/file", "https://api.example.com"));
        Assert.False(ProviderValues.IsSameOrigin("https://api.example.com:8443/file", "https://api.example.com"));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-same-origin.test.ts::isSameOrigin::fails closed on invalid input",
        Coverage = UpstreamCoverage.Covered)]
    public void Same_origin_rejects_invalid_urls()
    {
        Assert.False(ProviderValues.IsSameOrigin("not-a-url", "https://api.example.com"));
        Assert.False(ProviderValues.IsSameOrigin("https://api.example.com/file", "not-a-url"));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/media-type-to-extension.test.ts::mediaTypeToExtension()::should map %s to %s",
        Coverage = UpstreamCoverage.Covered)]
    public void Media_type_maps_audio_subtypes()
    {
        Assert.Equal("mp3", ProviderValues.MediaTypeToExtension("audio/mpeg"));
        Assert.Equal("mp3", ProviderValues.MediaTypeToExtension("audio/mp3"));
        Assert.Equal("wav", ProviderValues.MediaTypeToExtension("audio/wav"));
        Assert.Equal("wav", ProviderValues.MediaTypeToExtension("audio/x-wav"));
        Assert.Equal("webm", ProviderValues.MediaTypeToExtension("audio/webm"));
        Assert.Equal("ogg", ProviderValues.MediaTypeToExtension("audio/ogg"));
        Assert.Equal("ogg", ProviderValues.MediaTypeToExtension("audio/opus"));
        Assert.Equal("m4a", ProviderValues.MediaTypeToExtension("audio/mp4"));
        Assert.Equal("m4a", ProviderValues.MediaTypeToExtension("audio/x-m4a"));
        Assert.Equal("flac", ProviderValues.MediaTypeToExtension("audio/flac"));
        Assert.Equal("aac", ProviderValues.MediaTypeToExtension("audio/aac"));
        Assert.Equal("mp3", ProviderValues.MediaTypeToExtension("AUDIO/MPEG"));
        Assert.Equal("mp3", ProviderValues.MediaTypeToExtension("AUDIO/MP3"));
        Assert.Equal(string.Empty, ProviderValues.MediaTypeToExtension("nope"));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/extract-lines.test.ts::extractLines::returns the input unchanged when neither startLine nor endLine is set",
        Coverage = UpstreamCoverage.Covered)]
    public void Extract_lines_returns_the_text_when_no_range_is_set()
    {
        Assert.Equal("a\nb\nc", ProviderValues.ExtractLines("a\nb\nc"));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/extract-lines.test.ts::extractLines::slices a 1-based inclusive range from a \\n file",
        Coverage = UpstreamCoverage.Covered)]
    public void Extract_lines_slices_an_lf_file()
    {
        Assert.Equal("b\nc", ProviderValues.ExtractLines("a\nb\nc\nd", 2, 3));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/extract-lines.test.ts::extractLines::preserves \\r\\n line endings",
        Coverage = UpstreamCoverage.Covered)]
    public void Extract_lines_keeps_crlf()
    {
        Assert.Equal("b\r\nc", ProviderValues.ExtractLines("a\r\nb\r\nc\r\nd", 2, 3));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/extract-lines.test.ts::extractLines::preserves \\r line endings",
        Coverage = UpstreamCoverage.Covered)]
    public void Extract_lines_keeps_cr()
    {
        Assert.Equal("b\rc", ProviderValues.ExtractLines("a\rb\rc\rd", 2, 3));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/extract-lines.test.ts::extractLines::treats endLine past EOF as the last line",
        Coverage = UpstreamCoverage.Covered)]
    public void Extract_lines_clamps_the_end_to_the_last_line()
    {
        Assert.Equal("b\nc", ProviderValues.ExtractLines("a\nb\nc", 2, 99));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/extract-lines.test.ts::extractLines::defaults startLine to 1 when only endLine is set",
        Coverage = UpstreamCoverage.Covered)]
    public void Extract_lines_starts_at_the_first_line()
    {
        Assert.Equal("a\nb", ProviderValues.ExtractLines("a\nb\nc", endLine: 2));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/extract-lines.test.ts::extractLines::defaults endLine to the last line when only startLine is set",
        Coverage = UpstreamCoverage.Covered)]
    public void Extract_lines_ends_at_the_last_line()
    {
        Assert.Equal("b\nc", ProviderValues.ExtractLines("a\nb\nc", startLine: 2));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/extract-lines.test.ts::extractLines::returns input unchanged when there are no line breaks",
        Coverage = UpstreamCoverage.Covered)]
    public void Extract_lines_returns_a_single_line()
    {
        Assert.Equal("one-liner", ProviderValues.ExtractLines("one-liner", 1, 1));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/uint8-utils.test.ts::convertUint8ArrayToBase64::converts a byte array to base64",
        Coverage = UpstreamCoverage.Covered)]
    public void Base64_encodes_bytes()
    {
        Assert.Equal("SGVsbG8=", ByteEncoding.ToBase64(new byte[] { 72, 101, 108, 108, 111 }));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/uint8-utils.test.ts::convertUint8ArrayToBase64::handles an empty array",
        Coverage = UpstreamCoverage.Covered)]
    public void Base64_encodes_an_empty_array()
    {
        Assert.Equal(string.Empty, ByteEncoding.ToBase64(Array.Empty<byte>()));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/uint8-utils.test.ts::convertUint8ArrayToBase64::round-trips arrays larger than a single conversion chunk",
        Coverage = UpstreamCoverage.Covered)]
    public void Base64_round_trips_a_long_buffer()
    {
        var bytes = new byte[100000];
        for (var index = 0; index < bytes.Length; index++)
        {
            bytes[index] = (byte)(index % 256);
        }

        var decoded = ByteEncoding.FromBase64(ByteEncoding.ToBase64(bytes));
        Assert.Equal(bytes, decoded);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/uint8-utils.test.ts::convertBase64ToUint8Array::converts base64 to a byte array",
        Coverage = UpstreamCoverage.Covered)]
    public void Base64_decodes_standard_text()
    {
        Assert.Equal(new byte[] { 72, 101, 108, 108, 111 }, ByteEncoding.FromBase64("SGVsbG8="));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/uint8-utils.test.ts::convertBase64ToUint8Array::supports base64url characters",
        Coverage = UpstreamCoverage.Covered)]
    public void Base64_decodes_url_characters()
    {
        Assert.Equal(new byte[] { 251, 255 }, ByteEncoding.FromBase64("-_8="));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/uint8-utils.test.ts::convertBase64ToUint8Array::handles an empty string",
        Coverage = UpstreamCoverage.Covered)]
    public void Base64_decodes_an_empty_string()
    {
        Assert.Empty(ByteEncoding.FromBase64(string.Empty));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/uint8-utils.test.ts::convertToBase64::returns base64 strings unchanged",
        Coverage = UpstreamCoverage.Covered)]
    public void Convert_to_base64_returns_text()
    {
        Assert.Equal("SGVsbG8=", ByteEncoding.ConvertToBase64("SGVsbG8="));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/uint8-utils.test.ts::convertToBase64::converts byte arrays to base64",
        Coverage = UpstreamCoverage.Covered)]
    public void Convert_to_base64_encodes_bytes()
    {
        Assert.Equal("SGVsbG8=", ByteEncoding.ConvertToBase64(new byte[] { 72, 101, 108, 108, 111 }));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/normalize-batch-request-counts.test.ts::normalizeBatchRequestCounts::returns complete, consistent counts",
        Coverage = UpstreamCoverage.Covered)]
    public void Batch_counts_accept_a_consistent_total()
    {
        var counts = ProviderValues.NormalizeBatchRequestCounts(5, 2, 2, 1);
        Assert.NotNull(counts);
        Assert.Equal(5, counts!.Total);
        Assert.Equal(2, counts.Pending);
        Assert.Equal(2, counts.Completed);
        Assert.Equal(1, counts.Failed);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/normalize-batch-request-counts.test.ts::normalizeBatchRequestCounts::returns undefined for invalid counts %#",
        Coverage = UpstreamCoverage.Covered)]
    public void Batch_counts_reject_incomplete_or_inconsistent_numbers()
    {
        Assert.Null(ProviderValues.NormalizeBatchRequestCounts(null, 0, 0, 0));
        Assert.Null(ProviderValues.NormalizeBatchRequestCounts(1, -1, 1, 1));
        Assert.Null(ProviderValues.NormalizeBatchRequestCounts(1, 0.5, 0.5, 0));
        Assert.Null(ProviderValues.NormalizeBatchRequestCounts(9007199254740992d, 0, 0, 0));
        Assert.Null(ProviderValues.NormalizeBatchRequestCounts(2, 0, 1, 0));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/get-runtime-environment-user-agent.test.ts::getRuntimeEnvironmentUserAgent::should return the correct user agent for browsers",
        Coverage = UpstreamCoverage.Covered)]
    public void Runtime_user_agent_reports_a_browser()
    {
        Assert.Equal("runtime/browser", ProviderValues.GetRuntimeEnvironmentUserAgent(new ProviderValues.RuntimeProbe { Window = true }));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/get-runtime-environment-user-agent.test.ts::getRuntimeEnvironmentUserAgent::should return the correct user agent for test",
        Coverage = UpstreamCoverage.Covered)]
    public void Runtime_user_agent_reports_the_navigator_value()
    {
        Assert.Equal("runtime/test", ProviderValues.GetRuntimeEnvironmentUserAgent(new ProviderValues.RuntimeProbe { NavigatorUserAgent = "test" }));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/get-runtime-environment-user-agent.test.ts::getRuntimeEnvironmentUserAgent::should return the correct user agent for Edge Runtime",
        Coverage = UpstreamCoverage.Covered)]
    public void Runtime_user_agent_reports_the_edge_runtime()
    {
        Assert.Equal("runtime/vercel-edge", ProviderValues.GetRuntimeEnvironmentUserAgent(new ProviderValues.RuntimeProbe { EdgeRuntime = true }));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/get-runtime-environment-user-agent.test.ts::getRuntimeEnvironmentUserAgent::should return the correct user agent for Node.js",
        Coverage = UpstreamCoverage.Covered)]
    public void Runtime_user_agent_reports_node()
    {
        var probe = new ProviderValues.RuntimeProbe { NodeVersion = "test", ProcessVersion = "test" };
        Assert.Equal("runtime/node.js/test", ProviderValues.GetRuntimeEnvironmentUserAgent(probe));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-abort-error.test.ts::isAbortError::returns true for recognized Error names",
        Coverage = UpstreamCoverage.Covered)]
    public void Abort_error_recognizes_named_failures()
    {
        Assert.True(AbortErrors.IsAbortError(new NamedException("AbortError", "aborted")));
        Assert.True(AbortErrors.IsAbortError(new NamedException("ResponseAborted", "aborted")));
        Assert.True(AbortErrors.IsAbortError(new NamedException("TimeoutError", "timed out")));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-abort-error.test.ts::isAbortError::returns true for an abort DOMException",
        Coverage = UpstreamCoverage.Covered)]
    public void Abort_error_recognizes_a_cancelled_delay()
    {
        var error = new DelayAbortedException();
        Assert.True(AbortErrors.IsAbortError(error));
        Assert.Equal("AbortError", error.Name);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/is-abort-error.test.ts::isAbortError::returns false for unrelated errors",
        Coverage = UpstreamCoverage.Covered)]
    public void Abort_error_ignores_an_ordinary_exception()
    {
        Assert.False(AbortErrors.IsAbortError(new InvalidOperationException("TypeError")));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/delay.test.ts::delay > basic delay functionality::should resolve immediately when delayInMs is null",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_completes_immediately_for_null()
    {
        await Delay.WaitAsync(null);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/delay.test.ts::delay > basic delay functionality::should resolve immediately when delayInMs is undefined",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_completes_immediately_when_the_duration_is_omitted()
    {
        await Delay.WaitAsync();
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/delay.test.ts::delay > basic delay functionality::should resolve immediately when delayInMs is 0",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_completes_immediately_for_zero()
    {
        await Delay.WaitAsync(0);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/delay.test.ts::delay > basic delay functionality::should resolve after the specified delay",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_waits_for_the_requested_duration()
    {
        var watch = Stopwatch.StartNew();
        await Delay.WaitAsync(40);
        watch.Stop();
        Assert.True(watch.ElapsedMilliseconds >= 20);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/delay.test.ts::delay > abort signal functionality::should reject immediately if signal is already aborted",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_rejects_an_already_cancelled_token()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var error = await Assert.ThrowsAsync<DelayAbortedException>(() => Delay.WaitAsync(1000, source.Token));
        Assert.Equal("Delay was aborted", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/delay.test.ts::delay > abort signal functionality::should reject when signal is aborted during delay",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_rejects_when_the_token_is_cancelled()
    {
        using var source = new CancellationTokenSource();
        var waiting = Delay.WaitAsync(5000, source.Token);
        source.Cancel();
        await Assert.ThrowsAsync<DelayAbortedException>(() => waiting);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/delay.test.ts::delay > abort signal functionality::should clean up timeout when aborted",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_finishes_when_the_wait_is_cancelled()
    {
        using var source = new CancellationTokenSource();
        var waiting = Delay.WaitAsync(5000, source.Token);
        source.Cancel();
        await Assert.ThrowsAsync<DelayAbortedException>(() => waiting);
        Assert.True(waiting.IsCompleted);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/delay.test.ts::delay > abort signal functionality::should clean up event listener when delay completes normally",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_stays_completed_after_a_later_cancellation()
    {
        using var source = new CancellationTokenSource();
        var waiting = Delay.WaitAsync(0, source.Token);
        await waiting;
        source.Cancel();
        Assert.Equal(TaskStatus.RanToCompletion, waiting.Status);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/delay.test.ts::delay > abort signal functionality::should work without signal option",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_runs_without_a_token()
    {
        await Delay.WaitAsync(1);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/delay.test.ts::delay > error handling::should create proper DOMException for abort",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_abort_uses_the_abort_error_name()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var error = await Assert.ThrowsAsync<DelayAbortedException>(() => Delay.WaitAsync(1, source.Token));
        Assert.Equal("Delay was aborted", error.Message);
        Assert.Equal("AbortError", error.Name);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/delay.test.ts::delay > edge cases::should handle negative delays (treated as 0)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_treats_a_negative_duration_as_zero()
    {
        await Delay.WaitAsync(-5);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/delay.test.ts::delay > edge cases::should handle multiple delays simultaneously",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Delay_runs_several_waits_together()
    {
        var watch = Stopwatch.StartNew();
        await Task.WhenAll(Delay.WaitAsync(20), Delay.WaitAsync(40), Delay.WaitAsync(60));
        watch.Stop();
        Assert.True(watch.ElapsedMilliseconds >= 40);
    }

    private sealed class Marker
    {
    }
}
