// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ResolveProviderOptions;

/// <summary>The options shape of the upstream test schema: <c>a</c> is an optional string and <c>b</c> an optional number.</summary>
internal sealed record TestOptions(string? A = null, double? B = null);

internal static class ResolveProviderOptionsFixture
{
    public static IReadOnlyDictionary<string, JsonElement> ProviderOptions(params (string Name, string Json)[] entries)
    {
        var result = new Dictionary<string, JsonElement>();
        foreach (var (name, json) in entries)
        {
            using var document = JsonDocument.Parse(json);
            result[name] = document.RootElement.Clone();
        }

        return result;
    }

    /// <summary>Validates like the upstream schema does and throws <see cref="InvalidArgumentError"/> for an invalid value.</summary>
    public static TestOptions Parse(JsonElement element)
    {
        string? a = null;
        double? b = null;
        if (element.TryGetProperty("a", out var aValue))
        {
            a = aValue.ValueKind == JsonValueKind.String
                ? aValue.GetString()
                : throw new InvalidArgumentError("providerOptions", element.ToString(), "a must be a string");
        }

        if (element.TryGetProperty("b", out var bValue))
        {
            b = bValue.ValueKind == JsonValueKind.Number
                ? bValue.GetDouble()
                : throw new InvalidArgumentError("providerOptions", element.ToString(), "b must be a number");
        }

        return new TestOptions(a, b);
    }

    public static TestOptions Merge(TestOptions first, TestOptions second)
    {
        return new TestOptions(second.A ?? first.A, second.B ?? first.B);
    }

    public static ResolvedProviderOptions<TestOptions> Resolve(
        IReadOnlyDictionary<string, JsonElement> providerOptions,
        string[] namespaces,
        ProviderOptionsResolution resolution)
    {
        return ProviderOptionsResolver.Resolve(providerOptions, namespaces, resolution, Parse, Merge);
    }
}
