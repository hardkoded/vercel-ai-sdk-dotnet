// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

/// <summary>Generated text that carries the Responses output item id and its annotations.</summary>
public sealed class OpenAIText : GeneratedText
{
    /// <summary>Creates text.</summary>
    public OpenAIText(string text, IReadOnlyList<Citation>? citations, JsonElement providerMetadata)
        : base(text, citations)
    {
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Provider metadata: <c>openai.itemId</c>, <c>openai.phase</c> when the message has one, and, when the text is annotated, <c>openai.annotations</c>.</summary>
    public new JsonElement ProviderMetadata { get; }
}

/// <summary>
/// Maps Responses annotations, web-search sources, and file-search results. Retrieved references are sources.
/// Annotations are the citations that hang off the text they support.
/// </summary>
internal static class OpenAIResponsesOutput
{
    // JSON.stringify leaves characters such as '>' and non-ASCII text unescaped.
    private static readonly JsonSerializerOptions JsonOptions = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <summary>The source an annotation points at: a URL source for <c>url_citation</c>, else a document source.</summary>
    public static GeneratedContent AnnotationSource(JsonElement annotation, string id, string providerName = "openai")
    {
        var type = OpenAIJson.String(annotation, "type");
        if (type == "url_citation")
        {
            return new GeneratedSource(id, OpenAIJson.String(annotation, "url") ?? string.Empty, OpenAIJson.String(annotation, "title"));
        }

        var fileId = OpenAIJson.String(annotation, "file_id") ?? string.Empty;
        var filename = type == "file_path" ? fileId : OpenAIJson.String(annotation, "filename") ?? string.Empty;
        var metadata = new JsonObject { ["type"] = type, ["fileId"] = fileId };
        if (type == "container_file_citation")
        {
            metadata["containerId"] = OpenAIJson.String(annotation, "container_id");
        }
        else if (annotation.TryGetProperty("index", out var index) && index.ValueKind == JsonValueKind.Number)
        {
            metadata["index"] = JsonNode.Parse(index.GetRawText());
        }

        return new GeneratedDocumentSource(
            id,
            type == "file_path" ? "application/octet-stream" : "text/plain",
            filename,
            filename,
            OpenAIJson.ProviderMetadata(metadata, providerName));
    }

    /// <summary>The citations for a text block. A <c>file_path</c> annotation is a generated file, not a citation.</summary>
    public static IReadOnlyList<Citation>? Citations(IEnumerable<JsonElement> annotations)
    {
        var citations = new List<Citation>();
        foreach (var annotation in annotations)
        {
            var type = OpenAIJson.String(annotation, "type");
            if (type == "file_path")
            {
                continue;
            }

            var id = type == "url_citation" ? OpenAIJson.String(annotation, "url") : OpenAIJson.String(annotation, "file_id");
            var source = AnnotationSource(annotation, id ?? string.Empty);
            var start = Index(annotation, "start_index");
            var end = start == null ? null : Index(annotation, "end_index");
            citations.Add(source switch
            {
                GeneratedSource url => new Citation(url, start, end),
                _ => new Citation((GeneratedDocumentSource)source, start, end),
            });
        }

        return citations.Count == 0 ? null : citations;
    }

    /// <summary>The input of a <c>program</c> item: its code and replay fingerprint.</summary>
    public static string ProgramInput(JsonElement item) =>
        new JsonObject { ["code"] = OpenAIJson.String(item, "code"), ["fingerprint"] = OpenAIJson.String(item, "fingerprint") }.ToJsonString(JsonOptions);

    /// <summary>The result of a <c>program_output</c> item.</summary>
    public static string ProgramOutputResult(JsonElement item) =>
        new JsonObject { ["result"] = OpenAIJson.String(item, "result"), ["status"] = OpenAIJson.String(item, "status") }.ToJsonString(JsonOptions);

    /// <summary>The input of a <c>shell_call</c> item: its commands and the optional execution controls. Null controls are omitted.</summary>
    public static string ShellCallInput(JsonElement item)
    {
        var action = new JsonObject();
        var source = item.TryGetProperty("action", out var actionElement) && actionElement.ValueKind == JsonValueKind.Object ? actionElement : default;
        action["commands"] = source.ValueKind == JsonValueKind.Object && source.TryGetProperty("commands", out var commands)
            ? JsonNode.Parse(commands.GetRawText())
            : new JsonArray();
        if (source.ValueKind == JsonValueKind.Object && OpenAIJson.Int(source, "timeout_ms") is { } timeoutMs)
        {
            action["timeoutMs"] = timeoutMs;
        }

        if (source.ValueKind == JsonValueKind.Object && OpenAIJson.Int(source, "max_output_length") is { } maxOutputLength)
        {
            action["maxOutputLength"] = maxOutputLength;
        }

        return new JsonObject { ["action"] = action }.ToJsonString(JsonOptions);
    }

