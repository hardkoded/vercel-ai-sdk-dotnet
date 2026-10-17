// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>Argument schemas of the OpenAI provider-defined tools. Maps to the <c>*ArgsSchema</c> exports of <c>packages/openai/src/tool</c>.</summary>
internal static class OpenAIResponsesToolArgs
{
    private static readonly JsonNode CodeInterpreter = JsonNode.Parse("""
        {"type":"object","properties":{
          "container":{"anyOf":[
            {"type":"string"},
            {"type":"object","properties":{"fileIds":{"type":"array","items":{"type":"string"}}}}]}}}
        """)!;

    private static readonly JsonNode FileSearch = JsonNode.Parse("""
        {"type":"object","properties":{
          "vectorStoreIds":{"type":"array","items":{"type":"string"}},
          "maxNumResults":{"type":"number"},
          "ranking":{"type":"object","properties":{"ranker":{"type":"string"},"scoreThreshold":{"type":"number"}}},
          "filters":{}},
         "required":["vectorStoreIds"]}
        """)!;

    private static readonly JsonNode ImageGeneration = JsonNode.Parse("""
        {"type":"object","properties":{
          "action":{"enum":["generate","edit","auto"]},
          "background":{"enum":["auto","opaque","transparent"]},
          "inputFidelity":{"enum":["low","high"]},
          "inputImageMask":{"type":"object","properties":{"fileId":{"type":"string"},"imageUrl":{"type":"string"}}},
          "model":{"type":"string"},
          "moderation":{"enum":["auto","low"]},
          "outputCompression":{"type":"integer","minimum":0,"maximum":100},
          "outputFormat":{"enum":["png","jpeg","webp"]},
          "partialImages":{"type":"integer","minimum":0,"maximum":3},
          "quality":{"enum":["auto","low","medium","high","xhigh","max"]},
          "size":{"type":"string"}}}
        """)!;

    private static readonly string[] ImageGenerationKeys =
    {
        "action", "background", "inputFidelity", "inputImageMask", "model", "moderation", "outputCompression",
        "outputFormat", "partialImages", "quality", "size",
    };

