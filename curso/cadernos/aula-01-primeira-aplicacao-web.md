# Caderno 01 — A primeira aplicação web do SupportDesk AI

[Voltar ao índice do curso](00-indice.md)

**Nome:** ____________________ · **Data de início:** ____________________ · **Data de revisão:** ____________________

**Ambiente:** Windows, Visual Studio 2026 ou PowerShell, e .NET 10. **Pasta do projeto:**

```text
C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI
```

Este caderno é para estudar e executar. Os comandos e o código apresentados são instruções para tu realizares os exercícios. O curso anterior permanece guardado; usa a sequência deste caderno para começar.

**Escolhe como trabalhar:** mantém os comandos se preferires PowerShell, ou segue os blocos **Opção Visual Studio** junto de cada passo. São alternativas para o mesmo resultado; não cries nem inicies o mesmo projeto duas vezes. Se já tens o WebLab, abre-o e retoma onde ficaste.

## 1. Objetivo e resultado

Nesta aula vais passar de «escrevo programas em C#» para «consigo executar um programa em C# que recebe pedidos de um navegador».

No fim terás:

- Uma experiência pequena, chamada **WebLab**, que responde com texto.
- A solução **SupportDesk.sln** e o projeto **SupportDesk.Web**.
- Uma página inicial com o nome e a finalidade do produto.
- Um endereço **GET /api/health** que responde com dados em JSON.

Esse endpoint será a primeira forma de verificar que o processo do SupportDesk AI está a responder. Nesta aula, `status = "ok"` é um valor definido por nós: **não verifica uma base de dados, um modelo de IA ou a disponibilidade de todo o sistema**. Vamos melhorar a observação do sistema na aula 21.

**Duração estimada:** 2h30–3h30, incluindo leitura, prática e exercícios. Podes fazer duas sessões: secções 1–4 e, depois, secções 5–11. Faz uma pausa curta entre sessões.

**Matéria nova:** aplicação web, cliente e servidor, HTTP, URL, endpoint, SDK, projeto, solução e JSON.

**Revisão de C#:** variáveis, métodos, objetos, `new`, expressões lambda e instruções de nível superior. As duas últimas são explicadas abaixo; não precisam de estar dominadas para começares.

## 2. Antes de começar

### Ferramentas necessárias

- Windows e PowerShell. Podes abrir PowerShell pelo menu Iniciar ou num separador do Windows Terminal.
- .NET 10 SDK. Na inspeção de 27 de setembro de 2026, a versão instalada era **10.0.401**.
- Um editor de texto/código. Se usares Visual Studio, deve suportar .NET 10; a linha Visual Studio 2026 é adequada. Também podes usar um editor que já conheças e executar os comandos no PowerShell.
- Um navegador.

Não precisas de PostgreSQL, Docker, conta de IA ou de contratar alojamento nesta aula. A aplicação vai executar no teu computador.

#### Opção Visual Studio — preparar o ambiente

Abre o **Visual Studio Installer**, encontra o **Visual Studio 2026** e escolhe **Modificar (Modify)**. Em **Cargas de trabalho (Workloads)**, confirma **ASP.NET e desenvolvimento Web (ASP.NET and web development)**. Aplica a alteração se estiver em falta e volta a abrir o Visual Studio. Se tens várias versões instaladas, confirma esta opção na versão que vais usar.

Neste caderno, **Explorador de Soluções** corresponde a **Solution Explorer**; podes abri-lo em **Ver (View) → Solution Explorer**. Para os comandos que quiseres executar dentro do editor, usa **Ver → Terminal** e escolhe **PowerShell**. Os nomes traduzidos dos menus podem variar; os nomes ingleses ajudam a identificá-los.

### Diagnóstico: responde antes de começar

São perguntas para identificarmos o que precisas de rever. «Ainda não sei» é uma resposta válida e não impede começar.

1. Qual é a diferença entre declarar uma classe e criar um objeto dessa classe?

   **A minha resposta:** ___________________________________________________

2. O que significa devolver um valor com `return`? Em que difere de o escrever na consola?

   **A minha resposta:** ___________________________________________________

3. Já viste `() => "Olá"`? O que pensas que representa?

   **A minha resposta:** ___________________________________________________

4. Quando escreves um endereço no navegador, onde achas que executa o código C# de um site?

   **A minha resposta:** ___________________________________________________

5. O que achas que significa `localhost`?

   **A minha resposta:** ___________________________________________________

**Notas e dúvidas iniciais:**

________________________________________________________________________

________________________________________________________________________

## 3. Explicação dos conceitos

### 3.1. C#, .NET e ASP.NET Core

Já sabes usar C# para expressar instruções. A linguagem, por si só, não fornece tudo o que é necessário para compilar, executar, ler ficheiros ou receber pedidos web.

**.NET** é a plataforma que fornece ferramentas, bibliotecas e um ambiente de execução. O **runtime** é a parte que executa aplicações .NET. O **SDK**, ou kit de desenvolvimento, inclui as ferramentas para construir aplicações, além do necessário para as executar.

**ASP.NET Core** fornece as funcionalidades de desenvolvimento web em .NET. Por exemplo, consegue receber um pedido HTTP, escolher a função que deve tratar esse pedido e devolver uma resposta.

| Termo | Pergunta a que responde neste projeto |
|---|---|
| C# | Em que linguagem escrevo a lógica? |
| .NET | Que plataforma compila e executa o código? |
| SDK | Que ferramentas uso para criar e construir a aplicação? |
| ASP.NET Core | O que me permite receber pedidos web e responder? |

**Ponto de verificação P1:** explica a diferença entre C# e ASP.NET Core usando o exemplo de um pedido de suporte.

**A minha explicação:** ___________________________________________________

### 3.2. Um programa de consola e um servidor web

Um programa de consola simples pode executar uma sequência e terminar. Um **servidor web** mantém-se em execução à espera de pedidos. Nesta aula, o processo .NET no teu computador desempenha esse papel.

O **cliente** é quem faz o pedido. Aqui será o navegador; mais tarde pode ser outra aplicação. O servidor recebe o pedido, executa código e envia uma resposta. Cliente e servidor são papéis, não obrigatoriamente dois computadores diferentes.

```text
Navegador                       Aplicação ASP.NET Core
   |                                      |
   |  pedido: GET /api/health              |
   |------------------------------------->|
   |                                      | escolhe a função
   |                                      | executa C#
   |  resposta: estado 200 + dados JSON    |
   |<-------------------------------------|
   |                                      |
mostra o resultado                continua à espera de pedidos
```

O código C# deste exemplo executa no servidor. O navegador recebe o resultado; não precisa de executar esse C#.

### 3.3. HTTP, URL e porta

**HTTP** é o protocolo: as regras usadas para trocar pedidos e respostas web. Um pedido indica, entre outras coisas, um **método**, como GET, e um caminho. Nesta aula, **GET** significa que queremos consultar uma representação, sem pedir uma alteração aos dados.

Uma **URL** é um endereço. Observa este exemplo fictício:

```text
http://localhost:5130/api/health
|      |         |    |
|      |         |    +-- caminho dentro da aplicação
|      |         +------- porta
|      +----------------- computador de destino
+------------------------ protocolo
```

`localhost` refere-se ao próprio computador. A **porta** permite distinguir serviços que estão a escutar nesse computador. Dois programas não podem, em geral, escutar simultaneamente no mesmo endereço e porta.

O número 5130 é apenas ilustrativo. A tua porta será mostrada no terminal. O WebLab e o SupportDesk.Web podem receber portas diferentes: anota o endereço de cada um.

**HTTPS** é HTTP protegido por encriptação durante o transporte. Para reduzir a configuração inicial, vamos criar esta aplicação de aprendizagem com HTTP local. Não enviaremos palavras-passe nem dados reais. Na aula 10 configuraremos HTTPS antes de trabalhar com login.

### 3.4. Endpoint e rota

Um **endpoint** é uma operação acessível por um método e um endereço. `GET /api/health` é um endpoint. Uma **rota** é a regra que associa um caminho a uma operação.

