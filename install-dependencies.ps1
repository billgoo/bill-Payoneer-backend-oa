Write-Host "Begin installing NuGet packages..."

Set-Location OrdersApi.Core
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.VisualStudio.validation
Set-Location ..

Set-Location OrdersApi.API
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
Set-Location ..

Set-Location OrdersApi.Tests
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package xunit.runner.visualstudio
dotnet add package Moq
Set-Location ..

Write-Host "Installation of NuGet packages completed successfully."