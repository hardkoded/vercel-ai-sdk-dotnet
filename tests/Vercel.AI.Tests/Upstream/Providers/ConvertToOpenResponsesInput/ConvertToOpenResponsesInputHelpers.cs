// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenResponses;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToOpenResponsesInputTests;

internal static class ConvertToOpenResponsesInputHelpers
{
    public static ModelMessage User(params UserContentPart[] parts) => new UserModelMessage(parts);

    public static ModelMessage Assistant(params AssistantContentPart[] parts) => new AssistantModelMessage(parts);

    public static AssistantContentPart Call(string id, string name, string argsJson) => new AssistantToolCallPart(new GeneratedToolCall(id, name, argsJson));

    public static OpenResponsesInputResult Convert(string providerOptionsName, bool strict, params ModelMessage[] prompt) => OpenResponsesInput.Convert(prompt, providerOptionsName, strict);

    public static OpenResponsesInputResult Convert(params ModelMessage[] prompt) => OpenResponsesInput.Convert(prompt);

    public static void AssertInput(string expectedJson, OpenResponsesInputResult result)
    {
        var expected = JsonNode.Parse(expectedJson);
        Assert.True(JsonNode.DeepEquals(expected, result.Input), "Expected " + expected!.ToJsonString() + " but was " + result.Input.ToJsonString());
    }
}
