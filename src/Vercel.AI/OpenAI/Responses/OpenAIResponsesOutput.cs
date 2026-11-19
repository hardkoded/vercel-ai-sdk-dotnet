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
/// Maps typed Responses annotations, web-search sources, and file-search results. Retrieved references are sources.
/// Annotations are the citations that hang off the text they support.
/// </summary>
internal static class OpenAIResponsesOutput
{
    // JSON.stringify leaves characters such as '>' and non-ASCII text unescaped.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>The source an annotation points at: a URL source for <c>url_citation</c>, else a document source.</summary>
    public static GeneratedContent AnnotationSource(
        OpenAIResponsesApi.Annotation annotation,
        string id,
        string providerName = "openai"
    )
    {
        if (annotation is OpenAIResponsesApi.UrlCitation url)
        {
            return new GeneratedSource(id, url.Url, url.Title);
        }

        var isFilePath = annotation is OpenAIResponsesApi.FilePath;
        var fileId = AnnotationFileId(annotation);
        var filename = annotation switch
        {
            OpenAIResponsesApi.FileCitation file => file.Filename,
            OpenAIResponsesApi.ContainerFileCitation container => container.Filename,
            _ => fileId,
        };
        var metadata = new JsonObject
        {
            ["type"] = AnnotationType(annotation),
            ["fileId"] = fileId,
        };
        switch (annotation)
        {
            case OpenAIResponsesApi.ContainerFileCitation container:
                metadata["containerId"] = container.ContainerId;
                break;
            case OpenAIResponsesApi.FileCitation file:
                metadata["index"] = file.Index;
                break;
            case OpenAIResponsesApi.FilePath path:
                metadata["index"] = path.Index;
                break;
        }

        return new GeneratedDocumentSource(
            id,
            isFilePath ? "application/octet-stream" : "text/plain",
            filename,
            filename,
            OpenAIJson.ProviderMetadata(metadata, providerName)
        );
    }

    /// <summary>The annotation as the API returned it, for provider metadata.</summary>
    public static JsonObject AnnotationJson(OpenAIResponsesApi.Annotation annotation) =>
        annotation switch
        {
            OpenAIResponsesApi.UrlCitation url => new JsonObject
            {
                ["type"] = "url_citation",
                ["start_index"] = url.StartIndex,
                ["end_index"] = url.EndIndex,
                ["url"] = url.Url,
                ["title"] = url.Title,
            },
            OpenAIResponsesApi.FileCitation file => new JsonObject
            {
                ["type"] = "file_citation",
                ["file_id"] = file.FileId,
                ["filename"] = file.Filename,
                ["index"] = file.Index,
            },
            OpenAIResponsesApi.ContainerFileCitation container => new JsonObject
            {
                ["type"] = "container_file_citation",
                ["container_id"] = container.ContainerId,
                ["file_id"] = container.FileId,
                ["filename"] = container.Filename,
                ["start_index"] = container.StartIndex,
                ["end_index"] = container.EndIndex,
            },
            OpenAIResponsesApi.FilePath path => new JsonObject
            {
                ["type"] = "file_path",
                ["file_id"] = path.FileId,
                ["index"] = path.Index,
            },
            _ => throw new InvalidOperationException("Unknown annotation."),
        };

    /// <summary>The annotation type.</summary>
    public static string AnnotationType(OpenAIResponsesApi.Annotation annotation) =>
        annotation switch
        {
            OpenAIResponsesApi.UrlCitation => "url_citation",
            OpenAIResponsesApi.FileCitation => "file_citation",
            OpenAIResponsesApi.ContainerFileCitation => "container_file_citation",
            _ => "file_path",
        };

    /// <summary>The citations for a text block. A <c>file_path</c> annotation is a generated file, not a citation.</summary>
    public static IReadOnlyList<Citation>? Citations(
        IEnumerable<OpenAIResponsesApi.Annotation> annotations
    )
    {
        var citations = new List<Citation>();
        foreach (var annotation in annotations)
        {
            if (annotation is OpenAIResponsesApi.FilePath)
            {
                continue;
            }

            var id = annotation is OpenAIResponsesApi.UrlCitation url
                ? url.Url
                : AnnotationFileId(annotation);
            var source = AnnotationSource(annotation, id);
            int? start = null;
            int? end = null;
            switch (annotation)
            {
                case OpenAIResponsesApi.UrlCitation urlCitation:
                    start = urlCitation.StartIndex;
                    end = urlCitation.EndIndex;
                    break;
                case OpenAIResponsesApi.ContainerFileCitation container:
                    start = container.StartIndex;
                    end = container.EndIndex;
                    break;
            }

            citations.Add(
                source switch
                {
                    GeneratedSource sourceUrl => new Citation(sourceUrl, start, end),
                    _ => new Citation((GeneratedDocumentSource)source, start, end),
                }
            );
        }

        return citations.Count == 0 ? null : citations;
    }

