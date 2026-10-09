// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenAICompatible;

/// <summary>Request-body transforms used by thin OpenAI-compatible providers.</summary>
public static class OpenAICompatibleTransforms
{
    /// <summary>
    /// Maps Groq reasoning levels. <c>none</c> is kept only for <c>qwen/qwen3.6-27b</c>.
    /// <c>minimal</c> becomes <c>low</c> and <c>xhigh</c> and <c>max</c> become <c>high</c>.
    /// </summary>
    public static JsonObject Groq(JsonObject body, IList<CallWarning> warnings)
    {
        var effort = Text(body["reasoning_effort"]);
        var model = Text(body["model"]);
        if (effort == null)
        {
            return body;
        }

        if (effort == "none")
        {
            if (model != "qwen/qwen3.6-27b")
            {
                body.Remove("reasoning_effort");
                warnings.Add(new CallWarning("unsupported", "reasoning \"none\" is not supported by this model."));
            }

            return body;
        }

        if (effort == "minimal")
        {
            body["reasoning_effort"] = "low";
            warnings.Add(new CallWarning("compatibility", "reasoning \"minimal\" is not directly supported by this model. mapped to effort \"low\"."));
        }
        else if (effort == "xhigh" || effort == "max")
        {
            body["reasoning_effort"] = "high";
            warnings.Add(new CallWarning("compatibility", "reasoning \"" + effort + "\" is not directly supported by this model. mapped to effort \"high\"."));
        }

        return body;
    }

    /// <summary>Maps Cerebras field names and assistant <c>reasoning_content</c> onto <c>reasoning</c>.</summary>
    public static JsonObject Cerebras(JsonObject body, IList<CallWarning> warnings)
    {
        Rename(body, "max_tokens", "max_completion_tokens");
        Rename(body, "parallelToolCalls", "parallel_tool_calls");
        Rename(body, "topLogprobs", "top_logprobs");
        Rename(body, "logitBias", "logit_bias");
        Rename(body, "serviceTier", "service_tier");
        Rename(body, "reasoningFormat", "reasoning_format");
        Rename(body, "promptCacheKey", "prompt_cache_key");
        if (body["messages"] is JsonArray messages)
        {
            foreach (var node in messages)
            {
                if (node is not JsonObject message || Text(message["role"]) != "assistant" || !message.ContainsKey("reasoning_content"))
                {
                    continue;
                }

                var reasoning = message["reasoning_content"];
                message.Remove("reasoning_content");
                if (!message.ContainsKey("reasoning") && reasoning != null && reasoning.GetValueKind() != System.Text.Json.JsonValueKind.Null)
                {
                    message["reasoning"] = reasoning;
                }
            }
        }

        return body;
    }

    /// <summary>Maps Fireworks thinking, cache, service tier, and reasoning effort. Fireworks supports <c>low</c>, <c>medium</c>, and <c>high</c>.</summary>
    public static JsonObject Fireworks(JsonObject body, IList<CallWarning> warnings)
    {
        var thinking = body["thinking"] as JsonObject;
        var reasoningHistory = Text(body["reasoningHistory"]);
        var promptCacheKey = Text(body["promptCacheKey"]);
        var serviceTier = Text(body["serviceTier"]);
        var effort = Text(body["reasoning_effort"]);
        body.Remove("thinking");
        body.Remove("reasoningHistory");
        body.Remove("promptCacheKey");
        body.Remove("serviceTier");
        if (effort != null)
        {
            var mapped = effort == "minimal" ? "low" : effort == "xhigh" || effort == "max" ? "high" : effort;
            body["reasoning_effort"] = mapped;
            if (mapped != effort)
            {
                warnings.Add(new CallWarning("compatibility", "reasoning \"" + effort + "\" is not directly supported by this model. mapped to effort \"" + mapped + "\"."));
            }
        }

        if (promptCacheKey != null)
        {
            body["prompt_cache_key"] = promptCacheKey;
        }

        if (serviceTier != null)
        {
            body["service_tier"] = serviceTier;
        }

        if (thinking != null)
        {
            var next = new JsonObject();
            if (thinking["type"] != null)
            {
                next["type"] = thinking["type"]!.DeepClone();
            }

            if (thinking.ContainsKey("budgetTokens"))
            {
                next["budget_tokens"] = thinking["budgetTokens"]?.DeepClone();
            }

            body["thinking"] = next;
        }

        if (reasoningHistory != null)
        {
            body["reasoning_history"] = reasoningHistory;
        }

        return body;
    }

