// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.ProviderUtils.JsonBinaryFieldScanner.ScannerSupport;
using Scanner = Vercel.AI.ProviderUtils.JsonBinaryFieldScanner;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.JsonBinaryFieldScanner;

/// <summary>Port of <c>json-binary-field-scanner.test.ts</c> &gt; <c>createJsonBinaryFieldScanner &gt; placeholders</c>.</summary>
public sealed class ScannerPlaceholdersTests
{
    private const string Prefix = TestFile + "createJsonBinaryFieldScanner > placeholders::";

    [Fact]
    [UpstreamTest(
        Prefix + "uses a different placeholder for every string and every scanner",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Uses_a_different_placeholder_for_every_string_and_every_scanner()
    {
        var body = Json(O(("images", L(Base64Of(600), Base64Of(600)))));

        var placeholders = Scan(body).Binaries.Keys.Concat(Scan(body).Binaries.Keys).ToList();

        Assert.Equal(4, placeholders.Count);
        Assert.Equal(4, placeholders.Distinct().Count());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "replaces placeholders with their bytes, and nothing else",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Replaces_placeholders_with_their_bytes_and_nothing_else()
    {
        var scanned = Scan(Json(O(("images", L(Base64Of(600))))));
        var (placeholder, bytes) = Assert.Single(scanned.Binaries);
        var forged = Regex.Replace(placeholder, @"ref \d+-\d+", "ref 0-0");

        AssertSame(O(("images", L(placeholder))), ParseText(scanned.Text));
        using var parsed = JsonDocument.Parse(
            Json(
                O(
                    ("images", L(placeholder, forged, "other", 1, null)),
                    ("deep", O(("list", L(O(("value", placeholder))))))
                )
            )
        );
        AssertSame(
            O(
                ("images", L(bytes, forged, "other", 1d, null)),
                ("deep", O(("list", L(O(("value", bytes))))))
            ),
            Scanner.ReplaceBinaryFieldPlaceholders(parsed.RootElement, scanned.Binaries)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not modify the parsed value",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Does_not_modify_the_parsed_value()
    {
        var body = Json(
            O(
                ("images", L(Base64Of(600))),
                ("content", L(O(("data", O(("data", Base64Of(600, 2)), ("type", "data"))))))
            )
        );
        var scanned = Scan(body, [.. Images, .. Files]);
        using var parsed = JsonDocument.Parse(scanned.Text);

        var replaced = Scanner.ReplaceBinaryFieldPlaceholders(parsed.RootElement, scanned.Binaries);

        // A JsonElement is immutable: the parsed value still holds the placeholders.
        Assert.Equal(scanned.Text, parsed.RootElement.GetRawText());
        Assert.NotNull(replaced);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps an own __proto__ key an own key",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Keeps_an_own_proto_key_an_own_key()
    {
        using var parsed = JsonDocument.Parse(
            "{\"__proto__\":[\"x\"],\"a\":{\"__proto__\":\"y\"}}"
        );

        var replaced = Assert.IsType<Dictionary<string, object?>>(
            Scanner.ReplaceBinaryFieldPlaceholders(
                parsed.RootElement,
                new Dictionary<string, byte[]> { ["x"] = new byte[1] }
            )
        );

        Assert.Equal(["__proto__", "a"], replaced.Keys);
        AssertSame(L(new byte[1]), replaced["__proto__"]);
        Assert.IsType<Dictionary<string, object?>>(replaced["a"]);
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
