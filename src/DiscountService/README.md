# Discount service

ASP.NET Core 8 migration of `legacy/DiscountService`. The legacy files remain
unchanged. This project does not reference the repository's net10.0 Aspire
service defaults.

## Calculation contract

Namespace: `DiscountService`.

```csharp
public DiscountCalculator(IOptions<DiscountOptions> options);
public DiscountResult Calculate(string? customerId, decimal amount, int loyaltyYears);
```

The calculator has no I/O. All arithmetic uses `decimal`, except loyalty years
and rounding digits, which are `int`.

1. Start with `LoyaltyYearsPercent * loyaltyYears`.
2. Add `PreferredCustomerPercent` if the non-null customer ID starts with
   `PreferredCustomerPrefix`, using ordinal, case-sensitive comparison.
3. Add `VolumePercent` if `amount >= VolumeThreshold`.
4. If the accumulated percent exceeds `MaxPercent`, replace it with the cap.
5. Return the original customer ID and amount, the unrounded percent, and
   `decimal.Round(amount * (100m - percent) / 100m, RoundingDigits,
   MidpointRounding.AwayFromZero)`.

Only the final discounted amount is rounded. The original amount and percent
are not rounded before calculation. At the default threshold, exactly 100
receives the volume bonus; this differs from the informal "over 100" summary.
Exactly the cap is unchanged.

There is no lower cap or validation of amount or loyalty years. Zero loyalty
years earns no loyalty discount; negative years can produce a negative
discount, and negative amounts remain allowed. A null ID, empty ID, short ID
such as `PRE`, or lowercase `pref` earns no preferred bonus. `PREF` alone does
earn it. Decimal arithmetic overflow still throws. Invalid configured rounding
digits outside 0 through 28 throw when used.

## HTTP contract

Both routes accept GET and POST:

- `/api/discount`
- `/api/discount/{id}`, where the ID is ignored, matching the legacy
  `api/{controller}/{id}` conventional route.

GET reads `customerId` (string), `amount` (decimal), and `loyaltyYears` (int)
from the query string. Null, empty, or whitespace customer IDs return 400.
Amount and loyalty years are required. GET keeps the legacy 150 ms simulated
mainframe delay, implemented with cancellable `Task.Delay`.

POST reads a JSON object with `customerId`, `amount`, and `loyaltyYears`.
A null body returns 400. A non-null object does not validate customer ID:
null, empty, and whitespace IDs are allowed. Omitted customer ID defaults to
null; omitted amount and loyalty years default to zero. Unknown JSON
properties are ignored. Property matching is case-insensitive, and quoted
numeric values are accepted.

Both verbs return 200 and this JSON shape:

```json
{
  "customerId": "PREF-0001",
  "amount": 250.00,
  "discountPercent": 18,
  "discountedAmount": 205.00
}
```

System.Text.Json property attributes preserve the legacy Newtonsoft names.
Null customer IDs remain present as JSON null. Decimal values remain JSON
numbers; textual trailing zeros are not part of the contract.

Malformed JSON or invalid numeric query values return 400. Unsupported POST
content types return 415. Unmatched routes return 404; unsupported methods
return 405. ASP.NET Core supplies framework-level binding and error responses,
so their bodies are not byte-identical to Web API 2's error responses. The
legacy controller's explicit 400 responses had no application error payload.
Web API 2's default formatters also supported XML; this migration intentionally
provides the JSON contract only. No authentication is added.

## Configuration

`DiscountOptions` binds to the `DiscountService` section of `appsettings.json`.
Normal ASP.NET Core configuration overrides work, for example
`DiscountService__MaxPercent=15`.

| Option | Default | Legacy source |
| --- | --- | --- |
| `LoyaltyYearsPercent` | 1 | `Discount:LoyaltyYearsPercent` |
| `PreferredCustomerPercent` | 5 | `Discount:PreferredCustomerPercent` |
| `PreferredCustomerPrefix` | `"PREF"` | Hardcoded ordinal prefix |
| `VolumeThreshold` | 100 | `Discount:VolumeThreshold` |
| `VolumePercent` | 10 | `Discount:VolumePercent` |
| `MaxPercent` | 20 | `Discount:MaxPercent` |
| `RoundingDigits` | 2 | Hardcoded rounding digits |

Rounding direction remains `AwayFromZero`.
The legacy constructor parsed every `Discount:` appSetting on first use,
including unused keys, with current-culture `decimal.Parse`. Options binding
replaces that parsing and the static initialization lock.

`Audit:LogPath` was `~/App_Data/discount-audit.log`. It is intentionally
replaced by logging rather than retained as an unused option.
The `LegacyOrders` connection string was never used by the supplied code;
there is no SQL dependency to migrate.

## Audit

Each successful calculation writes one information-level record through
`ILogger<DiscountCalculator>` after calculation (and after GET's delay).
The message retains the legacy content:

```text
yyyy-MM-dd HH:mm:ss customerId|amount:0.00|discountPercent:0.0|discountedAmount:0.00
```

The timestamp uses local time. The numeric formatting uses the current
culture, as the legacy `DiscountResult.ToString()` did. Null customer IDs
produce an empty first field. The log template stores `AuditTimestamp` and
`AuditRecord` separately. Provider-specific prefixes, line endings, encoding,
and storage replace the old locked `File.AppendAllText` with
`Encoding.Default` and `Environment.NewLine`.

## Build and test

Use the SDK pinned by the repository's `global.json` and an installed ASP.NET
Core 8 runtime:

```sh
dotnet build src/DiscountService/DiscountService.csproj
dotnet build tests/DiscountService.UnitTests/DiscountService.UnitTests.csproj
dotnet test --project tests/DiscountService.UnitTests/DiscountService.UnitTests.csproj
dotnet run --project src/DiscountService/DiscountService.csproj
```

The xUnit project uses the repository's centrally managed
`xunit.v3.mtp-v2` package and Microsoft.Testing.Platform runner.
`Program` is public and partial for HTTP test hosting.
