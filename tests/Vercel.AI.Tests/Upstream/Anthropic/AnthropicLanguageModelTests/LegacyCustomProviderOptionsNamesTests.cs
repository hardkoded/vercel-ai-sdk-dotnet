// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; legacy custom provider options names</c>.</summary>
public sealed class LegacyCustomProviderOptionsNamesTests
{
    private const string Prefix = "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > legacy custom provider options names::";
    private const string Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d";

    private const string Response = "{\"id\":\"msg_legacy_test\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Hello, World!\"}],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":\"end_turn\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":10,\"output_tokens\":20}}";

    private static ILanguageModel CreateGoogleVertexNamedModel(CaptureHandler handler)
    {
        return AnthropicProvider
            .Create(new AnthropicOptions { ApiKey = "test-api-key", Name = "googleVertex" }, handler)
            .LanguageModel("claude-3-haiku-20240307");
    }

    private static LanguageModelCallOptions Call(string providerOptions, bool withTools)
    {
        var options = AnthropicParity.Hello();
        var parsed = new Dictionary<string, JsonElement>();
        using (var document = JsonDocument.Parse(providerOptions))
        {
            foreach (var property in document.RootElement.EnumerateObject())
            {
                parsed[property.Name] = property.Value.Clone();
            }
        }

        options.ProviderOptions = parsed;

        if (withTools)
        {
            options.Tools = new[] { new LanguageModelTool("testTool", null, AnthropicParity.Json("{\"type\":\"object\",\"properties\":{}}")) };
        }

        return options;
    }

    [Fact]
    [UpstreamTest(Prefix + "reads the legacy \"vertex\" key when \"googleVertex\" is absent", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Reads_the_legacy_vertex_key_when_googleVertex_is_absent()
    {
        var handler = AnthropicParity.Ok(Response);

        var result = await CreateGoogleVertexNamedModel(handler).DoGenerateAsync(
            Call("{\"vertex\":{\"disableParallelToolUse\":true}}", withTools: true),
            CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Body);
        var toolChoice = body.RootElement.GetProperty("tool_choice");
        Assert.Equal("auto", toolChoice.GetProperty("type").GetString());
        Assert.True(toolChoice.GetProperty("disable_parallel_tool_use").GetBoolean());
        Assert.Equal(new[] { "anthropic" }, result.ProviderMetadata!.Value.EnumerateObject().Select(property => property.Name).ToArray());
    }

    [Fact]
    [UpstreamTest(Prefix + "ignores the legacy \"vertex\" key when \"googleVertex\" is present", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Ignores_the_legacy_vertex_key_when_googleVertex_is_present()
    {
        var handler = AnthropicParity.Ok(Response);

        await CreateGoogleVertexNamedModel(handler).DoGenerateAsync(
            Call("{\"googleVertex\":{\"disableParallelToolUse\":false},\"vertex\":{\"disableParallelToolUse\":true}}", withTools: true),
            CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Body);
        var toolChoice = body.RootElement.GetProperty("tool_choice");
        Assert.False(toolChoice.TryGetProperty("disable_parallel_tool_use", out var disabled) && disabled.ValueKind == JsonValueKind.True);
    }

    [Fact]
    [UpstreamTest(Prefix + "ignores Vertex-only keys such as labels under \"vertex\"", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Ignores_Vertex_only_keys_such_as_labels_under_vertex()
    {
        var handler = AnthropicParity.Ok(Response);

        await CreateGoogleVertexNamedModel(handler).DoGenerateAsync(
            Call("{\"vertex\":{\"labels\":{\"team\":\"ai\"}}}", withTools: false),
            CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Body);
        Assert.False(body.RootElement.TryGetProperty("labels", out _));
    }
}