Nesta aula vamos usar um método chamado `MapGet`. Ele regista uma associação: «quando chegar um GET para este caminho, executa esta função».

O caminho `/api/health` é uma escolha nossa. A palavra `api` não ativa funcionalidades especiais. **API** significa interface de programação de aplicações: um conjunto de operações que outros programas podem usar. Esta é a nossa primeira operação.

### 3.5. Texto, HTML e JSON

O corpo de uma resposta pode transportar formatos diferentes.

- **Texto simples:** uma frase, como `Olá, Pedro!`.
- **HTML:** marcação que descreve elementos de uma página, como títulos e parágrafos. `<h1>SupportDesk AI</h1>` representa um título principal.
- **JSON:** um formato textual para representar dados com campos e valores, como `{"status":"ok"}`.

Um objeto C# e o texto JSON que o representa são coisas distintas. **Serializar** significa transformar dados num formato que possa ser transmitido ou guardado. ASP.NET Core serializa automaticamente o objeto simples que vamos devolver.

O cabeçalho **Content-Type** indica o formato: `text/plain`, `text/html` ou `application/json`, por exemplo. Um **cabeçalho** é informação sobre o pedido ou a resposta, separada do respetivo corpo.

Um **código de estado HTTP** indica o resultado do pedido. Hoje precisamos de dois: **200**, sucesso; **404**, caminho/recurso não encontrado. Vamos aprofundar os restantes na aula 2.

### 3.6. O C# que vais encontrar

Este código representa uma função sem parâmetros que devolve uma string:

```csharp
() => "Olá, Pedro!"
```

É uma **expressão lambda**, uma forma curta de escrever uma função. `()` indica ausência de parâmetros e a expressão depois de `=>` fornece o valor devolvido. Para a mesma finalidade, poderíamos escrever um método:

```csharp
string CriarSaudacao()
{
    return "Olá, Pedro!";
}
```

Nesta aula usamos a lambda porque a operação é pequena. A lógica de pedidos de suporte será organizada em métodos e serviços quando crescer.

Este código cria um **objeto anónimo**, um objeto cujo tipo não declaramos explicitamente com uma classe nossa:

```csharp
new { status = "ok", lesson = 1 }
```

`status` é uma string e `lesson` é um número. É uma simplificação útil para respostas pequenas e fixas. Usaremos classes com nomes para os pedidos e os dados da API nas aulas 2–3.

Poderás também ver instruções diretamente em `Program.cs`, sem uma classe `Program` e um `Main` escritos por ti. São **instruções de nível superior**: o compilador organiza o ponto de entrada. O programa continua a ter um início de execução.

### 3.7. Projeto, solução e template

Um **projeto** reúne código e configuração de compilação; é descrito por um ficheiro `.csproj`. Uma **solução** agrupa projetos; aqui será `SupportDesk.sln`. O nome da solução pode ser diferente do nome do projeto.

Um **template** é um modelo inicial. `dotnet new webapp` gera os ficheiros de uma aplicação Razor Pages. **Razor Pages** permite construir páginas web usando HTML e C# no servidor. Hoje só vamos alterar o título e um parágrafo de uma página gerada; construiremos páginas com dados nas aulas 8–9.

Uma ferramenta gerar código não significa que já o escreveste ou compreendeste. Vamos identificar cada pequena parte que alterares.

**Ponto de verificação P2:** num endereço que utiliza `localhost`, o navegador e o servidor podem estar no mesmo computador? Qual deles executa o nosso C#?

**A minha resposta:** ___________________________________________________

## 4. Exemplo guiado: uma resposta de texto

Vamos experimentar o percurso completo numa aplicação pequena antes de o aplicar ao SupportDesk. O WebLab fica numa pasta de experiências e não será adicionado à solução do produto.

### 4.1. Preparar o local e verificar a ferramenta

**Objetivo e razão:** executar os comandos no local certo e confirmar que o SDK necessário está disponível.

**Ficheiros alterados:** nenhum neste passo.

Abre PowerShell e executa:

```powershell
Set-Location -LiteralPath 'C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI'
Get-Location
dotnet --list-sdks
dotnet --version
```

`Set-Location` muda a pasta atual. As aspas mantêm o caminho com espaços como um único argumento. `Get-Location` mostra essa pasta. `--list-sdks` lista os SDKs instalados; `--version` indica o selecionado para a pasta atual. Para este caderno queremos 10.0.x; foi verificado com 10.0.401.

#### Opção Visual Studio

Podes fazer esta verificação em **Ver → Terminal → PowerShell**, com os mesmos comandos. Abrir o projeto no editor não garante que o terminal esteja na raiz `SupportDesk-AI`: confirma sempre com `Get-Location` antes de usar caminhos relativos.

**Verificação:** o caminho termina em `SupportDesk-AI` e existe um SDK 10.0.x. Se o SDK selecionado for outro ou um comando falhar, consulta a secção 8 antes de continuar.

### 4.2. Criar o pequeno laboratório

#### Opção Visual Studio — abrir ou criar o WebLab

**Se já existe:** escolhe **Ficheiro (File) → Abrir (Open) → Projeto/Solução (Project/Solution)** e abre `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\experiencias\aula-01\WebLab\WebLab.csproj`. Continua na secção 4.3; não repitas a criação abaixo.

**Se ainda não existe:**

1. Escolhe **Criar um novo projeto (Create a new project)** e procura **ASP.NET Core Empty**, em C#.
2. Nome: **WebLab**. Localização: `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\experiencias\aula-01`. Podes criar as pastas em falta no Explorador de Ficheiros.
3. Assinala **Colocar solução e projeto no mesmo diretório (Place solution and project in the same directory)**, para evitar uma pasta `WebLab` adicional dentro de outra.
4. Seleciona **.NET 10**, desmarca **Configure for HTTPS** e deixa o suporte a contentores desativado. Cria o projeto.
5. Confirma que o ficheiro criado está em `experiencias\aula-01\WebLab\WebLab.csproj`. A solução adicional que o Visual Studio possa criar para o laboratório fica separada da solução do produto.

#### Opção PowerShell

**Objetivo:** obter uma aplicação web mínima que possas modificar sem misturar a experiência com o produto.

**Pasta onde executar:** a raiz `SupportDesk-AI`, usada no passo anterior.

```powershell
Test-Path -LiteralPath '.\experiencias\aula-01\WebLab\WebLab.csproj'
```

Na primeira execução, esperamos `False`. Se obtiveres `True`, o laboratório já existe: abre-o e continua, sem recriar ficheiros por cima do teu trabalho.

Quando ainda não existir, executa:

```powershell
dotnet new web -n WebLab -o experiencias/aula-01/WebLab -f net10.0 --no-https
```

| Elemento | Significado |
|---|---|
| `dotnet new web` | Criar uma aplicação ASP.NET Core vazia a partir de um template. |
| `-n WebLab` | Dar o nome WebLab ao projeto. |
| `-o ...` | Escolher a pasta de saída, relativa à pasta atual. |
| `-f net10.0` | Destinar o projeto a .NET 10. |
| `--no-https` | Criar o exemplo local com HTTP para esta primeira aula. |

O comando pode mostrar mensagens de **restore**. Restore prepara as dependências declaradas pelo projeto; não é uma execução da aplicação. NuGet é o sistema de pacotes usado pelo ecossistema .NET. Não vamos adicionar bibliotecas nesta aula.

**Verificação:** executa novamente o `Test-Path` anterior. Deve devolver `True`.

### 4.3. Escrever a resposta

**Objetivo:** escolher o caminho e o texto que a aplicação devolve.

**Ficheiro a alterar:**

```text
C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\experiencias\aula-01\WebLab\Program.cs
```

Abre esse ficheiro no editor. Apenas no WebLab, substitui o conteúdo por este exemplo completo de quatro instruções:

**Opção Visual Studio:** no Explorador de Soluções, expande **WebLab** e abre **Program.cs**. Escreve o exemplo e guarda com **Ctrl+S**. Se já fizeste esta etapa e acrescentaste outras rotas como exercício, não precisas de apagar o teu trabalho para reler a aula.

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/ola", () => "Olá, Pedro!");

