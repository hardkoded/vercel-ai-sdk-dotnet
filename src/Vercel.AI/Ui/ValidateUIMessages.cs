// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Ui;

/// <summary>Options of <see cref="Ai.ValidateUIMessagesAsync"/>. Maps to <c>ValidateUIMessagesOptions</c>.</summary>
public sealed class ValidateUIMessagesOptions
{
    /// <summary>The messages to validate.</summary>
    public IReadOnlyList<UIMessage>? Messages { get; set; }

    /// <summary>JSON Schema of the message metadata. Without it the metadata is not validated. Maps to <c>metadataSchema</c>.</summary>
    public JsonNode? MetadataSchema { get; set; }

    /// <summary>JSON Schemas of the data parts, by data name. Without them the data parts are not validated. Maps to <c>dataSchemas</c>.</summary>
    public IReadOnlyDictionary<string, JsonNode>? DataSchemas { get; set; }

    /// <summary>
    /// The tools of the tool parts, by name. Only the input and output schemas are used. Without them the tool parts are not validated.
    /// </summary>
    public IReadOnlyDictionary<string, Tool>? Tools { get; set; }

    /// <summary>
    /// Rebuilds the input of a tool call from its <c>inputSchemaInput</c>, by tool name, for an input schema whose parsed value
    /// differs from its input. Maps to <c>experimental_refineToolInput</c>.
    /// </summary>
    public IReadOnlyDictionary<
        string,
        Func<JsonElement, Task<JsonElement>>
    >? ExperimentalRefineToolInput { get; set; }
}

/// <summary>The result of <see cref="Ai.SafeValidateUIMessagesAsync"/>. Maps to <c>SafeValidateUIMessagesResult</c>.</summary>
public sealed class SafeValidateUIMessagesResult
{
    private SafeValidateUIMessagesResult(
        bool success,
        IReadOnlyList<UIMessage>? data,
        Exception? error
    )
    {
        Success = success;
        Data = data;
        Error = error;
    }

    /// <summary>True when the messages are valid.</summary>
    public bool Success { get; }

    /// <summary>The validated messages. Null on failure.</summary>
    public IReadOnlyList<UIMessage>? Data { get; }

    /// <summary>The validation error. Null on success.</summary>
    public Exception? Error { get; }

    internal static SafeValidateUIMessagesResult Ok(IReadOnlyList<UIMessage> data) =>
        new(true, data, null);

    internal static SafeValidateUIMessagesResult Fail(Exception error) => new(false, null, error);
}

/// <summary>Maps to <c>validate-ui-messages.ts</c>.</summary>
internal static class UIMessageValidation
{
    /// <summary>Maps to <c>safeValidateUIMessages</c>.</summary>
    internal static Task<SafeValidateUIMessagesResult> SafeValidateUIMessagesAsync(
        ValidateUIMessagesOptions options
    )
    {
        return SafeValidateUIMessagesInternalAsync(
            options,
            convertMissingTerminalToolsToDynamic: false
        );
    }

    /// <summary>Maps to <c>validateUIMessages</c>.</summary>
    internal static async Task<IReadOnlyList<UIMessage>> ValidateUIMessagesAsync(
        ValidateUIMessagesOptions options
    )
    {
        var response = await SafeValidateUIMessagesAsync(options).ConfigureAwait(false);
        return response.Success ? response.Data! : throw response.Error!;
    }

    /// <summary>Maps to <c>validateUIMessagesForAgent</c>.</summary>
    internal static async Task<IReadOnlyList<UIMessage>> ValidateUIMessagesForAgentAsync(
        ValidateUIMessagesOptions options
    )
    {
        // Agent tool sets can include ephemeral tools (for example, tools from a disconnected MCP server), so terminal
        // history is converted to dynamic tool parts when those tools are no longer registered.
        var response = await SafeValidateUIMessagesInternalAsync(
                options,
                convertMissingTerminalToolsToDynamic: true
            )
            .ConfigureAwait(false);
        return response.Success ? response.Data! : throw response.Error!;
    }

