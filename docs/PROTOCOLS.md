# Protocols

SoapUI supports RCC and legacy RBXGS services through separate typed clients. Both use SOAP 1.1 over HTTP, but their endpoint paths, namespaces, action shapes, and response payloads differ.

## Protocol comparison

| | RCC | RBXGS |
|---|---|---|
| Endpoint | `http://<ip>:<port>` | `http://<ip>/RBXGS/WebService.dll` |
| Default | `127.0.0.1:64989` | Host IP; fixed legacy path |
| Envelope | RCC namespaces derived from configured base URL | `urn:Roblox`, using `roblox:` elements |
| Action elements | Usually `ns1:`-qualified | Usually `roblox:`-qualified with legacy child shapes |
| Main resource | Jobs with IDs, leases, categories, and core counts | Environments with IDs and stdout messages |
| Client | `RccServiceClient` | `RbxgsServiceClient` |

`ServiceEndpoint` builds the URL. RCC uses the configured port; RBXGS uses the fixed `/RBXGS/WebService.dll` path. The current implementation constructs `http://` URLs rather than HTTPS URLs. Use only with trusted hosts and networks.[^1]

## Actions

### RCC actions exposed in the UI

`HelloWorld`, `GetVersion`, `GetStatus`, `OpenJob`, `OpenJobEx`, `RenewLease`, `Execute`, `ExecuteEx`, `CloseJob`, `Diag`, `DiagEx`, `GetAllJobs`, `GetAllJobsEx`, `CloseExpiredJobs`, and `CloseAllJobs`.

The client also implements `BatchJob` and `BatchJobEx`, but these are not in the UI action list or dispatch switch.

### RBXGS actions exposed in the UI

`HelloWorld`, `GetVersion`, `GetStatus`, `Execute`, `GetStandardOutMessages`, `GetAllEnvironments`, `OpenEnvironment`, `CloseEnvironment`, `CloseAllEnvironments`, `CloseOrphanedEnvironments`, and `Update`.

## HTTP transport

`SoapTransport.Send` receives a `ServiceEndpoint`, action name, and action body, then:

1. Wraps the body in the RCC or RBXGS SOAP envelope.
2. Creates an HTTP `POST` to the endpoint URL.
3. Sends UTF-8 bytes with `Content-Type: text/xml; charset=utf-8` and `Accept: text/xml`.
4. Adds the `SOAPAction` header and legacy `Cache-Control` / `Pragma: no-cache` headers.
5. Uses a 30-second default timeout; a non-positive custom timeout also uses 30 seconds.
6. Returns the raw response text for parsing by the UI's selected parser.

## Request XML

`SoapEnvelopeBuilder` owns envelope templates and reusable fragments. The clients own the action-specific element names and call the builder for shared job, script, and argument data.

- RCC envelope namespaces are based on the configured base URL. RCC job and script elements use the RCC namespace prefixes.
- RBXGS uses the `urn:Roblox` namespace and its legacy action payload shapes.
- Normal element values use `XmlHelper.Escape`, which delegates to `SecurityElement.Escape`.
- Script text uses `XmlHelper.EscapeScript`: it encodes raw `<`, `>`, and `&`, while preserving entities already present in the text.
- RCC script arguments are emitted as `LuaValue` elements. RBXGS arguments include an item count and array-shaped item list.

Do not concatenate unescaped user-provided values into XML. Add shared serialization behavior to `SoapEnvelopeBuilder` and escaping behavior to `XmlHelper`.

## Response XML

`SoapResponseParser` dispatches on the SOAP action and the selected service mode. It uses local element names in shared lookups because equivalent results may use different namespaces in RCC and RBXGS responses.

Current parser behavior includes:

- RCC `GetStatus` creates a `ServiceStatus`; RBXGS status reads the returned version and environment count.
- RBXGS `Execute` formats typed return values and their count.
- `GetAllEnvironments` and `GetStandardOutMessages` produce log lines.
- Some actions currently return generic success text or status messages instead of decoding all response values.
- Malformed XML is reported as an `InvalidOperationException` with the parse exception attached.

Response values are not currently entity-decoded after parsing, and RCC `LuaValue[]` responses are not yet rendered as typed log entries.

## References

- [RCC WSDL](https://finobe.lol/Resources/RCCService.wsdl)
- [RCC service background](https://docs.mercs.dev/services/rcc/)
- [RBXGS background](https://uboomblox.miraheze.org/wiki/RBXGS)
- [SOAP specification overview](https://en.wikipedia.org/wiki/SOAP#Specification)
- [RCC service client](../Services/Soap/RccServiceClient.cs)
- [RBXGS service client](../Services/Soap/RbxgsServiceClient.cs)
- [Envelope builder](../Services/Soap/SoapEnvelopeBuilder.cs)
- [Transport](../Services/Soap/SoapTransport.cs)

[^1]: The URL helpers in [`ServiceEndpoint`](../Models/ServiceEndpoint.cs) currently hard-code the `http` scheme.