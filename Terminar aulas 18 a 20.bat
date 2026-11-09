@echo off
setlocal
rem ===========================================================================
rem  Aulas 18 a 20: instala o pgvector no PostgreSQL 18, cria a migracao,
rem  aplica-a a base e descarrega o modelo de embeddings (bge-m3).
rem  Se o pgvector ainda nao estiver instalado: botao direito > "Executar como administrador".
rem  Se ja estiver, basta abrir com duplo clique. A aplicacao tem de estar parada.
rem ===========================================================================
cd /d "%~dp0"

set "VCVARS=C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvarsall.bat"
set "PGROOT=C:\Program Files\PostgreSQL\18"

echo.
echo === 1/4  pgvector ===
if exist "%PGROOT%\share\extension\vector.control" (
    echo O pgvector ja esta instalado neste PostgreSQL. Passo seguinte.
) else (
    net session >nul 2>&1
    if errorlevel 1 (
        echo Para instalar o pgvector, este ficheiro tem de correr como administrador.
        echo Fecha esta janela e usa o botao direito ^> Executar como administrador.
        goto :erro
    )
    if not exist "%VCVARS%" (
        echo Nao encontrei "%VCVARS%".
        echo No Visual Studio Installer, junta a carga de trabalho "Desktop development with C++".
        goto :erro
    )
    if not exist "pgvector\Makefile.win" (
        git clone --branch v0.8.7 https://github.com/pgvector/pgvector.git || goto :erro
    )
    call "%VCVARS%" x64 || goto :erro
    pushd pgvector
    nmake /nologo /F Makefile.win || goto :erro_pgvector
    nmake /nologo /F Makefile.win install || goto :erro_pgvector
    popd
)

echo.
echo === 2/4  Migracao EmbeddingsAndDrafts ===
if exist "src\SupportDesk.Web\Migrations\*_EmbeddingsAndDrafts.cs" (
    echo A migracao ja existe. Passo seguinte.
) else (
    dotnet ef migrations add EmbeddingsAndDrafts --project src/SupportDesk.Web || goto :erro
)

echo.
echo === 3/4  Aplicar as migracoes a base supportdesk ===
dotnet ef database update --project src/SupportDesk.Web || goto :erro

echo.
echo === 4/4  Modelo de embeddings bge-m3 no Ollama ===
ollama pull bge-m3 || goto :erro

echo.
echo Tudo feito. Ja podes apagar a pasta pgvector que ficou dentro do projeto.
pause
exit /b 0

:erro_pgvector
popd
:erro
echo.
echo Falhou. Le a mensagem acima.
pause
exit /b 1
