# Caderno 03 — Criar pedidos: POST, DTOs e validação

[Voltar ao índice do curso](00-indice.md)

**Nome:** ____________________ · **Data de início:** ____________________ · **Data de revisão:** ____________________

**Ambiente:** Windows, Visual Studio 2026, .NET 10 e PowerShell. **Raiz do projeto:**

```text
C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI
```

Continuas a solução existente. Os exemplos são material de estudo para tu implementares. O caderno começa pelos ajustes encontrados na revisão do teu código, sem apagar os endpoints que já construíste.

## 1. Objetivo e resultado

Até agora consultaste pedidos escritos no código. Nesta aula vais enviar dados do cliente e criar um pedido novo através de **POST `/api/tickets`**.

No fim, deves conseguir:

- Enviar um título e uma descrição em JSON pelo Visual Studio ou PowerShell.
- Receber **201 Created**, o pedido criado e o cabeçalho **Location** com o seu endereço.
- Consultar o novo pedido com GET e vê-lo na lista e na contagem.
- Receber **400 Bad Request** para dados inválidos, sem aumentar a contagem.
- Garantir que o servidor escolhe o ID e o estado inicial `Open`.
- Guardar a versão verificada num commit local.

**Duração:** cerca de 3 horas, incluindo prática e revisão. Divide em duas sessões: primeiro até à criação funcionar; depois validação, exercícios e Git.

**Matéria nova:** corpo de um pedido HTTP, DTO, desserialização, validação com atributos, resposta 201, `Location`, `List<T>` e uma pequena proteção de acesso simultâneo. **Revisão:** classes, propriedades, `null`, condições, lambdas, estados HTTP e commits.

**Limite assumido:** os dados continuam apenas na memória de um processo. Reiniciar repõe os três pedidos iniciais. Vamos organizar o armazenamento num serviço na aula 4 e torná-lo persistente na aula 6. Não há base de dados, autenticação ou integração de IA nesta aula.

## 2. Antes de começar

### Avaliação do teu trabalho da aula 2

Revisão de **28 de setembro de 2026**, baseada nos teus ficheiros e em testes de uma cópia isolada.

| Parte avaliada | Evidência e resultado |
|---|---|
| Arranque e controllers | `AddControllers()` e `MapControllers()` estão nos locais corretos; `[Route("api/tickets")]` está definido. |
| Compilação | Sucesso, com **um aviso CS8618** na propriedade `Ticket.Title`. |
| Listagem e detalhe | A lista contém os três pedidos; os IDs existentes dão 200; um ID inexistente dá 404. |
| Exercícios A, B e C | O terceiro pedido, a consulta do título e a contagem estão implementados e responderam corretamente. |
| Comportamentos anteriores | Página inicial, endpoints da aula 1 e exemplo `/eco/21` do WebLab continuam a funcionar. |
| Git | Existem os commits `ef92fd1` e `f4865a8`. As alterações dos exercícios em `TicketsController.cs` ainda estão fora de um commit. |

**Parecer:** podes avançar, fazendo os acertos abaixo no início desta aula. A funcionalidade da aula 2 foi demonstrada; as respostas teóricas não foram avaliadas nesta revisão.

**Sobre o 404 anterior:** na cópia do código atual, GET `/api/tickets` devolveu 200. As tentativas de contactar a aplicação em `localhost:5011` não receberam resposta dentro do tempo de verificação. Isto não permite identificar a causa daquele 404. Antes de acrescentar POST, confirma a listagem na tua execução; o passo 1 explica como.

### Três pistas de revisão antes das correções

1. Em `GetById`, declaraste `ActionResult<Ticket[]>`, mas devolves um pedido. **Pista:** compara «um objeto» com «um array» e revê o tipo entre `< >`.
2. `GetTitleById` e `CountTickets` declaram `ActionResult<Ticket>`, mas devolvem objetos com outros formatos. **Pista:** a aula 2 apresentou um tipo de resultado sem prometer a forma de `Ticket`.
3. `Title` é uma string não anulável sem valor inicial. **Pista:** como inicializavas uma string vazia no exemplo da aula 2? Não desligues os avisos de nulabilidade para esconder a questão.

Estes tipos de retorno não impediram `Ok(...)` de enviar JSON: esse método devolve um resultado HTTP que também pode ser aceite por `ActionResult<T>`. Ainda assim, o tipo declarado deve descrever o contrato; ajuda quem lê o código e a documentação gerada. Os `using` sem utilização podem ser limpos depois: não são a causa do 404.

**As correções que tentaria:** ___________________________________________________

### Revisão curta

Abre `SupportDesk.sln` no Visual Studio. Mantém .NET 10 e os pacotes atuais; os exemplos deste caderno não exigem instalar uma biblioteca adicional.

1. Que diferença existe entre consultar com GET e pedir uma criação com POST?

   **A minha resposta:** ___________________________________________________

2. Se GET `/api/tickets/999` dá 404, isso prova que o servidor está desligado?

   **A minha resposta:** ___________________________________________________

3. Qual é a diferença entre `ActionResult<Ticket>` e `ActionResult<Ticket[]>`?

   **A minha resposta:** ___________________________________________________

4. As alterações que guardaste no editor já pertencem ao último commit?

   **A minha resposta:** ___________________________________________________

**Regra das pastas:** comandos de compilação e Git ficam na raiz `SupportDesk-AI`. Pedidos `Invoke-WebRequest` com URL completa podem correr **em qualquer pasta**, mantendo o servidor ativo. O caminho `/api/tickets` continua a ser uma rota HTTP, não uma pasta do Windows.

## 3. Explicação dos conceitos

### 3.1. O corpo de um POST

O método e o caminho escolhem a operação. O **corpo do pedido** transporta os dados para essa operação. `Content-Type: application/json` informa o servidor de que o corpo está em JSON.

```http
POST http://localhost:5011/api/tickets
Content-Type: application/json

{
  "title": "Impressora da receção sem ligação",
  "description": "A impressão deixou de funcionar esta manhã."
}
```

