// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;

namespace Vercel.AI.Tests.Upstream.CreateAgentUIStream;

/// <summary>Shared helpers of <c>create-agent-ui-stream.test.ts</c>. Maps to <c>createMockModel</c> and <c>currentTool</c>.</summary>
internal static class CreateAgentUIStreamSupport
{
    public static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    public static TestLanguageModel CreateMockModel() =>
        new()
        {
            StreamParts =
            [
                new StreamStartStreamPart([]),
                new ResponseMetadataStreamPart("id-0", "mock-model-id", DateTimeOffset.UnixEpoch),
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "response"),
                new TextEndStreamPart("1"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(1, 1, 2), "stop"),
            ],
        };

    public static Tool CurrentTool() =>
        new(
            "current",
            null,
            Json(
                """{"type":"object","properties":{"current":{"type":"string"}},"required":["current"]}"""
            ),
            null
        )
        {
            OutputSchema = Json(
                """{"type":"object","properties":{"result":{"type":"string"}},"required":["result"]}"""
            ),
        };

    public static AiClient Client() => SteeringTestSupport.Client();
}
