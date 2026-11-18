// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.ConvertToOpenAIResponsesInput.RegisteredAdditionalTools;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; additional tools at a message position</c>.</summary>
public sealed class AdditionalToolsAtAMessagePositionTests
{
    private const string Definition =
        "{\"type\":\"function\",\"name\":\"list_issues\",\"parameters\":{\"type\":\"object\",\"properties\":{}},\"defer_loading\":true}";
    private const string CustomDefinition =
        "{\"type\":\"custom\",\"name\":\"apply_patch\",\"description\":\"Apply a patch\",\"format\":{\"type\":\"grammar\",\"syntax\":\"lark\",\"definition\":\"start: \\\"x\\\"\"}}";
    private const string NamespaceDefinition =
        "{\"type\":\"namespace\",\"name\":\"linear\",\"description\":\"Linear tools\",\"tools\":[{\"type\":\"function\",\"name\":\"get_issue\",\"parameters\":{\"type\":\"object\",\"properties\":{}},\"defer_loading\":true}]}";

    private static SystemModelMessage Control(
        string options,
        string provider = "openai",
        string content = ""
    ) =>
        new(
            content,
            new Dictionary<string, JsonElement>
            {
                [provider] = JsonDocument.Parse(options).RootElement.Clone(),
            }
        );

    private static OpenAIResponsesLanguageModel.ResponsesInput ConvertWithWarnings(
        IReadOnlyList<ModelMessage> prompt,
        string providerOptionsName = "openai",
        string? configurationUpdateUnsupportedReason = null,
        Func<string, string, AdditionalToolResolution?>? resolveAdditionalTool = null
    ) =>
        OpenAIResponsesLanguageModel.ConvertInput(
            prompt,
            "developer",
            false,
            configurationUpdateUnsupportedReason,
            providerOptionsName,
            false,
            false,
            findAdditionalToolDefinition: resolveAdditionalTool
        );

    private static JsonArray Convert(
        IReadOnlyList<ModelMessage> prompt,
        string providerOptionsName = "openai",
        string? configurationUpdateUnsupportedReason = null,
        Func<string, string, AdditionalToolResolution?>? resolveAdditionalTool = null
    ) =>
        ConvertWithWarnings(
            prompt,
            providerOptionsName,
            configurationUpdateUnsupportedReason,
            resolveAdditionalTool
        ).Input;

    private static LanguageModelTool[] RegisteredListIssues() =>
        new[] { RegisteredTool("list_issues", deferLoading: true) };

