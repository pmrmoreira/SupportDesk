# Caderno 02 — Consultar pedidos: HTTP, JSON, controllers e Git

[Voltar ao índice do curso](00-indice.md)

**Nome:** ____________________ · **Data de início:** ____________________ · **Data de revisão:** ____________________

**Ambiente:** Windows, Visual Studio 2026, .NET 10; PowerShell como alternativa. **Raiz do projeto:**

```text
C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI
```

Esta aula continua o teu projeto existente. O código abaixo é para tu escreveres. Escolhe Visual Studio ou PowerShell para cada operação; não inicies duas instâncias da mesma aplicação.

## 1. Objetivo e resultado

Vais construir a primeira consulta de pedidos de suporte. Uma **API** é uma interface que outros programas podem usar; nesta aula, expões essa interface através de HTTP. Um **ticket** é um pedido de suporte. Usaremos `Ticket` no código e «pedido» nas explicações.

No fim, a aplicação deve permitir:

- `GET /api/tickets`: consultar dois pedidos fictícios, em JSON.
- `GET /api/tickets/1`: consultar um pedido pelo seu identificador.
- Receber `404` quando esse pedido não existe.
- Continuar a abrir a página inicial e os endpoints da aula 1.
- Guardar uma versão do trabalho num repositório Git local.

**Duração:** 2h30–3h30 com prática. Sugestão: uma sessão para HTTP e controllers; outra para Git, exercícios e revisão.

**Matéria nova:** métodos e estados HTTP, parâmetros de rota, controller, action, atributos, pesquisa com LINQ e Git. **Revisão:** classes, propriedades, arrays, `if`, `null`, lambdas e JSON.

**Simplificação assumida:** os pedidos estarão num array fixo no código. Ainda não existe criação de pedidos nem persistência. Na aula 3 acrescentamos criação e validação; na 4 retiramos responsabilidades do controller para serviços; na 6 guardamos dados com EF Core e PostgreSQL. A IA será introduzida depois de termos uma aplicação de suporte funcional.

## 2. Antes de começar

### Avaliação do teu trabalho da aula 1

Revisei os teus ficheiros em **28 de setembro de 2026** e testei cópias isoladas, mantendo o teu código de trabalho intacto.

| Evidência | Resultado observado |
|---|---|
| Solução e projeto .NET 10 | `SupportDesk.sln` referencia corretamente `src/SupportDesk.Web`. Compilação sem erros nem avisos. |
| Página inicial | Responde com HTML, título SupportDesk AI e descrição do produto. |
| `/api/health` | Responde com JSON, `status: "ok"` e `version: "0.1.0"`, como string. |
| `/api/info` | Tem `name` e `purpose`, ambos como texto. |
| `/api/learning` | Tem `lesson` como número, `topic` como texto e `completed` como booleano `false`. |
| WebLab | Compila e `/ola` devolve o texto `Hello World!`. |
| Rotas inexistentes | Devolvem 404 nas duas aplicações testadas. |

**Parecer:** a base funcional permite avançar. Implementaste os três exercícios de código da aula 1. Não tratei as respostas teóricas como concluídas: não as avaliei nesta revisão. O valor `completed = false` no teu endpoint é um dado fictício do exercício, não uma classificação tua.

Há dois ajustes de comunicação para fazeres, sem necessidade de refazer a aula:

- Em `Program.cs`, compara `"SupportDesk Ai"` com a grafia do nome na página inicial. **Pista:** o nome do produto deve ser consistente nas respostas.
- Em `/api/info`, escreveste «suporte no Desktop». **Pista:** a aplicação que construíste funciona através de um servidor web e de um navegador; reformula a finalidade para descrever esse produto.

No WebLab, manter `Hello World!` em vez da saudação personalizada não altera o funcionamento da rota. É apenas uma diferença de texto face ao exemplo.

### Material e revisão de cinco minutos

Abre `SupportDesk.sln` no Visual Studio. Usa a carga de trabalho ASP.NET e desenvolvimento Web que preparaste na aula 1. O SDK encontrado é **10.0.401** e o Git instalado é **2.45.1.windows.1**. Não precisas de pacotes NuGet adicionais, uma conta GitHub ou uma base de dados.

Responde sem consultar. Se hesitares, experimenta e volta à explicação indicada; podes fazer esta revisão no início da aula 2.

1. Em que pasta tens de estar para executar um pedido `Invoke-WebRequest` com uma URL completa? Porquê?

   **A minha resposta:** ___________________________________________________

2. Se o navegador mostrar `/api/health`, qual é o endereço base e qual é o caminho? Usa a porta real do teu projeto.

   **A minha resposta:** ___________________________________________________

3. O que acontece ao fazer um pedido depois de parares a aplicação com Shift+F5? É o mesmo que um 404?

   **A minha resposta:** ___________________________________________________

4. Qual é a diferença entre `false` e `"false"` em JSON?

   **A minha resposta:** ___________________________________________________

### A distinção que vamos fixar

| O que estás a usar | Exemplo | Para que serve |
|---|---|---|
| Pasta no computador | `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI` | Localizar ficheiros para compilar, abrir e guardar. |
| Endereço base da aplicação | `http://localhost:5011` | Contactar o servidor em execução. |
| Caminho HTTP | `/api/tickets/1` | Escolher a operação/recurso dentro dessa aplicação. |

Um pedido HTTP com URL completa pode ser executado **em qualquer pasta do PowerShell**. Já `dotnet build SupportDesk.sln` e os comandos Git desta aula serão executados **na raiz SupportDesk-AI**. Antes de cada bloco fica indicado qual é o caso.