app.Run();
```

**Antes de executar: o que esperas que aconteça quando abrires `/ola` no navegador? E quando abrires `/adeus`?**

**A minha previsão:** ___________________________________________________

**Explicação, parte a parte:**

1. `CreateBuilder(args)` prepara o configurador da aplicação. `args` representa argumentos fornecidos ao programa. `var` permite ao compilador inferir o tipo da variável; não significa ausência de tipo.
2. `builder.Build()` cria a aplicação com a configuração preparada.
3. `app.MapGet(...)` regista uma rota GET. A função ainda não está a responder a um pedido: está a ser associada ao caminho.
4. A lambda devolve a string quando a operação for chamada.
5. `app.Run()` inicia a aplicação e mantém o processo a servir pedidos até ser parado. Os registos de endpoints devem ficar antes desta linha.

O template já tinha a estrutura de arranque e uma resposta de exemplo. A nossa alteração relevante é a associação entre `/ola` e a função que devolve a saudação.

### 4.4. Executar, observar e parar

#### Opção Visual Studio — executar

1. No Explorador de Soluções, clica com o botão direito em **WebLab** e escolhe **Set as Startup Project**, se necessário.
2. Na lista junto do botão verde de executar, escolhe o perfil **http**.
3. Guarda e pressiona **F5**, ou escolhe **Debug → Start Debugging**. Mantém a aplicação em execução enquanto fazes pedidos.
4. Copia o endereço de `Now listening on`, na consola ou em **Ver → Output**. Também podes consultar `Properties/launchSettings.json`, em `profiles` → `http` → `applicationUrl`, e confirmar o endereço usado ao arrancar.
5. Abre esse endereço no navegador e acrescenta **`/ola`**. Se o navegador abrir só a raiz `/` e receberes 404, acrescenta o caminho: o exemplo apenas registou `/ola`.

Para parar uma execução iniciada com **F5**, usa o quadrado vermelho ou **Shift+F5** (**Debug → Stop Debugging**). Para voltar a testar uma alteração nesta aula: parar, guardar e executar novamente com F5. Não arranques também `dotnet run` enquanto esta instância estiver ativa.

#### Opção PowerShell — executar

**Pasta onde executar:** raiz `SupportDesk-AI`.

```powershell
dotnet run --project experiencias/aula-01/WebLab --launch-profile http
```

`run` compila quando necessário e executa. `--project` identifica o projeto. `--launch-profile http` seleciona o perfil chamado `http` em `Properties/launchSettings.json`, com a porta e a configuração local.

O terminal fica ocupado enquanto o servidor executa. Isto é normal. Não escrevas o comando seguinte como se o PowerShell já estivesse pronto; usa outra janela, ou termina a execução com **Ctrl+C**.

Procura a linha `Now listening on: http://localhost:...`. Copia o endereço real.

**Endereço do meu WebLab:** _______________________________________________

No navegador, acrescenta `/ola`. Deverás ver `Olá, Pedro!`. O terminal pode mostrar registos do pedido; não é necessário que a saudação apareça na consola, porque foi devolvida ao cliente.

#### Opção Visual Studio — consultar a resposta HTTP

Com o WebLab em execução, clica com o botão direito no projeto → **Add → New Item** e procura **HTTP File**. Chama-lhe **WebLab.http**. Se o template não aparecer, adiciona um ficheiro de texto com esse nome e extensão `.http`.

Escreve o seguinte, **substituindo 5000 pela porta real do WebLab**:

```http
@labUrl = http://localhost:5000

GET {{labUrl}}/ola
```

Guarda e clica em **Send Request** acima de `GET`. No painel de resposta, observa o estado **200**, o cabeçalho **Content-Type** e o corpo com a saudação. O ficheiro descreve um pedido de teste; não inicia o servidor nem acrescenta uma rota à aplicação.

#### Opção PowerShell — consultar a resposta HTTP

Para consultar o código de estado, abre uma **segunda janela PowerShell**. Estes comandos podem correr em qualquer pasta:

```powershell
$labUrl = Read-Host 'Copia o endereço do WebLab, sem caminho nem barra final'
$labResponse = Invoke-WebRequest -UseBasicParsing -Uri "$labUrl/ola"
$labResponse.StatusCode
$labResponse.Headers['Content-Type']
$labResponse.Content
```

Em PowerShell, variáveis começam por `$`. `Read-Host` pede um valor. `Invoke-WebRequest` faz o pedido. `-UseBasicParsing` permite usar esta forma também no Windows PowerShell 5.1. Acedemos depois a propriedades da resposta, de forma semelhante a um objeto em C#.

**Atenção ao endereço base:** se no navegador tens `http://localhost:5000/ola`, introduz apenas `http://localhost:5000` quando o comando pedir o endereço. O comando já acrescenta `/ola`. Se incluíres o caminho na variável, vais pedir `/ola/ola` e podes receber **404 Not Found**. Para corrigir, volta a executar o bloco e introduz só o endereço base, com a tua porta. A mesma regra aplica-se à variável do ficheiro `.http`.

**Resultado esperado:** estado `200`, um tipo de conteúdo que começa por `text/plain` e a saudação. Não copies o número de porta de um exemplo; usa o teu.

**Ponto de verificação P3:** experimenta `/adeus`. Regista o que observas e explica por que motivo escrever um caminho no navegador não cria uma nova função no servidor.

**Observação:** _________________________________________________________

**Explicação:** _________________________________________________________

Para parar, usa **Ctrl+C** no terminal onde executaste `dotnet run`, ou **Shift+F5** no Visual Studio se iniciaste com F5. Tenta novamente `/ola`, atualizando o navegador. Regista se recebeste uma resposta HTTP ou um erro de ligação. Fecha o WebLab antes de passar ao projeto.

**O que aconteceu ao parar o processo:** ___________________________________

**Pausa de estudo:** até aqui deves conseguir contar o percurso «pedido, escolha da função, execução C#, resposta». Se não conseguires, volta à secção 3.2 e aponta a etapa que ainda te confunde.

## 5. Construção no projeto, passo a passo

Vamos aplicar a ideia que acabaste de experimentar ao SupportDesk AI. O WebLab fica separado e parado. O código do produto ficará em `src/SupportDesk.Web`.

### Passo 1 — Confirmar a pasta e o trabalho existente

**Objetivo e razão:** evitar criar uma solução na pasta errada ou substituir trabalho numa segunda passagem pela aula.

**Opção visual:** no Explorador de Ficheiros do Windows, abre a pasta `SupportDesk-AI` e procura `SupportDesk.sln` e `src\SupportDesk.Web\SupportDesk.Web.csproj`. Se já tens a solução, abre-a pelo menu **File → Open → Project/Solution** do Visual Studio e retoma o passo correspondente. Os comandos abaixo fazem a mesma inspeção.

**Local:** abre PowerShell e executa:

```powershell
Set-Location -LiteralPath 'C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI'
Get-Location
Get-ChildItem
Test-Path -LiteralPath '.\SupportDesk.sln'
Test-Path -LiteralPath '.\src\SupportDesk.Web\SupportDesk.Web.csproj'
```

**Ficheiro alterado:** nenhum. `Get-ChildItem` lista o conteúdo da pasta. `Test-Path` devolve `True` ou `False` conforme o caminho existe.

**Resultado esperado na primeira passagem:** a pasta contém o material do curso e ambos os testes devolvem `False`. Se já executaste parte da aula, podem devolver `True`: salta a criação do que já existir e inspeciona os ficheiros, sem utilizar `--force`.

### Passo 2 — Criar a solução

**Objetivo e razão:** ter um ficheiro que agrupe a aplicação e, mais tarde, o projeto de testes.

**Pasta onde executar:** raiz `SupportDesk-AI`.

```powershell
dotnet new sln -n SupportDesk --format sln
```

**Ficheiro criado:** `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\SupportDesk.sln`.

