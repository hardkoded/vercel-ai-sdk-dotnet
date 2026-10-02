// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Google;

/// <summary>One streamed function-call argument fragment.</summary>
public sealed class GooglePartialArgument
{
    /// <summary>JSONPath such as <c>$.recipe.ingredients[0].name</c>.</summary>
    public string JsonPath { get; set; } = string.Empty;

    /// <summary>String fragment, when this chunk carries one.</summary>
    public string? StringValue { get; set; }

    /// <summary>True when <see cref="StringValue"/> was supplied, including an empty string.</summary>
    public bool HasString { get; set; }

    /// <summary>Number fragment.</summary>
    public double? NumberValue { get; set; }

    /// <summary>Boolean fragment.</summary>
    public bool? BoolValue { get; set; }

    /// <summary>True when the fragment is JSON null.</summary>
    public bool HasNull { get; set; }

    /// <summary>True when a string value will be continued by a later chunk.</summary>
    public bool? WillContinue { get; set; }
}

/// <summary>The object and text produced by one <see cref="GoogleJsonAccumulator"/> update.</summary>
public sealed class GooglePartialArgumentsUpdate
{
    /// <summary>Creates an update.</summary>
    public GooglePartialArgumentsUpdate(JsonObject current, string textDelta)
    {
        Current = current;
        TextDelta = textDelta ?? string.Empty;
    }

    /// <summary>Arguments accumulated so far.</summary>
    public JsonObject Current { get; }

    /// <summary>JSON text emitted for this chunk.</summary>
    public string TextDelta { get; }
}

/// <summary>The completed argument JSON and the suffix still needed to close it.</summary>
public sealed class GooglePartialArgumentsFinal
{
    /// <summary>Creates a finalization result.</summary>
    public GooglePartialArgumentsFinal(string finalJson, string closingDelta)
    {
        FinalJson = finalJson ?? "{}";
        ClosingDelta = closingDelta ?? string.Empty;
    }

    /// <summary>Complete JSON object.</summary>
    public string FinalJson { get; }

    /// <summary>Characters that close the streamed text.</summary>
    public string ClosingDelta { get; }
}

/// <summary>
/// Builds a JSON object from Gemini streaming <c>partialArgs</c> chunks.
/// Concatenated text deltas plus <see cref="Finalize"/> match the serialized object.
/// Keys such as <c>__proto__</c> are stored as ordinary properties.
/// </summary>
public sealed class GoogleJsonAccumulator
{
    private readonly Bag _root = Bag.Object();
    private readonly List<StackEntry> _stack = new();
    private string _jsonText = string.Empty;
    private bool _stringOpen;

    /// <summary>Applies one batch of partial arguments.</summary>
    public GooglePartialArgumentsUpdate Process(IReadOnlyList<GooglePartialArgument>? partialArgs)
    {
        var delta = new StringBuilder();
        if (partialArgs != null)
        {
            for (var i = 0; i < partialArgs.Count; i++)
            {
                delta.Append(Apply(partialArgs[i]));
            }
        }

        var text = delta.ToString();
        _jsonText += text;
        return new GooglePartialArgumentsUpdate(Snapshot(), text);
    }

    /// <summary>Closes any open containers and returns the suffix required to finish the JSON text.</summary>
    public GooglePartialArgumentsFinal Finalize()
    {
        var finalJson = Write(_root);
        var closing = finalJson.Length >= _jsonText.Length ? finalJson.Substring(_jsonText.Length) : string.Empty;
        return new GooglePartialArgumentsFinal(finalJson, closing);
    }

    private JsonObject Snapshot()
    {
        return JsonNode.Parse(Write(_root)) as JsonObject ?? new JsonObject();
    }

    private string Apply(GooglePartialArgument arg)
    {
        var rawPath = arg.JsonPath ?? string.Empty;
        if (rawPath.StartsWith("$.", StringComparison.Ordinal))
        {
            rawPath = rawPath.Substring(2);
        }

        if (rawPath.Length == 0)
        {
            return string.Empty;
        }

        var segments = ParsePath(rawPath);
        if (segments.Count == 0)
        {
            return string.Empty;
        }

        var existing = Get(_root, segments);
        if (arg.HasString && existing is { Kind: BagKind.String })
        {
            existing.String = (existing.String ?? string.Empty) + (arg.StringValue ?? string.Empty);
            return EscapeFragment(arg.StringValue ?? string.Empty);
        }

        if (!TryResolve(arg, out var value, out var valueJson))
        {
            return string.Empty;
        }

        Set(_root, segments, value);
        return Emit(segments, arg, valueJson);
    }

