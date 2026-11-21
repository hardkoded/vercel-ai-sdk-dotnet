// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.ProviderUtils.JsonBinaryFieldScanner.ScannerSupport;
using Scanner = Vercel.AI.ProviderUtils.JsonBinaryFieldScanner;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.JsonBinaryFieldScanner;

/// <summary>Port of <c>json-binary-field-scanner.test.ts</c> &gt; <c>createJsonBinaryFieldScanner &gt; skeleton</c>.</summary>
public sealed class ScannerSkeletonTests
{
    private const string Prefix = TestFile + "createJsonBinaryFieldScanner > skeleton::";

    public static TheoryData<string, byte[]> Bodies()
    {
        byte[] Utf8(string text) => Encoding.UTF8.GetBytes(text);
        byte[] Concat(params byte[][] parts) => parts.SelectMany(p => p).ToArray();
        return new TheoryData<string, byte[]>
        {
            { "plain object", Utf8("""{"a":1,"b":[true,false,null],"c":"text"}""") },
            { "whitespace and newlines", Utf8("{\n  \"a\" : 1 ,\n  \"b\" : [ 1 , 2 ]\n}\n") },
            {
                "multi-byte characters",
                Utf8("{\"text\":\"héllo wörld — ✓ 日本語 😀\",\"k\":\"ключ\"}")
            },
            { "escapes in strings", Utf8("""{"a":"quote \" backslash \\ slash \/ nl \n"}""") },
            { "structural characters inside strings", Utf8("""{"a":"{[,:]}","b":"\"{\""}""") },
            { "empty body", Utf8("") },
            { "whitespace only", Utf8("  \n ") },
            { "lone string", Utf8("\"just a string\"") },
            { "invalid json", Utf8("{\"a\": [1, 2,, }") },
            { "byte order mark", Concat([0xef, 0xbb, 0xbf], Utf8("""{"a":"b"}""")) },
            { "invalid utf-8", Concat(Utf8("{\"a\":\""), [0xff, 0xfe], Utf8("x\"}")) },
            { "truncated multi-byte sequence", Concat(Utf8("{\"a\":\""), [0xe2, 0x82]) },
        };
    }

    [Theory]
    [MemberData(nameof(Bodies))]
    [UpstreamTest(
        Prefix + "equals the decoded body at every chunk size: ${name}",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Equals_the_decoded_body_at_every_chunk_size(string name, byte[] bytes)
    {
        _ = name;
        // The regular path decodes the whole body as UTF-8 and the scanner decodes the skeleton once.
        var expected = Encoding.UTF8.GetString(bytes);

        foreach (var chunkSize in ChunkSizes)
        {
            var scanned = Scan(bytes, chunkSize: chunkSize);
            Assert.Equal(expected, scanned.Text);
            Assert.Empty(scanned.Binaries);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "equals the decoded body for every split into three chunks",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Equals_the_decoded_body_for_every_split_into_three_chunks()
    {
        var bytes = Encoding.UTF8.GetBytes(
            "{\"images\":[\"QUJD\",\"dà\",\"x\"],\"a\":{\"images\":[\"QUJD\"]},\"b\":\"\\\\\\\"\"}"
        );
        var expected = Encoding.UTF8.GetString(bytes);

        (byte[], int) Part(int start, int end) => (bytes[start..end], end - start);

        for (var first = 0; first <= bytes.Length; first++)
        {
            for (var second = first; second <= bytes.Length; second++)
            {
                var scanned = Scan(
                    bytes,
                    chunks: [Part(0, first), Part(first, second), Part(second, bytes.Length)]
                );
                Assert.Equal(expected, scanned.Text);
            }
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not change a body without any path matches",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Does_not_change_a_body_without_any_path_matches()
    {
        var body = Json(
            O(("images", Base64Of(3000)), ("content", L(O(("data", O(("data", Base64Of(3000))))))))
        );

        var scanned = Scan(body, ["other[*]"]);

        Assert.Equal(body, scanned.Text);
        Assert.Empty(scanned.Binaries);
    }
}
