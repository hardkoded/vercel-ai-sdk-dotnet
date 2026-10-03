// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;

namespace Vercel.AI.ProviderUtils;

/// <summary>One Server-Sent Event. Field names are compared without case folding.</summary>
public sealed class EventSourceMessage
{
    /// <summary>Creates a message.</summary>
    public EventSourceMessage(string data, string? eventType, string? id)
    {
        Data = data ?? string.Empty;
        Event = string.IsNullOrEmpty(eventType) ? null : eventType;
        Id = id;
    }

    /// <summary>Joined <c>data</c> fields.</summary>
    public string Data { get; }

    /// <summary>Event type. Null when the stream did not set one.</summary>
    public string? Event { get; }

    /// <summary>Event id. Null when the stream did not set one.</summary>
    public string? Id { get; }
}

/// <summary>
/// Incremental Server-Sent Events parser. A UTF-8 BOM is ignored at the start of the
/// stream. A trailing carriage return is held until the next chunk so it can form
/// a <c>\r\n</c> line ending. <c>[DONE]</c> is ordinary data.
/// </summary>
public sealed class EventSourceParser
{
    private readonly StringBuilder _pending = new StringBuilder();
    private readonly StringBuilder _data = new StringBuilder();
    private bool _firstChunk = true;
    private int _dataLines;
    private string? _id;
    private string? _eventType;

    /// <summary>Feeds the next text chunk. Callbacks run for events completed by this chunk.</summary>
    public void Feed(
        string? chunk,
        Action<EventSourceMessage> onEvent,
        Action<int>? onRetry = null,
        Action<string>? onComment = null,
        Action<string>? onError = null)
    {
        if (onEvent is null)
        {
            throw new ArgumentNullException(nameof(onEvent));
        }

        if (string.IsNullOrEmpty(chunk))
        {
            return;
        }

        if (_firstChunk)
        {
            _firstChunk = false;
            chunk = StripBom(chunk!);
            if (chunk.Length == 0)
            {
                return;
            }
        }

        _pending.Append(chunk);
        var text = _pending.ToString();
        var index = 0;
        while (index < text.Length)
        {
            var lineEnd = NextLineEnd(text, index);
            if (lineEnd < 0)
            {
                break;
            }

            if (lineEnd == text.Length - 1 && text[lineEnd] == '\r')
            {
                break;
            }

            ParseLine(text, index, lineEnd, onEvent, onRetry, onComment, onError);
            index = lineEnd + 1;
            if (lineEnd < text.Length && text[lineEnd] == '\r' && index < text.Length && text[index] == '\n')
            {
                index++;
            }
        }

        _pending.Clear();
        if (index < text.Length)
        {
            _pending.Append(text, index, text.Length - index);
        }
    }

    /// <summary>
    /// Clears parser state. When <paramref name="consume"/> is true, an incomplete
    /// line is parsed first. That can report <c>retry</c>, comments, and errors.
    /// It does not dispatch a data event.
    /// </summary>
    public void Reset(
        bool consume = false,
        Action<int>? onRetry = null,
        Action<string>? onComment = null,
        Action<string>? onError = null)
    {
        if (consume && _pending.Length > 0)
        {
            var line = _pending.ToString();
            ParseLine(line, 0, line.Length, _ => { }, onRetry, onComment, onError);
        }

        _pending.Clear();
        _data.Clear();
        _dataLines = 0;
        _id = null;
        _eventType = null;
        _firstChunk = true;
    }

    private void ParseLine(
        string text,
        int start,
        int end,
        Action<EventSourceMessage> onEvent,
        Action<int>? onRetry,
        Action<string>? onComment,
        Action<string>? onError)
    {
        if (start == end)
        {
            Dispatch(onEvent);
            return;
        }

        if (text[start] == ':')
        {
            if (onComment != null)
            {
                var commentStart = start + 1;
                if (commentStart < end && text[commentStart] == ' ')
                {
                    commentStart++;
                }

                onComment(text.Substring(commentStart, end - commentStart));
            }

            return;
        }

        var separator = IndexOfColon(text, start, end);
        string field;
        string value;
        if (separator < 0)
        {
            field = text.Substring(start, end - start);
            value = string.Empty;
        }
        else
        {
            field = text.Substring(start, separator - start);
            var valueStart = separator + 1;
            if (valueStart < end && text[valueStart] == ' ')
            {
                valueStart++;
            }

            value = valueStart >= end ? string.Empty : text.Substring(valueStart, end - valueStart);
        }

        switch (field)
        {
            case "data":
                if (_dataLines > 0)
                {
                    _data.Append('\n');
                }

                _data.Append(value);
                _dataLines++;
                break;
            case "event":
                _eventType = value.Length == 0 ? null : value;
                break;
            case "id":
                _id = value.IndexOf('\0') >= 0 ? null : value;
                break;
            case "retry":
                if (IsDigits(value) && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var retry))
                {
                    if (onRetry != null)
                    {
                        onRetry(retry);
                    }
                }
                else if (onError != null)
                {
                    onError("Invalid `retry` value: \"" + value + "\"");
                }

                break;
            default:
                if (onError != null)
                {
                    var shown = field.Length > 20 ? field.Substring(0, 20) + "…" : field;
                    onError("Unknown field \"" + shown + "\"");
                }

                break;
        }
    }

    private void Dispatch(Action<EventSourceMessage> onEvent)
    {
        if (_dataLines > 0)
        {
            onEvent(new EventSourceMessage(_data.ToString(), _eventType, _id));
        }

        _id = null;
        _eventType = null;
        _data.Clear();
        _dataLines = 0;
    }

    private static int NextLineEnd(string text, int start)
    {
        var carriage = text.IndexOf('\r', start);
        var line = text.IndexOf('\n', start);
        if (carriage < 0)
        {
            return line;
        }

        if (line < 0)
        {
            return carriage;
        }

        return carriage < line ? carriage : line;
    }

    private static int IndexOfColon(string text, int start, int end)
    {
        for (var index = start; index < end; index++)
        {
            if (text[index] == ':')
            {
                return index;
            }
        }

        return -1;
    }

    private static bool IsDigits(string value)
    {
        if (value.Length == 0)
        {
            return false;
        }

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (character < '0' || character > '9')
            {
                return false;
            }
        }

        return true;
    }

    private static string StripBom(string chunk)
    {
        if (chunk[0] == '\uFEFF')
        {
            return chunk.Substring(1);
        }

        if (chunk.Length >= 3 && chunk[0] == '\u00EF' && chunk[1] == '\u00BB' && chunk[2] == '\u00BF')
        {
            return chunk.Substring(3);
        }

        return chunk;
    }
}
