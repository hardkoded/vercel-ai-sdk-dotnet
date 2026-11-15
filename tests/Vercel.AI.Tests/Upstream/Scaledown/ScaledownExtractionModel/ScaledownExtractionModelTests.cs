// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.ScaleDown;
using Vercel.AI.Tests.Upstream.Scaledown.ScaledownDecisionModel;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Scaledown.ScaledownExtractionModel;

/// <summary>Port of the tests outside any <c>describe</c> in <c>scaledown-extraction-model.test.ts</c>.</summary>
public sealed class ScaledownExtractionModelTests
{
    private const string Prefix = "packages/scaledown/src/scaledown-extraction-model.test.ts::";
    private const string UpstreamCommit = "c6db7181764bdfe0b6a9d8a161755873299e3105";
    private const string Url = "https://api.scaledown.xyz/v1/chat/completions";
    private const string StateText = "Acme Corp. Contacts: Ana, Bo.";

    private static readonly JsonObject Content = JsonNode.Parse("""
        {"entities":[
          {"text":"","type":"vendor","confidence":1},
          {"text":"","type":"contacts","confidence":1},
          {"text":"","type":"missing","confidence":1},
          {"text":"","type":"empty","confidence":1},
          {"text":"Acme Corp","type":"vendor","confidence":0.97,"start":0,"end":9},
          {"text":"Ana","type":"contacts","confidence":0.9,"start":21,"end":24},
          {"text":"Bo","type":"contacts","confidence":0.85,"start":26,"end":28}],
         "structured_result":null,"input_tokens":42,"ocr_text":null}
        """)!.AsObject();

    private static ScaleDownServer NewServer(JsonObject? content = null, bool nullUsage = false)
    {
        return new ScaleDownServer { ResponseBody = ResponseJson(content ?? Content, nullUsage) };
    }

    private static string ResponseJson(JsonObject content, bool nullUsage = false)
    {
        return ResponseJson(content.ToJsonString(), nullUsage);
    }

    private static string ResponseJson(string content, bool nullUsage = false)
    {
        var response = new JsonObject
        {
            ["id"] = "extract-123",
            ["created"] = 1700000000,
            ["model"] = "extract-1",
            ["choices"] = new JsonArray(new JsonObject { ["message"] = new JsonObject { ["content"] = content } }),
            ["usage"] = nullUsage ? null : JsonNode.Parse("""{"prompt_tokens":42,"completion_tokens":12,"total_tokens":54}"""),
        };
        return response.ToJsonString();
    }

    private static ScaleDownExtractionModel Model(ScaleDownServer server, string id = "extract-1")
    {
        return ScaleDownProvider.Create(new ScaleDownProviderSettings { ApiKey = "test-key" }, server).ExtractionModel(id);
    }

    private static IReadOnlyDictionary<string, ExtractionField> Fields()
    {
        return new Dictionary<string, ExtractionField>
        {
            ["vendor"] = new("text", DecisionInput.From("Company name")),
            ["contacts"] = new("list", DecisionInput.Parse("""{"find":"Contact names"}""")) { Items = "text" },
            ["missing"] = new("text"),
            ["empty"] = new("list") { Items = "text" },
        };
    }

    private static ExtractionModelCall Call(
        IReadOnlyList<DecisionModelStatePart>? state = null,
        IReadOnlyDictionary<string, ExtractionField>? fields = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null,
        string? referenceDate = null,
        double? maxOutputTokens = null,
        string? reasoning = null,
        CancellationToken cancellationToken = default)
    {
        using var document = JsonDocument.Parse(providerOptions);
        return new ExtractionModelCall(
            state ?? [new DecisionModelTextPart(StateText)],
            fields ?? Fields(),
            document.RootElement.Clone(),
            headers ?? new Dictionary<string, string>(),
            cancellationToken,
            referenceDate,
            maxOutputTokens,
            reasoning);
    }

    private static DecisionModelJsonPart JsonPart(string json)
    {
        using var document = JsonDocument.Parse(json);
        return new DecisionModelJsonPart(document.RootElement.Clone());
    }

    private static DecisionModelFilePart FilePart(string mediaType, byte[]? bytes = null, string? base64 = null)
    {
        return new DecisionModelFilePart(mediaType, new LanguageModelFilePart("data", mediaType) { Bytes = bytes, Base64 = base64 });
    }

    private static void AssertSource(ExtractionSource? source, string type, int part, int? start = null, int? end = null)
    {
        Assert.NotNull(source);
        Assert.Equal(type, source!.Type);
        Assert.Equal(part, source.Part);
        Assert.Equal(start, source.Start);
        Assert.Equal(end, source.End);
    }