    /// <summary>The input of a <c>program</c> item: its code and replay fingerprint.</summary>
    public static string ProgramInput(OpenAIResponsesApi.Program item) =>
        new JsonObject { ["code"] = item.Code, ["fingerprint"] = item.Fingerprint }.ToJsonString(
            JsonOptions
        );

    /// <summary>The result of a <c>program_output</c> item.</summary>
    public static string ProgramOutputResult(OpenAIResponsesApi.ProgramOutput item) =>
        new JsonObject { ["result"] = item.Result, ["status"] = item.Status }.ToJsonString(
            JsonOptions
        );

    /// <summary>The input of a <c>shell_call</c> item: its commands and the optional execution controls. Null controls are omitted.</summary>
    public static string ShellCallInput(OpenAIResponsesApi.ShellCall item)
    {
        var action = new JsonObject { ["commands"] = ToArray(item.Action.Commands) };
        if (item.Action.TimeoutMs is { } timeoutMs)
        {
            action["timeoutMs"] = Number(timeoutMs);
        }

        if (item.Action.MaxOutputLength is { } maxOutputLength)
        {
            action["maxOutputLength"] = Number(maxOutputLength);
        }

        return new JsonObject { ["action"] = action }.ToJsonString(JsonOptions);
    }

    /// <summary>The input of a <c>local_shell_call</c> item: the action with the fields of upstream's <c>localShellInputSchema</c> parse, in snake_case as the API returned them.</summary>
    public static string LocalShellCallInput(OpenAIResponsesApi.LocalShellCall item)
    {
        var source = item.Action;
        var action = new JsonObject { ["type"] = "exec", ["command"] = ToArray(source.Command) };
        if (source.TimeoutMs is { } timeoutMs)
        {
            action["timeout_ms"] = Number(timeoutMs);
        }

        if (source.User is { } user)
        {
            action["user"] = user;
        }

        if (source.WorkingDirectory is { } workingDirectory)
        {
            action["working_directory"] = workingDirectory;
        }

        if (source.Env is { } env)
        {
            action["env"] = ToObject(env);
        }

        return new JsonObject { ["action"] = action }.ToJsonString(JsonOptions);
    }

    /// <summary>The input of a streamed <c>local_shell_call</c> item: an exec action with camelCase fields, as upstream's stream builds it.</summary>
    public static string LocalShellStreamCallInput(OpenAIResponsesApi.LocalShellCall item)
    {
        var source = item.Action;
        var action = new JsonObject { ["type"] = "exec", ["command"] = ToArray(source.Command) };
        if (source.TimeoutMs is { } timeoutMs)
        {
            action["timeoutMs"] = Number(timeoutMs);
        }

        if (source.User is { } user)
        {
            action["user"] = user;
        }

        if (source.WorkingDirectory is { } workingDirectory)
        {
            action["workingDirectory"] = workingDirectory;
        }

        if (source.Env is { } env)
        {
            action["env"] = ToObject(env);
        }

        return new JsonObject { ["action"] = action }.ToJsonString(JsonOptions);
    }

    /// <summary>The result of a <c>shell_call_output</c> item, in the shape of upstream's <c>shellOutputSchema</c>.</summary>
    public static string ShellCallOutputResult(OpenAIResponsesApi.ShellCallOutput item)
    {
        var result = new JsonObject();
        if (item.MaxOutputLength is { } maxOutputLength)
        {
            result["maxOutputLength"] = Number(maxOutputLength);
        }

        var output = new JsonArray();
        foreach (var entry in item.Output)
        {
            output.Add(
                new JsonObject
                {
                    ["stdout"] = entry.Stdout,
                    ["stderr"] = entry.Stderr,
                    ["outcome"] = entry.Outcome is OpenAIResponsesApi.ExitOutcome exit
                        ? new JsonObject { ["type"] = "exit", ["exitCode"] = Number(exit.ExitCode) }
                        : new JsonObject { ["type"] = "timeout" },
                }
            );
        }

        result["output"] = output;
        return result.ToJsonString(JsonOptions);
    }

