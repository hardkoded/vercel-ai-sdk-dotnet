// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.GenerateObject;

/// <summary>Maps to <c>parse-and-validate-object-result.ts</c>.</summary>
internal static class ParseAndValidateObjectResult
{
    /// <summary>
    /// Parses and validates a result string by parsing it as JSON and validating against the output strategy.
    /// Maps to <c>parseAndValidateObjectResult</c>.
    /// </summary>
    /// <exception cref="NoObjectGeneratedException">Parsing or validation fails.</exception>
    private static JsonElement Parse(
        string result,
        ObjectOutputStrategy outputStrategy,
        LanguageModelResponseMetadata response,
        LanguageModelUsage usage,
        FinishReason finishReason
    )
    {
        var parseResult = JsonParsing.SafeParse(result);
        if (!parseResult.Success)
        {
            throw new NoObjectGeneratedException(
                "No object generated: could not parse the response.",
                parseResult.Error,
                result,
                AsStepResponse(response),
                usage,
                finishReason
            );
        }

        var validationResult = outputStrategy.ValidateFinalResult(
            parseResult.Value,
            result,
            response,
            usage,
            finishReason
        );
        if (!validationResult.Success)
        {
            if (validationResult.Error is NoObjectGeneratedException noObject)
            {
                throw noObject;
            }

            throw new NoObjectGeneratedException(
                "No object generated: response did not match schema.",
                validationResult.Error,
                result,
                AsStepResponse(response),
                usage,
                finishReason
            );
        }

        return validationResult.Value;
    }

    /// <summary>
    /// Parses and validates a result string. If the result cannot be parsed, it attempts to repair the result using
    /// <paramref name="repairText"/>. Maps to <c>parseAndValidateObjectResultWithRepair</c>.
    /// </summary>
    /// <exception cref="NoObjectGeneratedException">Parsing or validation fails.</exception>
    internal static async Task<JsonElement> ParseAndValidateObjectResultWithRepairAsync(
        string result,
        ObjectOutputStrategy outputStrategy,
        Func<GenerateObjectRepairContext, CancellationToken, Task<string?>>? repairText,
        LanguageModelResponseMetadata response,
        LanguageModelUsage usage,
        FinishReason finishReason,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return Parse(result, outputStrategy, response, usage, finishReason);
        }
        catch (NoObjectGeneratedException error)
            when (repairText != null
                && error.InnerException
                    is JsonParseException
                        or ProviderUtils.TypeValidationException
            )
        {
            var repairedText = await repairText(
                    new GenerateObjectRepairContext(result, error.InnerException!),
                    cancellationToken
                )
                .ConfigureAwait(false);
            if (repairedText == null)
            {
                throw;
            }

            return Parse(repairedText, outputStrategy, response, usage, finishReason);
        }
    }

    private static StepResponse AsStepResponse(LanguageModelResponseMetadata response) =>
        response as StepResponse
        ?? new StepResponse(response.Id, response.Timestamp, response.ModelId, response.Headers);
}
