// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

public sealed class ShellReplayTests
{
    private static readonly ToolNameMapping Mapping = ToolNames.CreateToolNameMapping(
        new[] { new MappedTool("provider", "shell", "openai.shell") },
        new Dictionary<string, string> { ["openai.shell"] = "shell" });

    [Theory]
    [InlineData("[]")]
    [InlineData("1")]
    [InlineData("null")]
    public void A_shell_call_input_that_is_not_an_object_is_rejected(string argumentsJson)
    {
        var message = new AssistantModelMessage(null, new[] { new GeneratedToolCall("call_1", "shell", argumentsJson) }, null);

        Assert.Throws<AiSdkException>(() =>
            OpenAIResponsesLanguageModel.ConvertInput(new ModelMessage[] { message }, "system", false, null, "openai", false, false, toolNameMapping: Mapping));
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("1")]
    [InlineData("null")]
    public void A_shell_result_that_is_not_an_object_is_rejected(string outputJson)
    {
        var result = new ToolModelMessage("call_1", "shell", outputJson, false, "json");

        Assert.Throws<AiSdkException>(() =>
            OpenAIResponsesLanguageModel.ConvertInput(new ModelMessage[] { result }, "system", false, null, "openai", false, true, toolNameMapping: Mapping));
    }
}
