# Warnings

Providers return warnings when a call uses an unsupported feature, a compatibility mode, a deprecated setting, or something else worth knowing. `LogWarnings.Log` prints them.

## Turn them off or handle them yourself

```csharp
// Suppress all warnings.
LogWarnings.Logger = false;

// Receive every warning, including repeated deprecations.
LogWarnings.Logger = (Action<LogWarningsOptions>)(options =>
{
    // options.Warnings, options.Provider, options.Model
});
```

Set `LogWarnings.Logger` back to `null` to use the default logger.

## Deprecation warnings

The default logger emits each deprecation code once. Its type is `DeprecationWarning`, and every message carries a stable `AISDK_DEP_*` code. A provider deprecation is scoped to the provider, so the same setting from two models of one provider prints once.

`LogWarnings.ProcessEmitWarning` receives the message, the type, and the code (`null` for ordinary warnings). When it is `null`, `LogWarnings.ConsoleWarn` receives the message with the code in brackets, for example `[AISDK_DEP_GENERATE_OBJECT] ...`.

`Deprecations.GetDeprecationCode(setting, provider)` returns the code for a setting. A setting without a registered code gets `AISDK_DEP_SETTING_` plus the encoded setting name. A provider setting gets `AISDK_DEP_PROVIDER_<provider>__<setting>`. Characters other than `A-Z`, `a-z`, and `0-9` are encoded as `_` plus four hex digits.

| Code | Deprecated usage |
| --- | --- |
| `AISDK_DEP_GENERATE_OBJECT` | `generateObject` |
| `AISDK_DEP_STREAM_OBJECT` | `streamObject` |
| `AISDK_DEP_EXPERIMENTAL_GENERATE_SPEECH` | `experimental_generateSpeech` |
| `AISDK_DEP_EXPERIMENTAL_TRANSCRIBE` | `experimental_transcribe` |
| `AISDK_DEP_IMAGE_CONTENT_PART` | `image` message parts |
| `AISDK_DEP_TOOL_RESULT_*` | legacy tool-result content types |
| `AISDK_DEP_UI_MESSAGE_RAW_INPUT` | `rawInput` in UI tool parts |

Use `LogWarnings.ResetState()` in tests to clear the remembered codes.
