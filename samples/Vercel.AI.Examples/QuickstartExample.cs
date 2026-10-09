// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Examples;

public static class QuickstartExample
{
    public static async Task<string> RunAsync()
    {
        ILanguageModel model = new TestLanguageModel
        {
            OnGenerate = _ => TestLanguageModel.Text("A language model specification is a shared contract for calling models."),
        };

        #region quickstart
        var client = new AiClient(Vercel.AI.Gateway.GatewayProvider.Create(new() { ApiKey = "unused" }));
        var result = await client.GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Instructions = "Answer in one sentence.",
            Prompt = "What is a language model specification?",
        });
        Console.WriteLine(result.Text);
        #endregion

        return result.Text;
    }
}
