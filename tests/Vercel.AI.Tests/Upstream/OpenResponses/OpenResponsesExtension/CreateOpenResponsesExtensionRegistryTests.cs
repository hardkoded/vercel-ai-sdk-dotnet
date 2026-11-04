// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenResponses;
using Extension = Vercel.AI.OpenResponses.OpenResponsesExtension;

namespace Vercel.AI.Tests.Upstream.OpenResponses.OpenResponsesExtension;

/// <summary>Port of <c>open-responses-extension.test.ts</c> &gt; <c>createOpenResponsesExtensionRegistry</c>.</summary>
public sealed class CreateOpenResponsesExtensionRegistryTests
{
    private const string Prefix = "packages/open-responses/src/open-responses-extension.test.ts::createOpenResponsesExtensionRegistry::";

    private static Extension CreateExtension()
    {
        return new Extension
        {
            Id = "acme.search",
            ToolType = "acme:search",
            ItemTypes = new[] { "acme:search_call" },
            EncodeTool = _ => new JsonObject(),
            DecodeItem = _ => null,
        };
    }

    private static InvalidOperationException Register(params Extension[] extensions)
    {
        return Assert.Throws<InvalidOperationException>(() => OpenResponsesExtensions.CreateRegistry(extensions));
    }

    [Fact]
    [UpstreamTest(Prefix + "should index registered extension semantics", Coverage = UpstreamCoverage.Covered)]
    public void Should_index_registered_extension_semantics()
    {
        var extension = CreateExtension();
        extension.EventTypes = new[] { "acme:search_delta" };
        extension.DecodeEvent = _ => null;
        var registry = OpenResponsesExtensions.CreateRegistry(new[] { extension });

        Assert.Same(extension, registry.ByExtensionId["acme.search"]);
        Assert.Same(extension, registry.ByProviderToolId["acme.search"]);
        Assert.Same(extension, registry.ByToolType["acme:search"]);
        Assert.Same(extension, registry.ByItemType["acme:search_call"]);
        Assert.Same(extension, registry.ByEventType["acme:search_delta"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should index explicitly registered bare extension semantics", Coverage = UpstreamCoverage.Covered)]
    public void Should_index_explicitly_registered_bare_extension_semantics()
    {
        var extension = new Extension
        {
            Id = "acme.search",
            AllowBareTypes = true,
            BareToolType = "web_search",
            BareItemTypes = new[] { "web_search_call" },
            BareEventTypes = new[] { "response.web_search_call.completed" },
            EncodeTool = _ => new JsonObject(),
            DecodeItem = _ => null,
            DecodeEvent = _ => null,
        };
        var registry = OpenResponsesExtensions.CreateRegistry(new[] { extension });

        Assert.Same(extension, registry.ByProviderToolId["acme.search"]);
        Assert.Same(extension, registry.ByToolType["web_search"]);
        Assert.Same(extension, registry.ByItemType["web_search_call"]);
        Assert.Same(extension, registry.ByEventType["response.web_search_call.completed"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should register tool, item, and event capabilities independently", Coverage = UpstreamCoverage.Covered)]
    public void Should_register_tool_item_and_event_capabilities_independently()
    {
        var toolExtension = CreateExtension();
        toolExtension.ItemTypes = null;
        toolExtension.DecodeItem = null;
        var itemExtension = new Extension
        {
            Id = "acme.receipts",
            ItemTypes = new[] { "acme:receipt" },
            DecodeItem = _ => null,
        };
        var eventExtension = new Extension
        {
            Id = "acme.progress",
            EventTypes = new[] { "acme:progress_delta" },
            DecodeEvent = _ => null,
        };

        var registry = OpenResponsesExtensions.CreateRegistry(new[] { toolExtension, itemExtension, eventExtension });

        Assert.Same(toolExtension, registry.ByProviderToolId["acme.search"]);
        Assert.Same(itemExtension, registry.ByItemType["acme:receipt"]);
        Assert.Same(eventExtension, registry.ByEventType["acme:progress_delta"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject incomplete capabilities", Coverage = UpstreamCoverage.Covered)]
    public void Should_reject_incomplete_capabilities()
    {
        var error = Register(new Extension { Id = "acme.search", ToolType = "acme:search" });

        Assert.Contains("must provide toolType and encodeTool together", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject wire types outside the provider-tool namespace", Coverage = UpstreamCoverage.Covered)]
    public void Should_reject_wire_types_outside_the_provider_tool_namespace()
    {
        var extension = CreateExtension();
        extension.ToolType = "other:search";

        var error = Register(extension);

        Assert.Contains("Extension wire types must use the acme: namespace.", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject bare wire types in namespaced fields", Coverage = UpstreamCoverage.Covered)]
    public void Should_reject_bare_wire_types_in_namespaced_fields()
    {
        var extension = CreateExtension();
        extension.ToolType = "web_search";

        var error = Register(extension);

        Assert.Contains("Extension wire types must use the acme: namespace.", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject namespaced wire types in bare fields", Coverage = UpstreamCoverage.Covered)]
    public void Should_reject_namespaced_wire_types_in_bare_fields()
    {
        var error = Register(new Extension
        {
            Id = "acme.search",
            AllowBareTypes = true,
            BareToolType = "acme:web_search",
            EncodeTool = _ => new JsonObject(),
        });

        Assert.Contains("Bare extension wire types must be non-empty and must not contain a colon.", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should require an explicit bare type opt-in", Coverage = UpstreamCoverage.Covered)]
    public void Should_require_an_explicit_bare_type_opt_in()
    {
        var withoutOptIn = Register(new Extension
        {
            Id = "acme.search",
            BareToolType = "web_search",
            EncodeTool = _ => new JsonObject(),
        });
        Assert.Contains("must set allowBareTypes to true exactly when registering bare types", withoutOptIn.Message, StringComparison.Ordinal);

        var withoutBareTypes = Register(new Extension
        {
            Id = "acme.search",
            AllowBareTypes = true,
            ToolType = "acme:search",
            EncodeTool = _ => new JsonObject(),
        });
        Assert.Contains("must set allowBareTypes to true exactly when registering bare types", withoutBareTypes.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject core wire types in bare fields", Coverage = UpstreamCoverage.Covered)]
    public void Should_reject_core_wire_types_in_bare_fields()
    {
        var tool = Register(new Extension
        {
            Id = "acme.search",
            AllowBareTypes = true,
            BareToolType = "function",
            EncodeTool = _ => new JsonObject(),
        });
        Assert.Contains("cannot register core bareToolType value function", tool.Message, StringComparison.Ordinal);

        var item = Register(new Extension
        {
            Id = "acme.search",
            AllowBareTypes = true,
            BareItemTypes = new[] { "function_call" },
            DecodeItem = _ => null,
        });
        Assert.Contains("cannot register core bareItemTypes value function_call", item.Message, StringComparison.Ordinal);

        var evt = Register(new Extension
        {
            Id = "acme.search",
            AllowBareTypes = true,
            BareEventTypes = new[] { "response.output_text.delta" },
            DecodeEvent = _ => null,
        });
        Assert.Contains("cannot register core bareEventTypes value response.output_text.delta", evt.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject ambiguous tool type registration", Coverage = UpstreamCoverage.Covered)]
    public void Should_reject_ambiguous_tool_type_registration()
    {
        var extension = CreateExtension();
        extension.AllowBareTypes = true;
        extension.BareToolType = "web_search";

        var error = Register(extension);

        Assert.Contains("cannot provide toolType and bareToolType together", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject duplicate item registrations", Coverage = UpstreamCoverage.Covered)]
    public void Should_reject_duplicate_item_registrations()
    {
        var other = CreateExtension();
        other.Id = "acme.other_search";
        other.ToolType = "acme:other_search";

        var error = Register(CreateExtension(), other);

        Assert.Contains("item type acme:search_call because it is already registered by acme.search", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject duplicate bare registrations", Coverage = UpstreamCoverage.Covered)]
    public void Should_reject_duplicate_bare_registrations()
    {
        var error = Register(
            new Extension
            {
                Id = "acme.search",
                AllowBareTypes = true,
                BareItemTypes = new[] { "web_search_call" },
                DecodeItem = _ => null,
            },
            new Extension
            {
                Id = "other.search",
                AllowBareTypes = true,
                BareItemTypes = new[] { "web_search_call" },
                DecodeItem = _ => null,
            });

        Assert.Contains("item type web_search_call because it is already registered by acme.search", error.Message, StringComparison.Ordinal);
    }
}