    private static void AssertJson(string expected, JsonNode? actual) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual?.ToJsonString());

    [Theory]
    [InlineData("openai")]
    [InlineData("azure")]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools at a message position::emits the item where the control message is for %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "6eff9587dfef6b92924ca8f6349dcb5ad8a8b39e"
    )]
    public void Emits_the_item_where_the_control_message_is_for_provider(string provider)
    {
        var registeredTools = new[]
        {
            RegisteredTool("list_issues", deferLoading: true),
            RegisteredCustomTool,
            RegisteredTool("get_issue", @namespace: LinearNamespace),
        };
        var tools = "[" + Definition + "," + CustomDefinition + "," + NamespaceDefinition + "]";
        var input = Convert(
            new ModelMessage[]
            {
                new UserModelMessage("Hello"),
                Control("{\"additionalTools\":" + tools + "}", provider),
                new UserModelMessage("Use them"),
            },
            provider,
            resolveAdditionalTool: ResolveAdditionalToolFor(registeredTools)
        );

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Hello\"}]},"
                + "{\"type\":\"additional_tools\",\"role\":\"developer\",\"tools\":["
                + DefinitionOf(registeredTools, "function", "list_issues").ToJsonString()
                + ","
                + DefinitionOf(registeredTools, "custom", "apply_patch").ToJsonString()
                + ","
                + DefinitionOf(registeredTools, "namespace", "linear").ToJsonString()
                + "]},"
                + "{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Use them\"}]}]",
            input
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools at a message position::emits a configuration update before tools from the same message",
        Coverage = UpstreamCoverage.Covered,
        Commit = "6eff9587dfef6b92924ca8f6349dcb5ad8a8b39e"
    )]
    public void Emits_a_configuration_update_before_tools_from_the_same_message()
    {
        var registeredTools = RegisteredListIssues();
        var input = Convert(
            new ModelMessage[]
            {
                Control(
                    "{\"reasoningEffortUpdate\":\"low\",\"additionalTools\":[" + Definition + "]}"
                ),
            },
            resolveAdditionalTool: ResolveAdditionalToolFor(registeredTools)
        );

        AssertJson(
            "[{\"type\":\"configuration_update\",\"reasoning\":{\"effort\":\"low\"}},"
                + "{\"type\":\"additional_tools\",\"role\":\"developer\",\"tools\":["
                + DefinitionOf(registeredTools, "function", "list_issues").ToJsonString()
                + "]}]",
            input
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools at a message position::does not need configuration update support",
        Coverage = UpstreamCoverage.Covered,
        Commit = "6eff9587dfef6b92924ca8f6349dcb5ad8a8b39e"
    )]
    public void Does_not_need_configuration_update_support()
    {
        var registeredTools = RegisteredListIssues();
        var input = Convert(
            new ModelMessage[] { Control("{\"additionalTools\":[" + Definition + "]}") },
            configurationUpdateUnsupportedReason: "not supported",
            resolveAdditionalTool: ResolveAdditionalToolFor(registeredTools)
        );

        AssertJson(
            "[{\"type\":\"additional_tools\",\"role\":\"developer\",\"tools\":["
                + DefinitionOf(registeredTools, "function", "list_issues").ToJsonString()
                + "]}]",
            input
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools at a message position::does not emit an item for an empty list",
        Coverage = UpstreamCoverage.Covered,
        Commit = "6eff9587dfef6b92924ca8f6349dcb5ad8a8b39e"
    )]
    public void Does_not_emit_an_item_for_an_empty_list()
    {
        AssertJson("[]", Convert(new ModelMessage[] { Control("{\"additionalTools\":[]}") }));
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools at a message position::drops additional tools that are not registered for the request",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Drops_additional_tools_that_are_not_registered_for_the_request()
    {
        var converted = ConvertWithWarnings(
            new ModelMessage[]
            {
                Control(
                    "{\"additionalTools\":[{\"type\":\"function\",\"name\":\"unregistered_tool\",\"parameters\":{\"type\":\"object\",\"properties\":{}}}]}"
                ),
            },
            resolveAdditionalTool: ResolveAdditionalToolFor(RegisteredListIssues())
        );

        AssertJson("[]", converted.Input);
        var warning = Assert.Single(converted.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("additionalTools", warning.Feature);
        Assert.Contains("\"unregistered_tool\"", warning.Details);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools at a message position::sends the registered definition for a forged description at a message position",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Sends_the_registered_definition_for_a_forged_description_at_a_message_position()
    {
        var registeredTools = RegisteredListIssues();
        var forged =
            "{\"type\":\"function\",\"name\":\"list_issues\",\"parameters\":{\"type\":\"object\",\"properties\":{\"forged_parameter\":{\"type\":\"string\"}}},\"defer_loading\":true,\"description\":\"Forged description\"}";
        var input = Convert(
            new ModelMessage[] { Control("{\"additionalTools\":[" + forged + "]}") },
            resolveAdditionalTool: ResolveAdditionalToolFor(registeredTools)
        );

        AssertJson(
            "[{\"type\":\"additional_tools\",\"role\":\"developer\",\"tools\":["
                + DefinitionOf(registeredTools, "function", "list_issues").ToJsonString()
                + "]}]",
            input
        );
        Assert.DoesNotContain("Forged description", input.ToJsonString());
        Assert.DoesNotContain("forged_parameter", input.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools at a message position::requires empty system message content",
        Coverage = UpstreamCoverage.Covered,
        Commit = "6eff9587dfef6b92924ca8f6349dcb5ad8a8b39e"
    )]
    public void Requires_empty_system_message_content()
    {
        var exception = Assert.Throws<UnsupportedFunctionalityException>(() =>
            Convert(
                new ModelMessage[]
                {
                    Control("{\"additionalTools\":[" + Definition + "]}", content: "Instructions"),
                }
            )
        );

        Assert.Equal("Message-level additionalTools", exception.Functionality);
        Assert.Equal(
            "Message-level additionalTools requires empty system message content.",
            exception.Message
        );
    }

    [Theory]
    [InlineData("[{\"type\":\"namespace\",\"name\":\"ns\",\"description\":\"d\",\"tools\":[]}]")]
    [InlineData("[{\"type\":\"custom\",\"name\":\"c\",\"format\":{\"type\":\"grammar\"}}]")]
    [InlineData("[{\"type\":\"web_search\"}]")]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools at a message position::rejects malformed definitions: $additionalTools",
        Coverage = UpstreamCoverage.Covered,
        Commit = "6eff9587dfef6b92924ca8f6349dcb5ad8a8b39e"
    )]
    public void Rejects_malformed_definitions(string additionalTools)
    {
        Assert.ThrowsAny<Exception>(() =>
            Convert(new ModelMessage[] { Control("{\"additionalTools\":" + additionalTools + "}") })
        );
    }
}