A linha vazia separa os cabeçalhos do corpo. No navegador, escrever uma URL na barra de endereços faz um GET; isso não envia este POST. Vamos usar os ficheiros `.http` do Visual Studio ou PowerShell.

### 3.2. O cliente não decide todos os campos

Um **DTO**, de *Data Transfer Object*, é um tipo que descreve dados trocados numa operação. `CreateTicketRequest` vai descrever o que aceitamos ao criar: título e descrição. `Ticket` continua a representar o pedido existente, com ID e estado.

| Campo | Quem o fornece nesta operação? |
|---|---|
| `title` | Cliente; o servidor valida. |
| `description` | Cliente; é opcional nesta versão. |
| `id` | Servidor; escolhe o próximo identificador. |
| `status` | Servidor; fixa `Open` na criação. |

Se aceitasses diretamente `Ticket` e guardasses todas as suas propriedades, poderias permitir ao cliente escolher valores que pertencem às regras da aplicação. O DTO torna explícita a entrada pretendida. Não substitui autenticação ou permissões, que virão mais tarde.

Na configuração usada, propriedades JSON desconhecidas são ignoradas. Enviar `"status":"Resolved"` não provoca necessariamente 400; simplesmente não atribui esse estado ao pedido criado. A action construirá um novo `Ticket` com `Open`.

### 3.3. Ler, validar e só depois alterar os dados

**Desserializar** é transformar JSON num objeto C#. `[FromBody]` explicita que o parâmetro vem do corpo. Depois são verificadas regras de validação associadas às propriedades. Com `[ApiController]`, entradas inválidas podem originar 400 **antes de entrar na action**.

```text
JSON → leitura para CreateTicketRequest → validação dos atributos
                                          |
                           inválido → 400, sem criar
                                          |
                               válido → action → Ticket → 201
```

Usaremos `[Required]` para exigir o título e `[StringLength]` para limitar o seu tamanho. São **Data Annotations**, atributos de validação. Não confundas `[Required]` com inicializar uma propriedade: `string.Empty` evita um valor inicial nulo, mas não torna um texto vazio válido.

O corpo de erro terá normalmente `status`, `title` e `errors`. É uma resposta de validação, não uma exceção que tenhas de mostrar ao utilizador como «servidor avariado». Mensagens para JSON malformado podem vir em inglês; interessa identificar o campo ou o problema indicado.

### 3.4. Normalizar não é o mesmo que validar

`Trim()` remove espaços no início e no fim. Vamos usá-lo para guardar um título limpo. Os atributos verificam primeiro o texto recebido. Por isso, `"  a  "` pode satisfazer um mínimo de cinco caracteres antes de `Trim()`, mas ficar com apenas um depois.

Vamos verificar novamente esse mínimo na action, após normalizar, antes de inserir. O limite máximo dos atributos é aplicado ao texto recebido, incluindo espaços nas extremidades. Esta decisão fica explícita nos testes.

**Ponto de verificação P1:** se o título for inválido, deves atribuir um ID e acrescentar o pedido à coleção antes de devolver 400? Explica a ordem que escolherias.

**A minha previsão:** ___________________________________________________

### 3.5. A resposta de criação

**201 Created** comunica que criaste um recurso. **Location** é um cabeçalho que indica onde o cliente pode consultá-lo. `CreatedAtAction` constrói essa resposta com base numa action de consulta e nos valores da rota.

Vamos usar `nameof(GetById)` para obter o nome do método sem o escrever como texto solto. `new { id = ticket.Id }` fornece o valor para `{id:int}`. O terceiro argumento é o corpo da resposta.

GET continua a devolver 200 quando encontra um pedido. POST passa de 405, observado na aula 2, para 201 ou 400 conforme a entrada. Repetir um POST válido cria outro pedido nesta versão: ainda não existe um mecanismo para reconhecer repetições da mesma intenção.

### 3.6. Uma lista partilhada e pedidos simultâneos

Um array tem tamanho fixo; `List<Ticket>` permite acrescentar elementos com `Add`. A contagem da lista chama-se `Count`, enquanto o array tinha `Length`.

Vários pedidos HTTP podem executar ao mesmo tempo. Uma lista comum não deve ser lida enquanto outra operação a modifica sem coordenação. Usaremos um objeto partilhado `TicketsLock` e `lock` nas leituras e na criação. Só uma dessas regiões pode executar de cada vez para o mesmo objeto de sincronização.

O ID e a inserção ficam na mesma região protegida, evitando que duas criações escolham o mesmo ID. Na listagem copiamos os elementos para um array com `ToArray()` enquanto temos o lock; depois o framework pode serializar esse resultado sem percorrer a lista que está a crescer.

É uma solução curta para este laboratório, com leituras e criação num único processo e sem alterações aos objetos já criados. `lock` não guarda dados no disco nem coordena duas instâncias da aplicação. Na aula 4 vamos retirar estas responsabilidades do controller.

## 4. Exemplo guiado: receber uma mensagem sem a guardar

**Objetivo:** observar o percurso JSON → objeto C# → resposta antes de acrescentar armazenamento e validação.

### 4.1. Definir o objeto recebido

**Ficheiro novo:** `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\experiencias\aula-01\WebLab\Models\EchoRequest.cs`.

**Visual Studio:** abre o WebLab existente, cria a pasta `Models` e usa **Add → Class** para criar `EchoRequest.cs`. **Alternativa:** cria a pasta pelo Explorador de Ficheiros e o ficheiro no editor. Escreve:

```csharp
namespace WebLab.Models;

public class EchoRequest
{
    public string Text { get; set; } = string.Empty;
}
```

Este tipo descreve uma propriedade de texto. Não tem regras de validação do produto; é apenas um eco para observar o corpo de um pedido.

### 4.2. Acrescentar o endpoint

**Ficheiro a alterar:** `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\experiencias\aula-01\WebLab\Program.cs`.

No topo, acrescenta `using WebLab.Models;`. Antes de `app.Run()`, mantendo as rotas anteriores, acrescenta:

