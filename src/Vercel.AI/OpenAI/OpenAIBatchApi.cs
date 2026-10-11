// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>OpenAI Batch API. Downloads the JSONL output and error files of a finished batch.</summary>
public sealed class OpenAIBatchApi : BatchResultsApiBase
{
    private readonly OpenAIProvider _provider;

    internal OpenAIBatchApi(OpenAIProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public override string Provider => _provider.Name + ".batch";

    /// <inheritdoc />
    public override async Task<IReadOnlyList<BatchItem>> DoGetResultsAsync(BatchOperationCall call, CancellationToken cancellationToken)
    {
        var batchId = call.BatchId ?? string.Empty;
        var headers = _provider.CreateOpenAIHeaders(call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value));
        JsonElement batch;
        using (var document = await _provider.Http.SendJsonAsync(HttpMethod.Get, ApiKeys.Combine(_provider.Options.BaseUrl, "batches/" + Uri.EscapeDataString(batchId)), null, headers, cancellationToken).ConfigureAwait(false))
        {
            batch = document.RootElement.Clone();
        }

        var status = Str(batch, "status");
        if (status == "validating" || status == "in_progress" || status == "finalizing" || status == "cancelling")
        {
            throw new InvalidArgumentException("batchId", batchId, "OpenAI batch \"" + batchId + "\" is not complete.");
        }

        var fileIds = new List<string>();
        foreach (var name in new[] { "output_file_id", "error_file_id" })
        {
            var fileId = Str(batch, name);
            if (fileId != null)
            {
                fileIds.Add(fileId);
            }
        }

        if (status == "completed" && fileIds.Count == 0)
        {
            throw new AiSdkException("OpenAI batch \"" + batchId + "\" completed without batch output.");
        }

        var items = new List<BatchItem>();
        foreach (var fileId in fileIds)
        {
            var uri = ApiKeys.Combine(_provider.Options.BaseUrl, "files/" + Uri.EscapeDataString(fileId) + "/content");
            var lines = await CollectAsync(_provider.Http.SendJsonLinesAsync(HttpMethod.Get, uri, null, headers, _provider.MaxBatchLineBytes, cancellationToken), cancellationToken).ConfigureAwait(false);
            items.AddRange(lines.Select(Convert));
        }

        return items;
    }