## 3. Explicação dos conceitos

### 3.1. O pedido e a resposta têm partes diferentes

Um pedido HTTP combina um **método**, um endereço e cabeçalhos; pode também ter um corpo. O método indica a intenção. Hoje usamos GET para consultar. POST, introduzido na próxima aula, permitirá submeter um novo pedido de suporte.

Uma resposta tem um **código de estado**, cabeçalhos e, quando aplicável, um corpo. O estado informa o resultado; `Content-Type` descreve o formato do corpo. Não confundas o estado HTTP `200` com uma propriedade do teu JSON chamada `status`.

```text
Cliente                          Servidor SupportDesk.Web
GET /api/tickets/1   ---------->  encontra a rota e lê id = 1
                                 procura o pedido no array
200 + application/json <-------  transforma o objeto C# em JSON
```

| Estado | Significado nesta aula |
|---|---|
| 200 OK | A consulta foi tratada com sucesso. |
| 404 Not Found | A rota não corresponde ou o pedido procurado não existe. |
| 405 Method Not Allowed | O caminho existe, mas não aceita o método usado. |
| 500 Internal Server Error | O servidor encontrou uma falha ao tratar o pedido; é necessário ler o erro. |

Uma ligação recusada ocorre antes de receberes uma resposta HTTP. Primeiro confirmas que o servidor está ativo no endereço certo; depois investigas as rotas.

**Ponto de verificação P1:** o endpoint da aula 1 responde com `200` e `{"status":"ok"}`. Se mudares apenas o texto `ok` para `offline`, o código HTTP muda automaticamente? Justifica.

**A minha previsão:** ___________________________________________________

### 3.2. Um objeto e uma coleção em JSON

Um pedido individual é representado por um objeto, delimitado por `{ }`. Uma coleção usa `[ ]`. As propriedades têm nomes e valores; números e booleanos não levam aspas. Não vais montar JSON por concatenação de strings: devolves objetos C# e o framework faz a **serialização**.

```json
{
  "id": 1,
  "title": "Não consigo entrar",
  "status": "Open"
}
```

Uma lista vazia continua a ser uma lista válida: `[]`. Consultar uma lista sem elementos pode ter estado 200. Procurar um pedido específico que não existe terá 404 no contrato que vamos definir.

**Contrato** significa aqui a combinação de caminho, método, estados e formato dos dados que os clientes podem esperar. Manter esse contrato previsível facilita construir a interface e os testes mais tarde.

### 3.3. Controllers e actions

Um **controller** é uma classe que agrupa operações HTTP. Uma **action** é um método dessa classe exposto como operação. Em vez de acrescentar toda a API a `Program.cs`, criaremos `TicketsController` com métodos para consultar pedidos.

`ControllerBase` fornece métodos como `Ok(...)` e `NotFound()`. Os **atributos**, escritos entre `[ ]` acima de classes ou métodos, fornecem informação ao ASP.NET Core: `[Route]` define o caminho e `[HttpGet]` o método HTTP. `[ApiController]` ativa convenções próprias de APIs; veremos a validação automática na aula 3.

São necessárias duas ligações em `Program.cs`: `AddControllers()` prepara os serviços dos controllers; `MapControllers()` disponibiliza os endpoints descritos pelos atributos. Mantemos Razor Pages para a página inicial e `MapGet` para os pequenos endpoints anteriores.

### 3.4. Um identificador vindo do caminho

Em `/api/tickets/2`, o `2` identifica o pedido. Com `[HttpGet("{id:int}")]`, o segmento é capturado com o nome `id`, apenas se puder ser interpretado como um inteiro. O framework fornece esse valor ao parâmetro `int id` da action.

Há duas decisões distintas: encontrar uma rota e encontrar os dados. `/api/tickets/abc` não corresponde à rota com `:int`; `/api/tickets/999` corresponde, mas pode não existir nenhum pedido 999. Nesta aplicação, ambos dão 404, por razões diferentes. A restrição de rota não substitui a validação das regras do negócio.

### 3.5. Localizar um objeto com LINQ

**LINQ** é um conjunto de operações de consulta sobre dados em C#. Vais usar `FirstOrDefault`: percorre a coleção e devolve o primeiro elemento que satisfaz a condição. Para a nossa classe `Ticket`, devolve `null` se não encontrar nenhum.

```csharp
var ticket = Tickets.FirstOrDefault(ticket => ticket.Id == id);
```

A lambda recebe cada `ticket` e compara o seu `Id` com o parâmetro `id`. Depois precisas de decidir o que responder caso o resultado seja `null`. O nome da variável é independente da posição no array: o ID 2 não significa obrigatoriamente «segundo elemento».

### 3.6. Guardar versões com Git

Um **repositório Git** guarda o histórico de versões. Tens os ficheiros de trabalho, uma seleção de alterações preparada para guardar (**staging area**) e os **commits**, que registam essas versões com uma mensagem. Guardar no editor altera um ficheiro; fazer commit regista uma versão no histórico.

```text
Editar e guardar → rever diferenças → selecionar alterações → fazer commit
```

O ficheiro `.gitignore` indica ficheiros ainda não seguidos que não devem entrar no histórico, como resultados de compilação. Git e GitHub não são a mesma coisa: o primeiro funciona localmente; o segundo pode alojar repositórios remotos. Nesta aula fazemos apenas commits locais.

## 4. Exemplo guiado: um número vindo da URL

