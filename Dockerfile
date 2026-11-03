FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Application/Application.csproj Application/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY WebApi/WebApi.csproj WebApi/
RUN dotnet restore WebApi/WebApi.csproj

COPY Application/ Application/
COPY Infrastructure/ Infrastructure/
COPY WebApi/ WebApi/
RUN dotnet publish WebApi/WebApi.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN mkdir -p /app/data && chown -R $APP_UID /app/data

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ConnectionStrings__Database="Data Source=/app/data/TaskMaker.db"

COPY --from=build /app/publish .

USER $APP_UID

EXPOSE 8080
ENTRYPOINT ["dotnet", "WebApi.dll"]
