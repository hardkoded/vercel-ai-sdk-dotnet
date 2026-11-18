// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using PrepareTools = Vercel.AI.OpenAI.OpenAIResponsesPrepareTools;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

/// <summary>Port of <c>openai-responses-prepare-tools.test.ts</c> &gt; <c>findAdditionalToolDefinition</c>.</summary>
public sealed class FindAdditionalToolDefinitionTests
{
    private const string Id =
        "packages/openai/src/responses/openai-responses-prepare-tools.test.ts::findAdditionalToolDefinition::";

    private const string EmptySchema = """{"type":"object","properties":{}}""";

    private static readonly LanguageModelTool ListIssuesTool = Function(
        "list_issues",
        "List Linear issues.",
        """{"type":"object","properties":{"team":{"type":"string"}},"required":["team"],"additionalProperties":false}""",
        """{"openai":{"omitFromRequestTools":true,"deferLoading":true}}"""
    );

    private static LanguageModelTool Function(
        string name,
        string? description,
        string schema,
        string? providerOptions
    ) =>
        new(name, description, JsonDocument.Parse(schema).RootElement.Clone())
        {
            ProviderOptions =
                providerOptions == null
                    ? null
                    : JsonDocument.Parse(providerOptions).RootElement.Clone(),
        };

    private static LanguageModelTool Namespaced(
        string name,
        string? description,
        string providerOptions
    ) => Function(name, description, EmptySchema, providerOptions);

    private static AdditionalToolResolution? Find(
        IReadOnlyList<LanguageModelTool> tools,
        string type,
        string name,
        string providerOptionsName = "openai",
        ISet<string>? allowedToolNames = null
    ) =>
        PrepareTools.FindAdditionalToolDefinition(
            tools,
            (type, name),
            providerOptionsName,
            allowedToolNames
        );

