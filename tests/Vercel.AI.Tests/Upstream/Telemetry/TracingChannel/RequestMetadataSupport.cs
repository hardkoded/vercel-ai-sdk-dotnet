// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateTextSteeringInterrupt;
using Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateTextSteeringInterrupt.SteeringInterruptTestSupport;
using static Vercel.AI.Tests.Upstream.Telemetry.TracingChannel.TracingChannelSupport;

namespace Vercel.AI.Tests.Upstream.Telemetry.TracingChannel;

/// <summary>
/// The shared body of the <c>%s request metadata</c> tests of <c>tracing-channel.test.ts</c>. Upstream runs one <c>describe.each</c>
/// for <c>generateText</c> and <c>streamText</c>, so each API has its own test class.
/// </summary>
internal static class RequestMetadataSupport
{
    /// <summary>The cases of <c>sanitizes $request before telemetry observes the provider result</c>.</summary>
    public static TheoryData<string?, string?, bool> SanitizeCases =>
        new()
        {
            {
                "https://user:password@model.example.test:8443/private?key=secret#fragment",
                "https://model.example.test:8443",
                true
            },
            { "not a URL", null, true },
            { "data:text/plain,secret", null, true },
            { null, null, true },
            { null, null, false },
        };

    /// <summary>
    /// Runs the <c>sanitizes $request before telemetry observes the provider result</c> body for one API. The request has a
    /// body <c>keep</c>, and the URL of the case when the case has a request.
    /// </summary>
    public static async Task SanitizeRequestBeforeTelemetryObservesTheProviderResult(
        bool stream,
        string callId,
        string? url,
        string? expectedUrl,
        bool hasRequest
    )
    {
        var keep = JsonDocument.Parse("\"keep\"").RootElement.Clone();
        var providerRequest = hasRequest
            ? new LanguageModelStreamRequest(keep) { Url = url }
            : null;
        var integrationRequests = new List<LanguageModelStreamRequest?>();
        var model = new InterruptTestModel
        {
            Generate = (_, _) =>
                Task.FromResult(
                    new LanguageModelGenerateResult(
                        [new GeneratedText("Hello")],
                        FinishReason.Stop,
                        UsageOf(1),
                        "stop",
                        request: providerRequest
                    )
                ),
            Stream = (_, _) =>
                Task.FromResult(
                    new LanguageModelStreamResult(ToAsync(TextStream("Hello")), providerRequest)
                ),
        };

        var published = await Collect(async () =>
        {
            var telemetry = new TelemetryOptions
            {
                Integrations =
                [
                    new TestTelemetry
                    {
                        ExecuteLanguageModelCall = new RequestRecorder(integrationRequests),
                    },
                ],
            };
            if (stream)
            {
                var result = Client()
                    .StreamTextAsync(
                        new StreamTextOptions
                        {
                            Model = model,
                            Prompt = "Hello",
                            Telemetry = telemetry,
                            GenerateCallId = () => callId,
                        }
                    );
                Assert.Equal("Hello", await result.Text);
            }
            else
            {
                var result = await Client()
                    .GenerateTextAsync(
                        new GenerateTextOptions
                        {
                            Model = model,
                            Prompt = "Hello",
                            Telemetry = telemetry,
                            GenerateCallId = () => callId,
                        }
                    );
                Assert.Equal("Hello", result.Text);
            }
        });

        var tracedRequests = published
            .Where(item =>
                item.Channel == "asyncEnd"
                && item.Message.Type == TelemetryTracingEventType.LanguageModelCall
                && CallIdOf(item.Message) == callId
            )
            .Select(item =>
                item.Message.Result switch
                {
                    LanguageModelGenerateResult generated => generated.Request,
                    LanguageModelStreamResult streamed => streamed.Request,
                    _ => null,
                }
            )
            .ToList();

        var tracedRequest = Assert.Single(tracedRequests);
        var integrationRequest = Assert.Single(integrationRequests);
        foreach (var observed in new[] { tracedRequest, integrationRequest })
        {
            if (!hasRequest)
            {
                Assert.Null(observed);
                continue;
            }

            Assert.NotNull(observed);
            Assert.Equal(expectedUrl, observed.Url);
            Assert.Equal("keep", observed.Body?.GetString());
        }

        // The request of the provider is not changed.
        Assert.Equal(url, providerRequest?.Url);
    }

