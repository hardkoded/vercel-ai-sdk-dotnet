// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Google;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleLanguageModelTests;

/// <summary>Port of <c>google-language-model.test.ts</c> &gt; <c>doGenerate</c> (the tests added for AUDIO response modalities and thought signatures on text).</summary>
public sealed class DoGenerateTests
{
    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should use the custom code execution tool name for generated and streamed results", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_the_custom_code_execution_tool_name_for_generated_and_streamed_results()
    {
        const string response = """{"candidates":[{"content":{"parts":[{"executableCode":{"language":"PYTHON","code":"print('ok')\nprint(1/0)"}},{"codeExecutionResult":{"outcome":"OUTCOME_OK","output":"ok\n"}},{"codeExecutionResult":{"outcome":"OUTCOME_FAILED","output":"ZeroDivisionError: division by zero\n"}}],"role":"model"},"finishReason":"STOP"}]}""";
        var handler = new SequencedHandler().Json(response).Sse("data: " + response + "\n\n");
        var provider = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key", GenerateId = () => "test-id" }, handler);
        var model = provider.LanguageModel("gemini-2.0-pro");
        var options = GoogleUpstream.Hello();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("""{"google":{"providerTools":[{"id":"google.code_execution","name":"CodeExecutionTool","args":{}}]}}""");

        var result = await model.DoGenerateAsync(options, CancellationToken.None);

        Assert.Single(handler.Bodies);
        Assert.Equal(3, result.Content.Count);
        var call = Assert.IsType<GoogleToolCall>(result.Content[0]);
        GoogleUpstream.JsonEqual(GoogleUpstream.Element(call.ArgumentsJson), """{"language":"PYTHON","code":"print('ok')\nprint(1/0)"}""");
        Assert.True(call.ProviderExecuted);
        Assert.Equal("test-id", call.ToolCallId);
        Assert.Equal("CodeExecutionTool", call.ToolName);
        var ok = Assert.IsType<GoogleToolResult>(result.Content[1]);
        Assert.Equal(("test-id", "CodeExecutionTool", "OUTCOME_OK", "ok\n"), (ok.ToolCallId, ok.ToolName, ok.Outcome, ok.Output));
        var failed = Assert.IsType<GoogleToolResult>(result.Content[2]);
        Assert.Equal(("test-id", "CodeExecutionTool", "OUTCOME_FAILED", "ZeroDivisionError: division by zero\n"), (failed.ToolCallId, failed.ToolName, failed.Outcome, failed.Output));

        var streamed = await GoogleUpstream.Collect((await model.DoStreamAsync(options, CancellationToken.None)).Stream);
        Assert.Equal(2, handler.Bodies.Count);
        var streamCall = Assert.Single(streamed.OfType<ToolCallStreamPart>());
        GoogleUpstream.JsonEqual(GoogleUpstream.Element(streamCall.ArgumentsJson), """{"language":"PYTHON","code":"print('ok')\nprint(1/0)"}""");
        Assert.Equal("CodeExecutionTool", streamCall.ToolName);
        Assert.Equal("test-id", streamCall.ToolCallId);
        var results = streamed.OfType<ToolResultStreamPart>().ToList();
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.Equal("CodeExecutionTool", r.ToolName));
        Assert.All(results, r => Assert.Equal("test-id", r.ToolCallId));
        Assert.Equal("OUTCOME_OK", results[0].Result.GetProperty("outcome").GetString());
        Assert.Equal("ok\n", results[0].Result.GetProperty("output").GetString());
        Assert.Equal("OUTCOME_FAILED", results[1].Result.GetProperty("outcome").GetString());
        Assert.Equal("ZeroDivisionError: division by zero\n", results[1].Result.GetProperty("output").GetString());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should pass thinkingLevel in provider options", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_thinkingLevel_in_provider_options()
    {
        AssertThinkingLevel("high");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should pass thinkingLevel \"minimal\" in provider options", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_thinkingLevel_minimal_in_provider_options()
    {
        AssertThinkingLevel("minimal");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should pass thinkingLevel \"medium\" in provider options", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_thinkingLevel_medium_in_provider_options()
    {
        AssertThinkingLevel("medium");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should send videoMetadata on video file parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_videoMetadata_on_video_file_parts()
    {
        var handler = new RecordingHandler();
        var model = GoogleUpstream.Client(handler).LanguageModel("gemini-2.5-flash");
        var file = new FileContentPart("video/mp4", "https://www.youtube.com/watch?v=example", null, null)
        {
            ProviderOptions = GoogleUpstream.ProviderOptions(
                """{"google":{"videoMetadata":{"startOffset":"0s","endOffset":"10.5s","fps":0.5}}}"""),
        };

        await model.DoGenerateAsync(
            GoogleUpstream.Prompt(new UserModelMessage(new UserContentPart[] { file })),
            CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Body);
        GoogleUpstream.JsonEqual(
            body.RootElement.GetProperty("contents"),
            """[{"role":"user","parts":[{"fileData":{"mimeType":"video/mp4","fileUri":"https://www.youtube.com/watch?v=example"},"videoMetadata":{"startOffset":"0s","endOffset":"10.5s","fps":0.5}}]}]""");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should pass AUDIO responseModalities in provider options", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_AUDIO_responseModalities_in_provider_options()
    {
        var options = GoogleUpstream.Hello();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"responseModalities\":[\"TEXT\",\"AUDIO\"]}}");

        var prepared = GoogleRequest.Prepare("gemini-pro", "google.generative-ai", options, false);

        GoogleUpstream.JsonEqual(prepared.Body["generationConfig"]!["responseModalities"], "[\"TEXT\",\"AUDIO\"]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should include audio inlineData parts", Coverage = UpstreamCoverage.Covered)]
    public void Should_include_audio_inlineData_parts()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Here is audio:\"},{\"inlineData\":{\"mimeType\":\"audio/wav\",\"data\":\"YXVkaW9kYXRh\"}}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}]}");

        Assert.Equal(2, result.Content.Count);
        Assert.Equal("Here is audio:", Assert.IsAssignableFrom<GeneratedText>(result.Content[0]).Text);
        var file = Assert.IsAssignableFrom<GeneratedFile>(result.Content[1]);
        Assert.Equal("audio/wav", file.MediaType);
        Assert.Equal("audiodata", System.Text.Encoding.UTF8.GetString(file.Data));
    }

    [Fact]
    public void Should_throw_on_inlineData_that_is_not_valid_base64()
    {
        var ex = Assert.Throws<InvalidResponseDataException>(() => GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"inlineData\":{\"mimeType\":\"audio/wav\",\"data\":\"audiodata\"}}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}]}"));

        Assert.Contains("base64", ex.Message);
    }

