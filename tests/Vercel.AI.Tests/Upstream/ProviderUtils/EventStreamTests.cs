// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;
using Vercel.AI.Tests.Upstream;

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

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonLinesResponseHandler::parses JSON lines across byte boundaries",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Json_lines_cross_byte_boundaries_and_keep_headers()
    {
        var bytes = Encoding.UTF8.GetBytes("{\"id\":\"first\",\"text\":\"café\"}\r\n\n{\"id\":\"second\",\"text\":\"done\"}");
        using var stream = new MemoryStream();
        stream.Write(bytes, 0, 24);
        stream.Write(bytes, 24, 3);
        stream.Write(bytes, 27, bytes.Length - 27);
        stream.Position = 0;
        var values = new List<JsonElement>();
        await foreach (var value in JsonStreams.ReadJsonLinesAsync(stream))
        {
            values.Add(value);
        }

        Assert.Equal(2, values.Count);
        Assert.Equal("first", values[0].GetProperty("id").GetString());
        Assert.Equal("café", values[0].GetProperty("text").GetString());
        Assert.Equal("done", values[1].GetProperty("text").GetString());

        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        };
        response.Headers.TryAddWithoutValidation("x-test", "value");
        Assert.Equal("value", JsonStreams.ExtractResponseHeaders(response)["x-test"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonLinesResponseHandler::errors when a line is invalid JSON",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Json_lines_reject_invalid_json()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"id\":\"first\"}\n{invalid}\n"));
        var enumerator = JsonStreams.ReadJsonLinesAsync(stream).GetAsyncEnumerator();
        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal("first", enumerator.Current.GetProperty("id").GetString());
        await Assert.ThrowsAnyAsync<Exception>(async () => await enumerator.MoveNextAsync());
        await enumerator.DisposeAsync();
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonLinesResponseHandler::cancels the response body when iteration stops early",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Json_lines_cancel_when_iteration_stops()
    {
        var cancelled = false;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"id\":\"first\"}\n{\"id\":\"second\"}\n"));
        await foreach (var _ in JsonStreams.ReadJsonLinesAsync(stream, onCancel: () => cancelled = true))
        {
            break;
        }

        Assert.True(cancelled);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonLinesResponseHandler::throws EmptyResponseBodyError when the response body is null",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Json_lines_reject_a_missing_body()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in JsonStreams.ReadJsonLinesAsync(null))
            {
            }
        });
        Assert.Contains("Empty response body", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonResponseHandler::should return both parsed value and rawValue",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Json_response_keeps_the_raw_value_and_projects_known_properties()
    {
        var schema = JsonSchemas.Object(
            new[]
            {
                Pair("name", JsonSchemas.String()),
                Pair("age", JsonSchemas.Number()),
            },
            new[] { "name", "age" },
            additionalPropertiesFlag: false);
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{\"name\":\"John\",\"age\":30,\"extraField\":\"ignored\"}", Encoding.UTF8, "application/json"),
        };
        var body = await JsonStreams.ReadJsonAsync(response, schema);
        JsonAssert.Equal(body.Value, "{\"name\":\"John\",\"age\":30}");
        JsonAssert.Equal(body.RawValue, "{\"name\":\"John\",\"age\":30,\"extraField\":\"ignored\"}");
    }

    private static List<EventSourceMessage> Feed(string text, List<string>? errors = null)
    {
        var events = new List<EventSourceMessage>();
        var parser = new EventSourceParser();
        parser.Feed(text, events.Add, onError: errors == null ? null : errors.Add);
        return events;
    }

    private static KeyValuePair<string, JsonNode> Pair(string name, JsonNode schema)
    {
        return new KeyValuePair<string, JsonNode>(name, schema);
    }
}
