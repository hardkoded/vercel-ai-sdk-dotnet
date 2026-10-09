// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Examples;

public static class StreamingExample
{
    public static async Task RunAsync()
    {
        var client = new AiClient(Vercel.AI.Gateway.GatewayProvider.Create(new() { ApiKey = "unused" }));
        var model = new TestLanguageModel();

        #region streaming
        var stream = client.StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "Draft a changelog entry.",
        });

        await foreach (var delta in stream.TextStream())
        {
            Console.Write(delta);
        }

        Console.WriteLine();
        Console.WriteLine(await stream.FinishReason);
        #endregion
    }
}
