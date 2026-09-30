# SupportDesk AI — Índice dos cadernos de estudo

**Aluno:** Pedro · **Ponto de partida:** programação em C# · **Ambiente:** Windows, Visual Studio 2026 ou PowerShell

**Projeto:** `C:\Users\Pedro Moreira\Documents\Projectos\AI Projects\SupportDesk-AI`

## O que já existe e o que muda

Na preparação inicial, em 27 de setembro de 2026, a pasta continha `README.md` e o curso anterior de 16 aulas. Na revisão de 28 de setembro, o produto já tem a página personalizada, os endpoints da aula 1, um controller com lista, detalhe, título e contagem de pedidos, e um histórico Git inicial. O código da aula 2 foi testado numa cópia isolada; os acertos de tipos, um aviso de nulabilidade e o commit dos últimos exercícios são retomados na aula 3. O SDK .NET 10.0.401 está instalado.

O material anterior fica preservado. Este índice passa a orientar os novos cadernos. Mantemos o produto, a arquitetura simples e os nomes principais. Dividimos algumas aulas para separar conceitos que exigem prática própria: páginas e formulários, login e permissões, classificação e resumos, preparação de documentos e pesquisa, testes e automatização.

**Há 24 aulas previstas e três cadernos disponíveis.** As aulas 2 e 3 começam pela avaliação do teu trabalho anterior e uma revisão curta. Os seguintes serão escritos quando os pedires, considerando o código que realmente construíste e as dificuldades que encontrámos.

## Como estudar

Cada caderno contém explicação, exemplo guiado, construção no projeto, verificações, exercícios com apoio progressivamente menor, diagnóstico de erros e revisão. Há espaço para escrever previsões e respostas. As pistas e soluções aparecem no fim: tenta primeiro e consulta apenas o apoio necessário.

Em cada aula, produz duas provas: uma funcionalidade a funcionar e uma explicação tua sobre como funciona. Copiar código não basta para concluir. Se enviares uma solução, a revisão começa por identificar o comportamento e dar uma pista; a correção completa fica para quando a pedires ou para depois da tentativa.

As durações serão indicadas em cada caderno e incluem prática. Não há um prazo fixo para terminar o curso. Podemos dividir uma aula em duas sessões sem avançar a matéria.

## Índice do curso

Todas as aulas pressupõem as anteriores. A coluna de pré-requisitos destaca a matéria que vais reutilizar, sem exigir que estudes antecipadamente a tecnologia nova.

| Aula | Tema e tecnologia introduzida | Pré-requisitos que vamos rever | Resultado concreto |
|---|---|---|---|
| **01** | Primeira aplicação web: .NET e ASP.NET Core, com Visual Studio ou PowerShell | Variáveis, objetos e métodos em C# | Página inicial identificada como SupportDesk AI e `GET /api/health` com JSON. |
| **02** | HTTP, JSON e controllers; primeiros passos de Git | Executar/parar a aplicação, rotas e respostas | `GET /api/tickets` e consulta de um pedido fictício por ID; primeira versão guardada em Git. |
| **03** | Criação de pedidos, DTOs e validação | Classes, coleções, HTTP e JSON | `POST /api/tickets` cria um pedido em memória e rejeita entradas inválidas. |
| **04** | Serviços, injeção de dependências, estados e comentários | Métodos, interfaces e criação de pedidos | Regras fora do controller; alteração de estado e adição de comentários. |
| **05** | PostgreSQL e primeiras consultas SQL | Modelo de pedido e comentário | Base local de aprendizagem, tabelas relacionadas e consultas sobre dados fictícios. |
| **06** | EF Core, `DbContext`, migrations e `async/await` | SQL básico, relações e serviços | Pedidos e comentários persistem em PostgreSQL após reiniciar a aplicação. |
| **07** | Filtros, paginação e respostas de erro | LINQ, consultas na base e validação | Lista paginada e filtrada; erros previsíveis para operações inválidas. |
| **08** | Razor Pages: HTML, PageModel e apresentação | Serviços e listagem de pedidos | Páginas de lista e detalhe apresentam os dados reais do projeto. |
| **09** | Formulários, validação visual e submissões | HTML e PageModel | Criar pedidos, comentar e mudar estados na interface. |
| **10** | Autenticação, cookies, HTTPS local e Identity | Formulários, base de dados e configuração | Registo, login e logout; pedidos associados ao utilizador autenticado. |
| **11** | Autorização, clientes e técnicos | Identidade e relações na base | Cliente vê apenas os próprios pedidos; técnico recebe permissões de suporte. |
| **12** | Testes unitários com xUnit | Regras de estado, validação e serviços | Testes detetam transições de estado proibidas e outras regras quebradas. |
| **13** | Testes de integração de HTTP e PostgreSQL | xUnit, API e autorização | Testes verificam persistência e impedem acesso aos pedidos de outro cliente. |
| **14** | Modelos de linguagem e `Microsoft.Extensions.AI` | HTTP, serviços e programação assíncrona | Primeira operação experimental de IA, no servidor, com credenciais protegidas e erro tratado. |
| **15** | Respostas estruturadas e classificação | Chamada de IA e validação | Categoria e resumo curto sugeridos para um pedido, com revisão pelo técnico. |
| **16** | Resumo de conversas e gestão do contexto | Comentários, classificação e tokens | Resumo de uma conversa distingue problema, ações realizadas e informação em falta. |
| **17** | Base de conhecimento e preparação de documentos | Persistência e consultas | Artigos fictícios, trechos identificados e pesquisa textual para comparação futura. |
| **18** | Embeddings, pgvector e pesquisa semântica | Trechos, SQL e chamadas de IA | Pergunta recupera trechos relevantes; resultado comparável à pesquisa textual. |
| **19** | RAG, referências e revisão humana | Pesquisa e respostas estruturadas | Rascunho de resposta apresenta fontes verificáveis e pode pedir esclarecimentos. |
| **20** | Avaliação da IA e comparação de alterações | Classificação, resumos e RAG | Conjunto de casos, métricas, análise de falhas e primeiro relatório de qualidade. |
| **21** | Falhas, cancelamento, limites e observabilidade | Testes e avaliação | Aplicação continua utilizável quando a IA falha; chamadas têm limites e registos úteis. |
| **22** | Docker e execução reproduzível | Aplicação completa, configuração e PostgreSQL | Aplicação e base arrancam localmente através de instruções reproduzíveis. |
| **23** | GitHub Actions e integração contínua | Git, testes e ambiente reproduzível | Alterações enviadas para o repositório desencadeiam compilação e testes automáticos. |
| **24** | Publicação, demonstração e portefólio | Aplicação validada e configuração de produção | Demo, README verificável, vídeo curto e explicação das decisões e limitações. |

