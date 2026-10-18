// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

public sealed class ProgramReplayTests
{
    [Fact]
    public void Program_results_are_not_replayed_when_a_conversation_holds_them()
    {
        var mapping = ToolNames.CreateToolNameMapping(
            new[] { new MappedTool("provider", "program", "openai.programmatic_tool_calling") },
            new Dictionary<string, string> { ["openai.programmatic_tool_calling"] = "programmatic_tool_calling" });
        var output = new ToolModelMessage("call_1", "program", """{"result":"r","status":"completed"}""", false, "json");
        var options = new Dictionary<string, JsonElement> { ["openai"] = JsonDocument.Parse("""{"itemId":"cmo_1"}""").RootElement };
        var message = new AssistantModelMessage(new AssistantContentPart[] { new AssistantToolResultPart("call_1", "program", output, options) });

        var result = OpenAIResponsesLanguageModel.ConvertInput(new ModelMessage[] { message }, "system", false, null, "openai", false, true, hasConversation: true, toolNameMapping: mapping);

        Assert.Empty(result.Input);
    }
}
