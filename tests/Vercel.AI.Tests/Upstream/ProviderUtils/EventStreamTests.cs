// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;
using Vercel.AI.Tests.Upstream;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

public sealed class EventStreamTests
{
    [Fact]
    public void Parser_joins_data_fields_and_keeps_one_leading_space()
    {
        var events = Feed("event: ping\ndata:  hello\ndata: world\nid: abc\n\n");
        var message = Assert.Single(events);
        Assert.Equal(" hello\nworld", message.Data);
        Assert.Equal("ping", message.Event);
        Assert.Equal("abc", message.Id);
    }

    [Fact]
    public void Parser_handles_crlf_comments_retry_and_partial_chunks()
    {
        var parser = new EventSourceParser();
        var events = new List<EventSourceMessage>();
        var retries = new List<int>();
        var comments = new List<string>();
        parser.Feed("retry: 15\r", events.Add, retries.Add, comments.Add);
        parser.Feed("\n: note\r\ndata: hi\r\n\r\n", events.Add, retries.Add, comments.Add);
        Assert.Equal(new[] { 15 }, retries);
        Assert.Equal(new[] { "note" }, comments);
        Assert.Equal("hi", Assert.Single(events).Data);
    }

    [Fact]
    public void Parser_reports_invalid_retry_and_unknown_fields_and_strips_a_bom()
    {
        var errors = new List<string>();
        var events = Feed("\uFEFFdata: [DONE]\n\nfoo: bar\nretry: no\n", errors);
        Assert.Equal("[DONE]", Assert.Single(events).Data);
        Assert.Contains(errors, error => error.Contains("Unknown field"));
        Assert.Contains(errors, error => error.Contains("Invalid `retry` value"));
    }

    [Fact]
    public async Task Json_events_skip_done_and_parse_payloads()
    {
        var bytes = Encoding.UTF8.GetBytes("data: {\"ok\":true}\n\ndata: [DONE]\n\n");
        using var stream = new MemoryStream(bytes);
        var results = new List<JsonParseResult>();
        await foreach (var result in JsonStreams.ReadJsonEventsAsync(stream))
        {
            results.Add(result);
        }

        var parsed = Assert.Single(results);
        Assert.True(parsed.Success);
        Assert.True(parsed.Value!.Value.GetProperty("ok").GetBoolean());
    }

    private static List<EventSourceMessage> Feed(string text, List<string>? errors = null)
    {
        var events = new List<EventSourceMessage>();
        var parser = new EventSourceParser();
        parser.Feed(text, events.Add, onError: errors == null ? null : errors.Add);
        return events;
    }
}