Na aula 5, a parte funcional é o modelo de dados executável em SQL. Nas aulas de testes, a parte funcional é a deteção automática de regressões. Também contam como evolução do projeto, mesmo sem acrescentar uma página.

## Relação com as 16 aulas anteriores

| Aula anterior | Novos cadernos |
|---|---|
| 1 — Primeira aplicação | 1 |
| 2 — HTTP e controllers | 2 |
| 3 — Serviços e criação | 3–4 |
| 4 — SQL | 5 |
| 5 — EF Core | 6 |
| 6 — Consultas e erros | 7; regras de estado começam em 4 |
| 7 — Interface | 8–9 |
| 8 — Identity e autorização | 10–11 |
| 9 — Testes e automatização | 12–13 e 23 |
| 10 — Primeira integração de IA | 14 |
| 11 — Classificação e resumo | 15–16 |
| 12 — Documentos e pesquisa | 17–18 |
| 13 — RAG | 19 |
| 14 — Avaliação | 20 |
| 15 — Robustez | 21 |
| 16 — Publicação | 22 e 24 |

## Decisões que manteremos entre aulas

- Solução: `SupportDesk.sln`. Projeto web: `src/SupportDesk.Web/SupportDesk.Web.csproj`. Framework inicial: `.NET 10` (`net10.0`).
- Uma aplicação, com Razor Pages para a interface e controllers para a API. As páginas e a API partilham os serviços. O endpoint pequeno da aula 1 usa `MapGet` para tornar o primeiro exemplo legível.
- Estados: `Open`, `InProgress`, `Resolved`. Categorias de IA: `Access`, `Billing`, `Technical`, `Other`.
- C# e identificadores de código em inglês; explicações e texto do produto em português de Portugal.
- Dados fictícios. Sugestões da IA são revistas por um técnico. Permissões são verificadas pelo código da aplicação.
- A API, a base de dados e os testes aparecem gradualmente. Não precisas de instalar PostgreSQL, Docker ou um serviço de IA para a aula 1.
- Credenciais serão introduzidas com a configuração da base e da IA. A escolha do fornecedor de IA será feita na aula 14, de acordo com o acesso e o orçamento disponíveis nessa altura.
- HTTP local na primeira aula; HTTPS será configurado antes de trabalharmos com login. O endpoint inicial de estado verifica apenas se o processo responde.
- Cada caderno distingue o que a ferramenta gera do que escreves e revê as versões das ferramentas antes de introduzir comandos novos.

## Cadernos disponíveis

1. [Aula 01 — Primeira aplicação web e primeiro endpoint](aula-01-primeira-aplicacao-web.md)
2. [Aula 02 — HTTP, JSON, controllers e primeiros commits](aula-02-http-json-controllers-git.md)
3. [Aula 03 — Criar pedidos com POST, DTOs e validação](aula-03-criacao-dtos-validacao.md)

As aulas 4–24 estão planeadas, mas ainda não têm caderno. Não há exercícios implementados por mim na aplicação do aluno.

## Registo de progresso

| Data | Aula | O que consegui executar | O que consigo explicar | Dúvida a rever |
|---|---|---|---|---|
| | | | | |
| | | | | |
| | | | | |

**Como pedir continuidade:** «Terminei a aula 3. Estas são as minhas respostas e este é o meu código. Estes foram os exercícios que saltei.» Vamos rever os critérios de conclusão e preparar a aula seguinte quando a pedires.
