// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.ProviderUtils.JsonBinaryFieldScanner.ScannerSupport;
using Scanner = Vercel.AI.ProviderUtils.JsonBinaryFieldScanner;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.JsonBinaryFieldScanner;

/// <summary>Port of <c>json-binary-field-scanner.test.ts</c> &gt; <c>createJsonBinaryFieldScanner &gt; strings that are not canonical base64 stay as they are</c>.</summary>
public sealed class ScannerNonCanonicalTests
{
    private const string Prefix =
        TestFile
        + "createJsonBinaryFieldScanner > strings that are not canonical base64 stay as they are::";

    public static TheoryData<string, string> Cases() =>
        new()
        {
            { "base64url", "\"AB-_CDEFGHIJKLMNOP\"" },
            { "data url", "\"data:image/png;base64,QUJDREVGR0hJSktM\"" },
            { "escaped slash", "\"QUJD\\/EZH\"" },
            { "unicode escape", "\"QUJD\\u0045EZH\"" },
            { "escaped quote", "\"QUJDRU\\\"EZH\"" },
            { "escaped backslash", "\"QUJDRU\\\\EZH\"" },
            { "escaped newline", "\"QUJDRUZH\\nSUpL\"" },
            { "space", "\"QUJDRUZH SUpL\"" },
            { "raw newline", "\"QUJDRUZH\nSUpL\"" },
            { "raw tab", "\"QUJDRUZH\tSUpL\"" },
            { "unpadded one", "\"QUJDRUZHSUpLTA\"" },
            { "unpadded two", "\"QUJDRUZHSUpLT\"" },
            { "length not divisible by four", "\"QUJDRUZHSUpLTE1\"" },
            { "single padding after two characters", "\"QUJDRUZHSUpLTQ=\"" },
            { "triple padding", "\"QUJDRUZHSUpLTQ===\"" },
            // One leftover character with zero bits: only the padding is wrong.
            { "triple padding after one character", "\"QUJDRUZHSUpLA===\"" },
            { "padding in the middle", "\"QUJD=UZHSUpLTE1O\"" },
            { "double padding in the middle", "\"QU==RUZHSUpLTE1O\"" },
            { "data after padding", "\"QUJDRUZHSUpLTQ==A\"" },
            { "padding only", "\"====\"" },
            { "nonzero trailing bits with two paddings", "\"QUJDRUZHSUpLTR==\"" },
            { "nonzero trailing bits with one padding", "\"QUJDRUZHSUpLTE1P=\"" },
            { "non-ascii character", "\"QUJDRUZHSUpLTE1é\"" },
            { "non-ascii in the middle", "\"QUJDRéZHSUpLTE1O\"" },
            { "nul character", "\"QUJDRUZH\u0000SUpL\"" },
            { "high byte", "\"QUJDRUZH\u007fSUpL\"" },
        };

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(
        Prefix + "leaves the text untouched: ${name}",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Leaves_the_text_untouched(string name, string literal)
    {
        _ = name;
        var body = "{\"images\":[" + literal + "],\"after\":1}";

        foreach (var chunkSize in ChunkSizes)
        {
            var scanned = Scan(body, chunkSize: chunkSize, minBytes: 4);

            Assert.Equal(body, scanned.Text);
            Assert.Empty(scanned.Binaries);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "extracts strings that use the plus and slash characters",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Extracts_strings_that_use_the_plus_and_slash_characters()
    {
        byte[] bytes = [0xfb, 0xff, 0xbf, 0xfb, 0xef, 0xbe, 0x3e, 0x3f];
        var base64 = Convert.ToBase64String(bytes);

        Assert.Contains('+', base64);
        Assert.Contains('/', base64);
        AssertSame(
            O(("images", L(bytes))),
            ParseScanned(Json(O(("images", L(base64)))), chunkSize: 3)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "leaves a long non-canonical string untouched, even with a late offending character",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Leaves_a_long_non_canonical_string_untouched_even_with_a_late_offending_character()
    {
        var longText = Base64Of(50_000, 5);

        foreach (var offender in new[] { "-", "_", " ", "\\n", "\\/", "\\u0041", "=A", "=" })
        {
            var body = "{\"images\":[\"" + longText + offender + "\"],\"after\":[1]}";

            foreach (var chunkSize in new[] { 1021, 8192 })
            {
                var scanned = Scan(body, chunkSize: chunkSize, minBytes: 100);
                Assert.Equal(body, scanned.Text);
                Assert.Empty(scanned.Binaries);
            }
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "extracts a canonical string next to non-canonical ones",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Extracts_a_canonical_string_next_to_non_canonical_ones()
    {
        var bytes = RandomBytes(600, 9);
        var body = Json(
            O(("images", L("AB-_CDEF", Convert.ToBase64String(bytes), "data:x", 5, null)))
        );

        AssertSame(
            O(("images", L("AB-_CDEF", bytes, "data:x", 5d, null))),
            ParseScanned(body, chunkSize: 5)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "lets the regular parser report invalid JSON the same way",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Lets_the_regular_parser_report_invalid_JSON_the_same_way()
    {
        var body = "{\"images\":[\"" + Base64Of(3000) + "\"";

        var scanned = Scan(body, chunkSize: 11);

        Assert.StartsWith("{\"images\":[\"[binary data: 3000 bytes;", scanned.Text);
        Assert.ThrowsAny<JsonException>(() => JsonDocument.Parse(scanned.Text));
        Assert.ThrowsAny<JsonException>(() => JsonDocument.Parse(body));
    }
}
