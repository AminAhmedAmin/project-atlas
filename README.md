# Atlas

Company website with an admin dashboard. **Atlas** is a codename; the displayed company name,
logo and colors are configured from the dashboard.

Built with .NET 10, Blazor Web App (interactive server), MudBlazor, EF Core 10 (SQL Server) and
ASP.NET Core Identity, following Clean Architecture.

```
src/
  Atlas.Domain          entities, value objects, domain rules (no dependencies)
  Atlas.Application     use cases, handler interfaces, DTOs, validation
  Atlas.Infrastructure  EF Core, Identity, file storage, email
  Atlas.Web             Blazor public site + /admin dashboard (composition root)
tests/
  Atlas.Domain.Tests
  Atlas.Application.Tests
  Atlas.Architecture.Tests   layer dependency rules
```

## Build and test

```bash
dotnet build
dotnet test
```

More documentation will be added as the project grows.
