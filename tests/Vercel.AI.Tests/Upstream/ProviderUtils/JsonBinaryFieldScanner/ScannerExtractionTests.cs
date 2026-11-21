// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.ProviderUtils.JsonBinaryFieldScanner.ScannerSupport;
using Scanner = Vercel.AI.ProviderUtils.JsonBinaryFieldScanner;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.JsonBinaryFieldScanner;

/// <summary>Port of <c>json-binary-field-scanner.test.ts</c> &gt; <c>createJsonBinaryFieldScanner &gt; extraction</c>.</summary>
public sealed class ScannerExtractionTests
{
    private const string Prefix = TestFile + "createJsonBinaryFieldScanner > extraction::";

    [Fact]
    [UpstreamTest(
        Prefix + "replaces a long base64 string with a placeholder and returns its bytes",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Replaces_a_long_base64_string_with_a_placeholder_and_returns_its_bytes()
    {
        var bytes = RandomBytes(5000);
        var body = Json(O(("images", L(Convert.ToBase64String(bytes))), ("warnings", L())));

        var scanned = Scan(body);

        var binary = Assert.Single(scanned.Binaries);
        Assert.Equal(bytes, binary.Value);
        AssertSame(O(("images", L(binary.Key)), ("warnings", L())), ParseText(scanned.Text));
        Assert.Matches(@"^\[binary data: 5000 bytes; ref \d+-\d+\]\z", binary.Key);
        Assert.True(scanned.Text.Length < 200);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "extracts at every chunk size",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Extracts_at_every_chunk_size()
    {
        var bytes = RandomBytes(1000, 7);
        var body = Json(
            O(
                ("before", new string('x', 10)),
                ("images", L(Convert.ToBase64String(bytes))),
                ("after", O(("text", "y")))
            )
        );

        foreach (var chunkSize in ChunkSizes)
        {
            var scanned = Scan(body, chunkSize: chunkSize);
            var binary = Assert.Single(scanned.Binaries);

            Assert.Equal(bytes, binary.Value);
            AssertSame(
                O(
                    ("before", new string('x', 10)),
                    ("images", L(binary.Key)),
                    ("after", O(("text", "y")))
                ),
                ParseText(scanned.Text)
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "extracts every byte length, covering all padding variants",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Extracts_every_byte_length_covering_all_padding_variants()
    {
        for (var length = 8; length < 64; length++)
        {
            var bytes = RandomBytes(length, (uint)length);
            var body = Json(O(("images", L(Convert.ToBase64String(bytes)))));

            foreach (var chunkSize in new[] { 1, 3, 5, 4096 })
            {
                var scanned = Scan(body, chunkSize: chunkSize);
                Assert.Equal(bytes, Assert.Single(scanned.Binaries).Value);
            }
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "returns the same bytes as the regular base64 decoding",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Returns_the_same_bytes_as_the_regular_base64_decoding()
    {
        var base64 = Base64Of(100_001, 3);

        var scanned = Scan(Json(O(("images", L(base64)))), chunkSize: 4093);

        Assert.Equal(Convert.FromBase64String(base64), Assert.Single(scanned.Binaries).Value);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "handles strings around the sizes of the receiving segments",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Handles_strings_around_the_sizes_of_the_receiving_segments()
    {
        // The receiving segments hold whole groups of three bytes, and every new
        // one is as large as all of the previous ones together (at least 4096
        // bytes): 4095, 4095, 8190, ... These are the cumulative sizes.
        foreach (var boundary in new[] { 4095, 8190, 16_380, 32_760, 65_520 })
        {
            foreach (
                var size in new[]
                {
                    boundary - 2,
                    boundary - 1,
                    boundary,
                    boundary + 1,
                    boundary + 2,
                }
            )
            {
                var bytes = RandomBytes(size, (uint)size);
                var scanned = Scan(
                    Json(O(("images", L(Convert.ToBase64String(bytes))))),
                    chunkSize: 65_536
                );

                Assert.Equal(bytes, Assert.Single(scanned.Binaries).Value);
            }
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "extracts several strings in order, each at its own path position",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Extracts_several_strings_in_order_each_at_its_own_path_position()
    {
        var first = RandomBytes(900, 1);
        var second = RandomBytes(901, 2);
        var third = RandomBytes(902, 3);
        var body = Json(
            O(
                (
                    "images",
                    L(
                        Convert.ToBase64String(first),
                        Convert.ToBase64String(second),
                        Convert.ToBase64String(third)
                    )
                )
            )
        );

        foreach (var chunkSize in new[] { 1, 7, 4096 })
        {
            AssertSame(
                O(("images", L(first, second, third))),
                ParseScanned(body, chunkSize: chunkSize)
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "extracts at nested paths",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Extracts_at_nested_paths()
    {
        var first = RandomBytes(1200, 1);
        var second = RandomBytes(1300, 2);

        Dictionary<string, object?> File(object data) =>
            O(
                ("type", "file"),
                ("mediaType", "image/png"),
                ("data", O(("type", "data"), ("data", data)))
            );

        var body = Json(
            O(
                (
                    "content",
                    L(
                        O(("type", "text"), ("text", "hello")),
                        File(Convert.ToBase64String(first)),
                        File(Convert.ToBase64String(second)),
                        O(
                            ("type", "file"),
                            ("data", O(("type", "url"), ("url", "https://example.com")))
                        )
                    )
                ),
                ("finishReason", O(("unified", "stop")))
            )
        );

        foreach (var chunkSize in new[] { 1, 5, 4096 })
        {
            AssertSame(
                O(
                    (
                        "content",
                        L(
                            O(("type", "text"), ("text", "hello")),
                            File(first),
                            File(second),
                            O(
                                ("type", "file"),
                                ("data", O(("type", "url"), ("url", "https://example.com")))
                            )
                        )
                    ),
                    ("finishReason", O(("unified", "stop")))
                ),
                ParseScanned(body, Files, chunkSize)
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "supports several paths and object-only paths",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Supports_several_paths_and_object_only_paths()
    {
        var bytes = RandomBytes(500, 4);
        var base64 = Convert.ToBase64String(bytes);
        var body = Json(O(("a", O(("b", base64))), ("c", L(base64)), ("d", base64)));

        AssertSame(
            O(("a", O(("b", bytes))), ("c", L(bytes)), ("d", base64)),
            ParseScanned(body, ["a.b", "c[*]"], 3)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "supports a root array",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Supports_a_root_array()
    {
        var bytes = RandomBytes(500, 4);
        var body = Json(L(O(("data", Convert.ToBase64String(bytes))), O(("data", "x"))));

        AssertSame(L(O(("data", bytes)), O(("data", "x"))), ParseScanned(body, ["[*].data"], 5));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps strings shorter than the minimum as strings",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Keeps_strings_shorter_than_the_minimum_as_strings()
    {
        var shortText = Base64Of(100, 1);
        var longText = Base64Of(200, 2);
        var body = Json(O(("images", L(shortText, longText, shortText))));

        foreach (var chunkSize in new[] { 1, 7, 4096 })
        {
            var scanned = Scan(body, minBytes: 150, chunkSize: chunkSize);
            Assert.Single(scanned.Binaries);
            AssertSame(
                O(("images", L(shortText, Convert.FromBase64String(longText), shortText))),
                ParseScanned(scanned)
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "uses the minimum on the decoded size, inclusive",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Uses_the_minimum_on_the_decoded_size_inclusive()
    {
        var body = Json(O(("images", L(Base64Of(300)))));

        Assert.Single(Scan(body, minBytes: 300).Binaries);
        Assert.Empty(Scan(body, minBytes: 301).Binaries);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "counts padding characters towards the decoded size",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Counts_padding_characters_towards_the_decoded_size()
    {
        var body = Json(O(("images", L(Base64Of(301)))));

        Assert.Single(Scan(body, minBytes: 301).Binaries);
        Assert.Empty(Scan(body, minBytes: 302).Binaries);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "defaults to a minimum of 64 KiB",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Defaults_to_a_minimum_of_64_KiB()
    {
        int Run(int length)
        {
            var scanner = new Scanner(Scanner.CompileBinaryFieldPaths(Images));
            var bytes = Encoding.UTF8.GetBytes(Json(O(("images", L(Base64Of(length))))));
            scanner.Push(bytes, bytes.Length);
            return scanner.Finish().Binaries.Count;
        }

        Assert.Equal(65_536, Scanner.DefaultBinaryFieldMinBytes);
        Assert.Equal(0, Run(65_535));
        Assert.Equal(1, Run(65_536));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not retain the chunks it was given",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Does_not_retain_the_chunks_it_was_given()
    {
        var bytes = RandomBytes(2000, 3);
        var chunk = Encoding.UTF8.GetBytes(
            Json(
                O(
                    ("before", "kept text"),
                    ("images", L(Convert.ToBase64String(bytes))),
                    ("after", "also kept")
                )
            )
        );
        var scanner = new Scanner(Scanner.CompileBinaryFieldPaths(Images), 8);

        scanner.Push(chunk, chunk.Length);
        Array.Clear(chunk, 0, chunk.Length);
        var scanned = scanner.Finish();

        Assert.Equal(bytes, Assert.Single(scanned.Binaries).Value);
        AssertSame(
            O(
                ("before", "kept text"),
                ("images", L(scanned.Binaries.Keys.First())),
                ("after", "also kept")
            ),
            ParseText(scanned.Text)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "returns arrays that own their whole buffer",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Returns_arrays_that_own_their_whole_buffer()
    {
        var bytes = RandomBytes(5000, 3);
        var scanned = Scan(
            Json(O(("images", L(Convert.ToBase64String(bytes), Base64Of(70_000, 4))))),
            chunkSize: 4093
        );

        // A byte[] always owns its whole buffer: the decoded strings have the lengths of their bytes.
        Assert.Equal(new[] { 5000, 70_000 }, scanned.Binaries.Values.Select(b => b.Length));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "supports paths given in any order",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Supports_paths_given_in_any_order()
    {
        var first = RandomBytes(600, 1);
        var second = RandomBytes(700, 2);
        var body = Json(
            O(
                ("images", L(Convert.ToBase64String(first))),
                ("content", L(O(("data", O(("data", Convert.ToBase64String(second)))))))
            )
        );
        var expected = O(("images", L(first)), ("content", L(O(("data", O(("data", second)))))));

        AssertSame(expected, ParseScanned(body, [.. Files, .. Images]));
        AssertSame(expected, ParseScanned(body, [.. Images, .. Files]));
    }

    private static object? ParseText(string text)
    {
        using var document = JsonDocument.Parse(text);
        return Scanner.ReplaceBinaryFieldPlaceholders(
            document.RootElement,
            new Dictionary<string, byte[]>()
        );
    }
}
