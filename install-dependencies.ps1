Write-Host "Begin installing NuGet packages..."

# Note: Database files are stored in the Shared folder to avoid duplication

Set-Location OrdersApi.Core
dotnet add package Microsoft.EntityFrameworkCore --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 9.0.0
dotnet add package Microsoft.VisualStudio.Validation --version 17.9.21
dotnet add package Newtonsoft.Json --version 13.0.3
Set-Location ..

Set-Location OrdersApi.API
dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design --version 9.0.7
dotnet add package Newtonsoft.Json --version 13.0.3
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
Set-Location ..

Set-Location OrdersApi.Tests
dotnet add package coverlet.collector --version 6.0.2
dotnet add package Microsoft.NET.Test.Sdk --version 17.12.0
dotnet add package xunit --version 2.9.2
dotnet add package xunit.runner.visualstudio --version 2.8.2
dotnet add package Moq --version 4.20.70
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 9.0.7
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 9.0.7
dotnet add package FluentAssertions --version 6.12.0
Set-Location ..

Write-Host "Installation of NuGet packages completed successfully."
Write-Host "Database files are located in the Shared folder: orders.db"