    [Fact]
    [UpstreamTest(Prefix + "maps text and lists, missing values, confidence, sources, and usage", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Maps_text_and_lists_missing_values_confidence_sources_and_usage()
    {
        var server = NewServer();
        var result = await Model(server).DoExtractAsync(Call(providerOptions: """{"scaledown":{"threshold":0.5}}"""), CancellationToken.None);
        var call = Assert.Single(server.Calls);
        Assert.Equal(Url, call.Url);
        var expected = JsonNode.Parse("""
            {"model":"extract-1","messages":[{"role":"user","content":"Acme Corp. Contacts: Ana, Bo."}],
             "response_format":{"type":"json_schema","json_schema":{"name":"extraction","schema":{"type":"object","properties":{
               "vendor":{"type":"string","description":"Company name","top_n":1},
               "contacts":{"type":"string","description":"{\"find\":\"Contact names\"}","top_n":0},
               "missing":{"type":"string","top_n":1},
               "empty":{"type":"string","top_n":0}}}}},
             "threshold":0.5,"context_chars":0}
            """);
        Assert.True(JsonNode.DeepEquals(expected, call.Json), call.Body);

        Assert.Equal(["vendor", "contacts", "missing", "empty"], result.Answers.Keys);
        var vendor = result.Answers["vendor"];
        Assert.Equal("text", vendor.Type);
        Assert.Equal("Acme Corp", vendor.Text);
        Assert.Equal(0.97, vendor.Confidence);
        AssertSource(vendor.Source, "text", 0, 0, 9);
        var contacts = result.Answers["contacts"];
        Assert.Equal("list", contacts.Type);
        Assert.Equal(["Ana", "Bo"], contacts.TextItems);
        Assert.Equal(2, contacts.Items!.Count);
        Assert.Equal(0.9, contacts.Items[0].Confidence);
        AssertSource(contacts.Items[0].Source, "text", 0, 21, 24);
        Assert.Equal(0.85, contacts.Items[1].Confidence);
        AssertSource(contacts.Items[1].Source, "text", 0, 26, 28);
        var missing = result.Answers["missing"];
        Assert.Equal("text", missing.Type);
        Assert.Null(missing.Text);
        Assert.Null(missing.Confidence);
        Assert.Null(missing.Source);
        var empty = result.Answers["empty"];
        Assert.Equal("list", empty.Type);
        Assert.Null(empty.TextItems);
        Assert.Null(empty.Items);
        Assert.Equal(42, result.Usage!.InputTokens);
        Assert.Equal(12, result.Usage.OutputTokens);
        Assert.Equal("extract-123", result.Response!.Id);
        Assert.Equal(new DateTime(2023, 11, 14, 22, 13, 20, DateTimeKind.Utc), result.Response.Timestamp);
        Assert.Equal("extract-1", result.Response.ModelId);
        JsonAssert.Equal(result.Response.Body!.Value, server.ResponseBody);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves field IDs with spaces, camelCase, Unicode, and distinct casing", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Preserves_field_IDs_with_spaces_camelCase_Unicode_and_distinct_casing()
    {
        // Field IDs and entity text verified against the live ScaleDown API.
        var server = NewServer(JsonNode.Parse("""
            {"entities":[
              {"type":"invoice number","text":"INV-2041","confidence":0.9},
              {"type":"dueDate","text":"2026-12-01","confidence":0.9},
              {"type":"客户名称","text":"Zoë Martin","confidence":0.9},
              {"type":"Vendor","text":"Acme Corp","confidence":0.9}]}
            """)!.AsObject());
        var fields = new Dictionary<string, ExtractionField>
        {
            ["invoice number"] = new("text", DecisionInput.From("Invoice identifier")),
            ["dueDate"] = new("text", DecisionInput.From("Exact payment due date")),
            ["客户名称"] = new("text", DecisionInput.From("Name of the person receiving the invoice")),
            ["Vendor"] = new("text", DecisionInput.From("Name of the company issuing the invoice")),
            ["vendor"] = new("text", DecisionInput.From("Name of the company receiving the invoice")),
        };
        var result = await Model(server).DoExtractAsync(
            Call([new DecisionModelTextPart("Invoice INV-2041 issued by Acme Corp to Zoë Martin. Payment is due on 2026-12-01.")], fields),
            CancellationToken.None);
        var properties = Assert.Single(server.Calls).Json!["response_format"]!["json_schema"]!["schema"]!["properties"]!.AsObject();
        Assert.Equal(["invoice number", "dueDate", "客户名称", "Vendor", "vendor"], properties.Select(pair => pair.Key));
        foreach (var (id, text) in new[] { ("invoice number", "INV-2041"), ("dueDate", "2026-12-01"), ("客户名称", "Zoë Martin"), ("Vendor", "Acme Corp") })
        {
            Assert.Equal("text", result.Answers[id].Type);
            Assert.Equal(text, result.Answers[id].Text);
            Assert.Equal(0.9, result.Answers[id].Confidence);
            Assert.Null(result.Answers[id].Source);
        }

        Assert.Equal("text", result.Answers["vendor"].Type);
        Assert.Null(result.Answers["vendor"].Text);
        Assert.Null(result.Answers["vendor"].Confidence);
        Assert.Equal(5, result.Answers.Count);
    }

    [Fact]
    [UpstreamTest(Prefix + "converts UTF-8 offsets across JSON and Unicode text parts without inventing sources", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Converts_UTF8_offsets_across_JSON_and_Unicode_text_parts_without_inventing_sources()
    {
        var server = NewServer(JsonNode.Parse("""
            {"entities":[
              {"type":"names","text":"Acmé","confidence":0.9,"start":19,"end":24},
              {"type":"names","text":"Zoë","confidence":0.8,"start":29,"end":33},
              {"type":"note","text":"é","confidence":0.9,"start":9,"end":11},
              {"type":"invalid","text":"Acmé","confidence":0.9,"start":20,"end":25}]}
            """)!.AsObject());
        var fields = new Dictionary<string, ExtractionField>
        {
            ["names"] = new("list") { Items = "text" },
            ["note"] = new("text"),
            ["invalid"] = new("text"),
        };
        var result = await Model(server).DoExtractAsync(
            Call([JsonPart("""{"note":"é"}"""), new DecisionModelTextPart("🙂 Acmé and Zoë.")], fields),
            CancellationToken.None);
        Assert.Equal("{\"note\":\"é\"}\n🙂 Acmé and Zoë.", Assert.Single(server.Calls).Json!["messages"]![0]!["content"]!.GetValue<string>());
        var names = result.Answers["names"];
        AssertSource(names.Items![0].Source, "text", 1, 3, 7);
        AssertSource(names.Items[1].Source, "text", 1, 12, 15);
        Assert.Equal("text", result.Answers["note"].Type);
        Assert.Equal("é", result.Answers["note"].Text);
        Assert.Equal(0.9, result.Answers["note"].Confidence);
        Assert.Null(result.Answers["note"].Source);
        Assert.Equal("text", result.Answers["invalid"].Type);
        Assert.Equal("Acmé", result.Answers["invalid"].Text);
        Assert.Equal(0.9, result.Answers["invalid"].Confidence);
        Assert.Null(result.Answers["invalid"].Source);
    }

    [Fact]
    [UpstreamTest(Prefix + "sends a base64 document and returns file sources and OCR text", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Sends_a_base64_document_and_returns_file_sources_and_OCR_text()
    {
        var content = (JsonObject)Content.DeepClone();
        content["ocr_text"] = StateText;
        var server = NewServer(content, nullUsage: true);
        var result = await Model(server).DoExtractAsync(Call([FilePart("application/pdf", bytes: [1, 2, 3])]), CancellationToken.None);
        var body = Assert.Single(server.Calls).Json!;
        Assert.Equal("Extract the requested fields from the document.", body["messages"]![0]!["content"]!.GetValue<string>());
        Assert.Equal("user", body["messages"]![0]!["role"]!.GetValue<string>());
        Assert.Equal("AQID", body["document"]!.GetValue<string>());
        Assert.Equal("application/pdf", body["document_mime_type"]!.GetValue<string>());
        AssertSource(result.Answers["vendor"].Source, "file", 0);
        JsonAssert.Equal(result.ProviderMetadata!.Value, """{"scaledown":{"ocrText":"Acme Corp. Contacts: Ana, Bo."}}""");
        Assert.Equal(42, result.Usage!.InputTokens);
        Assert.Null(result.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects unsupported fields and files before sending a request", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Rejects_unsupported_fields_and_files_before_sending_a_request()
    {
        var server = NewServer();
        var model = Model(server);
        foreach (var field in new[]
        {
            new ExtractionField("number"),
            new ExtractionField("date"),
            new ExtractionField("list") { Items = "number" },
            new ExtractionField("list") { Items = "date" },
        })
        {
            await Assert.ThrowsAsync<ExtractionUnsupportedFieldTypeError>(() => model.DoExtractAsync(
                Call(fields: new Dictionary<string, ExtractionField> { ["value"] = field }),
                CancellationToken.None));
        }

        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => model.DoExtractAsync(
            Call([FilePart("audio/wav", base64: "AQID")]),
            CancellationToken.None));
        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes custom configuration and warns about unsupported settings",
        Coverage = UpstreamCoverage.Partial,
        Commit = UpstreamCommit,
        Note = "HttpClient links the cancellation token, so the test cannot assert that the same token reaches the handler. It asserts that a canceled token stops the call. ApiKey is a string, not an async function.")]
    public async Task Passes_custom_configuration_and_warns_about_unsupported_settings()
    {
        var server = NewServer();
        var provider = ScaleDownProvider.Create(
            new ScaleDownProviderSettings
            {
                ApiKey = "custom-key",
                BaseUrl = "https://custom.example/",
                Headers = new Dictionary<string, string> { ["shared"] = "provider" },
            },
            server);
        var model = provider.ExtractionModel("extract");
        var result = await model.DoExtractAsync(
            Call(headers: new Dictionary<string, string> { ["shared"] = "call" }, referenceDate: "2026-11-12", reasoning: "high", maxOutputTokens: 10),
            CancellationToken.None);
        var call = Assert.Single(server.Calls);
        Assert.Equal("https://custom.example/v1/chat/completions", call.Url);
        Assert.Equal("custom-key", call.Headers["x-api-key"]);
        Assert.Equal("call", call.Headers["shared"]);
        Assert.Collection(
            result.Warnings,
            warning => Assert.Equal(("unsupported", "referenceDate"), (warning.Type, warning.Feature)),
            warning => Assert.Equal(("unsupported", "maxOutputTokens"), (warning.Type, warning.Feature)),
            warning => Assert.Equal(("unsupported", "reasoning"), (warning.Type, warning.Feature)));

        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => model.DoExtractAsync(Call(cancellationToken: canceled.Token), canceled.Token));
    }

    [Fact]
    [UpstreamTest(Prefix + "restores workflow configuration and environment authentication", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Restores_workflow_configuration_and_environment_authentication()
    {
        var server = NewServer();
        var model = Model(server);
        var previous = Environment.GetEnvironmentVariable(ScaleDownProvider.ApiKeyEnvironmentVariable);
        Environment.SetEnvironmentVariable(ScaleDownProvider.ApiKeyEnvironmentVariable, "env-key");
        try
        {
            var serialized = model.Serialize();
            Assert.Equal("extract-1", serialized.ModelId);
            var headers = Assert.IsAssignableFrom<IDictionary<string, object?>>(serialized.Config["headers"]);
            Assert.Equal("test-key", headers["x-api-key"]);
            var restored = ScaleDownExtractionModel.Deserialize(
                new Vercel.AI.ProviderUtils.SerializedModelOptions(serialized.ModelId, new Dictionary<string, object?>
                {
                    ["provider"] = "scaledown.extraction",
                    ["baseURL"] = "https://api.scaledown.xyz",
                }),
                new HttpClient(server, disposeHandler: false));
            await restored.DoExtractAsync(Call(), CancellationToken.None);
            Assert.Equal("env-key", server.Calls[0].Headers["x-api-key"]);
            Assert.Equal(["text", "list"], restored.SupportedFieldTypes);
        }
        finally
        {
            Environment.SetEnvironmentVariable(ScaleDownProvider.ApiKeyEnvironmentVariable, previous);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects malformed inner JSON and surfaces API errors", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Rejects_malformed_inner_JSON_and_surfaces_API_errors()
    {
        foreach (var content in new[] { "not JSON", """{"entities":"invalid"}""" })
        {
            var server = new ScaleDownServer { ResponseBody = ResponseJson(content) };
            var error = await Assert.ThrowsAsync<InvalidResponseDataException>(() => Model(server).DoExtractAsync(Call(), CancellationToken.None));
            Assert.Equal(content, error.Data);
        }

        var failing = new ScaleDownServer { Status = HttpStatusCode.PaymentRequired, ResponseBody = """{"detail":"Insufficient credits"}""" };
        var apiError = await Assert.ThrowsAsync<ApiCallError>(() => Model(failing).DoExtractAsync(Call(), CancellationToken.None));
        Assert.Equal(402, apiError.StatusCode);
        Assert.Equal("Insufficient credits", apiError.Message);
    }
}
