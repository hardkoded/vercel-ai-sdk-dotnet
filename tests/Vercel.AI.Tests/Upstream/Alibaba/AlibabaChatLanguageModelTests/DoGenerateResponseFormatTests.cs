// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests.AlibabaChatTestSupport;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests;

/// <summary>Port of <c>alibaba-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; response format</c>.</summary>
public sealed class DoGenerateResponseFormatTests
{
    private const string Upstream = "packages/alibaba/src/alibaba-chat-language-model.test.ts::doGenerate > response format::";

    private const string Schema = "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"],\"additionalProperties\":false}";

    private const string PromptMessage = "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]}";

    private static LanguageModelCallOptions JsonOptions(bool withSchema)
    {
        var options = UpstreamChat.Prompt();
        options.JsonResponse = true;
        if (withSchema)
        {
            options.JsonSchema = JsonDocument.Parse(Schema).RootElement.Clone();
        }

        return options;
    }

    [Fact]
    [UpstreamTest(Upstream + "uses JSON Schema mode for supported Qwen models", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_JSON_Schema_mode_for_supported_Qwen_models()
    {
        var capture = CaptureFixture("alibaba-text");
        var schemaModel = Model(capture, "qwen3.8-flash");

        var result = await schemaModel.DoGenerateAsync(JsonOptions(withSchema: true), CancellationToken.None);

        var body = Body(capture);
        Assert.Equal("qwen3.8-flash", body["model"]!.GetValue<string>());
        AssertJson("[" + PromptMessage + "]", body["messages"]);
        AssertJson("{\"type\":\"json_schema\",\"json_schema\":{\"name\":\"response\",\"schema\":" + Schema + "}}", body["response_format"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Upstream + "uses JSON Object mode and injects the schema for other models", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_JSON_Object_mode_and_injects_the_schema_for_other_models()
    {
        var capture = CaptureFixture("alibaba-text");
        var jsonObjectModel = Model(capture, "deepseek-v4.1-flash");

        var result = await jsonObjectModel.DoGenerateAsync(JsonOptions(withSchema: true), CancellationToken.None);

        var body = Body(capture);
        Assert.Equal("deepseek-v4.1-flash", body["model"]!.GetValue<string>());
        var instruction = "JSON schema:\n" + JsonNode.Parse(Schema)!.ToJsonString() + "\nYou MUST answer with a JSON object that matches the JSON schema above.";
        AssertJson("[{\"role\":\"system\",\"content\":" + JsonValue.Create(instruction)!.ToJsonString() + "}," + PromptMessage + "]", body["messages"]);
        AssertJson("{\"type\":\"json_object\"}", body["response_format"]);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal("responseFormat JSON schema", warning.Feature);
        Assert.Equal("Alibaba does not support JSON Schema output for model deepseek-v4.1-flash. JSON Object mode is used instead. The schema was injected into the system message and will only be validated locally.", warning.Message);
    }

    [Fact]
    [UpstreamTest(Upstream + "injects a JSON instruction for schema-less JSON Object mode", Coverage = UpstreamCoverage.Covered)]
    public async Task Injects_a_JSON_instruction_for_schema_less_JSON_Object_mode()
    {
        var capture = CaptureFixture("alibaba-text");
        var model = Model(capture);

        var result = await model.DoGenerateAsync(JsonOptions(withSchema: false), CancellationToken.None);

        var body = Body(capture);
        AssertJson("[{\"role\":\"system\",\"content\":\"You MUST answer with JSON.\"}," + PromptMessage + "]", body["messages"]);
        AssertJson("{\"type\":\"json_object\"}", body["response_format"]);
        Assert.Empty(result.Warnings);
    }
}
