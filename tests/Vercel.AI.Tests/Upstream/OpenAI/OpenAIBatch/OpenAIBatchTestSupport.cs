// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIBatch;

internal static class OpenAIBatchTestSupport
{
    public const string BatchUrl = "https://api.openai.com/v1/batches/batch_123";
    public const string OutputUrl = "https://api.openai.com/v1/files/file-output/content";
    public const string ErrorsUrl = "https://api.openai.com/v1/files/file-errors/content";

    public static string BatchResponse(string? outputFileId = null, string? errorFileId = null)
    {
        return new JsonObject
        {
            ["id"] = "batch_123",
            ["status"] = "completed",
            ["output_file_id"] = outputFileId,
            ["error_file_id"] = errorFileId,
            ["created_at"] = 1_700_000_000,
            ["expires_at"] = 1_700_086_400,
            ["request_counts"] = new JsonObject { ["total"] = 2, ["completed"] = 2, ["failed"] = 0 },
            ["errors"] = null,
        }.ToJsonString();
    }

    public static JsonObject ResponsesResultBody(string text)
    {
        return new JsonObject
        {
            ["id"] = "resp_123",
            ["created_at"] = 1_700_000_000,
            ["model"] = "gpt-5.6",
            ["output"] = new JsonArray(MessageOutput(text)),
            ["service_tier"] = "default",
            ["reasoning"] = null,
            ["incomplete_details"] = null,
            ["usage"] = new JsonObject
            {
                ["input_tokens"] = 10,
                ["input_tokens_details"] = new JsonObject { ["cached_tokens"] = 2 },
                ["output_tokens"] = 3,
                ["output_tokens_details"] = new JsonObject { ["reasoning_tokens"] = 1 },
            },
        };
    }

    public static JsonObject MessageOutput(string text)
    {
        return new JsonObject
        {
            ["type"] = "message",
            ["role"] = "assistant",
            ["id"] = "msg_123",
            ["phase"] = null,
            ["content"] = new JsonArray(new JsonObject
            {
                ["type"] = "output_text",
                ["text"] = text,
                ["logprobs"] = null,
                ["annotations"] = new JsonArray(),
            }),
        };
    }

    public static JsonObject WithOutput(JsonObject body, params JsonNode[] output)
    {
        body["output"] = new JsonArray(output);
        return body;
    }

    public static string ResultLine(string id, JsonNode body)
    {
        return new JsonObject
        {
            ["custom_id"] = id,
            ["response"] = new JsonObject { ["status_code"] = 200, ["request_id"] = "openai-" + id, ["body"] = body },
            ["error"] = null,
        }.ToJsonString();
    }

    public static async Task<IReadOnlyList<BatchItem>> GetResultsAsync(RoutedHandler handler)
    {
        var batch = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-api-key" }, handler).ExperimentalBatch()!;
        return await batch.DoGetResultsAsync(new BatchOperationCall("batch_123", null, new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None);
    }
}
