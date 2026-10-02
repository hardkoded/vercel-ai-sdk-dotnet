// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;

namespace Vercel.AI.Util;

/// <summary>Repairs truncated JSON text. Maps to <c>fixJson</c>.</summary>
public static partial class JsonRepair
{
    private enum State
    {
        Root,
        Finish,
        InsideString,
        InsideStringEscape,
        InsideStringUnicodeEscape,
        InsideLiteral,
        InsideNumber,
        InsideObjectStart,
        InsideObjectKey,
        InsideObjectAfterKey,
        InsideObjectBeforeValue,
        InsideObjectAfterValue,
        InsideObjectAfterComma,
        InsideArrayStart,
        InsideArrayAfterValue,
        InsideArrayAfterComma,
    }

    /// <summary>
    /// Scans <paramref name="input"/> once and closes strings, literals, arrays, and objects
    /// that were cut off. Invalid JSON is left for a later parser to reject.
    /// </summary>
    public static string FixJson(string input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        var stack = new List<State> { State.Root };
        var lastValidIndex = -1;
        int? literalStart = null;
        var unicodeEscapeDigits = 0;

        void Pop()
        {
            stack.RemoveAt(stack.Count - 1);
        }

        void Push(State state)
        {
            stack.Add(state);
        }

        State Peek()
        {
            return stack[stack.Count - 1];
        }

        void ProcessValueStart(char character, int index, State swapState)
        {
            switch (character)
            {
                case '"':
                    lastValidIndex = index;
                    Pop();
                    Push(swapState);
                    Push(State.InsideString);
                    break;
                case 'f':
                case 't':
                case 'n':
                    lastValidIndex = index;
                    literalStart = index;
                    Pop();
                    Push(swapState);
                    Push(State.InsideLiteral);
                    break;
                case '-':
                    Pop();
                    Push(swapState);
                    Push(State.InsideNumber);
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    lastValidIndex = index;
                    Pop();
                    Push(swapState);
                    Push(State.InsideNumber);
                    break;
                case '{':
                    lastValidIndex = index;
                    Pop();
                    Push(swapState);
                    Push(State.InsideObjectStart);
                    break;
                case '[':
                    lastValidIndex = index;
                    Pop();
                    Push(swapState);
                    Push(State.InsideArrayStart);
                    break;
            }
        }

        void ProcessAfterObjectValue(char character, int index)
        {
            switch (character)
            {
                case ',':
                    Pop();
                    Push(State.InsideObjectAfterComma);
                    break;
                case '}':
                    lastValidIndex = index;
                    Pop();
                    break;
            }
        }

        void ProcessAfterArrayValue(char character, int index)
        {
            switch (character)
            {
                case ',':
                    Pop();
                    Push(State.InsideArrayAfterComma);
                    break;
                case ']':
                    lastValidIndex = index;
                    Pop();
                    break;
            }
        }

        for (var i = 0; i < input.Length; i++)
        {
            var character = input[i];
            switch (Peek())
            {
                case State.Root:
                    ProcessValueStart(character, i, State.Finish);
                    break;
                case State.InsideObjectStart:
                    switch (character)
                    {
                        case '"':
                            Pop();
                            Push(State.InsideObjectKey);
                            break;
                        case '}':
                            lastValidIndex = i;
                            Pop();
                            break;
                    }

                    break;
                case State.InsideObjectAfterComma:
                    if (character == '"')
                    {
                        Pop();
                        Push(State.InsideObjectKey);
                    }

                    break;
                case State.InsideObjectKey:
                    if (character == '"')
                    {
                        Pop();
                        Push(State.InsideObjectAfterKey);
                    }

                    break;
                case State.InsideObjectAfterKey:
                    if (character == ':')
                    {
                        Pop();
                        Push(State.InsideObjectBeforeValue);
                    }

                    break;
                case State.InsideObjectBeforeValue:
                    ProcessValueStart(character, i, State.InsideObjectAfterValue);
                    break;
                case State.InsideObjectAfterValue:
                    ProcessAfterObjectValue(character, i);
                    break;
                case State.InsideString:
                    switch (character)
                    {
                        case '"':
                            Pop();
                            lastValidIndex = i;
                            break;
                        case '\\':
                            Push(State.InsideStringEscape);
                            break;
                        default:
                            lastValidIndex = i;
                            break;
                    }

                    break;
                case State.InsideArrayStart:
                    if (character == ']')
                    {
                        lastValidIndex = i;
                        Pop();
                    }
                    else
                    {
                        lastValidIndex = i;
                        ProcessValueStart(character, i, State.InsideArrayAfterValue);
                    }

                    break;
                case State.InsideArrayAfterValue:
                    switch (character)
                    {
                        case ',':
                            Pop();
                            Push(State.InsideArrayAfterComma);
                            break;
                        case ']':
                            lastValidIndex = i;
                            Pop();
                            break;
                        default:
                            lastValidIndex = i;
                            break;
                    }

                    break;
                case State.InsideArrayAfterComma:
                    ProcessValueStart(character, i, State.InsideArrayAfterValue);
                    break;
                case State.InsideStringEscape:
                    Pop();
                    if (character == 'u')
                    {
                        unicodeEscapeDigits = 0;
                        Push(State.InsideStringUnicodeEscape);
                    }
                    else
                    {
                        lastValidIndex = i;
                    }

                    break;
                case State.InsideStringUnicodeEscape:
                    if (IsHexDigit(character))
                    {
                        unicodeEscapeDigits++;
                        if (unicodeEscapeDigits == 4)
                        {
                            Pop();
                            lastValidIndex = i;
                        }
                    }

                    break;
                case State.InsideNumber:
                    switch (character)
                    {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            lastValidIndex = i;
                            break;
                        case 'e':
                        case 'E':
                        case '-':
                        case '.':
                            break;
                        case ',':
                            Pop();
                            if (Peek() == State.InsideArrayAfterValue)
                            {
                                ProcessAfterArrayValue(character, i);
                            }

                            if (Peek() == State.InsideObjectAfterValue)
                            {
                                ProcessAfterObjectValue(character, i);
                            }

                            break;
                        case '}':
                            Pop();
                            if (Peek() == State.InsideObjectAfterValue)
                            {
                                ProcessAfterObjectValue(character, i);
                            }

                            break;
                        case ']':
                            Pop();
                            if (Peek() == State.InsideArrayAfterValue)
                            {
                                ProcessAfterArrayValue(character, i);
                            }

                            break;
                        default:
                            Pop();
                            break;
                    }

                    break;
                case State.InsideLiteral:
                    var partialLiteral = input.Substring(literalStart ?? 0, i + 1 - (literalStart ?? 0));
                    if (!"false".StartsWith(partialLiteral, StringComparison.Ordinal)
                        && !"true".StartsWith(partialLiteral, StringComparison.Ordinal)
                        && !"null".StartsWith(partialLiteral, StringComparison.Ordinal))
                    {
                        Pop();
                        if (Peek() == State.InsideObjectAfterValue)
                        {
                            ProcessAfterObjectValue(character, i);
                        }
                        else if (Peek() == State.InsideArrayAfterValue)
                        {
                            ProcessAfterArrayValue(character, i);
                        }
                    }
                    else
                    {
                        lastValidIndex = i;
                    }

                    break;
            }
        }

        var result = new StringBuilder(input.Substring(0, lastValidIndex + 1));
        for (var i = stack.Count - 1; i >= 0; i--)
        {
            switch (stack[i])
            {
                case State.InsideString:
                    result.Append('"');
                    break;
                case State.InsideObjectKey:
                case State.InsideObjectAfterKey:
                case State.InsideObjectAfterComma:
                case State.InsideObjectStart:
                case State.InsideObjectBeforeValue:
                case State.InsideObjectAfterValue:
                    result.Append('}');
                    break;
                case State.InsideArrayStart:
                case State.InsideArrayAfterComma:
                case State.InsideArrayAfterValue:
                    result.Append(']');
                    break;
                case State.InsideLiteral:
                    var partialLiteral = input.Substring(literalStart ?? 0);
                    if ("true".StartsWith(partialLiteral, StringComparison.Ordinal))
                    {
                        result.Append("true".Substring(partialLiteral.Length));
                    }
                    else if ("false".StartsWith(partialLiteral, StringComparison.Ordinal))
                    {
                        result.Append("false".Substring(partialLiteral.Length));
                    }
                    else if ("null".StartsWith(partialLiteral, StringComparison.Ordinal))
                    {
                        result.Append("null".Substring(partialLiteral.Length));
                    }

                    break;
            }
        }

        return result.ToString();
    }

    private static bool IsHexDigit(char character)
    {
        return (character >= '0' && character <= '9')
            || (character >= 'A' && character <= 'F')
            || (character >= 'a' && character <= 'f');
    }
}
