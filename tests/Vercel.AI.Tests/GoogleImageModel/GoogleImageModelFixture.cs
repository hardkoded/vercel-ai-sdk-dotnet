// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;

namespace Vercel.AI.Tests.GoogleImageModel;

internal static class GoogleImageModelFixture
{
    public const string ImageResponse = "{\"candidates\":[{\"content\":{\"parts\":[{\"inlineData\":{\"mimeType\":\"image/png\",\"data\":\"AQID\"}}],\"role\":\"model\"},\"finishReason\":\"STOP\"}]}";

    public static GoogleOptions Key() => new() { ApiKey = "test-api-key" };

    public static Vercel.AI.Google.GoogleImageModel Model(RecordingHandler handler, GoogleOptions? options = null) =>
        new(GoogleProvider.Create(options ?? Key(), handler), "gemini-2.5-flash-image");

    public static JsonObject RequestBody(RecordingHandler handler) => JsonNode.Parse(handler.Body)!.AsObject();
}
