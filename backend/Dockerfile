FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# プロジェクトファイルをコピーし依存関係を復元
COPY RetailNexus.Domain/RetailNexus.Domain.csproj RetailNexus.Domain/
COPY RetailNexus.Application/RetailNexus.Application.csproj RetailNexus.Application/
COPY RetailNexus.Infrastructure/RetailNexus.Infrastructure.csproj RetailNexus.Infrastructure/
COPY RetailNexus.Api/RetailNexus.Api.csproj RetailNexus.Api/
RUN dotnet restore RetailNexus.Api/RetailNexus.Api.csproj

# ソースコードをコピーしビルド
COPY . .
RUN dotnet publish RetailNexus.Api/RetailNexus.Api.csproj -c Release -o /app/publish --no-restore

# EF Core ツール（マイグレーション適用用）
RUN dotnet tool install --global dotnet-ef --version 8.0.*
ENV PATH="$PATH:/root/.dotnet/tools"

# 実行用イメージ
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5150
ENV ASPNETCORE_URLS=http://+:5150
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "RetailNexus.Api.dll"]
