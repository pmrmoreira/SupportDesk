# Termina a aula 7: cria as duas migrações que faltam, aplica-as e (se quiseres) faz o commit.
# Corre a partir da pasta SupportDesk-AI. A aplicação tem de estar parada (Shift+F5 no Visual Studio).
$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot
$proj  = 'src/SupportDesk.Web'
$migs  = Join-Path $PSScriptRoot 'src\SupportDesk.Web\Migrations'
$model = Join-Path $PSScriptRoot 'src\SupportDesk.Web\Models\Ticket.cs'

function Run($titulo, [scriptblock]$cmd) {
    Write-Host "`n== $titulo" -ForegroundColor Cyan
    & $cmd
    if ($LASTEXITCODE -ne 0) { throw "Falhou: $titulo (vê a mensagem acima)." }
}
function Has($nome) { [bool](Get-ChildItem -LiteralPath $migs -Filter "*_$nome.cs" -ErrorAction SilentlyContinue) }

Run 'Compilar o projeto' { dotnet build $proj --nologo -v q }

# 1) TicketUpdatedAt (exercício C da aula 6): gerada sem o Version, para cada migração ter só a sua mudança.
if (-not (Has 'TicketUpdatedAt')) {
    $orig = [IO.File]::ReadAllText($model)
    $semVersion = [regex]::Replace($orig, '\s*\[Timestamp\]\s*public uint Version \{ get; set; \}', '')
    [IO.File]::WriteAllText($model, $semVersion, (New-Object Text.UTF8Encoding $true))
    try {
        Run 'Migração TicketUpdatedAt' { dotnet ef migrations add TicketUpdatedAt --project $proj }
    } finally {
        [IO.File]::WriteAllText($model, $orig, (New-Object Text.UTF8Encoding $true))
    }
    # Os pedidos que já existem ficam com UpdatedAt = CreatedAt (e não com o ano 1).
    $f = Get-ChildItem -LiteralPath $migs -Filter '*_TicketUpdatedAt.cs' | Where-Object { $_.Name -notlike '*.Designer.cs' } | Select-Object -First 1
    $t = [IO.File]::ReadAllText($f.FullName)
    $down  = $t.IndexOf('protected override void Down')
    $brace = $t.LastIndexOf('}', $down)
    $line  = $t.LastIndexOf("`n", $brace) + 1
    $sql = "`r`n            // Os pedidos que já existem ficam com a data de criação.`r`n" +
           '            migrationBuilder.Sql("UPDATE \"Tickets\" SET \"UpdatedAt\" = \"CreatedAt\";");' + "`r`n"
    $t = $t.Insert($line, $sql)
    [IO.File]::WriteAllText($f.FullName, $t, (New-Object Text.UTF8Encoding $true))
    Write-Host "  + UPDATE de UpdatedAt acrescentado em $($f.Name)"
} else { Write-Host "`n== TicketUpdatedAt já existe, salto." }

# 2) TicketVersion (aula 7, ciclo 3): liga o Version à coluna de sistema xmin.
if (-not (Has 'TicketVersion')) {
    Run 'Migração TicketVersion' { dotnet ef migrations add TicketVersion --project $proj }
} else { Write-Host "`n== TicketVersion já existe, salto." }

Run 'Aplicar as migrações à base' { dotnet ef database update --project $proj }

Write-Host "`nPronto. Arranca a aplicação e envia os pedidos do SupportDesk.http." -ForegroundColor Green
$r = Read-Host "`nFazer commit e push agora? (s/n)"
if ($r -match '^[sS]') {
    git add -- src
    git commit -m "Aula 7: filtros, paginação e erros previsíveis (com os exercícios das aulas 6 e 7)"
    git push
}