    private static async Task<SafeValidateUIMessagesResult> SafeValidateUIMessagesInternalAsync(
        ValidateUIMessagesOptions options,
        bool convertMissingTerminalToolsToDynamic
    )
    {
        try
        {
            if (options.Messages == null)
            {
                return SafeValidateUIMessagesResult.Fail(
                    new InvalidArgumentException(
                        "messages",
                        null,
                        "messages parameter must be provided"
                    )
                );
            }

            if (options.Messages.Count == 0)
            {
                return SafeValidateUIMessagesResult.Fail(
                    new TypeValidationException(
                        ToJsonElement(options.Messages),
                        new Exception("Messages array must not be empty"),
                        null
                    )
                );
            }

            foreach (var message in options.Messages)
            {
                // Only an assistant message can be empty, as the last message of a chat that is waiting for a response.
                if (message.Role != UIMessageRole.Assistant && message.Parts.Count == 0)
                {
                    return SafeValidateUIMessagesResult.Fail(
                        new TypeValidationException(
                            ToJsonElement(options.Messages),
                            new Exception("Message must contain at least one part"),
                            null
                        )
                    );
                }
            }

            UIMessageRawInputWarning.WarnIfUIMessageHasDeprecatedRawInput(options.Messages);

            var validatedMessages = new List<UIMessage>(options.Messages.Count);
            var shouldValidateToolParts =
                options.Tools != null || convertMissingTerminalToolsToDynamic;
            for (var messageIndex = 0; messageIndex < options.Messages.Count; messageIndex++)
            {
                var message = options.Messages[messageIndex];
                var metadata = message.Metadata;
                if (options.MetadataSchema != null)
                {
                    metadata = ValidateOptional(
                        message.Metadata,
                        options.MetadataSchema,
                        new TypeValidationContext
                        {
                            Field = $"messages[{messageIndex}].metadata",
                            EntityId = message.Id,
                        }
                    );
                }

                var parts = new List<UIMessagePart>(message.Parts.Count);
                for (var partIndex = 0; partIndex < message.Parts.Count; partIndex++)
                {
                    var part = message.Parts[partIndex];
                    if (options.DataSchemas != null && part is DataUIPart dataPart)
                    {
                        if (!options.DataSchemas.TryGetValue(dataPart.Name, out var dataSchema))
                        {
                            return SafeValidateUIMessagesResult.Fail(
                                new TypeValidationException(
                                    dataPart.Data,
                                    new Exception(
                                        $"No data schema found for data part {dataPart.Name}"
                                    ),
                                    new TypeValidationContext
                                    {
                                        Field = $"messages[{messageIndex}].parts[{partIndex}].data",
                                        EntityName = dataPart.Name,
                                        EntityId = dataPart.Id,
                                    }
                                )
                            );
                        }

                        var data = JsonParsing.ValidateTypes(
                            dataPart.Data,
                            dataSchema,
                            new TypeValidationContext
                            {
                                Field = $"messages[{messageIndex}].parts[{partIndex}].data",
                                EntityName = dataPart.Name,
                                EntityId = dataPart.Id,
                            }
                        );
                        part = new DataUIPart(dataPart.Name, data) { Id = dataPart.Id };
                    }

                    if (shouldValidateToolParts && part is ToolUIPart { IsDynamic: false } toolPart)
                    {
                        var result = await ValidateToolPartAsync(
                                toolPart,
                                options,
                                convertMissingTerminalToolsToDynamic,
                                messageIndex,
                                partIndex
                            )
                            .ConfigureAwait(false);
                        if (result.Error != null)
                        {
                            return SafeValidateUIMessagesResult.Fail(result.Error);
                        }

                        part = result.Part!;
                    }

                    parts.Add(part);
                }

                validatedMessages.Add(
                    new UIMessage(message.Role, parts) { Id = message.Id, Metadata = metadata }
                );
            }

            return SafeValidateUIMessagesResult.Ok(validatedMessages);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return SafeValidateUIMessagesResult.Fail(error);
        }
    }