    /// <summary>The input of an <c>apply_patch_call</c> item: its call id and operation.</summary>
    public static string ApplyPatchInput(OpenAIResponsesApi.ApplyPatchCall item) =>
        new JsonObject
        {
            ["callId"] = item.CallId,
            ["operation"] = item.Operation switch
            {
                OpenAIResponsesApi.CreateFileOperation create => new JsonObject
                {
                    ["type"] = "create_file",
                    ["path"] = create.Path,
                    ["diff"] = create.Diff,
                },
                OpenAIResponsesApi.DeleteFileOperation delete => new JsonObject
                {
                    ["type"] = "delete_file",
                    ["path"] = delete.Path,
                },
                OpenAIResponsesApi.UpdateFileOperation update => new JsonObject
                {
                    ["type"] = "update_file",
                    ["path"] = update.Path,
                    ["diff"] = update.Diff,
                },
                _ => throw new InvalidOperationException("Unknown apply patch operation."),
            },
        }.ToJsonString(JsonOptions);

    /// <summary>The <c>type</c> of an apply patch operation.</summary>
    public static string ApplyPatchOperationType(
        OpenAIResponsesApi.ApplyPatchOperation operation
    ) =>
        operation switch
        {
            OpenAIResponsesApi.CreateFileOperation => "create_file",
            OpenAIResponsesApi.DeleteFileOperation => "delete_file",
            _ => "update_file",
        };

    /// <summary>Provider metadata that carries only the output item id.</summary>
    public static JsonElement ItemMetadata(string? id, string providerName = "openai") =>
        OpenAIJson.ProviderMetadata(new JsonObject { ["itemId"] = id }, providerName);

    /// <summary>The provider metadata of an <c>openai.compaction</c> custom part (<c>ResponsesCompactionProviderMetadata</c>).</summary>
    public static JsonElement CompactionMetadata(
        OpenAIResponsesApi.Compaction item,
        string providerName = "openai"
    ) =>
        OpenAIJson.ProviderMetadata(
            new JsonObject
            {
                ["type"] = "compaction",
                ["itemId"] = item.Id,
                ["encryptedContent"] = item.EncryptedContent,
            },
            providerName
        );

    /// <summary>The tool name of an <c>mcp_call</c> item: <c>mcp.</c> and the name of the MCP tool.</summary>
    public static string McpToolName(string name) => "mcp." + name;

    /// <summary>The result an <c>mcp_call</c> reports, in the shape of upstream's <c>mcpOutputSchema</c> call result.</summary>
    public static string McpCallResult(OpenAIResponsesApi.McpCall item)
    {
        var result = new JsonObject
        {
            ["type"] = "call",
            ["serverLabel"] = item.ServerLabel,
            ["name"] = item.Name,
            ["arguments"] = item.Arguments,
        };
        if (item.Output is { } output)
        {
            result["output"] = output;
        }

        switch (item.Error)
        {
            case OpenAIResponsesApi.ErrorText text:
                result["error"] = text.Value;
                break;
            case OpenAIResponsesApi.ErrorObject error:
                var errorObject = new JsonObject();
                if (error.Type is { } type)
                {
                    errorObject["type"] = type;
                }

                if (error.Code is { } code)
                {
                    errorObject["code"] = long.TryParse(
                        code,
                        System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var numeric
                    )
                        ? numeric
                        : code;
                }

                if (error.Message is { } message)
                {
                    errorObject["message"] = message;
                }

                result["error"] = errorObject;
                break;
        }

        return result.ToJsonString();
    }

