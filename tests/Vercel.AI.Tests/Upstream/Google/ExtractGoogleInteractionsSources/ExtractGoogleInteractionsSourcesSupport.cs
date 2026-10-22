// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.ExtractGoogleInteractionsSources;

/// <summary>The <c>generateId</c> of the upstream test file: <c>id-1</c>, <c>id-2</c>, and so on.</summary>
internal sealed class IdCounter
{
    private int _next;

    public string Next()
    {
        return "id-" + (++_next);
    }
}

internal static class SourceAssert
{
    public static void Equal(string expected, LanguageModelStreamPart? actual)
    {
        Assert.NotNull(actual);
        var node = GoogleInteractionsStreamSupport.Snapshot(actual!);
        var expectedNode = JsonNode.Parse(expected)!.AsObject();
        Assert.True(JsonNode.DeepEquals(expectedNode, node), node.ToJsonString() + " != " + expected);
    }

    public static void Equal(string expected, IEnumerable<LanguageModelStreamPart> actual)
    {
        var node = GoogleInteractionsStreamSupport.Snapshot(actual);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), node), node.ToJsonString() + " != " + expected);
    }

    public static List<string?> Urls(IEnumerable<LanguageModelStreamPart> sources)
    {
        return sources.Select(source => (source as SourceStreamPart)?.Url).ToList();
    }
}
