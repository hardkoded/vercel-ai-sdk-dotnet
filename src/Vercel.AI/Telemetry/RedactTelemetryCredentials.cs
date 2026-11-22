// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;

namespace Vercel.AI;

/// <summary>
/// Applies the provider-owned credential redaction to the copy of an event that telemetry gets. Only the metadata of the SDK is visited, never
/// prompts, tool outputs, or user context. Maps to <c>redactTelemetryCredentials</c> in <c>telemetry/redact-telemetry-credentials.ts</c>.
/// </summary>
internal static class RedactTelemetryCredentialsHelper
{
    /// <summary>Redacts the request and response bodies of every step of the result. Maps to <c>redactTelemetryCredentials</c>.</summary>
    internal static GenerateTextResult RedactTelemetryCredentials(GenerateTextResult value)
    {
        if (
            !value.Steps.Any(step =>
                step.Request.RedactBody != null || step.Response.RedactBody != null
            )
        )
        {
            return value;
        }

        return value.WithSteps(value.Steps.Select(RedactStep).ToList());
    }

    private static StepResult RedactStep(StepResult step)
    {
        if (step.Request.RedactBody == null && step.Response.RedactBody == null)
        {
            return step;
        }

        var request = new LanguageModelRequestMetadata
        {
            Url = step.Request.Url,
            Messages = step.Request.Messages,
            // A body whose redactor throws is omitted.
            Body = step.Request.Body is { } requestBody
                ? TelemetryRedaction.RedactTelemetryValue(requestBody, step.Request.RedactBody)
                : null,
        };
        var response = new StepResponse(
            step.Response.Id,
            step.Response.Timestamp,
            step.Response.ModelId,
            step.Response.Headers,
            step.Response.Messages,
            step.Response.Body is { } responseBody
                ? TelemetryRedaction.RedactTelemetryValue(responseBody, step.Response.RedactBody)
                : null
        );
        return step.WithRedactedMetadata(request, response);
    }
}