O template `sln` cria uma solução. Em .NET 10, o formato predefinido passou a ser `.slnx`; `--format sln` escolhe explicitamente o formato `.sln` que adotámos no curso. Ambos são formatos de solução. Não é necessário aprender o conteúdo interno do ficheiro nesta aula.

**Verificação:**

```powershell
Test-Path -LiteralPath '.\SupportDesk.sln'
```

**Resultado esperado:** `True`. Criar uma solução não cria automaticamente uma aplicação.

#### Opção Visual Studio — criar a solução

1. Escolhe **File → New → Project**, procura **Blank Solution** e avança.
2. Usa o nome **SupportDesk** e a localização `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI`.
3. Depois de criar, seleciona a solução no Explorador de Soluções e usa **File → Save Solution As**. Guarda na raiz `SupportDesk-AI`, com o nome **SupportDesk.sln**, escolhendo o tipo **Solution File (*.sln)** se houver seleção de formato. Isto permite corrigir a subpasta extra que o assistente possa ter criado; faz este acerto enquanto a solução ainda está vazia.
4. Confirma o ficheiro na raiz e abre essa cópia. É a solução que vais usar nos passos seguintes.

Se a tua versão só permitir guardar `.slnx`, esse formato também funciona. Para manter exatamente os nomes e comandos do caderno, usa **Ver → Terminal → PowerShell**, executa o `Set-Location` do passo 1 e o comando de criação `.sln` deste passo, e abre o resultado com **File → Open → Project/Solution**. Não alteres apenas a extensão de um ficheiro `.slnx`.

### Passo 3 — Criar a aplicação Razor Pages

**Objetivo e razão:** obter uma página inicial funcional e uma estrutura que possamos desenvolver nas aulas seguintes.

**Pasta onde executar:** raiz `SupportDesk-AI`.

```powershell
dotnet new webapp -n SupportDesk.Web -o src/SupportDesk.Web -f net10.0 --no-https
```

**Pasta criada:** `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\src\SupportDesk.Web`.

Os argumentos são os mesmos que usámos no laboratório, mas `webapp` escolhe um template com páginas Razor. `src` é apenas uma convenção para organizar o código-fonte. O ponto no nome `SupportDesk.Web` faz parte do nome do projeto; não significa uma subpasta.

Todo o código criado até este momento é **gerado pela ferramenta**. Ainda não acrescentaste a funcionalidade do endpoint nem o texto do produto.

**Verificação:**

```powershell
Test-Path -LiteralPath '.\src\SupportDesk.Web\SupportDesk.Web.csproj'
```

**Resultado esperado:** `True`. Abre o ficheiro `.csproj` e encontra `<TargetFramework>net10.0</TargetFramework>`. Esta propriedade identifica a versão de .NET a que o projeto se destina.

#### Opção Visual Studio — criar a aplicação

Com **SupportDesk.sln** aberto, clica com o botão direito na solução → **Add → New Project**. Escolhe **ASP.NET Core Web App (Razor Pages)**, em C#. Usa o nome **SupportDesk.Web** e a localização `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\src` (cria a pasta `src` se necessário).

Seleciona **.NET 10**, autenticação **None**, **Configure for HTTPS** desmarcado e suporte a contentores desativado. Confirma que o projeto fica em `src\SupportDesk.Web\SupportDesk.Web.csproj`. Ao usar **Add → New Project**, o Visual Studio também o adiciona à solução: a tarefa do passo 4 já fica feita.

### Passo 4 — Adicionar o projeto à solução

**Objetivo e razão:** permitir que o editor e os comandos da solução conheçam a aplicação.

**Pasta onde executar:** raiz `SupportDesk-AI`.

```powershell
dotnet sln SupportDesk.sln add src/SupportDesk.Web/SupportDesk.Web.csproj
dotnet sln SupportDesk.sln list
```

**Ficheiro alterado:** `SupportDesk.sln`, na raiz. O código C# não muda.

`add` regista o projeto na solução; `list` mostra os projetos registados. O projeto poderia executar sem estar numa solução, mas este agrupamento será útil quando existirem testes.

**Resultado esperado:** a listagem inclui `src\SupportDesk.Web\SupportDesk.Web.csproj`. O WebLab não faz parte desta solução.

**Opção Visual Studio:** se criaste o projeto pelo comando, clica com o botão direito na solução **SupportDesk** → **Add → Existing Project** e seleciona `src\SupportDesk.Web\SupportDesk.Web.csproj`. Se **SupportDesk.Web** já aparece na solução, não o adiciones novamente. Guarda com **File → Save All**.

### Passo 5 — Abrir os ficheiros e compilar

**Objetivo e razão:** conhecer o material gerado e confirmar que a base funciona antes das nossas alterações.

Abre `SupportDesk.sln` no Visual Studio, ou abre a pasta do projeto no editor que usas.

| Local, dentro de `src/SupportDesk.Web` | Para que serve | Vais alterá-lo hoje? |
|---|---|---|
| `Program.cs` | Prepara e inicia a aplicação; regista o endpoint. | Sim. |
| `Pages/Index.cshtml` | Define a página inicial. | Sim. |
| `Pages/Index.cshtml.cs` | Classe que prepara o comportamento dessa página. | Não. |
| `Pages/Shared/_Layout.cshtml` | Estrutura visual partilhada por páginas. | Não. |
| `wwwroot` | Ficheiros públicos, como estilos CSS. | Não. |
| `appsettings.json` | Configuração da aplicação. | Não. |
| `Properties/launchSettings.json` | Perfis de execução local e respetivos endereços. | Só consultar. |
| `SupportDesk.Web.csproj` | Configuração e referências do projeto. | Só consultar. |
| `bin` e `obj` | Resultados e ficheiros intermédios da compilação. | Não editar. |

`bin` e `obj` podem aparecer depois de restaurar ou compilar. O **CSS** define apresentação visual; não vamos trabalhar os estilos nesta aula.

**Pasta onde executar:** raiz `SupportDesk-AI`.

```powershell
dotnet build SupportDesk.sln
```

**Ficheiros criados pela ferramenta:** resultados de compilação em `bin` e `obj`. **Compilar** verifica e transforma o código para poder executar. `build` não deixa o servidor a correr.

**Resultado esperado:** a compilação termina com sucesso. A mensagem pode estar em inglês ou português. Se houver erros, resolve o primeiro erro relevante antes de continuar; a secção 8 mostra como.

**Opção Visual Studio:** usa **Build → Build Solution** ou **Ctrl+Shift+B**. Consulta **Ver → Output**, escolhendo **Build**, para confirmar o resultado. Se falhar, abre **Ver → Error List**; um duplo clique num erro leva-te ao ficheiro e à linha. Compilar pelo menu também não mantém o servidor em execução.

### Passo 6 — Executar a página gerada

**Objetivo e razão:** observar a aplicação a funcionar antes de a personalizares.

**Pasta onde executar:** raiz `SupportDesk-AI`.

```powershell
dotnet run --project src/SupportDesk.Web --launch-profile http
```

**Ficheiro alterado:** nenhum. O perfil `http` foi gerado pelo template.

**Opção Visual Studio:** define **SupportDesk.Web** como **Startup Project**, seleciona **http** junto do botão verde e pressiona **F5**. Usa o endereço desta aplicação, que pode ter uma porta diferente do WebLab. A página inicial abre na raiz `/`. Não inicies o comando acima em simultâneo.

Procura `Now listening on` e abre esse endereço no navegador, sem acrescentar caminho. Esse endereço usa o caminho `/`, a raiz do site. A página `Index.cshtml` é a página inicial.

**Endereço do meu SupportDesk.Web:** _______________________________________

**Resultado esperado:** a página inicial do template, habitualmente com o título visível `Welcome`.

**Ponto de verificação P4:** se `dotnet build` terminar sem erros, isso prova que há um servidor à escuta? Indica o comando que cria essa situação e explica a diferença.

**A minha resposta:** ___________________________________________________

Usa **Ctrl+C** no terminal do servidor, ou **Shift+F5** se o iniciaste no Visual Studio com F5. Vamos editar com o processo parado, para tornar explícita a relação entre guardar, compilar e executar. Ferramentas de atualização automática serão introduzidas depois.

