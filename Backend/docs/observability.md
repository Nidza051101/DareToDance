# Observability

Phase 1 — everything local. OpenTelemetry is the foundation: traces, metrics and logs
are exported via OTLP to a standalone [Aspire dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/standalone)
running in Docker. No cloud dependencies.

All wiring lives in one place: [`ObservabilityExtensions`](../src/DareToDance.Api/Common/Extensions/ObservabilityExtensions.cs)
(`builder.AddObservability()` + `app.UseObservability()` in `Program.cs`). When the export
target changes in Phase 2, that file is the only thing that changes.

## Components

| Concern            | How                                                                                     |
|--------------------|-----------------------------------------------------------------------------------------|
| Logging API        | Serilog (`UseSerilog(..., writeToProviders: true)`), console sink: text in Development, compact JSON elsewhere |
| Log export         | Serilog events flow into the OTel logger provider → OTLP, with `TraceId`/`SpanId` attached |
| HTTP server spans  | `OpenTelemetry.Instrumentation.AspNetCore` (health endpoints filtered out)               |
| Outgoing HTTP      | `OpenTelemetry.Instrumentation.Http`                                                     |
| DB spans           | `Npgsql.OpenTelemetry` (`AddNpgsql()`) — SQL text with `$1`-style placeholders, never parameter values |
| MediatR spans      | `TracingBehavior<,>` — first behavior in the pipeline, one span per command/query        |
| Custom spans       | `DareToDanceDiagnostics.ActivitySource` (source name `DareToDance`)                      |
| Metrics            | ASP.NET Core, HttpClient and .NET runtime instrumentation                                |
| Health checks      | `/health` (liveness), `/health/ready` (readiness incl. PostgreSQL)                       |

## Starting the dashboard

The dashboard is part of `docker-compose.yml`:

```bash
docker compose up -d aspire-dashboard
```

Or standalone, without compose:

```bash
docker run --rm -p 18888:18888 -p 4317:18889 \
  -e DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true \
  mcr.microsoft.com/dotnet/aspire-dashboard:latest
```

- UI: <http://localhost:18888>
- OTLP gRPC ingestion: `localhost:4317`

The dashboard keeps telemetry **in memory** — everything is lost when the container
stops. That is fine for dev.

Run the API as usual (Rider or `dotnet run` in `src/DareToDance.Api`, port 5015) and
telemetry appears under the `daretodance-api` resource.

## Environment variables

| Variable                      | Default                 | Purpose                                             |
|-------------------------------|-------------------------|-----------------------------------------------------|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | `http://localhost:4317` | Where OTLP export goes (all three signals). Inside docker-compose the api container uses `http://aspire-dashboard:18889`. |
| `ASPNETCORE_ENVIRONMENT`      | —                       | Becomes the `deployment.environment` resource attribute and switches the console sink format. |

Resource attributes: `service.name = daretodance-api`, `service.version` = assembly
informational version, `deployment.environment` = ASP.NET Core environment.

Log levels come from the `Serilog` section in `appsettings*.json` (`MinimumLevel` +
`Override`), not from the old `Logging` section. No secrets belong in config files.

## What a healthy trace looks like

`POST /users` with the Aspire dashboard running should produce **one** trace shaped like:

```
POST users                          (HTTP server span — AspNetCore instrumentation)
└── CreateUser                      (MediatR span — TracingBehavior, tag mediatr.request_type = CreateUserCommand)
    ├── SELECT daretodance          (Npgsql span — duplicate-email check)
    └── INSERT daretodance          (Npgsql span — the actual insert)
```

Structured logs in the dashboard (e.g. the Serilog request-completion event) carry the
same `TraceId`, so the Logs view links straight to the trace. The Metrics view shows
`http.server.request.duration`, `process.runtime.dotnet.*` etc. for `daretodance-api`.

Failed commands: the MediatR span gets `Status = Error` plus the exception recorded as
a span event, and the request log line is logged at `Error`.

### Rules

- **No PII in telemetry.** Names, emails and phone numbers must never appear in span
  tags or log properties. Tag IDs, not values. Npgsql spans only ever contain SQL with
  positional placeholders (`$1`), not parameter values.
- Health endpoints (`/health*`) are excluded from traces and request logs by design.
- `tenant.id` will be tagged on the MediatR span and pushed into Serilog's `LogContext`
  once tenant resolution exists (see the `TODO` in `TracingBehavior`) — no tenant
  abstraction before that.

## Health endpoints

| Endpoint        | Checks                          | Meant for                                        |
|-----------------|---------------------------------|--------------------------------------------------|
| `/health`       | none — process is up            | App Service health check (instance recycling)    |
| `/health/ready` | PostgreSQL (`SELECT 1`)         | CI post-deploy verification / rollback gate      |

`/health/ready` returns `503 Service Unavailable` when the database is unreachable.

> **Note:** the repo currently has no GitHub Actions workflow checked in, so there was
> nothing to align these paths with. When the deploy workflow is (re)added, point the
> App Service health check at `/health` and the post-deploy rollback gate at
> `/health/ready`, and update this table if the paths differ.

## Phase 2: Azure Application Insights

Planned change, deliberately kept to a minimal diff in `ObservabilityExtensions`:

1. Add the `Azure.Monitor.OpenTelemetry.AspNetCore` package (the Azure Monitor OTel distro).
2. In `AddObservability`, when `APPLICATIONINSIGHTS_CONNECTION_STRING` is present,
   call `UseAzureMonitor()` on the OpenTelemetry builder (instead of, or alongside,
   `UseOtlpExporter()` — the distro exports all three signals to Application Insights /
   Log Analytics). Locally the variable stays unset and everything keeps flowing to the
   Aspire dashboard.
3. Configure sampling (the distro samples; local OTLP stays always-on) and set a
   **daily ingestion cap** on the Log Analytics workspace so a traffic spike cannot
   produce a surprise bill.

Existing instrumentation, the MediatR behavior, Serilog setup and health checks are
untouched by Phase 2 — only the exporter block changes.
