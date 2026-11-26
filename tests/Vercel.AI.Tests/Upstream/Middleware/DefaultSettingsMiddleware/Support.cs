// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Settings = global::Vercel.AI.DefaultSettingsMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.DefaultSettingsMiddleware;

internal static class Support
{
    public const string FileId =
        "packages/ai/src/middleware/default-settings-middleware.test.ts::defaultSettingsMiddleware > ";

    public static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    public static LanguageModelCallOptions Params() =>
        new() { Prompt = new ModelMessage[] { new UserModelMessage("Hello, world!") } };

    public static Task<LanguageModelCallOptions> Transform(
        LanguageModelCallOptions settings,
        LanguageModelCallOptions? parameters = null
    ) =>
        new Settings(settings).TransformParamsAsync(
            parameters ?? Params(),
            LanguageModelCallType.Generate,
            new TestLanguageModel(),
            CancellationToken.None
        );

    public static void AssertJsonEqual(string expected, JsonElement actual) =>
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), JsonNode.Parse(actual.GetRawText())),
            $"Expected {expected} but was {actual.GetRawText()}"
        );
}
