# Readme For Identity Authentication and Authorization

## Projects

### Starter/IdentityNetCore

This project is the basic starter template for creating an MVC Authentication Service.

#### How to create from scratch

First you create a web application as a template to get started. In this example we will go with an MVC project. Create it by using the command:

```bash
dotnet new mvc -n IdentityNetCore -o src\Starter\IdentityNetCore
```

This bootstraps an MVC web application. You can also use a `Blazor` or `Razor` app if you prefer that.

Next you import the nessecary packages needed to implement authentication.

run the following commands:

```bash
dotnet add .\src\Starter\IdentityNetCore\ package Microsoft.EntityFrameworkCore

dotnet add .\src\Starter\IdentityNetCore\ package Microsoft.EntityFrameworkCore.Design

dotnet add .\src\Starter\IdentityNetCore\ package Microsoft.EntityFrameworkCore.Tools

dotnet add .\src\Starter\IdentityNetCore\ package Microsoft.AspNetCore.Identity.EntityFramework

```

And based on the persistance of your backend You need to add an EFCore handler.

```bash
dotnet add .\src\Starter\IdentityNetCore\ package Microsoft.EntityFrameworkCore.SqlServer
```
