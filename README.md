# Gestão de orçamentos

Aplicação web local em ASP.NET Core MVC utilizando Bootstrap.

## Requisitos

- SDK do .NET 10.

## Executar

Na raiz do repositório:

```powershell
dotnet restore GestaoOrcamentos.slnx
dotnet run --project src/GestaoOrcamentos.Web/GestaoOrcamentos.Web.csproj
```

Abra `http://127.0.0.1:5198` no navegador. Para verificar a compilação sem iniciar o servidor, execute `dotnet build GestaoOrcamentos.slnx`.
