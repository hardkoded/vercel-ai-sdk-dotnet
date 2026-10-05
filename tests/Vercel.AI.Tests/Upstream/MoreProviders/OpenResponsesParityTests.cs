// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenResponses;

namespace Vercel.AI.Tests;

/// <summary>Open Responses extension registry matched to the upstream catalog.</summary>
public sealed class OpenResponsesParityTests
{
    [Fact]
    [UpstreamTest("packages/open-responses/src/open-responses-extension.test.ts::createOpenResponsesExtensionRegistry::should index registered extension semantics", Coverage = UpstreamCoverage.Covered)]
    public void Registry_indexes_tool_item_and_event_types()
    {
        var extension = Extension(eventTypes: new[] { "acme:search_delta" }, decodeEvent: true);
        var registry = OpenResponsesExtensions.CreateRegistry(new[] { extension });
        Assert.Same(extension, registry.ByExtensionId["acme.search"]);
        Assert.Same(extension, registry.ByProviderToolId["acme.search"]);
        Assert.Same(extension, registry.ByToolType["acme:search"]);
        Assert.Same(extension, registry.ByItemType["acme:search_call"]);
        Assert.Same(extension, registry.ByEventType["acme:search_delta"]);
    }

    [Fact]
    [UpstreamTest("packages/open-responses/src/open-responses-extension.test.ts::createOpenResponsesExtensionRegistry::should register tool, item, and event capabilities independently", Coverage = UpstreamCoverage.Covered)]
    public void Registry_accepts_each_capability_on_its_own()
    {
        var tool = Extension();
        tool.ItemTypes = null;
        tool.DecodeItem = null;
        var item = new OpenResponsesExtension
        {
            Id = "acme.receipts",
            ItemTypes = new[] { "acme:receipt" },
            DecodeItem = _ => null,
        };
        var events = new OpenResponsesExtension
        {
            Id = "acme.progress",
            EventTypes = new[] { "acme:progress_delta" },
            DecodeEvent = _ => null,
        };
        var registry = OpenResponsesExtensions.CreateRegistry(new[] { tool, item, events });
        Assert.Same(tool, registry.ByProviderToolId["acme.search"]);
        Assert.Same(item, registry.ByItemType["acme:receipt"]);
        Assert.Same(events, registry.ByEventType["acme:progress_delta"]);
    }

    [Fact]
    [UpstreamTest("packages/open-responses/src/open-responses-extension.test.ts::createOpenResponsesExtensionRegistry::should reject incomplete capabilities", Coverage = UpstreamCoverage.Covered)]
    public void Registry_rejects_a_tool_type_without_an_encoder()
    {
        var error = Assert.Throws<InvalidOperationException>(() => OpenResponsesExtensions.CreateRegistry(new[]
        {
            new OpenResponsesExtension { Id = "acme.search", ToolType = "acme:search" },
        }));
        Assert.Contains("must provide toolType and encodeTool together", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/open-responses/src/open-responses-extension.test.ts::createOpenResponsesExtensionRegistry::should reject wire types outside the provider-tool namespace", Coverage = UpstreamCoverage.Covered)]
    public void Registry_rejects_a_foreign_tool_namespace()
    {
        var extension = Extension();
        extension.ToolType = "other:search";
        var error = Assert.Throws<InvalidOperationException>(() => OpenResponsesExtensions.CreateRegistry(new[] { extension }));
        Assert.Contains("Extension wire types must use the acme: namespace.", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/open-responses/src/open-responses-extension.test.ts::createOpenResponsesExtensionRegistry::should reject duplicate item registrations", Coverage = UpstreamCoverage.Covered)]
    public void Registry_rejects_a_repeated_item_type()
    {
        var other = Extension();
        other.Id = "acme.other_search";
        other.ToolType = "acme:other_search";
        var error = Assert.Throws<InvalidOperationException>(() => OpenResponsesExtensions.CreateRegistry(new[] { Extension(), other }));
        Assert.Contains("item type acme:search_call because it is already registered by acme.search", error.Message, StringComparison.Ordinal);
    }

    private static OpenResponsesExtension Extension(string[]? eventTypes = null, bool decodeEvent = false)
    {
        return new OpenResponsesExtension
        {
            Id = "acme.search",
            ToolType = "acme:search",
            ItemTypes = new[] { "acme:search_call" },
            EventTypes = eventTypes,
            EncodeTool = _ => new object(),
            DecodeItem = _ => null,
            DecodeEvent = decodeEvent ? _ => null : null,
        };
    }
}