    /// <summary>
    /// Runs the <c>applies OpenAI redaction through middleware with HTTP %s</c> body for one API with HTTP 200: a Responses model wrapped in
    /// middleware, a code interpreter tool with a domain secret, and subscribers that snapshot everything telemetry observes. The
    /// HTTP 503 case needs the model call retry of <c>generateText</c> and <c>streamText</c>, which the port does not have.
    /// </summary>
    public static async Task ApplyOpenAIRedactionThroughMiddleware(bool stream)
    {
        const string Secret = "test-domain-secret";
        var domainSecrets = new[]
        {
            new OpenAIShellDomainSecret
            {
                Domain = "example.com",
                Name = "KEY",
                Value = Secret,
            },
        };
        var interpreter = OpenAITools.CodeInterpreter(
            new OpenAICodeInterpreterToolOptions
            {
                AllowedDomains = ["example.com"],
                DomainSecrets = domainSecrets,
            }
        );
        var python = Tool.ProviderDefined(interpreter.ProviderToolId!, "python", interpreter.Args);
        python.RedactArgs = interpreter.RedactArgs;
        var originalArgs = interpreter.Args.GetRawText();

        var handler = new OpenAIResponsesHandler();
        var provider = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-key" }, handler);
        var model = provider.ResponsesModel("gpt-4o").WrapLanguageModel();

        var recorded = new List<string>();
        void Record(object? value)
        {
            var snapshot = Snapshot(value);
            lock (recorded)
            {
                recorded.Add(snapshot);
            }
        }

        var callId = "openai-redaction-" + Guid.NewGuid();
        var modelCallStarts = new List<LanguageModelCallStartEvent>();
        var integration = new TestTelemetry
        {
            OnStart = (e, _) => Task.Run(() => Record(e)),
            OnStepEnd = (e, _) => Task.Run(() => Record(e)),
            OnEnd = (e, _) => Task.Run(() => Record(e)),
            OnError = (e, _) => Task.Run(() => Record(e)),
            OnStepError = (e, _) => Task.Run(() => Record(e)),
            ExecuteLanguageModelCall = new ResultRecorder(Record),
        };

        using (var subscription = SubscribeForRecording(Record, callId))
        {
            var include = new GenerateTextInclude { RequestBody = true, ResponseBody = true };
            Task OnModelCallStart(LanguageModelCallStartEvent e, CancellationToken _)
            {
                lock (modelCallStarts)
                {
                    modelCallStarts.Add(e);
                }

                return Task.CompletedTask;
            }

            if (stream)
            {
                var result = Client()
                    .StreamTextAsync(
                        new StreamTextOptions
                        {
                            Model = model,
                            Tools = [python],
                            Prompt = "Hello",
                            MaxRetries = 1,
                            Include = new StreamTextInclude { RequestBody = true },
                            OnLanguageModelCallStart = OnModelCallStart,
                            OnError = (_, _) => Task.CompletedTask,
                            Telemetry = new TelemetryOptions { Integrations = [integration] },
                            GenerateCallId = () => callId,
                        }
                    );
                Assert.Equal("", await result.Text);
                var finalStep = await result.FinalStep;
                Assert.Contains(Secret, finalStep.Request.Body?.GetRawText());
            }
            else
            {
                var result = await Client()
                    .GenerateTextAsync(
                        new GenerateTextOptions
                        {
                            Model = model,
                            Tools = [python],
                            Prompt = "Hello",
                            MaxRetries = 1,
                            Include = include,
                            OnLanguageModelCallStart = OnModelCallStart,
                            OnError = (_, _) => Task.CompletedTask,
                            Telemetry = new TelemetryOptions { Integrations = [integration] },
                            GenerateCallId = () => callId,
                        }
                    );
                Assert.Equal("", result.Text);
                Assert.Contains(Secret, result.Steps[^1].Request.Body?.GetRawText());
                Assert.Contains(Secret, result.Steps[^1].Response.Body?.GetRawText());
            }
        }

        var everything = string.Join("\n", recorded);
        Assert.Contains("[redacted]", everything);
        Assert.DoesNotContain(Secret, everything);
        Assert.Single(handler.Bodies);
        foreach (var body in handler.Bodies)
        {
            Assert.Contains(Secret, body);
        }

        // The tool the caller gave is the tool the model call start reports, and the secret of the caller is unchanged.
        Assert.Equal(originalArgs, modelCallStarts[0].Tools![0].Args.GetRawText());
        Assert.Equal(Secret, domainSecrets[0].Value);
    }