```csharp
app.MapPost("/eco-mensagem", (EchoRequest request) => new
{
    recebida = request.Text
});
```

`MapPost` responde a POST. O parâmetro é um objeto vindo do corpo JSON; a função devolve outro objeto. Nenhuma coleção é alterada.

**Antes de executar:** com o corpo abaixo, que texto esperas em `recebida`? Isto cria um pedido de suporte?

**A minha previsão:** ___________________________________________________

**Visual Studio:** executa WebLab com o perfil `http` e F5. Acrescenta ao seu ficheiro `.http`:

```http
POST http://localhost:5023/eco-mensagem
Content-Type: application/json

{ "text": "Teste enviado pelo cliente" }
```

Na configuração inspecionada, WebLab usa **5023** e SupportDesk.Web usa **5011**. Confirma a porta efetiva ao arrancar. Clica **Send Request**. Depois de prever, observa o corpo e o estado: este eco devolve **200**, porque não estamos a anunciar a criação de um recurso.

**Alternativa PowerShell — para arrancar, raiz SupportDesk-AI:**

```powershell
Set-Location -LiteralPath 'C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI'
dotnet run --project experiencias/aula-01/WebLab --launch-profile http
```

**Pedido num segundo PowerShell — qualquer pasta:**

```powershell
$echoResponse = Invoke-WebRequest -UseBasicParsing -Method Post -Uri 'http://localhost:5023/eco-mensagem' -ContentType 'application/json' -Body '{"text":"Teste enviado pelo cliente"}'
$echoResponse.StatusCode
$echoResponse.Content
```

**Ponto de verificação P2:** altera apenas o valor de `text`, envia novamente e identifica o que mudou no cliente e no servidor. A resposta esperada está no apoio final.

**A minha explicação:** ___________________________________________________


Para o WebLab com Shift+F5, ou Ctrl+C no terminal que o iniciou. Volta a `SupportDesk.sln`.

## 5. Construção no projeto, passo a passo

### Passo 1 — Fechar a revisão da aula 2

**Objetivo:** começar com as rotas anteriores funcionais e declarações coerentes. **Ficheiros:** `src\SupportDesk.Web\Models\Ticket.cs` e `src\SupportDesk.Web\Controllers\TicketsController.cs`, dentro da raiz indicada no início.

Tenta primeiro as pistas da secção 2. Depois confirma estes resultados, sem substituir o corpo das actions nesta etapa:

| Local | Declaração pretendida |
|---|---|
| Propriedade Title de Ticket | `public string Title { get; set; } = string.Empty;` |
| Consulta por ID | `public ActionResult<Ticket> GetById(int id)` |
| Consulta do título | `public IActionResult GetTitleById(int id)` |
| Contagem | `public IActionResult CountTickets()` |

**Visual Studio:** guarda tudo, compila com Ctrl+Shift+B e confirma que o aviso CS8618 desapareceu. Seleciona **SupportDesk.Web**, perfil **http**, executa com F5 e abre `http://localhost:5011/api/tickets`. Deves ver os três pedidos. Se estiver parado num breakpoint, F5 retoma a execução.

Se ainda houver 404, testa `/api/health` no mesmo endereço. Confirma que estás no produto, que guardaste e reiniciaste o projeto correto e que os atributos do controller e `MapControllers()` correspondem ao caderno. Não acrescentes uma rota duplicada para contornar o diagnóstico.

**Alternativa PowerShell — raiz SupportDesk-AI:**

```powershell
Set-Location -LiteralPath 'C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI'
dotnet build SupportDesk.sln
dotnet run --project src/SupportDesk.Web --launch-profile http
```

Não executes este arranque em simultâneo com F5. Depois de verificar, para a aplicação. Guarda as correções e os exercícios anteriores num commit: **Git Changes → Stage → Commit Staged**. Pelo PowerShell, **na raiz**:

```powershell
git diff -- src/SupportDesk.Web/Models/Ticket.cs src/SupportDesk.Web/Controllers/TicketsController.cs
git add -- src/SupportDesk.Web/Models/Ticket.cs src/SupportDesk.Web/Controllers/TicketsController.cs
git diff --cached
git commit -m 'Concluir consultas e rever tipos da aula 2'
```

**Verificação:** o diff selecionado contém o teu código esperado; `git log -1 --oneline` mostra a versão guardada. Se já fizeste esse commit, não precisas de o repetir.

### Passo 2 — Descrever a entrada e acrescentar a descrição ao pedido

**Objetivo:** separar os dados enviados pelo cliente do pedido que o servidor cria.

**Ficheiro novo:** `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\src\SupportDesk.Web\Contracts\CreateTicketRequest.cs`.

**Visual Studio:** cria a pasta `Contracts` em SupportDesk.Web, depois **Add → Class → CreateTicketRequest.cs**. Escreve:

```csharp
using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Web.Contracts;

public class CreateTicketRequest
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(120, MinimumLength = 5,
        ErrorMessage = "O título deve ter entre 5 e 120 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "A descrição pode ter no máximo 2000 caracteres.")]
    public string? Description { get; set; }
}
```

`string?` permite que a descrição seja `null`, por exemplo quando é omitida. O título deve existir e respeitar os limites. `[Required]` rejeita também um texto só com espaços. As mensagens são os textos que queremos associar às regras.

**Alternativa para criar a pasta — PowerShell, raiz SupportDesk-AI:**

```powershell
New-Item -ItemType Directory -Path '.\src\SupportDesk.Web\Contracts' -Force
```

Cria o ficheiro no editor; o comando só cria a pasta. Não precisas de recriar o projeto.

**Ficheiro a alterar:** `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\src\SupportDesk.Web\Models\Ticket.cs`. Acrescenta esta propriedade dentro de `Ticket`, mantendo Id, Title e Status:

```csharp
public string Description { get; set; } = string.Empty;
```

Os pedidos iniciais passam a ter descrição vazia por predefinição. **Verificação:** compila. **Resultado esperado:** o projeto continua a compilar sem avisos; ainda não existe uma action POST.

