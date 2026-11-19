// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Vercel.AI.Util;

/// <summary>
/// The outcome of one <see cref="ArrayOutputStreamParser"/> call that did not fall back: the elements published so far and the
/// text that extends the JSON array published so far.
/// </summary>
internal sealed class ArrayOutputStreamParseResult
{
    // Maps to the 'fallback' result. The caller continues with the stateless partial parser.
    public static readonly ArrayOutputStreamParseResult Fallback = new([], string.Empty);

    public ArrayOutputStreamParseResult(IReadOnlyList<JsonElement> partial, string textDelta)
    {
        Partial = partial;
        TextDelta = textDelta;
    }

    public IReadOnlyList<JsonElement> Partial { get; }

    public string TextDelta { get; }
}

/// <summary>Maps to <c>ArrayOutputStreamParser</c>. Returns null for undefined, and <see cref="ArrayOutputStreamParseResult.Fallback"/> for 'fallback'.</summary>
internal delegate ArrayOutputStreamParseResult? ArrayOutputStreamParser(string textDelta);

internal static class ArrayOutputStream
{
    private static readonly Regex PrefixPattern = new(
        "^\\s*\\{\\s*\"elements\"\\s*:\\s*\\[\\z",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1)
    );

    // Maps to createArrayStreamParser. The parser state belongs to a stream, not to the reusable output specification.
    internal static ArrayOutputStreamParser? CreateArrayStreamParser(OutputSpec output)
    {
        return output.CreateArrayStreamParser();
    }

    // Maps to createArrayOutputStreamParser.
    internal static ArrayOutputStreamParser CreateArrayOutputStreamParser(
        JsonNode schema,
        bool skipInvalidElements
    )
    {
        return CreateArrayOutputStreamParser(
            element => ProviderUtils.JsonParsing.SafeValidateTypes(element, schema),
            skipInvalidElements
        );
    }

    /// <summary>
    /// Scan only new text and parse/validate each delimited element once. Keep the last element pending until another starts
    /// or the enclosing object closes, matching the partial JSON parser's treatment of an incomplete response.
    /// Nonstandard envelopes and malformed JSON use the existing partial parser.
    /// </summary>
    internal static ArrayOutputStreamParser CreateArrayOutputStreamParser(
        Func<JsonElement, ProviderUtils.JsonParseResult> validate,
        bool skipInvalidElements
    )
    {
        var phase = Phase.Prefix;
        var prefix = new StringBuilder();
        var elementText = new StringBuilder();
        var depth = 0;
        var inString = false;
        var escaped = false;
        var afterComma = false;
        JsonElement? pending = null;
        var elements = new List<JsonElement>();
        int? publishedCount = null;

        bool FinishElement()
        {
            var parsed = ProviderUtils.JsonParsing.SafeParse(elementText.ToString());
            if (!parsed.Success)
            {
                return false;
            }

            var validated = validate(parsed.Value!.Value);
            if (validated.Success)
            {
                pending = validated.Value;
            }
            else if (!skipInvalidElements)
            {
                return false;
            }

            elementText.Clear();
            return true;
        }

        void PublishPending()
        {
            if (pending != null)
            {
                elements.Add(pending.Value);
                pending = null;
            }
        }

        return textDelta =>
        {
            foreach (var character in textDelta)
            {
                if (phase == Phase.Prefix)
                {
                    prefix.Append(character);
                    if (character == '[')
                    {
                        // Bound prefix scanning; other valid object shapes retain their
                        // existing behavior through the stateless parser.
                        if (!PrefixPattern.IsMatch(prefix.ToString()))
                        {
                            return ArrayOutputStreamParseResult.Fallback;
                        }

                        phase = Phase.Elements;
                    }
                    else if (prefix.Length > 256)
                    {
                        return ArrayOutputStreamParseResult.Fallback;
                    }

                    continue;
                }

                if (phase is Phase.Suffix or Phase.Done)
                {
                    if (char.IsWhiteSpace(character))
                    {
                        continue;
                    }

                    if (phase == Phase.Suffix && character == '}')
                    {
                        PublishPending();
                        phase = Phase.Done;
                        continue;
                    }

                    return ArrayOutputStreamParseResult.Fallback;
                }

                if (!inString && depth == 0 && (character == ',' || character == ']'))
                {
                    if (elementText.Length > 0)
                    {
                        if (!FinishElement())
                        {
                            return ArrayOutputStreamParseResult.Fallback;
                        }
                    }
                    else if (character == ',' || afterComma)
                    {
                        return ArrayOutputStreamParseResult.Fallback;
                    }

                    afterComma = character == ',';
                    if (character == ']')
                    {
                        phase = Phase.Suffix;
                    }

                    continue;
                }

                if (elementText.Length == 0)
                {
                    if (char.IsWhiteSpace(character))
                    {
                        continue;
                    }

                    PublishPending();
                    afterComma = false;
                }

                elementText.Append(character);

                if (inString)
                {
                    if (escaped)
                    {
                        escaped = false;
                    }
                    else if (character == '\\')
                    {
                        escaped = true;
                    }
                    else if (character == '"')
                    {
                        inString = false;
                    }
                }
                else if (character == '"')
                {
                    inString = true;
                }
                else if (character is '{' or '[')
                {
                    depth++;
                }
                else if (character is '}' or ']')
                {
                    depth--;
                }
            }

            if (phase == Phase.Prefix || publishedCount == elements.Count)
            {
                return null;
            }

            var previousCount = publishedCount ?? 0;
            var delta = new StringBuilder();
            if (publishedCount == null)
            {
                delta.Append('[');
            }

            if (previousCount > 0 && elements.Count > previousCount)
            {
                delta.Append(',');
            }

            delta.Append(
                string.Join(
                    ",",
                    elements.Skip(previousCount).Select(value => JsonSerializer.Serialize(value))
                )
            );
            if (phase == Phase.Done)
            {
                delta.Append(']');
            }

            publishedCount = elements.Count;

            // A snapshot prevents subsequent chunks from mutating already emitted arrays.
            return new ArrayOutputStreamParseResult(elements.ToArray(), delta.ToString());
        };
    }

    private enum Phase
    {
        Prefix,
        Elements,
        Suffix,
        Done,
    }
}
