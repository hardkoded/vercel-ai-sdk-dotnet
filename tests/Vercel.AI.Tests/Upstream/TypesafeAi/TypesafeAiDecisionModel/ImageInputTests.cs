// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.TypeSafeAi;
using Vercel.AI.Util;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.TypesafeAi.TypesafeAiDecisionModel.TypesafeAiDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.TypesafeAi.TypesafeAiDecisionModel;

/// <summary>Port of <c>typesafe-ai-decision-model.test.ts</c> &gt; <c>image input</c>.</summary>
public sealed class ImageInputTests
{
    private const string Prefix = "packages/typesafe-ai/src/typesafe-ai-decision-model.test.ts::image input::";

    private static Task<DecisionModelResult> Decide(TypeSafeAiServer server, object? state, Dictionary<string, DecisionQuestion>? questions = null)
    {
        return CreateProvider(server).DecisionModel("jev-latest").DoDecideAsync(Call(questions, state), CancellationToken.None);
    }

    [Theory]
    [InlineData("[animal-photo]")]
    [InlineData("__proto__")]
    [InlineData("constructor")]
    [InlineData("request")]
    [UpstreamTest(Prefix + "uses an explicit reference in assets, state, instructions, and criteria: %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_an_explicit_reference_in_assets_state_instructions_and_criteria(string reference)
    {
        var server = Server();
        var questions = new Dictionary<string, DecisionQuestion>
        {
            ["cat"] = Q(
                "boolean",
                "Does " + reference + " show a cat?",
                new Dictionary<string, object?> { ["true"] = new Dictionary<string, object?> { ["image"] = reference } }),
        };
        var result = await Decide(
            server,
            new List<object?>
            {
                FilePart("image/png", "iVBORw==", "unrelated-name.png", reference, withReference: true),
                FilePart("image/jpeg", "/9j/4A=="),
            },
            questions);
        var call = Assert.Single(server.Calls);
        var request = JsonNode.Parse(call.Form.Single(part => part.Name == "request").Text)!.AsObject();
        var assets = request["assets"]!.AsObject();
        Assert.Equal(2, assets.Count);
        var generated = assets.Select(pair => pair.Key).Single(key => key != reference);
        Assert.Equal(reference + "\n" + generated, request["state"]!.GetValue<string>());
        Assert.Equal("image", assets[reference]!["type"]!.GetValue<string>());
        Assert.Equal("image-0", assets[reference]!["part"]!.GetValue<string>());
        Assert.Equal("unrelated-name.png", call.Form.Single(part => part.Name == "image-0").FileName);
        var cat = request["questions"]!["cat"]!;
        Assert.Equal("noul", cat["type"]!.GetValue<string>());
        Assert.Equal("Does " + reference + " show a cat?", cat["instructions"]!.GetValue<string>());
        Assert.Equal(reference, cat["criteria"]!["true"]!["image"]!.GetValue<string>());
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("\"\"")]
    [InlineData("\" \\t\\n\"")]
    [InlineData("123")]
    [InlineData("true")]
    [InlineData("null")]
    [UpstreamTest(Prefix + "rejects invalid references before HTTP: %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_references_before_HTTP(string referenceJson)
    {
        var server = Server();
        using var document = System.Text.Json.JsonDocument.Parse(referenceJson);
        object? reference = document.RootElement.ValueKind switch
        {
            System.Text.Json.JsonValueKind.String => document.RootElement.GetString(),
            System.Text.Json.JsonValueKind.Number => document.RootElement.GetDouble(),
            System.Text.Json.JsonValueKind.True => true,
            _ => null,
        };
        await Assert.ThrowsAsync<InvalidArgumentError>(() => Decide(
            server,
            new List<object?> { FilePart("image/png", "iVBORw==", reference: reference, withReference: true) }));
        Assert.Empty(server.Calls);
    }

    private static JsonObject Request(TypeSafeAiServer server)
    {
        return JsonNode.Parse(Assert.Single(server.Calls).Form.Single(part => part.Name == "request").Text)!.AsObject();
    }

    [Fact]
    [UpstreamTest(Prefix + "keeps JSON state as an object when it already contains the reference", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_JSON_state_as_an_object_when_it_already_contains_the_reference()
    {
        var server = Server();
        await Decide(
            server,
            new List<object?>
            {
                JsonPart(new Dictionary<string, object?> { ["photo"] = "[photo]", ["note"] = "Box arrived wet." }),
                FilePart("image/png", "iVBORw==", reference: "[photo]", withReference: true),
            });
        var call = Assert.Single(server.Calls);
        var request = Request(server);
        Assert.Equal("[photo]", request["state"]!["photo"]!.GetValue<string>());
        Assert.Equal("Box arrived wet.", request["state"]!["note"]!.GetValue<string>());
        Assert.Equal(2, request["state"]!.AsObject().Count);
        var assets = request["assets"]!.AsObject();
        Assert.Single(assets);
        Assert.Equal("image", assets["[photo]"]!["type"]!.GetValue<string>());
        Assert.Equal("image-1", assets["[photo]"]!["part"]!.GetValue<string>());
        Assert.Equal(new[] { "request", "image-1" }, call.Form.Select(part => part.Name).ToArray());
        Assert.Equal("true", call.Headers["typesafe-danger-zone-images"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "leaves text state unchanged when it already contains the reference", Coverage = UpstreamCoverage.Covered)]
    public async Task Leaves_text_state_unchanged_when_it_already_contains_the_reference()
    {
        var server = Server();
        await Decide(
            server,
            new List<object?>
            {
                TextPart("Is [photo] damaged?"),
                FilePart("image/jpeg", "/9j/4A==", reference: "[photo]", withReference: true),
            });
        var request = Request(server);
        Assert.Equal("Is [photo] damaged?", request["state"]!.GetValue<string>());
        Assert.Equal(new[] { "[photo]" }, request["assets"]!.AsObject().Select(pair => pair.Key).ToArray());
    }

    [Fact]
    [UpstreamTest(Prefix + "finds references in nested JSON string values but not in object keys", Coverage = UpstreamCoverage.Covered)]
    public async Task Finds_references_in_nested_JSON_string_values_but_not_in_object_keys()
    {
        var server = Server();
        var value = new Dictionary<string, object?>
        {
            ["frames"] = new List<object?>
            {
                new Dictionary<string, object?> { ["seconds_ago"] = 0, ["frame"] = "Latest frame: [frame]" },
            },
            ["[label]"] = "A key is not a reference.",
        };
        await Decide(
            server,
            new List<object?>
            {
                JsonPart(value),
                FilePart("image/png", "iVBORw==", reference: "[frame]", withReference: true),
                FilePart("image/png", "iVBORw==", reference: "[label]", withReference: true),
            });
        var request = Request(server);
        Assert.Equal(
            "{\"frames\":[{\"seconds_ago\":0,\"frame\":\"Latest frame: [frame]\"}],\"[label]\":\"A key is not a reference.\"}\n[label]",
            request["state"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "sends a question-only image without touching state when insertReference is false", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_a_question_only_image_without_touching_state_when_insertReference_is_false()
    {
        var server = Server();
        var questions = new Dictionary<string, DecisionQuestion>
        {
            ["same"] = Q("boolean", "Is the dog in `photo` the dog in [poster]?"),
        };
        await Decide(
            server,
            new List<object?>
            {
                JsonPart(new Dictionary<string, object?> { ["photo"] = "[found]" }),
                FilePart("image/jpeg", "/9j/4A==", reference: "[found]", withReference: true),
                FilePart("image/jpeg", "/9j/4A==", reference: "[poster]", withReference: true, insertReference: false),
            },
            questions);
        var request = Request(server);
        Assert.Equal("[found]", request["state"]!["photo"]!.GetValue<string>());
        Assert.Single(request["state"]!.AsObject());
        Assert.Equal(new[] { "[found]", "[poster]" }, request["assets"]!.AsObject().Select(pair => pair.Key).ToArray());
    }

    [Fact]
    [UpstreamTest(Prefix + "inserts a reference already in state when insertReference is true", Coverage = UpstreamCoverage.Covered)]
    public async Task Inserts_a_reference_already_in_state_when_insertReference_is_true()
    {
        var server = Server();
        await Decide(
            server,
            new List<object?>
            {
                TextPart("Before: [photo]"),
                FilePart("image/png", "iVBORw==", reference: "[photo]", withReference: true, insertReference: true),
            });
        Assert.Equal("Before: [photo]\n[photo]", Request(server)["state"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "inserts only the references that are not already placed", Coverage = UpstreamCoverage.Covered)]
    public async Task Inserts_only_the_references_that_are_not_already_placed()
    {
        var server = Server();
        await Decide(
            server,
            new List<object?>
            {
                TextPart("Compare [before] with the new photo."),
                FilePart("image/png", "iVBORw==", reference: "[before]", withReference: true, insertReference: false),
                FilePart("image/png", "iVBORw==", reference: "[after]", withReference: true),
            });
        var request = Request(server);
        Assert.Equal("Compare [before] with the new photo.\n[after]", request["state"]!.GetValue<string>());
        var assets = request["assets"]!.AsObject();
        Assert.Equal("image-1", assets["[before]"]!["part"]!.GetValue<string>());
        Assert.Equal("image-2", assets["[after]"]!["part"]!.GetValue<string>());
        Assert.Equal(2, assets.Count);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects insertReference: false without an explicit reference before HTTP", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_insertReference_false_without_an_explicit_reference_before_HTTP()
    {
        var server = Server();
        var error = await Assert.ThrowsAsync<InvalidArgumentError>(() => Decide(
            server,
            new List<object?> { FilePart("image/png", "iVBORw==", insertReference: false) }));
        Assert.Equal("providerOptions.typesafe.reference", error.Parameter);
        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects duplicate image references before HTTP", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_duplicate_image_references_before_HTTP()
    {
        var server = Server();
        var error = await Assert.ThrowsAsync<InvalidArgumentError>(() => Decide(
            server,
            new List<object?>
            {
                FilePart("image/png", "iVBORw==", reference: "[photo]", withReference: true),
                FilePart("image/png", "iVBORw==", reference: "[photo]", withReference: true),
            }));
        Assert.Equal("providerOptions.typesafe.reference", error.Parameter);
        Assert.Contains("Duplicate TypeSafe image reference: [photo]", error.Message);
        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "maps mixed text, JSON, and multiple images to referenced assets", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_mixed_text_JSON_and_multiple_images_to_referenced_assets()
    {
        var server = Server();
        await Decide(
            server,
            new List<object?>
            {
                TextPart("Compare these images."),
                FilePart("image/png", new byte[] { 137, 80, 78, 71 }),
                JsonPart(new Dictionary<string, object?> { ["label"] = "second" }),
                FilePart("image/jpeg", "/9j/4A=="),
            });
        var call = Assert.Single(server.Calls);
        var request = JsonNode.Parse(call.Form.Single(part => part.Name == "request").Text)!.AsObject();
        var assets = request["assets"]!.AsObject();
        Assert.Equal(2, assets.Count);
        var references = assets.Select(pair => pair.Key).ToList();
        Assert.NotEqual(references[0], references[1]);
        Assert.Equal("image-1", assets[references[0]]!["part"]!.GetValue<string>());
        Assert.Equal("image-3", assets[references[1]]!["part"]!.GetValue<string>());
        Assert.Equal(
            "Compare these images.\n" + references[0] + "\n{\"label\":\"second\"}\n" + references[1],
            request["state"]!.GetValue<string>());
        Assert.Equal(new[] { "request", "image-1", "image-3" }, call.Form.Select(part => part.Name).ToArray());
        Assert.Equal("application/json", call.Form[0].ContentType);
        var png = call.Form[1];
        Assert.Equal("image/png", png.ContentType);
        Assert.Equal("image-1.png", png.FileName);
        Assert.Equal(new byte[] { 137, 80, 78, 71 }, png.Bytes);
        var jpeg = call.Form[2];
        Assert.Equal("image/jpeg", jpeg.ContentType);
        Assert.Equal(new byte[] { 255, 216, 255, 224 }, jpeg.Bytes);
        Assert.StartsWith("multipart/form-data; boundary=", call.Headers["content-type"]);
        Assert.Equal("true", call.Headers["typesafe-danger-zone-images"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "omits assets and the image header for text-only input", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_assets_and_the_image_header_for_text_only_input()
    {
        var server = Server();
        await Decide(server, JsonState(ToObject(NativeRequest["state"])));
        var call = Assert.Single(server.Calls);
        Assert.False(JsonNode.Parse(call.Body)!.AsObject().ContainsKey("assets"));
        Assert.False(call.Headers.ContainsKey("typesafe-danger-zone-images"));
    }

    [Fact]
    [UpstreamTest(Prefix + "uses fresh references for each request", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_fresh_references_for_each_request()
    {
        var server = Server();
        await Decide(server, new List<object?> { FilePart("image/png", "iVBORw==") });
        await Decide(server, new List<object?> { FilePart("image/png", "iVBORw==") });
        var states = server.Calls
            .Select(call => JsonNode.Parse(call.Form.Single(part => part.Name == "request").Text)!["state"]!.GetValue<string>())
            .ToList();
        Assert.Equal(2, states.Count);
        Assert.NotEqual(states[0], states[1]);
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/webp")]
    [InlineData("image/gif")]
    [InlineData("image")]
    [UpstreamTest(Prefix + "rejects unsupported or unrecognized media before HTTP: %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_or_unrecognized_media_before_HTTP(string mediaType)
    {
        var server = Server();
        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => Decide(
            server,
            new List<object?> { FilePart(mediaType, "iVBORw==") }));
        Assert.Equal("TypeSafe AI decision image media type: " + mediaType, error.Functionality);
        Assert.Empty(server.Calls);
    }

    public static IEnumerable<object[]> UnsupportedFileData()
    {
        yield return new object[] { new Dictionary<string, object?> { ["type"] = "url", ["url"] = new Uri("https://example.com/image.png") }, "url" };
        yield return new object[]
        {
            new Dictionary<string, object?>
            {
                ["type"] = "reference",
                ["reference"] = new Dictionary<string, object?> { ["typesafe"] = "file-id" },
            },
            "reference",
        };
        yield return new object[] { new Dictionary<string, object?> { ["type"] = "text", ["text"] = "not an image" }, "text" };
    }

    [Theory]
    [MemberData(nameof(UnsupportedFileData))]
    [UpstreamTest(Prefix + "rejects unsupported file data before HTTP: $type", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unsupported_file_data_before_HTTP(Dictionary<string, object?> data, string dataType)
    {
        var server = Server();
        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => Decide(
            server,
            new List<object?> { FilePart("image/png", data) }));
        Assert.Equal("TypeSafe AI decision file input: image/png (" + dataType + ")", error.Functionality);
        Assert.Empty(server.Calls);
    }
}