    // Snapshot at publication time, including nested retry errors. Maps to `JSON.stringify` of the observed value.
    private static string Snapshot(object? value)
    {
        var builder = new StringBuilder();
        if (value is LanguageModelStreamResult streamResult)
        {
            // The parts of a stream are not read. Only its metadata is.
            value = new { streamResult.Request, streamResult.Response };
        }

        AppendException(builder, value);
        try
        {
            builder.Append(
                JsonSerializer.Serialize(
                    value,
                    new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.IgnoreCycles,
                        MaxDepth = 128,
                        Converters =
                        {
                            new ExceptionConverter(),
                            new OpaqueConverter(),
                            new ElementConverter(),
                        },
                        IncludeFields = false,
                    }
                )
            );
        }
        catch (Exception)
        {
            builder.Append(value);
        }

        return builder.ToString();
    }

    private static void AppendException(StringBuilder builder, object? value)
    {
        if (value is not Exception error)
        {
            return;
        }

        builder.Append(error.Message).Append(' ');
        if (error is ApiCallError api)
        {
            builder.Append(api.Url).Append(' ');
            builder.Append(Snapshot(api.RequestBodyValues)).Append(' ');
            builder.Append(api.ResponseBody).Append(' ');
        }

        if (error is RetryError retry)
        {
            foreach (var attempt in retry.Errors)
            {
                builder.Append(Snapshot(attempt)).Append(' ');
            }
        }

        if (error.InnerException != null)
        {
            AppendException(builder, error.InnerException);
        }
    }

    // Streams, delegates and tasks have no JSON form. `JSON.stringify` skips functions and does not read streams.
    private sealed class OpaqueConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeof(Delegate).IsAssignableFrom(typeToConvert)
            || typeof(Task).IsAssignableFrom(typeToConvert)
            || typeof(System.IO.Stream).IsAssignableFrom(typeToConvert)
            || typeToConvert
                .GetInterfaces()
                .Append(typeToConvert)
                .Any(type =>
                    type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>)
                );

        public override JsonConverter CreateConverter(
            Type typeToConvert,
            JsonSerializerOptions options
        ) =>
            (JsonConverter)
                Activator.CreateInstance(typeof(Opaque<>).MakeGenericType(typeToConvert))!;

        private sealed class Opaque<T> : JsonConverter<T>
        {
            public override T Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options
            ) => throw new NotSupportedException();

            public override void Write(
                Utf8JsonWriter writer,
                T value,
                JsonSerializerOptions options
            ) => writer.WriteNullValue();
        }
    }

    // An undefined JsonElement is an absent property, like `undefined` in `JSON.stringify`.
    private sealed class ElementConverter : JsonConverter<JsonElement>
    {
        public override JsonElement Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) => throw new NotSupportedException();

        public override void Write(
            Utf8JsonWriter writer,
            JsonElement value,
            JsonSerializerOptions options
        )
        {
            if (value.ValueKind == JsonValueKind.Undefined)
            {
                writer.WriteNullValue();
            }
            else
            {
                value.WriteTo(writer);
            }
        }
    }

    private sealed class ExceptionConverter : JsonConverter<Exception>
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeof(Exception).IsAssignableFrom(typeToConvert);

        public override Exception? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) => throw new NotSupportedException();

        public override void Write(
            Utf8JsonWriter writer,
            Exception value,
            JsonSerializerOptions options
        ) => writer.WriteStringValue(value.Message);
    }

    // Subscribes to the sub-channels that upstream's test subscribes to: start, asyncEnd and error.
    private static IDisposable SubscribeForRecording(Action<object?> record, string callId)
    {
        return System.Diagnostics.DiagnosticListener.AllListeners.Subscribe(
            new RecordingObserver(record, callId)
        );
    }

    private sealed class RecordingObserver(Action<object?> record, string callId)
        : IObserver<System.Diagnostics.DiagnosticListener>
    {
        public void OnNext(System.Diagnostics.DiagnosticListener listener)
        {
            if (listener.Name == Vercel.AI.TracingChannel.AiSdkTelemetryTracingChannel)
            {
                listener.Subscribe(new RecordingEvents(record, callId));
            }
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }
    }

    private sealed class RecordingEvents(Action<object?> record, string callId)
        : IObserver<KeyValuePair<string, object?>>
    {
        public void OnNext(KeyValuePair<string, object?> item)
        {
            // The tracing channel is shared by every test of the process, so only the spans of this call are recorded.
            if (
                item.Key is "start" or "asyncEnd" or "error"
                && item.Value is TelemetryTracingChannelMessage message
                && CallIdOf(message) == callId
            )
            {
                record(item.Value);
            }
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }
    }

    private sealed class ResultRecorder(Action<object?> record) : IExecuteLanguageModelCall
    {
        public async Task<T> ExecuteAsync<T>(
            ExecuteLanguageModelCallOptions<T> options,
            CancellationToken cancellationToken
        )
        {
            var result = await options.Execute();
            record(result);
            return result;
        }
    }

    private sealed class OpenAIResponsesHandler : HttpMessageHandler
    {
        public List<string> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var text = await request.Content!.ReadAsStringAsync(cancellationToken);
            lock (Bodies)
            {
                Bodies.Add(text);
            }

            var body = JsonDocument.Parse(text).RootElement;
            var tools = body.TryGetProperty("tools", out var echoed) ? echoed.GetRawText() : "[]";
            var response =
                "{\"id\":\"response-1\",\"model\":\"gpt-4o\",\"created_at\":0,\"output\":[],\"tools\":"
                + tools
                + ",\"usage\":{\"input_tokens\":1,\"output_tokens\":0,\"total_tokens\":1}}";
            var isStream = body.TryGetProperty("stream", out var flag) && flag.GetBoolean();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = isStream
                    ? new StringContent(
                        "data: {\"type\":\"response.completed\",\"response\":" + response + "}\n\n",
                        Encoding.UTF8,
                        "text/event-stream"
                    )
                    : new StringContent(response, Encoding.UTF8, "application/json"),
            };
        }
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> ToAsync(
        IReadOnlyList<LanguageModelStreamPart> parts
    )
    {
        foreach (var part in parts)
        {
            yield return part;
        }

        await Task.CompletedTask;
    }

    private sealed class RequestRecorder(List<LanguageModelStreamRequest?> requests)
        : IExecuteLanguageModelCall
    {
        public async Task<T> ExecuteAsync<T>(
            ExecuteLanguageModelCallOptions<T> options,
            CancellationToken cancellationToken
        )
        {
            var result = await options.Execute();
            lock (requests)
            {
                requests.Add(
                    result switch
                    {
                        LanguageModelGenerateResult generated => generated.Request,
                        LanguageModelStreamResult streamed => streamed.Request,
                        _ => null,
                    }
                );
            }

            return result;
        }
    }
}
