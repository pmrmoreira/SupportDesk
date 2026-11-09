# Prepara a aula 15 para testares: a tabela das sugestões passa a chamar-se "AiSuggestions", como no curso,
# a migração fica na base e os testes correm. O ciclo 3 não precisa de migração nova.
# Corre a partir da pasta SupportDesk-AI. A aplicação tem de estar parada (Shift+F5) e o PostgreSQL a correr.
$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot
$proj = 'src/SupportDesk.Web'

function Run($titulo, [scriptblock]$cmd) {
    Write-Host "`n== $titulo" -ForegroundColor Cyan
    & $cmd
    if ($LASTEXITCODE -ne 0) { throw "Falhou: $titulo (vê a mensagem acima)." }
}

# Corre um comando dotnet e devolve o texto que escreveu, sem parar o script se ele escrever na stderr.
function Capture([scriptblock]$cmd) {
    $ErrorActionPreference = 'Continue'
    $out = (& $cmd 2>&1 | Out-String)
    return @{ Text = $out; Code = $LASTEXITCODE }
}

Run 'Compilar o projeto' { dotnet build $proj --nologo -v q }

# A migração AiSuggestions foi criada antes do DbSet, por isso criou a tabela "TicketAiSuggestion".
# Ainda não aplicada: refaz-se. Já aplicada: uma migração nova muda o nome da tabela, sem perder dados.
Write-Host "`n== Ver se a migração AiSuggestions já está na base" -ForegroundColor Cyan
$lista = Capture { dotnet ef migrations list --project $proj --no-build }
if ($lista.Code -ne 0 -or $lista.Text -match 'error occurred while accessing the database') {
    Write-Host $lista.Text
    throw 'Não consegui ler as migrações da base. O PostgreSQL está a correr?'
}
$linha = ($lista.Text -split "`r?`n") | Where-Object { $_ -match '_AiSuggestions\b' } | Select-Object -First 1
if (-not $linha) { throw 'Não encontrei a migração AiSuggestions na pasta Migrations.' }

if ($linha -match '\(Pending\)') {
    Write-Host '   Ainda não aplicada: vou refazê-la com o nome certo.'
    Run 'Apagar a migração antiga' { dotnet ef migrations remove --project $proj }
    Run 'Criar outra vez a migração AiSuggestions' { dotnet ef migrations add AiSuggestions --project $proj }
} else {
    $pendentes = Capture { dotnet ef migrations has-pending-model-changes --project $proj --no-build }
    if ($pendentes.Text -match 'No changes have been made') {
        Write-Host '   Já aplicada e sem mudanças por migrar: não há nada a corrigir.'
    } else {
        Write-Host '   Já aplicada: vou criar uma migração que muda o nome da tabela.'
        Run 'Criar a migração RenameAiSuggestionsTable' { dotnet ef migrations add RenameAiSuggestionsTable --project $proj }
    }
}

Run 'Aplicar à base' { dotnet ef database update --project $proj }
Run 'Correr os testes' { dotnet test --nologo }

Write-Host "`nPronto. Arranca a aplicação (F5) e segue os passos de teste do chat." -ForegroundColor Green