    private static readonly Regex ImageSize = new(@"^[0-9]+x[0-9]+\z", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    private static readonly JsonNode Custom = JsonNode.Parse("""
        {"type":"object","properties":{
          "description":{"type":"string"},
          "async":{"type":"boolean"},
          "format":{"anyOf":[
            {"type":"object","properties":{
               "type":{"const":"grammar"},
               "syntax":{"enum":["regex","lark"]},
               "definition":{"type":"string"}},
             "required":["type","syntax","definition"]},
            {"type":"object","properties":{"type":{"const":"text"}},"required":["type"]}]}}}
        """)!;

    private static readonly JsonNode Mcp = JsonNode.Parse("""
        {"type":"object","properties":{
          "serverLabel":{"type":"string"},
          "allowedTools":{"anyOf":[
            {"type":"array","items":{"type":"string"}},
            {"type":"object","properties":{"readOnly":{"type":"boolean"},"toolNames":{"type":"array","items":{"type":"string"}}}}]},
          "authorization":{"type":"string"},
          "connectorId":{"type":"string"},
          "headers":{"type":"object","properties":{},"additionalProperties":{"type":"string"}},
          "requireApproval":{"anyOf":[
            {"enum":["always","never"]},
            {"type":"object","properties":{"never":{"type":"object","properties":{"toolNames":{"type":"array","items":{"type":"string"}}}}}}]},
          "serverDescription":{"type":"string"},
          "serverUrl":{"type":"string"}},
         "required":["serverLabel"]}
        """)!;

    private static readonly JsonNode Shell = JsonNode.Parse("""
        {"type":"object","properties":{
          "environment":{"anyOf":[
            {"type":"object","properties":{
               "type":{"const":"containerAuto"},
               "fileIds":{"type":"array","items":{"type":"string"}},
               "memoryLimit":{"enum":["1g","4g","16g","64g"]},
               "networkPolicy":{"anyOf":[
                 {"type":"object","properties":{"type":{"const":"disabled"}},"required":["type"]},
                 {"type":"object","properties":{
                    "type":{"const":"allowlist"},
                    "allowedDomains":{"type":"array","items":{"type":"string"}},
                    "domainSecrets":{"type":"array","items":{"type":"object","properties":{
                       "domain":{"type":"string"},"name":{"type":"string"},"value":{"type":"string"}},
                       "required":["domain","name","value"]}}},
                  "required":["type","allowedDomains"]}]},
               "skills":{"type":"array","items":{"anyOf":[
                 {"type":"object","properties":{
                    "type":{"const":"skillReference"},
                    "providerReference":{"type":"object","properties":{},"additionalProperties":{"type":"string"}},
                    "version":{"type":"string"}},
                  "required":["type","providerReference"]},
                 {"type":"object","properties":{
                    "type":{"const":"inline"},
                    "name":{"type":"string"},
                    "description":{"type":"string"},
                    "source":{"type":"object","properties":{
                       "type":{"const":"base64"},"mediaType":{"const":"application/zip"},"data":{"type":"string"}},
                       "required":["type","mediaType","data"]}},
                  "required":["type","name","description","source"]}]}}},
             "required":["type"]},
            {"type":"object","properties":{
               "type":{"const":"containerReference"},"containerId":{"type":"string"}},
             "required":["type","containerId"]},
            {"type":"object","properties":{
               "type":{"const":"local"},
               "skills":{"type":"array","items":{"type":"object","properties":{
                  "name":{"type":"string"},"description":{"type":"string"},"path":{"type":"string"}},
                  "required":["name","description","path"]}}}}]}}}
        """)!;

    private static readonly JsonNode ToolSearch = JsonNode.Parse("""
        {"type":"object","properties":{
          "execution":{"enum":["server","client"]},
          "description":{"type":"string"},
          "parameters":{"type":"object"}}}
        """)!;

    private static readonly JsonNode UserLocation = JsonNode.Parse("""
        {"type":"object","properties":{
          "type":{"const":"approximate"},
          "country":{"type":"string"},"city":{"type":"string"},"region":{"type":"string"},"timezone":{"type":"string"}},
         "required":["type"]}
        """)!;

    private static readonly JsonNode WebSearch = JsonNode.Parse("""
        {"type":"object","properties":{
          "externalWebAccess":{"type":"boolean"},
          "filters":{"type":"object","properties":{
             "allowedDomains":{"type":"array","items":{"type":"string"}},
             "blockedDomains":{"type":"array","items":{"type":"string"}}}},
          "searchContextSize":{"enum":["low","medium","high"]},
          "userLocation":{}}}
        """)!;

    private static readonly JsonNode WebSearchPreview = JsonNode.Parse("""
        {"type":"object","properties":{
          "searchContextSize":{"enum":["low","medium","high"]},
          "userLocation":{}}}
        """)!;

    static OpenAIResponsesToolArgs()
    {
        // The user location schema is shared. A node can have one parent, so each schema gets its own copy.
        WebSearch["properties"]!["userLocation"] = UserLocation.DeepClone();
        WebSearchPreview["properties"]!["userLocation"] = UserLocation.DeepClone();
    }

    /// <summary>
    /// Validates the arguments of a provider-defined tool and returns them without unknown properties.
    /// Returns null for a tool without an argument schema.
    /// </summary>
    /// <exception cref="TypeValidationException">The arguments do not match the schema of the tool.</exception>
    public static JsonObject? Validate(string providerToolId, JsonElement args)
    {
        var schema = SchemaFor(providerToolId);
        if (schema == null)
        {
            return null;
        }

        // Upstream always passes an object. An undefined value stands for no arguments.
        var value = args.ValueKind == JsonValueKind.Undefined ? JsonSerializer.Deserialize<JsonElement>("{}") : args;
        var validated = (JsonObject)JsonNode.Parse(JsonParsing.ValidateTypes(value, schema).GetRawText())!;
        switch (providerToolId)
        {
            case "openai.file_search":
                if (validated["filters"] is { } filters && !IsFilter(filters))
                {
                    throw new TypeValidationException("Invalid file search filters.", value);
                }

                break;
            case "openai.image_generation":
                // The upstream schema is strict: unknown properties are rejected.
                foreach (var property in value.EnumerateObject())
                {
                    if (Array.IndexOf(ImageGenerationKeys, property.Name) < 0)
                    {
                        throw new TypeValidationException("Unrecognized key \"" + property.Name + "\" in image_generation args.", value);
                    }
                }

                if (validated["size"]?.GetValue<string>() is { } size && size != "auto" && !ImageSize.IsMatch(size))
                {
                    throw new TypeValidationException("Invalid size \"" + size + "\". Expected auto or WIDTHxHEIGHT.", value);
                }

                break;
            case "openai.mcp":
                if (validated["serverUrl"] == null && validated["connectorId"] == null)
                {
                    throw new TypeValidationException("One of serverUrl or connectorId must be provided.", value);
                }

                break;
        }

        return validated;
    }

    private static JsonNode? SchemaFor(string providerToolId)
    {
        switch (providerToolId)
        {
            case "openai.code_interpreter": return CodeInterpreter;
            case "openai.file_search": return FileSearch;
            case "openai.image_generation": return ImageGeneration;
            case "openai.custom": return Custom;
            case "openai.mcp": return Mcp;
            case "openai.shell": return Shell;
            case "openai.tool_search": return ToolSearch;
            case "openai.web_search": return WebSearch;
            case "openai.web_search_preview": return WebSearchPreview;
            default: return null;
        }
    }

    private static bool IsFilter(JsonNode node) => node is JsonObject filter && (IsComparison(filter) || IsCompound(filter));

    private static bool IsComparison(JsonObject filter)
    {
        if (filter["key"] is not JsonValue key || !key.TryGetValue<string>(out _)
            || filter["type"] is not JsonValue type || !type.TryGetValue<string>(out var typeName)
            || Array.IndexOf(new[] { "eq", "ne", "gt", "gte", "lt", "lte", "in", "nin" }, typeName) < 0)
        {
            return false;
        }

        return filter["value"] switch
        {
            JsonArray items => items.All(item => item is JsonValue text && text.TryGetValue<string>(out _)),
            JsonValue value => value.TryGetValue<string>(out _) || value.TryGetValue<double>(out _) || value.TryGetValue<bool>(out _),
            _ => false,
        };
    }

    private static bool IsCompound(JsonObject filter)
    {
        return filter["type"] is JsonValue type
            && type.TryGetValue<string>(out var typeName)
            && (typeName == "and" || typeName == "or")
            && filter["filters"] is JsonArray filters
            && filters.All(child => child != null && IsFilter(child));
    }
}
