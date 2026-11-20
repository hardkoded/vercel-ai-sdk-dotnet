// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.ProviderUtils;
using Scanner = Vercel.AI.ProviderUtils.JsonBinaryFieldScanner;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.JsonBinaryFieldScanner;

/// <summary>Helpers of <c>json-binary-field-scanner.test.ts</c>.</summary>
internal static class ScannerSupport
{
    /// <summary>The upstream commit that adds the scanner tests.</summary>
    public const string Commit = "89aff56";

    public const string TestFile =
        "packages/provider-utils/src/json-binary-field-scanner.test.ts::";

    public static readonly string[] Images = ["images[*]"];

    public static readonly string[] Files = ["content[*].data.data"];

    // Chunk sizes that put chunk boundaries at every position of a group of four,
    // around escape sequences, and far beyond the body.
    public static readonly int[] ChunkSizes = [1, 2, 3, 4, 5, 7, 8, 13, 64, 1021, 64 * 1024];

    public static Func<double> CreateRandom(uint seed)
    {
        var state = seed == 0 ? 1u : seed;
        return () =>
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state / 4294967296.0;
        };
    }

    public static byte[] RandomBytes(int length, uint seed = 1)
    {
        var random = CreateRandom(seed);
        var bytes = new byte[length];
        for (var index = 0; index < length; index++)
        {
            bytes[index] = (byte)Math.Floor(random() * 256);
        }

        return bytes;
    }

    public static string Base64Of(int length, uint seed = 1) =>
        Convert.ToBase64String(RandomBytes(length, seed));

    public static List<(byte[] Buffer, int Count)> Split(byte[] bytes, int size)
    {
        var chunks = new List<(byte[], int)>();
        for (var at = 0; at < bytes.Length; at += size)
        {
            var count = Math.Min(size, bytes.Length - at);
            var chunk = new byte[count];
            Array.Copy(bytes, at, chunk, 0, count);
            chunks.Add((chunk, count));
        }

        return chunks;
    }

    public static JsonBinaryFieldScanResult Scan(
        byte[] body,
        string[]? paths = null,
        int chunkSize = 64 * 1024,
        IEnumerable<(byte[] Buffer, int Count)>? chunks = null,
        int minBytes = 8
    )
    {
        var scanner = new Scanner(Scanner.CompileBinaryFieldPaths(paths ?? Images), minBytes);
        foreach (var (buffer, count) in chunks ?? Split(body, chunkSize))
        {
            scanner.Push(buffer, count);
        }

        return scanner.Finish();
    }

    public static JsonBinaryFieldScanResult Scan(
        string body,
        string[]? paths = null,
        int chunkSize = 64 * 1024,
        int minBytes = 8
    ) => Scan(Encoding.UTF8.GetBytes(body), paths, chunkSize, null, minBytes);

    public static object? ParseScanned(
        string body,
        string[]? paths = null,
        int chunkSize = 64 * 1024,
        int minBytes = 8
    ) => ParseScanned(Scan(body, paths, chunkSize, minBytes));

    public static object? ParseScanned(JsonBinaryFieldScanResult scanned)
    {
        using var document = JsonDocument.Parse(scanned.Text);
        return Scanner.ReplaceBinaryFieldPlaceholders(document.RootElement, scanned.Binaries);
    }

    /// <summary>Deep equality for the values of <see cref="Scanner.ReplaceBinaryFieldPlaceholders"/>.</summary>
    public static void AssertSame(object? expected, object? actual, string path = "$")
    {
        switch (expected)
        {
            case byte[] bytes:
                Assert.True(actual is byte[] other && bytes.AsSpan().SequenceEqual(other), path);
                break;
            case List<object?> list:
                var actualList = Assert.IsType<List<object?>>(actual);
                Assert.Equal(list.Count, actualList.Count);
                for (var index = 0; index < list.Count; index++)
                {
                    AssertSame(list[index], actualList[index], path + "[" + index + "]");
                }

                break;
            case Dictionary<string, object?> map:
                var actualMap = Assert.IsType<Dictionary<string, object?>>(actual);
                Assert.Equal(
                    map.Keys.OrderBy(k => k, StringComparer.Ordinal),
                    actualMap.Keys.OrderBy(k => k, StringComparer.Ordinal)
                );
                foreach (var pair in map)
                {
                    AssertSame(pair.Value, actualMap[pair.Key], path + "." + pair.Key);
                }

                break;
            default:
                Assert.Equal(expected, actual);
                break;
        }
    }

    public static List<object?> L(params object?[] items) => [.. items];

    public static Dictionary<string, object?> O(params (string Key, object? Value)[] entries)
    {
        var map = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var (key, value) in entries)
        {
            map[key] = value;
        }

        return map;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>Like <c>JSON.stringify</c>: base64 characters such as <c>+</c> stay as they are.</summary>
    public static string Json(object? value) => JsonSerializer.Serialize(value, JsonOptions);
}