    /// <summary>The input of a <c>local_shell_call</c> item: the action with the fields of upstream's <c>localShellInputSchema</c> parse, in snake_case as the API returned them.</summary>
    public static string LocalShellCallInput(JsonElement item)
    {
        var action = new JsonObject();
        if (item.TryGetProperty("action", out var source) && source.ValueKind == JsonValueKind.Object)
        {
            foreach (var name in new[] { "type", "command", "timeout_ms", "user", "working_directory", "env" })
            {
                if (source.TryGetProperty(name, out var value))
                {
                    action[name] = JsonNode.Parse(value.GetRawText());
                }
            }
        }

        return new JsonObject { ["action"] = action }.ToJsonString(JsonOptions);
    }

    /// <summary>The input of a streamed <c>local_shell_call</c> item: an exec action with camelCase fields, as upstream's stream builds it.</summary>
    public static string LocalShellStreamCallInput(JsonElement item)
    {
        var source = item.TryGetProperty("action", out var actionElement) && actionElement.ValueKind == JsonValueKind.Object ? actionElement : default;
        var action = new JsonObject { ["type"] = "exec" };
        if (source.ValueKind == JsonValueKind.Object)
        {
            action["command"] = source.TryGetProperty("command", out var command) ? JsonNode.Parse(command.GetRawText()) : null;
            if (OpenAIJson.Int(source, "timeout_ms") is { } timeoutMs)
            {
                action["timeoutMs"] = timeoutMs;
            }

            if (OpenAIJson.String(source, "user") is { } user)
            {
                action["user"] = user;
            }

            if (OpenAIJson.String(source, "working_directory") is { } workingDirectory)
            {
                action["workingDirectory"] = workingDirectory;
            }

            if (source.TryGetProperty("env", out var env) && env.ValueKind == JsonValueKind.Object)
            {
                action["env"] = JsonNode.Parse(env.GetRawText());
            }
        }

        return new JsonObject { ["action"] = action }.ToJsonString(JsonOptions);
    }

    /// <summary>The result of a <c>shell_call_output</c> item, in the shape of upstream's <c>shellOutputSchema</c>.</summary>
    public static string ShellCallOutputResult(JsonElement item)
    {
        var result = new JsonObject();
        if (OpenAIJson.Int(item, "max_output_length") is { } maxOutputLength)
        {
            result["maxOutputLength"] = maxOutputLength;
        }

        var output = new JsonArray();
        if (item.TryGetProperty("output", out var entries) && entries.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in entries.EnumerateArray())
            {
                var outcome = entry.TryGetProperty("outcome", out var outcomeElement) ? outcomeElement : default;
                output.Add(new JsonObject
                {
                    ["stdout"] = OpenAIJson.String(entry, "stdout"),
                    ["stderr"] = OpenAIJson.String(entry, "stderr"),
                    ["outcome"] = outcome.ValueKind == JsonValueKind.Object && OpenAIJson.String(outcome, "type") == "exit"
                        ? new JsonObject { ["type"] = "exit", ["exitCode"] = OpenAIJson.Int(outcome, "exit_code") }
                        : new JsonObject { ["type"] = "timeout" },
                });
            }
        }

        result["output"] = output;
        return result.ToJsonString(JsonOptions);
    }

    /// <summary>The input of an <c>apply_patch_call</c> item: its call id and operation.</summary>
    public static string ApplyPatchInput(JsonElement item) =>
        new JsonObject
        {
            ["callId"] = OpenAIJson.String(item, "call_id"),
            ["operation"] = item.TryGetProperty("operation", out var operation) ? JsonNode.Parse(operation.GetRawText()) : null,
        }.ToJsonString(JsonOptions);

    /// <summary>Provider metadata that carries only the output item id.</summary>
    public static JsonElement ItemMetadata(JsonElement item, string providerName = "openai") =>
        OpenAIJson.ProviderMetadata(new JsonObject { ["itemId"] = OpenAIJson.String(item, "id") }, providerName);

    /// <summary>The tool name of an <c>mcp_call</c> item: <c>mcp.</c> and the name of the MCP tool.</summary>
    public static string McpToolName(JsonElement item) => "mcp." + OpenAIJson.String(item, "name");

    /// <summary>The result an <c>mcp_call</c> reports, in the shape of upstream's <c>mcpOutputSchema</c> call result.</summary>
    public static string McpCallResult(JsonElement item)
    {
        var result = new JsonObject
        {
            ["type"] = "call",
            ["serverLabel"] = OpenAIJson.String(item, "server_label"),
            ["name"] = OpenAIJson.String(item, "name"),
            ["arguments"] = OpenAIJson.String(item, "arguments"),
        };
        if (item.TryGetProperty("output", out var output) && output.ValueKind != JsonValueKind.Null)
        {
            result["output"] = JsonNode.Parse(output.GetRawText());
        }

        if (item.TryGetProperty("error", out var error) && error.ValueKind != JsonValueKind.Null)
        {
            result["error"] = JsonNode.Parse(error.GetRawText());
        }

        return result.ToJsonString();
    }

