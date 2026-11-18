// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Ui;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ToolApprovalFollowUp;

/// <summary>Port of <c>tool-approval-follow-up.test.ts</c> &gt; <c>%s approved tool continuation</c>, generateText half.</summary>
public sealed class ApprovedToolContinuationTests
{
    private const string Id =
        "packages/ai/src/generate-text/tool-approval-follow-up.test.ts::%s approved tool continuation::";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(
        Id
            + "should isolate in-place refinement during signed approval replay (schema input supplied: %s)",
        Commit = "3c09e0011c718c33f9287c5487c1f95190aeb772",
        Coverage = UpstreamCoverage.Partial,
        Note = "A JsonElement is immutable, so a refineToolInput function cannot change its input in place. It returns a changed input instead, which has the same effect on the comparison with the approved input. The streamText half is not ported: StreamTextAsync does not process approval responses in the history yet."
    )]
    public async Task Should_isolate_in_place_refinement_during_signed_approval_replay(
        bool hasInputSchemaInput
    )
    {
        const string secret = "test-secret";
        const string input = "{\"counter\":{\"value\":2}}";
        var executed = new List<string>();
        var signature = ToolApprovalSignatures.SignToolApproval(
            secret,
            "approval",
            "call",
            "record",
            JsonDocument.Parse(input).RootElement
        );
        var messages = new List<ModelMessage>
        {
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantToolCallPart(new GeneratedToolCall("call", "record", input)),
                    new AssistantToolApprovalRequestPart("approval", "call")
                    {
                        Signature = signature,
                        InputSchemaInput = hasInputSchemaInput
                            ? JsonDocument.Parse("{\"counter\":{\"value\":1}}").RootElement.Clone()
                            : null,
                    },
                }
            ),
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

        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = model,
                    Tools = new[] { tool },
                    ExperimentalToolApprovalSecret = secret,
                    RefineToolInput = refine,
                    ToolApproval = new ToolApprovalConfiguration { { "record", "user-approval" } },
                    Messages = messages,
                }
            );
        var responseMessages = result.ResponseMessages;

        Assert.Equal(hasInputSchemaInput ? 1 : 0, executed.Count);
        if (hasInputSchemaInput)
        {
            Assert.Equal(
                "{\"counter\":{\"value\":2}}",
                Assert.Single(executed).Replace(" ", string.Empty)
            );
        }

        Assert.Equal(originalCount, messages.Count);
        var assistant = Assert.IsType<AssistantModelMessage>(messages[0]);
        Assert.Equal(
            input,
            Assert.IsType<AssistantToolCallPart>(assistant.Parts[0]).ToolCall.ArgumentsJson
        );
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

    [Theory]
    [InlineData("toolMetadata", false)]
    [InlineData("toolMetadata", true)]
    [InlineData("dynamic", false)]
    [InlineData("dynamic", true)]
    [InlineData("title", false)]
    [InlineData("title", true)]
    [UpstreamTest(
        Id + "should enforce the current $field policy on continuation (secret: $secret)",
        Commit = "3c09e0011c718c33f9287c5487c1f95190aeb772",
        Coverage = UpstreamCoverage.Partial,
        Note = "The streamText half is not ported: StreamTextAsync does not process approval responses in the history yet (#847)."
    )]
    public async Task Should_enforce_the_current_field_policy_on_continuation(
        string field,
        bool withSecret
    )
    {
        string? secret = withSecret ? "test-secret" : null;
        var executed = 0;
        var tool = Tool.Function("readTheme", null, "{\"type\":\"object\"}", (_, _) =>
        {
            executed++;
            return Task.FromResult("dark");
        });
        tool.Title = "Read theme";
        tool.Metadata = new Dictionary<string, JsonElement>
        {
            ["restricted"] = JsonDocument.Parse("true").RootElement.Clone(),
        };
        tool.IsDynamic = field == "dynamic";
        var emptyInput = JsonDocument.Parse("{}").RootElement.Clone();
        var signature = secret is null
            ? null
            : ToolApprovalSignatures.SignToolApproval(
                secret,
                "approval",
                "call",
                "readTheme",
                emptyInput
            );
        var messages = UIMessageConversion.ConvertToModelMessages(
            new[]
            {
                new UIMessage(
                    UIMessageRole.Assistant,
                    new UIMessagePart[]
                    {
                        new ToolUIPart(
                            "readTheme",
                            "call",
                            ToolUIPartState.ApprovalResponded,
                            tool.IsDynamic
                        )
                        {
                            Input = emptyInput,
                            Approval = new ToolUIApproval("approval")
                            {
                                Approved = true,
                                Signature = signature,
                            },
                        },
                    }
                ),
            },
            new ConvertToModelMessagesOptions
            {
                Tools = new Dictionary<string, Tool> { ["readTheme"] = tool },
            }
        );
        var originalCount = messages.Count;
        var model = new TestLanguageModel { OnGenerate = _ => Text("OK") };
        var approvalCalls = 0;

        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = model,
                    Tools = new[] { tool },
                    Messages = messages,
                    ExperimentalToolApprovalSecret = secret,
                    ToolApproval = new ToolApprovalConfiguration(options =>
                    {
                        approvalCalls++;
                        var call = options.ToolCall;
                        var denied = field switch
                        {
                            "toolMetadata" => call.ToolMetadata?.ContainsKey("restricted") == true,
                            "dynamic" => call.Dynamic,
                            _ => call.Title == "Read theme",
                        };
                        return Task.FromResult<ToolApprovalStatus?>(
                            denied
                                ? new ToolApprovalStatus(
                                    ToolApprovalStatus.DeniedType,
                                    "Current policy denies this tool"
                                )
                                : ToolApprovalStatus.UserApproval
                        );
                    }),
                }
            );

        Assert.Equal(0, executed);
        Assert.Equal(1, approvalCalls);
        Assert.Equal(originalCount, messages.Count);
        var toolResult = Assert.Single(result.ResponseMessages.OfType<ToolModelMessage>());
        Assert.Equal("call", toolResult.ToolCallId);
        Assert.Equal("execution-denied", toolResult.OutputType);
        Assert.Contains("Current policy denies this tool", toolResult.OutputJson ?? string.Empty);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [UpstreamTest(
        Id + "should process only the first of repeated approval responses: %j",
        Commit = "3c09e0011c718c33f9287c5487c1f95190aeb772",
        Coverage = UpstreamCoverage.Partial,
        Note = "The streamText half is not ported: StreamTextAsync does not process approval responses in the history yet (#847)."
    )]
    public async Task Should_process_only_the_first_of_repeated_approval_responses(
        bool approved,
        bool replayApproved
    )
    {
        var executed = 0;
        var tool = Tool.Function("set_theme", null, "{\"type\":\"object\"}", (_, _) =>
        {
            executed++;
            return Task.FromResult("saved");
        });
        var messages = new List<ModelMessage>
        {
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantToolCallPart(new GeneratedToolCall("call", "set_theme", "{}")),
                    new AssistantToolApprovalRequestPart("approval", "call"),
                }
            ),
            new ToolApprovalResponseModelMessage("approval", approved, "test-reason"),
            new ToolApprovalResponseModelMessage("approval", replayApproved, "test-reason"),
        };
        var originalCount = messages.Count;
        var model = new TestLanguageModel { OnGenerate = _ => Text("OK") };

        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = model,
                    Messages = messages,
                    Tools = new[] { tool },
                    ToolApproval = new ToolApprovalConfiguration
                    {
                        { "set_theme", "user-approval" },
                    },
                }
            );

        Assert.Equal(approved ? 1 : 0, executed);
        Assert.Equal(originalCount, messages.Count);
        var toolResult = Assert.Single(result.ResponseMessages.OfType<ToolModelMessage>());
        if (approved)
        {
            Assert.Equal("text", toolResult.OutputType);
            Assert.Equal("saved", toolResult.OutputJson);
        }
        else
        {
            Assert.Equal("execution-denied", toolResult.OutputType);
            Assert.Contains("test-reason", toolResult.OutputJson ?? string.Empty);
        }

        var call = Assert.Single(model.Calls);
        Assert.Single(call.Prompt.OfType<ToolModelMessage>());
    }

    [Theory]
    [InlineData("unresolved-follow-up")]
    [InlineData("immediate")]
    [InlineData("completed-follow-up")]
    [UpstreamTest(
        Id + "should validate %s before calling the provider",
        Commit = "3c09e0011c718c33f9287c5487c1f95190aeb772",
        Coverage = UpstreamCoverage.Partial,
        Note = "The streamText half is not ported: StreamTextAsync does not process approval responses in the history yet (#847)."
    )]
    public async Task Should_validate_sequence_before_calling_the_provider(string sequence)
    {
        var executed = 0;
        var tool = Tool.Function("set_theme", null, "{\"type\":\"object\"}", (_, _) =>
        {
            executed++;
            return Task.FromResult("saved");
        });
        var model = new TestLanguageModel { OnGenerate = _ => Text("OK") };
        var part = new ToolUIPart(
            "set_theme",
            "call",
            sequence == "completed-follow-up"
                ? ToolUIPartState.OutputAvailable
                : ToolUIPartState.ApprovalResponded
        )
        {
            Input = JsonDocument.Parse("{}").RootElement.Clone(),
            Approval = new ToolUIApproval("approval") { Approved = true },
            Output =
                sequence == "completed-follow-up"
                    ? JsonDocument.Parse("\"saved\"").RootElement.Clone()
                    : null,
        };
        var uiMessages = new List<UIMessage>
        {
            new(UIMessageRole.Assistant, new UIMessagePart[] { part }) { Id = "assistant" },
        };
        if (sequence != "immediate")
        {
            uiMessages.Add(
                new UIMessage(
                    UIMessageRole.User,
                    new UIMessagePart[] { new TextUIPart("Reply with OK.") }
                )
                {
                    Id = "user",
                }
            );
        }

        var options = new GenerateTextOptions
        {
            Model = model,
            Tools = new[] { tool },
            ToolApproval = new ToolApprovalConfiguration { { "set_theme", "user-approval" } },
            Messages = UIMessageConversion.ConvertToModelMessages(
                uiMessages,
                new ConvertToModelMessagesOptions
                {
                    Tools = new Dictionary<string, Tool> { ["set_theme"] = tool },
                    IgnoreIncompleteToolCalls = true,
                }
            ),
        };
        var shouldReject = sequence == "unresolved-follow-up";

        if (shouldReject)
        {
            var error = await Assert.ThrowsAsync<MissingToolResultsError>(() =>
                Client().GenerateTextAsync(options)
            );
            Assert.Equal("AI_MissingToolResultsError", error.ErrorName);
            Assert.Equal(new[] { "call" }, error.ToolCallIds);
        }
        else
        {
            await Client().GenerateTextAsync(options);
        }

        Assert.Equal(sequence == "immediate" ? 1 : 0, executed);
        Assert.Equal(shouldReject ? 0 : 1, model.Calls.Count);
        if (!shouldReject)
        {
            var prompt = model.Calls[0].Prompt;
            // The prompt is [assistant, tool, ...]. Upstream: calls[0].prompt[1] is the tool message with the result.
            var toolMessage = Assert.IsType<ToolModelMessage>(prompt[1]);
            Assert.Equal("call", toolMessage.ToolCallId);
        }
    }
}