    /// <summary>Drops fields Z.AI does not accept and renames its provider options.</summary>
    public static JsonObject Zai(JsonObject body, IList<CallWarning> warnings)
    {
        var requestIdText = Text(body["requestId"]);
        if (requestIdText != null && (requestIdText.Length < 6 || requestIdText.Length > 64))
        {
            throw new AiSdkException("invalid zai provider options");
        }

        var userIdText = Text(body["userId"]);
        if (userIdText != null && (userIdText.Length < 6 || userIdText.Length > 128))
        {
            throw new AiSdkException("invalid zai provider options");
        }

        var unknown = new List<string>();
        foreach (var pair in body)
        {
            if (!IsZaiBodyField(pair.Key))
            {
                unknown.Add(pair.Key);
            }
        }

        foreach (var key in unknown)
        {
            body.Remove(key);
        }

        if (body.ContainsKey("frequency_penalty"))
        {
            body.Remove("frequency_penalty");
            warnings.Add(new CallWarning("unsupported", "frequencyPenalty"));
        }

        if (body.ContainsKey("presence_penalty"))
        {
            body.Remove("presence_penalty");
            warnings.Add(new CallWarning("unsupported", "presencePenalty"));
        }

        if (body.ContainsKey("seed"))
        {
            body.Remove("seed");
            warnings.Add(new CallWarning("unsupported", "seed"));
        }

        body.Remove("user");
        body.Remove("verbosity");
        var doSample = body["doSample"];
        var thinking = body["thinking"] as JsonObject;
        var toolStream = body["toolStream"];
        var requestId = Text(body["requestId"]);
        var userId = Text(body["userId"]);
        body.Remove("doSample");
        body.Remove("thinking");
        body.Remove("toolStream");
        body.Remove("requestId");
        body.Remove("userId");
        if (doSample != null)
        {
            body["do_sample"] = doSample.DeepClone();
        }

        if (thinking != null)
        {
            var next = new JsonObject();
            if (thinking["type"] != null)
            {
                next["type"] = thinking["type"]!.DeepClone();
            }

            if (thinking.ContainsKey("clearThinking"))
            {
                next["clear_thinking"] = thinking["clearThinking"]?.DeepClone();
            }

            body["thinking"] = next;
        }

        if (toolStream != null)
        {
            body["tool_stream"] = toolStream.DeepClone();
        }

        if (requestId != null)
        {
            body["request_id"] = requestId;
        }

        if (userId != null)
        {
            body["user_id"] = userId;
        }

        var choiceNode = body["tool_choice"];
        var choice = Text(choiceNode);
        if (choice == "none")
        {
            body.Remove("tools");
            body.Remove("tool_choice");
        }
        else if (choiceNode != null && choice != "auto")
        {
            body.Remove("tool_choice");
            var label = choice ?? "tool";
            warnings.Add(new CallWarning("unsupported", "toolChoice " + label + ". Z.AI currently supports only automatic tool selection."));
        }

        return body;
    }

    private static bool IsZaiBodyField(string name)
    {
        switch (name)
        {
            case "model":
            case "user":
            case "temperature":
            case "top_p":
            case "max_tokens":
            case "presence_penalty":
            case "frequency_penalty":
            case "stop":
            case "seed":
            case "response_format":
            case "reasoning_effort":
            case "verbosity":
            case "messages":
            case "tools":
            case "tool_choice":
            case "stream":
            case "stream_options":
            case "logprobs":
            case "top_logprobs":
            case "n":
            case "logit_bias":
            case "parallel_tool_calls":
            case "doSample":
            case "thinking":
            case "toolStream":
            case "requestId":
            case "userId":
                return true;
            default:
                return false;
        }
    }

    /// <summary>Maps a Z.AI finish reason. Unrecognized values fall through.</summary>
    public static FinishReason? ZaiFinish(string? raw)
    {
        switch (raw)
        {
            case "sensitive":
                return FinishReason.ContentFilter;
            case "model_context_window_exceeded":
                return FinishReason.Length;
            case "network_error":
                return FinishReason.Error;
            default:
                return null;
        }
    }

    /// <summary>Reads a GMI Cloud error, preferring the diagnostic nested in <c>error.details</c>.</summary>
    public static string? GmiCloudError(string? body)
    {
        if (string.IsNullOrWhiteSpace(body) || body!.TrimStart().StartsWith("{", StringComparison.Ordinal) == false)
        {
            return null;
        }

        try
        {
            var root = JsonNode.Parse(body) as JsonObject;
            if (root?["error"] is not JsonObject error)
            {
                return null;
            }

            var details = Text(error["details"]);
            var inner = DetailsMessage(details);
            if (!string.IsNullOrEmpty(inner))
            {
                return inner;
            }

            return Text(error["message"]);
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Reads a Baseten error. The Model APIs send <c>error</c> as a string; dedicated deployments send an
    /// object with a <c>message</c>.
    /// </summary>
    public static string? BasetenError(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            var error = (JsonNode.Parse(body!) as JsonObject)?["error"];
            return error is JsonObject envelope ? Text(envelope["message"]) : Text(error);
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    /// <summary>Reads a Cerebras error message from the top-level <c>message</c> field.</summary>
    public static string? CerebrasError(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            var root = JsonNode.Parse(body!) as JsonObject;
            return Text(root?["message"]);
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    /// <summary>Reads a Z.AI error from <c>error.message</c> or a top-level <c>message</c>.</summary>
    public static string? ZaiError(string? body)
    {
        var nested = OpenAICompatibleChat.ReadErrorMessage(body);
        if (!string.IsNullOrEmpty(nested))
        {
            return nested;
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            var root = JsonNode.Parse(body!) as JsonObject;
            if (root == null || root["error"] != null)
            {
                return null;
            }

            return Text(root["message"]);
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    private static string? DetailsMessage(string? details)
    {
        if (string.IsNullOrEmpty(details))
        {
            return null;
        }

        try
        {
            var parsed = JsonNode.Parse(details!) as JsonObject;
            if (parsed?["error"] is JsonObject error)
            {
                var message = Text(error["message"]);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    return message;
                }
            }
        }
        catch (System.Text.Json.JsonException)
        {
        }

        return null;
    }

    private static void Rename(JsonObject body, string from, string to)
    {
        if (!body.ContainsKey(from))
        {
            return;
        }

        var value = body[from];
        body.Remove(from);
        if (value != null)
        {
            body[to] = value;
        }
    }

    private static string? Text(JsonNode? node)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            return text;
        }

        return null;
    }
}
