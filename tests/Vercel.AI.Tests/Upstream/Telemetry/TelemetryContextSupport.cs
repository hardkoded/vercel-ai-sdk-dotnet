// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Tests.Upstream.Telemetry;

/// <summary>The runtime and tools context fixtures that the "in telemetry" upstream tests share.</summary>
internal static class TelemetryContextSupport
{
    private const string WeatherContextSchema =
        "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]}";

    internal static IReadOnlyDictionary<string, object?> RuntimeContext() =>
        new Dictionary<string, object?> { ["userId"] = "user-123", ["requestId"] = "request-123" };

    internal static Tool WeatherTool() =>
        Tool.Function("weather", null, WeatherContextSchema, (_, _) => Task.FromResult("sunny"));

    internal static IReadOnlyDictionary<string, JsonElement> WeatherToolsContext() =>
        new Dictionary<string, JsonElement>
        {
            ["weather"] = JsonDocument
                .Parse("{\"apiKey\":\"secret-api-key\",\"region\":\"eu\"}")
                .RootElement.Clone(),
        };

    /// <summary>Records the runtime context that onStart, onStepStart, onStepEnd and onEnd receive, as JSON.</summary>
    internal static TestTelemetry RuntimeContextIntegration(List<string> contexts) =>
        new()
        {
            OnStart = (e, _) => Record(contexts, ((GenerateTextStartEvent)e).RuntimeContext),
            OnStepStart = (e, _) => Record(contexts, e.RuntimeContext),
            OnStepEnd = (e, _) => Record(contexts, e.RuntimeContext),
            OnEnd = (e, _) => Record(contexts, ((GenerateTextResult)e).RuntimeContext),
        };

    /// <summary>Records the tools context that onStart, onStepStart, onStepEnd and onEnd receive, as JSON.</summary>
    internal static TestTelemetry ToolsContextIntegration(List<string> contexts) =>
        new()
        {
            OnStart = (e, _) => Record(contexts, ((GenerateTextStartEvent)e).ToolsContext),
            OnStepStart = (e, _) => Record(contexts, e.ToolsContext),
            OnStepEnd = (e, _) => Record(contexts, e.ToolsContext),
            OnEnd = (e, _) => Record(contexts, ((GenerateTextResult)e).ToolsContext),
        };

    private static Task Record(List<string> contexts, object? context)
    {
        contexts.Add(JsonSerializer.Serialize(context));
        return Task.CompletedTask;
    }
}
