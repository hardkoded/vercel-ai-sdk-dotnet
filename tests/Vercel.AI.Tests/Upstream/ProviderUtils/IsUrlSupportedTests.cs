// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

public sealed class IsUrlSupportedTests
{
    private const string File = "packages/provider-utils/src/is-url-supported.test.ts::isUrlSupported > ";

    [Fact]
    [UpstreamTest(File + "when the model does not support any URLs::should return false", Coverage = UpstreamCoverage.Covered)]
    public void Returns_false_when_no_urls_are_supported()
    {
        Assert.False(UrlSupport.IsUrlSupported("text/plain", "https://example.com", new Dictionary<string, IReadOnlyList<Regex>>()));
    }

    [Fact]
    [UpstreamTest(File + "when the model supports specific media types and URLs::should return true for exact media type and exact URL match", Coverage = UpstreamCoverage.Covered)]
    public void Matches_an_exact_media_type_and_url()
    {
        Assert.True(UrlSupport.IsUrlSupported(
            "text/plain",
            "https://example.com",
            Urls(("text/plain", new[] { @"https://example\.com" }))));
    }

    [Fact]
    [UpstreamTest(File + "when the model supports specific media types and URLs::should return true for exact media type and regex URL match", Coverage = UpstreamCoverage.Covered)]
    public void Matches_an_exact_media_type_and_regex_url()
    {
        Assert.True(UrlSupport.IsUrlSupported(
            "image/png",
            "https://images.example.com/cat.png",
            Urls(("image/png", new[] { @"https://images\.example\.com/.+" }))));
    }

    [Fact]
    [UpstreamTest(File + "when the model supports specific media types and URLs::should return true for exact media type and one of multiple regex URLs match", Coverage = UpstreamCoverage.Covered)]
    public void Matches_one_of_several_url_patterns()
    {
        Assert.True(UrlSupport.IsUrlSupported(
            "image/png",
            "https://another.com/img.png",
            Urls(("image/png", new[] { @"https://images\.example\.com/.+", @"https://another\.com/img\.png" }))));
    }

