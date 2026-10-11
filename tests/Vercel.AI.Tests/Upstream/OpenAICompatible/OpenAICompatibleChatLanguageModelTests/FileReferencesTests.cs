// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAICompatibleChatLanguageModelTests;

/// <summary>Port of <c>openai-compatible-chat-language-model.test.ts</c> &gt; <c>file references (%s)</c>.</summary>
public sealed class FileReferencesTests
{
    [Theory]
    [InlineData("doGenerate")]
    [InlineData("doStream")]
    [UpstreamTest("packages/openai-compatible/src/chat/openai-compatible-chat-language-model.test.ts::file references (%s)::should resolve file IDs using the provider name even with camelCase options", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_resolve_file_IDs_using_the_provider_name_even_with_camelCase_options(string mode)
    {
        var capture = new UpstreamCapture();
        if (mode == "doStream")
        {
            capture.MediaType = "text/event-stream";
            capture.ResponseBody = UpstreamChat.Sse("{\"choices\":[{\"delta\":{\"content\":\"Hi\"},\"finish_reason\":\"stop\"}]}");
        }

        using var userOptions = JsonDocument.Parse("{\"user\":\"test-user\"}");
        var reference = new Dictionary<string, string> { ["test-provider"] = "file-pdf-123", ["testProvider"] = "file-wrong-provider" };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[] { FileContentPart.FromProviderReference("application/pdf", reference) }),
            },
            ProviderOptions = new Dictionary<string, JsonElement> { ["testProvider"] = userOptions.RootElement.Clone() },
        };
        var model = UpstreamChat.Model(capture, "test-provider");

        if (mode == "doStream")
        {
            await UpstreamChat.Read(model.DoStreamAsync(options, CancellationToken.None));
        }
        else
        {
            await model.DoGenerateAsync(options, CancellationToken.None);
        }

        var content = UpstreamChat.Body(capture)["messages"]![0]!["content"]!;
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("[{\"type\":\"file\",\"file\":{\"file_id\":\"file-pdf-123\"}}]"), content));
    }
}