**Objetivo:** perceber de onde vem um parâmetro antes de o usar para procurar pedidos.

**Ficheiro a alterar:** `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\experiencias\aula-01\WebLab\Program.cs`.

No Visual Studio, abre o `WebLab.csproj` existente, para a aplicação e acrescenta este bloco **antes de `app.Run()`**, mantendo `/ola`:

```csharp
app.MapGet("/eco/{numero:int}", (int numero) => new
{
    valor = numero,
    dobro = numero * 2
});
```

É código que tu acrescentas, não código gerado pelo template. O nome `numero` aparece no caminho e no parâmetro. O objeto anónimo contém dois números; a multiplicação é C# normal.

**Antes de executar:** que corpo esperas em `/eco/21`? E que estado esperas em `/eco/texto`?

**A minha previsão:** ___________________________________________________

**Visual Studio:** guarda, seleciona WebLab e o perfil `http`, pressiona F5 e abre os dois caminhos no navegador ou num ficheiro `.http`. Usa a porta do laboratório em `Properties/launchSettings.json`; na inspeção era **5023**, diferente da porta 5011 do produto.

**Alternativa PowerShell — pasta: raiz SupportDesk-AI.**

```powershell
Set-Location -LiteralPath 'C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI'
dotnet run --project experiencias/aula-01/WebLab --launch-profile http
```

**Verificação — outro PowerShell, qualquer pasta.** Usa a porta efetivamente mostrada ao arrancar:

```powershell
Invoke-WebRequest -UseBasicParsing -Uri 'http://localhost:5023/eco/21' |
    Select-Object StatusCode, Content
```

Depois de experimentares, compara o corpo com os cálculos que escreveste. Não precisas de alterar o servidor para consultar outro número. As respostas comentadas estão no apoio final.

**Ponto de verificação P2:** substitui apenas `21` por `7` na URL. Explica onde executou a multiplicação e porque não precisaste de recompilar.

**A minha explicação:** ___________________________________________________

Para o WebLab com Shift+F5, ou Ctrl+C se usaste `dotnet run`. Volta a abrir `SupportDesk.sln` para construir a funcionalidade do produto.

## 5. Construção no projeto, passo a passo

### Passo 1 — Confirmar o ponto de partida

**Objetivo:** trabalhar no projeto que já construíste. **Ficheiros:** não há criação de solução nem de projeto nesta aula.

**Visual Studio:** abre `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\SupportDesk.sln`. Confirma `SupportDesk.Web` no Explorador de Soluções e abre `Program.cs`. Deves reconhecer os endpoints `/api/health`, `/api/info` e `/api/learning`. Faz os dois pequenos ajustes de texto sugeridos na avaliação, se concordares com a finalidade descrita.

**PowerShell — pasta: raiz SupportDesk-AI.** Este bloco leva-te até ela e confirma os ficheiros:

```powershell
Set-Location -LiteralPath 'C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI'
Get-Location
Test-Path -LiteralPath '.\SupportDesk.sln'
Test-Path -LiteralPath '.\src\SupportDesk.Web\Program.cs'
```

**Resultado esperado:** ambos os testes dão `True`. Se um falhar, confirma a pasta antes de criar qualquer coisa. Para a aplicação enquanto editas os passos seguintes.

### Passo 2 — Dar um tipo aos pedidos

**Objetivo:** representar cada pedido com a mesma estrutura C#. **Ficheiro novo:** `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\src\SupportDesk.Web\Models\Ticket.cs`.

**Visual Studio:** botão direito em **SupportDesk.Web → Add → New Folder**, nome `Models`. Dentro dela: **Add → Class**, nome `Ticket.cs`. Substitui o conteúdo dessa classe nova pelo seguinte. Se já existir um ficheiro com esse nome, abre-o antes de repetir a criação.

```csharp
namespace SupportDesk.Web.Models;

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
}
```

**Alternativa PowerShell — raiz SupportDesk-AI:** cria a pasta, depois cria e escreve o ficheiro no editor que usas:

```powershell
New-Item -ItemType Directory -Path '.\src\SupportDesk.Web\Models' -Force
```

O **namespace** organiza nomes de tipos; o ponto em `SupportDesk.Web.Models` não é um caminho HTTP. `get; set;` permite ler e atribuir propriedades. `string.Empty` inicializa o título sem `null`; não valida o título. `"Open"` é um estado inicial simplificado, que vamos controlar melhor na aula 4.

**Verificação:** guarda e compila com Ctrl+Shift+B, ou `dotnet build SupportDesk.sln` na raiz. **Resultado esperado:** sem erros. Criar uma classe ainda não cria uma rota.

### Passo 3 — Ligar os controllers à aplicação

**Objetivo:** permitir que o ASP.NET Core encontre e execute as actions. **Ficheiro a alterar:** `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\src\SupportDesk.Web\Program.cs`.

Junto de `AddRazorPages()`, **antes de `builder.Build()`**, acrescenta:

```csharp
builder.Services.AddControllers();
```

Depois dos mapeamentos existentes e **antes de `app.Run()`**, acrescenta:

```csharp
app.MapControllers();
```

Estas são inserções de uma linha em dois locais diferentes. Mantém `AddRazorPages()`, `MapRazorPages()`, os endpoints anteriores e o restante código gerado. Não precisas de criar um novo projeto do tipo Web API.

**Visual Studio:** abre `Program.cs`, insere as linhas, guarda e compila. **PowerShell — raiz SupportDesk-AI:** `dotnet build SupportDesk.sln`.