    private string Emit(IReadOnlyList<Segment> segments, GooglePartialArgument arg, string valueJson)
    {
        var fragment = string.Empty;
        if (_stringOpen)
        {
            fragment += "\"";
            _stringOpen = false;
        }

        if (_stack.Count == 0)
        {
            _stack.Add(new StackEntry(string.Empty, false));
            fragment += "{";
        }

        var containers = new List<Segment>();
        for (var i = 0; i < segments.Count - 1; i++)
        {
            containers.Add(segments[i]);
        }

        var leaf = segments[segments.Count - 1];
        fragment += CloseDownTo(CommonDepth(containers));
        fragment += OpenDownTo(containers, leaf);
        fragment += EmitLeaf(leaf, arg, valueJson);
        return fragment;
    }

    private int CommonDepth(IReadOnlyList<Segment> containers)
    {
        var max = Math.Min(_stack.Count - 1, containers.Count);
        var common = 0;
        for (var i = 0; i < max; i++)
        {
            if (_stack[i + 1].Segment == containers[i].Key)
            {
                common++;
            }
            else
            {
                break;
            }
        }

        return common + 1;
    }

    private string CloseDownTo(int targetDepth)
    {
        var fragment = string.Empty;
        while (_stack.Count > targetDepth)
        {
            var entry = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            fragment += entry.IsArray ? "]" : "}";
        }

        return fragment;
    }

    private string OpenDownTo(IReadOnlyList<Segment> containers, Segment leaf)
    {
        var fragment = string.Empty;
        for (var i = _stack.Count - 1; i < containers.Count; i++)
        {
            var segment = containers[i];
            var parent = _stack[_stack.Count - 1];
            if (parent.ChildCount > 0)
            {
                fragment += ",";
            }

            parent.ChildCount++;
            if (segment.Name != null)
            {
                fragment += JsonSerializer.Serialize(segment.Name, GoogleJson.Options) + ":";
            }

            var child = i + 1 < containers.Count ? containers[i + 1] : leaf;
            var isArray = child.Index != null;
            fragment += isArray ? "[" : "{";
            _stack.Add(new StackEntry(segment.Key, isArray));
        }

        return fragment;
    }

    private string EmitLeaf(Segment leaf, GooglePartialArgument arg, string valueJson)
    {
        var fragment = string.Empty;
        var container = _stack[_stack.Count - 1];
        if (container.ChildCount > 0)
        {
            fragment += ",";
        }

        container.ChildCount++;
        if (leaf.Name != null)
        {
            fragment += JsonSerializer.Serialize(leaf.Name, GoogleJson.Options) + ":";
        }

        if (arg.HasString && arg.WillContinue == true)
        {
            fragment += valueJson.Substring(0, valueJson.Length - 1);
            _stringOpen = true;
        }
        else
        {
            fragment += valueJson;
        }

        return fragment;
    }

    private static bool TryResolve(GooglePartialArgument arg, out Bag value, out string json)
    {
        if (arg.HasString)
        {
            value = Bag.FromString(arg.StringValue ?? string.Empty);
            json = JsonSerializer.Serialize(arg.StringValue ?? string.Empty, GoogleJson.Options);
            return true;
        }

        if (arg.NumberValue is { } number)
        {
            value = Bag.FromNumber(number);
            json = value.NumberJson;
            return true;
        }

        if (arg.BoolValue is { } flag)
        {
            value = Bag.FromBool(flag);
            json = flag ? "true" : "false";
            return true;
        }

        if (arg.HasNull)
        {
            value = Bag.Null();
            json = "null";
            return true;
        }

        value = Bag.Null();
        json = string.Empty;
        return false;
    }

    private static string EscapeFragment(string value)
    {
        var json = JsonSerializer.Serialize(value, GoogleJson.Options);
        return json.Length >= 2 ? json.Substring(1, json.Length - 2) : string.Empty;
    }

    private static List<Segment> ParsePath(string rawPath)
    {
        var segments = new List<Segment>();
        var parts = rawPath.Split('.');
        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.Length == 0)
            {
                continue;
            }

            var bracket = part.IndexOf('[');
            if (bracket < 0)
            {
                segments.Add(Segment.Property(part));
                continue;
            }

            if (bracket > 0)
            {
                segments.Add(Segment.Property(part.Substring(0, bracket)));
            }

