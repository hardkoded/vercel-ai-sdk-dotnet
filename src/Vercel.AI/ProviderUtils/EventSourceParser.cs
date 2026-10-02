// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;

namespace Vercel.AI.ProviderUtils;

/// <summary>One Server-Sent Event. Maps to <c>EventSourceMessage</c>.</summary>
public sealed class EventSourceMessage
{
    /// <summary>Creates an event.</summary>
    public EventSourceMessage(string data, string eventName, string id, int? retry)
    {
        Data = data ?? string.Empty;
        EventName = eventName ?? string.Empty;
        Id = id ?? string.Empty;
        Retry = retry;
    }

    /// <summary>Event data, without the trailing newline added by the parser.</summary>
    public string Data { get; }

    /// <summary>Event type. Empty when the block did not set one.</summary>
    public string EventName { get; }

    /// <summary>Last event id.</summary>
    public string Id { get; }

    /// <summary>Reconnection time, when the block set one.</summary>
    public int? Retry { get; }
}

/// <summary>
/// Incremental WHATWG event-stream parser. Maps to <c>eventsource-parser</c>.
/// Does not change <see cref="SseParser.ReadDataAsync"/>.
/// </summary>
public sealed class EventSourceParser
{
    private readonly StringBuilder _pending = new StringBuilder();
    private readonly StringBuilder _data = new StringBuilder();
    private string _event = string.Empty;
    private string _id = string.Empty;
    private int? _retry;
    private bool _sawCarriageReturn;

    /// <summary>Feeds a text chunk and returns events completed by it.</summary>
    public IReadOnlyList<EventSourceMessage> Push(string text)
    {
        var events = new List<EventSourceMessage>();
        if (text is null)
        {
            return events;
        }

        for (var i = 0; i < text.Length; i++)
        {
            var character = text[i];
            if (character == '\r')
            {
                FinishLine(events);
                _sawCarriageReturn = true;
            }
            else if (character == '\n')
            {
                if (_sawCarriageReturn)
                {
                    _sawCarriageReturn = false;
                    continue;
                }

                FinishLine(events);
            }
            else
            {
                _sawCarriageReturn = false;
                _pending.Append(character);
            }
        }

        return events;
    }

    /// <summary>Finishes a trailing line that was not terminated.</summary>
    public IReadOnlyList<EventSourceMessage> Flush()
    {
        var events = new List<EventSourceMessage>();
        if (_pending.Length > 0)
        {
            FinishLine(events);
        }

        return events;
    }

    private void FinishLine(List<EventSourceMessage> events)
    {
        var line = _pending.ToString();
        _pending.Clear();
        if (line.Length == 0)
        {
            Dispatch(events);
            return;
        }

        if (line[0] == ':')
        {
            return;
        }

        var colon = line.IndexOf(':');
        string field;
        string value;
        if (colon < 0)
        {
            field = line;
            value = string.Empty;
        }
        else
        {
            field = line.Substring(0, colon);
            value = line.Substring(colon + 1);
            if (value.Length > 0 && value[0] == ' ')
            {
                value = value.Substring(1);
            }
        }

        if (field == "data")
        {
            _data.Append(value);
            _data.Append('\n');
        }
        else if (field == "event")
        {
            _event = value;
        }
        else if (field == "id")
        {
            if (value.IndexOf('\0') < 0)
            {
                _id = value;
            }
        }
        else if (field == "retry" && value.Length > 0 && AllDigits(value))
        {
            int parsed;
            if (int.TryParse(value, out parsed))
            {
                _retry = parsed;
            }
        }
    }

    private void Dispatch(List<EventSourceMessage> events)
    {
        if (_data.Length == 0)
        {
            return;
        }

        if (_data[_data.Length - 1] == '\n')
        {
            _data.Length = _data.Length - 1;
        }

        events.Add(new EventSourceMessage(_data.ToString(), _event, _id, _retry));
        _data.Clear();
        _event = string.Empty;
    }

    private static bool AllDigits(string value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] < '0' || value[i] > '9')
            {
                return false;
            }
        }

        return true;
    }
}
