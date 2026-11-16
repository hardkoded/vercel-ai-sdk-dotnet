// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Mapper = global::Vercel.AI.Google.GoogleRealtimeEventMapper;

namespace Vercel.AI.Tests.Upstream.Google.GoogleRealtimeEventMapper;

/// <summary>Port of <c>google-realtime-event-mapper.test.ts</c> &gt; <c>buildGoogleSessionConfig</c>.</summary>
public sealed class BuildGoogleSessionConfigTests
{
    private const string Id = "packages/google/src/realtime/google-realtime-event-mapper.test.ts::buildGoogleSessionConfig::";

    [Fact]
    [UpstreamTest(Id + "builds config with model path", Coverage = UpstreamCoverage.Covered)]
    public void Builds_config_with_model_path()
    {
        var result = Mapper.BuildGoogleSessionConfig(null, "gemini-2.0-flash");

        GoogleUpstream.JsonEqual(result, """{"generationConfig":{"responseModalities":["AUDIO"]},"model":"models/gemini-2.0-flash"}""");
    }

    [Fact]
    [UpstreamTest(Id + "builds config with instructions and voice", Coverage = UpstreamCoverage.Covered)]
    public void Builds_config_with_instructions_and_voice()
    {
        var result = Mapper.BuildGoogleSessionConfig(new RealtimeModelV4SessionConfig { Instructions = "Be helpful", Voice = "Puck" }, "gemini-2.0-flash");

        GoogleUpstream.JsonEqual(
            result,
            """
            {"generationConfig":{"responseModalities":["AUDIO"],"speechConfig":{"voiceConfig":{"prebuiltVoiceConfig":{"voiceName":"Puck"}}}},
            "model":"models/gemini-2.0-flash","systemInstruction":{"parts":[{"text":"Be helpful"}]}}
            """);
    }

    [Fact]
    [UpstreamTest(Id + "builds config with tools", Coverage = UpstreamCoverage.Covered)]
    public void Builds_config_with_tools()
    {
        var result = Mapper.BuildGoogleSessionConfig(
            new RealtimeModelV4SessionConfig
            {
                Tools =
                [
                    new RealtimeModelV4ToolDefinition("getWeather", "Get weather", GoogleUpstream.Element("""{"type":"object","properties":{"city":{"type":"string"}},"required":["city"]}""")),
                ],
            },
            "gemini-2.0-flash");

        GoogleUpstream.JsonEqual(
            result["tools"],
            """
            [{"functionDeclarations":[{"description":"Get weather","name":"getWeather",
            "parametersJsonSchema":{"properties":{"city":{"type":"string"}},"required":["city"],"type":"object"}}]}]
            """);
    }

    [Fact]
    [UpstreamTest(Id + "builds config with preserved local JSON Schema references", Coverage = UpstreamCoverage.Covered)]
    public void Builds_config_with_preserved_local_JSON_Schema_references()
    {
        var schema = """{"type":"object","properties":{"locale":{"$ref":"#/$defs/Locale"}},"required":["locale"],"$defs":{"Locale":{"type":"string","enum":["de","en"]}}}""";
        var result = Mapper.BuildGoogleSessionConfig(
            new RealtimeModelV4SessionConfig { Tools = [new RealtimeModelV4ToolDefinition("formatDate", "Format a date", GoogleUpstream.Element(schema))] },
            "gemini-2.0-flash");

        GoogleUpstream.JsonEqual(
            result["tools"],
            "[{\"functionDeclarations\":[{\"name\":\"formatDate\",\"description\":\"Format a date\",\"parametersJsonSchema\":" + schema + "}]}]");
    }

    [Fact]
    [UpstreamTest(Id + "maps output modalities to uppercase", Coverage = UpstreamCoverage.Covered)]
    public void Maps_output_modalities_to_uppercase()
    {
        var result = Mapper.BuildGoogleSessionConfig(new RealtimeModelV4SessionConfig { OutputModalities = ["audio", "text"] }, "model");

        GoogleUpstream.JsonEqual(result["generationConfig"]!["responseModalities"], """["AUDIO","TEXT"]""");
    }

    [Fact]
    [UpstreamTest(Id + "enables input audio transcription", Coverage = UpstreamCoverage.Covered)]
    public void Enables_input_audio_transcription()
    {
        var result = Mapper.BuildGoogleSessionConfig(new RealtimeModelV4SessionConfig { InputAudioTranscription = new RealtimeModelV4AudioTranscription() }, "model");

        GoogleUpstream.JsonEqual(result["inputAudioTranscription"], "{}");
    }

