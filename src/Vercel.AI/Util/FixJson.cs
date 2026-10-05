// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>
/// Repairs truncated JSON by scanning once and closing open strings, literals, arrays, and objects.
/// </summary>
public static class FixJson
{
    private const string Root = "ROOT";
    private const string Finish = "FINISH";
    private const string InsideString = "INSIDE_STRING";
    private const string InsideStringEscape = "INSIDE_STRING_ESCAPE";
    private const string InsideStringUnicodeEscape = "INSIDE_STRING_UNICODE_ESCAPE";
    private const string InsideLiteral = "INSIDE_LITERAL";
    private const string InsideNumber = "INSIDE_NUMBER";
    private const string InsideObjectStart = "INSIDE_OBJECT_START";
    private const string InsideObjectKey = "INSIDE_OBJECT_KEY";
    private const string InsideObjectAfterKey = "INSIDE_OBJECT_AFTER_KEY";
    private const string InsideObjectBeforeValue = "INSIDE_OBJECT_BEFORE_VALUE";
    private const string InsideObjectAfterValue = "INSIDE_OBJECT_AFTER_VALUE";
    private const string InsideObjectAfterComma = "INSIDE_OBJECT_AFTER_COMMA";
    private const string InsideArrayStart = "INSIDE_ARRAY_START";
    private const string InsideArrayAfterValue = "INSIDE_ARRAY_AFTER_VALUE";
    private const string InsideArrayAfterComma = "INSIDE_ARRAY_AFTER_COMMA";

    /// <summary>Returns <paramref name="input"/> with truncated JSON closed enough for a standard parser.</summary>
    public static string Repair(string? input)
    {
        if (input == null)
        {
            input = string.Empty;
        }

        var stack = new List<string> { Root };
        var lastValidIndex = -1;
        int? literalStart = null;
        var unicodeEscapeDigits = 0;

        for (var i = 0; i < input.Length; i++)
        {
            var ch = input[i];
            var currentState = stack[stack.Count - 1];
            switch (currentState)
            {
                case Root:
                    ProcessValueStart(ch, i, Finish, input, stack, ref lastValidIndex, ref literalStart);
                    break;
                case InsideObjectStart:
                    if (ch == '"')
                    {
                        stack.RemoveAt(stack.Count - 1);
                        stack.Add(InsideObjectKey);
                    }
                    else if (ch == '}')
                    {
                        lastValidIndex = i;
                        stack.RemoveAt(stack.Count - 1);
                    }

                    break;
                case InsideObjectAfterComma:
                    if (ch == '"')
                    {
                        stack.RemoveAt(stack.Count - 1);
                        stack.Add(InsideObjectKey);
                    }

                    break;
                case InsideObjectKey:
                    if (ch == '"')
                    {
                        stack.RemoveAt(stack.Count - 1);
                        stack.Add(InsideObjectAfterKey);
                    }

                    break;
                case InsideObjectAfterKey:
                    if (ch == ':')
                    {
                        stack.RemoveAt(stack.Count - 1);
                        stack.Add(InsideObjectBeforeValue);
                    }

                    break;
                case InsideObjectBeforeValue:
                    ProcessValueStart(ch, i, InsideObjectAfterValue, input, stack, ref lastValidIndex, ref literalStart);
                    break;
                case InsideObjectAfterValue:
                    ProcessAfterObjectValue(ch, i, stack, ref lastValidIndex);
                    break;
                case InsideString:
                    if (ch == '"')
                    {
                        stack.RemoveAt(stack.Count - 1);
                        lastValidIndex = i;
                    }
                    else if (ch == '\\')
                    {
                        stack.Add(InsideStringEscape);
                    }
                    else
                    {
                        lastValidIndex = i;
                    }

                    break;
                case InsideArrayStart:
                    if (ch == ']')
                    {
                        lastValidIndex = i;
                        stack.RemoveAt(stack.Count - 1);
                    }
                    else
                    {
                        lastValidIndex = i;
                        ProcessValueStart(ch, i, InsideArrayAfterValue, input, stack, ref lastValidIndex, ref literalStart);
                    }

                    break;
                case InsideArrayAfterValue:
                    if (ch == ',')
                    {
                        stack.RemoveAt(stack.Count - 1);
                        stack.Add(InsideArrayAfterComma);
                    }
                    else if (ch == ']')
                    {
                        lastValidIndex = i;
                        stack.RemoveAt(stack.Count - 1);
                    }
                    else
                    {
                        lastValidIndex = i;
                    }

                    break;
                case InsideArrayAfterComma:
                    ProcessValueStart(ch, i, InsideArrayAfterValue, input, stack, ref lastValidIndex, ref literalStart);
                    break;
                case InsideStringEscape:
                    stack.RemoveAt(stack.Count - 1);
                    if (ch == 'u')
                    {
                        unicodeEscapeDigits = 0;
                        stack.Add(InsideStringUnicodeEscape);
                    }
                    else
                    {
                        lastValidIndex = i;
                    }

                    break;
                case InsideStringUnicodeEscape:
                    if (IsHexDigit(ch))
                    {
                        unicodeEscapeDigits++;
                        if (unicodeEscapeDigits == 4)
                        {
                            stack.RemoveAt(stack.Count - 1);
                            lastValidIndex = i;
                        }
                    }

                    break;
                case InsideNumber:
                    ProcessNumber(ch, i, stack, ref lastValidIndex);
                    break;
                case InsideLiteral:
                    ProcessLiteral(input, i, stack, literalStart, ref lastValidIndex);
                    break;
            }
        }

        var result = lastValidIndex + 1 <= 0 ? string.Empty : input.Substring(0, lastValidIndex + 1);
        for (var i = stack.Count - 1; i >= 0; i--)
        {
            var state = stack[i];
            switch (state)
            {
                case InsideString:
                    result += "\"";
                    break;
                case InsideObjectKey:
                case InsideObjectAfterKey:
                case InsideObjectAfterComma:
                case InsideObjectStart:
                case InsideObjectBeforeValue:
                case InsideObjectAfterValue:
                    result += "}";
                    break;
                case InsideArrayStart:
                case InsideArrayAfterComma:
                case InsideArrayAfterValue:
                    result += "]";
                    break;
                case InsideLiteral:
                    var partialLiteral = literalStart == null ? string.Empty : input.Substring(literalStart.Value);
                    if ("true".StartsWith(partialLiteral, StringComparison.Ordinal))
                    {
                        result += "true".Substring(partialLiteral.Length);
                    }
                    else if ("false".StartsWith(partialLiteral, StringComparison.Ordinal))
                    {
                        result += "false".Substring(partialLiteral.Length);
                    }
                    else if ("null".StartsWith(partialLiteral, StringComparison.Ordinal))
                    {
                        result += "null".Substring(partialLiteral.Length);
                    }

                    break;
            }
        }

        return result;
    }

