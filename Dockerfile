FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BeachTennis.Api/BeachTennis.Api.csproj BeachTennis.Api/
COPY BeachTennis.Application/BeachTennis.Application.csproj BeachTennis.Application/
COPY BeachTennis.Domain/BeachTennis.Domain.csproj BeachTennis.Domain/
COPY BeachTennis.Infrastructure/BeachTennis.Infrastructure.csproj BeachTennis.Infrastructure/
RUN dotnet restore BeachTennis.Api/BeachTennis.Api.csproj

COPY . .
RUN dotnet publish BeachTennis.Api/BeachTennis.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "BeachTennis.Api.dll"]
