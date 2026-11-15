// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ToolApprovalFollowUp;

/// <summary>Port of <c>tool-approval-follow-up.test.ts</c> &gt; <c>%s approved tool continuation</c>, generateText half.</summary>
public sealed class ApprovedToolContinuationTests
{
    private const string Id = "packages/ai/src/generate-text/tool-approval-follow-up.test.ts::%s approved tool continuation::";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(Id + "should isolate in-place refinement during signed approval replay (schema input supplied: %s)", Commit = "3c09e0011c718c33f9287c5487c1f95190aeb772", Coverage = UpstreamCoverage.Partial, Note = "A JsonElement is immutable, so a refineToolInput function cannot change its input in place. It returns a changed input instead, which has the same effect on the comparison with the approved input. The streamText half is not ported: StreamTextAsync does not process approval responses in the history yet.")]
    public async Task Should_isolate_in_place_refinement_during_signed_approval_replay(bool hasInputSchemaInput)
    {
        const string secret = "test-secret";
        const string input = "{\"counter\":{\"value\":2}}";
        var executed = new List<string>();
        var signature = ToolApprovalSignatures.SignToolApproval(secret, "approval", "call", "record", JsonDocument.Parse(input).RootElement);
        var messages = new List<ModelMessage>
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantToolCallPart(new GeneratedToolCall("call", "record", input)),
                new AssistantToolApprovalRequestPart("approval", "call")
                {
                    Signature = signature,
                    InputSchemaInput = hasInputSchemaInput ? JsonDocument.Parse("{\"counter\":{\"value\":1}}").RootElement.Clone() : null,
                },
            }),
            new ToolApprovalResponseModelMessage("approval", true),
        };
        var originalCount = messages.Count;
        var model = new TestLanguageModel
        {
            OnGenerate = _ => Text("OK"),
            StreamParts = new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(null),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), null),
            },
        };
        var tool = Tool.Function("record", null, "{\"type\":\"object\"}", (arguments, _) =>
        {
            executed.Add(arguments.GetRawText());
            return Task.FromResult("recorded");
        });
        var refine = new Dictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>
        {
            ["record"] = (value, _) =>
            {
                var bumped = value.GetProperty("counter").GetProperty("value").GetInt32() + 1;
                using var document = JsonDocument.Parse("{\"counter\":{\"value\":" + bumped + "}}");
                return Task.FromResult(document.RootElement.Clone());
            },
        };

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Tools = new[] { tool },
            ExperimentalToolApprovalSecret = secret,
            RefineToolInput = refine,
            ToolApproval = new ToolApprovalConfiguration { { "record", "user-approval" } },
            Messages = messages,
        });
        var responseMessages = result.ResponseMessages;

        Assert.Equal(hasInputSchemaInput ? 1 : 0, executed.Count);
        if (hasInputSchemaInput)
        {
            Assert.Equal("{\"counter\":{\"value\":2}}", Assert.Single(executed).Replace(" ", string.Empty));
        }

        Assert.Equal(originalCount, messages.Count);
        var assistant = Assert.IsType<AssistantModelMessage>(messages[0]);
        Assert.Equal(input, Assert.IsType<AssistantToolCallPart>(assistant.Parts[0]).ToolCall.ArgumentsJson);
        var results = responseMessages.OfType<ToolModelMessage>().ToList();
        var toolResult = Assert.Single(results);
        if (hasInputSchemaInput)
        {
            Assert.Equal("text", toolResult.OutputType);
            Assert.Equal("recorded", toolResult.OutputJson);
        }
        else
        {
            Assert.Equal("error-text", toolResult.OutputType);
            Assert.Contains("does not match the validated schema output", toolResult.OutputJson);
        }
    }
}