**Resultado esperado:** compilação sem erros. A rota `/api/tickets` só existirá depois de criares o controller.

### Passo 4 — Listar dois pedidos fictícios

**Objetivo:** implementar a primeira action. **Ficheiro novo:** `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\src\SupportDesk.Web\Controllers\TicketsController.cs`.

**Visual Studio:** cria a pasta `Controllers` no projeto e adiciona uma **Class** chamada `TicketsController.cs`. Uma classe normal chega; não precisas de um assistente que gere operações CRUD ou base de dados.

**Alternativa PowerShell — raiz SupportDesk-AI:** cria a pasta e escreve o ficheiro no editor:

```powershell
New-Item -ItemType Directory -Path '.\src\SupportDesk.Web\Controllers' -Force
```

Vamos juntar três peças: atributos que definem a rota, dois objetos fictícios e um método que devolve a coleção. Escreve este conteúdo no ficheiro novo:

```csharp
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Web.Models;

namespace SupportDesk.Web.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private static readonly Ticket[] Tickets =
    {
        new Ticket { Id = 1, Title = "Não consigo entrar", Status = "Open" },
        new Ticket { Id = 2, Title = "Fatura com valor incorreto", Status = "InProgress" }
    };

    [HttpGet]
    public ActionResult<Ticket[]> GetAll()
    {
        return Ok(Tickets);
    }
}
```

`using` permite referir os tipos desses namespaces. A classe é pública e herda de `ControllerBase`. `[Route("api/tickets")]` define o prefixo; `[HttpGet]` sem um caminho adicional responde exatamente nesse prefixo. O nome `GetAll` é uma escolha nossa, não um segmento acrescentado automaticamente à URL.

`Ticket[]` é um array. `static` faz este campo pertencer à classe; `readonly` impede trocar a referência do campo depois da inicialização, **mas não torna os objetos imutáveis**. Não vamos modificar este array nem os objetos nesta aula. São dados fictícios de leitura, não uma solução de armazenamento concorrente.

`ActionResult<Ticket[]>` permite devolver os dados ou um resultado HTTP. `Ok(Tickets)` produz 200 com a coleção serializada. Nas opções padrão desta aplicação, `Id` em C# aparece como `id` em JSON.

**Verificação:** guarda, executa SupportDesk.Web no perfil `http` com F5 e abre `http://localhost:5011/api/tickets`, se a porta continua a ser 5011. **Resultado esperado:** estado 200 e um array JSON com dois objetos. A raiz `/` continua a mostrar a página inicial. Para a aplicação antes da alteração seguinte.

**Ponto de verificação P3:** prevê a URL desta action se alterares apenas o nome C# `GetAll` para `ListTickets`. Confirma a tua previsão sem mudar o atributo da rota.

**A minha previsão e observação:** ___________________________________________________

### Passo 5 — Consultar pelo ID e tratar a ausência

**Objetivo:** distinguir «encontrei o pedido» de «não existe». **Ficheiro a alterar:** o mesmo `Controllers\TicketsController.cs` do passo anterior.

Acrescenta esta action **dentro da classe**, depois de `GetAll` e antes da última chaveta. Não a coloques dentro do método `GetAll`.

```csharp
    [HttpGet("{id:int}")]
    public ActionResult<Ticket> GetById(int id)
    {
        var ticket = Tickets.FirstOrDefault(ticket => ticket.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }
```

Agora a rota junta `api/tickets` e `{id:int}`. A condição `ticket is null` decide o resultado. O método devolve `ActionResult<Ticket>` porque o sucesso contém um objeto, enquanto a ausência é um resultado HTTP 404. Não devolvas um objeto fictício vazio para disfarçar a ausência.

O projeto tem `ImplicitUsings` ativado, por isso LINQ já fica disponível; se usares outro projeto com essa opção desativada, poderás precisar de `using System.Linq;`. Com `[ApiController]`, `NotFound()` pode produzir um corpo de erro normalizado pelo framework. Aqui verificamos o estado; não dependemos de o corpo estar vazio.

**Verificação:** guarda, executa e consulta `/api/tickets/1`, `/api/tickets/2` e `/api/tickets/999`. **Resultado esperado:** os dois primeiros devolvem o objeto correspondente; o terceiro devolve 404. Confirma pelo ID, não apenas pelo aspeto do JSON.

### Passo 6 — Verificar o contrato pelo Visual Studio ou PowerShell

**Objetivo:** registar resultados verificáveis, incluindo erros esperados. **Ficheiro de pedidos:** `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\src\SupportDesk.Web\SupportDesk.http`.

**Visual Studio:** adiciona **HTTP File** pelo menu **Add → New Item**, ou abre o ficheiro existente. Acrescenta estes pedidos. `###` separa os pedidos; cada um tem o seu botão **Send Request**. Inicia o servidor com F5 antes de enviar.

```http
@baseUrl = http://localhost:5011

### Lista
GET {{baseUrl}}/api/tickets

### Pedido existente
GET {{baseUrl}}/api/tickets/1

### Pedido inexistente
GET {{baseUrl}}/api/tickets/999

### Segmento que não é inteiro
GET {{baseUrl}}/api/tickets/abc

### Método ainda não implementado
POST {{baseUrl}}/api/tickets
```

O primeiro valor é o endereço base, sem `/api/tickets`. Se já tens uma variável `@baseUrl` no ficheiro, utiliza-a uma única vez. Confirma a porta em `Properties/launchSettings.json`, no perfil `http`, e no endereço efetivamente usado ao arrancar.

**Alternativa PowerShell — qualquer pasta.** O servidor pode estar a correr no Visual Studio:

```powershell
$baseUrl = 'http://localhost:5011'
$response = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/api/tickets"
$response.StatusCode
$response.Headers['Content-Type']
$response.Content
$tickets = $response.Content | ConvertFrom-Json
$tickets.Count
```

Aqui atribuímos o endereço diretamente; não existe uma pergunta `Read-Host`. O resultado esperado é 200, JSON e contagem 2. Para o detalhe, executa um novo pedido:

```powershell
$response = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/api/tickets/1"
$response.StatusCode
$response.Content
```

No Windows PowerShell 5.1, um estado HTTP de erro faz `Invoke-WebRequest` lançar uma exceção. Para observar um 404 esperado, sem o confundir com uma falha do servidor, usa **em qualquer pasta**:

```powershell
try {
    $response = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/api/tickets/999"
    $response.StatusCode
}
catch {
    if ($null -ne $_.Exception.Response) {
        [int]$_.Exception.Response.StatusCode
    }
    else {
        $_.Exception.Message
    }
}
```

`catch` trata a exceção; `$_` representa o erro atual. Se houve resposta HTTP, lemos o seu estado. Se não houve, mostramos a mensagem de ligação. Este bloco não corrige a API nem transforma um erro em sucesso: apenas torna o diagnóstico legível.

**Verificação esperada antes dos exercícios:**

| Pedido | Estado | O que observar |
|---|---|---|
| GET `/api/tickets` | 200 | Array com dois pedidos. |
| GET `/api/tickets/1` | 200 | Objeto com `id: 1`. |
| GET `/api/tickets/999` | 404 | A rota foi encontrada, mas não o pedido. |
| GET `/api/tickets/abc` | 404 | O segmento não satisfaz `:int`. |
| POST `/api/tickets` | 405 | Ainda não existe uma action POST nesse caminho. |
| GET `/` e GET `/api/health` | 200 | A página e o endpoint da aula 1 continuam disponíveis. |

**Ponto de verificação P4:** usa um breakpoint na primeira linha de `GetById` no Visual Studio. Com F5, compara as chamadas `/999` e `/abc`. Em qual delas entraste no método? Para continuar depois da pausa, pressiona F5. Retira o breakpoint quando terminares.

**A minha observação:** ___________________________________________________

### Passo 7 — Preparar o repositório local

**Objetivo:** começar um histórico recuperável. **Local:** a raiz `SupportDesk-AI`, que contém a solução. A inspeção desta revisão não encontrou um repositório Git nessa pasta nem nos seus pais.

**Ficheiro novo:** `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\.gitignore`. Cria-o no Explorador de Ficheiros/editor ou com **Add → New Item → Text File** numa solução, confirmando que fica ao lado de `SupportDesk.sln`. Não deixes a extensão `.txt` no fim. Se já existe, lê-o e mantém as regras úteis, acrescentando apenas as que faltem.

Para esta fase do curso, estas regras simples chegam:

```gitignore
.vs/
**/bin/
**/obj/
*.user
*.suo
```

As pastas `bin` e `obj` são recriadas pela compilação; `.vs` e os ficheiros de utilizador guardam estado local do editor. O `.gitignore` é código de configuração escrito por ti. Não ignora automaticamente um ficheiro que já tenha entrado num commit. Vamos rever as regras quando introduzirmos configuração com credenciais.

**Visual Studio:** escolhe **Git → Create Git Repository → Local only**. Confirma que o caminho é a raiz `SupportDesk-AI` e mantém o `.gitignore` existente. Esta opção cria um repositório local; não é preciso publicar no GitHub. Se a tua versão fizer logo um commit inicial, confirma o histórico antes de continuares; a versão pode já estar guardada.

**Alternativa PowerShell — raiz SupportDesk-AI:**

```powershell
Set-Location -LiteralPath 'C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI'
git --version
git rev-parse --show-toplevel
```

Se o último comando disser **not a git repository**, é o resultado esperado antes da inicialização. Executa então, **na mesma pasta**:

```powershell
git init
git status
```

Se já mostrar a raiz correta, não precisas de inicializar novamente. Se mostrar uma pasta diferente, não cries um repositório dentro de outro: confirma primeiro o que abriste. **Resultado esperado:** o Git reconhece a raiz do produto; o nome da branch pode variar e não precisa de ser alterado nesta aula.

### Passo 8 — Rever e guardar uma versão

**Objetivo:** guardar o código que acabaste de verificar. **Ficheiros a incluir:** `.gitignore`, `SupportDesk.sln`, `README.md` e o código de `src`. O laboratório e os cadernos podem ficar por adicionar neste primeiro commit.

**Visual Studio:** abre **View → Git Changes**. Revê os ficheiros, usa **Stage** ou o botão `+` nos que pretendes incluir e confirma a lista de **Staged Changes**. Escreve `Aula 2: consulta de pedidos por lista e ID` e escolhe **Commit Staged**. Se a criação do repositório já guardou tudo, verifica esse commit; não precisas de fabricar uma alteração apenas para fazer outro.

**Alternativa PowerShell — raiz SupportDesk-AI:**

```powershell
git status
git add -- .gitignore SupportDesk.sln README.md src
git diff --cached --stat
git diff --cached
```

`add` prepara a seleção; ainda não faz commit. `diff --cached` mostra exatamente o que vai entrar. Não devem aparecer `.vs`, `bin` ou `obj`. Se aparecerem ficheiros inesperados, não avances com o commit até perceberes a seleção. Para tirar um ficheiro da seleção, usa **Unstage** no Visual Studio; não apaga o ficheiro de trabalho.