### Passo 3 — Preparar a coleção que pode crescer

**Objetivo:** guardar criações durante a execução atual. **Ficheiro a alterar:** `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\src\SupportDesk.Web\Controllers\TicketsController.cs`.

No topo, acrescenta `using SupportDesk.Web.Contracts;`. Dentro da classe, substitui o campo `Ticket[] Tickets` por estes campos, mantendo os três pedidos que já tens:

```csharp
    private static readonly List<Ticket> Tickets = new()
    {
        new Ticket { Id = 1, Title = "Não consigo entrar", Status = "Open" },
        new Ticket { Id = 2, Title = "Fatura com valor incorreto", Status = "InProgress" },
        new Ticket { Id = 3, Title = "Impressora indisponível", Status = "Open" }
    };

    private static readonly object TicketsLock = new();
    private static int NextId = 4;
```

`List<Ticket>` vem de `System.Collections.Generic`, já disponível neste projeto. `new()` usa o tipo declarado à esquerda. Os campos são `static` para serem partilhados pelas instâncias do controller dentro do processo. Sem essa partilha, criar uma coleção nova para cada pedido HTTP faria desaparecer o estado entre chamadas.

`NextId` começa em 4 porque os dados iniciais têm os IDs 1, 2 e 3. Se alterares esses dados no código, ajusta o início para um número superior ao maior ID inicial. `readonly` protege a referência à lista, não impede `Add`.

**Verificação intermédia:** se compilares agora, podes ver um erro na antiga utilização de `Tickets.Length`. É a diferença esperada entre array e lista; o passo seguinte atualiza as leituras. Não ignores o erro e tentes correr uma compilação antiga.

### Passo 4 — Adaptar as consultas existentes

**Objetivo:** preservar a aula 2 e coordenar leituras com futuras inserções. **Ficheiro:** o mesmo `TicketsController.cs`.

Substitui cada action existente pelo bloco correspondente. **Não acrescentes uma segunda cópia da mesma action ou rota.** As mudanças são pequenas: lock, cópia da lista e Count.

```csharp
    [HttpGet]
    public ActionResult<Ticket[]> GetAll()
    {
        lock (TicketsLock)
        {
            return Ok(Tickets.ToArray());
        }
    }
```

A resposta continua a ser um array, por isso `ActionResult<Ticket[]>` faz sentido. `ToArray()` cria a cópia sob proteção; a serialização acontece depois. Nesta aula não alteramos os objetos já criados, apenas acrescentamos objetos novos.

```csharp
    [HttpGet("{id:int}")]
    public ActionResult<Ticket> GetById(int id)
    {
        lock (TicketsLock)
        {
            var ticket = Tickets.FirstOrDefault(ticket => ticket.Id == id);
            if (ticket is null)
            {
                return NotFound();
            }

            return Ok(ticket);
        }
    }
```

O lock também é libertado quando o código faz `return`; não precisas de o libertar manualmente. Mantemos a distinção entre encontrar e não encontrar.

```csharp
    [HttpGet("{id:int}/title")]
    public IActionResult GetTitleById(int id)
    {
        lock (TicketsLock)
        {
            var ticket = Tickets.FirstOrDefault(ticket => ticket.Id == id);
            if (ticket is null)
            {
                return NotFound();
            }

            return Ok(new { id = ticket.Id, title = ticket.Title });
        }
    }
```

```csharp
    [HttpGet("count")]
    public IActionResult CountTickets()
    {
        lock (TicketsLock)
        {
            return Ok(new { total = Tickets.Count });
        }
    }
```

**Verificação — Visual Studio:** guarda, compila, executa e repete lista, detalhe, título e contagem. **PowerShell — raiz:** `dotnet build SupportDesk.sln`, seguido do arranque habitual. **Resultado esperado:** continuam a responder como na aula 2, com o campo `description` adicional no pedido completo. Para a aplicação antes de continuar.

**Ponto de verificação P3:** porque não basta proteger apenas `Tickets.Add(...)`, deixando a atribuição do ID e a listagem sem coordenação?

**A minha explicação:** ___________________________________________________

### Passo 5 — Criar o pedido e responder 201

**Objetivo:** ligar entrada válida, criação e resposta HTTP. **Ficheiro:** `TicketsController.cs`, dentro da classe, após as actions anteriores.

Vamos normalizar o título antes de mexer no armazenamento. `ModelState` reúne erros de entrada. `ValidationProblem(...)` produz uma resposta 400 com esses erros. Depois construímos um objeto novo, escolhemos o ID e acrescentamo-lo à lista na mesma região protegida.

Acrescenta esta action:

```csharp
    [HttpPost]
    public ActionResult<Ticket> Create([FromBody] CreateTicketRequest request)
    {
        var title = request.Title.Trim();
        if (title.Length < 5)
        {
            ModelState.AddModelError(nameof(request.Title),
                "O título deve ter pelo menos 5 caracteres depois de remover espaços nas extremidades.");
            return ValidationProblem(ModelState);
        }

        Ticket ticket;
        lock (TicketsLock)
        {
            ticket = new Ticket
            {
                Id = NextId++,
                Title = title,
                Description = request.Description?.Trim() ?? string.Empty,
                Status = "Open"
            };
            Tickets.Add(ticket);
        }

        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }
```

`NextId++` usa o valor atual e incrementa-o para a criação seguinte. `?.` só chama Trim se a descrição não for nula; `?? string.Empty` escolhe texto vazio quando for nula. O objeto é novo e o estado é atribuído pelo servidor.

Se o corpo omitir o título, tiver `null`, um título vazio ou demasiado comprido, `[ApiController]` e os atributos rejeitam a entrada antes desta action. O `if` trata a regra adicional depois de Trim. Não precisas de acrescentar um segundo teste genérico `ModelState.IsValid` para repetir a validação automática.

Mantém `[ApiController]`, `[Route("api/tickets")]`, `AddControllers()` e `MapControllers()` como estavam. **Verificação:** guarda tudo e compila; depois executa o produto no perfil `http`. O próximo passo verifica tanto a resposta como a alteração real dos dados.

