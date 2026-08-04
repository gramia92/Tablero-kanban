FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY IdeasGroup.Kanban.sln .
COPY src/IdeasGroup.Kanban.Domain/IdeasGroup.Kanban.Domain.csproj src/IdeasGroup.Kanban.Domain/
COPY src/IdeasGroup.Kanban.Application/IdeasGroup.Kanban.Application.csproj src/IdeasGroup.Kanban.Application/
COPY src/IdeasGroup.Kanban.Infrastructure/IdeasGroup.Kanban.Infrastructure.csproj src/IdeasGroup.Kanban.Infrastructure/
COPY src/IdeasGroup.Kanban.WebApi/IdeasGroup.Kanban.WebApi.csproj src/IdeasGroup.Kanban.WebApi/
COPY tests/IdeasGroup.Kanban.Domain.Tests/IdeasGroup.Kanban.Domain.Tests.csproj tests/IdeasGroup.Kanban.Domain.Tests/
COPY tests/IdeasGroup.Kanban.Application.Tests/IdeasGroup.Kanban.Application.Tests.csproj tests/IdeasGroup.Kanban.Application.Tests/

RUN dotnet restore IdeasGroup.Kanban.sln

COPY . .
RUN dotnet publish src/IdeasGroup.Kanban.WebApi/IdeasGroup.Kanban.WebApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "IdeasGroup.Kanban.WebApi.dll"]