### Passo 7 — Dar identidade à página inicial

**Objetivo e razão:** a página passa a apresentar a finalidade do produto que vais construir.

**Ficheiro a alterar:**

```text
C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\src\SupportDesk.Web\Pages\Index.cshtml
```

Mantém as linhas geradas `@page`, `@model IndexModel` e a estrutura da página. Vamos fazer três alterações pequenas.

**Opção Visual Studio:** expande **SupportDesk.Web → Pages** e abre **Index.cshtml**. Faz as alterações abaixo, guarda com **Ctrl+S** e executa novamente com **F5** para verificar no navegador.

**7.1. Título usado pela página:** dentro do bloco C# que já existe, altera só o valor:

```csharp
ViewData["Title"] = "SupportDesk AI";
```

`ViewData` permite fornecer dados à apresentação. Aqui o layout gerado usa este valor no título do documento, que o navegador apresenta no separador. Não precisas de adicionar um segundo bloco.

**7.2. Título visível:** substitui o conteúdo do elemento `h1` para obter:

```html
<h1 class="display-4">SupportDesk AI</h1>
```

`h1` representa um título principal. `class="display-4"` associa estilos já presentes no template; não é uma classe C#.

**7.3. Descrição:** substitui o parágrafo de exemplo por:

```html
<p>Gestão de pedidos de suporte com assistência de IA.</p>
```

`p` representa um parágrafo. Estamos a descrever o objetivo do produto, não a afirmar que a integração de IA já está implementada.

**Verificação:** guarda, executa novamente o comando do passo 6 e atualiza o navegador. Deves ver o nome e a descrição. O separador pode incluir também um sufixo com o nome do projeto, definido no layout; não é um erro.

**Resultado esperado:** o texto do produto aparece na página. Para já, a alteração é de apresentação: não existem pedidos de suporte guardados.

Para continuar, para novamente o servidor com **Ctrl+C**, ou com **Shift+F5** no Visual Studio.

### Passo 8 — Acrescentar o endpoint JSON

**Objetivo e razão:** disponibilizar uma resposta simples que um programa também consiga consultar e interpretar.

**Opção Visual Studio:** abre **Program.cs** dentro de **SupportDesk.Web**. Acrescenta o endpoint indicado abaixo antes de `app.Run()`, mantendo a configuração Razor Pages gerada. Guarda e volta a executar com **F5**. Confirma que estás a editar o produto e não o Program.cs do WebLab.

**Ficheiro a alterar:**

```text
C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI\src\SupportDesk.Web\Program.cs
```

Abre o ficheiro. Já reconheces o configurador, `Build()` e `Run()`. O template tem ainda código para disponibilizar páginas, ficheiros públicos e tratamento de pedidos.

| Código gerado | Leitura suficiente para esta aula |
|---|---|
| `builder.Services.AddRazorPages()` | Regista as funcionalidades necessárias às páginas Razor. |
| `if (!app.Environment.IsDevelopment())` | Distingue execução fora do ambiente de desenvolvimento. |
| `app.UseExceptionHandler("/Error")` | Define uma página para certos erros não tratados fora do desenvolvimento. |
| `app.UseRouting()` | Participa na seleção do endpoint para o pedido. |
| `app.UseAuthorization()` | Integra a verificação de permissões quando existirem regras configuradas. Não cria login sozinho. |
| `app.MapStaticAssets()` | Disponibiliza os ficheiros estáticos do projeto. |
| `app.MapRazorPages().WithStaticAssets()` | Disponibiliza as páginas e associa os seus ficheiros estáticos. |

Essas chamadas formam parte do processamento de pedidos. Chamamos **middleware** a componentes que podem trabalhar num pedido/resposta ao longo desse processamento. Na aula 4 estudaremos os serviços; nas aulas 10–11 estudaremos autenticação e permissões. Hoje mantém este código gerado.

**O código novo que vais escrever é apenas este bloco**, imediatamente antes da linha `app.Run();` já existente:

```csharp
app.MapGet("/api/health", () => new
{
    application = "SupportDesk AI",
    status = "ok"
});
```

Antes de executar, escreve a tua previsão sobre o formato da resposta e onde será apresentada.

**A minha previsão:** ___________________________________________________

O que mudou relativamente ao WebLab?

- O caminho é `/api/health`.
- A função devolve um objeto anónimo, com duas propriedades string.
- ASP.NET Core transforma esse objeto em JSON e devolve-o ao cliente.

Para confirmares a posição, o final do ficheiro deve ter esta forma. É apenas o trecho final; não substituas o ficheiro inteiro por ele:

```csharp
app.MapRazorPages()
   .WithStaticAssets();

app.MapGet("/api/health", () => new
{
    application = "SupportDesk AI",
    status = "ok"
});

app.Run();
```

**Verificação:** guarda o ficheiro e executa novamente o comando do passo 6. Se não compilar, lê a primeira mensagem de erro: parênteses, vírgulas e ponto e vírgula são causas frequentes neste bloco.

Abre o endereço real da aplicação seguido de `/api/health`.

**Resultado esperado:** estes campos e valores, possivelmente apresentados numa só linha ou com outra formatação:

```json
{
  "application": "SupportDesk AI",
  "status": "ok"
}
```

Não escrevas este JSON manualmente no corpo da lambda: estamos a devolver um objeto C# e a deixar o framework serializá-lo.

### Passo 9 — Confirmar o resultado sem depender da aparência

**Objetivo e razão:** distinguir «o navegador mostrou qualquer coisa» de «a operação devolveu o formato e os dados esperados».

#### Opção Visual Studio

Adiciona ao projeto **SupportDesk.Web** um ficheiro **SupportDesk.http**, como fizeste no laboratório. Usa o endereço base do produto, trocando a porta ilustrativa **5000** pela real:

```http
@baseUrl = http://localhost:5000

GET {{baseUrl}}/api/health
```

Com a aplicação em execução, clica em **Send Request**. Confirma estado **200**, **Content-Type** começado por `application/json` e os valores `"application": "SupportDesk AI"` e `"status": "ok"` no corpo. Aqui inspecionas o JSON no editor; o exemplo PowerShell abaixo mostra também como o transformar num objeto e ler as propriedades. Podes executá-lo no terminal integrado, mesmo tendo iniciado o servidor com F5.

#### Opção PowerShell

**Ficheiros alterados:** nenhum. Deixa o servidor ativo e abre uma segunda janela PowerShell, em qualquer pasta:

```powershell
$baseUrl = Read-Host 'Copia o endereço do SupportDesk.Web, sem caminho nem barra final'
$response = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/api/health"
$response.StatusCode
$response.Headers['Content-Type']
$response.Content
```

**Resultado esperado:** `200`, um valor que começa por `application/json` e os dois campos do exemplo. Pode existir também `charset=utf-8`, que indica a codificação do texto.

Agora interpreta o JSON como dados:

```powershell
$data = $response.Content | ConvertFrom-Json
$data.application
$data.status
```

O símbolo `|` passa o resultado de um comando ao seguinte. `ConvertFrom-Json` transforma o texto JSON num objeto PowerShell. Deves obter `SupportDesk AI` e `ok`. Este processo inverso de construir objetos a partir de uma representação chama-se **desserialização**.

**Ponto de verificação P5:** a página `/` devolve HTML e `/api/health` devolve JSON. Porque é que os dois pertencem à mesma aplicação? Qual escolherias para uma pessoa ler a apresentação e qual para outro programa consultar o estado?

**A minha resposta:** ___________________________________________________

### Estado do projeto após estes passos

Esta estrutura é um resultado esperado, não uma pasta que precises de escrever à mão:

```text
SupportDesk-AI/
  README.md
  SupportDesk.sln
  curso/
    curso-supportdesk-ai.md
    cadernos/
      00-indice.md
      aula-01-primeira-aplicacao-web.md
  experiencias/
    aula-01/
      WebLab/
  src/
    SupportDesk.Web/
      SupportDesk.Web.csproj
      Program.cs
      Pages/
      Properties/
      wwwroot/
```