    private static void AssertJson(string expected, JsonNode? actual) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual?.ToJsonString());

    private static void AssertWarning(
        CallWarning warning,
        string type,
        string feature,
        string details
    )
    {
        Assert.Equal(type, warning.Type);
        Assert.Equal(feature, warning.Feature);
        Assert.Equal(details, warning.Message);
    }

    [Fact]
    [UpstreamTest(
        Id + "derives the function definition from the registered tool",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Derives_the_function_definition_from_the_registered_tool()
    {
        var resolved = Find(new[] { ListIssuesTool }, "function", "list_issues");

        Assert.NotNull(resolved);
        AssertJson(
            """
            {"type":"function","name":"list_issues","description":"List Linear issues.","parameters":{"type":"object","properties":{"team":{"type":"string"}},"required":["team"],"additionalProperties":false},"strict":false,"defer_loading":true}
            """,
            resolved.Definition
        );
        Assert.Empty(resolved.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Id + "reads provider options of another provider name",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Reads_provider_options_of_another_provider_name()
    {
        var tool = Function(
            "list_issues",
            "List Linear issues.",
            """{"type":"object","properties":{"team":{"type":"string"}},"required":["team"],"additionalProperties":false}""",
            """{"azure":{"omitFromRequestTools":true,"deferLoading":true}}"""
        );

        var resolved = Find(new[] { tool }, "function", "list_issues", "azure");

        var definition = resolved!.Definition!;
        Assert.Equal("list_issues", definition["name"]!.GetValue<string>());
        Assert.Equal("List Linear issues.", definition["description"]!.GetValue<string>());
        Assert.True(definition["defer_loading"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest(
        Id + "derives a namespace definition from the registered namespace tools",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Derives_a_namespace_definition_from_the_registered_namespace_tools()
    {
        const string ns =
            """{"openai":{"namespace":{"name":"linear","description":"Linear tools"}}}""";
        var tools = new[]
        {
            Namespaced("get_issue", null, ns),
            Namespaced("list_issues", "List issues.", ns),
        };

        var resolved = Find(tools, "namespace", "linear");

        AssertJson(
            """
            {"type":"namespace","name":"linear","description":"Linear tools","tools":[
              {"type":"function","name":"get_issue","parameters":{"type":"object","properties":{}},"strict":false},
              {"type":"function","name":"list_issues","description":"List issues.","parameters":{"type":"object","properties":{}},"strict":false}
            ]}
            """,
            resolved!.Definition
        );
        Assert.Empty(resolved.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Id + "does not resolve a namespaced function as a function",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Does_not_resolve_a_namespaced_function_as_a_function()
    {
        var tools = new[]
        {
            Namespaced(
                "get_issue",
                null,
                """{"openai":{"namespace":{"name":"linear","description":"Linear tools"}}}"""
            ),
        };

        Assert.Null(Find(tools, "function", "get_issue"));
    }

    [Fact]
    [UpstreamTest(
        Id + "derives a custom definition from the registered custom tool",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Derives_a_custom_definition_from_the_registered_custom_tool()
    {
        var tool = LanguageModelTool.ProviderDefined(
            "openai.custom",
            "apply_patch",
            JsonDocument
                .Parse("""{"description":"Apply a patch","format":{"type":"text"}}""")
                .RootElement.Clone()
        );

        var resolved = Find(new[] { tool }, "custom", "apply_patch");

        AssertJson(
            """{"type":"custom","name":"apply_patch","description":"Apply a patch","format":{"type":"text"}}""",
            resolved!.Definition
        );
        Assert.Empty(resolved.Warnings);
    }

    [Theory]
    [InlineData("function", "unregistered")]
    [InlineData("custom", "apply_patch")]
    [InlineData("namespace", "unregistered")]
    [UpstreamTest(
        Id + "does not resolve an unregistered entry: $type $name",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Does_not_resolve_an_unregistered_entry(string type, string name)
    {
        Assert.Null(Find(new[] { ListIssuesTool }, type, name));
    }

    private const string PropertyNamesSchema =
        """{"type":"object","properties":{"values":{"type":"object","propertyNames":{"type":"string","format":"uuid"}}}}""";

    [Fact]
    [UpstreamTest(
        Id + "returns schema warnings of the resolved tool",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Returns_schema_warnings_of_the_resolved_tool()
    {
        var tool = Function(
            "list_issues",
            "List Linear issues.",
            PropertyNamesSchema,
            """{"openai":{"omitFromRequestTools":true,"deferLoading":true}}"""
        );

        var resolved = Find(new[] { tool }, "function", "list_issues");

        var warning = Assert.Single(resolved!.Warnings);
        AssertWarning(
            warning,
            "compatibility",
            "JSON Schema propertyNames",
            "OpenAI does not support JSON Schema propertyNames. It was removed before sending the schema, so OpenAI will not enforce property-name constraints."
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "does not repeat schema warnings of tools in the request tools",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Does_not_repeat_schema_warnings_of_tools_in_the_request_tools()
    {
        var tool = Function("list_issues", "List Linear issues.", PropertyNamesSchema, null);

        var resolved = Find(new[] { tool }, "function", "list_issues");

        Assert.Empty(resolved!.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Id + "does not load a tool that restricts allowed callers",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Does_not_load_a_tool_that_restricts_allowed_callers()
    {
        var tool = Function(
            "list_issues",
            "List Linear issues.",
            EmptySchema,
            """{"openai":{"omitFromRequestTools":true,"allowedCallers":["programmatic"]}}"""
        );

        var resolved = Find(new[] { tool }, "function", "list_issues");

        Assert.Null(resolved!.Definition);
        AssertWarning(
            Assert.Single(resolved.Warnings),
            "unsupported",
            "additionalTools entry \"list_issues\"",
            "the registered tool restricts allowed callers, which a definition loaded from history cannot express; the entry is not loaded"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "leaves restricted tools out of a loaded namespace",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Leaves_restricted_tools_out_of_a_loaded_namespace()
    {
        const string ns = "\"namespace\":{\"name\":\"linear\",\"description\":\"Linear tools\"}";
        var tools = new[]
        {
            Namespaced("get_issue", null, "{\"openai\":{" + ns + "}}"),
            Namespaced(
                "delete_issue",
                null,
                "{\"openai\":{" + ns + ",\"allowedCallers\":[\"programmatic\"]}}"
            ),
        };

        var resolved = Find(tools, "namespace", "linear");

        var definition = resolved!.Definition!;
        Assert.Equal("namespace", definition["type"]!.GetValue<string>());
        var kept = Assert.Single(definition["tools"]!.AsArray())!;
        Assert.Equal("get_issue", kept["name"]!.GetValue<string>());
        AssertWarning(
            Assert.Single(resolved.Warnings),
            "unsupported",
            "additionalTools namespace \"linear\"",
            "tool \"delete_issue\" restricts allowed callers, which a definition loaded from history cannot express; it is not loaded"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "warns about conflicting namespace descriptions",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Warns_about_conflicting_namespace_descriptions()
    {
        var tools = new[]
        {
            Namespaced(
                "get_issue",
                null,
                """{"openai":{"namespace":{"name":"linear","description":"First"}}}"""
            ),
            Namespaced(
                "list_issues",
                null,
                """{"openai":{"namespace":{"name":"linear","description":"Second"}}}"""
            ),
        };

        var resolved = Find(tools, "namespace", "linear");

        Assert.Equal("First", resolved!.Definition!["description"]!.GetValue<string>());
        AssertWarning(
            Assert.Single(resolved.Warnings),
            "unsupported",
            "conflicting descriptions for OpenAI tool namespace \"linear\"",
            "the first registered description is used for the loaded namespace"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "does not load a tool outside allowedTools",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Does_not_load_a_tool_outside_allowedTools()
    {
        var resolved = Find(
            new[] { ListIssuesTool },
            "function",
            "list_issues",
            allowedToolNames: new HashSet<string> { "search" }
        );

        Assert.Null(resolved!.Definition);
        AssertWarning(
            Assert.Single(resolved.Warnings),
            "unsupported",
            "additionalTools entry \"list_issues\"",
            "the tool is not part of allowedTools for this request; the entry is not loaded"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "does not load a namespace while allowedTools restricts callable tools",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Does_not_load_a_namespace_while_allowedTools_restricts_callable_tools()
    {
        var tools = new[]
        {
            Namespaced(
                "get_issue",
                null,
                """{"openai":{"namespace":{"name":"linear","description":"Linear tools"}}}"""
            ),
        };

        var resolved = Find(
            tools,
            "namespace",
            "linear",
            allowedToolNames: new HashSet<string> { "search" }
        );

        Assert.Null(resolved!.Definition);
        AssertWarning(
            Assert.Single(resolved.Warnings),
            "unsupported",
            "additionalTools namespace \"linear\"",
            "tool namespaces cannot be loaded while allowedTools restricts the callable tools; the entry is not loaded"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "explains a namespace whose members all restrict allowed callers",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Explains_a_namespace_whose_members_all_restrict_allowed_callers()
    {
        var tools = new[]
        {
            Namespaced(
                "get_issue",
                null,
                """{"openai":{"namespace":{"name":"linear","description":"Linear tools"},"allowedCallers":["programmatic"]}}"""
            ),
        };

        var resolved = Find(tools, "namespace", "linear");

        Assert.Null(resolved!.Definition);
        AssertWarning(
            Assert.Single(resolved.Warnings),
            "unsupported",
            "additionalTools namespace \"linear\"",
            "tool \"get_issue\" restricts allowed callers, which a definition loaded from history cannot express; it is not loaded"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "does not prepare tools the entry does not reference",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public void Does_not_prepare_tools_the_entry_does_not_reference()
    {
        var tools = new[]
        {
            Function(
                "unsupported_schema",
                null,
                """{"type":"object","properties":{"values":{"type":"object","propertyNames":{"type":"number"}}}}""",
                null
            ),
            ListIssuesTool,
        };

        var resolved = Find(tools, "function", "list_issues");

        Assert.Equal("list_issues", resolved!.Definition!["name"]!.GetValue<string>());
    }
}
