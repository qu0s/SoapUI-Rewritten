# Development

This guide describes how to build the existing project, how its Release packaging works, and where to make changes without crossing layer boundaries.

## Requirements

- Windows with Visual Studio 2019 or 2022 and the .NET desktop workload, or equivalent MSBuild tools.
- .NET Framework 4.7.2 targeting pack.
- NuGet CLI/package restore for dependencies declared in `packages.config`.

The solution is a classic, non-SDK-style project and pins C# to 7.3. Build with a Visual Studio Developer PowerShell:

```powershell
nuget restore SoapUI.sln
msbuild SoapUI.sln /p:Configuration=Debug
```

To produce the optimized Release executable:

```powershell
nuget restore SoapUI.sln
msbuild SoapUI.sln /p:Configuration=Release
```

Open `SoapUI.sln` in Visual Studio and press **F5** for interactive debugging. Outputs are written under `bin\Debug` and `bin\Release` respectively.

## Build configurations

| Configuration | Optimization | Dependency output |
|---|---|---|
| Debug | Disabled | Newtonsoft.Json is a normal copy-local reference |
| Release | Enabled | Newtonsoft.Json is embedded in `SoapUI.exe` and resolved at startup |

The embedded resource is added only in Release. `Program.Main` registers `AssemblyResolve` only when `DEBUG` is not defined, then loads the requested dependency from the executable's manifest resources. The Release output does not require a separate `Newtonsoft.Json.dll`; it still requires the .NET Framework 4.7.2 runtime.

## Adding or changing an action

Keep action behavior within the existing owners:

1. Add the action to the appropriate list in `Common/AppConstants.cs` if it belongs in the UI.
2. Add a typed method in `RccServiceClient` or `RbxgsServiceClient`; validate inputs and send the action body with `SoapTransport`.
3. Add/reuse payload fragments in `SoapEnvelopeBuilder`. Escape values through `XmlHelper`; do not concatenate raw user input into XML.
4. Add the UI dispatch case in `MainUI` and gather user input into domain models there.
5. Add the matching response case in `SoapResponseParser`; return a `ResponseOutcome` rather than showing UI.
6. Verify request XML and response parsing with representative, redacted fixtures when tests are available.

For argument import/export changes, update `ArgumentFileStore`; keep file dialogs and their UI state in `MainUI`.

## Design conventions

- RCC and RBXGS share the HTTP transport but retain distinct service clients, envelope formats, endpoint rules, and response shapes.
- Services and parsers do not display dialogs. `MainUI` owns presentation.
- `SoapEnvelopeBuilder` owns reusable SOAP envelopes and payload fragments.
- `LuaArgument` is the shared model for UI rows, argument files, and request serialization.
- Use typed locals and safe XML helper lookups; avoid introducing `dynamic` into new code.
- Keep compatibility with .NET Framework 4.7.2 and the C# 7.3 language ceiling unless the project target is deliberately migrated.

## Current limitations

- SOAP calls run synchronously and block the UI thread.
- There is no dedicated automated test project yet.
- `GetExpiration` is not implemented in the client/UI.
- RCC `BatchJob` and `BatchJobEx` have client methods but no UI action or dispatch case.
- Some action responses are represented by generic success/log messages rather than fully decoded values.
- RCC `LuaValue[]` response values are not rendered as typed log entries, and response values are not entity-decoded after parsing.
- The project still uses the legacy project format and `packages.config`.

## Potential improvements

- Move network execution to `async`/`await` with cancellation so a slow service does not freeze the form.
- Add tests for `SoapEnvelopeBuilder` escaping and `SoapResponseParser` using redacted XML fixtures.
- Introduce an HTTP abstraction if service clients need to be isolated in unit tests.
- Evaluate an SDK-style project migration while preserving framework compatibility requirements.

## Related references

- [Architecture overview](ARCHITECTURE.md)
- [Request flow](REQUEST-FLOW.md)
- [Protocol behavior](PROTOCOLS.md)
- [Data formats](DATA-FORMATS.md)
- [CI workflow](../.github/workflows/dotnet.yml)