**Ponto de verificação P4:** em `CreatedAtAction`, o que representam `nameof(GetById)`, `new { id = ticket.Id }` e o último `ticket`? Explica antes de testar.

**A minha explicação:** ___________________________________________________

### Passo 6 — Testar pelo Visual Studio

**Objetivo:** provar que criaste um recurso consultável e que rejeitas entradas inválidas. **Ficheiro:** `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\src\SupportDesk.Web\SupportDesk.http`.

Abre esse ficheiro ou cria-o com **Add → New Item → HTTP File**. Usa apenas uma declaração `@baseUrl` no ficheiro. Confirma a porta real; a configuração inspecionada continua a usar 5011.

```http
@baseUrl = http://localhost:5011

### Observar a contagem antes
GET {{baseUrl}}/api/tickets/count

### Criar um pedido válido
POST {{baseUrl}}/api/tickets
Content-Type: application/json

{
  "title": "Impressora da receção sem ligação",
  "description": "A impressão deixou de funcionar esta manhã."
}

### Consultar o criado: substitui 4 pelo ID recebido
GET {{baseUrl}}/api/tickets/4

### Entrada inválida
POST {{baseUrl}}/api/tickets
Content-Type: application/json

{ "title": "" }

### Espaços que escondem um título demasiado curto
POST {{baseUrl}}/api/tickets
Content-Type: application/json

{ "title": "  a  " }

### Voltar a observar a contagem
GET {{baseUrl}}/api/tickets/count
```

Executa um pedido de cada vez com **Send Request**. Depois de um arranque novo, a contagem começa em 3. O primeiro POST válido recebe 201, `id: 4` e `status: "Open"`. Abre **Headers** na resposta e encontra `Location`; faz GET nesse endereço e compara o ID com o corpo do POST.

Se já enviaste outros POST válidos, o ID pode ser diferente. Usa sempre o recebido; não assumas que será 4 em todas as tentativas. Os dois POST inválidos devem dar 400. Compara a contagem antes e depois deles: não deve mudar.

Experimenta também um POST com título válido e os campos extra `"id":999` e `"status":"Resolved"`. Na configuração atual, são ignorados: o servidor atribui o próximo ID e `Open`. Esse teste **cria um pedido válido**, portanto aumenta a contagem. Não o contes como um teste de rejeição.

**Ponto de verificação P5:** envia duas vezes o mesmo POST válido. Prevê primeiro se haverá um pedido ou dois, e justifica. O comportamento fica explicado no apoio final.

**A minha previsão e observação:** ___________________________________________________

### Passo 7 — A mesma verificação em PowerShell

**Objetivo:** compreender o pedido independentemente da ferramenta. **Pasta: qualquer uma.** Mantém SupportDesk.Web a correr no Visual Studio ou noutro terminal.

```powershell
$baseUrl = 'http://localhost:5011'
$before = Invoke-RestMethod -Uri "$baseUrl/api/tickets/count"

$json = @{
    title = 'Impressora da receção sem ligação'
    description = 'A impressão deixou de funcionar esta manhã.'
} | ConvertTo-Json

$body = [System.Text.Encoding]::UTF8.GetBytes($json)
$response = Invoke-WebRequest -UseBasicParsing -Method Post -Uri "$baseUrl/api/tickets" -ContentType 'application/json; charset=utf-8' -Body $body

$response.StatusCode
$response.Headers['Location']
$created = $response.Content | ConvertFrom-Json
$created

$detail = Invoke-RestMethod -Uri "$baseUrl/api/tickets/$($created.id)"
$detail
$after = Invoke-RestMethod -Uri "$baseUrl/api/tickets/count"
$after.total - $before.total
```

O bloco `@{ ... }` é uma tabela de propriedades PowerShell. `ConvertTo-Json` gera o texto e UTF8 produz os bytes, preservando os acentos também no Windows PowerShell 5.1. `Invoke-RestMethod` interpreta automaticamente o JSON recebido; `Invoke-WebRequest` permite observar facilmente o estado e os cabeçalhos. `$(...)` avalia `created.id` dentro da string da URL.

**Resultado esperado:** 201, cabeçalho Location, o pedido criado com `Open`, o mesmo pedido no GET e diferença de contagem igual a 1. Se fizeste o teste pelo Visual Studio e depois este, criaste dois pedidos: escolhe uma ferramenta ou contabiliza as duas criações.

Para observar um erro de validação, **em qualquer pasta**:

```powershell
try {
    Invoke-WebRequest -UseBasicParsing -Method Post -Uri "$baseUrl/api/tickets" -ContentType 'application/json' -Body '{"title":""}'
}
catch {
    if ($null -ne $_.Exception.Response) {
        [int]$_.Exception.Response.StatusCode
        $_.ErrorDetails.Message
    }
    else {
        $_.Exception.Message
    }
}
```

O resultado HTTP esperado é 400. O conteúdo detalhado mostrado pelo PowerShell pode variar com a versão; podes inspecionar o JSON de erro no painel de resposta do ficheiro `.http`. Confirma sempre a contagem depois de uma entrada inválida; uma mensagem de erro sozinha não prova que não houve uma alteração.

### Passo 8 — Verificar limites e guardar a versão

**Objetivo:** confirmar o contrato antes do commit. **Ficheiros:** os que alteraste no produto; não há uma nova solução nem configuração de base de dados.

| Entrada ou ação | Resultado esperado nesta versão |
|---|---|
| Título válido; descrição omitida | 201; descrição guardada como texto vazio. |
| Título com espaços nas extremidades, mas válido após Trim | 201; título guardado sem esses espaços. |
| Título omitido, `null`, vazio ou só com espaços | 400; contagem inalterada. |
| Título com 4 caracteres ou 121 caracteres | 400; contagem inalterada. |
| Título `"  a  "` | 400 pela regra após Trim; contagem inalterada. |
| Descrição com 2001 caracteres | 400; contagem inalterada. |
| JSON incompleto, por exemplo sem a chaveta final | 400; a action não recebe um objeto válido. |
| Corpo JSON enviado como `text/plain` | 415 Unsupported Media Type; formato de conteúdo não aceite por esta action. |
| GET no endereço Location de uma criação | 200; mesmo ID e dados. |
| Parar e voltar a executar | Reaparecem apenas os três pedidos iniciais; os criados em memória desaparecem. |

