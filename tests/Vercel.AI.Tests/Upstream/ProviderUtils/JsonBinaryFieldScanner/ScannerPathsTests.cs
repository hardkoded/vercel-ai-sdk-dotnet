// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.ProviderUtils.JsonBinaryFieldScanner.ScannerSupport;
using Scanner = Vercel.AI.ProviderUtils.JsonBinaryFieldScanner;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.JsonBinaryFieldScanner;

/// <summary>Port of <c>json-binary-field-scanner.test.ts</c> &gt; <c>createJsonBinaryFieldScanner &gt; paths</c>.</summary>
public sealed class ScannerPathsTests
{
    private const string Prefix = TestFile + "createJsonBinaryFieldScanner > paths::";

    private static readonly string Base64 = Base64Of(600, 1);

    public static TheoryData<string, string> NotExtracted()
    {
        var b = Base64;
        return new TheoryData<string, string>
        {
            { "another key", Json(O(("other", L(b)))) },
            { "key with different case", Json(O(("Images", L(b)))) },
            { "longer key", Json(O(("imagesX", L(b)))) },
            { "shorter key", Json(O(("image", L(b)))) },
            { "nested below the path", Json(O(("a", O(("images", L(b)))))) },
            { "array of arrays", Json(O(("images", L(L(b))))) },
            { "object instead of array", Json(O(("images", O(("a", b))))) },
            { "string instead of array", Json(O(("images", b))) },
            { "array of objects", Json(O(("images", L(O(("a", b)))))) },
            { "sibling of the path", Json(O(("images", L()), ("x", b))) },
        };
    }

