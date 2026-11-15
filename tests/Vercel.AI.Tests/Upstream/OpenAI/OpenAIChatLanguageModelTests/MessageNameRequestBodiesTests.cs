// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIChatLanguageModelTests;

/// <summary>Port of <c>openai-chat-language-model.test.ts</c> &gt; <c>message name request bodies</c>.</summary>
public sealed class MessageNameRequestBodiesTests
{
    private const string Prefix = "packages/openai/src/chat/openai-chat-language-model.test.ts::message name request bodies::";

    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [UpstreamTest(Prefix + "serializes names in %s requests", Coverage = UpstreamCoverage.Covered, Commit = "243bf221a89315cbe3d0c91161e5e61b763736d7")]
    public async Task Serializes_names_in_requests(string mode)
    {
        var capture = new OpenAICapture();
        if (mode == "stream")
        {
            capture.ServerSentEvents =
                "data: {\"id\":\"chatcmpl-1\",\"object\":\"chat.completion.chunk\",\"created\":1,\"model\":\"gpt-4o\",\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"OK\"},\"finish_reason\":null}]}\n\n"
                + "data: {\"id\":\"chatcmpl-1\",\"object\":\"chat.completion.chunk\",\"created\":1,\"model\":\"gpt-4o\",\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\"}]}\n\n"
                + "data: [DONE]\n\n";
        }

        var model = OpenAIUpstream.Provider(capture).ChatModel("gpt-4o");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("Hello") { ProviderOptions = Named("alice") },
                new AssistantModelMessage("Hi", null, null) { ProviderOptions = Named("bot") },
                new UserModelMessage("Say OK") { ProviderOptions = Named("bob") },
            },
        };
        if (mode == "generate")
        {
            await model.DoGenerateAsync(options, CancellationToken.None);
        }
        else
        {
            await OpenAIUpstream.Read(await model.DoStreamAsync(options, CancellationToken.None));
        }

        var body = JsonNode.Parse(capture.Body)!;
        OpenAIUpstream.Equal(
            body["messages"],
            "[{\"role\":\"user\",\"name\":\"alice\",\"content\":\"Hello\"},{\"role\":\"assistant\",\"name\":\"bot\",\"content\":\"Hi\"},{\"role\":\"user\",\"name\":\"bob\",\"content\":\"Say OK\"}]");
    }

    private static Dictionary<string, JsonElement> Named(string name)
    {
        return new Dictionary<string, JsonElement>
        {
            ["openai"] = OpenAIUpstream.Json("{\"name\":\"" + name + "\"}"),
        };
    }
}
