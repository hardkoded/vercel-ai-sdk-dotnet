// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.ProviderUtils.JsonBinaryFieldScanner.ScannerSupport;
using Scanner = Vercel.AI.ProviderUtils.JsonBinaryFieldScanner;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.JsonBinaryFieldScanner;

/// <summary>Port of <c>json-binary-field-scanner.test.ts</c> &gt; <c>createJsonBinaryFieldScanner &gt; truncated bodies</c>.</summary>
public sealed class ScannerTruncatedBodiesTests
{
    private const string Prefix = TestFile + "createJsonBinaryFieldScanner > truncated bodies::";

    [Fact]
    [UpstreamTest(
        Prefix + "returns the received text when the body ends inside a candidate string",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Returns_the_received_text_when_the_body_ends_inside_a_candidate_string()
    {
        var base64 = Base64Of(500, 3);
        var body = "{\"images\":[\"" + base64 + "\",\"" + base64 + "\"]}";
        var bytes = Encoding.UTF8.GetBytes(body);

        for (var cut = 0; cut <= bytes.Length; cut++)
        {
            foreach (var chunkSize in new[] { 1, 5, 64 * 1024 })
            {
                var truncated = bytes[..cut];
                var scanned = Scan(truncated, chunkSize: chunkSize);
                var expected = Encoding.UTF8.GetString(truncated);
                foreach (var placeholder in scanned.Binaries.Keys)
                {
                    Assert.Contains(placeholder, scanned.Text);
                    var at = expected.IndexOf(base64, StringComparison.Ordinal);
                    expected = expected[..at] + placeholder + expected[(at + base64.Length)..];
                }

                Assert.Equal(expected, scanned.Text);
            }
        }
    }
}