Depois de rever, **na raiz**:

```powershell
git commit -m 'Aula 2: consulta de pedidos por lista e ID'
git log -1 --oneline
git status
```

Se o Git pedir identidade, configura **o teu nome e o email que pretendes associar aos commits**, em **Git → Settings → Git Repository Settings**. Pelo terminal, usa `git config user.name 'O teu nome'` e `git config user.email 'o-teu-email'`, substituindo os exemplos; omitir `--global` limita a configuração a este repositório. Depois repete o commit. A identidade fica no histórico local e acompanhará os commits se um dia os publicares.

**Resultado esperado:** `git log -1` mostra um commit. `git status` pode continuar a mostrar os cadernos ou experiências como não seguidos, porque escolheste guardar apenas parte do projeto. Isso não significa que o commit do código falhou. Um commit local também não é, por si só, uma cópia de segurança fora do computador.

## 6. Pontos de verificação

Faz esta pausa antes dos exercícios. Não basta o JSON aparecer: tenta explicar o percurso de um pedido sem ler o controller.

| Ponto | Prova curta |
|---|---|
| P1 | Distinguir o estado HTTP de uma propriedade `status` no JSON. |
| P2 | Explicar como uma parte da URL chegou a um parâmetro C#. |
| P3 | Distinguir o nome do método do caminho definido pelos atributos. |
| P4 | Mostrar que uma rota não correspondente não executa a action. |
| P5 | Explicar a diferença entre guardar no editor, fazer stage e fazer commit. |

**Ponto de verificação P5:** altera uma palavra num título fictício, guarda e observa o Git Changes. A alteração já está no último commit? Como a guardarias numa nova versão? Mantém ou desfaz essa edição conscientemente antes de continuar.

**A minha explicação:** ___________________________________________________

**Ainda preciso de rever:** ___________________________________________________

## 7. Exercícios com apoio progressivamente menor

As pistas e soluções estão no final. Mantém a página inicial e os endpoints anteriores. Faz um novo commit depois de verificares os exercícios que resolveres.

### Exercício A — Acompanhado: acrescentar um terceiro pedido

Acrescenta ao array um pedido fictício com **ID 3**, título **Impressora indisponível** e estado **Open**. Segue a forma dos objetos existentes e verifica as vírgulas entre elementos. Não alteres os IDs anteriores.

**Aceitação:** GET `/api/tickets` contém três objetos; GET `/api/tickets/3` devolve esse pedido com 200; `/999` continua a dar 404. Guarda os resultados sem copiar a solução.

**O que fiz e confirmei:** ___________________________________________________

### Exercício B — Menos ajuda: consultar apenas o título

Acrescenta uma action para **GET `/api/tickets/{id:int}/title`**. Quando existe, devolve JSON com `id` e `title`. Quando não existe, devolve 404. Reutiliza a ideia da procura por ID; não escrevas uma resposta fixa só para o pedido 1.

**Aceitação:** `/1/title` e `/3/title` devolvem os respetivos títulos; `/999/title` dá 404. O detalhe completo continua a funcionar. Podes usar `IActionResult` como tipo de retorno para esta resposta com objeto anónimo: representa um resultado HTTP sem indicar um tipo de dados específico no `<...>`.

**A minha abordagem:** ___________________________________________________

### Exercício C — Autónomo: uma contagem que acompanha os dados

Cria **GET `/api/tickets/count`**, que devolve um objeto JSON com `total`, calculado a partir do array. Não escrevas o número 3 diretamente. Antes de consultar uma pista, identifica que informação um array já sabe sobre si próprio.

**Aceitação:** depois do exercício A, o total é 3. Acrescenta temporariamente um quarto objeto, guarda e reinicia: o total passa a 4. Remove esse objeto de teste, reinicia e confirma que volta a 3. `/api/tickets/1` continua a funcionar.

**Porque a rota count não é confundida com um ID:** ___________________________________________________

## 8. Erros frequentes e como os investigar

Primeiro identifica a fase: **compilação**, **arranque**, **ligação**, **rota**, **dados** ou **Git**. Copia a mensagem exata e faz uma hipótese que consigas testar. Mudar várias coisas ao mesmo tempo dificulta saber qual resolveu o problema.