    [Fact]
    [UpstreamTest(Id + "enables output audio transcription", Coverage = UpstreamCoverage.Covered)]
    public void Enables_output_audio_transcription()
    {
        var result = Mapper.BuildGoogleSessionConfig(new RealtimeModelV4SessionConfig { OutputAudioTranscription = new RealtimeModelV4AudioTranscription() }, "model");

        GoogleUpstream.JsonEqual(result["outputAudioTranscription"], "{}");
    }

    [Fact]
    [UpstreamTest(Id + "maps providerOptions.google.translationConfig to generationConfig", Coverage = UpstreamCoverage.Covered)]
    public void Maps_providerOptions_google_translationConfig_to_generationConfig()
    {
        var result = Mapper.BuildGoogleSessionConfig(
            Options("""{"google":{"translationConfig":{"targetLanguageCode":"es","echoTargetLanguage":true}}}"""),
            "gemini-3.5-live-translate-preview");

        GoogleUpstream.JsonEqual(
            result,
            """
            {"model":"models/gemini-3.5-live-translate-preview",
            "generationConfig":{"responseModalities":["AUDIO"],"translationConfig":{"targetLanguageCode":"es","echoTargetLanguage":true}}}
            """);
    }

    [Fact]
    [UpstreamTest(Id + "merges translation config into raw generationConfig provider options", Coverage = UpstreamCoverage.Covered)]
    public void Merges_translation_config_into_raw_generationConfig_provider_options()
    {
        var result = Mapper.BuildGoogleSessionConfig(
            Options("""{"generationConfig":{"responseModalities":["AUDIO"],"temperature":0.2},"google":{"translationConfig":{"targetLanguageCode":"fr"}}}"""),
            "gemini-3.5-live-translate-preview");

        GoogleUpstream.JsonEqual(
            result["generationConfig"],
            """{"responseModalities":["AUDIO"],"temperature":0.2,"translationConfig":{"targetLanguageCode":"fr"}}""");
    }

    [Fact]
    [UpstreamTest(Id + "maps providerOptions.google.thinkingConfig to generationConfig", Coverage = UpstreamCoverage.Covered)]
    public void Maps_providerOptions_google_thinkingConfig_to_generationConfig()
    {
        var result = Mapper.BuildGoogleSessionConfig(
            Options("""{"google":{"thinkingConfig":{"thinkingLevel":"high","includeThoughts":true}}}"""),
            "gemini-3.8-live-extended-thinking");

        GoogleUpstream.JsonEqual(
            result,
            """
            {"model":"models/gemini-3.8-live-extended-thinking",
            "generationConfig":{"responseModalities":["AUDIO"],"thinkingConfig":{"thinkingLevel":"high","includeThoughts":true}}}
            """);
    }

    [Fact]
    [UpstreamTest(Id + "defaults thinkingLevel to low on background-reasoning Live models", Coverage = UpstreamCoverage.Covered)]
    public void Defaults_thinkingLevel_to_low_on_background_reasoning_Live_models()
    {
        foreach (var modelId in new[] { "gemini-3.8-live-extended-thinking", "models/gemini-3.8-live-extended-thinking" })
        {
            GoogleUpstream.JsonEqual(
                Mapper.BuildGoogleSessionConfig(null, modelId),
                """
                {"model":"models/gemini-3.8-live-extended-thinking",
                "generationConfig":{"responseModalities":["AUDIO"],"thinkingConfig":{"thinkingLevel":"low"}}}
                """);
        }

        GoogleUpstream.JsonEqual(
            Mapper.BuildGoogleSessionConfig(
                new RealtimeModelV4SessionConfig { OutputModalities = ["audio"], Instructions = "Be brief." },
                "gemini-3.8-live-extended-thinking")["generationConfig"],
            """{"responseModalities":["AUDIO"],"thinkingConfig":{"thinkingLevel":"low"}}""");
    }

