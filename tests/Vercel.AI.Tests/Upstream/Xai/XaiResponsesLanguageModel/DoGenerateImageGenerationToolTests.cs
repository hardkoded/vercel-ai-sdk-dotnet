// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doGenerate &gt; image_generation tool</c>.</summary>
public sealed class DoGenerateImageGenerationToolTests
{
    private const string Prefix = TestFile + " > doGenerate > image_generation tool::";

    [Fact]
    [UpstreamTest(Prefix + "should handle image_generation tool call from response fixture", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_image_generation_tool_call_from_response_fixture()
    {
        var result = await Model(JsonFixture("xai-image-generation-tool.1")).DoGenerateAsync(ImageGeneration("image_generation"), CancellationToken.None);

        JsonAssert.Equal(ToJson(result.Content), """
            [{"input":"{}","providerExecuted":true,"toolCallId":"ig_00000000-0000-4000-8000-000000000001_call-00000000-0000-4000-8000-00000000000a-0","toolName":"image_generation","type":"tool-call"},
             {"result":{"prompt":"A cute robot holding a flower, adorable design with big friendly eyes, shiny metallic body with soft rounded edges, holding a colorful flower in its hand, cheerful expression, simple clean background, high quality digital art","result":"ZmFrZUltYWdlQmFzZTY0RGF0YQ=="},
              "toolCallId":"ig_00000000-0000-4000-8000-000000000001_call-00000000-0000-4000-8000-00000000000a-0","toolName":"image_generation","type":"tool-result"},
             {"text":"Here's a cute robot holding a flower! 🌸🤖","type":"text"}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include image_generation tool call and result with providerExecuted true", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_image_generation_tool_call_and_result_with_providerExecuted_true()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","status":"completed","model":"grok-4-fast-non-reasoning","output":[
              {"type":"image_generation_call","id":"ig_123","status":"completed","prompt":"A corgi surfing a big wave, Japanese woodblock print style","result":"base64imagedata"},
              {"type":"message","id":"msg_123","status":"completed","role":"assistant","content":[{"type":"output_text","text":"Here is your image of a corgi surfing.","annotations":[]}]}],
             "usage":{"input_tokens":100,"output_tokens":20}}
            """);

        var result = await Model(capture).DoGenerateAsync(ImageGeneration("image_generation"), CancellationToken.None);

        JsonAssert.Equal(ToJson(result.Content), """
            [{"input":"{}","providerExecuted":true,"toolCallId":"ig_123","toolName":"image_generation","type":"tool-call"},
             {"result":{"prompt":"A corgi surfing a big wave, Japanese woodblock print style","result":"base64imagedata"},"toolCallId":"ig_123","toolName":"image_generation","type":"tool-result"},
             {"text":"Here is your image of a corgi surfing.","type":"text"}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should emit an error tool result for image_generation calls without result", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_emit_an_error_tool_result_for_image_generation_calls_without_result()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","status":"completed","model":"grok-4-fast-non-reasoning",
             "output":[{"type":"image_generation_call","id":"ig_456","status":"failed"}],"usage":{"input_tokens":100,"output_tokens":20}}
            """);

        var result = await Model(capture).DoGenerateAsync(ImageGeneration("image_generation"), CancellationToken.None);

        JsonAssert.Equal(ToJson(result.Content), """
            [{"input":"{}","providerExecuted":true,"toolCallId":"ig_456","toolName":"image_generation","type":"tool-call"},
             {"isError":true,"result":"Image generation failed (status: failed).","toolCallId":"ig_456","toolName":"image_generation","type":"tool-result"}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use the custom tool name for image_generation calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_the_custom_tool_name_for_image_generation_calls()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","status":"completed","model":"grok-4-fast-non-reasoning",
             "output":[{"type":"image_generation_call","id":"ig_123","status":"completed","prompt":"A hot air balloon over the desert","result":"base64imagedata"}],
             "usage":{"input_tokens":100,"output_tokens":20}}
            """);

        var result = await Model(capture).DoGenerateAsync(ImageGeneration("generateImage"), CancellationToken.None);

        JsonAssert.Equal(ToJson(result.Content), """
            [{"input":"{}","providerExecuted":true,"toolCallId":"ig_123","toolName":"generateImage","type":"tool-call"},
             {"result":{"prompt":"A hot air balloon over the desert","result":"base64imagedata"},"toolCallId":"ig_123","toolName":"generateImage","type":"tool-result"}]
            """);
    }

    private static LanguageModelCallOptions ImageGeneration(string name)
    {
        return WithTools(ProviderTool("xai.image_generation", name));
    }
}
