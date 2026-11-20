// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.ProviderUtils.JsonBinaryFieldScanner.ScannerSupport;
using Scanner = Vercel.AI.ProviderUtils.JsonBinaryFieldScanner;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.JsonBinaryFieldScanner;

/// <summary>Port of <c>json-binary-field-scanner.test.ts</c> &gt; <c>createJsonBinaryFieldScanner &gt; differential test against JSON.parse and atob</c>.</summary>
public sealed class ScannerDifferentialTests
{
    private const string Prefix =
        TestFile + "createJsonBinaryFieldScanner > differential test against JSON.parse and atob::";

    // Strings at the paths: canonical ones (checked with an independent oracle
    // below), and many ways to be almost canonical.
    private static string RandomString(Func<double> random)
    {
        var roll = (int)Math.Floor(random() * 14);
        var length = (int)Math.Floor(random() * 60);
        var valid = Base64Of(length, (uint)(Math.Floor(random() * 1e6) + 1));
        switch (roll)
        {
            case 0:
                return valid.TrimEnd('=');
            case 1:
                return valid + "=";
            case 2:
                return valid.Length > 2 ? valid[..2] + "-_" + valid[4..] : "-";
            case 3:
                return "data:image/png;base64," + valid;
            case 4:
                var half = valid.Length / 2;
                return valid[..half] + "\n" + valid[half..];
            case 5:
                return "";
            case 6:
                return string.Concat(Enumerable.Repeat("é", (int)Math.Floor(random() * 5)));
            case 7:
                return (valid.Length > 0 ? valid[..^1] : "") + (valid.EndsWith('=') ? "B" : "=");
            default:
                return valid;
        }
    }

    private static object? Noise(Func<double> random, int depth)
    {
        var roll = (int)Math.Floor(random() * 8);
        if (depth > 3 || roll < 3)
        {
            object?[] scalars = [1, "text", null, true, "q\"uote", "\\", "é"];
            return scalars[(int)Math.Floor(random() * 7)];
        }

        if (roll < 5)
        {
            var count = (int)Math.Floor(random() * 3);
            var list = new List<object?>();
            for (var index = 0; index < count; index++)
            {
                list.Add(Noise(random, depth + 1));
            }

            return list;
        }

        var obj = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var key in new[] { "images", "content", "data", "x", "Images", "type" })
        {
            if (random() < 0.4)
            {
                obj[key] = Noise(random, depth + 1);
            }
        }