    [Fact]
    [UpstreamTest(Id + "does not default thinkingConfig on Live models without background reasoning", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_default_thinkingConfig_on_Live_models_without_background_reasoning()
    {
        foreach (var modelId in new[] { "gemini-3.8-live", "gemini-3.1-flash-live-preview", "gemini-3.5-live-translate-preview" })
        {
            GoogleUpstream.JsonEqual(
                Mapper.BuildGoogleSessionConfig(new RealtimeModelV4SessionConfig { OutputModalities = ["audio"] }, modelId)["generationConfig"],
                """{"responseModalities":["AUDIO"]}""");
        }
    }

    [Fact]
    [UpstreamTest(Id + "does not add a default thinkingLevel when thinkingBudget is set", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_add_a_default_thinkingLevel_when_thinkingBudget_is_set()
    {
        var result = Mapper.BuildGoogleSessionConfig(
            Options("""{"google":{"thinkingConfig":{"thinkingBudget":256}}}"""),
            "gemini-3.8-live-extended-thinking");

        GoogleUpstream.JsonEqual(result["generationConfig"], """{"responseModalities":["AUDIO"],"thinkingConfig":{"thinkingBudget":256}}""");
    }

    [Fact]
    [UpstreamTest(Id + "keeps the default thinkingLevel under a raw generationConfig provider option", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_the_default_thinkingLevel_under_a_raw_generationConfig_provider_option()
    {
        var result = Mapper.BuildGoogleSessionConfig(
            Options("""{"generationConfig":{"temperature":0.2}}"""),
            "gemini-3.8-live-extended-thinking");

        GoogleUpstream.JsonEqual(result["generationConfig"], """{"temperature":0.2,"thinkingConfig":{"thinkingLevel":"low"}}""");
    }

    [Fact]
    [UpstreamTest(Id + "merges thinking config into raw generationConfig provider options", Coverage = UpstreamCoverage.Covered)]
    public void Merges_thinking_config_into_raw_generationConfig_provider_options()
    {
        var result = Mapper.BuildGoogleSessionConfig(
            Options("""{"generationConfig":{"responseModalities":["AUDIO"],"temperature":0.2},"google":{"thinkingConfig":{"thinkingBudget":512}}}"""),
            "gemini-3.8-live-extended-thinking");

        GoogleUpstream.JsonEqual(
            result["generationConfig"],
            """{"responseModalities":["AUDIO"],"temperature":0.2,"thinkingConfig":{"thinkingBudget":512}}""");
    }

    [Fact]
    [UpstreamTest(Id + "merges translation and thinking config together into generationConfig", Coverage = UpstreamCoverage.Covered)]
    public void Merges_translation_and_thinking_config_together_into_generationConfig()
    {
        var result = Mapper.BuildGoogleSessionConfig(
            Options("""{"google":{"translationConfig":{"targetLanguageCode":"fr"},"thinkingConfig":{"thinkingBudget":1024}}}"""),
            "gemini-3.8-live-extended-thinking");

        GoogleUpstream.JsonEqual(
            result["generationConfig"],
            """{"responseModalities":["AUDIO"],"translationConfig":{"targetLanguageCode":"fr"},"thinkingConfig":{"thinkingBudget":1024}}""");
    }

    [Fact]
    [UpstreamTest(Id + "adds the default thinkingLevel when thinkingConfig sets neither thinkingLevel nor thinkingBudget", Coverage = UpstreamCoverage.Covered)]
    public void Adds_the_default_thinkingLevel_when_thinkingConfig_sets_neither_thinkingLevel_nor_thinkingBudget()
    {
        foreach (var modelId in new[] { "gemini-3.8-live-extended-thinking", "models/gemini-3.8-live-extended-thinking" })
        {
            GoogleUpstream.JsonEqual(
                Mapper.BuildGoogleSessionConfig(Options("""{"google":{"thinkingConfig":{"includeThoughts":true}}}"""), modelId)["generationConfig"],
                """{"responseModalities":["AUDIO"],"thinkingConfig":{"includeThoughts":true,"thinkingLevel":"low"}}""");
        }

        // `{ thinkingLevel: undefined }` has no thinkingLevel on the wire, which `{}` and an explicit null both mirror.
        foreach (var thinkingConfig in new[] { "{}", """{"thinkingLevel":null}""" })
        {
            GoogleUpstream.JsonEqual(
                Mapper.BuildGoogleSessionConfig(Options("{\"google\":{\"thinkingConfig\":" + thinkingConfig + "}}"), "gemini-3.8-live-extended-thinking")["generationConfig"],
                """{"responseModalities":["AUDIO"],"thinkingConfig":{"thinkingLevel":"low"}}""");
        }
    }

    [Fact]
    [UpstreamTest(Id + "keeps thinkingBudget: 0 without adding a default thinkingLevel", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_thinkingBudget_0_without_adding_a_default_thinkingLevel()
    {
        var result = Mapper.BuildGoogleSessionConfig(
            Options("""{"google":{"thinkingConfig":{"thinkingBudget":0}}}"""),
            "gemini-3.8-live-extended-thinking");

        GoogleUpstream.JsonEqual(result["generationConfig"], """{"responseModalities":["AUDIO"],"thinkingConfig":{"thinkingBudget":0}}""");
    }

    [Fact]
    [UpstreamTest(Id + "does not overwrite a raw generationConfig.thinkingConfig that sets thinkingLevel or thinkingBudget", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_overwrite_a_raw_generationConfig_thinkingConfig_that_sets_thinkingLevel_or_thinkingBudget()
    {
        foreach (var thinkingConfig in new[] { """{"thinkingLevel":"high"}""", """{"thinkingBudget":1024,"includeThoughts":true}""" })
        {
            GoogleUpstream.JsonEqual(
                Mapper.BuildGoogleSessionConfig(
                    Options("{\"generationConfig\":{\"responseModalities\":[\"AUDIO\"],\"thinkingConfig\":" + thinkingConfig + "}}"),
                    "gemini-3.8-live-extended-thinking")["generationConfig"],
                "{\"responseModalities\":[\"AUDIO\"],\"thinkingConfig\":" + thinkingConfig + "}");
        }
    }

    [Fact]
    [UpstreamTest(Id + "adds the default thinkingLevel to a raw generationConfig.thinkingConfig that sets neither", Coverage = UpstreamCoverage.Covered)]
    public void Adds_the_default_thinkingLevel_to_a_raw_generationConfig_thinkingConfig_that_sets_neither()
    {
        var result = Mapper.BuildGoogleSessionConfig(
            Options("""{"generationConfig":{"temperature":0.2,"thinkingConfig":{"includeThoughts":true}}}"""),
            "gemini-3.8-live-extended-thinking");

        GoogleUpstream.JsonEqual(
            result["generationConfig"],
            """{"temperature":0.2,"thinkingConfig":{"includeThoughts":true,"thinkingLevel":"low"}}""");
    }

    [Fact]
    [UpstreamTest(Id + "prefers a typed thinkingConfig over a raw generationConfig.thinkingConfig", Coverage = UpstreamCoverage.Covered)]
    public void Prefers_a_typed_thinkingConfig_over_a_raw_generationConfig_thinkingConfig()
    {
        var result = Mapper.BuildGoogleSessionConfig(
            Options("""{"generationConfig":{"thinkingConfig":{"thinkingLevel":"high"}},"google":{"thinkingConfig":{"thinkingBudget":512}}}"""),
            "gemini-3.8-live-extended-thinking");

        GoogleUpstream.JsonEqual(result["generationConfig"], """{"thinkingConfig":{"thinkingBudget":512}}""");
    }

    [Fact]
    [UpstreamTest(Id + "does not add a default thinkingLevel on Live models without background reasoning", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_add_a_default_thinkingLevel_on_Live_models_without_background_reasoning()
    {
        var result = Mapper.BuildGoogleSessionConfig(
            Options("""{"google":{"thinkingConfig":{"includeThoughts":true}}}"""),
            "gemini-3.8-live");

        GoogleUpstream.JsonEqual(result["generationConfig"], """{"responseModalities":["AUDIO"],"thinkingConfig":{"includeThoughts":true}}""");
    }

    [Fact]
    [UpstreamTest(Id + "stamps defaultToolBehavior onto every function declaration", Coverage = UpstreamCoverage.Covered)]
    public void Stamps_defaultToolBehavior_onto_every_function_declaration()
    {
        var config = Options("""{"google":{"defaultToolBehavior":"BLOCKING"}}""");
        config.Tools =
        [
            new RealtimeModelV4ToolDefinition("getWeather", "Get weather", GoogleUpstream.Element("""{"type":"object","properties":{"city":{"type":"string"}},"required":["city"]}""")),
            new RealtimeModelV4ToolDefinition("getTime", null, GoogleUpstream.Element("""{"type":"object","properties":{}}""")),
        ];

        var result = Mapper.BuildGoogleSessionConfig(config, "gemini-3.8-live");

        var declarations = result["tools"]![0]!["functionDeclarations"]!.AsArray();
        Assert.Equal(["BLOCKING", "BLOCKING"], declarations.Select(d => (string)d!["behavior"]!).ToArray());
    }

    [Fact]
    [UpstreamTest(Id + "omits behavior from function declarations when defaultToolBehavior is unset", Coverage = UpstreamCoverage.Covered)]
    public void Omits_behavior_from_function_declarations_when_defaultToolBehavior_is_unset()
    {
        var result = Mapper.BuildGoogleSessionConfig(
            new RealtimeModelV4SessionConfig { Tools = [new RealtimeModelV4ToolDefinition("getWeather", null, GoogleUpstream.Element("""{"type":"object","properties":{}}"""))] },
            "gemini-3.8-live");

        Assert.False(result["tools"]![0]!["functionDeclarations"]![0]!.AsObject().ContainsKey("behavior"));
    }

    [Fact]
    [UpstreamTest(Id + "preserves model path that already includes slash", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_model_path_that_already_includes_slash()
    {
        var result = Mapper.BuildGoogleSessionConfig(null, "models/gemini-2.0-flash");

        Assert.Equal("models/gemini-2.0-flash", (string?)result["model"]);
    }

    private static RealtimeModelV4SessionConfig Options(string providerOptions)
    {
        return new RealtimeModelV4SessionConfig { ProviderOptions = JsonNode.Parse(providerOptions)!.AsObject() };
    }
}
