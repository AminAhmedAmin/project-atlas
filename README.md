# Atlas

A company website with an admin dashboard, built with **.NET 10**, **Blazor Web App** (interactive
server), **MudBlazor**, **EF Core 10** (SQL Server) and **ASP.NET Core Identity**.

> **Atlas is a codename.** It is used only for the solution, projects and namespaces. The company
> name, logo and brand color shown on the website are stored in the database and edited from the
> dashboard (**/admin/settings**), with defaults in `appsettings.json`. See
> [Renaming the codename](#renaming-the-codename).

- [Features](#features)
- [Architecture](#architecture)
- [Running locally](#running-locally)
- [Admin credentials](#admin-credentials)
- [Database migrations](#database-migrations)
- [Configuration reference](#configuration-reference)
- [Telegram alerts](#telegram-alerts)
- [Docker](#docker)
- [Hosting without Docker](#hosting-without-docker)
- [Deploying to Azure](#deploying-to-azure)
- [Health checks and logging](#health-checks-and-logging)
- [Tests and CI](#tests-and-ci)
- [Renaming the codename](#renaming-the-codename)

## Features

**Public website**: Home (hero with illustration, key numbers, client logos, services, "How we
work" steps, testimonials, FAQ and call to action), Services, About and Contact.
- All page text, the services list and SEO meta descriptions are editable from the dashboard.
  Editable text can use the `{company}` token, which is replaced with the configured company name.
- The contact form asks for name, e-mail, optional phone, the service of interest, a budget range
  (in SAR) and the message, and stores it in the database. "Ask about this service" links on the
  Services page preselect the service. A honeypot field and a short cooldown stop simple spam, and
  the team gets a [Telegram alert](#telegram-alerts).
- **Portfolio** at `/work` (and `/ar/work`): case studies with cover image, client, tags, results
  and story. Featured ones appear on the home page; the menu link appears once one is published.
- **Live chat**: a chat bubble on every page (English and Arabic). Visitors leave their name, an
  optional e-mail or phone and a message; the team replies from **/admin/chat** and replies appear
  instantly. Conversations survive page reloads (an unguessable token is kept in the browser), and
  visitors are rate-limited. New chats show a badge and a notification in the dashboard and send
  a [Telegram alert](#telegram-alerts).
- A floating **WhatsApp** button appears on every page once a WhatsApp number is set in
  **/admin/settings** (Saudi numbers like `05x xxx xxxx` are converted to `9665…` automatically).
- **English and Arabic.** English pages live at `/…` and Arabic pages at `/ar/…`, with a
  right-to-left layout, an Arabic font (IBM Plex Sans Arabic) and a language switch in the header.
  Each language has its own page text, services and home page sections, edited in the dashboard
  with the **English / العربية** switch. Fixed interface text is translated in
  `src/Atlas.Web/Localization/UiText.cs`.
- SEO basics: per-page `<title>`, meta description, Open Graph tags, canonical URL, `hreflang`
  links between the two languages, `robots.txt` and `sitemap.xml` (both languages). Pages are prerendered on the server, so crawlers get full HTML.
- The layout is responsive and uses the configured logo and primary color everywhere.

**Admin dashboard** (`/admin`, *Admin* role only)
- **Overview**: KPI cards (new messages, total messages, services) and a chart of messages over
  the last 30 days.
- **Messages**: inbox with search, unread filter and paging. Open a message, mark it read or
  unread, reply by e-mail, or delete it.
- **Pages**: edit the text and SEO description of each public page.
- **Home page**: manage the numbers band, client logos (with upload), process steps, testimonials
  and FAQ. FAQ entries are also published as `FAQPage` structured data for Google. Sample
  testimonials and client logos are seeded **hidden**: replace them with real ones before
  publishing.
- **Services**: create, edit, reorder, publish/hide and delete services.
- **Settings**: company name, tagline, contact e-mail, WhatsApp number, primary color, logo upload
  and [Telegram alerts](#telegram-alerts). Changes
  apply to every open page immediately.
- **Portfolio**: create case studies per language with cover upload; mark them published and
  featured. A hidden sample shows the format.
- **Live chat**: conversation list with unread counts, real-time thread, reply, close and delete;
  phone contacts get a one-click WhatsApp link.
- **Users**: list users, create users, and grant or revoke the Admin role. You can't remove your
  own Admin role, and the last admin can't be demoted.
- Sidebar navigation, dark/light mode toggle (remembered per browser) and a responsive layout.

## Architecture

The solution follows Clean Architecture. Dependencies point inwards only, and
`tests/Atlas.Architecture.Tests` enforces this.

```
                ┌──────────────────────────────────────────┐
                │ Atlas.Web  (composition root, Blazor UI) │
                └──────┬────────────────────────┬──────────┘
                       │                        │
        ┌──────────────▼───────┐    ┌───────────▼───────────────┐
        │ Atlas.Infrastructure │───►│ Atlas.Application         │
        │ EF Core, Identity,   │    │ use cases, handlers, DTOs,│
        │ files, alerts, seed  │    │ validation, interfaces    │
        └──────────────────────┘    └───────────┬───────────────┘
                                                │
                                    ┌───────────▼───────────────┐
                                    │ Atlas.Domain              │
                                    │ entities, value objects,  │
                                    │ rules (no dependencies)   │
                                    └───────────────────────────┘
```

| Project | Responsibility |
|---|---|
| `src/Atlas.Domain` | Entities (`SiteSettings`, `PageContent`, `Service`, `ContentBlock`, `ContactMessage`), value objects (`HexColor`, `EmailAddress`) and invariants. Has no package references. |
| `src/Atlas.Application` | One file per use case: a command or query record, an optional validator and a handler. Defines the interfaces for persistence, file storage, team alerts and user administration. Maps to DTOs by hand. |
| `src/Atlas.Infrastructure` | `AtlasDbContext` (Identity + domain tables), repositories, migrations, `UserAdminService` over ASP.NET Core Identity, local file storage, the Telegram Bot API client, the database seeder and the health check. |
| `src/Atlas.Web` | Blazor Web App: public pages, `/admin` dashboard, sign-in pages, DI wiring, health/SEO endpoints. |
| `tests/*` | xUnit v3: domain rules, handlers (with in-memory fakes), infrastructure (SQLite, seeding, migration drift) and architecture rules. |

**Design choices**
- **No MediatR, AutoMapper or FluentAssertions.** Handlers implement `ICommandHandler<TCommand>`,
  `ICommandHandler<TCommand, TResult>` or `IQueryHandler<TQuery, TResult>` and are registered by
  a small reflection scan (`AddApplication()`). Mapping uses explicit `ToDto()` methods.
  Validation uses hand-written `IValidator<T>` classes. An architecture test fails if any of those
  three libraries is referenced.
- **Results, not exceptions.** Commands return `Result`/`Result<T>` with field-level errors that
  the dashboard shows next to the inputs.
- **One DI scope per operation.** Blazor Server circuits are long-lived, so the UI calls handlers
  through `UseCases`, which creates a fresh scope (and `DbContext`) for every call.
- **Branding is data.** `BrandingService` caches the settings and notifies open circuits when they
  change. No page contains hard-coded brand text.
- **Render modes.** Pages are interactive server with prerendering. Sign-in pages under
  `/account` render statically, because they must set cookies.
- **Central package management.** Versions live in `Directory.Packages.props`. Shared build
  settings (net10.0, nullable, warnings as errors, analyzers) live in `Directory.Build.props`.

## Running locally

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download), plus SQL Server
(LocalDB on Windows, or the Docker image anywhere). Restore the local tools once:

```bash
dotnet tool restore        # installs dotnet-ef from dotnet-tools.json
```

### Option A: SQL Server (recommended)

On **Windows with LocalDB**, the default connection string in `appsettings.json` works as is.
On **macOS/Linux**, start SQL Server in Docker and point the app at it:

```bash
docker run -d --name atlas-sql -p 1433:1433 \
  -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='Your-Str0ng-Passw0rd' \
  mcr.microsoft.com/mssql/server:2022-latest

dotnet user-secrets --project src/Atlas.Web set "ConnectionStrings:DefaultConnection" \
  "Server=localhost,1433;Database=Atlas;User Id=sa;Password=Your-Str0ng-Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

Set the admin credentials (see [Admin credentials](#admin-credentials)) and run:

```bash
dotnet run --project src/Atlas.Web
```

In the Development environment, pending migrations are applied at startup
(`Database:ApplyMigrationsOnStartup` is `true` in `appsettings.Development.json`). Open the URL
printed in the console. The dashboard is at `/admin`.

### Option B: quick demo with SQLite (no SQL Server)

```bash
Database__Provider=Sqlite \
ConnectionStrings__DefaultConnection="Data Source=atlas-demo.db" \
Seed__AdminEmail=admin@example.com Seed__AdminPassword='Demo-password-123' \
dotnet run --project src/Atlas.Web
```

SQLite mode creates the schema directly from the model (`EnsureCreated`) instead of running the
SQL Server migrations. Use it for demos and UI work only.

### Option C: everything in Docker

```bash
cp .env.example .env       # edit the passwords
docker compose up --build
```

The site runs at <http://localhost:8080> and the dashboard at <http://localhost:8080/admin>.

## Admin credentials

The first admin is created at startup from configuration. **Never commit real credentials.**
The `Seed` values in `appsettings.json` are intentionally empty.

| Setting | Environment variable | Purpose |
|---|---|---|
| `Seed:AdminEmail` | `Seed__AdminEmail` | E-mail/user name of the initial admin |
| `Seed:AdminPassword` | `Seed__AdminPassword` | Password, used only when that user does not exist yet |

**Locally, use user-secrets:**

```bash
dotnet user-secrets --project src/Atlas.Web set "Seed:AdminEmail" "you@yourcompany.com"
dotnet user-secrets --project src/Atlas.Web set "Seed:AdminPassword" "a-Long-unique-passw0rd"
```

**In Azure / containers**, set the environment variables (App Service → *Configuration*, or Key
Vault references).

How seeding behaves:
- It creates the `Admin` role, then the user if missing, and adds the user to the role. It never
  resets an existing user's password, so after the first start you can remove
  `Seed:AdminPassword`.
- If no admin e-mail is configured, it logs a warning and skips the user.
- Passwords need at least 12 characters, with upper- and lower-case letters and a digit.
- Further users are managed from **/admin/users**. There is no public registration.

Starter settings, page texts, services and home page sections (written for web and mobile app
work in Saudi Arabia) are written **once**, on first run (when
no settings row exists). After that the dashboard owns them. Deleting a starter service does not
bring it back.

## Database migrations

Migrations live in `src/Atlas.Infrastructure/Persistence/Migrations` and target SQL Server. The
EF CLI is pinned in `dotnet-tools.json` (`dotnet tool restore`).

```bash
# Add a migration after changing the model
dotnet ef migrations add <Name> \
  --project src/Atlas.Infrastructure --startup-project src/Atlas.Web \
  --output-dir Persistence/Migrations

# Apply migrations to the database in the current configuration
dotnet ef database update --project src/Atlas.Infrastructure --startup-project src/Atlas.Web

# Produce a self-contained migration bundle for CI/CD
dotnet ef migrations bundle --project src/Atlas.Infrastructure --startup-project src/Atlas.Web \
  --self-contained -o efbundle
./efbundle --connection "<production connection string>"

# Or produce an idempotent SQL script for a DBA to review
dotnet ef migrations script --idempotent \
  --project src/Atlas.Infrastructure --startup-project src/Atlas.Web -o migrate.sql
```

- **Development** applies migrations automatically at startup.
- **Production** does **not** (`ApplyMigrationsOnStartup` is `false`). Run the bundle or script
  as a deployment step, before the new version starts.
- The test `Model_has_no_changes_missing_from_migrations` fails if you change the model and forget
  to add a migration.

## Configuration reference

| Key | Default | Description |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | LocalDB | Database connection string |
| `Database:Provider` | `SqlServer` | `SqlServer` or `Sqlite` (demo only) |
| `Database:ApplyMigrationsOnStartup` | `false` (`true` in Development) | Run `Migrate()` on startup |
| `Branding:CompanyName` | `Atlas` | Default company name until one is saved in the dashboard |
| `Branding:Tagline` | `Web and mobile apps for Saudi businesses.` | Default tagline |
| `Branding:ArabicCompanyName` / `Branding:ArabicTagline` | *(empty)* / Arabic tagline | Defaults for the Arabic site (falls back to English) |
| `Branding:PrimaryColor` | `#1e63e9` | Default brand color (`#RRGGBB`) |
| `Branding:ContactEmail` | *(empty)* | Default contact/notification e-mail |
| `Branding:LogoUrl` | *(empty)* | Optional default logo URL |
| `Seed:AdminEmail` / `Seed:AdminPassword` | *(empty)* | Initial admin, see above |
| `FileStorage:RootPath` | `App_Data/uploads` | Folder for uploaded files, relative to the content root or absolute |
| `FileStorage:RequestPath` | `/uploads` | URL prefix for uploaded files |
| `DataProtection:KeysPath` | *(empty)* | Folder where Data Protection keys are persisted (set in containers) |
| `Telegram:BotToken` | *(empty)* | Bot token from @BotFather (a secret: use user-secrets or `Telegram__BotToken`). Alerts are off while empty |
| `Telegram:SiteUrl` | *(empty)* | Public site address, e.g. `https://example.com`, for "Open dashboard" links in alerts |
| `DisableHttpsRedirection` | `false` | Set `true` when TLS is not available at all (e.g. local compose) |

The `Branding` values are only **defaults**. Once settings are saved in the dashboard, the
database wins.

## Telegram alerts

The team gets a Telegram message for every new contact message, every new live chat, and when a
visitor writes again in a chat the team had already read. No e-mail server is needed.

1. In Telegram, open **@BotFather**, send `/newbot` and follow the steps. Copy the **token**.
2. Give the token to the website as a secret, then restart it:
   - locally: `dotnet user-secrets --project src/Atlas.Web set "Telegram:BotToken" "<token>"`
   - on a host: environment variable `Telegram__BotToken=<token>` (and `Telegram__SiteUrl=https://your-site`).
3. Send any message to your bot in Telegram (or add the bot to a team group and post there).
4. In the dashboard open **Settings → Telegram alerts → Find chats**, pick the chat, and press
   **Send test alert**.

Alerts are best effort: if Telegram is unreachable the message is still saved and shown in the
dashboard. Visitor text is HTML-escaped, and HTTP request logging is disabled for the Telegram
client so the token (which is part of Bot API URLs) never reaches the logs.

## Docker

`src/Atlas.Web/Dockerfile` is a multi-stage build that runs as the non-root `app` user on port
8080. Build it from the repository root:

```bash
docker build -f src/Atlas.Web/Dockerfile -t atlas-web .
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e Seed__AdminEmail=... -e Seed__AdminPassword=... \
  -v atlas-data:/app/App_Data \
  atlas-web
```

`/app/App_Data` holds uploaded files and Data Protection keys, so mount a volume there.
`docker-compose.yml` runs the site together with SQL Server for local testing.

## Hosting without Docker

Docker is optional. The app talks to SQL Server directly through
`ConnectionStrings:DefaultConnection`; Docker is just one convenient way to install the app and
SQL Server on a bare server. Without it:

- **Hosting that includes SQL Server** (Azure App Service + Azure SQL, or Windows/.NET shared
  hosting): run `dotnet publish src/Atlas.Web -c Release -o publish`, upload the `publish` folder,
  and set the connection string and secrets in the host's control panel.
- **Your own Linux server (VPS)**: install the ASP.NET Core 10 runtime and SQL Server (or SQL
  Server Express, which is free) from Microsoft's package repositories, copy the published files,
  run the app as a `systemd` service, and put Caddy or Nginx in front for HTTPS.
- **A Windows server**: install the .NET 10 Hosting Bundle, SQL Server Express and IIS, then
  publish to an IIS site.

In every case, apply migrations with the EF bundle or SQL script described above, and point
`FileStorage:RootPath` and `DataProtection:KeysPath` at folders that survive redeployments.

## Deploying to Azure

A typical setup is **Azure App Service (Linux, container or code)** plus **Azure SQL Database**:

1. Create the Azure SQL database and set `ConnectionStrings__DefaultConnection` in the App
   Service configuration (or use a Key Vault reference / managed identity connection string).
2. Set `Seed__AdminEmail` and `Seed__AdminPassword` for the first start, then remove the password.
3. Run migrations as a release step (`efbundle` or the SQL script, see above).
4. **Enable WebSockets** (App Service → Configuration → General settings). Blazor Server needs
   them, and ARR affinity must stay on if you scale out.
5. Point the App Service **Health check** at `/health`.
6. Persist uploads and keys: mount Azure Files at `/app/App_Data` (containers), or set
   `FileStorage:RootPath` and `DataProtection:KeysPath` to a persistent path such as
   `/home/data/...`. For several instances, consider implementing `IFileStorage` with Azure Blob
   Storage and storing Data Protection keys in Blob Storage/Key Vault.
7. Live chat pushes updates through the server's memory, which suits one instance. If you scale
   out to several instances, add a backplane (e.g. Azure SignalR Service or Redis) for
   `ChatNotifier`.
8. Set `Telegram__BotToken` and `Telegram__SiteUrl` to receive [Telegram alerts](#telegram-alerts).

## Health checks and logging

| Endpoint | Checks | Use |
|---|---|---|
| `GET /health` | App and database (`AtlasDbContext` can connect) | Readiness / Azure health check |
| `GET /health/live` | Process is responding | Liveness |

Both return a small JSON document with status and timings only (no exception details).

Logging uses the built-in `ILogger` with source-generated `LoggerMessage` methods, so every event
is structured (named properties such as `{MessageId}` and `{Email}`). The console writes **JSON**
by default (good for Azure Monitor / container logs), and single-line text in Development. Adjust
levels under `Logging:LogLevel`.

## Tests and CI

```bash
dotnet build
dotnet test
```

| Project | What it covers |
|---|---|
| `Atlas.Domain.Tests` | Entity and value-object rules |
| `Atlas.Application.Tests` | Handlers and validators with in-memory fakes and `FakeTimeProvider` |
| `Atlas.Infrastructure.Tests` | Repositories, seeding, Identity and file storage against SQLite, plus a migration drift check |
| `Atlas.Web.Tests` | Language routing (`/ar/…`) and interface translations |
| `Atlas.Architecture.Tests` | Layer dependency rules (NetArchTest), project references, sealed handlers, no public setters on entities, forbidden libraries |

`.github/workflows/ci.yml` runs restore, build (Release, warnings as errors) and all tests on
every push and pull request, and then builds the Docker image.

## Renaming the codename

Once the company name is chosen, rename the code from `Atlas` to the new name with the included
script. It updates file contents (namespaces, project names, config keys) and renames files and
folders:

```bash
git switch -c rename-codename
scripts/rename-codename.sh Northwind     # PascalCase, letters and digits
dotnet build && dotnet test
git add -A && git commit -m "Rename codename Atlas to Northwind"
```

Notes:
- The **displayed** company name is unaffected. Set it in `/admin/settings`, and optionally
  change the `Branding` defaults in `appsettings.json`.
- The user-secrets id changes, so set your local secrets again.
- The Data Protection application name and cookie name change, so everyone must sign in again.
- Database table names do not contain the codename. Existing databases keep working. If you also
  want to rename the database itself, change it in the connection string.
- Update the Docker image name, CI names and Azure resources to match, if you want.
