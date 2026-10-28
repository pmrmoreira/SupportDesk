# SupportDesk AI

Aplicação web para gerir pedidos de suporte informático: abrir pedidos, comentá-los e acompanhar o estado até à resolução, por páginas web ou por uma API REST.

ASP.NET Core 10 (Razor Pages e Web API), EF Core 10 com PostgreSQL 18.

> Em desenvolvimento. A seguir: contas e login, permissões para clientes e técnicos, testes automáticos e IA local para classificar pedidos e sugerir respostas.

## O que faz

- Pedidos com título, descrição, prioridade e estado (Aberto, Em curso, Resolvido). As mudanças de estado seguem regras: só se resolve um pedido em curso, e um pedido resolvido só pode ser reaberto.
- Comentários em cada pedido. Cada mudança de estado fica registada como comentário do «Sistema».
- Lista com filtro por estado, pesquisa no título e na descrição, e paginação.
- Formulários para criar pedidos, comentar e mudar o estado, com validação no browser e no servidor.
- API REST com as mesmas operações. Os erros vêm em JSON (ProblemDetails), com 503 quando a base de dados não responde.
- Concorrência otimista: se duas pessoas alterarem o mesmo pedido ao mesmo tempo, a segunda recebe um aviso em vez de apagar a alteração da primeira.

## Como correr

Precisas do .NET SDK 10 e do PostgreSQL 18. Na raiz do repositório:

```powershell
dotnet tool restore
dotnet user-secrets set "ConnectionStrings:SupportDesk" "Host=localhost;Port=5432;Database=supportdesk;Username=postgres;Password=A_TUA_PALAVRA_PASSE" --project src/SupportDesk.Web
dotnet ef database update --project src/SupportDesk.Web
dotnet run --project src/SupportDesk.Web
```

O `database update` cria as tabelas e três pedidos de exemplo. A aplicação fica em http://localhost:5011. A palavra-passe da base fica nos user-secrets, fora do repositório.

Para experimentar a API, abre `src/SupportDesk.Web/SupportDesk.http` no Visual Studio.

## API

| Método | Caminho | O que faz |
|---|---|---|
| GET | `/api/tickets` | Lista paginada. Filtros: `status`, `priority`, `search`, `sort`, `page`, `pageSize` |
| GET | `/api/tickets/{id}` | Um pedido |
| GET | `/api/tickets/stats` | Número de pedidos por estado |
| POST | `/api/tickets` | Cria um pedido |
| PUT | `/api/tickets/{id}/status` | Muda o estado |
| GET | `/api/tickets/{id}/comments` | Comentários de um pedido |
| POST | `/api/tickets/{id}/comments` | Acrescenta um comentário |

## Organização

O código está em `src/SupportDesk.Web`. As páginas (`Pages`) e a API (`Controllers`) chamam o mesmo serviço (`Services/TicketService.cs`), onde estão as consultas e as regras. O acesso à base é feito com EF Core (`Data`), e o esquema evolui por migrações (`Migrations`).