            var cursor = bracket;
            while (cursor < part.Length)
            {
                var open = part.IndexOf('[', cursor);
                if (open < 0)
                {
                    break;
                }

                var close = part.IndexOf(']', open + 1);
                if (close < 0)
                {
                    break;
                }

                if (int.TryParse(part.Substring(open + 1, close - open - 1), NumberStyles.None, CultureInfo.InvariantCulture, out var index))
                {
                    segments.Add(Segment.At(index));
                }

                cursor = close + 1;
            }
        }

        return segments;
    }

    private static Bag? Get(Bag root, IReadOnlyList<Segment> segments)
    {
        Bag? current = root;
        for (var i = 0; i < segments.Count; i++)
        {
            if (current == null)
            {
                return null;
            }

            current = current.Child(segments[i]);
        }

        return current;
    }

    private static void Set(Bag root, IReadOnlyList<Segment> segments, Bag value)
    {
        var current = root;
        for (var i = 0; i < segments.Count - 1; i++)
        {
            current = current.Ensure(segments[i], segments[i + 1].Index != null);
        }

        current.Put(segments[segments.Count - 1], value);
    }

    private static string Write(Bag bag)
    {
        var builder = new StringBuilder();
        bag.Write(builder);
        return builder.ToString();
    }

    private enum BagKind
    {
        Object,
        Array,
        String,
        Number,
        Bool,
        Null,
    }

    private sealed class Bag
    {
        private readonly List<string> _order = new();
        private readonly Dictionary<string, Bag> _props = new(StringComparer.Ordinal);
        private readonly List<Bag?> _items = new();

        public BagKind Kind { get; private set; }

        public string? String { get; set; }

        public string NumberJson { get; private set; } = "0";

        public bool Bool { get; private set; }

        public static Bag Object()
        {
            return new Bag { Kind = BagKind.Object };
        }

        public static Bag Array()
        {
            return new Bag { Kind = BagKind.Array };
        }

        public static Bag FromString(string value)
        {
            return new Bag { Kind = BagKind.String, String = value };
        }

        public static Bag FromNumber(double number)
        {
            string json;
            if (number == Math.Truncate(number) && number <= int.MaxValue && number >= int.MinValue)
            {
                json = ((int)number).ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                json = JsonSerializer.Serialize(number, GoogleJson.Options);
            }

            return new Bag { Kind = BagKind.Number, NumberJson = json };
        }

        public static Bag FromBool(bool value)
        {
            return new Bag { Kind = BagKind.Bool, Bool = value };
        }

        public static Bag Null()
        {
            return new Bag { Kind = BagKind.Null };
        }

        public Bag? Child(Segment segment)
        {
            if (segment.Name != null)
            {
                return _props.TryGetValue(segment.Name, out var child) ? child : null;
            }

            var index = segment.Index ?? -1;
            return index >= 0 && index < _items.Count ? _items[index] : null;
        }

        public Bag Ensure(Segment segment, bool nextIsArray)
        {
            var existing = Child(segment);
            if (existing != null && (existing.Kind == BagKind.Object || existing.Kind == BagKind.Array))
            {
                return existing;
            }

            var created = nextIsArray ? Array() : Object();
            Put(segment, created);
            return created;
        }

        public void Put(Segment segment, Bag value)
        {
            if (segment.Name != null)
            {
                if (!_props.ContainsKey(segment.Name))
                {
                    _order.Add(segment.Name);
                }

                _props[segment.Name] = value;
                return;
            }

            var index = segment.Index ?? 0;
            while (_items.Count <= index)
            {
                _items.Add(null);
            }

            _items[index] = value;
        }

        public void Write(StringBuilder builder)
        {
            switch (Kind)
            {
                case BagKind.Object:
                    builder.Append('{');
                    for (var i = 0; i < _order.Count; i++)
                    {
                        if (i > 0)
                        {
                            builder.Append(',');
                        }

                        var name = _order[i];
                        builder.Append(JsonSerializer.Serialize(name, GoogleJson.Options));
                        builder.Append(':');
                        _props[name].Write(builder);
                    }

                    builder.Append('}');
                    break;
                case BagKind.Array:
                    builder.Append('[');
                    for (var i = 0; i < _items.Count; i++)
                    {
                        if (i > 0)
                        {
                            builder.Append(',');
                        }

                        if (_items[i] == null)
                        {
                            builder.Append("null");
                        }
                        else
                        {
                            _items[i]!.Write(builder);
                        }
                    }

                    builder.Append(']');
                    break;
                case BagKind.String:
                    builder.Append(JsonSerializer.Serialize(String ?? string.Empty, GoogleJson.Options));
                    break;
                case BagKind.Number:
                    builder.Append(NumberJson);
                    break;
                case BagKind.Bool:
                    builder.Append(Bool ? "true" : "false");
                    break;
                default:
                    builder.Append("null");
                    break;
            }
        }
    }

    private sealed class StackEntry
    {
        public StackEntry(string segment, bool isArray)
        {
            Segment = segment;
            IsArray = isArray;
        }

        public string Segment { get; }

        public bool IsArray { get; }

        public int ChildCount { get; set; }
    }

    private readonly struct Segment
    {
        private Segment(string? name, int? index)
        {
            Name = name;
            Index = index;
        }

        public string? Name { get; }

        public int? Index { get; }

        public string Key
        {
            get { return Name ?? (Index?.ToString(CultureInfo.InvariantCulture) ?? string.Empty); }
        }

        public static Segment Property(string name)
        {
            return new Segment(name, null);
        }

        public static Segment At(int index)
        {
            return new Segment(null, index);
        }
    }
}
