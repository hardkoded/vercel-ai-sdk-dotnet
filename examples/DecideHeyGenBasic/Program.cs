// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.HeyGen;
using Vercel.AI.Operations;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HEYGEN_API_KEY")))
{
    Console.WriteLine("Set HEYGEN_API_KEY to run this example.");
    return;
}

#region decide-heygen-basic
var result = await Decide.DecideAsync(
    new DecideRequest
    {
        Model = HeyGenProvider.Create().DecisionModel("heygen-decision-1"),
        State =
            "Video export fails for everyone on our team after today’s update. We have no workaround and cannot deliver our project.",
        Questions = new Dictionary<string, DecisionQuestion>
        {
            ["team"] = new(
                "choice",
                "Which support team should handle this request?",
                new DecisionChoiceCriteria(
                    new Dictionary<string, DecisionInput?>
                    {
                        ["video"] = "Video creation, rendering, or export problems",
                        ["account"] = "Sign-in, access, or workspace administration",
                        ["billing"] = "Payments, invoices, or subscription charges",
                    }
                )
            ),
            ["blocked"] = new(
                "boolean",
                "Is the customer unable to complete their work without a workaround?"
            ),
            ["impact"] = new(
                "score",
                "How much does this issue affect the customer’s work?",
                new DecisionScoreCriteria([
                    "Cosmetic issue; work can continue normally",
                    "Work is slower, but a usable workaround exists",
                    "Work cannot continue and no workaround is available",
                ])
            ),
        },
    }
);
#endregion

Console.WriteLine("Answers: " + JsonSerializer.Serialize(result.Answers));
Console.WriteLine(
    "Usage: " + result.Usage.InputTokens + " in, " + result.Usage.OutputTokens + " out"
);
Console.WriteLine("Model: " + result.Response?.ModelId);
Console.WriteLine("Provider metadata: " + result.ProviderMetadata);