| Sintoma | Causa provável | Como investigar |
|---|---|---|
| `ControllerBase` ou `HttpGet` não é reconhecido | Falta o namespace ou o ficheiro está no projeto errado. | Confirma `using Microsoft.AspNetCore.Mvc;` e que estás em SupportDesk.Web, um projeto com SDK Web. Não instales pacotes ao acaso. |
| `Ticket` não é reconhecido | Namespace diferente ou `using` em falta. | Compara o namespace de Ticket.cs com `using SupportDesk.Web.Models;`. |
| Erro ao registar os serviços | `AddControllers()` ficou depois de `builder.Build()`. | Coloca o registo junto de `AddRazorPages()`, antes de construir a aplicação. |
| Todas as rotas de tickets dão 404, mas health funciona | Controllers não mapeados ou atributo diferente. | Confirma `MapControllers()` antes de `app.Run()` e `[Route("api/tickets")]`. |
| `/999` dá 404 e o PowerShell mostra vermelho | O pedido não existe e a ferramenta lança exceção para esse estado. | Usa o ficheiro `.http` ou o bloco `try/catch` do passo 6. Aqui 404 é o resultado esperado. |
| Ligação recusada | Servidor parado ou porta incorreta. | Confirma F5, perfil `http` e o endereço real. A pasta do PowerShell não resolve uma ligação recusada. |
| Recebo JSON antigo | Alteração não guardada ou processo anterior. | Para a tua instância, guarda, executa e faz um novo pedido. `$response` não se atualiza sozinho. |
| A aplicação responde, mas não aos endpoints do produto | Estás a usar a porta do WebLab. | Compara os perfis: na revisão, 5023 era WebLab e 5011 era SupportDesk.Web. |
| `IndexOutOfRangeException` na consulta por ID | Usaste o ID como posição do array. | Procura um objeto cujo `Id` corresponda; índices começam em zero e não são IDs. |
| Exceção de referência nula ao ler o título | Tentaste usar um pedido que não foi encontrado. | Verifica `ticket is null` antes de aceder às suas propriedades. |
| `git` não é reconhecido | Git indisponível nesse terminal. | Reabre o terminal e executa `git --version`; usa o Git do Visual Studio enquanto confirmas a instalação. |
| `not a git repository` depois da inicialização | Estás noutra pasta. | Usa o Set-Location da raiz e `git rev-parse --show-toplevel`. |
| `Author identity unknown` | Falta nome/email para o commit. | Configura a identidade local conforme o passo 8 e repete o commit. |
| `nothing added to commit` | Não selecionaste as alterações ou já foram guardadas. | Compara Git Changes, `git status` e `git log -1`. Não repitas a criação do projeto. |
| Aviso `LF will be replaced by CRLF` | O Git está a aplicar a configuração de fins de linha do Windows. | É um aviso sobre a representação das quebras de linha; confirma o resultado do comando e o diff. Não é, por si só, uma falha do commit. |

**O meu erro, hipótese e teste:** ___________________________________________________

## 9. Revisão sem consultar

1. Qual é a diferença entre uma pasta Windows, o endereço base e uma rota? Dá um exemplo de cada no teu projeto.

   **A minha resposta:** ___________________________________________________

2. Que papéis diferentes têm `AddControllers()` e `MapControllers()`?

   **A minha resposta:** ___________________________________________________

3. Porque é que devolver uma lista vazia e procurar um pedido inexistente podem ter estados diferentes?

   **A minha resposta:** ___________________________________________________

4. Em `/api/tickets/abc` e `/api/tickets/999`, chegamos à mesma linha de C#? Justifica usando o atributo da rota.

   **A minha resposta:** ___________________________________________________

5. O que existe depois de guardares um ficheiro, depois de stage e depois de commit? Qual dessas operações envia código para o GitHub?

   **A minha resposta:** ___________________________________________________

### Reconstrução de dez minutos

Sem olhar para o exemplo, escreve num rascunho o método que consulta um pedido pelo ID, incluindo o atributo e o caso de ausência. Compara depois com a tua versão funcional. Se o introduzires no projeto para testar, substitui temporariamente a action original; não deixes duas actions iguais registadas.

**O que consegui reconstruir e o que faltou:** ___________________________________________________

## 10. Critérios de conclusão

### Consigo fazer

- [ ] Listar pedidos com 200 e um array JSON. Se não: rever o passo 4.
- [ ] Consultar um ID existente com o objeto correto. Se não: rever o passo 5.
- [ ] Confirmar 404 para um ID inexistente e distinguir uma falha de ligação. Se não: rever o passo 6.
- [ ] Confirmar que a página inicial e `/api/health` continuam a funcionar. Se não: rever o passo 3.
- [ ] Acrescentar um objeto fictício e verificar lista e detalhe. Se não: exercício A.
- [ ] Ver um commit local e os ficheiros que contém. Se não: passos 7–8.

### Consigo explicar

- [ ] Porque a pasta atual não altera um pedido HTTP com URL completa. Se não: secção 2.
- [ ] A relação entre controller, action, atributos e parâmetro de rota. Se não: secções 3.3–3.4.
- [ ] Porque verifico `null` antes de devolver o pedido. Se não: secção 3.5 e passo 5.
- [ ] Porque estes dados não são uma base de dados e `readonly` não os torna imutáveis. Se não: passos 2 e 4.
- [ ] Porque uma mudança de texto num objeto não decide automaticamente o código HTTP. Se não: P1.
- [ ] A diferença entre guardar, stage, commit e publicar num servidor remoto. Se não: secção 3.6 e passo 8.

Os exercícios B e C servem para ganhar autonomia. Se quiseres avançar sem os completar, indica quais saltaste e confirma os critérios essenciais acima; não vou presumir compreensão apenas porque o código compila.

## 11. Resumo e revisão posterior

Transformaste uma lista de objetos C# numa API de consulta. O caminho e o método selecionam uma action; o parâmetro chega do pedido; a action procura dados e escolhe um resultado HTTP. O framework serializa os dados. Git guarda versões das alterações que selecionas.

**Amanhã — 10 minutos:** sem consultar o caderno, executa uma consulta existente e outra inexistente. Explica os dois 404 diferentes e mostra o último commit. Não é necessário reescrever tudo.

**Daqui a uma semana — 15 minutos:** reconstrói a action de detalhe num rascunho, acrescenta um pedido fictício e verifica a API. Antes de fazer commit, identifica no diff exatamente o que mudou.

**Próxima aula:** vais receber dados do cliente através de POST, definir um DTO — um objeto com os dados de entrada —, validar esses dados e responder com estados de sucesso ou erro adequados. A lista fixa desta aula será adaptada para permitir criação em memória. Não tentes acrescentar PostgreSQL já.

