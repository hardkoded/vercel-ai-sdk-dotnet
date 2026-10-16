// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Operations;

/// <summary>
/// Content for decision instructions and criteria descriptions: a string, a JSON object, or a JSON array.
/// Maps to <c>DecisionModelV4Input</c>.
/// </summary>
public sealed class DecisionInput
{
    private DecisionInput(JsonElement value)
    {
        Value = value;
    }

    /// <summary>The JSON value. It is a string, an object, or an array.</summary>
    public JsonElement Value { get; }

    /// <summary>Creates an input from a string.</summary>
    public static DecisionInput From(string text)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(text));
        return new DecisionInput(document.RootElement.Clone());
    }

    /// <summary>Creates an input from a JSON string, object, or array. Other JSON values throw <see cref="InvalidArgumentException"/>.</summary>
    public static DecisionInput FromJson(JsonElement value)
    {
        if (value.ValueKind is not (JsonValueKind.String or JsonValueKind.Object or JsonValueKind.Array))
        {
            throw new InvalidArgumentException("input", value.ValueKind == JsonValueKind.Undefined ? null : value.GetRawText(), "must be a JSON-compatible string, object, or array");
        }

        return new DecisionInput(value.Clone());
    }

    /// <summary>Parses JSON text into an input. The text must be a JSON string, object, or array.</summary>
    public static DecisionInput Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return FromJson(document.RootElement);
    }

    /// <summary>Converts a string to an input.</summary>
    public static implicit operator DecisionInput(string text)
    {
        return From(text);
    }

    /// <summary>The string itself, or the JSON text of an object or array.</summary>
    public override string ToString()
    {
        return Value.ValueKind == JsonValueKind.String ? Value.GetString()! : Value.GetRawText();
    }

    internal JsonNode? ToNode()
    {
        return JsonNode.Parse(Value.GetRawText());
    }
}

/// <summary>Criteria of a decision question. Maps to the <c>criteria</c> member of <c>DecisionModelV4Question</c>.</summary>
public abstract class DecisionCriteria
{
    private protected DecisionCriteria()
    {
    }

    /// <summary>Number of options or levels. A boolean has none.</summary>
    public virtual int Count => 0;

    internal abstract JsonNode ToNode();

    internal static JsonNode? Node(DecisionInput? input)
    {
        return input?.ToNode();
    }
}

/// <summary>Criteria of a choice question: a nonempty map of option names to descriptions. A null description means none.</summary>
public sealed class DecisionChoiceCriteria : DecisionCriteria
{
    /// <summary>Creates choice criteria.</summary>
    public DecisionChoiceCriteria(IReadOnlyDictionary<string, DecisionInput?> options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>Option names with their descriptions, in order.</summary>
    public IReadOnlyDictionary<string, DecisionInput?> Options { get; }

    /// <inheritdoc />
    public override int Count => Options.Count;

    internal override JsonNode ToNode()
    {
        var node = new JsonObject();
        foreach (var option in Options)
        {
            node[option.Key] = Node(option.Value);
        }

        return node;
    }
}

/// <summary>Criteria of a score question: at least two ordered levels, indexed from zero. A null description means none.</summary>
public sealed class DecisionScoreCriteria : DecisionCriteria
{
    /// <summary>Creates score criteria.</summary>
    public DecisionScoreCriteria(IReadOnlyList<DecisionInput?> levels)
    {
        Levels = levels ?? throw new ArgumentNullException(nameof(levels));
    }

    /// <summary>Level descriptions, lowest first.</summary>
    public IReadOnlyList<DecisionInput?> Levels { get; }

    /// <inheritdoc />
    public override int Count => Levels.Count;

    internal override JsonNode ToNode()
    {
        var node = new JsonArray();
        foreach (var level in Levels)
        {
            node.Add(Node(level));
        }

        return node;
    }
}

/// <summary>
/// Optional descriptions of the true and false outcomes of a boolean question. A side that is never set is left out of the wire
/// request. A side set to null is sent as null.
/// </summary>
public sealed class DecisionBooleanCriteria : DecisionCriteria
{
    private DecisionInput? _true;
    private DecisionInput? _false;

    /// <summary>Description of the true outcome.</summary>
    public DecisionInput? True
    {
        get => _true;
        init
        {
            _true = value;
            HasTrue = true;
        }
    }

    /// <summary>Description of the false outcome.</summary>
    public DecisionInput? False
    {
        get => _false;
        init
        {
            _false = value;
            HasFalse = true;
        }
    }

    /// <summary>True when <see cref="True"/> was set, including to null.</summary>
    public bool HasTrue { get; private set; }

    /// <summary>True when <see cref="False"/> was set, including to null.</summary>
    public bool HasFalse { get; private set; }

    internal override JsonNode ToNode()
    {
        var node = new JsonObject();
        if (HasTrue)
        {
            node["true"] = Node(True);
        }

        if (HasFalse)
        {
            node["false"] = Node(False);
        }

        return node;
    }
}