    /// <summary>Provider metadata for a text block.</summary>
    public static JsonElement TextMetadata(string? itemId, IReadOnlyList<JsonElement> annotations, string? phase = null, string providerName = "openai")
    {
        var metadata = new JsonObject { ["itemId"] = itemId };
        if (phase != null)
        {
            metadata["phase"] = phase;
        }

        if (annotations.Count > 0)
        {
            var array = new JsonArray();
            foreach (var annotation in annotations)
            {
                array.Add(JsonNode.Parse(annotation.GetRawText()));
            }

            metadata["annotations"] = array;
        }

        return OpenAIJson.ProviderMetadata(metadata, providerName);
    }

    /// <summary>The URL sources of a <c>web_search_call</c> search action.</summary>
    public static List<string> WebSearchUrls(JsonElement item)
    {
        var urls = new List<string>();
        if (item.TryGetProperty("action", out var action)
            && action.ValueKind == JsonValueKind.Object
            && OpenAIJson.String(action, "type") == "search"
            && action.TryGetProperty("sources", out var sources)
            && sources.ValueKind == JsonValueKind.Array)
        {
            foreach (var source in sources.EnumerateArray())
            {
                if (OpenAIJson.String(source, "type") == "url" && OpenAIJson.String(source, "url") is { } url)
                {
                    urls.Add(url);
                }
            }
        }

        return urls;
    }

    /// <summary>The result a <c>web_search_call</c> reports.</summary>
    public static string WebSearchResult(JsonElement item)
    {
        if (!item.TryGetProperty("action", out var action) || action.ValueKind != JsonValueKind.Object)
        {
            return "{}";
        }

        var result = new JsonObject();
        switch (OpenAIJson.String(action, "type"))
        {
            case "search":
                var search = new JsonObject { ["type"] = "search" };
                if (OpenAIJson.String(action, "query") is { } query)
                {
                    search["query"] = query;
                }

                if (action.TryGetProperty("queries", out var queries) && queries.ValueKind == JsonValueKind.Array)
                {
                    search["queries"] = JsonNode.Parse(queries.GetRawText());
                }

                result["action"] = search;
                if (action.TryGetProperty("sources", out var sources) && sources.ValueKind == JsonValueKind.Array)
                {
                    result["sources"] = JsonNode.Parse(sources.GetRawText());
                }

                break;
            case "open_page":
                result["action"] = new JsonObject { ["type"] = "openPage", ["url"] = OpenAIJson.String(action, "url") };
                break;
            case "find_in_page":
                result["action"] = new JsonObject
                {
                    ["type"] = "findInPage",
                    ["url"] = OpenAIJson.String(action, "url"),
                    ["pattern"] = OpenAIJson.String(action, "pattern"),
                };
                break;
        }

        return result.ToJsonString();
    }

    /// <summary>The result a <c>file_search_call</c> reports.</summary>
    public static string FileSearchResult(JsonElement item)
    {
        var queries = item.TryGetProperty("queries", out var queryArray) && queryArray.ValueKind == JsonValueKind.Array
            ? JsonNode.Parse(queryArray.GetRawText())
            : new JsonArray();
        JsonNode? results = null;
        if (item.TryGetProperty("results", out var resultArray) && resultArray.ValueKind == JsonValueKind.Array)
        {
            var mapped = new JsonArray();
            foreach (var result in resultArray.EnumerateArray())
            {
                mapped.Add(new JsonObject
                {
                    ["attributes"] = result.TryGetProperty("attributes", out var attributes) ? JsonNode.Parse(attributes.GetRawText()) : new JsonObject(),
                    ["fileId"] = OpenAIJson.String(result, "file_id"),
                    ["filename"] = OpenAIJson.String(result, "filename"),
                    ["score"] = result.TryGetProperty("score", out var score) ? JsonNode.Parse(score.GetRawText()) : null,
                    ["text"] = OpenAIJson.String(result, "text"),
                });
            }

            results = mapped;
        }

        return new JsonObject { ["queries"] = queries, ["results"] = results }.ToJsonString();
    }

    /// <summary>The retrieved files of a <c>file_search_call</c>, one per file id.</summary>
    public static IEnumerable<(string FileId, string Filename)> RetrievedFiles(JsonElement item)
    {
        if (item.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
        {
            foreach (var result in results.EnumerateArray())
            {
                yield return (OpenAIJson.String(result, "file_id") ?? string.Empty, OpenAIJson.String(result, "filename") ?? string.Empty);
            }
        }
    }

    /// <summary>Provider metadata of a retrieved file source.</summary>
    public static JsonElement FileSearchMetadata(string fileId, string providerName = "openai") =>
        OpenAIJson.ProviderMetadata(new JsonObject { ["type"] = "file_search", ["fileId"] = fileId }, providerName);

    public static bool HasResults(JsonElement item) =>
        item.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array && results.GetArrayLength() > 0;

    private static int? Index(JsonElement annotation, string name) =>
        annotation.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number
            : null;
}
