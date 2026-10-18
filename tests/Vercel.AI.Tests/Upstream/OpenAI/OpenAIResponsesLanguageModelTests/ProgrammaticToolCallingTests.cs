// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; programmatic tool calling</c>.</summary>
public sealed class ProgrammaticToolCallingTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > programmatic tool calling::";

    private static IReadOnlyList<LanguageModelTool> Tools()
    {
        using var empty = JsonDocument.Parse("{}");
        return new[]
        {
            LanguageModelTool.ProviderDefined("openai.programmatic_tool_calling", "program", empty.RootElement.Clone()),
            new LanguageModelTool("getInventory", null, OpenAIUpstream.Json("""{"type":"object","properties":{"sku":{"type":"string"}},"required":["sku"]}""")),
            new LanguageModelTool("getDemand", null, OpenAIUpstream.Json("""{"type":"object","properties":{"sku":{"type":"string"}},"required":["sku"]}""")),
        };
    }

    private static LanguageModelCallOptions Options()
    {
        var options = OpenAIUpstream.Hello();
        options.Tools = Tools();
        return options;
    }

    [Fact]
    [UpstreamTest(Prefix + "should map programmatic tool calling across generate steps from real fixtures", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_programmatic_tool_calling_across_generate_steps_from_real_fixtures()
    {
        var content = new List<GeneratedContent>();

        foreach (var step in new[] { 1, 2, 3 })
        {
            var capture = new OpenAICapture { ResponseJson = OpenAIResponsesSupport.Fixture($"programmatic-tool-calling.{step}.json") };
            var result = await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5.6").DoGenerateAsync(Options(), CancellationToken.None);
            content.AddRange(result.Content);
        }

        var program = Assert.Single(content.OfType<GeneratedToolCall>(), call => call.ToolName == "program");
        Assert.Equal("call_O2IvSLQcJ0bwIvJZ2ovGV69M", program.ToolCallId);
        Assert.True(program.ProviderExecuted);
        JsonAssert.Equal(program.ProviderMetadata!.Value, """{"openai":{"itemId":"cm_0742d30c1d273351016a6145f1d2d0819faa3ecfc950fceec4"}}""");

        var inventory = Assert.Single(content.OfType<GeneratedToolCall>(), call => call.ToolName == "getInventory");
        Assert.Equal("call_rj6LW6NEyodD5YVKeoexoLNz", inventory.ToolCallId);
        Assert.Equal("""{"sku":"sku_123"}""", inventory.ArgumentsJson);
        JsonAssert.Equal(
            inventory.ProviderMetadata!.Value,
            """{"openai":{"itemId":"fc_0742d30c1d273351016a6145f1dac0819fb0053980ae918c16","caller":{"type":"program","callerId":"call_O2IvSLQcJ0bwIvJZ2ovGV69M"}}}""");

        var demand = Assert.Single(content.OfType<GeneratedToolCall>(), call => call.ToolName == "getDemand");
        Assert.Equal("call_IYnPSr6i8TyBPs1H9U539pUP", demand.ToolCallId);
        Assert.Equal("""{"sku":"sku_123"}""", demand.ArgumentsJson);
        JsonAssert.Equal(
            demand.ProviderMetadata!.Value,
            """{"openai":{"itemId":"fc_0742d30c1d273351016a6145f446e8819f9a2bd24df7057cd8","caller":{"type":"program","callerId":"call_O2IvSLQcJ0bwIvJZ2ovGV69M"}}}""");

        var programResult = Assert.Single(content.OfType<GeneratedToolResult>());
        Assert.Equal("call_O2IvSLQcJ0bwIvJZ2ovGV69M", programResult.ToolCallId);
        Assert.Equal("program", programResult.ToolName);
        JsonAssert.Equal(
            JsonDocument.Parse(programResult.ResultJson).RootElement,
            """{"result":"{\"sku\":\"sku_123\",\"availableUnits\":42,\"requestedUnits\":31,\"sufficient\":true}","status":"completed"}""");
        JsonAssert.Equal(programResult.ProviderMetadata!.Value, """{"openai":{"itemId":"cmo_0742d30c1d273351016a6145f6ba7c819f93fcba5b06569347"}}""");

        Assert.Contains(content.OfType<GeneratedText>(), part => part.Text.Contains("Inventory is sufficient for `sku_123`"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream programmatic tool calling across steps from real fixtures", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_programmatic_tool_calling_across_steps_from_real_fixtures()
    {
        var parts = new List<LanguageModelStreamPart>();

        foreach (var step in new[] { 1, 2, 3 })
        {
            var events = OpenAIResponsesSupport.Fixture($"programmatic-tool-calling.{step}.chunks.txt")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => JsonNode.Parse(line)!);
            var capture = new OpenAICapture { ServerSentEvents = OpenAIResponsesSupport.Sse(events) };
            var stream = await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5.6").DoStreamAsync(Options(), CancellationToken.None);
            parts.AddRange(await OpenAIUpstream.Read(stream));
        }

        var program = Assert.Single(parts.OfType<ToolCallStreamPart>(), call => call.ToolName == "program");
        Assert.Equal("call_voPdoCqf8APY4DMpam3bdmxq", program.ToolCallId);
        Assert.True(program.ProviderExecuted);

        var inventory = Assert.Single(parts.OfType<ToolCallStreamPart>(), call => call.ToolName == "getInventory");
        Assert.Equal("call_VgDSZztLociNcutQZWkC2fmL", inventory.ToolCallId);
        JsonAssert.Equal(
            inventory.ProviderMetadata!.Value,
            """{"openai":{"itemId":"fc_0bac52ec5f239d30016a61460099bc8192a9ebe7381b9efd87","caller":{"type":"program","callerId":"call_voPdoCqf8APY4DMpam3bdmxq"}}}""");

        var demand = Assert.Single(parts.OfType<ToolCallStreamPart>(), call => call.ToolName == "getDemand");
        Assert.Equal("call_8GZvm5Bs4q0YSJIFH8hZeIcp", demand.ToolCallId);
        JsonAssert.Equal(
            demand.ProviderMetadata!.Value,
            """{"openai":{"itemId":"fc_0bac52ec5f239d30016a6146031b5081928dcd2cd4ed0747ff","caller":{"type":"program","callerId":"call_voPdoCqf8APY4DMpam3bdmxq"}}}""");

        var programResult = Assert.Single(parts.OfType<ToolResultStreamPart>(), result => result.ToolName == "program");
        Assert.Equal("call_voPdoCqf8APY4DMpam3bdmxq", programResult.ToolCallId);
        JsonAssert.Equal(
            programResult.Result,
            """{"result":"{\"inventory\":{\"availableUnits\":42,\"sku\":\"sku_123\"},\"demand\":{\"requestedUnits\":31,\"sku\":\"sku_123\"}}","status":"completed"}""");

        Assert.Contains(
            "Inventory is sufficient for `sku_123`",
            string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta)));
    }
}
