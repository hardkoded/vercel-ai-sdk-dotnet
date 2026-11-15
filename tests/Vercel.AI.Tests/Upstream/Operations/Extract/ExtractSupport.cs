// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Operations.Extract;

/// <summary>Shared fixtures for the <c>extract.test.ts</c> ports: <c>state</c>, <c>fields</c>, <c>answers</c>, and <c>setup</c>.</summary>
internal static class ExtractSupport
{
    internal const string State = "Invoice INV-2041, total due $1,250.00 by March 3, 2027 at 17:30. Call Ana or Bo within 2 hours.";

    internal static Dictionary<string, ExtractionField> Fields()
    {
        return new Dictionary<string, ExtractionField>
        {
            ["invoiceNumber"] = new ExtractionField("text", "The invoice number"),
            ["total"] = new ExtractionField("number", "Total due") { Unit = "USD", Decimals = 2 },
            ["dueDate"] = new ExtractionField("date", "Due date"),
            ["dueTime"] = new ExtractionField("time", "Due time"),
            ["dueAt"] = new ExtractionField("datetime", "Due date and time"),
            ["callWithin"] = new ExtractionField("duration", "Call window") { Unit = "hours" },
            ["contacts"] = new ExtractionField("list", "Contact names") { Items = "text" },
        };
    }

    internal static Dictionary<string, ExtractionAnswer> Answers()
    {
        return new Dictionary<string, ExtractionAnswer>
        {
            ["invoiceNumber"] = new ExtractionAnswer("text", text: "INV-2041", confidence: 0.98, source: new ExtractionSource("text", 0, 8, 16)),
            ["total"] = new ExtractionAnswer("number", number: 1250, pNull: 0.01),
            ["dueDate"] = new ExtractionAnswer("date", text: "2027-03-03"),
            ["dueTime"] = new ExtractionAnswer("time", text: "17:30"),
            ["dueAt"] = new ExtractionAnswer("datetime", text: "2027-03-03T17:30"),
            ["callWithin"] = new ExtractionAnswer("duration", number: 2),
            ["contacts"] = new ExtractionAnswer(
                "list",
                textItems: ["Ana", "Bo"],
                items: [new ExtractionItem(0.9, new ExtractionSource("text", 0, 70, 73)), new ExtractionItem(0.8)]),
        };
    }

    /// <summary>Maps to <c>setup</c>: a mock model whose <c>doExtract</c> resolves to <paramref name="result"/>.</summary>
    internal static ExtractionMockModel Setup(ExtractionModelResult? result = null)
    {
        var resolved = result ?? new ExtractionModelResult(Answers(), []);
        return new ExtractionMockModel { OnExtract = _ => Task.FromResult(resolved) };
    }

    /// <summary>Answers as the upstream snapshot prints them: <c>value</c> and <c>guess</c> merged back, keys sorted.</summary>
    internal static string Canon(IReadOnlyDictionary<string, ExtractionAnswer> answers)
    {
        var result = new JsonObject();
        foreach (var pair in answers.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            result[pair.Key] = Canon(pair.Value);
        }

        return result.ToJsonString();
    }

    /// <summary>Sorts the keys of a JSON text so it compares with <see cref="Canon(IReadOnlyDictionary{string, ExtractionAnswer})"/>.</summary>
    internal static string Sorted(string json)
    {
        return Sort(JsonNode.Parse(json))?.ToJsonString() ?? "null";
    }

    private static JsonNode? Sort(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                var sorted = new JsonObject();
                foreach (var pair in obj.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                {
                    sorted[pair.Key] = Sort(pair.Value?.DeepClone());
                }

                return sorted;
            case JsonArray array:
                return new JsonArray(array.Select(item => Sort(item?.DeepClone())).ToArray());
            default:
                return node?.DeepClone();
        }
    }

    private static JsonObject Canon(ExtractionAnswer answer)
    {
        var node = new SortedDictionary<string, JsonNode?>(StringComparer.Ordinal) { ["type"] = answer.Type };
        if (answer.Type != "refusal")
        {
            node["value"] = answer.Text is { } text ? text
                : answer.Number is { } number ? number
                : answer.TextItems is { } textItems ? new JsonArray(textItems.Select(item => (JsonNode?)item).ToArray())
                : answer.NumberItems is { } numberItems ? new JsonArray(numberItems.Select(item => (JsonNode?)item).ToArray())
                : null;
        }

        if (answer.GuessText is { } guessText)
        {
            node["guess"] = guessText;
        }
        else if (answer.GuessNumber is { } guessNumber)
        {
            node["guess"] = guessNumber;
        }

        if (answer.PNull is { } pNull)
        {
            node["pNull"] = pNull;
        }

        if (answer.Confidence is { } confidence)
        {
            node["confidence"] = confidence;
        }

        if (answer.Source is { } source)
        {
            node["source"] = Canon(source);
        }

        if (answer.Items is { } items)
        {
            node["items"] = new JsonArray(items.Select(item =>
            {
                var entry = new JsonObject();
                if (item.Confidence is { } itemConfidence)
                {
                    entry["confidence"] = itemConfidence;
                }

                if (item.Source is { } itemSource)
                {
                    entry["source"] = Canon(itemSource);
                }

                return (JsonNode?)entry;
            }).ToArray());
        }

        var result = new JsonObject();
        foreach (var pair in node)
        {
            result[pair.Key] = pair.Value;
        }

        return result;
    }

    private static JsonObject Canon(ExtractionSource source)
    {
        var result = new JsonObject();
        if (source.End is { } end)
        {
            result["end"] = end;
        }

        result["part"] = source.Part;
        if (source.Start is { } start)
        {
            result["start"] = start;
        }

        result["type"] = source.Type;
        return result;
    }
}