    [Fact]
    public void Should_throw_on_inlineData_without_data()
    {
        Assert.Throws<InvalidResponseDataException>(() => GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"inlineData\":{\"mimeType\":\"audio/wav\"}}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}]}"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should correctly parse thought signatures with reasoning parts", Coverage = UpstreamCoverage.Covered)]
    public void Should_correctly_parse_thought_signatures_with_reasoning_parts()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Visible text part 1. \",\"thoughtSignature\":\"sig1\"},{\"text\":\"This is a thought process.\",\"thought\":true,\"thoughtSignature\":\"sig2\"},{\"text\":\"Visible text part 2.\",\"thoughtSignature\":\"sig3\"}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}],\"usageMetadata\":{\"promptTokenCount\":10,\"candidatesTokenCount\":20,\"totalTokenCount\":30}}");

        Assert.Equal(new[] { "text", "reasoning", "text" }, result.Content.Select(part => part.Type).ToArray());
        var first = Assert.IsAssignableFrom<GeneratedText>(result.Content[0]);
        Assert.Equal("Visible text part 1. ", first.Text);
        GoogleUpstream.JsonEqual(first.ProviderMetadata!.Value, "{\"google\":{\"thoughtSignature\":\"sig1\"}}");
        var reasoning = Assert.IsAssignableFrom<GeneratedReasoning>(result.Content[1]);
        Assert.Equal("This is a thought process.", reasoning.Text);
        GoogleUpstream.JsonEqual(reasoning.ProviderMetadata!.Value, "{\"google\":{\"thoughtSignature\":\"sig2\"}}");
        var last = Assert.IsAssignableFrom<GeneratedText>(result.Content[2]);
        Assert.Equal("Visible text part 2.", last.Text);
        GoogleUpstream.JsonEqual(last.ProviderMetadata!.Value, "{\"google\":{\"thoughtSignature\":\"sig3\"}}");
    }

    private static void AssertThinkingLevel(string level)
    {
        var options = GoogleUpstream.Hello();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"thinkingConfig\":{\"thinkingLevel\":\"" + level + "\"}}}");

        var prepared = GoogleRequest.Prepare("gemini-pro", "google.generative-ai", options, false);

        GoogleUpstream.JsonEqual(prepared.Body["generationConfig"]!["thinkingConfig"], "{\"thinkingLevel\":\"" + level + "\"}");
    }
}
