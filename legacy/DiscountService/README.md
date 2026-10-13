# Legacy Discount Service — Episode 2 modernization demo

This is the deliberately dated .NET Framework 4.8 service for the
"Modernizing .NET with agents" episode. It exists only on this branch
(`episode-2/legacy-discount-service`); `main` stays the Episode 1 baseline.
Missions migrates this service to .NET 8 with tests passing, live.

What makes it legacy:

- ASP.NET Web API 2 on System.Web, hosted on IIS (`Global.asax`,
  `Web.config`), old-style non-SDK project file, `packages.config`.
- Rules are read from `web.config` appSettings; state is static and
  lock-protected; I/O is synchronous; audit lines are appended to
  `App_Data/discount-audit.log` in a pipe-delimited 2012 batch format.
- `Thread.Sleep(150)` stands in for the retired mainframe round-trip.
- Newtonsoft.Json is pinned at 9.0.1; SQL access goes through
  `System.Data.SqlClient` against `LEGACYSQL01`.

It cannot be built on macOS or Linux (.NET Framework + IIS), by design:
the migration is the demo. What a good migration delivers:

- ASP.NET Core 8 (minimal API or MVC), async end to end.
- Options pattern + `appsettings.json` instead of `ConfigurationManager`.
- `ILogger` / OpenTelemetry instead of the `App_Data` audit file.
- `System.Text.Json` instead of Newtonsoft.Json 9.
- xUnit coverage for the rules: loyalty accrual, preferred-customer tier,
  volume threshold, the 20% cap, and rounding away from zero.