    [Fact]
    [UpstreamTest(File + "when the model supports specific media types and URLs::should return false for exact media type but URL mismatch", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_url_outside_the_pattern()
    {
        Assert.False(UrlSupport.IsUrlSupported(
            "text/plain",
            "https://another.com",
            Urls(("text/plain", new[] { @"https://example\.com" }))));
    }

    [Fact]
    [UpstreamTest(File + "when the model supports specific media types and URLs::should return false for URL match but media type mismatch", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_different_media_type()
    {
        Assert.False(UrlSupport.IsUrlSupported(
            "image/png",
            "https://example.com",
            Urls(("text/plain", new[] { @"https://example\.com" }))));
    }

    [Fact]
    [UpstreamTest(File + "when the model supports URLs via wildcard media type (*)::should return true for wildcard media type and exact URL match", Coverage = UpstreamCoverage.Covered)]
    public void Wildcard_media_type_matches_an_exact_url()
    {
        Assert.True(UrlSupport.IsUrlSupported(
            "text/plain",
            "https://example.com",
            Urls(("*", new[] { @"https://example\.com" }))));
    }

    [Fact]
    [UpstreamTest(File + "when the model supports URLs via wildcard media type (*)::should return true for wildcard media type and regex URL match", Coverage = UpstreamCoverage.Covered)]
    public void Wildcard_media_type_matches_a_regex_url()
    {
        Assert.True(UrlSupport.IsUrlSupported(
            "image/jpeg",
            "https://images.example.com/dog.jpg",
            Urls(("*", new[] { @"https://images\.example\.com/.+" }))));
    }

    [Fact]
    [UpstreamTest(File + "when the model supports URLs via wildcard media type (*)::should return false for wildcard media type but URL mismatch", Coverage = UpstreamCoverage.Covered)]
    public void Wildcard_media_type_rejects_a_url_mismatch()
    {
        Assert.False(UrlSupport.IsUrlSupported(
            "video/mp4",
            "https://another.com",
            Urls(("*", new[] { @"https://example\.com" }))));
    }

    [Fact]
    [UpstreamTest(File + "when both specific and wildcard media types are defined::should return true if URL matches under specific media type", Coverage = UpstreamCoverage.Covered)]
    public void Specific_entry_matches_before_considering_the_wildcard()
    {
        Assert.True(UrlSupport.IsUrlSupported("text/plain", "https://text.com", SpecificAndWildcard()));
    }

    [Fact]
    [UpstreamTest(File + "when both specific and wildcard media types are defined::should return true if URL matches under wildcard media type even if specific exists", Coverage = UpstreamCoverage.Covered)]
    public void Falls_back_to_the_wildcard_when_the_specific_pattern_misses()
    {
        Assert.True(UrlSupport.IsUrlSupported("text/plain", "https://any.com", SpecificAndWildcard()));
    }

    [Fact]
    [UpstreamTest(File + "when both specific and wildcard media types are defined::should return true if URL matches under wildcard for a non-specified media type", Coverage = UpstreamCoverage.Covered)]
    public void Wildcard_matches_a_media_type_without_its_own_entry()
    {
        Assert.True(UrlSupport.IsUrlSupported("image/png", "https://any.com", SpecificAndWildcard()));
    }

    [Fact]
    [UpstreamTest(File + "when both specific and wildcard media types are defined::should return false if URL matches neither specific nor wildcard", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_url_that_matches_neither_entry()
    {
        Assert.False(UrlSupport.IsUrlSupported("text/plain", "https://other.com", SpecificAndWildcard()));
    }

    [Fact]
    [UpstreamTest(File + "when both specific and wildcard media types are defined::should return false if URL does not match wildcard for a non-specified media type", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_an_unlisted_media_type_when_the_wildcard_misses()
    {
        Assert.False(UrlSupport.IsUrlSupported("image/png", "https://other.com", SpecificAndWildcard()));
    }

    [Fact]
    [UpstreamTest(File + "edge cases::should return true if an empty URL matches a pattern", Coverage = UpstreamCoverage.Covered)]
    public void Empty_url_matches_a_pattern_that_allows_it()
    {
        Assert.True(UrlSupport.IsUrlSupported("text/plain", string.Empty, Urls(("text/plain", new[] { ".*" }))));
    }

    [Fact]
    [UpstreamTest(File + "edge cases::should return false if an empty URL does not match a pattern", Coverage = UpstreamCoverage.Covered)]
    public void Empty_url_misses_a_pattern_that_requires_text()
    {
        Assert.False(UrlSupport.IsUrlSupported("text/plain", string.Empty, Urls(("text/plain", new[] { @"https://.+" }))));
    }

    [Fact]
    [UpstreamTest(File + "case sensitivity::should be case-insensitive for media types", Coverage = UpstreamCoverage.Covered)]
    public void Media_types_are_case_insensitive()
    {
        Assert.True(UrlSupport.IsUrlSupported(
            "TEXT/PLAIN",
            "https://example.com",
            Urls(("text/plain", new[] { @"https://example\.com" }))));
    }

    [Fact]
    [UpstreamTest(File + "case sensitivity::should handle case-insensitive regex for URLs if specified", Coverage = UpstreamCoverage.Covered)]
    public void Urls_are_compared_after_lowercasing()
    {
        Assert.True(UrlSupport.IsUrlSupported(
            "text/plain",
            "https://EXAMPLE.com/path",
            Urls(("text/plain", new[] { @"https://example\.com/path" }))));
    }

    [Fact]
    [UpstreamTest(File + "case sensitivity::should be case-insensitive for URL paths by default regex", Coverage = UpstreamCoverage.Covered)]
    public void Url_paths_are_lowercased_before_the_pattern()
    {
        Assert.True(UrlSupport.IsUrlSupported(
            "text/plain",
            "https://example.com/PATH",
            Urls(("text/plain", new[] { @"https://example\.com/path" }))));
    }

    [Fact]
    [UpstreamTest(File + "wildcard subtypes in media types::should return true for wildcard subtype match", Coverage = UpstreamCoverage.Covered)]
    public void Subtype_wildcard_matches_the_top_level_type()
    {
        Assert.True(UrlSupport.IsUrlSupported(
            "image/png",
            "https://example.com",
            Urls(("image/*", new[] { @"https://example\.com" }))));
    }

    [Fact]
    [UpstreamTest(File + "wildcard subtypes in media types::should use full wildcard \"*\" if subtype wildcard is not matched or supported", Coverage = UpstreamCoverage.Covered)]
    public void Full_wildcard_is_used_when_the_subtype_wildcard_misses()
    {
        Assert.True(UrlSupport.IsUrlSupported(
            "image/png",
            "https://any.com",
            Urls(
                ("image/*", new[] { @"https://images\.com" }),
                ("*", new[] { @"https://any\.com" }))));
    }

    [Fact]
    [UpstreamTest(File + "top-level-only media types::should match a `type/*` key for a top-level-only media type", Coverage = UpstreamCoverage.Covered)]
    public void Top_level_media_matches_a_type_wildcard_key()
    {
        Assert.True(UrlSupport.IsUrlSupported(
            "image",
            "https://example.com/cat.png",
            Urls(("image/*", new[] { @"https://example\.com/.+" }))));
    }

    [Fact]
    [UpstreamTest(File + "top-level-only media types::should match the wildcard `*` key for a top-level-only media type", Coverage = UpstreamCoverage.Covered)]
    public void Top_level_media_matches_the_full_wildcard()
    {
        Assert.True(UrlSupport.IsUrlSupported(
            "image",
            "https://example.com",
            Urls(("*", new[] { @"https://example\.com" }))));
    }

    [Fact]
    [UpstreamTest(File + "top-level-only media types::should NOT match a specific `type/subtype` key for a top-level-only media type", Coverage = UpstreamCoverage.Covered)]
    public void Top_level_media_does_not_match_a_specific_subtype_key()
    {
        Assert.False(UrlSupport.IsUrlSupported(
            "image",
            "https://example.com/cat.png",
            Urls(("image/png", new[] { @"https://example\.com/.+" }))));
    }

    [Fact]
    [UpstreamTest(File + "top-level-only media types::should NOT match a different top-level `type/*` key", Coverage = UpstreamCoverage.Covered)]
    public void Top_level_media_does_not_match_a_different_type_wildcard()
    {
        Assert.False(UrlSupport.IsUrlSupported(
            "image",
            "https://example.com/audio.mp3",
            Urls(("audio/*", new[] { @"https://example\.com/.+" }))));
    }

    [Fact]
    [UpstreamTest(File + "empty URL arrays for a media type::should return false if the specific media type has an empty URL array", Coverage = UpstreamCoverage.Covered)]
    public void Empty_url_list_does_not_match()
    {
        Assert.False(UrlSupport.IsUrlSupported(
            "text/plain",
            "https://example.com",
            new Dictionary<string, IReadOnlyList<Regex>> { ["text/plain"] = Array.Empty<Regex>() }));
    }

    [Fact]
    [UpstreamTest(File + "empty URL arrays for a media type::should fall back to wildcard \"*\" if specific media type has empty array but wildcard matches", Coverage = UpstreamCoverage.Covered)]
    public void Empty_specific_list_falls_back_to_the_wildcard()
    {
        Assert.True(UrlSupport.IsUrlSupported(
            "text/plain",
            "https://any.com",
            new Dictionary<string, IReadOnlyList<Regex>>
            {
                ["text/plain"] = Array.Empty<Regex>(),
                ["*"] = new[] { new Regex(@"https://any\.com") },
            }));
    }

    [Fact]
    [UpstreamTest(File + "empty URL arrays for a media type::should return false if specific media type has empty array and wildcard does not match", Coverage = UpstreamCoverage.Covered)]
    public void Empty_specific_list_and_a_missed_wildcard_return_false()
    {
        Assert.False(UrlSupport.IsUrlSupported(
            "text/plain",
            "https://another.com",
            new Dictionary<string, IReadOnlyList<Regex>>
            {
                ["text/plain"] = Array.Empty<Regex>(),
                ["*"] = new[] { new Regex(@"https://any\.com") },
            }));
    }

    private static Dictionary<string, IReadOnlyList<Regex>> SpecificAndWildcard()
    {
        return Urls(
            ("text/plain", new[] { @"https://text\.com" }),
            ("*", new[] { @"https://any\.com" }));
    }

    private static Dictionary<string, IReadOnlyList<Regex>> Urls(params (string Type, string[] Patterns)[] entries)
    {
        var map = new Dictionary<string, IReadOnlyList<Regex>>();
        foreach (var entry in entries)
        {
            var patterns = new Regex[entry.Patterns.Length];
            for (var i = 0; i < entry.Patterns.Length; i++)
            {
                patterns[i] = new Regex(entry.Patterns[i]);
            }

            map[entry.Type] = patterns;
        }

        return map;
    }
}
