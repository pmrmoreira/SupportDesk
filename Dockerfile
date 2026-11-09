# Aula 22: a imagem da aplicação, em duas etapas.
# Etapa 1: compilar e publicar com o SDK (só existe durante o docker build).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primeiro só o .csproj: enquanto os pacotes não mudarem, o restore vem da cache do Docker.
COPY src/SupportDesk.Web/SupportDesk.Web.csproj src/SupportDesk.Web/
RUN dotnet restore src/SupportDesk.Web/SupportDesk.Web.csproj

# Depois o resto do projeto web, e a publicação (os artigos vão com ela, pelo .csproj da aula 17).
COPY src/SupportDesk.Web/ src/SupportDesk.Web/
RUN dotnet publish src/SupportDesk.Web/SupportDesk.Web.csproj -c Release -o /app --no-restore

# Etapa 2: só o runtime do ASP.NET Core e o que a etapa 1 publicou.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# Exercício B: a pasta das chaves da Data Protection, já com o dono certo. Corre como root, antes do USER;
# quando o volume keys ainda está vazio, o Docker copia para ele esta pasta, com o dono.
RUN mkdir /keys && chown $APP_UID /keys
# O utilizador sem privilégios que as imagens do .NET já trazem (app).
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "SupportDesk.Web.dll"]