`experiencias` é um espaço de aprendizagem; `src` contém o produto. Não precisas de voltar a criar a solução ou o projeto para fazer os exercícios.

## 6. Pontos de verificação

Os pontos P1–P5 apareceram durante a aula. Antes dos exercícios, completa estas duas tarefas. As respostas estão na secção final, não imediatamente abaixo das perguntas.

**P6 — Prever uma alteração.** Se mudares apenas a rota de `/api/health` para `/api/status`, guardares e reiniciares a aplicação, o que acontece a cada um desses dois caminhos? Não alteres permanentemente o nome adotado no projeto: se experimentares, repõe `/api/health`.

**A minha previsão:** ___________________________________________________

**O que observei:** _____________________________________________________

**P7 — Identificar um erro.** Um colega colocou `app.MapGet(...)` depois de `app.Run()` e não consegue obter a resposta enquanto o servidor está ativo. Explica o problema usando a ordem de execução, sem dizer apenas «está no sítio errado».

**A minha explicação:** ___________________________________________________

## 7. Exercícios com apoio progressivamente menor

Executa-os no SupportDesk.Web. Mantém o endpoint `/api/health` e a página inicial a funcionar. Usa a rotina **prever → escrever → guardar → reiniciar → verificar**. As pistas e soluções comentadas estão no fim do caderno.

### Exercício A — Acompanhado: identificar a versão

**Tarefa:** acrescenta ao objeto devolvido por `/api/health` uma propriedade `version` com o valor string `0.1.0`.

**Ficheiro:** `src/SupportDesk.Web/Program.cs`, dentro da pasta do projeto.

**Comportamento esperado:** o mesmo endpoint continua a devolver `application` e `status`, e passa a devolver também `version`. A versão é um valor escolhido para esta primeira etapa; não é o número do SDK.

**Verificação:** faz um novo pedido, volta a converter a resposta e consulta `$data.version`. Não reutilizes o objeto antigo sem fazer um novo pedido.

**Antes de escrever:** em que local do objeto vais acrescentar a propriedade? Que separador é necessário entre propriedades?

**Resposta e plano:** ___________________________________________________

**Resultado observado:** ________________________________________________

Se precisares de ajuda, consulta primeiro a pista A1 e só depois A2.

### Exercício B — Menos ajuda: explicar o produto

**Tarefa:** cria um novo endpoint `GET /api/info` com os campos `name` e `purpose`. Usa `SupportDesk AI` como nome e escreve uma finalidade breve em português.

**Ficheiro:** o mesmo `Program.cs`. Reutiliza o conceito do endpoint anterior, sem o substituir.

**Comportamento esperado:** `/api/info` devolve JSON com esses dois campos; `/api/health` continua a funcionar.

**Verificação:** consulta os dois endereços e regista os códigos de estado e o tipo de conteúdo. A propriedade `purpose` deve existir e ter texto, mas a frase pode ser tua.

**O meu plano, antes de escrever:** _______________________________________

**Resultado observado:** ________________________________________________

### Exercício C — Desafio autónomo: dados com tipos diferentes

**Tarefa:** cria `GET /api/learning` para descrever esta etapa do curso. A resposta deve conter:

| Campo | Valor esperado | Tipo |
|---|---|---|
| `lesson` | `1` | Número |
| `topic` | `Primeira aplicação web` | Texto |
| `completed` | `false` | Booleano |

**Comportamento esperado:** os tipos no JSON correspondem à tabela. `lesson` não deve ser o texto `"1"` e `completed` não deve ser o texto `"false"`.

**Verificação:** consulta o endpoint e compara os três valores e os seus tipos. Confirma que os endpoints anteriores e a página inicial continuam a responder.

`completed` é apenas um valor de demonstração, não um registo persistente do teu progresso. Este endpoint serve para praticar; decidirás nas aulas seguintes se deve permanecer no produto.

**Código ou explicação da minha solução:**

________________________________________________________________________

________________________________________________________________________

**Como confirmei o resultado:** ___________________________________________

**Ajuda utilizada:** nenhuma / pista / solução completa. Se consultaste a solução, repete o exercício mais tarde sem a abrir.

## 8. Erros frequentes e como os investigar

### Um método de diagnóstico que vamos reutilizar

1. Identifica a fase: **comando**, **compilação**, **arranque** ou **pedido HTTP**. Uma falha antes de o servidor arrancar não é um erro do navegador.
2. Lê a primeira mensagem de erro relevante, não apenas a última linha «Build failed».
3. Procura o ficheiro, a linha e a descrição. Compara com a última alteração que fizeste.
4. Faz uma alteração pequena e testa novamente. Evita mudar várias coisas ao mesmo tempo.
5. Regista a causa com as tuas palavras; isso será mais útil do que decorar a correção.

Uma mensagem de compilação pode ter este formato ilustrativo:

```text
Program.cs(12,18): error CS1002: ; expected
```

`Program.cs` é o ficheiro, `12` a linha, `18` a coluna e `CS1002` o identificador do erro. `; expected` significa que o compilador esperava um ponto e vírgula. A causa pode estar também na linha anterior, por exemplo uma expressão que ficou por terminar. Os números de linha dependem do teu ficheiro.

### Situações prováveis nesta aula

| Sintoma | Causa provável | Como investigar e corrigir |
|---|---|---|
| Não aparecem os templates ASP.NET Core no Visual Studio | Falta a carga de trabalho Web nessa instalação. | No Visual Studio Installer, modifica a versão que estás a usar e seleciona ASP.NET and web development. Confirma também que o editor suporta .NET 10. |
| O pedido termina em `/ola/ola` e devolve 404 | O endereço base já incluía `/ola`. | Define `$labUrl` ou `@labUrl` só com `http://localhost:porta`, sem caminho nem barra final. Repete o pedido, que acrescenta `/ola` uma única vez. |
| `dotnet` não é reconhecido | SDK em falta ou terminal sem a configuração de caminho atualizada. | Fecha e volta a abrir o terminal. Executa `dotnet --list-sdks`. Se continuar a falhar, instala o SDK .NET 10 e volta a verificar; o runtime sozinho não chega. |
| `net10.0` é rejeitado ou aparece `NETSDK1045` | Está a ser usado um SDK anterior ao necessário. | Executa `dotnet --version` na raiz do projeto e `dotnet --list-sdks`. Se tens 10 instalado mas outro foi selecionado, investiga se existe um `global.json` na pasta ou numa pasta acima a fixar a versão. Não apagues configurações sem compreender a sua função; traz os valores para analisarmos. |
| «Project file does not exist» | Pasta atual ou argumento `--project` incorreto. | Executa `Get-Location` e `Test-Path '.\src\SupportDesk.Web\SupportDesk.Web.csproj'`. Volta à raiz com o `Set-Location` completo do passo 1. |
| `SupportDesk.sln` não é encontrado, mas existe `.slnx` | A solução foi criada sem `--format sln`. | Confirma os nomes com `Get-ChildItem`. O formato `.slnx` é válido: podes usar esse nome nos comandos de solução e anotar a diferença para a continuidade. Não basta mudar a extensão, pois o conteúdo usa outro formato. |
| A ferramenta avisa que vai substituir ficheiros | Repetiste um comando de criação numa pasta já utilizada. | Interrompe a criação, abre o projeto existente e retoma no passo seguinte. Não uses `--force` para esconder o aviso. |
| Restore falha com uma mensagem sobre NuGet, rede ou acesso | Uma origem de pacotes, permissões ou conectividade impediu preparar dependências. | Lê qual a origem ou ficheiro mencionado e tenta `dotnet restore .\src\SupportDesk.Web\SupportDesk.Web.csproj` na raiz. Se continuar, guarda a mensagem exata para diagnóstico. Não desatives proteções nem publiques configurações que contenham credenciais. |
| O terminal parece não terminar depois de `dotnet run` | O servidor está ativo. | Procura `Now listening on`. Usa o navegador ou outro terminal para fazer pedidos. Ctrl+C para parar. |
| O navegador indica ligação recusada | Não existe servidor no endereço/porta usados, ou já o paraste. | Confirma que o terminal ainda executa e copia o endereço real de `Now listening on`. Verifica se estás a usar a porta do laboratório ou a do projeto. |
| A ligação falha quando usas `https://` | O projeto desta aula foi criado apenas para HTTP local. | Usa exatamente o endereço `http://...` mostrado no terminal. Não resolves isto alterando o caminho `/api/health`. |
| O servidor diz que a porta está em utilização | Outra instância ocupa esse endereço. | Confirma se a aplicação já está iniciada por F5 no Visual Studio ou por `dotnet run` num terminal. Para a tua instância com Shift+F5 ou Ctrl+C, respetivamente. Não termines processos desconhecidos. Se persistir, verifica `applicationUrl` no perfil `http` para escolher uma porta local livre. |
| A aplicação responde, mas recebes 404 | O método/caminho não corresponde a um endpoint registado. | Confirma a grafia, o processo em execução e a posição de `MapGet`. No WebLab, `/` não foi registado: o caminho criado foi `/ola`. No produto, verifica `/api/health`. |
| A chamada devolve 500 depois de copiares o endpoint | Pode existir uma exceção, por exemplo dois endpoints iguais. | Lê o erro no terminal. Se indicar correspondência ambígua, procura duas chamadas `MapGet` com o mesmo caminho e mantém apenas a pretendida. Não concluas que qualquer 500 tem essa causa. |
| A página ou o JSON continua com o texto antigo | Ficheiro não guardado, processo antigo ou projeto errado. | Guarda, termina o servidor correto, volta a executar e atualiza o navegador. No PowerShell, faz um novo pedido; `$response` guarda o resultado anterior e não se atualiza sozinho. |
| `; expected`, `) expected` ou erro semelhante | Um delimitador ficou em falta. | Inspeciona a linha indicada e a anterior. No endpoint, confirma o fecho `});` e as vírgulas entre propriedades. Não copies as marcas de bloco de código do Markdown para o ficheiro C#. |

