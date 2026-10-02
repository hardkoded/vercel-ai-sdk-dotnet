// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>
/// Tracks whether streamed tool-call arguments contain one complete object or array.
/// The scan is structural, including strings and escapes, so a scalar prefix is not treated as finished JSON.
/// </summary>
internal sealed class StreamingToolCallArgumentState
{
    private enum StructureKind
    {
        Undetermined,
        Other,
        Structured,
    }

    private readonly List<char> _stack = new();
    private StructureKind _kind;
    private bool _inString;
    private bool _escaped;
    private bool _complete;

    /// <summary>Creates a scanner and applies <paramref name="initialValue"/>.</summary>
    public StreamingToolCallArgumentState(string initialValue = "")
    {
        Append(initialValue);
    }

    /// <summary>True after a <c>{...}</c> or <c>[...]</c> value has closed.</summary>
    public bool HasCompleteStructuredValue
    {
        get { return _kind == StructureKind.Structured && _complete; }
    }

    /// <summary>True when the first non-whitespace character is <c>{</c> or <c>[</c>.</summary>
    public static bool StartsWithStructuredValue(string? value)
    {
        if (value == null)
        {
            return false;
        }

        for (var i = 0; i < value.Length; i++)
        {
            if (char.IsWhiteSpace(value[i]))
            {
                continue;
            }

            return value[i] == '{' || value[i] == '[';
        }

        return false;
    }

    /// <summary>Continues the structural scan with the next fragment.</summary>
    public void Append(string delta)
    {
        foreach (var character in delta)
        {
            if (_kind == StructureKind.Undetermined)
            {
                if (char.IsWhiteSpace(character))
                {
                    continue;
                }

                if (character != '{' && character != '[')
                {
                    _kind = StructureKind.Other;
                    continue;
                }

                _kind = StructureKind.Structured;
                _stack.Add(character);
                _inString = false;
                _escaped = false;
                _complete = false;
                continue;
            }

            if (_kind != StructureKind.Structured || _complete)
            {
                continue;
            }

            if (_inString)
            {
                if (_escaped)
                {
                    _escaped = false;
                }
                else if (character == '\\')
                {
                    _escaped = true;
                }
                else if (character == '"')
                {
                    _inString = false;
                }

                continue;
            }

            if (character == '"')
            {
                _inString = true;
            }
            else if (character == '{' || character == '[')
            {
                _stack.Add(character);
            }
            else if (character == '}' || character == ']')
            {
                var expected = character == '}' ? '{' : '[';
                if (_stack.Count == 0 || _stack[_stack.Count - 1] != expected)
                {
                    _kind = StructureKind.Other;
                    continue;
                }

                _stack.RemoveAt(_stack.Count - 1);
                if (_stack.Count == 0)
                {
                    _complete = true;
                }
            }
        }
    }
}
