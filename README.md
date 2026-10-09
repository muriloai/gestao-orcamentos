# Gestão de orçamentos

Aplicação web local em ASP.NET Core MVC e Razor Pages utilizando Bootstrap.

Permite cadastrar, pesquisar, consultar, editar e excluir clientes.
A página de configurações guarda os dados do negócio.
Também permite criar e pesquisar orçamentos com itens, quantidade, unidade e preço, além de acompanhar aprovação e recusa.

## Arquitetura

É um projeto web .NET 10 + testes.

### As operações de clientes seguem este fluxo:

```text
Views Razor ↔ ClientesController → ClienteService → GestaoOrcamentosDbContext → SQL Server LocalDB
```

| Parte                    | Responsabilidade                                                                                                                                         |
| ------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| MVC e Razor              | O controller recebe as requisições, já as views apresentam listagem, detalhes, formulários e erros.                                                      |
| Razor Pages              | A página `/Configuracoes` apresenta e salva os dados do negócio.                                                                                         |
| Orçamentos MVC           | Listagem, pesquisa, criação e consulta; `OrcamentoService` valida itens, altera rascunhos e controla as transições de situação.                          |
| Cálculo de valores       | Cada subtotal é arredondado para centavos antes da soma do total, usando `decimal` e `AwayFromZero`. O número exibido usa o ID do banco com seis dígitos. |
| `ClienteFormulario`      | Modelo usado no cadastro e na edição, com validações por Data Annotations, e o serviço também as verifica antes de gravar.                               |
| `ClienteService`         | Pesquisa, consulta, normaliza dados e coordena cadastro, edição e exclusão. É registrado como serviço _scoped_ por injeção de dependência.               |
| Entity Framework Core    | O `DbContext` mapeia clientes, configurações, orçamentos e itens; migrations versionam as tabelas. A configuração possui um único registro, com ID fixo em 1. Cópias cadastrais ficam no orçamento. |
| Padrão Post/Redirect/Get | Após cadastro ou edição, o POST redireciona aos detalhes. Após exclusão, redireciona à listagem de clientes ou ao cliente vinculado. A confirmação usa `TempData`. |

Os POSTs usam token antifalsificação. A exclusão exige uma página de confirmação, e a requisição GET nunca remove dados. As consultas usam `AsNoTracking`, e as operações de banco são assíncronas e recebem o token de cancelamento da requisição. A interface usa cultura `pt-BR`, Bootstrap local e textos em português. Os testes xUnit verificam validações, consultas e persistência em um banco LocalDB temporário.

Para criar um orçamento, cadastre um cliente e use **Novo orçamento** no menu ou na página do cliente. A exclusão de clientes com orçamentos é bloqueada pela aplicação e pelo banco.
Na página de um rascunho, **Editar** permite alterar os dados e adicionar ou remover itens; **Excluir** exige confirmação. Uma edição inválida não grava parte das alterações.
A lista **Orçamentos** permite pesquisar pelo nome do cliente ou pelo título. A página de cada cliente mostra seus orçamentos vinculados.

Um orçamento começa como **rascunho**, quando pode ser editado ou excluído. Ao marcá-lo como **pendente de aprovação**, a aplicação exige as configurações do negócio e registra cópias dos dados do cliente e do negócio. Enquanto estiver pendente, é possível **aprovar**, **recusar** ou **devolver ao rascunho**. Aprovados e recusados ficam disponíveis para consulta, sem edição ou exclusão. A aprovação e a recusa são registradas pelo operador da aplicação.

As consultas de orçamentos fora de rascunho mostram os dados registrados no envio, mesmo que os cadastros sejam alterados depois. Devolver ao rascunho descarta essas cópias; um novo envio registra os dados atualizados. A lista permite filtrar pela situação.

Qualquer orçamento pode ser duplicado como novo rascunho. A cópia recebe outro número e a data atual de São Paulo, mantém cliente, título, observações e itens, e deixa a validade em branco. Seus itens são independentes do original; os dados históricos registrados não são copiados.

## Requisitos

- SDK do .NET 10.
- SQL Server LocalDB.

## Preparar o banco

Confirme que a instância LocalDB está instalada e que a conexão funciona:

```powershell
SqlLocalDB info MSSQLLocalDB
sqlcmd -S '(localdb)\MSSQLLocalDB' -E -Q 'SELECT 1'
```

A conexão de desenvolvimento está em `src/GestaoOrcamentos.Web/appsettings.Development.json` e usa autenticação integrada do Windows. No Visual Studio, abra o Console do Gerenciador de Pacotes, selecione `GestaoOrcamentos.Web` como projeto padrão e execute:

```powershell
Update-Database
```

Se usar o terminal e tiver `dotnet-ef` instalado globalmente, o comando equivalente é:

```powershell
dotnet ef database update --project src/GestaoOrcamentos.Web/GestaoOrcamentos.Web.csproj
```

## Executar

Na raiz do repositório:

```powershell
dotnet restore GestaoOrcamentos.slnx
dotnet run --project src/GestaoOrcamentos.Web/GestaoOrcamentos.Web.csproj
```

Abra `http://127.0.0.1:5198` no navegador. Para verificar a compilação sem iniciar o servidor, execute `dotnet build GestaoOrcamentos.slnx`.

## Testes

Execute `dotnet test GestaoOrcamentos.slnx`. O teste de persistência cria e remove um banco LocalDB próprio.