Se pedires ajuda, envia: comando executado, pasta indicada por `Get-Location`, mensagem exata e o trecho do ficheiro alterado. Primeiro vamos localizar a causa; depois tentarás uma correção com uma pista.

**O meu registo de diagnóstico:**

| Sintoma observado | Fase | Hipótese | Teste que fiz | Causa encontrada |
|---|---|---|---|---|
| | | | | |
| | | | | |

## 9. Revisão sem consultar

Fecha o exemplo e responde com palavras tuas. Não é necessário usar exatamente a formulação do caderno.

1. Qual é a diferença entre C#, .NET, SDK e ASP.NET Core?

   **Resposta:** _________________________________________________________

2. Descreve o percurso desde escrever `/api/health` no navegador até receber os dados. Onde executa a lambda?

   **Resposta:** _________________________________________________________

3. Explica protocolo, computador, porta e caminho numa URL local. O que muda se trocares apenas a porta?

   **Resposta:** _________________________________________________________

4. Porque é que uma string e um objeto anónimo devolvidos por `MapGet` produzem formatos diferentes? Qual é a função do Content-Type?

   **Resposta:** _________________________________________________________

5. Porque é que compilar com sucesso e receber `status = "ok"` não demonstram que todas as futuras funcionalidades do produto estão saudáveis?

   **Resposta:** _________________________________________________________

### Reconstrução sem copiar

Com o servidor parado, usa uma cópia de segurança do teu `Program.cs` ou o histórico do editor para poderes recuperar a versão anterior. Remove apenas o bloco de `/api/health` e tenta reconstruí-lo sem consultar o exemplo. Mantém o resto do código gerado e os outros endpoints.

O resultado deve voltar a conter `application`, `status` e a `version` acrescentada no exercício A. Executa e verifica o código HTTP, o tipo de conteúdo e os valores.

Se não te recordares de um nome de método, consulta uma pista e regista essa necessidade. Repete depois sem apoio. Não precisas de memorizar todos os comandos do SDK; precisas de perceber o que estás a pedir a cada comando.

**Consegui reconstruir?** sim / parcialmente / ainda não

**O que tive de consultar:** ______________________________________________

## 10. Critérios de conclusão

### Consigo fazer

- [ ] Confirmar a pasta atual e o SDK selecionado. Se não: rever 4.1 e passo 1.
- [ ] Distinguir a experiência WebLab do projeto SupportDesk.Web. Se não: rever 4.2 e a estrutura no fim da secção 5.
- [ ] Criar ou retomar a solução e o projeto sem substituir trabalho. Se não: rever passos 1–4.
- [ ] Compilar, iniciar e parar o servidor. Se não: rever passos 5–6.
- [ ] Alterar o título e a descrição da página inicial. Se não: rever passo 7.
- [ ] Fazer `/api/health` responder com JSON e estado 200. Se não: rever passos 8–9.
- [ ] Acrescentar a versão e construir os dois endpoints de exercício. Se não: voltar aos exercícios e consultar primeiro as pistas.
- [ ] Reconstruir o endpoint principal com compreensão do que escrevo. Se não: repetir a tarefa da secção 9 após uma pausa.

### Consigo explicar

- [ ] A diferença entre linguagem, plataforma, SDK e framework web. Se não: rever 3.1.
- [ ] Quem é o cliente, quem é o servidor e onde executa o C#. Se não: rever 3.2.
- [ ] O significado das partes de uma URL e de um endpoint GET. Se não: rever 3.3–3.4.
- [ ] A diferença entre HTML, texto e JSON, incluindo serialização. Se não: rever 3.5.
- [ ] O que fazem `CreateBuilder`, `Build`, `MapGet` e `Run`, ao nível desta aula. Se não: rever 4.3.
- [ ] Que ficheiros foram gerados e que alterações escrevi. Se não: rever a tabela do passo 5 e identificar as minhas linhas.
- [ ] Porque é que `status = "ok"` é uma resposta fixa e não uma verificação completa do sistema. Se não: rever a secção 1.
- [ ] Como distinguir compilação falhada, ligação recusada e um 404. Se não: rever a secção 8 e fazer uma experiência controlada.

**Antes de avançar:** um critério por cumprir indica matéria a rever, não uma falha pessoal. Traz a dificuldade e trabalharemos esse ponto. Para concluir a aula, deves conseguir executar a funcionalidade e explicar o percurso dos dados sem depender de ler o código linha a linha.

**Critério em que ainda preciso de apoio:** _________________________________

## 11. Resumo e revisão posterior

Construíste uma aplicação que recebe pedidos HTTP e devolve respostas. O navegador é o cliente; ASP.NET Core recebe o pedido, encontra a operação e executa o C#. Uma página serve HTML e um endpoint pode servir JSON. A solução agrupa projetos; o projeto contém código e configuração. Compilar verifica/constrói; executar mantém o servidor ativo.

O código novo foi pequeno: uma associação entre método/caminho e uma função. A infraestrutura restante veio do template. O facto de estar gerada não a torna irrelevante, mas vamos estudá-la quando for necessária para a próxima funcionalidade.

### Amanhã — 10 minutos

1. Sem abrir as soluções, desenha cliente → pedido → servidor → resposta.
2. Executa a aplicação e faz um pedido novo a `/api/health`.
3. Explica por que motivo o `status` não muda sozinho quando o servidor é desligado: sem o servidor, não há essa resposta.
4. Revê uma dificuldade que anotaste.

### Daqui a uma semana — 15 minutos

1. Reconstrói um endpoint pequeno numa experiência, sem copiar.
2. Devolve um objeto com uma string, um número e um booleano.
3. Verifica estado HTTP, Content-Type e valores.
4. Explica a alguém, em um minuto, onde corre o C# e para que serve a porta.

Estas são sugestões de estudo, não lembretes agendados.

### Ligação à aula 2

O próximo caderno explicará pedidos e respostas HTTP com mais detalhe e introduzirá controllers para organizar a API. Em vez de informação fixa sobre a aplicação, vamos devolver uma lista de pedidos fictícios e consultar um pedido por ID. Também aprenderás o mínimo de Git necessário para guardar versões do teu trabalho.

