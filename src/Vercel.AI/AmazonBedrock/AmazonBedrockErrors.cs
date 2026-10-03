// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Maps a Bedrock JSON error body onto the SDK exception hierarchy.</summary>
public static class AmazonBedrockErrors
{
    /// <summary>
    /// Returns the provider message. When <c>type</c> is present the message is <c>{type}: {message}</c>.
    /// </summary>
    public static string? FormatMessage(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            return FormatMessage(document.RootElement);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Formats a parsed Bedrock error object.</summary>
    public static string? FormatMessage(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("message", out var messageElement) || messageElement.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var message = messageElement.GetString() ?? string.Empty;
        if (root.TryGetProperty("type", out var typeElement) && typeElement.ValueKind == JsonValueKind.String)
        {
            var type = typeElement.GetString();
            if (!string.IsNullOrEmpty(type))
            {
                return type + ": " + message;
            }
        }

        return message;
    }

    /// <summary>Creates the status exception, using the Bedrock <c>type</c> prefix when the body has one.</summary>
    public static ApiException Create(int statusCode, string? body)
    {
        var message = FormatMessage(body) ?? JsonValues.ExtractErrorMessage(body, statusCode);
        switch (statusCode)
        {
            case 400:
                return new BadRequestException(message, body);
            case 401:
                return new AuthenticationException(message, body);
            case 403:
                return new PermissionDeniedException(message, body);
            case 404:
                return new NotFoundException(message, body);
            case 422:
                return new UnprocessableEntityException(message, body);
            case 429:
                return new RateLimitException(message, body);
            default:
                if (statusCode >= 500 && statusCode <= 599)
                {
                    return new InternalServerException(message, statusCode, body);
                }

                return new ApiException(message, statusCode, body);
        }
    }
}