    private static void ProcessValueStart(
        char ch,
        int index,
        string swapState,
        string input,
        List<string> stack,
        ref int lastValidIndex,
        ref int? literalStart)
    {
        switch (ch)
        {
            case '"':
                lastValidIndex = index;
                stack.RemoveAt(stack.Count - 1);
                stack.Add(swapState);
                stack.Add(InsideString);
                break;
            case 'f':
            case 't':
            case 'n':
                lastValidIndex = index;
                literalStart = index;
                stack.RemoveAt(stack.Count - 1);
                stack.Add(swapState);
                stack.Add(InsideLiteral);
                break;
            case '-':
                stack.RemoveAt(stack.Count - 1);
                stack.Add(swapState);
                stack.Add(InsideNumber);
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
                stack.RemoveAt(stack.Count - 1);
                stack.Add(swapState);
                stack.Add(InsideNumber);
                break;
            case '{':
                lastValidIndex = index;
                stack.RemoveAt(stack.Count - 1);
                stack.Add(swapState);
                stack.Add(InsideObjectStart);
                break;
            case '[':
                lastValidIndex = index;
                stack.RemoveAt(stack.Count - 1);
                stack.Add(swapState);
                stack.Add(InsideArrayStart);
                break;
        }
    }

    private static void ProcessAfterObjectValue(char ch, int index, List<string> stack, ref int lastValidIndex)
    {
        if (ch == ',')
        {
            stack.RemoveAt(stack.Count - 1);
            stack.Add(InsideObjectAfterComma);
        }
        else if (ch == '}')
        {
            lastValidIndex = index;
            stack.RemoveAt(stack.Count - 1);
        }
    }

    private static void ProcessAfterArrayValue(char ch, int index, List<string> stack, ref int lastValidIndex)
    {
        if (ch == ',')
        {
            stack.RemoveAt(stack.Count - 1);
            stack.Add(InsideArrayAfterComma);
        }
        else if (ch == ']')
        {
            lastValidIndex = index;
            stack.RemoveAt(stack.Count - 1);
        }
    }

    private static void ProcessNumber(char ch, int index, List<string> stack, ref int lastValidIndex)
    {
        switch (ch)
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
                lastValidIndex = index;
                break;
            case 'e':
            case 'E':
            case '-':
            case '.':
                break;
            case ',':
                stack.RemoveAt(stack.Count - 1);
                if (stack.Count > 0 && stack[stack.Count - 1] == InsideArrayAfterValue)
                {
                    ProcessAfterArrayValue(ch, index, stack, ref lastValidIndex);
                }

                if (stack.Count > 0 && stack[stack.Count - 1] == InsideObjectAfterValue)
                {
                    ProcessAfterObjectValue(ch, index, stack, ref lastValidIndex);
                }

                break;
            case '}':
                stack.RemoveAt(stack.Count - 1);
                if (stack.Count > 0 && stack[stack.Count - 1] == InsideObjectAfterValue)
                {
                    ProcessAfterObjectValue(ch, index, stack, ref lastValidIndex);
                }

                break;
            case ']':
                stack.RemoveAt(stack.Count - 1);
                if (stack.Count > 0 && stack[stack.Count - 1] == InsideArrayAfterValue)
                {
                    ProcessAfterArrayValue(ch, index, stack, ref lastValidIndex);
                }

                break;
            default:
                stack.RemoveAt(stack.Count - 1);
                break;
        }
    }

    private static void ProcessLiteral(string input, int index, List<string> stack, int? literalStart, ref int lastValidIndex)
    {
        var start = literalStart ?? 0;
        var partialLiteral = input.Substring(start, index + 1 - start);
        if (!"false".StartsWith(partialLiteral, StringComparison.Ordinal)
            && !"true".StartsWith(partialLiteral, StringComparison.Ordinal)
            && !"null".StartsWith(partialLiteral, StringComparison.Ordinal))
        {
            stack.RemoveAt(stack.Count - 1);
            if (stack.Count > 0 && stack[stack.Count - 1] == InsideObjectAfterValue)
            {
                ProcessAfterObjectValue(input[index], index, stack, ref lastValidIndex);
            }
            else if (stack.Count > 0 && stack[stack.Count - 1] == InsideArrayAfterValue)
            {
                ProcessAfterArrayValue(input[index], index, stack, ref lastValidIndex);
            }
        }
        else
        {
            lastValidIndex = index;
        }
    }

    private static bool IsHexDigit(char ch)
    {
        return (ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f');
    }
}
