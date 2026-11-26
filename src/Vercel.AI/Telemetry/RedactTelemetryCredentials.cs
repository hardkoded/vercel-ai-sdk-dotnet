// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI;

/// <summary>
/// Applies the provider-owned credential redaction to the copy of an event that telemetry gets. Only the metadata of the SDK is visited, never
/// prompts, tool outputs, or user context. Maps to <c>redactTelemetryCredentials</c> in <c>telemetry/redact-telemetry-credentials.ts</c>.
/// </summary>
internal static class RedactTelemetryCredentialsHelper
{
    private const string Redacted = "[redacted]";

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

    /// <summary>
    /// Redacts a model-call result or error before a nested integration sees it. Any other value is returned as is.
    /// Maps to <c>redactTelemetryCredentials</c>.
    /// </summary>
    internal static T RedactTelemetryCredentials<T>(T value)
    {
        object? redacted = value switch
        {
            LanguageModelStreamResult { Request.RedactBody: not null } stream =>
                stream.WithTelemetryRequest(
                    new LanguageModelStreamRequest(stream.Request.RedactTelemetryBody())
                    {
                        Url = stream.Request.Url,
                    }
                ),
            LanguageModelGenerateResult generate
                when generate.Request?.RedactBody != null || generate.RedactRawResponse != null =>
                generate.WithRedactedTelemetry(),
            Exception error => RedactTelemetryCredentials(error),
            _ => value,
        };
        return (T)redacted!;
    }

    /// <summary>Replaces a download error with a copy that has no URL. Any other error is returned as is.</summary>
    internal static Exception RedactTelemetryCredentials(Exception error)
    {
        switch (error)
        {
            case DownloadError download:
                return new DownloadError(
                    Redacted,
                    "Failed to download a file.",
                    download.StatusCode,
                    download.StatusText
                );
            case ApiCallError { RequestBodyValues: TelemetryRedactedValue body } api
                when api.GetType() == typeof(ApiCallError):
                // Provider errors carry the same request body outside the request metadata.
                return new ApiCallError(
                    api.Message,
                    api.Url,
                    TelemetryRedaction.RedactTelemetryValue(body) is { } redacted
                        ? redacted
                        : new object(),
                    api.StatusCode,
                    api.ResponseHeaders,
                    api.ResponseBody,
                    api.InnerException,
                    api.IsRetryable,
                    api.Data
                );
            case ApiException { RequestBodyValues: TelemetryRedactedValue body } api:
                return ProviderHttp.MapStatusWithHeaders(
                    api.StatusCode,
                    api.Message,
                    api.ResponseBody,
                    api.ResponseHeaders,
                    api.Url,
                    TelemetryRedaction.RedactTelemetryValue(body) is { } redactedBody
                        ? redactedBody
                        : new object(),
                    api.IsRetryable
                );
            // Gateway errors keep the failed API call as their cause, and the call keeps the request body.
            case Vercel.AI.Gateway.GatewayError { Cause: { } cause } gateway:
                return
                    RedactTelemetryCredentials(cause) is var redactedCause
                    && !ReferenceEquals(redactedCause, cause)
                    ? GatewayErrors.AsGatewayError(redactedCause, gateway.AuthMethod)
                    : error;
            // RetryError exposes both the full attempt history and the final error.
            case RetryError retry:
                var errors = retry
                    .Errors.Select(entry =>
                        entry is Exception inner ? RedactTelemetryCredentials(inner) : entry
                    )
                    .ToList();
                return errors
                    .Where((entry, index) => !ReferenceEquals(entry, retry.Errors[index]))
                    .Any()
                    ? new RetryError(retry.Message, retry.Reason, errors)
                    : error;
            default:
                return error;
        }
    }
}
