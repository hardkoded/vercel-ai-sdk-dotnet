// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Examples = global::Vercel.AI.AddToolInputExamplesMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.AddToolInputExamplesMiddleware;

internal static class Support
{
    public const string FileId =
        "packages/ai/src/middleware/add-tool-input-examples-middleware.test.ts::addToolInputExamplesMiddleware > ";

    public static JsonElement LocationSchema =>
        Json("{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\"}}}");

    public static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    public static ToolInputExample Example(string json) => new(Json(json));

    public static LanguageModelTool Tool(
        string name,
        string? description,
        JsonElement schema,
        params ToolInputExample[] examples
    ) => new(name, description, schema) { InputExamples = examples };

    public static Task<LanguageModelCallOptions> Transform(
        Examples middleware,
        IReadOnlyList<LanguageModelTool>? tools
    )
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello, world!") },
            Tools = tools,
        };
        return middleware.TransformParamsAsync(
            options,
            LanguageModelCallType.Generate,
            new TestLanguageModel(),
            CancellationToken.None
        );
    }
}
