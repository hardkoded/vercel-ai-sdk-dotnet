// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.OpenAI;

/// <summary>The <c>openai</c> options of a tool. Maps to <c>OpenAIToolOptions</c> in <c>openai-responses-prepare-tools.ts</c>.</summary>
public sealed class OpenAIToolOptions
{
    /// <summary>Who may call the tool: <c>direct</c> and/or <c>programmatic</c>.</summary>
    public IReadOnlyList<string>? AllowedCallers { get; set; }

    /// <summary>Whether the model can continue generating after calling this tool without waiting for its result.</summary>
    public bool? Async { get; set; }

    /// <summary>Defers loading the tool.</summary>
    public bool? DeferLoading { get; set; }

    /// <summary>
    /// Omits this function from the Responses request's top-level tools array. Use for tools supplied in history
    /// through <c>additionalTools</c>. The tool stays active in the SDK so its calls can still be validated and executed.
    /// </summary>
    public bool? OmitFromRequestTools { get; set; }

    /// <summary>The JSON Schema of the tool output, an object.</summary>
    public JsonElement? OutputSchema { get; set; }

    /// <summary>The namespace the tool belongs to.</summary>
    public OpenAIToolNamespace? Namespace { get; set; }

    /// <summary>
    /// Returns the options as tool provider options under <paramref name="providerOptionsName"/> (<c>openai</c> by
    /// default), ready for <c>Tool.ProviderOptions</c>.
    /// </summary>
    public JsonElement ToProviderOptions(string providerOptionsName = "openai")
    {
        var json = new JsonObject();
        if (AllowedCallers != null)
        {
            var callers = new JsonArray();
            foreach (var caller in AllowedCallers)
            {
                callers.Add(caller);
            }

            json["allowedCallers"] = callers;
        }

        if (Async != null)
        {
            json["async"] = Async;
        }

        if (DeferLoading != null)
        {
            json["deferLoading"] = DeferLoading;
        }

        if (OmitFromRequestTools != null)
        {
            json["omitFromRequestTools"] = OmitFromRequestTools;
        }

        if (OutputSchema is { } outputSchema)
        {
            json["outputSchema"] = JsonNode.Parse(outputSchema.GetRawText());
        }

        if (Namespace != null)
        {
            json["namespace"] = new JsonObject
            {
                ["name"] = Namespace.Name,
                ["description"] = Namespace.Description,
            };
        }

        return JsonSerializer.SerializeToElement(new JsonObject { [providerOptionsName] = json });
    }
}

/// <summary>The namespace of an <see cref="OpenAIToolOptions"/>.</summary>
public sealed class OpenAIToolNamespace
{
    /// <summary>The namespace name.</summary>
    public string Name { get; set; } = "";

    /// <summary>The namespace description.</summary>
    public string Description { get; set; } = "";
}
