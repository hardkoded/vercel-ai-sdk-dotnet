// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Prompt;

public sealed class StandardizePromptTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/prompt/standardize-prompt.test.ts::standardizePrompt::should throw InvalidPromptError when messages contain a system message by default", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_system_messages_by_default()
    {
        var error = Assert.Throws<InvalidPromptException>(() => Prompts.StandardizePrompt(new StandardizePromptInput
        {
            Messages = new[] { new PromptMessage("system", "INSTRUCTIONS") },
        }));
        Assert.Equal("AI_InvalidPromptError", error.Name);
        Assert.Contains("System messages are not allowed", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/standardize-prompt.test.ts::standardizePrompt::should throw InvalidPromptError when prompt messages contain a system message by default", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_system_messages_in_prompt()
    {
        var error = Assert.Throws<InvalidPromptException>(() => Prompts.StandardizePrompt(new StandardizePromptInput
        {
            Prompt = new PromptMessage[] { new("system", "INSTRUCTIONS") },
        }));
        Assert.Equal("AI_InvalidPromptError", error.Name);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/standardize-prompt.test.ts::standardizePrompt::should allow system messages in messages when allowSystemInMessages is true", Coverage = UpstreamCoverage.Covered)]
    public void Allows_system_messages_when_requested()
    {
        var result = Prompts.StandardizePrompt(new StandardizePromptInput
        {
            AllowSystemInMessages = true,
            Messages = new[]
            {
                new PromptMessage("system", "INSTRUCTIONS"),
                new PromptMessage("user", "Hello, world!"),
            },
        });
        Assert.Null(result.Instructions);
        Assert.Equal(2, result.Messages.Count);
        Assert.Equal("system", result.Messages[0].Role);
        Assert.Equal("INSTRUCTIONS", result.Messages[0].Content);
        Assert.Equal("user", result.Messages[1].Role);
        Assert.Equal("Hello, world!", result.Messages[1].Content);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/standardize-prompt.test.ts::standardizePrompt::should allow system messages in prompt messages when allowSystemInMessages is true", Coverage = UpstreamCoverage.Covered)]
    public void Allows_system_messages_inside_prompt()
    {
        var result = Prompts.StandardizePrompt(new StandardizePromptInput
        {
            AllowSystemInMessages = true,
            Prompt = new PromptMessage[]
            {
                new("system", "INSTRUCTIONS"),
                new("user", "Hello, world!"),
            },
        });
        Assert.Null(result.Instructions);
        Assert.Equal("system", result.Messages[0].Role);
        Assert.Equal("INSTRUCTIONS", result.Messages[0].Content);
        Assert.Equal("user", result.Messages[1].Role);
        Assert.Equal("Hello, world!", result.Messages[1].Content);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/standardize-prompt.test.ts::standardizePrompt::should throw InvalidPromptError when an allowed system message has parts", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_system_message_parts()
    {
        var error = Assert.Throws<InvalidPromptException>(() => Prompts.StandardizePrompt(new StandardizePromptInput
        {
            AllowSystemInMessages = true,
            Messages = new[] { new PromptMessage("system", new[] { new Dictionary<string, string> { ["type"] = "text", ["text"] = "test" } }) },
        }));
        Assert.Equal("The messages do not match the ModelMessage[] schema.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/standardize-prompt.test.ts::standardizePrompt::should throw InvalidPromptError when messages array is empty", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_empty_messages()
    {
        var error = Assert.Throws<InvalidPromptException>(() => Prompts.StandardizePrompt(new StandardizePromptInput
        {
            Messages = Array.Empty<PromptMessage>(),
        }));
        Assert.Equal("messages must not be empty", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/standardize-prompt.test.ts::standardizePrompt::should support SystemModelMessage instructions", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_a_system_message_as_instructions()
    {
        var instructions = new PromptMessage("system", "INSTRUCTIONS");
        var result = Prompts.StandardizePrompt(new StandardizePromptInput
        {
            Instructions = instructions,
            InstructionsSet = true,
            Prompt = "Hello, world!",
        });
        var message = Assert.IsType<PromptMessage>(result.Instructions);
        Assert.Equal("system", message.Role);
        Assert.Equal("INSTRUCTIONS", message.Content);
        Assert.Equal("user", result.Messages[0].Role);
        Assert.Equal("Hello, world!", result.Messages[0].Content);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/standardize-prompt.test.ts::standardizePrompt::should support array of SystemModelMessage instructions", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_a_list_of_system_instructions()
    {
        var instructions = new PromptMessage[]
        {
            new("system", "INSTRUCTIONS"),
            new("system", "INSTRUCTIONS 2"),
        };
        var result = Prompts.StandardizePrompt(new StandardizePromptInput
        {
            Instructions = instructions,
            InstructionsSet = true,
            Prompt = "Hello, world!",
        });
        var list = Assert.IsAssignableFrom<IReadOnlyList<PromptMessage>>(result.Instructions);
        Assert.Equal(2, list.Count);
        Assert.Equal("INSTRUCTIONS", list[0].Content);
        Assert.Equal("INSTRUCTIONS 2", list[1].Content);
        Assert.Equal("Hello, world!", result.Messages[0].Content);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/standardize-prompt.test.ts::standardizePrompt::should fall back to system when instructions is not defined", Coverage = UpstreamCoverage.Covered)]
    public void Falls_back_to_system()
    {
        var result = Prompts.StandardizePrompt(new StandardizePromptInput
        {
            System = "SYSTEM",
            Prompt = "Hello, world!",
        });
        Assert.Equal("SYSTEM", result.Instructions);
        Assert.Equal("Hello, world!", result.Messages[0].Content);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/standardize-prompt.test.ts::standardizePrompt::should prefer instructions over system", Coverage = UpstreamCoverage.Covered)]
    public void Prefers_instructions_over_system()
    {
        var result = Prompts.StandardizePrompt(new StandardizePromptInput
        {
            Instructions = "INSTRUCTIONS",
            InstructionsSet = true,
            System = "SYSTEM",
            Prompt = "Hello, world!",
        });
        Assert.Equal("INSTRUCTIONS", result.Instructions);
    }
}

public sealed class PrepareToolChoiceTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/prompt/prepare-tool-choice.test.ts::prepareToolChoice::returns auto when tool choice is not provided", Coverage = UpstreamCoverage.Covered)]
    public void Missing_choice_is_auto()
    {
        var result = ToolChoices.PrepareToolChoice((ToolChoice?)null);
        Assert.Equal("auto", result.Type);
        Assert.Null(result.ToolName);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/prepare-tool-choice.test.ts::prepareToolChoice::handles string tool choice: none", Coverage = UpstreamCoverage.Covered)]
    public void String_none()
    {
        Assert.Equal("none", ToolChoices.PrepareToolChoice("none").Type);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/prepare-tool-choice.test.ts::prepareToolChoice::handles object tool choice", Coverage = UpstreamCoverage.Covered)]
    public void Named_tool_choice()
    {
        var result = ToolChoices.PrepareToolChoice(ToolChoice.Tool("tool2"));
        Assert.Equal("tool", result.Type);
        Assert.Equal("tool2", result.ToolName);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/prepare-tool-choice.test.ts::prepareToolChoice::handles string tool choice: auto", Coverage = UpstreamCoverage.Covered)]
    public void String_auto()
    {
        Assert.Equal("auto", ToolChoices.PrepareToolChoice("auto").Type);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/prepare-tool-choice.test.ts::prepareToolChoice::handles string tool choice: required", Coverage = UpstreamCoverage.Covered)]
    public void String_required()
    {
        Assert.Equal("required", ToolChoices.PrepareToolChoice("required").Type);
    }
}

public sealed class PrepareToolsTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/prompt/prepare-tools.test.ts::prepareTools::returns undefined when tools are not provided", Coverage = UpstreamCoverage.Covered)]
    public void Missing_tools_are_null()
    {
        Assert.Null(PromptTools.PrepareTools(null));
        Assert.Null(PromptTools.PrepareTools(new Dictionary<string, PromptTool>()));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/prepare-tools.test.ts::prepareTools::returns all tools", Coverage = UpstreamCoverage.Covered)]
    public void Returns_function_tools()
    {
        var result = PromptTools.PrepareTools(SampleTools());
        Assert.NotNull(result);
        Assert.Equal(new[] { "tool1", "tool2" }, result.Select(tool => tool.Name).ToArray());
        Assert.Equal("function", result[0].Type);
        Assert.Equal("Tool 1 description", result[0].Description);
        Assert.Equal("Tool 2 description", result[1].Description);
        Assert.Contains("\"city\"", result[1].InputSchema!.Value.GetRawText(), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/prepare-tools.test.ts::prepareTools::handles provider-defined tools", Coverage = UpstreamCoverage.Covered)]
    public void Copies_provider_tools()
    {
        var tools = SampleTools();
        tools["providerTool"] = new PromptTool("provider")
        {
            Id = "provider.tool-id",
            Args = Json("{\"key\":\"value\"}"),
        };
        var result = PromptTools.PrepareTools(tools);
        Assert.NotNull(result);
        Assert.Equal("provider", result[2].Type);
        Assert.Equal("providerTool", result[2].Name);
        Assert.Equal("provider.tool-id", result[2].Id);
        Assert.Equal("value", result[2].Args!.Value.GetProperty("key").GetString());
        Assert.Null(result[2].Description);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/prepare-tools.test.ts::prepareTools::orders tools according to a partial toolOrder and appends omitted tools alphabetically", Coverage = UpstreamCoverage.Covered)]
    public void Partial_order_then_alphabetical()
    {
        var tools = new Dictionary<string, PromptTool>
        {
            ["zebra"] = Named("Zebra tool"),
            ["alpha"] = Named("Alpha tool"),
            ["providerTool"] = new PromptTool("provider") { Id = "provider.tool-id" },
            ["middle"] = Named("Middle tool"),
        };
        var result = PromptTools.PrepareTools(tools, new[] { "middle" });
        Assert.Equal(new[] { "middle", "alpha", "providerTool", "zebra" }, result!.Select(tool => tool.Name).ToArray());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/prepare-tools.test.ts::prepareTools::preserves toolOrder entries before alphabetically sorting the remaining tools", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_tool_order()
    {
        var tools = new Dictionary<string, PromptTool>
        {
            ["zebra"] = Named("Zebra tool"),
            ["alpha"] = Named("Alpha tool"),
            ["middle"] = Named("Middle tool"),
        };
        var result = PromptTools.PrepareTools(tools, new[] { "zebra", "middle" });
        Assert.Equal(new[] { "zebra", "middle", "alpha" }, result!.Select(tool => tool.Name).ToArray());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/prepare-tools.test.ts::prepareTools::does not duplicate tools when toolOrder contains duplicate names", Coverage = UpstreamCoverage.Covered)]
    public void Duplicate_order_names_are_kept_once()
    {
        var result = PromptTools.PrepareTools(SampleTools(), new[] { "tool2", "tool2" });
        Assert.Equal(new[] { "tool2", "tool1" }, result!.Select(tool => tool.Name).ToArray());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/prepare-tools.test.ts::prepareTools::passes through provider options", Coverage = UpstreamCoverage.Covered)]
    public void Passes_provider_options()
    {
        var tools = new Dictionary<string, PromptTool>
        {
            ["tool1"] = new PromptTool("function")
            {
                Description = "Tool 1 description",
                InputSchema = Json("{\"type\":\"object\",\"properties\":{}}"),
                ProviderOptions = Json("{\"aProvider\":{\"aSetting\":\"aValue\"}}"),
            },
        };
        var result = PromptTools.PrepareTools(tools);
        Assert.Equal("aValue", result![0].ProviderOptions!.Value.GetProperty("aProvider").GetProperty("aSetting").GetString());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/prepare-tools.test.ts::prepareTools::passes through strict mode settings", Coverage = UpstreamCoverage.Covered)]
    public void Passes_strict_mode()
    {
        var tools = new Dictionary<string, PromptTool>
        {
            ["tool1"] = new PromptTool("function")
            {
                Description = "Tool 1 description",
                InputSchema = Json("{\"type\":\"object\"}"),
                Strict = true,
            },
        };
        Assert.True(PromptTools.PrepareTools(tools)![0].Strict);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/prepare-tools.test.ts::prepareTools::passes through input examples", Coverage = UpstreamCoverage.Covered)]
    public void Passes_input_examples()
    {
        var tools = new Dictionary<string, PromptTool>
        {
            ["tool1"] = new PromptTool("function")
            {
                Description = "Tool 1 description",
                InputSchema = Json("{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}}}"),
                InputExamples = Json("[{\"input\":{\"city\":\"New York\"}}]"),
            },
        };
        var example = PromptTools.PrepareTools(tools)![0].InputExamples!.Value;
        Assert.Equal("New York", example[0].GetProperty("input").GetProperty("city").GetString());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/prompt/prepare-tools.test.ts::prepareTools::resolves function descriptions from toolsContext and sandbox", Coverage = UpstreamCoverage.Covered)]
    public void Resolves_function_descriptions()
    {
        var tools = new Dictionary<string, PromptTool>
        {
            ["contextual"] = new PromptTool("dynamic")
            {
                DescriptionFactory = context =>
                {
                    var map = (IReadOnlyDictionary<string, object?>)context.Context!;
                    map.TryGetValue("userName", out var name);
                    return "User is " + name;
                },
                InputSchema = Json("{}"),
            },
            ["withSandbox"] = new PromptTool("dynamic")
            {
                DescriptionFactory = context => "Env: " + (context.SandboxDescription ?? "none"),
                InputSchema = Json("{}"),
            },
        };
        var result = PromptTools.PrepareTools(
            tools,
            toolsContext: new Dictionary<string, object?> { ["contextual"] = new Dictionary<string, object?> { ["userName"] = "Ada" } },
            sandboxDescription: "test-sandbox");
        Assert.Equal("contextual", result![0].Name);
        Assert.Equal("User is Ada", result[0].Description);
        Assert.Equal("withSandbox", result[1].Name);
        Assert.Equal("Env: test-sandbox", result[1].Description);
    }

    private static Dictionary<string, PromptTool> SampleTools()
    {
        return new Dictionary<string, PromptTool>
        {
            ["tool1"] = new PromptTool("function")
            {
                Description = "Tool 1 description",
                InputSchema = Json("{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"additionalProperties\":false,\"properties\":{},\"type\":\"object\"}"),
            },
            ["tool2"] = new PromptTool("function")
            {
                Description = "Tool 2 description",
                InputSchema = Json("{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"additionalProperties\":false,\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"],\"type\":\"object\"}"),
            },
        };
    }

    private static PromptTool Named(string description)
    {
        return new PromptTool("function")
        {
            Description = description,
            InputSchema = Json("{\"type\":\"object\"}"),
        };
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