Para gerar um caso comprido em PowerShell, usa `('x' * 121)` como valor de `title` na tabela de propriedades do passo 7. Não coloques essa expressão literalmente dentro de um ficheiro JSON; é uma expressão PowerShell que gera texto.

Mantém lista, detalhe, título, contagem, página inicial e `/api/health` a funcionar. Reinicia de propósito no fim para comprovar a limitação da memória. Um separador antigo do navegador pode continuar a mostrar uma resposta anterior: faz um novo pedido.

**Visual Studio:** revê o diff em Git Changes e seleciona apenas as alterações que pretendes guardar. **PowerShell — raiz SupportDesk-AI:**

```powershell
Set-Location -LiteralPath 'C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI'
git status
git diff -- src/SupportDesk.Web
git add -- src/SupportDesk.Web
git diff --cached
git commit -m 'Aula 3: criar pedidos com validação'
git log -1 --oneline
```

**Resultado esperado:** um commit com o DTO, o modelo, o controller e o ficheiro de pedidos que escreveste. O WebLab pode ser guardado num commit separado. As regras do teu `.gitignore` continuam a excluir `bin`, `obj` e o estado do Visual Studio.

## 6. Pontos de verificação

Confirma estes pontos antes dos exercícios. Se uma resposta te custar, faz uma experiência pequena e regista o resultado.

| Ponto | O que deves conseguir mostrar |
|---|---|
| P1 | Uma entrada inválida não obtém ID nem entra na coleção. |
| P2 | Um POST transporta dados no corpo até um objeto C#. |
| P3 | Atribuir o ID e inserir pertencem à mesma região protegida; leituras também precisam de coordenação. |
| P4 | Location aponta para o recurso criado, não para uma página escolhida ao acaso. |
| P5 | Repetir um POST cria novos pedidos nesta versão. |

**Experiência adicional:** coloca um breakpoint no início de `Create`. Compara um título vazio e `"  a  "`. Um pode ser rejeitado antes da action e o outro pela regra escrita dentro dela. Usa F5 para continuar quando o depurador parar; depois remove o breakpoint.

**O que observei no depurador:** ___________________________________________________

**O conceito que ainda preciso de rever:** ___________________________________________________

## 7. Exercícios com apoio progressivamente menor

Mantém o ID e o estado sob controlo do servidor. Os testes inválidos devem sempre preservar a contagem. As pistas e as soluções estão recolhidas no final do HTML.

### Exercício A — Acompanhado: um limite mais curto

Altera o máximo do título para **80 caracteres**, mantendo o mínimo 5. Atualiza a mensagem de erro para corresponder à regra. A pista está no atributo que já conheces.

**Aceitação:** um título com exatamente 80 caracteres cria; um com 81 dá 400 e não altera a contagem. Um título com 4 continua inválido. Usa texto sem espaços nas extremidades nestes testes de limite.

**Alteração e resultados:** ___________________________________________________

### Exercício B — Menos ajuda: exigir uma descrição útil

A descrição passa a ser obrigatória, com mínimo de **10 caracteres depois de Trim** e máximo de **2000 caracteres na entrada**. Podes manter `string?` no DTO para representar a possibilidade de receber `null`; a validação deve rejeitá-lo.

**Aceitação:** descrição omitida, `null`, vazia, só espaços, com nove caracteres ou que fique com menos de dez após Trim dá 400. Uma descrição válida é guardada limpa. Um erro não acrescenta um pedido. Explica porque um atributo de comprimento, sozinho, pode não bastar depois da normalização.

**A minha estratégia:** ___________________________________________________

### Exercício C — Autónomo: prioridade validada

Acrescenta `Priority` como inteiro ao DTO e ao modelo. Permite apenas **1, 2 ou 3** e usa **2** por predefinição quando a propriedade é omitida. Copia o valor validado para o pedido criado. Nesta aula a prioridade é escolhida pelo cliente; o estado continua a ser escolhido pelo servidor.

Para definir um intervalo numérico, existe o atributo `[Range(minimo, maximo)]` no mesmo namespace das validações anteriores. Aplica-o à propriedade correta e tenta primeiro sem abrir a solução.

**Aceitação:** omitir prioridade cria com 2; enviar 1 ou 3 preserva esse valor; enviar 0 ou 4 dá 400 e mantém a contagem. Confirma o valor tanto no corpo de criação como no GET pelo ID. Se fizeste o exercício B, envia também uma descrição válida nestes testes.

**Como garanti que o valor chega ao GET:** ___________________________________________________

## 8. Erros frequentes e como os investigar