**O que quero trazer à próxima revisão:** ___________________________________________________

## Apoio final — Pistas e soluções comentadas

Consulta depois de tentares. Os fragmentos de actions abaixo ficam dentro de `TicketsController`, sem duplicar rotas que já criaste.

### Revisão inicial e pontos de verificação

- A pasta pode ser qualquer uma para um pedido com URL completa. Na configuração inspecionada, o endereço base do produto é `http://localhost:5011` e `/api/health` é o caminho. Um servidor parado não produz um novo 404: falta uma resposta HTTP. `false` é booleano; `"false"` é texto.
- **P1:** mudar um valor do objeto não muda automaticamente o estado HTTP; o exemplo continua com 200.
- **Exemplo guiado:** `/eco/21` devolve `{"valor":21,"dobro":42}` com 200. `/eco/texto` não satisfaz `:int` e dá 404.
- **P2:** com 7, o servidor calcula 14. Mudou o valor de entrada, não o código compilado.
- **P3:** a rota continua `/api/tickets`, porque está nos atributos, não no nome do método.
- **P4:** 999 entra na action e a pesquisa falha. `abc` não corresponde à rota e não chega à action.
- **P5:** guardar altera o ficheiro; stage prepara a seleção; commit regista essa seleção. Nenhuma dessas operações publica no GitHub.

### Exercício A — Pista e solução

**Pista:** acrescenta um elemento ao array, separado por uma vírgula; não cries outra action.

```csharp
new Ticket { Id = 3, Title = "Impressora indisponível", Status = "Open" }
```

Esta é apenas a expressão do novo elemento. Coloca uma vírgula após o elemento anterior. O método existente já procura qualquer ID do array, por isso encontra o novo pedido sem outro `if` específico.

### Exercício B — Pista e solução

**Pista:** começa pela action de detalhe, muda o sufixo da rota e projeta só duas propriedades no resultado.

```csharp
    [HttpGet("{id:int}/title")]
    public IActionResult GetTitle(int id)
    {
        var ticket = Tickets.FirstOrDefault(ticket => ticket.Id == id);
        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(new { id = ticket.Id, title = ticket.Title });
    }
```

O caminho tem um segmento adicional `title`. A procura e o tratamento da ausência são os mesmos; o corpo da resposta contém apenas os dados pedidos.

### Exercício C — Pista e solução

**Pista:** um array tem a propriedade `Length`. O literal `count` não é um inteiro.

```csharp
    [HttpGet("count")]
    public IActionResult GetCount()
    {
        return Ok(new { total = Tickets.Length });
    }
```

Usamos o tamanho dos dados existentes, não uma constante. A rota de detalhe exige `:int`, por isso `count` não a satisfaz. Se acrescentares ou retirares dados no código, tens de guardar e reiniciar para a nova versão executar.

### Revisão final — Ideias essenciais

1. Os ficheiros têm caminhos no sistema operativo; a aplicação tem um endereço de rede e rotas HTTP.
2. Registar os serviços e mapear os endpoints são passos diferentes, ambos necessários.
3. Uma coleção vazia pode ser um resultado válido. A ausência de um recurso individual é comunicada com 404 neste contrato.
4. O valor `abc` falha a seleção da rota; 999 entra no método e falha a procura dos dados.
5. Ficheiro, seleção e commit são estados diferentes. Nenhum publica automaticamente um repositório local.

## Notas livres

________________________________________________________________________

________________________________________________________________________

## Referências oficiais para aprofundar

O caderno é suficiente para fazer a aula; estas páginas fundamentam as APIs e os fluxos usados:

- [APIs com controllers e configuração no ASP.NET Core 10](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-10.0).
- [Rotas e parâmetros dos controllers](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-10.0).
- [Tipos de retorno das actions](https://learn.microsoft.com/en-us/aspnet/core/web-api/action-return-types?view=aspnetcore-10.0).
- [Git: selecionar alterações e fazer commits](https://git-scm.com/book/en/v2/Git-Basics-Recording-Changes-to-the-Repository).
- [Criar um repositório local no Visual Studio](https://learn.microsoft.com/en-us/visualstudio/version-control/git-create-repository?view=visualstudio) e [guardar commits pelo editor](https://learn.microsoft.com/en-us/visualstudio/version-control/git-make-commit?view=visualstudio).

## Nota de verificação deste material

Preparado em 28 de setembro de 2026 a partir do teu projeto. A revisão da aula 1 foi feita numa cópia isolada: compilação da solução e do WebLab, página inicial, três endpoints JSON, texto do laboratório e rotas inexistentes. O estado de compreensão das respostas teóricas não foi inferido desses testes.

Os exemplos da aula 2 e as soluções comentadas foram extraídos deste caderno e compilados numa cópia separada com .NET 10.0.401, sem erros nem avisos. Foram verificadas as respostas da lista, detalhes, ID inexistente, segmento não inteiro, método POST não permitido, títulos, contagem e exemplo do WebLab, mantendo a página e os endpoints anteriores. Também foi criado um commit apenas na cópia de validação, confirmando que os ficheiros de compilação ficaram excluídos.

Não foram acrescentados controllers, modelos ou commits ao teu projeto por mim. Os menus do Visual Studio foram conferidos com documentação oficial, sem executar a sequência na tua instalação. O HTML foi verificado com navegação entre aulas, respostas e progresso separados, exportação/importação e preservação dos identificadores das respostas anteriores. No ambiente de validação foi usada uma configuração NuGet local para os testes; não a introduzas no teu projeto.
