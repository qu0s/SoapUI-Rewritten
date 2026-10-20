# Data Formats

This guide covers the small domain objects passed between the UI and service layer, and the formats used to save or load script arguments.

## Domain types

| Type | Purpose | Behavior |
|---|---|---|
| `ServiceEndpoint` | Connection details and protocol mode | Trims IP and base URL; builds the RCC or RBXGS URL |
| `ServiceMode` | RCC or RBXGS selection | Determines client, endpoint, envelope, and parser behavior |
| `Job` | RCC job request | Holds ID, expiration seconds, category, and core count; validates ID and non-negative expiration/cores |
| `Script` | Script payload | Holds name, content, arguments, and environment ID; defaults the latter to `RBX0` |
| `LuaArgument` | A typed Lua value | Holds `Type` and an object `Value`; `ValueAsString` is used in SOAP XML |
| `ServiceStatus` | Parsed service status | Holds RCC version and environment count |
| `ResponseOutcome` | Presentation-ready result | Holds optional dialog title/text and log lines |
| `SoapTransportException` | Transport/service failure | Holds action, response body, HTTP status, and parsed SOAP fault details when available |

The model layer has no WinForms dependency. Constructors and `Common.Guard` provide basic validation at input boundaries; the UI is responsible for parsing strings from controls into numeric values.

## Lua argument JSON

JSON is the native export format. `ArgumentFileStore.Save` writes an indented array with the `type` and `value` property names:

```json
[
  {
    "type": "LUA_TSTRING",
    "value": "example"
  },
  {
    "type": "LUA_TNUMBER",
    "value": 42
  }
]
```

`LuaArgument.Value` is an `object`, so imported JSON values can retain their JSON value types. SOAP serialization reads the value through `ValueAsString`.

The application currently lists these supported argument types in `AppConstants`: `LUA_TNIL`, `LUA_TBOOLEAN`, `LUA_TNUMBER`, `LUA_TSTRING`, and `LUA_TTABLE`.

## Lua argument XML import

XML is accepted on import. The expected shape is:

```xml
<arguments>
  <argument>
    <type>LUA_TSTRING</type>
    <value>example</value>
  </argument>
</arguments>
```

Missing `<type>` defaults to `LUA_TNIL`; missing `<value>` becomes an empty string. XML export is not implemented.

## File format detection and I/O

`ArgumentFileStore.Load` reads UTF-8 text and chooses a parser as follows:

| Filename/content | Behavior |
|---|---|
| `.xml` extension | Parse as XML |
| `.json` or `.txt` extension | Parse as JSON |
| Other extension | Treat as XML if trimmed content begins with `<`; otherwise JSON |

`Save` creates the destination directory if needed, serializes JSON with indentation, writes UTF-8 text, and returns the resulting path. File pickers and user interaction stay in `MainUI`; `ArgumentFileStore` owns the file formats.

## Related code

- [LuaArgument](../Models/LuaArgument.cs)
- [Script](../Models/Script.cs)
- [Job](../Models/Job.cs)
- [ServiceEndpoint](../Models/ServiceEndpoint.cs)
- [ArgumentFileStore](../Services/IO/ArgumentFileStore.cs)
- [Lua types and defaults](../Common/AppConstants.cs)