Não precisas de começar essa implementação agora. Quando pedires a aula 2, vamos partir do teu código e confirmar os critérios da aula 1.

**Uma explicação de 60 segundos para praticar:** «O meu projeto é uma aplicação ASP.NET Core que…»

________________________________________________________________________

________________________________________________________________________

---

## Apoio final — Pistas e soluções comentadas

**Consulta esta parte depois de tentares.** As soluções são exemplos válidos; a tua pode ser diferente e cumprir o mesmo comportamento. Usa uma pista de cada vez.

### Diagnóstico da secção 2

1. Uma classe descreve estrutura e comportamento; um objeto é uma instância desse tipo.
2. `return` entrega um valor a quem chamou a função. `Console.WriteLine` escreve na saída da consola; não devolve automaticamente uma resposta ao navegador.
3. `() => "Olá"` é uma função sem parâmetros que devolve uma string.
4. O C# desta aplicação executa no processo do servidor. O navegador recebe HTML, texto ou JSON produzido pelo servidor.
5. `localhost` designa o próprio computador de onde é feito o pedido.

### Pontos de verificação P1–P7

- **P1:** C# permite escrever, por exemplo, a função que descreve um pedido. ASP.NET Core fornece a infraestrutura para chamar essa função em resposta a um pedido web e transmitir o resultado.
- **P2:** sim, os dois processos podem executar no mesmo computador. Neste projeto, o servidor executa o C# e o navegador apresenta a resposta.
- **P3:** `/adeus` não corresponde a uma rota registada e devolve 404. Depois de parar o processo, uma nova tentativa pode falhar ao estabelecer ligação, o que é diferente de receber uma resposta HTTP 404. A aparência de conteúdo antigo no navegador não prova que o servidor continua ativo.
- **P4:** compilar não inicia um servidor permanente. `dotnet run --project src/SupportDesk.Web --launch-profile http` inicia a aplicação e deixa-a à escuta até ser parada.
- **P5:** o mesmo processo trata rotas distintas e pode devolver formatos distintos. A apresentação em HTML é adequada à pessoa; a representação JSON facilita a consulta por outro programa.
- **P6:** depois de substituir a rota e reiniciar, `/api/status` passa a responder e `/api/health` deixa de corresponder àquele endpoint. Repõe o caminho original para manter a continuidade do curso.
- **P7:** `app.Run()` mantém a execução ocupada a servir pedidos até o servidor parar. O registo que ficou depois não é executado a tempo de servir os pedidos dessa execução. As rotas devem ser registadas antes de iniciar esse ciclo.

### Exercício A — Pistas

**A1:** procura o objeto criado por `new`, não o texto da página inicial.

**A2:** as propriedades ficam separadas por vírgulas. A versão pedida é texto, por isso usa aspas duplas em C#.

### Exercício A — Solução comentada

Substitui apenas o bloco do endpoint existente; não cries outro com a mesma rota:

```csharp
app.MapGet("/api/health", () => new
{
    application = "SupportDesk AI",
    status = "ok",
    version = "0.1.0"
});
```

A vírgula depois de `status` separa a nova propriedade. Todas as três propriedades são strings. Depois de reiniciar, faz um novo pedido na segunda janela PowerShell:

```powershell
$response = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/api/health"
$data = $response.Content | ConvertFrom-Json
$data.version
```

O resultado esperado é `0.1.0`. O objeto PowerShell anterior não mudaria automaticamente por teres alterado o servidor.

### Exercício B — Pista

O novo endpoint precisa da sua própria chamada `MapGet` antes de `app.Run()`. Escolhe o caminho novo e devolve um objeto com os nomes de campos pedidos.

### Exercício B — Solução comentada

Acrescenta um bloco separado do endpoint de estado:

```csharp
app.MapGet("/api/info", () => new
{
    name = "SupportDesk AI",
    purpose = "Gerir pedidos de suporte e apoiar os técnicos com IA."
});
```

O caminho diferente permite manter ambas as operações. A frase é a finalidade do produto; as funções de IA serão desenvolvidas mais tarde.

### Exercício C — Pista opcional, depois da tua tentativa

O objeto anónimo pode ter propriedades de tipos diferentes. Os literais C# `1` e `false` não precisam de aspas. Se os colocares entre aspas, passas a devolver strings.

### Exercício C — Solução comentada

```csharp
app.MapGet("/api/learning", () => new
{
    lesson = 1,
    topic = "Primeira aplicação web",
    completed = false
});
```

O JSON esperado é:

```json
{
  "lesson": 1,
  "topic": "Primeira aplicação web",
  "completed": false
}
```

Em JSON, números e booleanos aparecem sem aspas. A string aparece entre aspas. O valor `false` continua a ser fixo; guardar progresso exigiria outra funcionalidade.

### Revisão da secção 9 — Ideias que devem aparecer nas respostas

1. Linguagem, plataforma, ferramentas de desenvolvimento e funcionalidades web têm responsabilidades diferentes.
2. O cliente envia um GET; a aplicação encontra a rota; a lambda executa no servidor; o objeto é serializado; a resposta volta ao cliente.
3. A porta participa na escolha do serviço de destino. Trocar a porta pode apontar para outro processo ou para um local onde ninguém está à escuta.
4. Uma string é enviada como texto simples neste exemplo; o objeto é serializado como JSON. Content-Type descreve esse formato para o cliente.
5. Compilar não verifica todos os comportamentos em execução. O valor `ok` foi escrito por nós e não testa dependências. Desligar o servidor impede que uma nova resposta seja produzida.

## Notas livres

**A decisão que agora consigo explicar:** ___________________________________

**Uma coisa que compreendi melhor depois de experimentar:** _________________

________________________________________________________________________

**Pergunta para a próxima revisão:** _______________________________________

## Referências oficiais para aprofundar

O conteúdo necessário está explicado no caderno. Estes recursos são opcionais e fundamentam os comandos e comportamentos usados:

- [Mudança de formato das soluções em .NET 10 e opção `--format sln`](https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/10.0/dotnet-new-sln-slnx-default).
- [Primeira aplicação Razor Pages: SDK, template e ficheiros gerados](https://learn.microsoft.com/en-us/aspnet/core/tutorials/razor-pages/razor-pages-start?view=aspnetcore-10.0).
- [Respostas de endpoints: strings, JSON e Content-Type](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-10.0).
- [Criar e gerir projetos e soluções no Visual Studio](https://learn.microsoft.com/en-us/visualstudio/ide/creating-solutions-and-projects?view=visualstudio).
- [Guardar soluções e escolher o formato no Visual Studio](https://devblogs.microsoft.com/visualstudio/new-simpler-solution-file-format/).
- [Testar pedidos com ficheiros .http no Visual Studio](https://learn.microsoft.com/en-us/aspnet/core/test/http-files?view=aspnetcore-10.0).

## Nota de verificação deste material

Preparado com inspeção do projeto e consulta da documentação em 27 de setembro de 2026. Os templates, a criação da solução `.sln`, a compilação dos exemplos e as respostas base foram verificados numa pasta temporária com o SDK 10.0.401. A verificação incluiu `/ola` como texto, `/api/health` como JSON, a página inicial personalizada e uma rota inexistente com 404. Estes testes não executam por ti a prática nem comprovam a tua compreensão.

Atualização de 28 de setembro de 2026: acrescentadas opções pelo Visual Studio, mantendo os comandos PowerShell. As instruções dos menus e dos ficheiros `.http` foram conferidas com documentação oficial; a sequência de cliques não foi executada na tua instalação. Mantêm-se os campos de respostas e os critérios de conclusão do HTML.

Na preparação isolada foi usada uma configuração local de NuGet, pois o ambiente de validação tinha restrições de acesso à configuração global. Essa configuração não foi introduzida no teu projeto. Os comandos do caderno usam a configuração normal do teu ambiente; a secção 8 explica como investigar uma eventual falha de restore.