    private static async Task<(UIMessagePart? Part, Exception? Error)> ValidateToolPartAsync(
        ToolUIPart toolPart,
        ValidateUIMessagesOptions options,
        bool convertMissingTerminalToolsToDynamic,
        int messageIndex,
        int partIndex
    )
    {
        var toolName = toolPart.ToolName;
        Tool? tool = null;
        options.Tools?.TryGetValue(toolName, out tool);
        var isTerminal =
            toolPart.State
            is ToolUIPartState.OutputAvailable
                or ToolUIPartState.OutputError
                or ToolUIPartState.OutputDenied;

        if (tool == null && isTerminal)
        {
            // Persisted terminal history can reference tools that are no longer registered. Normalize those parts so
            // callers do not receive unvalidated values under current static tool types.
            return options.Tools != null || convertMissingTerminalToolsToDynamic
                ? (UnavailableTool.MarkToolPartAsUnavailable(toolPart), null)
                : (toolPart, null);
        }

        if (tool == null)
        {
            return (
                null,
                new TypeValidationException(
                    toolPart.Input,
                    new Exception($"No tool schema found for tool part {toolName}"),
                    new TypeValidationContext
                    {
                        Field = $"messages[{messageIndex}].parts[{partIndex}].input",
                        EntityName = toolName,
                        EntityId = toolPart.ToolCallId,
                    }
                )
            );
        }

        var inputContext = new TypeValidationContext
        {
            Field = $"messages[{messageIndex}].parts[{partIndex}].input",
            EntityName = toolName,
            EntityId = toolPart.ToolCallId,
        };
        var inputSchemaInput = toolPart.Approval?.InputSchemaInput;
        var inputToValidate = inputSchemaInput ?? toolPart.Input;
        var convertToDynamic = false;

        // Tool input validation
        if (
            toolPart.State != ToolUIPartState.InputStreaming
            && (
                toolPart.State != ToolUIPartState.OutputError
                || inputSchemaInput != null
                || toolPart.Input != null
            )
        )
        {
            Exception? inputError = null;
            JsonElement validatedInput = JsonSerializer.SerializeToElement<object?>(null);
            try
            {
                validatedInput = ValidateOptional(
                    inputToValidate,
                    ToSchema(tool.InputSchema),
                    inputContext
                ) ?? validatedInput;
            }
            catch (TypeValidationException error)
            {
                inputError = error;
            }

            if (inputError == null && inputSchemaInput != null)
            {
                try
                {
                    var refine = ExperimentalRefine(options, toolName);
                    var reconstructedInput =
                        refine == null
                            ? validatedInput
                            : await refine(validatedInput).ConfigureAwait(false);
                    if (toolPart.Input is not { } input || !IsDeepEqual(reconstructedInput, input))
                    {
                        inputError = new TypeValidationException(
                            toolPart.Input,
                            new Exception(
                                "Tool input does not match the output reconstructed from inputSchemaInput."
                            ),
                            inputContext
                        );
                    }
                }
                catch (Exception error) when (error is not OperationCanceledException)
                {
                    inputError = new TypeValidationException(inputToValidate, error, inputContext);
                }
            }

            if (inputError != null)
            {
                // Failed calls and empty terminal inputs must remain loadable, but must not claim to match the current
                // static input type.
                if (
                    toolPart.State == ToolUIPartState.OutputError
                    || (
                        toolPart.State == ToolUIPartState.OutputAvailable
                        && IsEmptyObject(toolPart.Input)
                    )
                )
                {
                    convertToDynamic = true;
                }
                else
                {
                    return (null, inputError);
                }
            }
        }

        // Tool output validation
        if (
            toolPart.State == ToolUIPartState.OutputAvailable
            && tool.OutputSchema is { } outputSchema
            && ToSchema(outputSchema) is { } schema
        )
        {
            JsonParsing.ValidateTypes(
                toolPart.Output ?? default,
                schema,
                new TypeValidationContext
                {
                    Field = $"messages[{messageIndex}].parts[{partIndex}].output",
                    EntityName = toolName,
                    EntityId = toolPart.ToolCallId,
                }
            );
        }

        return (
            convertToDynamic ? UnavailableTool.MarkToolPartAsUnavailable(toolPart) : toolPart,
            null
        );
    }

    private static Func<JsonElement, Task<JsonElement>>? ExperimentalRefine(
        ValidateUIMessagesOptions options,
        string toolName
    )
    {
        Func<JsonElement, Task<JsonElement>>? refine = null;
        options.ExperimentalRefineToolInput?.TryGetValue(toolName, out refine);
        return refine;
    }

    private static JsonElement ToJsonElement(IReadOnlyList<UIMessage> messages)
    {
        var array = new JsonArray();
        foreach (var message in messages)
        {
            array.Add(UIMessageJson.ToJson(message));
        }

        return JsonSerializer.SerializeToElement(array);
    }

    private static JsonNode? ToSchema(JsonElement schema)
    {
        return schema.ValueKind == JsonValueKind.Undefined
            ? null
            : JsonNode.Parse(schema.GetRawText());
    }

    private static JsonElement? ValidateOptional(
        JsonElement? value,
        JsonNode? schema,
        TypeValidationContext context
    )
    {
        if (schema == null)
        {
            return value;
        }

        // An undefined value is validated as null, which a schema rejects unless it allows null.
        return JsonParsing.ValidateTypes(
            value ?? JsonSerializer.SerializeToElement<object?>(null),
            schema,
            context
        );
    }

    private static bool IsEmptyObject(JsonElement? value)
    {
        return value is { ValueKind: JsonValueKind.Object } element
            && !element.EnumerateObject().Any();
    }

    private static bool IsDeepEqual(JsonElement left, JsonElement right)
    {
        return JsonNode.DeepEquals(
            JsonNode.Parse(left.GetRawText()),
            JsonNode.Parse(right.GetRawText())
        );
    }
}