| Sintoma | Causa provável | Como investigar e corrigir |
|---|---|---|
| CS8618 em `Ticket.Title` | Propriedade não anulável sem inicialização. | Revê a declaração do passo 1. Inicializar não substitui as regras do DTO. |
| `List<Ticket>` não contém `Length` | Ficou código do array anterior. | Usa Count para a lista e procura outras utilizações antigas no controller. |
| `CreateTicketRequest` ou os atributos não são reconhecidos | Namespace ou ficheiro incorreto. | Confirma `Contracts`, o namespace e os dois `using` apresentados. Não acrescentes pacotes sem identificar a causa. |
| GET funciona, mas POST dá 405 | Processo antigo ou action sem `[HttpPost]`. | Guarda, recompila, reinicia a aplicação correta e confirma o método no cliente. |
| POST dá 404 | URL, porta ou rota incorreta. | Confirma o mesmo endereço base em que GET `/api/tickets` funciona. O POST não usa o caminho de uma pasta. |
| O pedido parece ficar à espera | Servidor pausado num breakpoint ou sem responder. | Confirma o estado do depurador e usa F5 para continuar. Depois compara o endereço e o processo em execução. |
| Resposta 415 | Content-Type ausente ou inadequado para o corpo. | Usa `application/json` e a linha vazia antes do corpo no ficheiro `.http`. |
| Resposta 400 sem entrar na action | Falha de leitura ou validação automática. | Lê `errors`, confirma o JSON, os nomes e os limites do DTO. Isso pode ser o comportamento correto. |
| O título normalizado fica demasiado curto | Validaste só o texto antes de Trim. | Faz a verificação do passo 5 antes de alterar a coleção. |
| POST dá 500 com «No route matches the supplied values» | `CreatedAtAction` não conseguiu construir o endereço. | Confirma `nameof(GetById)`, a rota `{id:int}` e a propriedade `id` nos valores fornecidos. O problema pode surgir ao formatar a resposta, depois da inserção; consulta a lista antes de repetir pedidos. |
| Dois cliques criam dois pedidos | Enviaste dois POST válidos. | Este é o contrato atual; observa os IDs distintos. Não há reconhecimento de duplicados nesta aula. |
| Os pedidos criados desaparecem | Reiniciaste o processo ou criaste dados noutra instância. | Confirma o endereço e a limitação da memória. Commit guarda código, não os objetos vivos da aplicação. |
| A contagem cresce depois de devolver 400 | Inserção feita antes de validar. | Coloca os caminhos de rejeição antes da atribuição do ID e de Add. |
| O servidor guardou o estado enviado pelo cliente | Aceitaste ou copiaste campos indevidos. | Mantém o DTO só com a entrada autorizada e atribui Open no novo Ticket. |

**O meu diagnóstico: sintoma, hipótese, teste e resultado:** ___________________________________________________

## 9. Revisão sem consultar

1. Porque recebemos `CreateTicketRequest` e construímos um novo `Ticket`, em vez de aceitar o modelo inteiro?

   **A minha resposta:** ___________________________________________________

2. Como pode uma entrada devolver 400 sem executar a primeira linha da action? Em que caso da aula a action é necessária para rejeitar o título?

   **A minha resposta:** ___________________________________________________

3. Qual é a diferença entre inicializar uma string, validar o seu tamanho e aplicar Trim?

   **A minha resposta:** ___________________________________________________

4. Que informação acrescentam 201 e Location? Como verificas se o endereço corresponde ao pedido criado?

   **A minha resposta:** ___________________________________________________

5. Porque usamos estado estático e lock nesta versão? O que continua a falhar se reiniciarmos ou executarmos dois processos separados?

   **A minha resposta:** ___________________________________________________

### Reconstrução curta

Sem copiar, escreve num rascunho o DTO com as regras do título e uma sequência de passos para Create: validar, normalizar, verificar, atribuir ID, guardar e responder. Indica quais acontecem antes da action. Compara depois com o código e identifica uma diferença concreta. Não registes uma segunda action com a mesma rota para experimentar.

**O que reconstruí e o que precisei de rever:** ___________________________________________________

## 10. Critérios de conclusão

### Consigo fazer

- [ ] Compilar o produto sem o aviso CS8618 identificado na revisão. Se não: passo 1.
- [ ] Enviar um POST JSON válido e receber 201. Se não: passos 2 e 5–7.
- [ ] Consultar o recurso indicado em Location e confirmar os mesmos dados. Se não: passos 5–6.
- [ ] Confirmar que dados inválidos devolvem 400 e não mudam a contagem. Se não: passos 6–8.
- [ ] Manter lista, detalhe, título, contagem e endpoints anteriores a funcionar. Se não: passo 4.
- [ ] Guardar num commit apenas a versão que verifiquei. Se não: passo 8.

### Consigo explicar

- [ ] A diferença entre DTO de entrada e modelo de pedido. Se não: secção 3.2.
- [ ] Quem decide o ID e o estado e porque não os copio do cliente. Se não: passo 5.
- [ ] A diferença entre validação automática e verificação depois de Trim. Se não: secções 3.3–3.4.
- [ ] O significado de 201, 400 e 415 neste contrato. Se não: passos 6–8.
- [ ] Porque lock protege o acesso simultâneo mas não oferece persistência. Se não: secção 3.6.
- [ ] Porque repetir um POST válido pode criar outro pedido. Se não: P5.

Se saltares exercícios, indica quais na próxima revisão. Vamos distinguir a funcionalidade construída das decisões que já consegues explicar.

## 11. Resumo e revisão posterior

O cliente envia uma entrada delimitada pelo DTO. O framework lê e valida os dados; a action normaliza e aplica regras adicionais antes de alterar o estado. O servidor escolhe ID e estado, guarda temporariamente o pedido e devolve 201 com um endereço de consulta. GET e os exercícios anteriores continuam disponíveis.

**Amanhã — 10 minutos:** faz um POST válido e outro inválido, compara as contagens e segue Location. Explica por que motivo um `status` extra enviado no JSON não muda o estado inicial.

**Daqui a uma semana — 15 minutos:** reconstrói o DTO sem consultar, testa um valor no limite e outro fora dele, e explica o que se perde ao reiniciar. Mostra no histórico o commit da aula.

**Ligação à aula 4:** o controller está a acumular leitura HTTP, armazenamento e regras. Vamos introduzir serviços e injeção de dependências, reorganizar o código e acrescentar estados e comentários. O comportamento HTTP que acabaste de verificar servirá de referência para confirmar que a reorganização não o estragou.

**Dúvidas que quero trazer:** ___________________________________________________

## Apoio final — Pistas e soluções comentadas

Abre depois da tua tentativa. São alterações aos ficheiros existentes, não novos controllers nem novas rotas.

### Revisão e pontos de verificação