    private static BatchItem Convert(JsonElement line)
    {
        var id = Str(line, "custom_id") ?? string.Empty;
        if (line.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
        {
            var code = Str(error, "code");
            var status = code == "batch_cancelled" ? "cancelled" : code == "batch_expired" ? "expired" : "failed";
            return new BatchItem("text", id, status) { ErrorCode = code, ErrorMessage = Str(error, "message") };
        }

        if (!line.TryGetProperty("response", out var response) || response.ValueKind != JsonValueKind.Object)
        {
            return new BatchItem("text", id, "failed") { ErrorCode = "invalid_batch_result", ErrorMessage = "OpenAI returned a batch result without a response or error." };
        }

        response.TryGetProperty("body", out var body);
        if (!response.TryGetProperty("status_code", out var codeElement) || !codeElement.TryGetDouble(out var statusCode))
        {
            return new BatchItem("text", id, "failed") { ErrorCode = "invalid_batch_result", ErrorMessage = "OpenAI returned a batch result without a response or error." };
        }

        if (statusCode < 200 || statusCode >= 300)
        {
            return ConvertErrorResponse(id, body, (int)statusCode);
        }

        // Upstream converts only the Responses API shape for OpenAI batches.
        if (body.ValueKind != JsonValueKind.Object || !IsValidResponse(body))
        {
            return InvalidResponse(id, "OpenAI returned an invalid Responses batch result.");
        }

        if (body.TryGetProperty("error", out var bodyError) && bodyError.ValueKind == JsonValueKind.Object)
        {
            return new BatchItem("text", id, "failed") { ErrorCode = Str(bodyError, "code"), ErrorType = Str(bodyError, "type"), ErrorMessage = Str(bodyError, "message") };
        }

        body.TryGetProperty("output", out var output);
        var incompleteReason = body.TryGetProperty("incomplete_details", out var details) ? Str(details, "reason") : null;
        if (output.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return InvalidResponse(id, incompleteReason != null ? "OpenAI Responses returned no output (" + incompleteReason + ")." : "OpenAI Responses returned no output.");
        }

        var content = new List<BatchContentPart>();
        var logprobs = new JsonArray();
        var hasFunctionCall = false;
        foreach (var entry in output.EnumerateArray())
        {
            var type = Str(entry, "type");
            switch (type)
            {
                case "reasoning":
                    var summaries = entry.GetProperty("summary").EnumerateArray().Select(summary => Str(summary, "text") ?? string.Empty).ToList();
                    if (summaries.Count == 0)
                    {
                        summaries.Add(string.Empty);
                    }

                    foreach (var summary in summaries)
                    {
                        content.Add(new BatchContentPart("reasoning")
                        {
                            Text = summary,
                            ProviderMetadata = OpenAIJson.ProviderMetadata(new JsonObject
                            {
                                ["itemId"] = Str(entry, "id"),
                                ["reasoningEncryptedContent"] = Str(entry, "encrypted_content"),
                            }),
                        });
                    }

                    break;
                case "message":
                    foreach (var part in entry.GetProperty("content").EnumerateArray())
                    {
                        content.Add(new BatchContentPart("text") { Text = Str(part, "text") ?? string.Empty });
                        if (part.TryGetProperty("logprobs", out var partLogprobs) && partLogprobs.ValueKind != JsonValueKind.Null)
                        {
                            logprobs.Add(Node(partLogprobs));
                        }
                    }

                    break;
                case "function_call":
                    hasFunctionCall = true;
                    var metadata = new JsonObject { ["itemId"] = Str(entry, "id") };
                    if (Str(entry, "namespace") is { } ns)
                    {
                        metadata["namespace"] = ns;
                    }

                    if (entry.TryGetProperty("caller", out var caller) && caller.ValueKind == JsonValueKind.Object)
                    {
                        metadata["caller"] = Str(caller, "type") == "program"
                            ? new JsonObject { ["type"] = "program", ["callerId"] = Str(caller, "caller_id") }
                            : Node(caller);
                    }

                    content.Add(new BatchContentPart("tool-call") { ToolCallId = Str(entry, "call_id"), ToolName = Str(entry, "name"), Text = Str(entry, "arguments"), ProviderMetadata = OpenAIJson.ProviderMetadata(metadata) });
                    break;
                case "custom_tool_call":
                    hasFunctionCall = true;
                    content.Add(new BatchContentPart("tool-call")
                    {
                        ToolCallId = Str(entry, "call_id"),
                        ToolName = Str(entry, "name"),
                        Text = Str(entry, "input") is { } customInput ? JsonSerializer.Serialize(customInput) : "null",
                        ProviderMetadata = OpenAIJson.ProviderMetadata(new JsonObject { ["itemId"] = Str(entry, "id") }),
                    });
                    break;
                case "web_search_call":
                    var searchId = Str(entry, "id");
                    AddProviderToolCall(content, searchId, "web_search", "{}");
                    var searchStatus = Str(entry, "status");
                    var searchResult = new BatchContentPart("tool-result") { ToolCallId = searchId, ToolName = "web_search", Dynamic = true };
                    if (searchStatus is "failed" or "incomplete")
                    {
                        searchResult.IsError = true;
                        searchResult.Output = OpenAIJson.Element(new JsonObject { ["status"] = searchStatus });
                    }
                    else
                    {
                        searchResult.Output = OpenAIJson.Element(MapWebSearchOutput(entry.TryGetProperty("action", out var action) ? action : default));
                    }

                    content.Add(searchResult);
                    break;
                case "file_search_call":
                    var fileSearchId = Str(entry, "id");
                    AddProviderToolCall(content, fileSearchId, "file_search", "{}");
                    var files = entry.TryGetProperty("results", out var fileResults) && fileResults.ValueKind == JsonValueKind.Array ? new JsonArray() : null;
                    if (files != null)
                    {
                        foreach (var result in fileResults.EnumerateArray())
                        {
                            files.Add(new JsonObject
                            {
                                ["attributes"] = result.TryGetProperty("attributes", out var attributes) ? Node(attributes) : null,
                                ["fileId"] = Str(result, "file_id"),
                                ["filename"] = Str(result, "filename"),
                                ["score"] = result.TryGetProperty("score", out var score) ? Node(score) : null,
                                ["text"] = Str(result, "text"),
                            });
                        }
                    }

                    content.Add(new BatchContentPart("tool-result")
                    {
                        ToolCallId = fileSearchId,
                        ToolName = "file_search",
                        Dynamic = true,
                        Output = OpenAIJson.Element(new JsonObject { ["queries"] = entry.TryGetProperty("queries", out var queries) ? Node(queries) : null, ["results"] = files }),
                    });
                    break;
                case "code_interpreter_call":
                    var interpreterId = Str(entry, "id");
                    AddProviderToolCall(content, interpreterId, "code_interpreter", OpenAIJson.Element(new JsonObject { ["code"] = Str(entry, "code"), ["containerId"] = Str(entry, "container_id") }).GetRawText());
                    content.Add(new BatchContentPart("tool-result")
                    {
                        ToolCallId = interpreterId,
                        ToolName = "code_interpreter",
                        Dynamic = true,
                        Output = OpenAIJson.Element(new JsonObject { ["outputs"] = entry.TryGetProperty("outputs", out var outputs) ? Node(outputs) : null }),
                    });
                    break;
                default:
                    return new BatchItem("text", id, "failed") { ErrorCode = "unsupported_content", ErrorMessage = "OpenAI returned an unsupported \"" + type + "\" output item in an AI SDK text batch." };
            }
        }

        var openai = new JsonObject { ["responseId"] = Str(body, "id") };
        if (logprobs.Count > 0)
        {
            openai["logprobs"] = logprobs;
        }

        if (Str(body, "service_tier") is { } serviceTier)
        {
            openai["serviceTier"] = serviceTier;
        }

        if (body.TryGetProperty("reasoning", out var reasoning) && reasoning.ValueKind == JsonValueKind.Object
            && reasoning.TryGetProperty("context", out var reasoningContext) && reasoningContext.ValueKind != JsonValueKind.Null)
        {
            openai["reasoningContext"] = Node(reasoningContext);
        }

        var item = new BatchItem("text", id, "succeeded");
        item.Text = new BatchTextGeneration
        {
            Content = content,
            FinishReason = incompleteReason switch
            {
                null => hasFunctionCall ? "tool-calls" : "stop",
                "max_output_tokens" => "length",
                "content_filter" => "content-filter",
                _ => hasFunctionCall ? "tool-calls" : "other",
            },
            RawFinishReason = incompleteReason,
            Usage = ConvertUsage(body),
            ResponseId = Str(body, "id"),
            Timestamp = body.TryGetProperty("created_at", out var created) && created.ValueKind == JsonValueKind.Number && created.TryGetInt64(out var seconds)
                ? DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime
                : null,
            ModelId = Str(body, "model"),
            ProviderMetadata = OpenAIJson.ProviderMetadata(openai),
        };
        return item;
    }

    private static JsonNode? Node(JsonElement element) => JsonNode.Parse(element.GetRawText());

    private static void AddProviderToolCall(List<BatchContentPart> content, string? id, string name, string input)
    {
        content.Add(new BatchContentPart("tool-call") { ToolCallId = id, ToolName = name, Text = input, ProviderExecuted = true, Dynamic = true });
    }

    private static JsonObject MapWebSearchOutput(JsonElement action)
    {
        if (action.ValueKind != JsonValueKind.Object)
        {
            return new JsonObject();
        }

        switch (Str(action, "type"))
        {
            case "search":
                var search = new JsonObject { ["type"] = "search" };
                if (Str(action, "query") is { } query)
                {
                    search["query"] = query;
                }

                if (action.TryGetProperty("queries", out var queries) && queries.ValueKind != JsonValueKind.Null)
                {
                    search["queries"] = Node(queries);
                }

                var result = new JsonObject { ["action"] = search };
                if (action.TryGetProperty("sources", out var sources) && sources.ValueKind != JsonValueKind.Null)
                {
                    result["sources"] = Node(sources);
                }

                return result;
            case "open_page":
                return new JsonObject { ["action"] = new JsonObject { ["type"] = "openPage", ["url"] = Str(action, "url") } };
            case "find_in_page":
                return new JsonObject { ["action"] = new JsonObject { ["type"] = "findInPage", ["url"] = Str(action, "url"), ["pattern"] = Str(action, "pattern") } };
            default:
                return new JsonObject();
        }
    }

    private static BatchModelUsage? ConvertUsage(JsonElement body)
    {
        if (!body.TryGetProperty("usage", out var usage) || usage.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var input = Int(usage, "input_tokens");
        var output = Int(usage, "output_tokens");
        var cached = usage.TryGetProperty("input_tokens_details", out var inputDetails) ? Int(inputDetails, "cached_tokens") : null;
        var reasoning = usage.TryGetProperty("output_tokens_details", out var outputDetails) ? Int(outputDetails, "reasoning_tokens") : null;
        return new BatchModelUsage(
            input,
            input == null ? (int?)null : input - (cached ?? 0),
            cached ?? (input == null ? (int?)null : 0),
            null,
            output,
            output == null ? (int?)null : output - (reasoning ?? 0),
            reasoning ?? (output == null ? (int?)null : 0),
            usage.Clone());
    }

    private static int? Int(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) ? number : null;
    }

    private static BatchItem ConvertErrorResponse(string id, JsonElement body, int statusCode)
    {
        var item = new BatchItem("text", id, "failed") { ErrorStatusCode = statusCode };
        if (body.ValueKind == JsonValueKind.Object && body.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object && Str(error, "message") is { } message)
        {
            item.ErrorMessage = message;
            item.ErrorType = Str(error, "type");
            item.ErrorCode = error.TryGetProperty("code", out var code) && code.ValueKind is JsonValueKind.String or JsonValueKind.Number ? (code.ValueKind == JsonValueKind.String ? code.GetString() : code.GetRawText()) : null;
        }
        else
        {
            item.ErrorMessage = "OpenAI batch request failed with status code " + statusCode + ".";
        }

        return item;
    }

    /// <summary>Checks the parts of the Responses schema that the conversion reads.</summary>
    private static bool IsValidResponse(JsonElement body)
    {
        if (body.TryGetProperty("id", out var responseId) && responseId.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        if (!body.TryGetProperty("output", out var output) || output.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (output.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var entry in output.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.Object || Str(entry, "type") == null)
            {
                return false;
            }

            var valid = Str(entry, "type") switch
            {
                "message" => entry.TryGetProperty("content", out var parts) && parts.ValueKind == JsonValueKind.Array && parts.EnumerateArray().All(part => Str(part, "text") != null),
                "reasoning" => Str(entry, "id") != null && entry.TryGetProperty("summary", out var summary) && summary.ValueKind == JsonValueKind.Array && summary.EnumerateArray().All(part => Str(part, "text") != null),
                "function_call" => Str(entry, "call_id") != null && Str(entry, "name") != null && Str(entry, "arguments") != null && Str(entry, "id") != null,
                "custom_tool_call" => Str(entry, "call_id") != null && Str(entry, "name") != null && Str(entry, "input") != null && Str(entry, "id") != null,
                "web_search_call" or "file_search_call" => Str(entry, "id") != null,
                "code_interpreter_call" => Str(entry, "id") != null && Str(entry, "container_id") != null,
                _ => true,
            };
            if (!valid)
            {
                return false;
            }
        }

        return true;
    }

    private static BatchItem InvalidResponse(string id, string message)
    {
        return new BatchItem("text", id, "failed") { ErrorCode = "invalid_response", ErrorMessage = message };
    }
}
