# Development Setup

## Prerequisites

- .NET SDK 10.0 or later.
- Visual Studio 2026 or Visual Studio Code with C# Dev Kit.

## Commands

```powershell
dotnet restore HoneyBadgerVsWallabee.sln
dotnet build HoneyBadgerVsWallabee.sln
dotnet test HoneyBadgerVsWallabee.sln
dotnet run --project src/Wallabee.API
```

The API uses HTTPS from its launch profile. Set `Wallabee:LabRoot` to the intended isolated laboratory directory before scanning files.

Useful optional tooling: GitHub Copilot, GitHub Copilot Chat, SonarQube or SonarLint, a Markdown editor, and SQL Server Management Studio. SQL Server is the persistence runtime dependency; see DATABASE.md.