- GET consulta; POST solicita uma operação de criação neste contrato. Um 404 é uma resposta do servidor, não uma ligação recusada. `Ticket` representa um objeto; `Ticket[]` representa uma coleção. Guardar no editor não faz commit.
- **P1:** validar antes de atribuir ID e inserir. Uma entrada rejeitada não deve alterar a coleção.
- **Exemplo guiado:** o eco devolve `{"recebida":"Teste enviado pelo cliente"}` com 200. Não cria um pedido de suporte.
- **P2:** mudou o valor no corpo do pedido e, consequentemente, a propriedade Text do objeto recebido; a lógica compilada permaneceu igual.
- **P3:** coordenar só Add não impede duas chamadas de escolher o mesmo ID, nem protege uma leitura enquanto outra chamada altera a lista.
- **P4:** `nameof` identifica a action, os valores preenchem a rota e o último argumento é o corpo da resposta. O GET em Location deve devolver o mesmo pedido.
- **P5:** duas chamadas válidas criam dois objetos com IDs distintos. Um POST repetido não é automaticamente reconhecido como a mesma criação.
- No depurador, um título vazio é rejeitado pela validação automática; `"  a  "` passa o comprimento da entrada e é rejeitado depois de Trim na action.

### Exercício A — Pista e solução

**Pista:** altera a regra declarada no DTO e a mensagem que explica essa mesma regra.

```csharp
[StringLength(80, MinimumLength = 5,
    ErrorMessage = "O título deve ter entre 5 e 80 caracteres.")]
```

Mantém `[Required]`. A verificação do mínimo após Trim continua a ser 5. Testa 80 e 81, porque um único caso «normal» não confirma o limite.

### Exercício B — Pista e solução

**Pista:** combina a exigência de um valor com o intervalo na entrada e verifica o tamanho do texto normalizado antes de inserir.

Substitui os atributos da descrição no DTO por:

```csharp
[Required(ErrorMessage = "A descrição é obrigatória.")]
[StringLength(2000, MinimumLength = 10,
    ErrorMessage = "A descrição deve ter entre 10 e 2000 caracteres.")]
public string? Description { get; set; }
```

Na action Create, depois da verificação do título e **antes de `Ticket ticket;`**, acrescenta:

```csharp
        var description = request.Description?.Trim() ?? string.Empty;
        if (description.Length < 10)
        {
            ModelState.AddModelError(nameof(request.Description),
                "A descrição deve ter pelo menos 10 caracteres depois de remover espaços nas extremidades.");
            return ValidationProblem(ModelState);
        }
```

No inicializador do novo Ticket, substitui a atribuição da descrição por `Description = description,`. A regra é aplicada antes de modificar a coleção. Continuas a validar no servidor mesmo que uma futura página também impeça entradas vazias.

### Exercício C — Pista e solução

**Pista:** são necessárias três peças: entrada validada, propriedade no modelo e atribuição na criação. Alterar apenas o DTO não guarda o valor.

Dentro de `CreateTicketRequest`, acrescenta:

```csharp
[Range(1, 3, ErrorMessage = "A prioridade deve ser 1, 2 ou 3.")]
public int Priority { get; set; } = 2;
```

Dentro de `Ticket`, acrescenta:

```csharp
public int Priority { get; set; } = 2;
```

No inicializador da action Create, acrescenta `Priority = request.Priority,`. O valor inicial 2 é usado quando a propriedade é omitida; valores explícitos 0 ou 4 são rejeitados pelo atributo. Os pedidos iniciais também terão 2 por predefinição.

### Revisão final — Ideias essenciais

1. O DTO descreve a entrada; o modelo inclui dados definidos pelo servidor. A criação liga os dois explicitamente.
2. `[ApiController]` devolve erros de leitura/validação antes da action. As regras sobre o texto normalizado são verificadas depois.
3. Inicializar evita um valor inicial nulo; validar verifica condições; normalizar transforma o texto.
4. 201 comunica criação e Location permite consultar o resultado. Compara ID, título e estado no POST e no GET.
5. A lista e o contador estáticos vivem no processo. O lock coordena acesso nesse processo. Nenhum dos dois oferece armazenamento persistente ou partilha entre processos.

## Notas livres

________________________________________________________________________

________________________________________________________________________

## Referências oficiais para aprofundar

O conteúdo necessário está explicado no caderno. Estas referências permitem confirmar as APIs usadas:

- [APIs com controllers, entrada e respostas de validação](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-10.0).
- [Validação de modelos em ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-10.0).
- [Resultados de actions, incluindo CreatedAtAction](https://learn.microsoft.com/en-us/aspnet/core/web-api/action-return-types?view=aspnetcore-10.0).
- [Required](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.requiredattribute?view=net-10.0), [StringLength](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.stringlengthattribute?view=net-10.0) e [Range](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.rangeattribute?view=net-10.0).
- [Sincronização com lock em C#](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/lock).
- [Pedidos em ficheiros .http no Visual Studio](https://learn.microsoft.com/en-us/aspnet/core/test/http-files?view=aspnetcore-10.0).

## Nota de verificação deste material

Preparado em 28 de setembro de 2026. A avaliação da aula 2 foi feita numa cópia dos teus ficheiros: compilação com um aviso CS8618, 16 verificações HTTP no produto e quatro no WebLab. Foi lido o estado do Git, sem alterar os teus commits ou a seleção de ficheiros. O código do aluno não foi corrigido automaticamente.

Os exemplos e as soluções desta aula foram extraídos do caderno e compilados em cópias separadas com o SDK .NET 10.0.401, sem erros nem avisos. Os testes confirmaram criação com 201, Location consultável, campos controlados pelo servidor, limites de entrada, normalização, respostas 400 e 415 sem inserção, IDs distintos em criações simultâneas e perda dos dados temporários ao reiniciar. Foram também verificados o eco do WebLab e os três exercícios, incluindo prioridade e descrição obrigatória.

O HTML foi verificado com três aulas, navegação, soluções recolhidas, respostas independentes, exportação/importação e os 120 identificadores de respostas anteriores preservados. Nenhum código dos exercícios, correção ou commit foi aplicado à tua aplicação. A sequência de menus do Visual Studio não foi executada na tua instalação. As verificações técnicas não comprovam, por si só, a compreensão das respostas teóricas.