        return obj;
    }

    private static object? RandomDocument(Func<double> random)
    {
        object? Field() => random() < 0.15 ? Noise(random, 1) : RandomString(random);
        var document = new Dictionary<string, object?>(StringComparer.Ordinal);
        if (random() < 0.8)
        {
            var list = new List<object?>();
            var count = (int)Math.Floor(random() * 4);
            for (var index = 0; index < count; index++)
            {
                list.Add(Field());
            }

            document["images"] = list;
        }

        if (random() < 0.8)
        {
            var list = new List<object?>();
            var count = (int)Math.Floor(random() * 4);
            for (var index = 0; index < count; index++)
            {
                list.Add(
                    random() < 0.2
                        ? Noise(random, 1)
                        : O(
                            ("type", "file"),
                            (
                                "data",
                                random() < 0.15
                                    ? Noise(random, 1)
                                    : O(("type", "data"), ("data", Field()))
                            )
                        )
                );
            }

            document["content"] = list;
        }

        if (random() < 0.5)
        {
            document["noise"] = Noise(random, 0);
        }

        return document;
    }

    // Re-encoding what was decoded only reproduces the string if the string was canonical.
    private static bool TryDecodeCanonical(string text, out byte[] bytes)
    {
        try
        {
            bytes = Convert.FromBase64String(text);
            return Convert.ToBase64String(bytes) == text;
        }
        catch (FormatException)
        {
            bytes = [];
            return false;
        }
    }

    private static object? Plain(JsonElement value) =>
        Scanner.ReplaceBinaryFieldPlaceholders(value, new Dictionary<string, byte[]>());

    private static object? ExpectedValue(JsonElement value, List<string[]> paths, int minBytes)
    {
        object? Visit(JsonElement current, List<string[]> level)
        {
            switch (current.ValueKind)
            {
                case JsonValueKind.String:
                    var text = current.GetString()!;
                    return
                        level.Any(path => path.Length == 0)
                        && TryDecodeCanonical(text, out var bytes)
                        && bytes.Length >= minBytes
                        ? bytes
                        : text;
                case JsonValueKind.Array:
                    var next = level
                        .Where(path => path.Length > 0 && path[0] == "*")
                        .Select(path => path[1..])
                        .ToList();
                    return next.Count == 0
                        ? Plain(current)
                        : current.EnumerateArray().Select(item => Visit(item, next)).ToList();
                case JsonValueKind.Object:
                    var result = new Dictionary<string, object?>(StringComparer.Ordinal);
                    foreach (var property in current.EnumerateObject())
                    {
                        var inner = level
                            .Where(path => path.Length > 0 && path[0] == property.Name)
                            .Select(path => path[1..])
                            .ToList();
                        result[property.Name] =
                            inner.Count > 0 ? Visit(property.Value, inner) : Plain(property.Value);
                    }

                    return result;
                default:
                    return Plain(current);
            }
        }

        return Visit(value, paths);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "matches the reference implementation for random documents and chunkings",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Matches_the_reference_implementation_for_random_documents_and_chunkings()
    {
        var random = CreateRandom(20260101);
        string[] paths = [.. Images, .. Files];
        List<string[]> referencePaths =
        [
            ["images", "*"],
            ["content", "*", "data", "data"],
        ];

        for (var iteration = 0; iteration < 600; iteration++)
        {
            var document = RandomDocument(random);
            var body = JsonSerializer.Serialize(
                document,
                new JsonSerializerOptions
                {
                    WriteIndented = random() < 0.3,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                }
            );
            var minBytes = new[] { 0, 1, 8, 20 }[(int)Math.Floor(random() * 4)];
            var chunkSize = new[] { 1, 2, 3, 5, 8, 13, 64, 4096 }[(int)Math.Floor(random() * 8)];

            using var parsedBody = JsonDocument.Parse(body);
            var expected = ExpectedValue(parsedBody.RootElement, referencePaths, minBytes);
            var scanned = Scan(body, paths, chunkSize, minBytes);
            AssertSame(expected, ParseScanned(scanned));

            // Writing the extracted strings back gives the original body.
            var restored = scanned.Text;
            foreach (var (placeholder, bytes) in scanned.Binaries)
            {
                var at = restored.IndexOf(placeholder, StringComparison.Ordinal);
                restored =
                    restored[..at]
                    + Convert.ToBase64String(bytes)
                    + restored[(at + placeholder.Length)..];
            }

            Assert.Equal(body, restored);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "produces random chunk boundaries independent of the result",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Produces_random_chunk_boundaries_independent_of_the_result()
    {
        var random = CreateRandom(7);
        string[] paths = [.. Images, .. Files];
        var document = O(
            ("images", L(Base64Of(300, 1), "x", Base64Of(301, 2))),
            (
                "content",
                L(O(("type", "file"), ("data", O(("type", "data"), ("data", Base64Of(302, 3))))))
            ),
            ("noise", O(("a", L("é", "\\\"", "{"))))
        );
        var bytes = Encoding.UTF8.GetBytes(Json(document));
        var reference = ParseScanned(Scan(bytes, paths, bytes.Length));

        for (var iteration = 0; iteration < 200; iteration++)
        {
            var chunks = new List<(byte[], int)>();
            for (var at = 0; at < bytes.Length; )
            {
                var size = 1 + (int)Math.Floor(random() * (random() < 0.8 ? 12 : 400));
                var count = Math.Min(size, bytes.Length - at);
                chunks.Add((bytes[at..(at + count)], count));
                at += size;
            }

            AssertSame(reference, ParseScanned(Scan(bytes, paths, chunks: chunks)));
        }
    }
}
