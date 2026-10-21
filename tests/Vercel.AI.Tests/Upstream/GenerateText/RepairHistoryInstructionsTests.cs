// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.ResultToolCallsTests;

namespace Vercel.AI.Tests.Upstream.GenerateText;

// Regression test with no upstream `it`: a PrepareStep that replaces the messages must not make the repair history lose a real message.
public sealed class RepairHistoryInstructionsTests
{
    private const string ValueSchema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}";

    [Fact]
    public async Task Repair_history_keeps_a_system_message_that_is_not_an_instruction()
    {
        var a = new SystemModelMessage("A");
        var b = new SystemModelMessage("B");
        var c = new SystemModelMessage("C");
        ToolCallRepairContext? seen = null;

        await new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel { OnGenerate = _ => ToolCallResponse("{ \"value\": broken") },
            Tools = new[] { Tool.Function("tool1", null, ValueSchema, (_, _) => Task.FromResult("\"ok\"")) },
            Instructions = new[] { a, b },
            Prompt = "test-input",
            PrepareStep = _ => new PrepareStepUpdate { Messages = new ModelMessage[] { a, c, new UserModelMessage("test-input") } },
            RepairToolCall = context =>
            {
                seen = context;
                return Task.FromResult<GeneratedToolCall?>(new GeneratedToolCall("call-1", "tool1", "{ \"value\": \"repaired\" }"));
            },
        });

        Assert.Equal(2, seen!.Instructions!.Messages!.Count);
        Assert.Equal(2, seen.Messages.Count);
        Assert.Same(c, seen.Messages[0]);
        Assert.IsType<UserModelMessage>(seen.Messages[1]);
    }
}