    [Theory]
    [MemberData(nameof(NotExtracted))]
    [UpstreamTest(
        Prefix + "does not extract: %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Does_not_extract(string name, string body)
    {
        _ = name;
        var scanned = Scan(body, chunkSize: 5);

        Assert.Equal(body, scanned.Text);
        Assert.Empty(scanned.Binaries);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not extract values at the wrong level of a nested path",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Does_not_extract_values_at_the_wrong_level_of_a_nested_path()
    {
        var body = Json(
            O(
                (
                    "content",
                    L(
                        O(("data", Base64)),
                        O(("data", O(("other", Base64)))),
                        O(("data", O(("data", O(("data", Base64)))))),
                        O(("x", O(("data", O(("data", Base64)))))),
                        L(O(("data", O(("data", Base64)))))
                    )
                ),
                ("data", O(("data", Base64)))
            )
        );

        var scanned = Scan(body, Files);

        Assert.Equal(body, scanned.Text);
        Assert.Empty(scanned.Binaries);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not match keys written with escape sequences",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Does_not_match_keys_written_with_escape_sequences()
    {
        foreach (
            var key in new[]
            {
                "\\u0069mages",
                "im\\u0061ges",
                "images\\u0000",
                "im\\/ages",
                "images\\/",
                "\\/images",
                "\\imag",
            }
        )
        {
            var body = "{\"" + key + "\":[\"" + Base64 + "\"]}";
            var scanned = Scan(body, chunkSize: 3);

            Assert.Equal(body, scanned.Text);
            Assert.Empty(scanned.Binaries);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not match keys with non-ASCII bytes",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Does_not_match_keys_with_non_ASCII_bytes()
    {
        var body = "{\"imagés\":[\"" + Base64 + "\"],\"images \":[\"" + Base64 + "\"]}";

        var scanned = Scan(body, chunkSize: 3);

        Assert.Equal(body, scanned.Text);
        Assert.Empty(scanned.Binaries);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not mistake one key for a path of several keys or items",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Does_not_mistake_one_key_for_a_path_of_several_keys_or_items()
    {
        var body = Json(O(("a.b", Base64), ("a[*]", L(Base64)), ("x[*][*]", L(L(Base64)))));

        var scanned = Scan(body, ["a.b", "a[*]", "x[*][*]"], chunkSize: 3);

        Assert.Equal(body, scanned.Text);
        Assert.Empty(scanned.Binaries);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps scanning after a key with an escape sequence",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Keeps_scanning_after_a_key_with_an_escape_sequence()
    {
        var bytes = RandomBytes(600, 3);
        var body =
            "{\"\\u0061\":1,\"a\\\"b\":2,\"images\":[\"" + Convert.ToBase64String(bytes) + "\"]}";

        foreach (var chunkSize in new[] { 1, 3, 4096 })
        {
            AssertSame(
                O(("a", 1d), ("a\"b", 2d), ("images", L(bytes))),
                ParseScanned(body, chunkSize: chunkSize)
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "matches keys that contain structural characters in other keys",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Matches_keys_that_contain_structural_characters_in_other_keys()
    {
        var body =
            "{\"a,b:c{d}[e]\":1,\"images\":[\""
            + Base64
            + "\"],\"x\\\"images\":[\""
            + Base64
            + "\"]}";

        var scanned = Scan(body, chunkSize: 3);

        Assert.Single(scanned.Binaries);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "lets the last of duplicate keys win, as JSON.parse does",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Lets_the_last_of_duplicate_keys_win_as_JSON_parse_does()
    {
        var bytes = RandomBytes(600, 2);
        var encoded = Convert.ToBase64String(bytes);

        AssertSame(
            O(("images", L(bytes))),
            ParseScanned(
                "{\"images\":[\"" + Base64 + "\"],\"images\":[\"" + encoded + "\"]}",
                chunkSize: 7
            )
        );
        AssertSame(
            O(("images", L("short"))),
            ParseScanned("{\"images\":[\"" + Base64 + "\"],\"images\":[\"short\"]}", chunkSize: 7)
        );
        AssertSame(
            O(("images", L(1d))),
            ParseScanned("{\"images\":[\"" + Base64 + "\"],\"images\":[1]}", chunkSize: 7)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "extracts after structures that were skipped",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Extracts_after_structures_that_were_skipped()
    {
        var bytes = RandomBytes(600, 2);
        var encoded = Convert.ToBase64String(bytes);
        var body = Json(
            O(
                (
                    "skipped",
                    O(("deep", O(("deeper", O(("deepest", L(O(("images", L(encoded))))))))))
                ),
                ("images", L(encoded)),
                (
                    "content",
                    L(O(("meta", O(("x", L(1, L(2, L(3))))))), O(("data", O(("data", encoded)))))
                )
            )
        );

        AssertSame(
            O(
                (
                    "skipped",
                    O(("deep", O(("deeper", O(("deepest", L(O(("images", L(encoded))))))))))
                ),
                ("images", L(bytes)),
                (
                    "content",
                    L(O(("meta", O(("x", L(1d, L(2d, L(3d))))))), O(("data", O(("data", bytes)))))
                )
            ),
            ParseScanned(body, [.. Images, .. Files], 3)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "is not confused by structure in strings, escapes and chunk borders",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Is_not_confused_by_structure_in_strings_escapes_and_chunk_borders()
    {
        var bytes = RandomBytes(600, 2);
        var encoded = Convert.ToBase64String(bytes);
        Dictionary<string, object?> Document(object?[] images) =>
            O(
                ("a", "\\\"}],\"images\":[\"x"),
                ("b", "\""),
                ("c", "\\"),
                ("d", "\\\\\""),
                ("images", L(images))
            );
        var body = Json(Document([encoded, "\"]", encoded]));

        foreach (var chunkSize in ChunkSizes)
        {
            AssertSame(Document([bytes, "\"]", bytes]), ParseScanned(body, chunkSize: chunkSize));
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "survives nesting that is much deeper than any path",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Survives_nesting_that_is_much_deeper_than_any_path()
    {
        var bytes = RandomBytes(600, 2);
        var encoded = Convert.ToBase64String(bytes);
        const int depth = 50_000;
        var body =
            "{\"deep\":"
            + new string('[', depth)
            + "{\"images\":[\""
            + encoded
            + "\"]}"
            + new string(']', depth)
            + ",\"images\":[\""
            + encoded
            + "\"]}";

        var scanned = Scan(body, chunkSize: 4096);

        Assert.Single(scanned.Binaries);
        Assert.True(scanned.Text.Length < body.Length);
        Assert.EndsWith("]}", scanned.Text);
        Assert.Contains(
            "\"deep\":" + new string('[', depth) + "{\"images\":[\"" + encoded + "\"]}",
            scanned.Text
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "treats unbalanced closing brackets without failing",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Treats_unbalanced_closing_brackets_without_failing()
    {
        var body = "]}]}{\"images\":[\"" + Base64Of(600) + "\"]}";

        Scan(body, chunkSize: 3);
    }
}
