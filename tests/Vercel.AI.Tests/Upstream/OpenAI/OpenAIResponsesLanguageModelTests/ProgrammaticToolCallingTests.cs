// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; programmatic tool calling</c>.</summary>
public sealed class ProgrammaticToolCallingTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > programmatic tool calling::";

    private static readonly LanguageModelTool[] Tools =
    {
        LanguageModelTool.ProviderDefined("openai.programmatic_tool_calling", "program", OpenAIUpstream.Json("{}")),
        new LanguageModelTool(
            "getInventory",
            null,
            OpenAIUpstream.Json("""{"type":"object","properties":{"sku":{"type":"string"}},"required":["sku"]}""")),
        new LanguageModelTool(
            "getDemand",
            null,
            OpenAIUpstream.Json("""{"type":"object","properties":{"sku":{"type":"string"}},"required":["sku"]}""")),
    };

    private static LanguageModelCallOptions Options()
    {
        var options = OpenAIUpstream.Hello();
        options.Tools = Tools;
        return options;
    }

    private static string Fixture(string kind, int step) =>
        OpenAIResponsesSupport.Fixture("openai-programmatic-tool-calling." + step + "." + kind);

    [Fact]
    [UpstreamTest(Prefix + "should map programmatic tool calling across generate steps from real fixtures", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_programmatic_tool_calling_across_generate_steps_from_real_fixtures()
    {
        var content = new List<GeneratedContent>();
        foreach (var step in new[] { 1, 2, 3 })
        {
            var capture = new OpenAICapture { ResponseJson = Fixture("json", step) };
            var result = await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5.6").DoGenerateAsync(Options(), CancellationToken.None);
            content.AddRange(result.Content);
        }

        var program = Assert.Single(content.OfType<GeneratedToolCall>(), call => call.ToolCallId == "call_O2IvSLQcJ0bwIvJZ2ovGV69M");
        Assert.Equal("program", program.ToolName);
        Assert.True(program.ProviderExecuted);
        JsonAssert.Equal(program.ProviderMetadata!.Value, """{"openai":{"itemId":"cm_0742d30c1d273351016a6145f1d2d0819faa3ecfc950fceec4"}}""");
        var programInput = JsonNode.Parse(program.ArgumentsJson)!.AsObject();
        Assert.Equal(new[] { "code", "fingerprint" }, programInput.Select(property => property.Key));

        var inventory = Assert.Single(content.OfType<GeneratedToolCall>(), call => call.ToolCallId == "call_rj6LW6NEyodD5YVKeoexoLNz");
        Assert.Equal("getInventory", inventory.ToolName);
        Assert.Equal("""{"sku":"sku_123"}""", inventory.ArgumentsJson);
        Assert.False(inventory.ProviderExecuted);
        JsonAssert.Equal(
            inventory.ProviderMetadata!.Value,
            """{"openai":{"itemId":"fc_0742d30c1d273351016a6145f1dac0819fb0053980ae918c16","caller":{"type":"program","callerId":"call_O2IvSLQcJ0bwIvJZ2ovGV69M"}}}""");

        var demand = Assert.Single(content.OfType<GeneratedToolCall>(), call => call.ToolCallId == "call_IYnPSr6i8TyBPs1H9U539pUP");
        Assert.Equal("getDemand", demand.ToolName);
        Assert.Equal("""{"sku":"sku_123"}""", demand.ArgumentsJson);
        Assert.False(demand.ProviderExecuted);
        JsonAssert.Equal(
            demand.ProviderMetadata!.Value,
            """{"openai":{"itemId":"fc_0742d30c1d273351016a6145f446e8819f9a2bd24df7057cd8","caller":{"type":"program","callerId":"call_O2IvSLQcJ0bwIvJZ2ovGV69M"}}}""");

        var output = Assert.Single(content.OfType<GeneratedToolResult>(), result => result.ToolCallId == "call_O2IvSLQcJ0bwIvJZ2ovGV69M");
        Assert.Equal("program", output.ToolName);
        Assert.False(output.IsError);
        JsonAssert.Equal(
            System.Text.Json.JsonDocument.Parse(output.ResultJson).RootElement,
            """{"result":"{\"sku\":\"sku_123\",\"availableUnits\":42,\"requestedUnits\":31,\"sufficient\":true}","status":"completed"}""");
        JsonAssert.Equal(output.ProviderMetadata!.Value, """{"openai":{"itemId":"cmo_0742d30c1d273351016a6145f6ba7c819f93fcba5b06569347"}}""");

        Assert.Contains(content.OfType<GeneratedText>(), part => part.Text.Contains("Inventory is sufficient for `sku_123`"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream programmatic tool calling across steps from real fixtures", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_programmatic_tool_calling_across_steps_from_real_fixtures()
    {
        var parts = new List<LanguageModelStreamPart>();
        foreach (var step in new[] { 1, 2, 3 })
        {
            var events = Fixture("chunks.txt", step)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Where(line => line.Trim().Length > 0)
                .Select(line => "data: " + line + "\n\n");
            var capture = new OpenAICapture { ServerSentEvents = string.Concat(events) + "data: [DONE]\n\n" };
            var result = await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5.6").DoStreamAsync(Options(), CancellationToken.None);
            parts.AddRange(await OpenAIUpstream.Read(result));
        }

        var program = Assert.Single(parts.OfType<ToolCallStreamPart>(), call => call.ToolCallId == "call_voPdoCqf8APY4DMpam3bdmxq");
        Assert.Equal("program", program.ToolName);
        Assert.True(program.ProviderExecuted);
        JsonAssert.Equal(program.ProviderMetadata!.Value, """{"openai":{"itemId":"cm_0bac52ec5f239d30016a61460092b08192afc4b546af158c46"}}""");
        Assert.Equal(new[] { "code", "fingerprint" }, JsonNode.Parse(program.ArgumentsJson)!.AsObject().Select(property => property.Key));

        var inventory = Assert.Single(parts.OfType<ToolCallStreamPart>(), call => call.ToolCallId == "call_VgDSZztLociNcutQZWkC2fmL");
        Assert.Equal("getInventory", inventory.ToolName);
        JsonAssert.Equal(
            inventory.ProviderMetadata!.Value,
            """{"openai":{"itemId":"fc_0bac52ec5f239d30016a61460099bc8192a9ebe7381b9efd87","caller":{"type":"program","callerId":"call_voPdoCqf8APY4DMpam3bdmxq"}}}""");

        var demand = Assert.Single(parts.OfType<ToolCallStreamPart>(), call => call.ToolCallId == "call_8GZvm5Bs4q0YSJIFH8hZeIcp");
        Assert.Equal("getDemand", demand.ToolName);
        JsonAssert.Equal(
            demand.ProviderMetadata!.Value,
            """{"openai":{"itemId":"fc_0bac52ec5f239d30016a6146031b5081928dcd2cd4ed0747ff","caller":{"type":"program","callerId":"call_voPdoCqf8APY4DMpam3bdmxq"}}}""");

        var output = Assert.Single(parts.OfType<ToolResultStreamPart>(), result => result.ToolCallId == "call_voPdoCqf8APY4DMpam3bdmxq");
        Assert.Equal("program", output.ToolName);
        JsonAssert.Equal(output.ProviderMetadata!.Value, """{"openai":{"itemId":"cmo_0bac52ec5f239d30016a614605889881929f4b52f614383886"}}""");
        JsonAssert.Equal(
            output.Result,
            """{"result":"{\"inventory\":{\"availableUnits\":42,\"sku\":\"sku_123\"},\"demand\":{\"requestedUnits\":31,\"sku\":\"sku_123\"}}","status":"completed"}""");

        Assert.Contains("Inventory is sufficient for `sku_123`", string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta)));
    }
}
