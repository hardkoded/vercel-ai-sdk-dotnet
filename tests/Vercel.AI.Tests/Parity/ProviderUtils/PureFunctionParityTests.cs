// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http;
using System.Numerics;
using System.Text.RegularExpressions;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

public sealed class PureFunctionParityTests
{
    [Fact]
    [UpstreamTest("packages/provider-utils/src/as-array.test.ts::asArray::returns an empty array for undefined", Coverage = UpstreamCoverage.Covered)]
    public void As_array_returns_empty_for_undefined()
    {
        var result = (object[])Arrays.AsArray(JsUndefined.Value);
        Assert.Empty(result);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/as-array.test.ts::asArray::wraps a single value in an array", Coverage = UpstreamCoverage.Covered)]
    public void As_array_wraps_a_single_value()
    {
        Assert.Equal(new object[] { "value" }, (object[])Arrays.AsArray("value"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/as-array.test.ts::asArray::returns an array value unchanged", Coverage = UpstreamCoverage.Covered)]
    public void As_array_returns_the_same_array()
    {
        var value = new[] { "a", "b" };
        Assert.Same(value, Arrays.AsArray(value));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/filter-nullable.test.ts::filterNullable::removes null and undefined values from a value list", Coverage = UpstreamCoverage.Covered)]
    public void Filter_nullable_drops_null_and_undefined()
    {
        Assert.Equal(new object?[] { 1, 2, 3 }, NullableValues.FilterNullable(1, null, 2, JsUndefined.Value, 3));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/filter-nullable.test.ts::filterNullable::preserves other falsy values", Coverage = UpstreamCoverage.Covered)]
    public void Filter_nullable_keeps_other_falsy_values()
    {
        Assert.Equal(new object?[] { 0, false, "" }, NullableValues.FilterNullable(0, false, "", null, JsUndefined.Value));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/without-trailing-slash.test.ts::withoutTrailingSlash::removes a trailing slash", Coverage = UpstreamCoverage.Covered)]
    public void Without_trailing_slash_removes_one_slash()
    {
        Assert.Equal("https://example.com", Urls.WithoutTrailingSlash("https://example.com/"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/without-trailing-slash.test.ts::withoutTrailingSlash::returns undefined when the URL is undefined", Coverage = UpstreamCoverage.Covered)]
    public void Without_trailing_slash_keeps_null()
    {
        Assert.Null(Urls.WithoutTrailingSlash(null));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/without-trailing-slash.test.ts::withoutTrailingSlash::preserves an empty string", Coverage = UpstreamCoverage.Covered)]
    public void Without_trailing_slash_keeps_an_empty_string()
    {
        Assert.Equal(string.Empty, Urls.WithoutTrailingSlash(string.Empty));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/strip-file-extension.test.ts::stripFileExtension::should strip the extension from a filename", Coverage = UpstreamCoverage.Covered)]
    public void Strip_file_extension_removes_the_suffix()
    {
        Assert.Equal("report", FileNames.StripFileExtension("report.pdf"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/strip-file-extension.test.ts::stripFileExtension::should return the input when there is no extension", Coverage = UpstreamCoverage.Covered)]
    public void Strip_file_extension_keeps_a_name_without_a_dot()
    {
        Assert.Equal("report", FileNames.StripFileExtension("report"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/strip-file-extension.test.ts::stripFileExtension::should strip all extension segments for multi-dot filenames", Coverage = UpstreamCoverage.Covered)]
    public void Strip_file_extension_stops_at_the_first_dot()
    {
        Assert.Equal("archive", FileNames.StripFileExtension("archive.tar.gz"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/strip-file-extension.test.ts::stripFileExtension::should strip a trailing dot", Coverage = UpstreamCoverage.Covered)]
    public void Strip_file_extension_removes_a_trailing_dot()
    {
        Assert.Equal("report", FileNames.StripFileExtension("report."));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/media-type-to-extension.test.ts::mediaTypeToExtension()::should map %s to %s", Coverage = UpstreamCoverage.Covered)]
    public void Media_type_to_extension_maps_every_catalog_pair()
    {
        Assert.Equal("mp3", FileNames.MediaTypeToExtension("audio/mpeg"));
        Assert.Equal("mp3", FileNames.MediaTypeToExtension("audio/mp3"));
        Assert.Equal("wav", FileNames.MediaTypeToExtension("audio/wav"));
        Assert.Equal("wav", FileNames.MediaTypeToExtension("audio/x-wav"));
        Assert.Equal("webm", FileNames.MediaTypeToExtension("audio/webm"));
        Assert.Equal("ogg", FileNames.MediaTypeToExtension("audio/ogg"));
        Assert.Equal("ogg", FileNames.MediaTypeToExtension("audio/opus"));
        Assert.Equal("m4a", FileNames.MediaTypeToExtension("audio/mp4"));
        Assert.Equal("m4a", FileNames.MediaTypeToExtension("audio/x-m4a"));
        Assert.Equal("flac", FileNames.MediaTypeToExtension("audio/flac"));
        Assert.Equal("aac", FileNames.MediaTypeToExtension("audio/aac"));
        Assert.Equal("mp3", FileNames.MediaTypeToExtension("AUDIO/MPEG"));
        Assert.Equal("mp3", FileNames.MediaTypeToExtension("AUDIO/MP3"));
        Assert.Equal(string.Empty, FileNames.MediaTypeToExtension("nope"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-record.test.ts::isRecord::returns true for non-null, non-array objects", Coverage = UpstreamCoverage.Covered)]
    public void Is_record_accepts_plain_objects_and_dates()
    {
        Assert.True(Records.IsRecord(new Dictionary<string, object?>()));
        Assert.True(Records.IsRecord(new NullPrototype()));
        Assert.True(Records.IsRecord(DateTime.UtcNow));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-record.test.ts::isRecord::returns false for arrays, null, and primitive values", Coverage = UpstreamCoverage.Covered)]
    public void Is_record_rejects_arrays_and_primitives()
    {
        Assert.False(Records.IsRecord(Array.Empty<object>()));
        Assert.False(Records.IsRecord(null));
        Assert.False(Records.IsRecord(JsUndefined.Value));
        Assert.False(Records.IsRecord("value"));
        Assert.False(Records.IsRecord(42));
        Assert.False(Records.IsRecord(true));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-json-serializable.test.ts::isJSONSerializable::returns true for null and undefined", Coverage = UpstreamCoverage.Covered)]
    public void Json_serializable_accepts_null_and_undefined()
    {
        Assert.True(Records.IsJsonSerializable(null));
        Assert.True(Records.IsJsonSerializable(JsUndefined.Value));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-json-serializable.test.ts::isJSONSerializable::returns true for primitive JSON-compatible values", Coverage = UpstreamCoverage.Covered)]
    public void Json_serializable_accepts_primitives()
    {
        Assert.True(Records.IsJsonSerializable("test"));
        Assert.True(Records.IsJsonSerializable(42));
        Assert.True(Records.IsJsonSerializable(true));
        Assert.True(Records.IsJsonSerializable(false));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-json-serializable.test.ts::isJSONSerializable::returns false for unsupported primitive values", Coverage = UpstreamCoverage.Covered)]
    public void Json_serializable_rejects_functions_symbols_and_bigint()
    {
        Assert.False(Records.IsJsonSerializable(new Action(() => { })));
        Assert.False(Records.IsJsonSerializable(new JsSymbol("test")));
        Assert.False(Records.IsJsonSerializable(new BigInteger(1)));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-json-serializable.test.ts::isJSONSerializable::returns true for arrays when all values are serializable", Coverage = UpstreamCoverage.Covered)]
    public void Json_serializable_accepts_arrays_of_plain_values()
    {
        var nested = new Dictionary<string, object?> { ["nested"] = new object[] { "value" } };
        Assert.True(Records.IsJsonSerializable(new object?[] { "test", 42, true, null, JsUndefined.Value, nested }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-json-serializable.test.ts::isJSONSerializable::returns false for arrays containing non-serializable values", Coverage = UpstreamCoverage.Covered)]
    public void Json_serializable_rejects_arrays_with_functions()
    {
        Assert.False(Records.IsJsonSerializable(new object[] { "test", new Action(() => { }) }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-json-serializable.test.ts::isJSONSerializable::returns true for plain objects when all values are serializable", Coverage = UpstreamCoverage.Covered)]
    public void Json_serializable_accepts_plain_dictionaries()
    {
        var value = new Dictionary<string, object?>
        {
            ["string"] = "test",
            ["number"] = 42,
            ["boolean"] = true,
            ["nullValue"] = null,
            ["undefinedValue"] = JsUndefined.Value,
            ["nested"] = new Dictionary<string, object?> { ["array"] = new object[] { "value" } },
        };
        Assert.True(Records.IsJsonSerializable(value));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-json-serializable.test.ts::isJSONSerializable::returns false for plain objects containing non-serializable values", Coverage = UpstreamCoverage.Covered)]
    public void Json_serializable_rejects_nested_functions()
    {
        var value = new Dictionary<string, object?>
        {
            ["nested"] = new Dictionary<string, object?> { ["callback"] = new Action(() => { }) },
        };
        Assert.False(Records.IsJsonSerializable(value));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-json-serializable.test.ts::isJSONSerializable::returns false for non-plain objects", Coverage = UpstreamCoverage.Covered)]
    public void Json_serializable_rejects_dates_regexes_and_null_prototypes()
    {
        Assert.False(Records.IsJsonSerializable(DateTime.UtcNow));
        Assert.False(Records.IsJsonSerializable(new Regex("test")));
        Assert.False(Records.IsJsonSerializable(new PlainClass()));
        Assert.False(Records.IsJsonSerializable(new NullPrototype()));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/remove-undefined-entries.test.ts::should remove undefined entries from record", Coverage = UpstreamCoverage.Covered)]
    public void Remove_undefined_entries_drops_undefined()
    {
        var result = Records.RemoveUndefinedEntries(new Dictionary<string, object?>
        {
            ["a"] = 1,
            ["b"] = JsUndefined.Value,
            ["c"] = "test",
            ["d"] = JsUndefined.Value,
        });
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result["a"]);
        Assert.Equal("test", result["c"]);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/remove-undefined-entries.test.ts::should handle empty object", Coverage = UpstreamCoverage.Covered)]
    public void Remove_undefined_entries_keeps_an_empty_object_empty()
    {
        Assert.Empty(Records.RemoveUndefinedEntries(new Dictionary<string, object?>()));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/remove-undefined-entries.test.ts::should handle object with all undefined values", Coverage = UpstreamCoverage.Covered)]
    public void Remove_undefined_entries_drops_every_undefined_value()
    {
        var result = Records.RemoveUndefinedEntries(new Dictionary<string, object?>
        {
            ["a"] = JsUndefined.Value,
            ["b"] = JsUndefined.Value,
        });
        Assert.Empty(result);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/remove-undefined-entries.test.ts::should remove null values", Coverage = UpstreamCoverage.Covered)]
    public void Remove_undefined_entries_drops_null()
    {
        var result = Records.RemoveUndefinedEntries(new Dictionary<string, object?>
        {
            ["a"] = null,
            ["b"] = JsUndefined.Value,
            ["c"] = "test",
        });
        Assert.Equal(new Dictionary<string, object?> { ["c"] = "test" }, result);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/remove-undefined-entries.test.ts::should preserve falsy values except null and undefined", Coverage = UpstreamCoverage.Covered)]
    public void Remove_undefined_entries_keeps_false_zero_and_empty_string()
    {
        var result = Records.RemoveUndefinedEntries(new Dictionary<string, object?>
        {
            ["a"] = false,
            ["b"] = 0,
            ["c"] = "",
            ["d"] = JsUndefined.Value,
            ["e"] = null,
        });
        Assert.Equal(false, result["a"]);
        Assert.Equal(0, result["b"]);
        Assert.Equal("", result["c"]);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-same-origin.test.ts::isSameOrigin::returns true for identical origins (ignoring path/query)", Coverage = UpstreamCoverage.Covered)]
    public void Same_origin_ignores_path_and_query()
    {
        Assert.True(Urls.IsSameOrigin("https://api.example.com/v1/file", "https://api.example.com"));
        Assert.True(Urls.IsSameOrigin("https://api.example.com/a?x=1", "https://api.example.com/b"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-same-origin.test.ts::isSameOrigin::returns false for a different host", Coverage = UpstreamCoverage.Covered)]
    public void Same_origin_rejects_a_different_host()
    {
        Assert.False(Urls.IsSameOrigin("https://cdn.evil.com/file", "https://api.example.com"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-same-origin.test.ts::isSameOrigin::returns false for a different scheme or port", Coverage = UpstreamCoverage.Covered)]
    public void Same_origin_rejects_a_different_scheme_or_port()
    {
        Assert.False(Urls.IsSameOrigin("http://api.example.com/file", "https://api.example.com"));
        Assert.False(Urls.IsSameOrigin("https://api.example.com:8443/file", "https://api.example.com"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-same-origin.test.ts::isSameOrigin::fails closed on invalid input", Coverage = UpstreamCoverage.Covered)]
    public void Same_origin_fails_closed()
    {
        Assert.False(Urls.IsSameOrigin("not-a-url", "https://api.example.com"));
        Assert.False(Urls.IsSameOrigin("https://api.example.com/file", "not-a-url"));
    }

    private sealed class NullPrototype : Dictionary<string, object?>
    {
    }

    private sealed class PlainClass
    {
    }
}