    /// <summary>Provider metadata for a text block.</summary>
    public static JsonElement TextMetadata(
        string? itemId,
        IReadOnlyList<OpenAIResponsesApi.Annotation> annotations,
        string? phase = null,
        string providerName = "openai"
    )
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
                array.Add(AnnotationJson(annotation));
            }

            metadata["annotations"] = array;
        }

        return OpenAIJson.ProviderMetadata(metadata, providerName);
    }

    /// <summary>The wire name of a message phase.</summary>
    public static string? PhaseName(OpenAIResponsesApi.MessagePhase? phase) =>
        phase switch
        {
            OpenAIResponsesApi.MessagePhase.Commentary => "commentary",
            OpenAIResponsesApi.MessagePhase.FinalAnswer => "final_answer",
            _ => null,
        };

    /// <summary>The URL sources of a <c>web_search_call</c> search action.</summary>
    public static List<string> WebSearchUrls(OpenAIResponsesApi.WebSearchAction? action)
    {
        var urls = new List<string>();
        if (action is OpenAIResponsesApi.WebSearchSearch { Sources: { } sources })
        {
            foreach (var source in sources)
            {
                if (source is OpenAIResponsesApi.WebSearchUrlSource url)
                {
                    urls.Add(url.Url);
                }
            }
        }

        return urls;
    }

    /// <summary>The result a <c>web_search_call</c> reports.</summary>
    public static string WebSearchResult(OpenAIResponsesApi.WebSearchAction? action)
    {
        var result = new JsonObject();
        switch (action)
        {
            case null:
                return "{}";
            case OpenAIResponsesApi.WebSearchSearch searchAction:
                var search = new JsonObject { ["type"] = "search" };
                if (searchAction.Query is { } query)
                {
                    search["query"] = query;
                }

                if (searchAction.Queries is { } queries)
                {
                    search["queries"] = ToArray(queries);
                }

                result["action"] = search;
                if (searchAction.Sources is { } sources)
                {
                    var array = new JsonArray();
                    foreach (var source in sources)
                    {
                        array.Add(
                            source switch
                            {
                                OpenAIResponsesApi.WebSearchUrlSource url => new JsonObject
                                {
                                    ["type"] = "url",
                                    ["url"] = url.Url,
                                },
                                OpenAIResponsesApi.WebSearchApiSource api => new JsonObject
                                {
                                    ["type"] = "api",
                                    ["name"] = api.Name,
                                },
                                _ => throw new InvalidOperationException("Unknown source."),
                            }
                        );
                    }

                    result["sources"] = array;
                }

                break;
            case OpenAIResponsesApi.WebSearchOpenPage openPage:
                result["action"] = new JsonObject { ["type"] = "openPage", ["url"] = openPage.Url };
                break;
            case OpenAIResponsesApi.WebSearchFindInPage findInPage:
                result["action"] = new JsonObject
                {
                    ["type"] = "findInPage",
                    ["url"] = findInPage.Url,
                    ["pattern"] = findInPage.Pattern,
                };
                break;
        }

        return result.ToJsonString();
    }

    /// <summary>The result a <c>file_search_call</c> reports.</summary>
    public static string FileSearchResult(
        IReadOnlyList<string> queries,
        IReadOnlyList<OpenAIResponsesApi.FileSearchResult>? results
    )
    {
        JsonNode? mapped = null;
        if (results != null)
        {
            var array = new JsonArray();
            foreach (var result in results)
            {
                var attributes = new JsonObject();
                foreach (var attribute in result.Attributes)
                {
                    attributes[attribute.Key] = attribute.Value switch
                    {
                        string text => JsonValue.Create(text),
                        bool flag => JsonValue.Create(flag),
                        double number => Number(number),
                        _ => null,
                    };
                }

                array.Add(
                    new JsonObject
                    {
                        ["attributes"] = attributes,
                        ["fileId"] = result.FileId,
                        ["filename"] = result.Filename,
                        ["score"] = Number(result.Score),
                        ["text"] = result.Text,
                    }
                );
            }

            mapped = array;
        }

        return new JsonObject
        {
            ["queries"] = ToArray(queries),
            ["results"] = mapped,
        }.ToJsonString();
    }

    /// <summary>The retrieved files of a <c>file_search_call</c>, one per file id.</summary>
    public static IEnumerable<(string FileId, string Filename)> RetrievedFiles(
        IReadOnlyList<OpenAIResponsesApi.FileSearchResult>? results
    ) => results?.Select(result => (result.FileId, result.Filename)) ?? [];

    /// <summary>Provider metadata of a retrieved file source.</summary>
    public static JsonElement FileSearchMetadata(string fileId, string providerName = "openai") =>
        OpenAIJson.ProviderMetadata(
            new JsonObject { ["type"] = "file_search", ["fileId"] = fileId },
            providerName
        );

    /// <summary>Whether a <c>file_search_call</c> has any result.</summary>
    public static bool HasResults(IReadOnlyList<OpenAIResponsesApi.FileSearchResult>? results) =>
        results is { Count: > 0 };

    private static string AnnotationFileId(OpenAIResponsesApi.Annotation annotation) =>
        annotation switch
        {
            OpenAIResponsesApi.FileCitation file => file.FileId,
            OpenAIResponsesApi.ContainerFileCitation container => container.FileId,
            OpenAIResponsesApi.FilePath path => path.FileId,
            _ => string.Empty,
        };

    private static JsonArray ToArray(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        return array;
    }

    private static JsonObject ToObject(IReadOnlyDictionary<string, string> values)
    {
        var result = new JsonObject();
        foreach (var entry in values)
        {
            result[entry.Key] = entry.Value;
        }

        return result;
    }

    // A whole number is written without a fraction, as JSON.stringify does.
    private static JsonNode Number(double value) =>
        value == Math.Floor(value) && Math.Abs(value) < 9e15
            ? JsonValue.Create((long)value)
            : JsonValue.Create(value);
}
