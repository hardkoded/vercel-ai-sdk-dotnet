// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Sdk;
using Vercel.AI.Sdk.AspNetCore;
using Vercel.AI.Sdk.Gateway;
using Vercel.AI.Sdk.Provider;
using Vercel.AI.Sdk.Testing;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var gatewayKey = Environment.GetEnvironmentVariable("AI_GATEWAY_API_KEY");
var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = gatewayKey ?? "local" }));

app.MapGet("/", () => Results.Text("POST /chat with {\"prompt\":\"...\"} to stream a UI message response."));

app.MapPost("/chat", (ChatRequest request, CancellationToken cancellationToken) =>
{
    ILanguageModel model = string.IsNullOrEmpty(gatewayKey)
        ? new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new TextDeltaStreamPart("text", "Hello from the Vercel AI SDK for .NET."),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(1, 8, 9), "stop"),
            },
        }
        : GatewayProvider.Create().LanguageModel("openai/gpt-4.1-mini");

    var stream = client.StreamTextAsync(new StreamTextOptions
    {
        Model = model,
        Prompt = string.IsNullOrWhiteSpace(request.Prompt) ? "Hello" : request.Prompt,
    }, cancellationToken);
    return stream.ToUIMessageStreamResult();
});

app.Run("http://127.0.0.1:43123");

internal sealed record ChatRequest(string? Prompt);
